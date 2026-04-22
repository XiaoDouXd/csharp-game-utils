using System;
using XD.Common.AsyncUtil;
using XD.Common.FunctionalUtil;
using XD.Common.ScopeUtil;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

namespace XD.GameModule.Module.MTick
{
    // ============================================================================================
    // Ticker / PhysicalTicker / LateTicker 的弱引用语义
    // --------------------------------------------------------------------------------------------
    // 设计目标: 当外部业务代码不再引用 Ticker 时, tick 链应该能被 GC 回收并自动停止 tick,
    //         避免 "业务对象通过 Action/method group 被 _regInfoDict 强引用" 导致的隐式泄漏.
    //
    // 引用链:
    //     业务对象 (e.g. Character)
    //         ↓ 字段
    //       Ticker: 业务持有, 唯一强引用源; 同时实现了 Cb/Action 的承载
    //         ↑ 弱引用
    //       WeakForwarder: 注册到 E.Tick 的就是这个 forwarder
    //         ↑ _regInfoDict 强引用
    //       TickModule
    //
    // 自动清理:
    //     Ticker 被 GC -> WeakForwarder.TryGetTarget 失败 -> forwarder 下次 tick 时 Unregister 自己
    //     -> _regInfoDict 不再持有 forwarder -> forwarder 也被 GC. 一次/两次 GC 内链路完整释放.
    // ============================================================================================

    /// <summary>
    /// 基于 <see cref="TickModule"/> 的 Tick 驱动包装. 每个 <see cref="Ticker"/> 对应一条 ITick 订阅.
    /// <para/>
    /// 生命周期:
    ///   - 业务显式 <see cref="Del"/> / <see cref="Dispose"/>: 立即 Unregister (强路径);
    ///   - 业务丢弃引用: forwarder 自动检测并 Unregister (弱路径, 约两轮 GC);
    ///   - 不使用终结器, 避免 GC 线程触碰 E.Tick.
    /// </summary>
    public sealed class Ticker : XDObject
    {
        public static Ticker New()
        {
            var obj = new Ticker();
            E.Tick?.Register(obj._forwarder, false);
            return obj;
        }

        public static Ticker New(Action act)
        {
            var obj = new Ticker();
            obj.SetTarget(act);
            E.Tick?.Register(obj._forwarder, false);
            return obj;
        }

        public static Ticker New(Action<float, float> act)
        {
            var obj = new Ticker();
            obj.SetTarget(act);
            E.Tick?.Register(obj._forwarder, false);
            return obj;
        }

        public static void Del(Ticker? obj)
        {
            if (obj == null) return;
            obj.SetTarget();
            E.Tick?.Unregister(obj._forwarder);
        }

        /// <summary> tick 间隔 (秒, 0 表示每帧). </summary>
        public float Interval { get; set; }

        public void SetTarget()
        {
            _cb = F.Empty;
            _action = null;
            _actionWithParam = null;
        }

        public void SetTarget(Action? act)
        {
            _action = act;
            _actionWithParam = null;
            _cb = act == null ? F.Empty : OnUpdateNoParam;
        }

        public void SetTarget(Action<float, float>? act)
        {
            _action = null;
            _actionWithParam = act;
            _cb = act == null ? F.Empty : OnUpdateWithParam;
        }

        public override void Dispose()
        {
            Del(this);
            base.Dispose();
        }

        private Ticker()
        {
            _cb = F.Empty;
            _forwarder = new WeakForwarder(this);
        }

        // 实际回调: 通过 _cb 统一分派, 避免每帧判断 Action / Action<,> 分支.
        private void OnTickInner(float dt, float rdt) => _cb(dt, rdt);
        private void OnUpdateNoParam(float _, float __) => _action!();
        private void OnUpdateWithParam(float dt, float rdt) => _actionWithParam!(dt, rdt);

        private Action<float, float> _cb;
        private Action? _action;
        private Action<float, float>? _actionWithParam;

        // forwarder 持有 WeakReference<Ticker>, 注册到 E.Tick. 不会强引用 this.
        private readonly WeakForwarder _forwarder;

        /// <summary>
        /// 注册到 E.Tick 的 ITick 转发器. 对 Ticker 是弱引用,
        /// 一旦 Ticker 被 GC, 自身在下一次 tick 时主动 Unregister.
        /// </summary>
        private sealed class WeakForwarder : ITick
        {
            public float TickInterval =>
                _weak.TryGetTarget(out var owner) ? owner.Interval : 0f;

            public WeakForwarder(Ticker owner) => _weak = new WeakReference<Ticker>(owner);

            public void OnTick(float dt, float rdt)
            {
                if (_weak.TryGetTarget(out var owner))
                {
                    owner.OnTickInner(dt, rdt);
                    return;
                }
                E.Tick?.Unregister(this);
            }

            private readonly WeakReference<Ticker> _weak;
        }
    }

    public sealed class PhysicalTicker : XDObject
    {
        public static PhysicalTicker New()
        {
            var obj = new PhysicalTicker();
            E.Tick?.Register(obj._forwarder, false);
            return obj;
        }

        public static PhysicalTicker? New(Action? act)
        {
            if (act == null) return null;
            var obj = new PhysicalTicker();
            obj.SetTarget(act);
            E.Tick?.Register(obj._forwarder, false);
            return obj;
        }

        public static PhysicalTicker? New(Action<float, float>? act)
        {
            if (act == null) return null;
            var obj = new PhysicalTicker();
            obj.SetTarget(act);
            E.Tick?.Register(obj._forwarder, false);
            return obj;
        }

        public static void Del(PhysicalTicker? obj)
        {
            if (obj == null) return;
            obj.SetTarget();
            E.Tick?.Unregister(obj._forwarder);
        }

        public float Interval { get; set; }

        public void SetTarget()
        {
            _cb = F.Empty;
            _action = null;
            _actionWithParam = null;
        }

        public void SetTarget(Action? act)
        {
            _action = act;
            _actionWithParam = null;
            _cb = act == null ? F.Empty : OnUpdateNoParam;
        }

        public void SetTarget(Action<float, float>? act)
        {
            _action = null;
            _actionWithParam = act;
            _cb = act == null ? F.Empty : OnUpdateWithParam;
        }

        public override void Dispose()
        {
            Del(this);
            base.Dispose();
        }

        private PhysicalTicker()
        {
            _cb = F.Empty;
            _forwarder = new WeakForwarder(this);
        }

        private void OnPhysicalTickInner(float dt, float rdt) => _cb(dt, rdt);
        private void OnUpdateNoParam(float _, float __) => _action!();
        private void OnUpdateWithParam(float dt, float rdt) => _actionWithParam!(dt, rdt);

        private Action<float, float> _cb;
        private Action? _action;
        private Action<float, float>? _actionWithParam;

        private readonly WeakForwarder _forwarder;

        private sealed class WeakForwarder : IPhysicalTicker
        {
            public float PhysicalTickInterval =>
                _weak.TryGetTarget(out var owner) ? owner.Interval : 0f;

            public WeakForwarder(PhysicalTicker owner) => _weak = new WeakReference<PhysicalTicker>(owner);

            public void OnPhysicalTick(float dt, float rdt)
            {
                if (_weak.TryGetTarget(out var owner))
                {
                    owner.OnPhysicalTickInner(dt, rdt);
                    return;
                }
                E.Tick?.Unregister(this);
            }

            private readonly WeakReference<PhysicalTicker> _weak;
        }
    }

    public sealed class LateTicker : XDObject
    {
        public static LateTicker New()
        {
            var obj = new LateTicker();
            E.Tick?.Register(obj._forwarder, false);
            return obj;
        }

        public static LateTicker? New(Action? act)
        {
            if (act == null) return null;
            var obj = new LateTicker();
            obj.SetTarget(act);
            E.Tick?.Register(obj._forwarder, false);
            return obj;
        }

        public static LateTicker? New(Action<float, float>? act)
        {
            if (act == null) return null;
            var obj = new LateTicker();
            obj.SetTarget(act);
            E.Tick?.Register(obj._forwarder, false);
            return obj;
        }

        public static void Del(LateTicker? obj)
        {
            if (obj == null) return;
            obj.SetTarget();
            E.Tick?.Unregister(obj._forwarder);
        }

        public float Interval { get; set; }

        public void SetTarget()
        {
            _cb = F.Empty;
            _action = null;
            _actionWithParam = null;
        }

        public void SetTarget(Action? act)
        {
            _action = act;
            _actionWithParam = null;
            _cb = act == null ? F.Empty : OnUpdateNoParam;
        }

        public void SetTarget(Action<float, float>? act)
        {
            _action = null;
            _actionWithParam = act;
            _cb = act == null ? F.Empty : OnUpdateWithParam;
        }

        public override void Dispose()
        {
            Del(this);
            base.Dispose();
        }

        private LateTicker()
        {
            _cb = F.Empty;
            _forwarder = new WeakForwarder(this);
        }

        private void OnLateTickInner(float dt, float rdt) => _cb(dt, rdt);
        private void OnUpdateNoParam(float _, float __) => _action!();
        private void OnUpdateWithParam(float dt, float rdt) => _actionWithParam!(dt, rdt);

        private Action<float, float> _cb;
        private Action? _action;
        private Action<float, float>? _actionWithParam;

        private readonly WeakForwarder _forwarder;

        private sealed class WeakForwarder : ILateTicker
        {
            public float LateTickInterval =>
                _weak.TryGetTarget(out var owner) ? owner.Interval : 0f;

            public WeakForwarder(LateTicker owner) => _weak = new WeakReference<LateTicker>(owner);

            public void OnLateTick(float dt, float rdt)
            {
                if (_weak.TryGetTarget(out var owner))
                {
                    owner.OnLateTickInner(dt, rdt);
                    return;
                }
                E.Tick?.Unregister(this);
            }

            private readonly WeakReference<LateTicker> _weak;
        }
    }

    public sealed class Delay : XDObject, IAwaitable<IAwaiter>, IAwaiter
    {
        public Delay() : this(0) {}
        public Delay(float delay) => E.Tick?.Delay(Complete, delay)?.Bind(this);

        public IAwaiter GetAwaiter() => this;
        public void OnCompleted(Action continuation)
        {
            if (IsCompleted) continuation();
            else OnCompletedDelegate += continuation;
        }

        public bool IsCompleted { get; private set; }
        public void GetResult() {}

        private void Complete()
        {
            if (IsCompleted) return;
            IsCompleted = true;
            OnCompletedDelegate?.Invoke();
            OnCompletedDelegate = null;
        }
        private event Action? OnCompletedDelegate;
    }
}
