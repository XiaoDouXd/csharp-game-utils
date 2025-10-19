using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeProtected.Global

namespace XD.Common.ScopeUtil
{
    // ReSharper disable once InconsistentNaming
    public class XDObject : XDDisposableObjectBase
    {
        public override void Dispose() => OnDisposed();

        public XDObject() {}
        public XDObject(Action onDisposedCallback) => AddOnDispose(onDisposedCallback);
    }
}