#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XD.Common;
using XD.Common.ScopeUtil;
using E = XD.GameModule.Module.E;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable MemberCanBePrivate.Global

namespace XD.GameModule.Module.MEvent
{
    public interface IEventHandler : IDisposableWithFlag {}

    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class EventModule
    {
        #region event handler holder

        public bool Unregister(IEventHandler? listener)
        {
            if (listener == null) return false;
            if (!_listenerMap.TryGetValue(listener, out var handlerList)) return false;
            lock (_eventMap)
            {
                foreach (var handler in handlerList)
                {
                    if (handler.NodeEventMap == null) continue;
                    var nodeList = handler.NodeEventMap.List;
                    nodeList?.Remove(handler.NodeEventMap);
                    if (nodeList is { Count: <= 0 }) _eventMap.TryRemove(handler.Id, out _);
                }
            }
            _listenerMap.TryRemove(listener, out _);
            return true;
        }

        public bool Unregister<TEnum>(TEnum eventEnum) where TEnum : unmanaged, Enum => Unregister(eventEnum, null);
        public bool Unregister<TEnum>(TEnum eventEnum, IEventHandler? listener) where TEnum : unmanaged, Enum
        {
            var eventId = EventIdentify.Get(eventEnum);
            if (listener != null) return UnregisterListenerInner(listener, eventId);

            // ReSharper disable once InconsistentlySynchronizedField
            if (!_eventMap.TryGetValue(eventId, out var handlerLinkedList)) return false;
            var handlerBase = handlerLinkedList.Count > 0 ? handlerLinkedList.First : null;
            var deletedOne = false;
            while (handlerBase != null)
            {
                var next = handlerBase.Next;
                deletedOne |= UnregisterListenerInner(handlerBase.Value, eventId);
                handlerBase = next;
            }
            return deletedOne;

            bool UnregisterListenerInner(IEventHandler listenerInner, in EventIdentify eventIdentify)
            {
                if (!_listenerMap.TryGetValue(listenerInner, out var handlerList)) return false;
                for (var i = 0; i < handlerList.Count; i++)
                {
                    var handler = handlerList[i];
                    if (handler.Id != eventIdentify) continue;
                    if (handler.NodeEventMap != null)
                    {
                        var nodeList = handler.NodeEventMap.List;
                        nodeList?.Remove(handler.NodeEventMap);

                        // ReSharper disable once InconsistentlySynchronizedField
                        if (nodeList is { Count: <= 0 }) _eventMap.TryRemove(eventIdentify, out _);
                    }
                    handlerList.RemoveAt(i);
                    if (handlerList.Count <= 0) _listenerMap.TryRemove(listenerInner, out _);
                    return true;
                }
                return false;
            }
        }

        private bool CheckHandlerFailure<TEnum>(TEnum eventEnum, IEventHandler? listener) where TEnum : unmanaged, Enum
        {
            if (listener == null) return false;
            var eventId = EventIdentify.Get(eventEnum);
            if (!_listenerMap.TryGetValue(listener, out var handlerList)) return false;
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var handler in handlerList) if (handler.Id == eventId) return true;
            return false;
        }

        private IEventHandler RegisterInner<TEnum>(TEnum eventEnum, EventHandlerBase handler) where TEnum : unmanaged, Enum
        {
            var listener = handler.Listener ??= handler;
            var id = handler.Id = EventIdentify.Get(eventEnum);

            lock (_eventMap)
            {
                if (!_eventMap.TryGetValue(id, out var handlerHandlerList))
                    handlerHandlerList = _eventMap[id] = new LinkedList<EventHandlerBase>();
                var handlerNode = handler.NodeEventMap = new LinkedListNode<EventHandlerBase>(handler);
                handlerHandlerList.AddLast(handlerNode);

                if (!_listenerMap.TryGetValue(listener, out var handlerList))
                    handlerList = _listenerMap[listener] = new List<EventHandlerBase>();
                handlerList.Add(handler);
            }
            return handler.Listener;
        }

        private readonly ConcurrentDictionary<IEventHandler, List<EventHandlerBase>> _listenerMap = new();
        private readonly ConcurrentDictionary<EventIdentify, LinkedList<EventHandlerBase>> _eventMap = new();

        private readonly struct EventIdentify : IEquatable<EventIdentify>
        {
            private readonly long _val;
            private readonly Type? _type;

            private EventIdentify(long val, Type type)
            {
                _val = val;
                _type = type;
            }

            public static EventIdentify Get<T>(T enumVal) where T : unmanaged, Enum =>
                new(enumVal.AsLong(), typeof(T));

            public bool Equals(EventIdentify o) => o._val == _val && o._type == _type;
            public override bool Equals(object? obj) => obj is EventIdentify v && Equals(v);
            public override int GetHashCode() => HashCode.Combine(_val, _type == null ? 0 : _type.GetHashCode());

            public static bool operator ==(EventIdentify a, EventIdentify b) => a.Equals(b);
            public static bool operator !=(EventIdentify a, EventIdentify b) => !(a == b);
        }

        private abstract class EventHandlerBase : XDDisposableObjectBase, IEventHandler
        {
            public EventIdentify Id { get; set; }
            public abstract object? Source { get; }

            public IEventHandler? Listener { get; set; }
            public LinkedListNode<EventHandlerBase>? NodeEventMap { get; set; }

            public abstract void Del();
            public abstract void Run(IReadOnlyList<ParamPack> pack);

            public override void Dispose()
            {
                if (IsDisposed) return;
                E.Event?.Unregister(this);
                OnDisposed();
            }
        }

        #endregion

        #region handler def

        public IEventHandler? Register<TEnum>(TEnum eventEnum, Action? act, IEventHandler? listener = null) where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private sealed class EventHandler : EventHandlerBase
        {
            public void Bind(Action? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke();

            public override void Del() => _act = null;
            public override object? Source => _act;
            private Action? _act;
        }

        #region gen code

        public IEventHandler? Register<TEnum, T0>(TEnum eventEnum, Action<T0?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler<T0>();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private class EventHandler<T0> : EventHandlerBase
        {
            public void Bind(Action<T0?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0)
                );

            public override void Del() => _act = null;
            public override object? Source => _act;
            private Action<T0?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1>(TEnum eventEnum, Action<T0?, T1?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler<T0, T1>();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private class EventHandler<T0, T1> : EventHandlerBase
        {
            public void Bind(Action<T0?, T1?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1)
                );

            public override void Del() => _act = null;
            public override object? Source => _act;
            private Action<T0?, T1?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2>(TEnum eventEnum, Action<T0?, T1?, T2?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler<T0, T1, T2>();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private class EventHandler<T0, T1, T2> : EventHandlerBase
        {
            public void Bind(Action<T0?, T1?, T2?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2)
                );

            public override void Del() => _act = null;
            public override object? Source => _act;
            private Action<T0?, T1?, T2?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3>(TEnum eventEnum, Action<T0?, T1?, T2?, T3?>? act,
            IEventHandler? listener = null) where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3>();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private class EventHandler<T0, T1, T2, T3> : EventHandlerBase
        {
            public void Bind(Action<T0?, T1?, T2?, T3?>? act = null) => _act = act;
            public override void Run(IReadOnlyList<ParamPack> pack) =>
                _act?.Invoke(
                    EventParams.Get<T0>(pack, 0),
                    EventParams.Get<T1>(pack, 1),
                    EventParams.Get<T2>(pack, 2),
                    EventParams.Get<T3>(pack, 3)
                );

            public override void Del() => _act = null;
            public override object? Source => _act;
            private Action<T0?, T1?, T2?, T3?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4>(TEnum eventEnum, Action<T0?, T1?, T2?, T3?, T4?>? act,
            IEventHandler? listener = null) where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4>();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private class EventHandler<T0, T1, T2, T3, T4> : EventHandlerBase
        {
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
            public override object? Source => _act;
            private Action<T0?, T1?, T2?, T3?, T4?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?>? act, IEventHandler? listener = null) where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5>();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private class EventHandler<T0, T1, T2, T3, T4, T5> : EventHandlerBase
        {
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
            public override object? Source => _act;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?>? act, IEventHandler? listener = null) where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6>();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private class EventHandler<T0, T1, T2, T3, T4, T5, T6> : EventHandlerBase
        {
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
            public override object? Source => _act;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?>? act, IEventHandler? listener = null) where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7>();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7> : EventHandlerBase
        {
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
            public override object? Source => _act;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8>();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8> : EventHandlerBase
        {
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
            public override object? Source => _act;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : EventHandlerBase
        {
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
            public override object? Source => _act;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>? _act;
        }

        public IEventHandler? Register<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TEnum eventEnum,
            Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>? act, IEventHandler? listener = null)
            where TEnum : unmanaged, Enum
        {
            if (act == null || CheckHandlerFailure(eventEnum, listener)) return null;
            var e = new EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
            e.Bind(act);
            e.Listener = listener;
            return RegisterInner(eventEnum, e);
        }

        private class EventHandler<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : EventHandlerBase
        {
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
            public override object? Source => _act;
            private Action<T0?, T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>? _act;
        }

        #endregion

        #endregion
    }
}