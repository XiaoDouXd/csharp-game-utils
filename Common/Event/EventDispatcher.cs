using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using XD.Common.ScopeUtil;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace XD.Common.Event
{
    /// <summary>
    /// 事件订阅句柄. 订阅者通过 Dispose 解除订阅.
    /// </summary>
    public interface IEventSubscription : IDisposableWithFlag {}

    /// <summary>
    /// 事件分发器核心 (强类型, 零参数装箱).
    /// <para>
    /// 设计概要:
    /// - 每个事件由 <see cref="EventIdentify"/> 索引, <see cref="EventMap"/> 保存其所有 handler 的链表.
    /// - Handler 内部持有具体 <see cref="Action"/> 委托, 通过 <see cref="IInvoker{TArgs}"/> 接口与具体参数 struct 对接.
    /// - 广播走栈上 struct <c>TArgs</c> + 泛型派发, 对值类型参数零装箱.
    /// - 如广播端与订阅端的参数签名不匹配, 会直接跳过该 handler 并 (仅 Debug) 打日志, 不会静默传入 default.
    /// </para>
    /// </summary>
    public partial class EventDispatcher : IDisposableWithFlag
    {
        // ---------------- EventIdentify ----------------

        /// <summary>
        /// 事件标识符. 由 enum 值 + enum 类型唯一确定.
        /// </summary>
        public readonly struct EventIdentify : IEquatable<EventIdentify>
        {
            private readonly long _id;
            private readonly Type? _type;
            private readonly int _hash;

            private EventIdentify(long id, Type type)
            {
                _id = id;
                _type = type;
                // 预计算 hash, 避免每次广播都进入 Type.GetHashCode.
                _hash = HashCode.Combine(id, type);
            }

            public static EventIdentify Get<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
                new(enumVal.AsLong(), typeof(TEnum));

            public bool Equals(EventIdentify o) => o._id == _id && o._type == _type;
            public override bool Equals(object? obj) => obj is EventIdentify v && Equals(v);
            public override int GetHashCode() => _hash;

            public static bool operator ==(EventIdentify a, EventIdentify b) => a.Equals(b);
            public static bool operator !=(EventIdentify a, EventIdentify b) => !a.Equals(b);

            public override string ToString() => _type == null ? "<invalid>" : $"{_type.Name}({_id})";
        }

        // ---------------- Handler 基类 ----------------

        /// <summary>
        /// Handler 基类. 具体子类由 <c>Register(EventKey...)</c> 按参数个数动态生成.
        /// </summary>
        protected internal abstract class EventHandlerBase : IEventSubscription
        {
            public EventIdentify Id { get; }
            public EventDispatcher Dispatcher { get; }
            public LinkedListNode<EventHandlerBase>? NodeEventMap { get; set; }
            public bool IsDisposed { get; private set; }

            protected EventHandlerBase(EventDispatcher dispatcher, in EventIdentify id)
            {
                Id = id;
                Dispatcher = dispatcher;
            }

            /// <summary>
            /// 释放回调引用, 供 Dispatcher 在 Clear/Dispose 时统一清理.
            /// </summary>
            protected abstract void ClearCallback();

            public void Dispose()
            {
                if (IsDisposed || Dispatcher.IsDisposed) return;
                IsDisposed = true;
                Dispatcher.UnregisterInternal(this);
                ClearCallback();
            }

            /// <summary>
            /// 供 Dispatcher 在 Clear/Dispose 时调用, 仅置位 + 清回调, 不再从 Map 删除.
            /// </summary>
            internal void DisposeByDispatcher()
            {
                if (IsDisposed) return;
                IsDisposed = true;
                NodeEventMap = null;
                ClearCallback();
            }
        }

        /// <summary>
        /// 强类型分发接口. 具体 Handler 通过实现 <c>IInvoker&lt;Args_N&gt;</c> 来支持零装箱分发.
        /// </summary>
        protected internal interface IInvoker<TArgs> where TArgs : struct
        {
            void Invoke(in TArgs args);
        }

        // ---------------- 核心数据结构 ----------------

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 事件 Id → handler 链表.
        /// <para>
        /// 线程模型:
        /// - <see cref="EventMap"/> 本身的 add / remove 依赖 <see cref="ConcurrentDictionary{TKey,TValue}"/> 保证线程安全,
        ///   不需要外层锁.
        /// - 每条 handler 链表 (<see cref="LinkedList{T}"/>) 的结构修改 (AddLast/Remove) 必须在 <c>lock(list)</c> 下进行.
        ///   这把锁同时保护 "判断 list 是否空 → 把对应 id 从 <see cref="EventMap"/> 删除" 这一段临界区.
        /// - <see cref="EventHandlerBase.NodeEventMap"/> 的赋值与链表插入/移除绑定在 <c>lock(list)</c> 中一并完成.
        /// </para>
        /// </summary>
        protected readonly ConcurrentDictionary<EventIdentify, LinkedList<EventHandlerBase>> EventMap = new();

        private static readonly ConcurrentStack<List<EventHandlerBase>> SnapshotPool = new();
        private static int _maxSnapshotCache = 32;

        /// <summary>
        /// 广播时 handler 快照列表的缓存上限, 避免高频广播下 List 重复分配.
        /// </summary>
        public static int MaxSnapshotCache
        {
            get => _maxSnapshotCache;
            set
            {
                _maxSnapshotCache = value;
                SnapshotPool.Clear();
            }
        }

        // ---------------- 注册内部 ----------------

        /// <summary>
        /// 将 handler 挂入 EventMap.
        /// </summary>
        private IEventSubscription? RegisterInternal(EventHandlerBase? handler)
        {
            if (IsDisposed || handler == null) return null;

            var id = handler.Id;
            while (true)
            {
                // 取已有或新建链表. ConcurrentDictionary 保证在多线程竞争下只有一个实例胜出.
                var list = EventMap.GetOrAdd(id, static _ => new LinkedList<EventHandlerBase>());

                // 在链表锁内完成插入; 同时要防止在我们加锁之前,
                // 另一个线程的 Unregister 已经判断 list 为空并把 id 从 EventMap 中移除.
                lock (list)
                {
                    // 若此 list 已被其他线程从 EventMap 移除, 则换下一轮重新 GetOrAdd.
                    if (!EventMap.TryGetValue(id, out var current) || !ReferenceEquals(current, list)) continue;

                    var node = new LinkedListNode<EventHandlerBase>(handler);
                    handler.NodeEventMap = node;
                    list.AddLast(node);
                    return handler;
                }
            }
        }

        private void UnregisterInternal(EventHandlerBase handler)
        {
            var node = handler.NodeEventMap;
            if (node == null) return;
            var list = node.List;
            if (list == null) return;

            lock (list)
            {
                // 双检: 并发下节点可能已被其他操作从链表中移除.
                if (handler.NodeEventMap != node) return;

                list.Remove(node);
                handler.NodeEventMap = null;

                // list 变空时, 顺便把 id 从 EventMap 删除.
                // RegisterInternal 的双检机制会处理 "拿到旧 list 但已被移除" 的边界.
                if (list.Count <= 0) EventMap.TryRemove(handler.Id, out _);
            }
        }

        // ---------------- 公共 Unregister / Clear / Dispose ----------------

        /// <summary>
        /// 反注册指定 subscription (等价于 subscription.Dispose()).
        /// </summary>
        public bool Unregister(IEventSubscription? subscription)
        {
            if (subscription is not EventHandlerBase handler) return false;
            if (handler.IsDisposed || handler.Dispatcher != this) return false;
            handler.Dispose();
            return true;
        }

        /// <summary>
        /// 反注册指定事件 id 下的所有 handler.
        /// </summary>
        public bool Unregister<TEnum>(TEnum eventEnum) where TEnum : unmanaged, Enum =>
            Unregister(EventIdentify.Get(eventEnum));

        public bool Unregister(in EventIdentify id)
        {
            if (IsDisposed) return false;
            if (!EventMap.TryRemove(id, out var list) || list == null) return false;

            // 持链表锁遍历, 避免与并发 Register/Unregister 冲突.
            lock (list)
            {
                foreach (var h in list) h.DisposeByDispatcher();
                list.Clear();
            }
            return true;
        }

        /// <summary>
        /// 清空所有订阅.
        /// </summary>
        public virtual void Clear()
        {
            // 快照 Keys 避免边遍历边改.
            foreach (var id in EventMap.Keys)
            {
                if (!EventMap.TryRemove(id, out var list) || list == null) continue;
                lock (list)
                {
                    foreach (var h in list) h.DisposeByDispatcher();
                    list.Clear();
                }
            }
        }

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Clear();
        }

        // ---------------- 分发核心 ----------------

        /// <summary>
        /// 强类型广播. 对订阅者逐个用 <c>IInvoker&lt;TArgs&gt;</c> 接口分发; 签名不匹配的 handler 会被跳过.
        /// </summary>
        protected void DispatchTyped<TArgs>(in EventIdentify id, in TArgs args) where TArgs : struct
        {
            if (IsDisposed) return;
            if (!EventMap.TryGetValue(id, out var list)) return;

            // 先在链表锁下拷贝快照, 然后脱锁执行 handler —— 避免 handler 内部 Register/Unregister 形成嵌套锁,
            // 也避免 handler 执行耗时影响到其他事件的注册.
            var snapshot = RentSnapshot();
            try
            {
                lock (list) { snapshot.AddRange(list); }

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < snapshot.Count; i++)
                {
                    var handler = snapshot[i];
                    if (handler.IsDisposed) continue;

                    if (handler is IInvoker<TArgs> invoker)
                    {
                        try { invoker.Invoke(in args); }
                        catch (Exception e) { SafeLog(e, "Dispatch", id); }
                    }
                    else
                    {
                        // 参数签名不一致, 这是使用方的 bug, 仅提醒, 不抛.
                        ReportSignatureMismatch<TArgs>(id, handler);
                    }
                }
            }
            finally { ReturnSnapshot(snapshot); }
        }

        // ---------------- Snapshot 池 ----------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<EventHandlerBase> RentSnapshot() =>
            SnapshotPool.TryPop(out var v) ? v : new List<EventHandlerBase>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReturnSnapshot(List<EventHandlerBase> list)
        {
            list.Clear();
            if (SnapshotPool.Count < _maxSnapshotCache) SnapshotPool.Push(list);
        }

        // ---------------- 日志 ----------------

        internal static void SafeLog(Exception e, string tag, in EventIdentify id)
        {
            try { Log.Log.Error($"[EventDispatcher][{tag}][{id}] {e}"); }
            catch { /* swallow: 日志本身也不能再抛 */ }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void ReportSignatureMismatch<TArgs>(in EventIdentify id, EventHandlerBase handler)
            where TArgs : struct
        {
            try
            {
                Log.Log.Warning($"[EventDispatcher][SignatureMismatch][{id}] " +
                                $"broadcast args '{typeof(TArgs).Name}' not accepted by handler '{handler.GetType().Name}'");
            }
            catch { /* swallow */ }
        }
    }
}
