using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace XD.GameModule.Module.MEvent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class EventModule
    {
        #region inner

        private const int MaxFrameAsyncHoldSize = 128;
        private const long MaxFrameAsyncOvertimeMs = 1000;

        private void BroadcastInner<TEnum>(TEnum eventEnum, List<ParamPack> paramList) where TEnum : unmanaged, Enum
        {
            var id = EventIdentify.Get(eventEnum);
            if (!_eventMap.TryGetValue(id, out var eventHandlerBases)) return;

            var eventHandlerList = NewTaskList(); // 虽然只有这里在用...但是为了多线程的情况考虑还是保留这个池吧
            lock (eventHandlerBases) { eventHandlerList.AddRange(eventHandlerBases); }

            try
            {
                foreach (var handler in eventHandlerBases)
                    handler.Run(paramList);
            }
            catch (Exception)
            {
                DelParamList(paramList);
                DelTaskList(eventHandlerList);
                throw;
            }

            DelParamList(paramList);
            DelTaskList(eventHandlerList);
        }

        private void BroadcastFrameAsyncInner<TEnum>(TEnum eventEnum, List<ParamPack> paramList) where TEnum : unmanaged, Enum
        {
            var id = EventIdentify.Get(eventEnum);
            if (!_eventMap.TryGetValue(id, out var eventHandlerBases)) return;

            lock (eventHandlerBases)
            {
                foreach (var handler in eventHandlerBases)
                    _frameAsyncQueue.Enqueue(new EventTaskInfo(handler, paramList));
            }
            _frameAsyncQueue.Enqueue(new EventTaskInfo(null, paramList));
        }

        private void BroadcastFrameDelayInner<TEnum>(TEnum eventEnum, List<ParamPack> paramList) where TEnum : unmanaged, Enum
        {
            var id = EventIdentify.Get(eventEnum);
            if (!_eventMap.TryGetValue(id, out var eventHandlerBases)) return;

            lock (eventHandlerBases)
            {
                foreach (var handler in eventHandlerBases)
                    _frameDelayQueue.Enqueue(new EventTaskInfo(handler, paramList));
            }
            _frameDelayQueue.Enqueue(new EventTaskInfo(null, paramList));
        }

        private void OnTick(float _, float __)
        {
            FrameDelay();
            FrameAsync();
            return;

            void FrameDelay()
            {
                if (_frameDelayQueue.Count <= 0) return;
                try
                {
                    while (_frameDelayQueue.TryDequeue(out var task))
                        task.Do(this);
                }
                catch (Exception)
                {
                    while (_frameDelayQueue.TryDequeue(out var task))
                    {
                        if (task.Handler != null) continue;
                        task.Do(this);
                    }
                    throw;
                }
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

        private readonly struct EventTaskInfo
        {
            public EventHandlerBase? Handler { get; }
            private List<ParamPack> ParamList { get; }

            public EventTaskInfo(EventHandlerBase? handler, List<ParamPack> paramList)
            {
                Handler = handler;
                ParamList = paramList;
            }

            public void Do(EventModule module)
            {
                if (Handler == null)
                {
                    if (ParamList == null) return;
                    module.DelParamList(ParamList);
                    return;
                }
                Handler.Run(ParamList);
            }
        }

        private readonly Stopwatch _timeWatcher = new();
        private readonly ConcurrentQueue<EventTaskInfo> _frameAsyncQueue = new();
        private readonly ConcurrentQueue<EventTaskInfo> _frameDelayQueue = new();

        #endregion

        #region interface

        #region broadcast

        public void Broadcast<TEnum>(TEnum eventEnum) where TEnum : unmanaged, Enum
            => BroadcastInner(eventEnum, _emptyParamList);

        public void Broadcast<TEnum, T0>(TEnum eventEnum, T0? p0) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            BroadcastInner(eventEnum, paramList);
        }

        public void Broadcast<TEnum, T0, T1>(TEnum eventEnum, T0? p0, T1? p1) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            BroadcastInner(eventEnum, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2>(TEnum eventEnum, T0? p0, T1? p1, T2? p2) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            BroadcastInner(eventEnum, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            BroadcastInner(eventEnum, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            BroadcastInner(eventEnum, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            BroadcastInner(eventEnum, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            BroadcastInner(eventEnum, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            BroadcastInner(eventEnum, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            paramList.Add(NewValuePack(p8));
            BroadcastInner(eventEnum, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            paramList.Add(NewValuePack(p8));
            paramList.Add(NewValuePack(p9));
            BroadcastInner(eventEnum, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            paramList.Add(NewValuePack(p8));
            paramList.Add(NewValuePack(p9));
            paramList.Add(NewValuePack(p10));
            BroadcastInner(eventEnum, paramList);
        }

        #endregion

        #region broadcast frame async

        public void BroadcastFrameAsync<TEnum>(TEnum eventEnum) where TEnum : unmanaged, Enum
            => BroadcastFrameAsyncInner(eventEnum, _emptyParamList);

        public void BroadcastFrameAsync<TEnum, T0>(TEnum eventEnum, T0? p0) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            BroadcastFrameAsyncInner(eventEnum, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1>(TEnum eventEnum, T0? p0, T1? p1) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            BroadcastFrameAsyncInner(eventEnum, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2>(TEnum eventEnum, T0? p0, T1? p1, T2? p2) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            BroadcastFrameAsyncInner(eventEnum, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            BroadcastFrameAsyncInner(eventEnum, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            BroadcastFrameAsyncInner(eventEnum, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            BroadcastFrameAsyncInner(eventEnum, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            BroadcastFrameAsyncInner(eventEnum, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            BroadcastFrameAsyncInner(eventEnum, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            paramList.Add(NewValuePack(p8));
            BroadcastFrameAsyncInner(eventEnum, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            paramList.Add(NewValuePack(p8));
            paramList.Add(NewValuePack(p9));
            BroadcastFrameAsyncInner(eventEnum, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            paramList.Add(NewValuePack(p8));
            paramList.Add(NewValuePack(p9));
            paramList.Add(NewValuePack(p10));
            BroadcastFrameAsyncInner(eventEnum, paramList);
        }

        #endregion

        #region broadcast frame delay

        public void BroadcastFrameDelay<TEnum>(TEnum eventEnum) where TEnum : unmanaged, Enum
            => BroadcastFrameDelayInner(eventEnum, _emptyParamList);

        public void BroadcastFrameDelay<TEnum, T0>(TEnum eventEnum, T0? p0) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            BroadcastFrameDelayInner(eventEnum, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1>(TEnum eventEnum, T0? p0, T1? p1) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            BroadcastFrameDelayInner(eventEnum, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2>(TEnum eventEnum, T0? p0, T1? p1, T2? p2) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            BroadcastFrameDelayInner(eventEnum, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            BroadcastFrameDelayInner(eventEnum, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            BroadcastFrameDelayInner(eventEnum, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            BroadcastFrameDelayInner(eventEnum, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            BroadcastFrameDelayInner(eventEnum, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            BroadcastFrameDelayInner(eventEnum, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            paramList.Add(NewValuePack(p8));
            BroadcastFrameDelayInner(eventEnum, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            paramList.Add(NewValuePack(p8));
            paramList.Add(NewValuePack(p9));
            BroadcastFrameDelayInner(eventEnum, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10) where TEnum : unmanaged, Enum
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            paramList.Add(NewValuePack(p7));
            paramList.Add(NewValuePack(p8));
            paramList.Add(NewValuePack(p9));
            paramList.Add(NewValuePack(p10));
            BroadcastFrameDelayInner(eventEnum, paramList);
        }

        #endregion

        #endregion
    }
}