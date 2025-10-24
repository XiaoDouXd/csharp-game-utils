using System;
using XD.Common.FunctionalUtil;
using XD.Common.ScopeUtil;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ConvertConstructorToMemberInitializers

namespace XD.GameModule.Module.MTick
{
    /// <summary>
    /// 刷新器
    /// </summary>
    public sealed class Ticker : XDObject
    {
        public static int PoolSize { get; set; } = 512;

        public static Ticker New()
        {
            var obj = NewInner();
            E.Tick?.Register(obj._inner, false);
            return obj;
        }

        public static Ticker New(Action act)
        {
            var obj = NewInner();
            obj.SetTarget(act);
            E.Tick?.Register(obj._inner, false);
            return obj;
        }

        public static Ticker New(Action<float, float> act)
        {
            var obj = NewInner();
            obj.SetTarget(act);
            E.Tick?.Register(obj._inner, false);
            return obj;
        }

        public static void Del(Ticker? obj)
        {
            if (obj == null) return;
            obj.SetTarget();
            E.Tick?.Unregister(obj._inner);
        }

        private static Ticker NewInner()
        {
            var obj = new Ticker();
            obj.Interval = 0;
            return obj;
        }

        public float Interval
        {
            get => _inner.TickInterval;
            set => _inner.TickInterval = value;
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

        ~Ticker()
        {
            SetTarget();
            E.Tick?.Unregister(_inner);
        }

        private Ticker() => _inner = new TickerInner();

        private class TickerInner : ITick
        {
            public float TickInterval { get; set; }

            public Action<float, float> Cb = F.Empty;
            public void OnTick(float dt, float rdt) => Cb(dt, rdt);

            public void OnUpdateWhite(float _, float __) => Action!();
            public void OnUpdateWithParam(float dt, float rdt) => ActionWithParam!(dt, rdt);

            public Action? Action;
            public Action<float, float>? ActionWithParam;
        }
        private readonly TickerInner _inner;
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class PhysicalTicker : XDObject
    {
        public static int PoolSize { get; set; } = 512;

        public static PhysicalTicker New()
        {
            var obj = NewInner();
            E.Tick?.Register(obj._inner);
            return obj;
        }

        public static PhysicalTicker? New(Action? act)
        {
            if (act == null) return null;
            var obj = NewInner();
            obj.SetTarget(act);
            E.Tick?.Register(obj._inner);
            return obj;
        }

        public static PhysicalTicker? New(Action<float, float>? act)
        {
            if (act == null) return null;
            var obj = NewInner();
            obj.SetTarget(act);
            E.Tick?.Register(obj._inner);
            return obj;
        }

        public static void Del(PhysicalTicker? obj)
        {
            if (obj == null) return;
            obj.SetTarget();
            E.Tick?.Unregister(obj._inner);
        }

        private static PhysicalTicker NewInner()
        {
            var obj = new PhysicalTicker();
            obj.Interval = 0;
            return obj;
        }

        public float Interval
        {
            get => _inner.PhysicalTickInterval;
            set => _inner.PhysicalTickInterval = value;
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

        ~PhysicalTicker()
        {
            SetTarget();
            E.Tick?.Unregister(_inner);
        }

        private PhysicalTicker() => _inner = new TickerInner();

        /// <summary>
        ///
        /// </summary>
        private class TickerInner : IPhysicalTicker
        {
            public TickerInner() => Cb = F.Empty;
            public float PhysicalTickInterval { get; set; }

            public Action<float, float> Cb;
            public void OnPhysicalTick(float dt, float rdt) => Cb(dt, rdt);

            public void OnUpdateWhite(float _, float __) => Action!();
            public void OnUpdateWithParam(float dt, float rdt) => ActionWithParam!(dt, rdt);

            public Action? Action;
            public Action<float, float>? ActionWithParam;
        }
        private readonly TickerInner _inner;
    }

    public sealed class LateTicker : XDObject
    {
        public static int PoolSize { get; set; } = 512;

        public static LateTicker New()
        {
            var obj = NewInner();
            E.Tick?.Register(obj._inner);
            return obj;
        }

        public static LateTicker? New(Action? act)
        {
            if (act == null) return null;
            var obj = NewInner();
            obj.SetTarget(act);
            E.Tick?.Register(obj._inner);
            return obj;
        }

        public static LateTicker? New(Action<float, float>? act)
        {
            if (act == null) return null;
            var obj = NewInner();
            obj.SetTarget(act);
            E.Tick?.Register(obj._inner);
            return obj;
        }

        public static void Del(LateTicker? obj)
        {
            if (obj == null) return;
            obj.SetTarget();
            E.Tick?.Unregister(obj._inner);
        }

        private static LateTicker NewInner()
        {
            var obj = new LateTicker();
            obj.Interval = 0;
            return obj;
        }

        public float Interval
        {
            get => _inner.LateTickInterval;
            set => _inner.LateTickInterval = value;
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

        ~LateTicker()
        {
            SetTarget();
            E.Tick?.Unregister(_inner);
        }

        private LateTicker() => _inner = new TickerInner();

        private class TickerInner : ILateTicker
        {
            public TickerInner() => Cb = F.Empty;
            public float LateTickInterval { get; set; }

            public Action<float, float> Cb;
            public void OnLateTick(float dt, float rdt) => Cb(dt, rdt);

            public void OnUpdateWhite(float _, float __) => Action!();
            public void OnUpdateWithParam(float dt, float rdt) => ActionWithParam!(dt, rdt);

            public Action? Action;
            public Action<float, float>? ActionWithParam;
        }
        private readonly TickerInner _inner;
    }
}