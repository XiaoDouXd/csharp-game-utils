#nullable enable

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
    public sealed class UpdateTask : XDObject, IUpdate, IAwaiter, IAwaitable<UpdateTask>
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
        public UpdateTask GetAwaiter() => this;

        public void OnUpdate(float dt = 0, float rdt = 0)
        {
            _isCompleted = true;
            if (IsDisposed) return;
            if (!IsCompleted) return;

            Dispose();
            _continuation?.Invoke();
        }

        public UpdateTask() => _isCompleted = false;
        public UpdateTask(Task task) => _task = task;

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
    public sealed class UpdateTask<T> : XDObject, IUpdate, IAwaiter<T>, IAwaitable<UpdateTask<T>, T>
    {
        public void OnCompleted(Action? continuation)
        {
            if (continuation == null) return;
            if (IsCompleted) continuation();
            else _continuation += continuation;
        }

        public bool IsCompleted => _task.IsCompleted;
        public T GetResult() => _task.Result;
        public UpdateTask<T> GetAwaiter() => this;

        public void OnUpdate(float dt = 0, float rdt = 0)
        {
            if (IsDisposed) return;
            if (!_task.IsCompleted) return;

            Dispose();
            _continuation?.Invoke();
        }

        public UpdateTask(Task<T> task) => _task = task;
        private readonly Task<T> _task;
        private Action? _continuation;
    }

    public sealed class UpdateConditionTask : XDObject, IUpdate, IAwaiter, IAwaitable<UpdateConditionTask>
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
        public UpdateConditionTask GetAwaiter() => this;

        public void OnUpdate(float dt = 0, float rdt = 0)
        {
            _isCompleted = true;
            if (IsDisposed) return;
            if (!IsCompleted) return;

            Dispose();
            _continuation?.Invoke();
        }

        public UpdateConditionTask() => _isCompleted = false;
        public UpdateConditionTask(Func<bool>? condition) => _condition = condition;

        private readonly Func<bool>? _condition;
        private Action? _continuation;
        private bool _isCompleted = true;
    }
}