using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using XD.Common.ScopeUtil;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace XD.Common.Event
{
    /// <summary>
    /// 异步事件分发器. 在同步分发基础上增加:
    /// - BroadcastFrameDelay: 事件延到下一帧 OnTick 中分发 (完整消费, 失败不影响其他任务).
    /// - BroadcastFrameAsync: 事件分帧摊薄分发 (单帧耗时或队列长度超阈值时主动让出).
    /// <para>
    /// 参数通过泛型 <c>Args&lt;...&gt;</c> struct 打包进 <see cref="DeferredTask{TArgs}"/>,
    /// 每次广播仅产生 1 次对象分配 (跨帧保存 Args 必须装箱), 不再使用 ParamPack / List 池.
    /// </para>
    /// </summary>
    public partial class EventDispatcherAsync : EventDispatcher, ITick
    {
        public int MaxFrameAsyncHoldSize { get; set; } = 128;
        public long MaxFrameAsyncOvertimeMs { get; set; } = 1000;

        /// <summary>
        /// 每帧 delay 分发的最大耗时 (ms). 防止业务一次性把大量 delay 事件推进导致卡帧.
        /// 默认值相对宽松 (delay 的语义本身就是"这一帧必须处理完"), 如业务能接受背压可调小.
        /// </summary>
        public long MaxFrameDelayOvertimeMs { get; set; } = 2000;

        public void OnTick(float _, float __)
        {
            FrameDelay();
            FrameAsync();
            return;

            void FrameDelay()
            {
                if (_frameDelayQueue.IsEmpty) return;

                // 与 FrameAsync 对称, 加入耗时保护; 单条事件异常不影响后续事件.
                _timeWatcher.Restart();
                try
                {
                    while (_frameDelayQueue.TryDequeue(out var task))
                    {
                        try { task.Run(this); }
                        catch (Exception e) { SafeAsyncLog(e, "FrameDelay handler"); }

                        if (_timeWatcher.ElapsedMilliseconds >= MaxFrameDelayOvertimeMs) break;
                    }
                }
                finally { _timeWatcher.Stop(); }
            }

            void FrameAsync()
            {
                if (_frameAsyncQueue.IsEmpty) return;

                _timeWatcher.Restart();
                try
                {
                    var isContinue = true;
                    while (isContinue && _frameAsyncQueue.TryDequeue(out var task))
                    {
                        try { task.Run(this); }
                        catch (Exception e) { SafeAsyncLog(e, "FrameAsync handler"); }

                        isContinue = _timeWatcher.ElapsedMilliseconds < MaxFrameAsyncOvertimeMs &&
                                     _frameAsyncQueue.Count <= MaxFrameAsyncHoldSize;
                    }
                }
                finally { _timeWatcher.Stop(); }
            }
        }

        /// <summary>
        /// 清理订阅表的同时, 清空两个 pending 队列. DeInit/Reinit 时调用.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            // 清空遗留的事件任务, 避免跨 Init 边界分发到已销毁的 listener.
            while (_frameDelayQueue.TryDequeue(out _)) { }
            while (_frameAsyncQueue.TryDequeue(out _)) { }
        }

        // ---------------- 内部任务抽象 ----------------

        /// <summary>
        /// 延迟/异步任务的统一接口. 每次 BroadcastFrameAsync/Delay 会产生 1 个 DeferredTask 对象.
        /// </summary>
        private interface IDeferredTask
        {
            void Run(EventDispatcherAsync dispatcher);
        }

        /// <summary>
        /// 强类型延迟任务. 持有事件 id + args, 执行时再向 EventMap 查找 handler 列表分发.
        /// </summary>
        private sealed class DeferredTask<TArgs> : IDeferredTask where TArgs : struct
        {
            public EventIdentify Id;
            public TArgs Args;

            public void Run(EventDispatcherAsync dispatcher) => dispatcher.DispatchTyped(Id, in Args);
        }

        private void EnqueueDelay<TArgs>(in EventIdentify id, in TArgs args) where TArgs : struct
        {
            if (IsDisposed) return;
            // 1 次对象分配; args struct 通过字段赋值拷贝进来, 对值类型无额外装箱.
            _frameDelayQueue.Enqueue(new DeferredTask<TArgs> { Id = id, Args = args });
        }

        private void EnqueueAsync<TArgs>(in EventIdentify id, in TArgs args) where TArgs : struct
        {
            if (IsDisposed) return;
            _frameAsyncQueue.Enqueue(new DeferredTask<TArgs> { Id = id, Args = args });
        }

        // ---------------- 日志 ----------------

        private static void SafeAsyncLog(Exception e, string tag)
        {
            try { Log.Log.Error($"[EventDispatcherAsync][{tag}] {e}"); }
            catch { /* swallow: 日志本身也不能再抛 */ }
        }

        // ---------------- 队列 ----------------

        private readonly Stopwatch _timeWatcher = new();
        private readonly ConcurrentQueue<IDeferredTask> _frameAsyncQueue = new();
        private readonly ConcurrentQueue<IDeferredTask> _frameDelayQueue = new();
    }
}
