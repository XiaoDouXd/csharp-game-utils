using System;

namespace XD.Common.Event
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedType.Global
    public partial class EventDispatcherAsync
    {
        #region broadcast frame async

        public void BroadcastFrameAsync<TEnum>(TEnum eventEnum) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(eventEnum, EmptyParamList);
        public void BroadcastFrameAsync(in EventIdentify id) => BroadcastFrameAsync(id, EmptyParamList);

        public void BroadcastFrameAsync<TEnum, T0>(TEnum eventEnum, T0? p0) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0);
        public void BroadcastFrameAsync<T0>(in EventIdentify id, T0? p0)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1>(TEnum eventEnum, T0? p0, T1? p1) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1);
        public void BroadcastFrameAsync<T0, T1>(in EventIdentify id, T0? p0, T1? p1)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2>(TEnum eventEnum, T0? p0, T1? p1, T2? p2) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2);
        public void BroadcastFrameAsync<T0, T1, T2>(in EventIdentify id, T0? p0, T1? p1, T2? p2)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3);
        public void BroadcastFrameAsync<T0, T1, T2, T3>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            paramList.Add(PackParam(p11));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            paramList.Add(PackParam(p11));
            paramList.Add(PackParam(p12));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            paramList.Add(PackParam(p11));
            paramList.Add(PackParam(p12));
            paramList.Add(PackParam(p13));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            paramList.Add(PackParam(p11));
            paramList.Add(PackParam(p12));
            paramList.Add(PackParam(p13));
            paramList.Add(PackParam(p14));
            BroadcastFrameAsync(id, paramList);
        }

        public void BroadcastFrameAsync<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14, T15? p15) where TEnum : unmanaged, Enum =>
            BroadcastFrameAsync(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14, T15? p15)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            paramList.Add(PackParam(p11));
            paramList.Add(PackParam(p12));
            paramList.Add(PackParam(p13));
            paramList.Add(PackParam(p14));
            paramList.Add(PackParam(p15));
            BroadcastFrameAsync(id, paramList);
        }

        #endregion

        #region broadcast frame delay

        public void BroadcastFrameDelay<TEnum>(TEnum eventEnum) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum));
        public void BroadcastFrameDelay(in EventIdentify id) =>
            BroadcastFrameDelay(id, EmptyParamList);

        public void BroadcastFrameDelay<TEnum, T0>(TEnum eventEnum, T0? p0) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0);
        public void BroadcastFrameDelay<T0>(in EventIdentify id, T0? p0)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1>(TEnum eventEnum, T0? p0, T1? p1) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1);
        public void BroadcastFrameDelay<T0, T1>(in EventIdentify id, T0? p0, T1? p1)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2>(TEnum eventEnum, T0? p0, T1? p1, T2? p2) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2);
        public void BroadcastFrameDelay<T0, T1, T2>(in EventIdentify id, T0? p0, T1? p1, T2? p2)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3);
        public void BroadcastFrameDelay<T0, T1, T2, T3>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            paramList.Add(PackParam(p11));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            paramList.Add(PackParam(p11));
            paramList.Add(PackParam(p12));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            paramList.Add(PackParam(p11));
            paramList.Add(PackParam(p12));
            paramList.Add(PackParam(p13));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            paramList.Add(PackParam(p11));
            paramList.Add(PackParam(p12));
            paramList.Add(PackParam(p13));
            paramList.Add(PackParam(p14));
            BroadcastFrameDelay(id, paramList);
        }

        public void BroadcastFrameDelay<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14, T15? p15) where TEnum : unmanaged, Enum =>
            BroadcastFrameDelay(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14, T15? p15)
        {
            var paramList = NewParamList();
            paramList.Add(PackParam(p0));
            paramList.Add(PackParam(p1));
            paramList.Add(PackParam(p2));
            paramList.Add(PackParam(p3));
            paramList.Add(PackParam(p4));
            paramList.Add(PackParam(p5));
            paramList.Add(PackParam(p6));
            paramList.Add(PackParam(p7));
            paramList.Add(PackParam(p8));
            paramList.Add(PackParam(p9));
            paramList.Add(PackParam(p10));
            paramList.Add(PackParam(p11));
            paramList.Add(PackParam(p12));
            paramList.Add(PackParam(p13));
            paramList.Add(PackParam(p14));
            paramList.Add(PackParam(p15));
            BroadcastFrameDelay(id, paramList);
        }

        #endregion
    }
}
