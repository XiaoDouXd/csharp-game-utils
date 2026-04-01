using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XD.Common.ScopeUtil;

namespace XD.Common.Event
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedMemberInSuper.Global
    // ReSharper disable UnusedMethodReturnValue.Global
    public partial class EventDispatcher : IDisposableWithFlag
    {
        public interface IEventHandler : IDisposableWithFlag
        {
            public EventDispatcher Dispatcher { get; }
        }

        public readonly struct EventIdentify : IEquatable<EventIdentify>
        {
            private readonly long _id;
            private readonly Type? _type;

            private EventIdentify(long id, Type type)
            {
                _id = id;
                _type = type;
            }

            public static EventIdentify Get<T>(T enumVal) where T : unmanaged, Enum =>
                new(enumVal.AsLong(), typeof(T));

            public bool Equals(EventIdentify o) => o._id == _id && o._type == _type;
            public override bool Equals(object? obj) => obj is EventIdentify v && Equals(v);
            public override int GetHashCode() => HashCode.Combine(_id, _type == null ? 0 : _type.GetHashCode());

            public static bool operator ==(EventIdentify a, EventIdentify b) => a.Equals(b);
            public static bool operator !=(EventIdentify a, EventIdentify b) => !(a == b);
        }

        protected abstract class EventHandlerBase : XDDisposableObjectBase, IEventHandler
        {
            public EventIdentify Id { get; }
            public IEventHandler Listener { get; }
            public EventDispatcher Dispatcher { get; }
            public LinkedListNode<EventHandlerBase>? NodeEventMap { get; set; }

            protected EventHandlerBase(EventDispatcher dispatcher, in EventIdentify id, IEventHandler? listener)
            {
                Id = id;
                Listener = listener ?? this;
                Dispatcher = dispatcher;
            }

            public abstract void Del();
            public abstract void Run(IReadOnlyList<ParamPack> pack);

            public override void Dispose()
            {
                if (IsDisposed || Dispatcher.IsDisposed) return;
                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                Dispatcher.Unregister(Id, this);
                OnDisposed();
            }
        }

        public bool IsDisposed { get; private set; }

        public bool Unregister(IEventHandler? listener)
        {
            if (IsDisposed) return false;
            if (listener == null) return false;
            if (!ListenerMap.TryGetValue(listener, out var handlerList)) return false;
            lock (EventMap)
            {
                foreach (var handler in handlerList)
                {
                    if (handler.NodeEventMap == null) continue;
                    var nodeList = handler.NodeEventMap.List;
                    nodeList?.Remove(handler.NodeEventMap);
                    if (nodeList is { Count: <= 0 }) EventMap.TryRemove(handler.Id, out _);
                }
            }
            ListenerMap.TryRemove(listener, out _);
            return true;
        }

        public bool Unregister<TEnum>(TEnum eventEnum, IEventHandler? listener = null) where TEnum : unmanaged, Enum =>
            Unregister(EventIdentify.Get(eventEnum), listener);
        public bool Unregister(in EventIdentify eventId, IEventHandler? listener)
        {
            if (IsDisposed) return false;
            if (listener != null) return UnregisterListenerInner(listener, eventId);

            // ReSharper disable once InconsistentlySynchronizedField
            if (!EventMap.TryGetValue(eventId, out var handlerLinkedList)) return false;
            var handlerBase = handlerLinkedList.Count > 0 ? handlerLinkedList.First : null;
            var deletedOne = false;
            lock (EventMap)
            {
                while (handlerBase != null)
                {
                    var next = handlerBase.Next;
                    deletedOne |= UnregisterListenerInner(handlerBase.Value, eventId);
                    handlerBase = next;
                }
            }
            return deletedOne;

            bool UnregisterListenerInner(IEventHandler listenerInner, in EventIdentify eventIdentify)
            {
                if (!ListenerMap.TryGetValue(listenerInner, out var handlerList)) return false;
                for (var i = 0; i < handlerList.Count; i++)
                {
                    var handler = handlerList[i];
                    if (handler.Id != eventIdentify) continue;
                    if (handler.NodeEventMap != null)
                    {
                        var nodeList = handler.NodeEventMap.List;
                        nodeList?.Remove(handler.NodeEventMap);

                        // ReSharper disable once InconsistentlySynchronizedField
                        if (nodeList is { Count: <= 0 }) EventMap.TryRemove(eventIdentify, out _);
                    }
                    handlerList.RemoveAt(i);
                    if (handlerList.Count <= 0) ListenerMap.TryRemove(listenerInner, out _);
                    return true;
                }
                return false;
            }
        }

        public void Clear()
        {
            lock (EventMap)
            {
                EventMap.Clear();
                ListenerMap.Clear();
            }
        }

        public void Dispose()
        {
            IsDisposed = true;
            Clear();
        }

        private IEventHandler? Register(EventHandlerBase? handler)
        {
            if (IsDisposed || handler == null) return null;
            var listener = handler.Listener;
            lock (EventMap)
            {
                var id = handler.Id;
                if (!EventMap.TryGetValue(id, out var handlerHandlerList))
                    handlerHandlerList = EventMap[id] = new LinkedList<EventHandlerBase>();
                var handlerNode = handler.NodeEventMap = new LinkedListNode<EventHandlerBase>(handler);
                handlerHandlerList.AddLast(handlerNode);

                if (!ListenerMap.TryGetValue(listener, out var handlerList))
                    handlerList = ListenerMap[listener] = new List<EventHandlerBase>();
                handlerList.Add(handler);
            }
            return handler.Listener;
        }

        private bool CheckHandlerFailure(in EventIdentify eventId, IEventHandler? listener)
        {
            if (listener == null) return false;
            if (!ListenerMap.TryGetValue(listener, out var handlerList)) return false;
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var handler in handlerList) if (handler.Id == eventId) return true;
            return false;
        }

        protected readonly ConcurrentDictionary<IEventHandler, List<EventHandlerBase>> ListenerMap = new();
        protected readonly ConcurrentDictionary<EventIdentify, LinkedList<EventHandlerBase>> EventMap = new();
    }
}

