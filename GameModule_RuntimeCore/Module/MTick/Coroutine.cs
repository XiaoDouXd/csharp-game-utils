using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XD.Common.AsyncUtil;

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
            /// 运行一个协程, 协程返回值为 double, 表示等待时间, 单位秒
            /// <example> var registerHandle = UpdateModule.Coroutine.Run(coroutine); </example>
            /// </summary>
            /// <param name="coroutineGenerator"> 协程生成函数 </param>
            /// <param name="type"> 协程调用时机 </param>
            /// <returns> 协程删除句柄 registerHandle </returns>
            public ICoroutineHandle? Run(Func<IEnumerator<double>?> coroutineGenerator, ECoroutineType type = ECoroutineType.Tick);

            /// <summary>
            /// 运行一个协程, 协程返回值为 double, 表示等待时间, 单位秒
            /// </summary>
            /// <param name="coroutine"> 协程 </param>
            /// <param name="type"> 协程调用时机 </param>
            /// <returns> 协程删除句柄 registerHandle </returns>
            public ICoroutineHandle? Run(IEnumerator<double>? coroutine, ECoroutineType type = ECoroutineType.Tick);

            /// <summary>
            /// 在一个协程结束或销毁之后运行一个协程
            /// </summary>
            /// <param name="registerHandle"> 协程删除句柄 registerHandle </param>
            /// <param name="coroutineGenerator"> 协程生成函数 </param>
            /// <param name="type"> 新协程调用时机 </param>
            /// <returns></returns>
            public ICoroutineHandle? RunAfter(ICoroutineHandle? registerHandle, Func<IEnumerator<double>?>? coroutineGenerator,
                ECoroutineType type = ECoroutineType.Tick);

            /// <summary>
            /// 在一个协程结束或销毁之后运行一个协程
            /// </summary>
            /// <param name="registerHandle"> 协程删除句柄 registerHandle </param>
            /// <param name="coroutine"> 协程 </param>
            /// <param name="type"> 新协程调用时机 </param>
            /// <returns></returns>
            public ICoroutineHandle? RunAfter(ICoroutineHandle? registerHandle, IEnumerator<double>? coroutine,
                ECoroutineType type = ECoroutineType.Tick);

            /// <summary>
            /// 取消运行协程
            /// </summary>
            /// <param name="registerHandle"> 协程删除句柄 </param>
            /// <returns></returns>
            public bool Del(ICoroutineHandle? registerHandle);

            /// <summary>
            /// 合并协程为序列, 序列中的协程将按顺序执行
            /// </summary>
            /// <param name="coroutineGenerators"></param>
            /// <returns></returns>
            public Func<IEnumerator<double>?> Combine(params Func<IEnumerator<double>>?[]? coroutineGenerators)
            {
                if (coroutineGenerators is not { Length: > 0 }) return () => null;
                var arr = new IEnumerator<double>[coroutineGenerators.Length];
                for (int i = 0, j = 0; i < coroutineGenerators.Length; i++)
                {
                    var coroutine = coroutineGenerators[i]?.Invoke();
                    if (coroutine is not null) arr[j++] = coroutine;
                }
                return CombineFunc;

                IEnumerator<double> CombineFunc()
                {
                    foreach (var coroutine in arr)
                        while (coroutine.MoveNext())
                            yield return coroutine.Current;
                }
            }
        }

        private sealed class CoroutineInterface : ICoroutineInterface
        {
            public ICoroutineHandle? Run(Func<IEnumerator<double>?>? coroutineGenerator, ECoroutineType type = ECoroutineType.Tick)
                => Run(coroutineGenerator?.Invoke(), type);

            public ICoroutineHandle? Run(IEnumerator<double>? coroutine, ECoroutineType type = ECoroutineType.Tick)
            {
                var task = new Task(coroutine);
                lock (task.Node) return !task.Do(0) ? null : RunImmInner(task, type);
            }

            public ICoroutineHandle? RunAfter(ICoroutineHandle? registerHandle, Func<IEnumerator<double>?>? coroutineGenerator, ECoroutineType type = ECoroutineType.Tick)
                => RunAfter(registerHandle, coroutineGenerator?.Invoke(), type);

            public ICoroutineHandle? RunAfter(ICoroutineHandle? registerHandle, IEnumerator<double>? coroutine, ECoroutineType type = ECoroutineType.Tick)
            {
                if (registerHandle is not Task targetTask) return null;
                lock (targetTask)
                {
                    if (_waitingTasks.TryGetValue(targetTask, out _))
                    {
                        var task = new Task(coroutine);
                        var act = (Action)(() =>
                        {
                            if (!_waitingTasks.TryRemove(task, out _)) return;
                            RunImmInner(task, type);
                        });
                        if (!_waitingTasks.TryAdd(task, act)) return null;
                        targetTask.OnDel += act;
                        return task;
                    }

                    var isImmRun = false;
                    lock (_tickTasks)
                    {
                        if (targetTask.Node.List == _tickTasks)
                        {
                            var task = new Task(coroutine);
                            if (targetTask.IsCompleted) isImmRun = true;
                            else
                            {
                                var act = (Action)(() =>
                                {
                                    if (!_waitingTasks.TryRemove(task, out _)) return;
                                    RunImmInner(task, type);
                                });
                                if (!_waitingTasks.TryAdd(task, act)) return null;
                                targetTask.OnDel += act;
                                return task;
                            }
                        }
                    }

                    if (isImmRun)
                    {
                        var task = new Task(coroutine);
                        return !task.Do(0) ? null : RunImmInner(task, type);
                    }

                    lock (_lateTickTasks)
                    {
                        var task = new Task(coroutine);
                        if (targetTask.IsCompleted) isImmRun = true;
                        else
                        {
                            var act = (Action)(() =>
                            {
                                if (!_waitingTasks.TryRemove(task, out _)) return;
                                RunImmInner(task, type);
                            });
                            if (!_waitingTasks.TryAdd(task, act)) return null;
                            targetTask.OnDel += act;
                            return task;
                        }
                    }

                    if (isImmRun)
                    {
                        var task = new Task(coroutine);
                        return !task.Do(0) ? null : RunImmInner(task, type);
                    }

                    lock (_physicalTickTasks)
                    {
                        var task = new Task(coroutine);
                        if (targetTask.IsCompleted) isImmRun = true;
                        else
                        {
                            var act = (Action)(() =>
                            {
                                if (!_waitingTasks.TryRemove(task, out _)) return;
                                RunImmInner(task, type);
                            });
                            if (!_waitingTasks.TryAdd(task, act)) return null;
                            targetTask.OnDel += act;
                            return task;
                        }
                    }

                    if (!isImmRun) return null;
                    {
                        var task = new Task(coroutine);
                        return !task.Do(0) ? null : RunImmInner(task, type);
                    }
                }
            }

            public bool Del(ICoroutineHandle? registerHandle)
            {
                // ReSharper disable InvertIf
                if (registerHandle is not Task task) return false;
                lock (task)
                {
                    if (_waitingTasks.TryGetValue(task, out _)) return _waitingTasks.TryRemove(task, out _);

                    var isRemoveSuccess = false;
                    lock (_tickTasks)
                    {
                        if (task.Node.List == _tickTasks)
                        {
                            _tickTasks.Remove(task);
                            isRemoveSuccess = true;
                        }
                    }

                    if (isRemoveSuccess)
                    {
                        task.OnDel?.Invoke();
                        return true;
                    }

                    lock (_lateTickTasks)
                    {
                        if (task.Node.List == _lateTickTasks)
                        {
                            _lateTickTasks.Remove(task);
                            isRemoveSuccess = true;
                        }
                    }

                    if (isRemoveSuccess)
                    {
                        task.OnDel?.Invoke();
                        return true;
                    }

                    lock (_physicalTickTasks)
                    {
                        if (task.Node.List == _physicalTickTasks)
                        {
                            _physicalTickTasks.Remove(task);
                            isRemoveSuccess = true;
                        }
                    }

                    if (isRemoveSuccess)
                    {
                        task.OnDel?.Invoke();
                        return true;
                    }
                }
                return false;
                // ReSharper restore InvertIf
            }

            public void Tick(float dt, float rdt)
            {
                lock (_tickTasks)
                {
                    var node = _tickTasks.First;
                    while (node != null)
                    {
                        var next = node.Next;
                        lock (node)
                        {
                            if (!node.Value.Do(dt))
                            {
                                _tickTasks.Remove(node);
                                node.Value.OnDel?.Invoke();
                            }
                        }
                        node = next;
                    }
                }
            }

            public void LateTick(float dt, float rdt)
            {
                lock (_lateTickTasks)
                {
                    var node = _lateTickTasks.First;
                    while (node != null)
                    {
                        var next = node.Next;
                        if (!node.Value.Do(dt))
                        {
                            _lateTickTasks.Remove(node);
                            node.Value.OnDel?.Invoke();
                        }
                        node = next;
                    }
                }
            }

            public void PhysicalTick(float dt, float rdt)
            {
                lock (_physicalTickTasks)
                {
                    var node = _physicalTickTasks.First;
                    while (node != null)
                    {
                        var next = node.Next;
                        if (!node.Value.Do(dt))
                        {
                            _physicalTickTasks.Remove(node);
                            node.Value.OnDel?.Invoke();
                        }
                        node = next;
                    }
                }
            }

            private Task? RunImmInner(Task task, ECoroutineType type = ECoroutineType.Tick)
            {
                switch (type)
                {
                    case ECoroutineType.Tick:
                        lock (_tickTasks) _tickTasks.AddLast(task.Node);
                        return task;
                    case ECoroutineType.LateTick:
                        lock (_lateTickTasks) _lateTickTasks.AddLast(task.Node);
                        return task;
                    case ECoroutineType.PhysicalTick:
                        lock (_physicalTickTasks) _physicalTickTasks.AddLast(task.Node);
                        return task;
                    default: return null;
                }
            }

            private readonly LinkedList<Task> _tickTasks = new();
            private readonly LinkedList<Task> _lateTickTasks = new();
            private readonly LinkedList<Task> _physicalTickTasks = new();
            private readonly ConcurrentDictionary<Task, Action> _waitingTasks = new();

            private sealed class Task : ICoroutineHandle
            {
                public Action? OnDel { get; set; }
                public LinkedListNode<Task> Node { get; }

                private double _ret;
                private double _cumulative;
                private IEnumerator<double>? _enumerator;

                public Task(IEnumerator<double /* 时间间隔 */>? enumerable)
                {
                    _ret = 0;
                    OnDel = null;
                    _cumulative = 0;
                    _enumerator = enumerable;
                    Node = new LinkedListNode<Task>(this);
                }

                public bool Do(float dt)
                {
                    if (_enumerator == null) return false;

                    // 时间间隔
                    if (_ret > 0 && _cumulative < _ret)
                    {
                        _cumulative += dt;
                        return true;
                    }
                    _cumulative += dt;

                    if (!_enumerator.MoveNext())
                    {
                        _ret = _cumulative = 0;
                        _enumerator = null;
                        return false;
                    }
                    _ret = _enumerator.Current;
                    _cumulative = 0;
                    return true;
                }

                #region awaiter
                public void OnCompleted(Action? continuation)
                {
                    if (continuation == null) return;
                    if (IsCompleted) continuation();
                    else OnDel += continuation;
                }

                public bool IsCompleted => _enumerator == null;
                public ICoroutineHandle GetAwaiter() => this;
                public void GetResult() {}
                #endregion
            }
        }

        private readonly CoroutineInterface _coroutineInterface = new();
    }
}