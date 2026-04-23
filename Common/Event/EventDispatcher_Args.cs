// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace XD.Common.Event
{
    /// <summary>
    /// 强类型广播的参数载体. 所有 Args_N 都是 readonly struct, 分发时以 in 引用传递, 不发生装箱或拷贝.
    /// </summary>
    internal readonly struct Args0 {}

    internal readonly struct Args<T0>
    {
        public readonly T0? P0;
        public Args(T0? p0) { P0 = p0; }
    }

    internal readonly struct Args<T0, T1>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public Args(T0? p0, T1? p1) { P0 = p0; P1 = p1; }
    }

    internal readonly struct Args<T0, T1, T2>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public Args(T0? p0, T1? p1, T2? p2) { P0 = p0; P1 = p1; P2 = p2; }
    }

    internal readonly struct Args<T0, T1, T2, T3>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3) { P0 = p0; P1 = p1; P2 = p2; P3 = p3; }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4)
        { P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4; }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4, T5>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public readonly T5? P5;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5)
        { P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4; P5 = p5; }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4, T5, T6>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public readonly T5? P5;
        public readonly T6? P6;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6)
        { P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4; P5 = p5; P6 = p6; }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4, T5, T6, T7>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public readonly T5? P5;
        public readonly T6? P6;
        public readonly T7? P7;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7)
        { P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4; P5 = p5; P6 = p6; P7 = p7; }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public readonly T5? P5;
        public readonly T6? P6;
        public readonly T7? P7;
        public readonly T8? P8;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8)
        { P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4; P5 = p5; P6 = p6; P7 = p7; P8 = p8; }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public readonly T5? P5;
        public readonly T6? P6;
        public readonly T7? P7;
        public readonly T8? P8;
        public readonly T9? P9;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9)
        { P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4; P5 = p5; P6 = p6; P7 = p7; P8 = p8; P9 = p9; }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public readonly T5? P5;
        public readonly T6? P6;
        public readonly T7? P7;
        public readonly T8? P8;
        public readonly T9? P9;
        public readonly T10? P10;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10)
        {
            P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4;
            P5 = p5; P6 = p6; P7 = p7; P8 = p8; P9 = p9;
            P10 = p10;
        }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public readonly T5? P5;
        public readonly T6? P6;
        public readonly T7? P7;
        public readonly T8? P8;
        public readonly T9? P9;
        public readonly T10? P10;
        public readonly T11? P11;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11)
        {
            P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4;
            P5 = p5; P6 = p6; P7 = p7; P8 = p8; P9 = p9;
            P10 = p10; P11 = p11;
        }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public readonly T5? P5;
        public readonly T6? P6;
        public readonly T7? P7;
        public readonly T8? P8;
        public readonly T9? P9;
        public readonly T10? P10;
        public readonly T11? P11;
        public readonly T12? P12;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12)
        {
            P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4;
            P5 = p5; P6 = p6; P7 = p7; P8 = p8; P9 = p9;
            P10 = p10; P11 = p11; P12 = p12;
        }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public readonly T5? P5;
        public readonly T6? P6;
        public readonly T7? P7;
        public readonly T8? P8;
        public readonly T9? P9;
        public readonly T10? P10;
        public readonly T11? P11;
        public readonly T12? P12;
        public readonly T13? P13;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13)
        {
            P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4;
            P5 = p5; P6 = p6; P7 = p7; P8 = p8; P9 = p9;
            P10 = p10; P11 = p11; P12 = p12; P13 = p13;
        }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public readonly T5? P5;
        public readonly T6? P6;
        public readonly T7? P7;
        public readonly T8? P8;
        public readonly T9? P9;
        public readonly T10? P10;
        public readonly T11? P11;
        public readonly T12? P12;
        public readonly T13? P13;
        public readonly T14? P14;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14)
        {
            P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4;
            P5 = p5; P6 = p6; P7 = p7; P8 = p8; P9 = p9;
            P10 = p10; P11 = p11; P12 = p12; P13 = p13; P14 = p14;
        }
    }

    internal readonly struct Args<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
    {
        public readonly T0? P0;
        public readonly T1? P1;
        public readonly T2? P2;
        public readonly T3? P3;
        public readonly T4? P4;
        public readonly T5? P5;
        public readonly T6? P6;
        public readonly T7? P7;
        public readonly T8? P8;
        public readonly T9? P9;
        public readonly T10? P10;
        public readonly T11? P11;
        public readonly T12? P12;
        public readonly T13? P13;
        public readonly T14? P14;
        public readonly T15? P15;
        public Args(T0? p0, T1? p1, T2? p2, T3? p3, T4? p4, T5? p5, T6? p6, T7? p7, T8? p8, T9? p9, T10? p10, T11? p11, T12? p12, T13? p13, T14? p14, T15? p15)
        {
            P0 = p0; P1 = p1; P2 = p2; P3 = p3; P4 = p4;
            P5 = p5; P6 = p6; P7 = p7; P8 = p8; P9 = p9;
            P10 = p10; P11 = p11; P12 = p12; P13 = p13; P14 = p14; P15 = p15;
        }
    }
}
