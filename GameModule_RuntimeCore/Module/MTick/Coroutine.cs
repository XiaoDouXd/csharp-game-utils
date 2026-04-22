using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using XD.Common.AsyncUtil;
using XDLog = XD.Common.Log.Log;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ArrangeTrailingCommaInMultilineLists

namespace XD.GameModule.Module.MTick
{
    public enum ECoroutineType
    {
        Tick,
        LateTick,
        PhysicalTick,
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class TickModule
    {
        /// <summary> 协程接口 </summary>
        // ReSharper disable once IdentifierTypo
        public ICoroutineInterface Coro => _coroutineInterface;

        /// <summary> 协程句柄 </summary>
        public interface ICoroutineHandle : IAwaiter, IAwaitable<ICoroutineHandle>
        {
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            public bool Cancel() => E.Tick?.Coro.Del(this) ?? false;
        }

        public interface ICoroutineInterface
        {
            /// <summary>
            /// 运行一个协程, 协程返回值为 double, 表示等待时间, 单位秒.
            /// <para/> 线程安全: 可在任意线程调用, 实际挂入链表在主线程下一次 Tick 完成.
            /// </summary>
            public ICoroutineHandle? Run(Func<IEnumerator<double>?> coroutineGenerator, ECoroutineType type = ECoroutineType.Tick);

            /// <summary>
            /// 运行一个协程, 协程返回值为 double, 表示等待时间, 单位秒.
            /// <para/> 线程安全: 可在任意线程调用.
            /// </summary>
            public ICoroutineHandle? Run(IEnumerator<double>? coroutine, ECoroutineType type = ECoroutineType.Tick);

            /// <summary>
            /// 在一个协程结束或销毁之后运行一个协程.
            /// <para/> 线程安全: 可在任意线程调用.
            /// </summary>
            public ICoroutineHandle? RunAfter(ICoroutineHandle? registerHandle, Func<IEnumerator<double>?>? coroutineGenerator,
                ECoroutineType type = ECoroutineType.Tick);

            /// <summary>
            /// 在一个协程结束或销毁之后运行一个协程.
            /// <para/> 线程安全: 可在任意线程调用.
            /// </summary>
            public ICoroutineHandle? RunAfter(ICoroutineHandle? registerHandle, IEnumerator<double>? coroutine,
                ECoroutineType type = ECoroutineType.Tick);

            /// <summary>
            /// 取消运行协程.
            /// <para/> 线程安全: 可在任意线程调用.
            /// </summary>
            public bool Del(ICoroutineHandle? registerHandle);

            /// <summary>
            /// 合并协程为序列, 序列中的协程将按顺序执行.
            /// </summary>
            public Func<IEnumerator<double>?> Combine(params Func<IEnumerator<double>>?[]? coroutineGenerators)
            {
                if (coroutineGenerators is not { Length: > 0 }) return () => null;

                // 立即求值各 generator, 跳过 null, 避免运行时 NRE (§2.4).
                var list = new List<IEnumerator<double>>(coroutineGenerators.Length);
                for (var i = 0; i < coroutineGenerators.Length; i++)
                {
                    var coroutine = coroutineGenerators[i]?.Invoke();
                    if (coroutine != null) list.Add(coroutine);
                }
                if (list.Count == 0) return () => null;

                return CombineFunc;

                IEnumerator<double> CombineFunc()
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var coroutine = list[i];
                        try
                        {
                            while (coroutine.MoveNext()) yield return coroutine.Current;
                        }
                        finally { coroutine.Dispose(); }
                    }
                }
            }
        }

        /// <summary>
        /// 协程接口实现.
        /// <para/>
        /// 线程模型:
        ///   - 对外 API (Run / RunAfter / Del) 可在任意线程调用, 操作通过 pending 队列排到主线程.
        ///   - Tick / LateTick / PhysicalTick 由主线程驱动, 独占三张链表, 完全无锁 (§2.1/§2.2).
        /// </summary>
        private sealed class CoroutineInterface : ICoroutineInterface
        {
            #region Public API (任意线程)

            public ICoroutineHandle? Run(Func<IEnumerator<double>?>? coroutineGenerator, ECoroutineType type = ECoroutineType.Tick)
                => Run(coroutineGenerator?.Invoke(), type);

            public ICoroutineHandle? Run(IEnumerator<double>? coroutine, ECoroutineType type = ECoroutineType.Tick)
            {
                if (coroutine == null) return null;
                var task = new Task(coroutine, type);
                _pendingAdd.Enqueue(task);
                return task;
            }

            public ICoroutineHandle? RunAfter(ICoroutineHandle? registerHandle, Func<IEnumerator<double>?>? coroutineGenerator,
                ECoroutineType type = ECoroutineType.Tick)
                => RunAfter(registerHandle, coroutineGenerator?.Invoke(), type);

            public ICoroutineHandle? RunAfter(ICoroutineHandle? registerHandle, IEnumerator<double>? coroutine,
                ECoroutineType type = ECoroutineType.Tick)
            {
                if (coroutine == null) return null;
                var task = new Task(coroutine, type);

                // 无有效父句柄 -> 直接运行.
                if (registerHandle is not Task parent)
                {
                    _pendingAdd.Enqueue(task);
                    return task;
                }

                // 挂到父协程的 OnCompleted: 父完成或被取消时, 子协程入 pending 队列.
                // Task.OnCompleted 保证 "父已完成时立即回调", 所以这里不存在丢单.
                var self = this;
                parent.OnCompleted(() => self._pendingAdd.Enqueue(task));
                return task;
            }

            public bool Del(ICoroutineHandle? registerHandle)
            {
                if (registerHandle is not Task task) return false;
                if (!task.TryMarkCancel()) return false;
                _pendingDel.Enqueue(task);
                return true;
            }

            #endregion

            #region Main-thread drivers

            public void Tick(float dt, float rdt) => DriveOnce(_tickTasks, dt);
            public void LateTick(float dt, float rdt) => DriveOnce(_lateTickTasks, dt);
            public void PhysicalTick(float dt, float rdt) => DriveOnce(_physicalTickTasks, dt);

            private void DriveOnce(LinkedList<Task> list, float dt)
            {
                // 1. flush pending add (按类型分派到对应链表)
                while (_pendingAdd.TryDequeue(out var task))
                {
                    if (task.IsCancelled)
                    {
                        task.CompleteAndNotify();
                        continue;
                    }
                    GetList(task.Type).AddLast(task.Node);
                }

                // 2. flush pending del
                while (_pendingDel.TryDequeue(out var task))
                {
                    var ownerList = task.Node.List;
                    if (ownerList != null) ownerList.Remove(task.Node);
                    task.CompleteAndNotify();
                }

                // 3. 驱动 list
                var node = list.First;
                while (node != null)
                {
                    var next = node.Next;
                    var task = node.Value;

                    if (task.IsCancelled)
                    {
                        list.Remove(node);
                        task.CompleteAndNotify();
                        node = next;
                        continue;
                    }

                    bool alive;
                    try { alive = task.Do(dt); }
                    catch (Exception e)
                    {
                        XDLog.Error(e);
                        alive = false;
                    }

                    if (!alive)
                    {
                        list.Remove(node);
                        task.CompleteAndNotify();
                    }
                    node = next;
                }
            }

            private LinkedList<Task> GetList(ECoroutineType type) => type switch
            {
                ECoroutineType.Tick => _tickTasks,
                ECoroutineType.LateTick => _lateTickTasks,
                ECoroutineType.PhysicalTick => _physicalTickTasks,
                _ => _tickTasks,
            };

            #endregion

            #region Fields

            private readonly LinkedList<Task> _tickTasks = new();
            private readonly LinkedList<Task> _lateTickTasks = new();
            private readonly LinkedList<Task> _physicalTickTasks = new();

            private readonly ConcurrentQueue<Task> _pendingAdd = new();
            private readonly ConcurrentQueue<Task> _pendingDel = new();

            #endregion

            #region Task

            /// <summary>
            /// 协程 Task.
            /// <para/>
            /// 字段 <c>_state</c> 状态机 (Interlocked):
            ///   Pending(0)  --Do/完成-->  Completed(2)
            ///               --Cancel-->   Cancelling(1)  --flush-->  Completed(2)
            /// <para/>
            /// 一次性通知语义: CompleteAndNotify 只会触发一次 OnCompleted 回调.
            /// </summary>
            private sealed class Task : ICoroutineHandle
            {
                public LinkedListNode<Task> Node { get; }
                public ECoroutineType Type { get; }

                public bool IsCancelled => Volatile.Read(ref _state) != StatePending;
                public bool IsCompleted => Volatile.Read(ref _state) == StateCompleted;

                public Task(IEnumerator<double> enumerator, ECoroutineType type)
                {
                    _enumerator = enumerator;
                    _state = StatePending;
                    Type = type;
                    Node = new LinkedListNode<Task>(this);
                }

                /// <summary> 标记取消; 若已取消/完成返回 false (避免重复入 del 队列). </summary>
                public bool TryMarkCancel() =>
                    Interlocked.CompareExchange(ref _state, StateCancelling, StatePending) == StatePending;

                /// <summary>
                /// 推进协程一步 (主线程).
                /// 返回 true = 存活, false = 已结束 (主线程应 Remove + CompleteAndNotify).
                /// </summary>
                public bool Do(float dt)
                {
                    var enumerator = _enumerator;
                    if (enumerator == null) return false;

                    // 时间间隔等待.
                    if (_ret > 0 && _cumulative < _ret)
                    {
                        _cumulative += dt;
                        return true;
                    }
                    _cumulative += dt;

                    bool moved;
                    try { moved = enumerator.MoveNext(); }
                    catch
                    {
                        // 出现异常, 保证 dispose 被调用 (§2.5).
                        DisposeEnumerator();
                        throw;
                    }
                    if (!moved)
                    {
                        DisposeEnumerator();
                        return false;
                    }
                    _ret = enumerator.Current;
                    _cumulative = 0;
                    return true;
                }

                /// <summary>
                /// 标记为 Completed 并触发订阅的 continuation. 幂等.
                /// 主线程调用. 调用前一般已经从链表中 Remove.
                /// </summary>
                public void CompleteAndNotify()
                {
                    // 若当前还没到 Completed, 一次性切换.
                    Interlocked.Exchange(ref _state, StateCompleted);
                    DisposeEnumerator();

                    var cb = _onCompleted;
                    _onCompleted = null;
                    if (cb == null) return;

                    // 分解多播委托, 单个回调异常不影响后续回调.
                    var list = cb.GetInvocationList();
                    for (int i = 0, n = list.Length; i < n; i++)
                    {
                        try { ((Action)list[i])(); }
                        catch (Exception e) { XDLog.Error(e); }
                    }
                }

                private void DisposeEnumerator()
                {
                    var e = _enumerator;
                    _enumerator = null;
                    if (e == null) return;
                    try { e.Dispose(); }
                    catch (Exception ex) { XDLog.Error(ex); }
                }

                #region awaiter

                /// <summary>
                /// 注册完成回调.
                /// - 若已 Completed, 立即调用 (在当前线程).
                /// - 否则累加到 _onCompleted, 在 CompleteAndNotify 时统一触发 (主线程).
                /// <para/>
                /// 注意: 回调可能在 "注册线程" 或 "主线程" 触发, 视调用时刻而定.
                /// </summary>
                public void OnCompleted(Action? continuation)
                {
                    if (continuation == null) return;
                    if (IsCompleted)
                    {
                        try { continuation(); }
                        catch (Exception e) { XDLog.Error(e); }
                        return;
                    }

                    // 多线程环境下累加多播委托需要原子操作.
                    Action? prev, desired;
                    do
                    {
                        prev = _onCompleted;
                        desired = (Action?)Delegate.Combine(prev, continuation);
                    } while (Interlocked.CompareExchange(ref _onCompleted, desired, prev) != prev);

                    // 防竞争: 若 CAS 过程中任务已 Completed, 触发我们这次订阅 (CompleteAndNotify 已经读走过 cb).
                    if (IsCompleted)
                    {
                        Action? taken;
                        do
                        {
                            taken = _onCompleted;
                        } while (taken != null && Interlocked.CompareExchange(ref _onCompleted, null, taken) != taken);

                        if (taken == null) return;
                        foreach (var d in taken.GetInvocationList())
                        {
                            try { ((Action)d)(); }
                            catch (Exception e) { XDLog.Error(e); }
                        }
                    }
                }

                public ICoroutineHandle GetAwaiter() => this;
                public void GetResult() { }

                #endregion

                private const int StatePending = 0;
                private const int StateCancelling = 1;
                private const int StateCompleted = 2;

                private int _state;
                private double _ret;
                private double _cumulative;
                private IEnumerator<double>? _enumerator;
                private Action? _onCompleted;
            }

            #endregion
        }

        private readonly CoroutineInterface _coroutineInterface = new();
    }
}
