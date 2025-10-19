#nullable enable

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

namespace XD.GameModule.Module.MUpdate
{
    public enum ECoroutineType
    {
        Update,
        LateUpdate,
        FixedUpdate,
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class UpdateModule
    {
        /// <summary> 协程接口 </summary>
        // ReSharper disable once IdentifierTypo
        public ICoroutineInterface Coro => _coroutineInterface;

        /// <summary> 协程句柄 </summary>
        public interface ICoroutineHandle : IAwaiter, IAwaitable<ICoroutineHandle> {}

        public interface ICoroutineInterface
        {
            /// <summary>
            /// 运行一个协程
            /// <example> var registerHandle = UpdateModule.Coroutine.Run(coroutine); </example>
            /// </summary>
            /// <param name="coroutineGenerator"> 协程生成函数 </param>
            /// <param name="type"> 协程调用时机 </param>
            /// <returns> 协程删除句柄 registerHandle </returns>
            public ICoroutineHandle? Run(Func<IEnumerator<double>?> coroutineGenerator, ECoroutineType type = ECoroutineType.Update);

            /// <summary>
            /// 在一个协程结束或销毁之后运行一个协程
            /// </summary>
            /// <param name="registerHandle"> 协程删除句柄 registerHandle </param>
            /// <param name="coroutineGenerator"> 协程生成函数 </param>
            /// <param name="type"> 新协程调用时机 </param>
            /// <returns></returns>
            public ICoroutineHandle? RunAfter(ICoroutineHandle? registerHandle, Func<IEnumerator<double>?>? coroutineGenerator,
                ECoroutineType type = ECoroutineType.Update);

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
            public ICoroutineHandle? Run(Func<IEnumerator<double>?>? coroutineGenerator, ECoroutineType type = ECoroutineType.Update)
            {
                var task = new Task(coroutineGenerator?.Invoke());
                lock (task.Node) return !task.Do(0) ? null : RunImmInner(task, type);
            }

            public ICoroutineHandle? RunAfter(ICoroutineHandle? registerHandle, Func<IEnumerator<double>?>? coroutineGenerator, ECoroutineType type = ECoroutineType.Update)
            {
                if (registerHandle is not Task targetTask) return null;
                lock (targetTask)
                {
                    if (_waitingTasks.TryGetValue(targetTask, out _))
                    {
                        var task = new Task(coroutineGenerator?.Invoke());
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
                    lock (_updateTasks)
                    {
                        if (targetTask.Node.List == _updateTasks)
                        {
                            var task = new Task(coroutineGenerator?.Invoke());
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
                        var task = new Task(coroutineGenerator?.Invoke());
                        return !task.Do(0) ? null : RunImmInner(task, type);
                    }

                    lock (_lateUpdateTasks)
                    {
                        var task = new Task(coroutineGenerator?.Invoke());
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
                        var task = new Task(coroutineGenerator?.Invoke());
                        return !task.Do(0) ? null : RunImmInner(task, type);
                    }

                    lock (_fixedUpdateTasks)
                    {
                        var task = new Task(coroutineGenerator?.Invoke());
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
                        var task = new Task(coroutineGenerator?.Invoke());
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
                    lock (_updateTasks)
                    {
                        if (task.Node.List == _updateTasks)
                        {
                            _updateTasks.Remove(task);
                            isRemoveSuccess = true;
                        }
                    }

                    if (isRemoveSuccess)
                    {
                        task.OnDel?.Invoke();
                        return true;
                    }

                    lock (_lateUpdateTasks)
                    {
                        if (task.Node.List == _lateUpdateTasks)
                        {
                            _lateUpdateTasks.Remove(task);
                            isRemoveSuccess = true;
                        }
                    }

                    if (isRemoveSuccess)
                    {
                        task.OnDel?.Invoke();
                        return true;
                    }

                    lock (_fixedUpdateTasks)
                    {
                        if (task.Node.List == _fixedUpdateTasks)
                        {
                            _fixedUpdateTasks.Remove(task);
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

            public void Update(float dt, float rdt)
            {
                lock (_updateTasks)
                {
                    var node = _updateTasks.First;
                    while (node != null)
                    {
                        var next = node.Next;
                        lock (node)
                        {
                            if (!node.Value.Do(dt))
                            {
                                _updateTasks.Remove(node);
                                node.Value.OnDel?.Invoke();
                            }
                        }
                        node = next;
                    }
                }
            }

            public void LateUpdate(float dt, float rdt)
            {
                lock (_lateUpdateTasks)
                {
                    var node = _lateUpdateTasks.First;
                    while (node != null)
                    {
                        var next = node.Next;
                        if (!node.Value.Do(dt))
                        {
                            _lateUpdateTasks.Remove(node);
                            node.Value.OnDel?.Invoke();
                        }
                        node = next;
                    }
                }
            }

            public void FixedUpdate(float dt, float rdt)
            {
                lock (_fixedUpdateTasks)
                {
                    var node = _fixedUpdateTasks.First;
                    while (node != null)
                    {
                        var next = node.Next;
                        if (!node.Value.Do(dt))
                        {
                            _fixedUpdateTasks.Remove(node);
                            node.Value.OnDel?.Invoke();
                        }
                        node = next;
                    }
                }
            }

            private Task? RunImmInner(Task task, ECoroutineType type = ECoroutineType.Update)
            {
                switch (type)
                {
                    case ECoroutineType.Update:
                        lock (_updateTasks) _updateTasks.AddLast(task.Node);
                        return task;
                    case ECoroutineType.LateUpdate:
                        lock (_lateUpdateTasks) _lateUpdateTasks.AddLast(task.Node);
                        return task;
                    case ECoroutineType.FixedUpdate:
                        lock (_fixedUpdateTasks) _fixedUpdateTasks.AddLast(task.Node);
                        return task;
                    default: return null;
                }
            }

            private readonly LinkedList<Task> _updateTasks = new();
            private readonly LinkedList<Task> _lateUpdateTasks = new();
            private readonly LinkedList<Task> _fixedUpdateTasks = new();
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