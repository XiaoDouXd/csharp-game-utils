using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace XD.Common.Event
{
    public partial class EventDispatcher
    {
        // ==================== 0 param ====================

        private sealed class EventHandler0 : EventHandlerBase, IInvoker<Args0>
        {
            private Action? _act;
            public EventHandler0(EventDispatcher d, in EventIdentify id, Action act) : base(d, id) { _act = act; }
            public void Invoke(in Args0 args) => _act?.Invoke();
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register(EventKey key, Action? act) =>
            act == null ? null : RegisterInternal(new EventHandler0(this, key.Id, act));

        public void Broadcast(EventKey key) => DispatchTyped(key.Id, new Args0());

        // ==================== 1 param ====================

        private sealed class EventHandler<T0> : EventHandlerBase, IInvoker<Args<T0>>
        {
            private Action<T0?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id, Action<T0?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0> args) => _act?.Invoke(args.P0);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0>(EventKey<T0> key, Action<T0?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0>(this, key.Id, act));

        public void Broadcast<T0>(EventKey<T0> key, T0? p0) =>
            DispatchTyped(key.Id, new Args<T0>(p0));

        // ==================== 2 params ====================

        private sealed class EventHandler<T0, T1> : EventHandlerBase, IInvoker<Args<T0, T1>>
        {
            private Action<T0?, T1?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id, Action<T0?, T1?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1> args) => _act?.Invoke(args.P0, args.P1);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1>(EventKey<T0, T1> key, Action<T0?, T1?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1>(this, key.Id, act));

        public void Broadcast<T0, T1>(EventKey<T0, T1> key, T0? p0, T1? p1) =>
            DispatchTyped(key.Id, new Args<T0, T1>(p0, p1));

        // ==================== 3 params ====================

        private sealed class EventHandler<T0, T1, T2> : EventHandlerBase, IInvoker<Args<T0, T1, T2>>
        {
            private Action<T0?, T1?, T2?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id, Action<T0?, T1?, T2?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2> args) => _act?.Invoke(args.P0, args.P1, args.P2);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2>(EventKey<T0, T1, T2> key, Action<T0?, T1?, T2?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2>(this, key.Id, act));

        public void Broadcast<T0, T1, T2>(EventKey<T0, T1, T2> key, T0? p0, T1? p1, T2? p2) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2>(p0, p1, p2));

        // ==================== 4 params ====================

        private sealed class EventHandler<T0, T1, T2, T3> : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3>>
        {
            private Action<T0?, T1?, T2?, T3?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id, Action<T0?, T1?, T2?, T3?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3> args) => _act?.Invoke(args.P0, args.P1, args.P2, args.P3);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3>(EventKey<T0, T1, T2, T3> key,
            Action<T0?, T1?, T2?, T3?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3>(EventKey<T0, T1, T2, T3> key,
            T0? p0, T1? p1, T2? p2, T3? p3) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3>(p0, p1, p2, p3));

        // ==================== 5 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4> : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id, Action<T0?, T1?, T2?, T3?, T4?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4>(EventKey<T0, T1, T2, T3, T4> key,
            Action<T0?, T1?, T2?, T3?, T4?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4>(EventKey<T0, T1, T2, T3, T4> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4>(p0, p1, p2, p3, p4));

        // ==================== 6 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5> : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4, T5>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?, T5?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id, Action<T0?, T1?, T2?, T3?, T4?, T5?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4, T5> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4, args.P5);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4, T5>(EventKey<T0, T1, T2, T3, T4, T5> key,
            Action<T0?, T1?, T2?, T3?, T4?, T5?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4, T5>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4, T5>(EventKey<T0, T1, T2, T3, T4, T5> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4, T5>(p0, p1, p2, p3, p4, p5));

        // ==================== 7 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6> : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4, T5, T6>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id, Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4, T5, T6> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4, args.P5, args.P6);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4, T5, T6>(EventKey<T0, T1, T2, T3, T4, T5, T6> key,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4, T5, T6>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4, T5, T6>(EventKey<T0, T1, T2, T3, T4, T5, T6> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6>(p0, p1, p2, p3, p4, p5, p6));

        // ==================== 8 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7>
            : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4, T5, T6, T7>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id,
                Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4, T5, T6, T7> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4, args.P5, args.P6, args.P7);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4, T5, T6, T7>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7> key,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7>(p0, p1, p2, p3, p4, p5, p6, p7));

        // ==================== 9 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8>
            : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4, T5, T6, T7, T8>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id,
                Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4, T5, T6, T7, T8> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4, args.P5, args.P6, args.P7, args.P8);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> key,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8>(p0, p1, p2, p3, p4, p5, p6, p7, p8));

        // ==================== 10 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
            : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id,
                Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4, args.P5, args.P6, args.P7, args.P8, args.P9);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> key,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9));

        // ==================== 11 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
            : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id,
                Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4, args.P5, args.P6, args.P7, args.P8, args.P9, args.P10);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> key,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10));

        // ==================== 12 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
            : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id,
                Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4, args.P5, args.P6, args.P7, args.P8, args.P9, args.P10, args.P11);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> key,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11));

        // ==================== 13 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
            : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id,
                Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4, args.P5, args.P6, args.P7, args.P8, args.P9, args.P10, args.P11, args.P12);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> key,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12));

        // ==================== 14 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
            : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id,
                Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4, args.P5, args.P6, args.P7, args.P8, args.P9, args.P10, args.P11, args.P12, args.P13);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> key,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));

        // ==================== 15 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
            : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id,
                Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4, args.P5, args.P6, args.P7, args.P8, args.P9, args.P10, args.P11, args.P12, args.P13, args.P14);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> key,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14));

        // ==================== 16 params ====================

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
            : EventHandlerBase, IInvoker<Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>
        {
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?>? _act;
            public EventHandler(EventDispatcher d, in EventIdentify id,
                Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?> act) : base(d, id) { _act = act; }
            public void Invoke(in Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> args) =>
                _act?.Invoke(args.P0, args.P1, args.P2, args.P3, args.P4, args.P5, args.P6, args.P7, args.P8, args.P9, args.P10, args.P11, args.P12, args.P13, args.P14, args.P15);
            protected override void ClearCallback() => _act = null;
        }

        public IEventSubscription? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> key,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?>? act) =>
            act == null ? null : RegisterInternal(new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, key.Id, act));

        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14, T15? p15) =>
            DispatchTyped(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15));
    }
}
