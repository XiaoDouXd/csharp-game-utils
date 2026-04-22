using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using XD.Common.ScopeUtil;

namespace XD.Common.Event
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
    // ReSharper disable UnusedType.Global
    public partial class EventDispatcherAsync: EventDispatcher, ITick
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
                        try { task.Do(this); }
                        catch (Exception e) { SafeLog(e, "FrameDelay handler"); }

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
                        try { task.Do(this); }
                        catch (Exception e) { SafeLog(e, "FrameAsync handler"); }

                        isContinue = _timeWatcher.ElapsedMilliseconds < MaxFrameAsyncOvertimeMs &&
                                     _frameAsyncQueue.Count <= MaxFrameAsyncHoldSize;
                    }
                }
                finally { _timeWatcher.Stop(); }
            }
        }

        /// <summary>
        /// 清理订阅表的同时, 清空两个 pending 队列. DeInit/Reinit 时调用.
        /// 注意: 基类 <see cref="EventDispatcher.Clear"/> 不是 virtual, 这里用 new 覆盖;
        /// 项目内对 I 的静态类型均为 EventDispatcherAsync, 不会走到基类版本.
        /// </summary>
        public new void Clear()
        {
            base.Clear();
            // 清空遗留的事件任务, 避免跨 Init 边界分发到已销毁的 listener.
            while (_frameDelayQueue.TryDequeue(out _)) { }
            while (_frameAsyncQueue.TryDequeue(out _)) { }
        }

        private void BroadcastFrameAsync(in EventIdentify id, List<ParamPack> paramList)
        {
            if (!EventMap.TryGetValue(id, out var eventHandlerBases)) return;

            foreach (var handler in eventHandlerBases)
                _frameAsyncQueue.Enqueue(new EventTaskInfo(handler, paramList));
            _frameAsyncQueue.Enqueue(new EventTaskInfo(null, paramList));
        }

        private void BroadcastFrameDelay(in EventIdentify id, List<ParamPack> paramList)
        {
            if (!EventMap.TryGetValue(id, out var eventHandlerBases)) return;

            foreach (var handler in eventHandlerBases)
                _frameDelayQueue.Enqueue(new EventTaskInfo(handler, paramList));
            _frameDelayQueue.Enqueue(new EventTaskInfo(null, paramList));
        }

        private static void SafeLog(Exception e, string tag)
        {
            try { Log.Log.Error($"[EventDispatcherAsync][{tag}] {e}"); }
            catch { /* swallow: 日志本身也不能再抛 */ }
        }

        private readonly struct EventTaskInfo
        {
            private EventHandlerBase? Handler { get; }
            private List<ParamPack> ParamList { get; }

            public EventTaskInfo(EventHandlerBase? handler, List<ParamPack> paramList)
            {
                Handler = handler;
                ParamList = paramList;
            }

            public void Do(EventDispatcherAsync dispatcher)
            {
                if (Handler == null)
                {
                    if (ParamList == null) return;
                    dispatcher.DelParamList(ParamList);
                    return;
                }
                Handler.Run(ParamList);
            }
        }

        private readonly Stopwatch _timeWatcher = new();
        private readonly ConcurrentQueue<EventTaskInfo> _frameAsyncQueue = new();
        private readonly ConcurrentQueue<EventTaskInfo> _frameDelayQueue = new();
    }
}
