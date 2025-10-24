using System;
using System.Threading.Tasks;
using XD.Common.ScopeUtil;

// ReSharper disable UnusedMember.Global
namespace XD.Common.AsyncUtil
{
    /// <summary>
    /// 把可能在子线程中的异步 task 拉到 update 线程中. 适用方法:
    /// <example>
    /// await E.Upd.Register(new UpdateTask(asyncTask));
    /// </example>
    /// </summary>
    public sealed class TickTask : XDObject, ITick, IAwaiter, IAwaitable<TickTask>
    {
        public void OnCompleted(Action? continuation)
        {
            if (continuation == null) return;
            if (IsCompleted)
            {
                continuation();
                return;
            }
            _continuation += continuation;
        }

        public bool IsCompleted => _isCompleted && (_task?.IsCompleted ?? true);
        public void GetResult() {}
        public TickTask GetAwaiter() => this;

        public void OnTick(float dt = 0, float rdt = 0)
        {
            _isCompleted = true;
            if (IsDisposed) return;
            if (!IsCompleted) return;

            Dispose();
            _continuation?.Invoke();
        }

        public TickTask() => _isCompleted = false;
        public TickTask(Task task) => _task = task;

        private readonly Task? _task;
        private Action? _continuation;
        private bool _isCompleted = true;
    }

    /// <summary>
    /// 把可能在子线程中的异步 task 拉到 update 线程中. 适用方法:
    /// <example>
    /// var t = await E.Upd.Register(new UpdateTask(asyncTask));
    /// </example>
    /// </summary>
    public sealed class TickTask<T> : XDObject, ITick, IAwaiter<T>, IAwaitable<TickTask<T>, T>
    {
        public void OnCompleted(Action? continuation)
        {
            if (continuation == null) return;
            if (IsCompleted) continuation();
            else _continuation += continuation;
        }

        public bool IsCompleted => _task.IsCompleted;
        public T GetResult() => _task.Result;
        public TickTask<T> GetAwaiter() => this;

        public void OnTick(float dt = 0, float rdt = 0)
        {
            if (IsDisposed) return;
            if (!_task.IsCompleted) return;

            Dispose();
            _continuation?.Invoke();
        }

        public TickTask(Task<T> task) => _task = task;
        private readonly Task<T> _task;
        private Action? _continuation;
    }

    public sealed class TickConditionTask : XDObject, ITick, IAwaiter, IAwaitable<TickConditionTask>
    {
        public void OnCompleted(Action? continuation)
        {
            if (continuation == null) return;
            if (IsCompleted)
            {
                continuation();
                return;
            }
            _continuation += continuation;
        }

        public bool IsCompleted => _isCompleted && (_condition?.Invoke() ?? true);
        public void GetResult() {}
        public TickConditionTask GetAwaiter() => this;

        public void OnTick(float dt = 0, float rdt = 0)
        {
            _isCompleted = true;
            if (IsDisposed) return;
            if (!IsCompleted) return;

            Dispose();
            _continuation?.Invoke();
        }

        public TickConditionTask() => _isCompleted = false;
        public TickConditionTask(Func<bool>? condition) => _condition = condition;

        private readonly Func<bool>? _condition;
        private Action? _continuation;
        private bool _isCompleted = true;
    }
}