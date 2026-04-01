using System;
using System.Collections.Generic;

namespace XD.Common.Event
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    public partial class EventDispatcher
    {
        public void Broadcast<TEnum>(TEnum eventEnum) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum));
        public void Broadcast(in EventIdentify id) => Broadcast(id, EmptyParamList);

        public void Broadcast<TEnum, T0>(TEnum eventEnum, T0? p0) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0);
        public void Broadcast<T0>(in EventIdentify id, T0? p0)
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1>(TEnum eventEnum, T0? p0, T1? p1) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1);
        public void Broadcast<T0, T1>(in EventIdentify id, T0? p0, T1? p1)
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2>(TEnum eventEnum, T0? p0, T1? p1, T2? p2) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2);
        public void Broadcast<T0, T1, T2>(in EventIdentify id, T0? p0, T1? p1, T2? p2)
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3);
        public void Broadcast<T0, T1, T2, T3>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3)
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4);
        public void Broadcast<T0, T1, T2, T3, T4>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4)
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5);
        public void Broadcast<T0, T1, T2, T3, T4, T5>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5)
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6);
        public void Broadcast<T0, T1, T2, T3, T4, T5, T6>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6)
        {
            var paramList = NewParamList();
            paramList.Add(NewValuePack(p0));
            paramList.Add(NewValuePack(p1));
            paramList.Add(NewValuePack(p2));
            paramList.Add(NewValuePack(p3));
            paramList.Add(NewValuePack(p4));
            paramList.Add(NewValuePack(p5));
            paramList.Add(NewValuePack(p6));
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7);
        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7)
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
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8);
        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8)
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
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9);
        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9)
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
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10)
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
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11)
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
            paramList.Add(NewValuePack(p11));
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12)
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
            paramList.Add(NewValuePack(p11));
            paramList.Add(NewValuePack(p12));
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13)
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
            paramList.Add(NewValuePack(p11));
            paramList.Add(NewValuePack(p12));
            paramList.Add(NewValuePack(p13));
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14)
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
            paramList.Add(NewValuePack(p11));
            paramList.Add(NewValuePack(p12));
            paramList.Add(NewValuePack(p13));
            paramList.Add(NewValuePack(p14));
            Broadcast(id, paramList);
        }

        public void Broadcast<TEnum, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(TEnum eventEnum, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14, T15? p15) where TEnum : unmanaged, Enum =>
            Broadcast(EventIdentify.Get(eventEnum), p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
        public void Broadcast<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in EventIdentify id, T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14, T15? p15)
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
            paramList.Add(NewValuePack(p11));
            paramList.Add(NewValuePack(p12));
            paramList.Add(NewValuePack(p13));
            paramList.Add(NewValuePack(p14));
            paramList.Add(NewValuePack(p15));
            Broadcast(id, paramList);
        }

        private void Broadcast(in EventIdentify id, List<ParamPack> paramList)
        {
            if (!EventMap.TryGetValue(id, out var eventHandlerBases)) return;

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
    }
}
