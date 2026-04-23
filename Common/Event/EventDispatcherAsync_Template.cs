// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace XD.Common.Event
{
    public partial class EventDispatcherAsync
    {
        // ==================== FrameDelay: 0 ~ 16 params ====================

        public void BroadcastFrameDelay(EventKey key) =>
            EnqueueDelay(key.Id, new Args0());

        public void BroadcastFrameDelay<T0>(EventKey<T0> key, T0? p0) =>
            EnqueueDelay(key.Id, new Args<T0>(p0));

        public void BroadcastFrameDelay<T0, T1>(EventKey<T0, T1> key, T0? p0, T1? p1) =>
            EnqueueDelay(key.Id, new Args<T0, T1>(p0, p1));

        public void BroadcastFrameDelay<T0, T1, T2>(EventKey<T0, T1, T2> key, T0? p0, T1? p1, T2? p2) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2>(p0, p1, p2));

        public void BroadcastFrameDelay<T0, T1, T2, T3>(EventKey<T0, T1, T2, T3> key,
            T0? p0, T1? p1, T2? p2, T3? p3) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3>(p0, p1, p2, p3));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4>(EventKey<T0, T1, T2, T3, T4> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4>(p0, p1, p2, p3, p4));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5>(EventKey<T0, T1, T2, T3, T4, T5> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4, T5>(p0, p1, p2, p3, p4, p5));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6>(EventKey<T0, T1, T2, T3, T4, T5, T6> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6>(p0, p1, p2, p3, p4, p5, p6));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7>(p0, p1, p2, p3, p4, p5, p6, p7));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8>(p0, p1, p2, p3, p4, p5, p6, p7, p8));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14));

        public void BroadcastFrameDelay<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14, T15? p15) =>
            EnqueueDelay(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15));

        // ==================== FrameAsync: 0 ~ 16 params ====================

        public void BroadcastFrameAsync(EventKey key) =>
            EnqueueAsync(key.Id, new Args0());

        public void BroadcastFrameAsync<T0>(EventKey<T0> key, T0? p0) =>
            EnqueueAsync(key.Id, new Args<T0>(p0));

        public void BroadcastFrameAsync<T0, T1>(EventKey<T0, T1> key, T0? p0, T1? p1) =>
            EnqueueAsync(key.Id, new Args<T0, T1>(p0, p1));

        public void BroadcastFrameAsync<T0, T1, T2>(EventKey<T0, T1, T2> key, T0? p0, T1? p1, T2? p2) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2>(p0, p1, p2));

        public void BroadcastFrameAsync<T0, T1, T2, T3>(EventKey<T0, T1, T2, T3> key,
            T0? p0, T1? p1, T2? p2, T3? p3) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3>(p0, p1, p2, p3));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4>(EventKey<T0, T1, T2, T3, T4> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4>(p0, p1, p2, p3, p4));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5>(EventKey<T0, T1, T2, T3, T4, T5> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4, T5>(p0, p1, p2, p3, p4, p5));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6>(EventKey<T0, T1, T2, T3, T4, T5, T6> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6>(p0, p1, p2, p3, p4, p5, p6));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7>(p0, p1, p2, p3, p4, p5, p6, p7));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8>(p0, p1, p2, p3, p4, p5, p6, p7, p8));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14));

        public void BroadcastFrameAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> key,
            T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14, T15? p15) =>
            EnqueueAsync(key.Id, new Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
                p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15));
    }
}
