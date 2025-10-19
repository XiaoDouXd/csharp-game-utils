using System;
using XD.Common.FunctionalUtil;
using XD.Common.ScopeUtil;
using E = XD.GameModule.Module.E;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ConvertConstructorToMemberInitializers

namespace XD.GameModule.Module.MUpdate
{
    /// <summary>
    /// 刷新器
    /// </summary>
    public sealed class Updater : XDObject
    {
        public static int PoolSize { get; set; } = 512;

        public static Updater New()
        {
            var obj = NewInner();
            E.Upd?.Register(obj._inner, false);
            return obj;
        }

        public static Updater New(Action act)
        {
            var obj = NewInner();
            obj.SetTarget(act);
            E.Upd?.Register(obj._inner, false);
            return obj;
        }

        public static Updater New(Action<float, float> act)
        {
            var obj = NewInner();
            obj.SetTarget(act);
            E.Upd?.Register(obj._inner, false);
            return obj;
        }

        public static void Del(Updater? obj)
        {
            if (obj == null) return;
            obj.SetTarget();
            E.Upd?.Unregister(obj._inner);
        }

        private static Updater NewInner()
        {
            var obj = new Updater();
            obj.Interval = 0;
            return obj;
        }

        public float Interval
        {
            get => _inner.UpdateInterval;
            set => _inner.UpdateInterval = value;
        }

        public void SetTarget()
        {
            _inner.Cb = F.Empty;
            _inner.Action = null;
            _inner.ActionWithParam = null;
        }

        public void SetTarget(Action? act)
        {
            _inner.Action = act;
            _inner.ActionWithParam = null;
            if (act == null) _inner.Cb = F.Empty;
            else _inner.Cb = _inner.OnUpdateWhite;
        }

        public void SetTarget(Action<float, float>? act)
        {
            _inner.Action = null;
            _inner.ActionWithParam = act;
            if (act == null) _inner.Cb = F.Empty;
            else _inner.Cb = _inner.OnUpdateWithParam;
        }

        public override void Dispose()
        {
            Del(this);
            base.Dispose();
        }

        ~Updater()
        {
            SetTarget();
            E.Upd?.Unregister(_inner);
        }

        private Updater() => _inner = new UpdaterInner();

        private class UpdaterInner : IUpdate
        {
            public float UpdateInterval { get; set; }

            public Action<float, float> Cb = F.Empty;
            public void OnUpdate(float dt, float rdt) => Cb(dt, rdt);

            public void OnUpdateWhite(float _, float __) => Action!();
            public void OnUpdateWithParam(float dt, float rdt) => ActionWithParam!(dt, rdt);

            public Action? Action;
            public Action<float, float>? ActionWithParam;
        }
        private readonly UpdaterInner _inner;
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class FixedUpdater : XDObject
    {
        public static int PoolSize { get; set; } = 512;

        public static FixedUpdater New()
        {
            var obj = NewInner();
            E.Upd?.Register(obj._inner);
            return obj;
        }

        public static FixedUpdater? New(Action? act)
        {
            if (act == null) return null;
            var obj = NewInner();
            obj.SetTarget(act);
            E.Upd?.Register(obj._inner);
            return obj;
        }

        public static FixedUpdater? New(Action<float, float>? act)
        {
            if (act == null) return null;
            var obj = NewInner();
            obj.SetTarget(act);
            E.Upd?.Register(obj._inner);
            return obj;
        }

        public static void Del(FixedUpdater? obj)
        {
            if (obj == null) return;
            obj.SetTarget();
            E.Upd?.Unregister(obj._inner);
        }

        private static FixedUpdater NewInner()
        {
            var obj = new FixedUpdater();
            obj.Interval = 0;
            return obj;
        }

        public float Interval
        {
            get => _inner.FixedUpdateInterval;
            set => _inner.FixedUpdateInterval = value;
        }

        public void SetTarget()
        {
            _inner.Cb = F.Empty;
            _inner.Action = null;
            _inner.ActionWithParam = null;
        }

        public void SetTarget(Action? act)
        {
            _inner.Action = act;
            _inner.ActionWithParam = null;
            if (act == null) _inner.Cb = F.Empty;
            else _inner.Cb = _inner.OnUpdateWhite;
        }

        public void SetTarget(Action<float, float>? act)
        {
            _inner.Action = null;
            _inner.ActionWithParam = act;
            if (act == null) _inner.Cb = F.Empty;
            else _inner.Cb = _inner.OnUpdateWithParam;
        }


        public override void Dispose()
        {
            Del(this);
            base.Dispose();
        }

        ~FixedUpdater()
        {
            SetTarget();
            E.Upd?.Unregister(_inner);
        }

        private FixedUpdater() => _inner = new UpdaterInner();

        /// <summary>
        ///
        /// </summary>
        private class UpdaterInner : IFixedUpdate
        {
            public UpdaterInner() => Cb = F.Empty;
            public float FixedUpdateInterval { get; set; }

            public Action<float, float> Cb;
            public void OnFixedUpdate(float dt, float rdt) => Cb(dt, rdt);

            public void OnUpdateWhite(float _, float __) => Action!();
            public void OnUpdateWithParam(float dt, float rdt) => ActionWithParam!(dt, rdt);

            public Action? Action;
            public Action<float, float>? ActionWithParam;
        }
        private readonly UpdaterInner _inner;
    }

    public sealed class LateUpdater : XDObject
    {
        public static int PoolSize { get; set; } = 512;

        public static LateUpdater New()
        {
            var obj = NewInner();
            E.Upd?.Register(obj._inner);
            return obj;
        }

        public static LateUpdater? New(Action? act)
        {
            if (act == null) return null;
            var obj = NewInner();
            obj.SetTarget(act);
            E.Upd?.Register(obj._inner);
            return obj;
        }

        public static LateUpdater? New(Action<float, float>? act)
        {
            if (act == null) return null;
            var obj = NewInner();
            obj.SetTarget(act);
            E.Upd?.Register(obj._inner);
            return obj;
        }

        public static void Del(LateUpdater? obj)
        {
            if (obj == null) return;
            obj.SetTarget();
            E.Upd?.Unregister(obj._inner);
        }

        private static LateUpdater NewInner()
        {
            var obj = new LateUpdater();
            obj.Interval = 0;
            return obj;
        }

        public float Interval
        {
            get => _inner.LateUpdateInterval;
            set => _inner.LateUpdateInterval = value;
        }

        public void SetTarget()
        {
            _inner.Cb = F.Empty;
            _inner.Action = null;
            _inner.ActionWithParam = null;
        }

        public void SetTarget(Action? act)
        {
            _inner.Action = act;
            _inner.ActionWithParam = null;
            if (act == null) _inner.Cb = F.Empty;
            else _inner.Cb = _inner.OnUpdateWhite;
        }

        public void SetTarget(Action<float, float>? act)
        {
            _inner.Action = null;
            _inner.ActionWithParam = act;
            if (act == null) _inner.Cb = F.Empty;
            else _inner.Cb = _inner.OnUpdateWithParam;
        }

        public override void Dispose()
        {
            Del(this);
            base.Dispose();
        }

        ~LateUpdater()
        {
            SetTarget();
            E.Upd?.Unregister(_inner);
        }

        private LateUpdater() => _inner = new UpdaterInner();

        private class UpdaterInner : ILateUpdate
        {
            public UpdaterInner() => Cb = F.Empty;
            public float LateUpdateInterval { get; set; }

            public Action<float, float> Cb;
            public void OnLateUpdate(float dt, float rdt) => Cb(dt, rdt);

            public void OnUpdateWhite(float _, float __) => Action!();
            public void OnUpdateWithParam(float dt, float rdt) => ActionWithParam!(dt, rdt);

            public Action? Action;
            public Action<float, float>? ActionWithParam;
        }
        private readonly UpdaterInner _inner;
    }
}