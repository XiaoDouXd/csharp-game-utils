using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantDefaultMemberInitializer

namespace XD.Common.ScopeUtil
{
    /// <summary>
    /// 带有已销毁 flag 的 disposable 对象
    /// </summary>
    public interface IDisposableWithFlag : IDisposable
    {
        public bool IsDisposed { get; }
    }

    /// <summary>
    /// 可销毁对象
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public interface IXDDisposable : IDisposableWithFlag
    {
        public void AddOnDispose(Action? callback);
        public void RemoveOnDispose(Action? callback);

        /// <summary>
        /// 绑定到目标 IXDDisposable (在目标 dispose 后 dispose 当前对象)
        /// </summary>
        /// <param name="disposable"></param>
        public void Bind(IXDDisposable? disposable = null);
    }

    /// <summary>
    /// 可销毁对象基类
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public abstract class XDDisposableObjectBase : IXDDisposable
    {
        /// <summary>
        /// 重写该函数后, 请务必先判断该类是否已经 IsDisposed,
        /// 并于结束 dispose 时主动调用 OnDispose()
        /// <example>
        /// public override void Dispose()<br/>
        /// {<br/>
        ///     if (IsDisposed) return;<br/>
        ///     /* .... */<br/>
        ///     /* ... do something ... */<br/>
        ///     /* .... */<br/>
        ///     base.OnDisposed();<br/>
        /// }<br/>
        /// </example>
        /// </summary>
        public abstract void Dispose();

        public bool IsDisposed { get; private set; }

        public void AddOnDispose(Action? callback)
        {
            if (IsDisposed || callback == null) return;
            OnDisposeCallback += callback;
        }

        public void RemoveOnDispose(Action? callback)
        {
            if (IsDisposed || callback == null) return;
            OnDisposeCallback -= callback;
        }

        public void Bind(IXDDisposable? disposable = null)
        {
            // check disposed state
            if (IsDisposed) return;

            // unbind
            if (_bindTarget != null)
            {
                _bindTarget.RemoveOnDispose(Dispose);
                _bindTarget = null;
            }
            if (disposable == null) return;

            // bind
            if (disposable.IsDisposed) Dispose();
            else (_bindTarget = disposable).AddOnDispose(Dispose);
        }

        /// <summary>
        /// 在 dispose 逻辑结束后请务必调用该方法
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected void OnDisposed()
        {
            // check disposed
            if (IsDisposed) return;
            IsDisposed = true;

            // unbind
            if (_bindTarget != null)
            {
                _bindTarget.RemoveOnDispose(Dispose);
                _bindTarget = null;
            }

            // dispose cb
            OnDisposeCallback?.Invoke();
            OnDisposeCallback = null;
        }

        /// <summary>
        /// 如果 dispose 未调用, 这里做一个兜底
        /// </summary>
        ~XDDisposableObjectBase() => Dispose();

        private IXDDisposable? _bindTarget;
        private event Action? OnDisposeCallback;
    }
}