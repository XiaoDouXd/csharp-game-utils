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

        public void OnTick(float _, float __)
        {
            FrameDelay();
            FrameAsync();
            return;

            void FrameDelay()
            {
                if (_frameDelayQueue.Count <= 0) return;
                while (_frameDelayQueue.TryDequeue(out var task)) task.Do(this);
            }

            void FrameAsync()
            {
                if (_frameAsyncQueue.Count <= 0) return;
                _timeWatcher.Restart();
                try
                {
                    var isContinue = true;
                    while (isContinue && _frameAsyncQueue.TryDequeue(out var task))
                    {
                        task.Do(this);
                        isContinue = _timeWatcher.ElapsedMilliseconds < MaxFrameAsyncOvertimeMs &&
                                     _frameAsyncQueue.Count <= MaxFrameAsyncHoldSize;
                    }
                }
                catch (Exception)
                {
                    _timeWatcher.Stop();
                    throw;
                }
                _timeWatcher.Stop();
            }
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

