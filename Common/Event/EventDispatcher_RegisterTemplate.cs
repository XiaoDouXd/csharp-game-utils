using System;
using System.Collections.Generic;

namespace XD.Common.Event
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    public partial class EventDispatcher
    {
        public IEventHandler? Register<TEnum>(TEnum eventEnum, Action? act, IEventHandler? listener = null) where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register(in EventIdentify id, Action? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke();

            public override void Del() => _act = null;
            private Action? _act;
        }

        public IEventHandler? Register<TEnum, T0>(TEnum eventEnum, Action<T0?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum => Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0>(in EventIdentify id, Action<T0?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0)
                );

            public override void Del() => _act = null;
            private Action<T0?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1>(TEnum eventEnum, Action<T0?, T1?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum => Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1>(in EventIdentify id, Action<T0?, T1?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2>(TEnum eventEnum, Action<T0?, T1?, T2?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum => Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2>(in EventIdentify id, Action<T0?, T1?, T2?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3>(TEnum eventEnum, Action<T0?, T1?, T2?, T3?>? act,
            IEventHandler? listener = null) where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3>(in EventIdentify id, Action<T0?, T1?, T2?, T3?>? act,
            IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4>(TEnum eventEnum, Action<T0?, T1?, T2?, T3?, T4?>? act,
            IEventHandler? listener = null) where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4>(in EventIdentify id, Action<T0?, T1?, T2?, T3?, T4?>? act,
            IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?>? act, IEventHandler? listener = null) where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4, T5>(in EventIdentify id,
            Action<T0?, T1?, T2?, T3?, T4?, T5?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?, T5?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4),
                    EventParams.Get<T5>(pack, 5)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?>? act, IEventHandler? listener = null) where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4, T5, T6>(in EventIdentify id,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4),
                    EventParams.Get<T5>(pack, 5),
                    EventParams.Get<T6>(pack, 6)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?>? act, IEventHandler? listener = null) where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4, T5, T6, T7>(in EventIdentify id,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4),
                    EventParams.Get<T5>(pack, 5),
                    EventParams.Get<T6>(pack, 6),
                    EventParams.Get<T7>(pack, 7)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in EventIdentify id,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4),
                    EventParams.Get<T5>(pack, 5),
                    EventParams.Get<T6>(pack, 6),
                    EventParams.Get<T7>(pack, 7),
                    EventParams.Get<T8>(pack, 8)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in EventIdentify id,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4),
                    EventParams.Get<T5>(pack, 5),
                    EventParams.Get<T6>(pack, 6),
                    EventParams.Get<T7>(pack, 7),
                    EventParams.Get<T8>(pack, 8),
                    EventParams.Get<T9>(pack, 9)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in EventIdentify id,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4),
                    EventParams.Get<T5>(pack, 5),
                    EventParams.Get<T6>(pack, 6),
                    EventParams.Get<T7>(pack, 7),
                    EventParams.Get<T8>(pack, 8),
                    EventParams.Get<T9>(pack, 9),
                    EventParams.Get<T10>(pack, 10)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in EventIdentify id,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4),
                    EventParams.Get<T5>(pack, 5),
                    EventParams.Get<T6>(pack, 6),
                    EventParams.Get<T7>(pack, 7),
                    EventParams.Get<T8>(pack, 8),
                    EventParams.Get<T9>(pack, 9),
                    EventParams.Get<T10>(pack, 10),
                    EventParams.Get<T11>(pack, 11)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in EventIdentify id,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4),
                    EventParams.Get<T5>(pack, 5),
                    EventParams.Get<T6>(pack, 6),
                    EventParams.Get<T7>(pack, 7),
                    EventParams.Get<T8>(pack, 8),
                    EventParams.Get<T9>(pack, 9),
                    EventParams.Get<T10>(pack, 10),
                    EventParams.Get<T11>(pack, 11),
                    EventParams.Get<T12>(pack, 12)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in EventIdentify id,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4),
                    EventParams.Get<T5>(pack, 5),
                    EventParams.Get<T6>(pack, 6),
                    EventParams.Get<T7>(pack, 7),
                    EventParams.Get<T8>(pack, 8),
                    EventParams.Get<T9>(pack, 9),
                    EventParams.Get<T10>(pack, 10),
                    EventParams.Get<T11>(pack, 11),
                    EventParams.Get<T12>(pack, 12),
                    EventParams.Get<T13>(pack, 13)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in EventIdentify id,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4),
                    EventParams.Get<T5>(pack, 5),
                    EventParams.Get<T6>(pack, 6),
                    EventParams.Get<T7>(pack, 7),
                    EventParams.Get<T8>(pack, 8),
                    EventParams.Get<T9>(pack, 9),
                    EventParams.Get<T10>(pack, 10),
                    EventParams.Get<T11>(pack, 11),
                    EventParams.Get<T12>(pack, 12),
                    EventParams.Get<T13>(pack, 13),
                    EventParams.Get<T14>(pack, 14)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum =>
            Register(EventIdentify.Get(eventEnum), act, listener);
        public IEventHandler? Register<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in EventIdentify id,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?>? act, IEventHandler? listener = null)
        {
            if (act == null || CheckHandlerFailure(id, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, id, listener);
            e.Bind(act);
            return Register(e);
        }

        private sealed class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : EventHandlerBase
        {
            public EventHandler(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener) : base(dispatcher, id, listener) {}
            public void Bind(Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3),
                    EventParams.Get<T4>(pack, 4),
                    EventParams.Get<T5>(pack, 5),
                    EventParams.Get<T6>(pack, 6),
                    EventParams.Get<T7>(pack, 7),
                    EventParams.Get<T8>(pack, 8),
                    EventParams.Get<T9>(pack, 9),
                    EventParams.Get<T10>(pack, 10),
                    EventParams.Get<T11>(pack, 11),
                    EventParams.Get<T12>(pack, 12),
                    EventParams.Get<T13>(pack, 13),
                    EventParams.Get<T14>(pack, 14),
                    EventParams.Get<T15>(pack, 15)
                );

            public override void Del() => _act = null;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?>? _act;
        }
    }
}