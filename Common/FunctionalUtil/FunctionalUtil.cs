// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;

namespace XD.Common.FunctionalUtil
{
    public static class F
    {
        #region Default Func
        public static void Empty() {}
        public static void Empty<T>(T _) {}
        public static void Empty<T1, T2>(T1 _0, T2 _1) {}
        public static void Empty<T1, T2, T3>(T1 _0, T2 _1, T3 _2) {}
        public static void Empty<T1, T2, T3, T4>(T1 _0, T2 _1, T3 _2, T4 _3) {}
        public static void Empty<T1, T2, T3, T4, T5>(T1 _0, T2 _1, T3 _2, T4 _3, T5 _4) {}
        public static void Empty<T1, T2, T3, T4, T5, T6>(T1 _0, T2 _1, T3 _2, T4 _3, T5 _4, T6 _5) {}
        public static void Empty<T1, T2, T3, T4, T5, T6, T7>(T1 _0, T2 _1, T3 _2, T4 _3, T5 _4, T6 _5, T7 _6) {}
        public static void Empty<T1, T2, T3, T4, T5, T6, T7, T8>(T1 _0, T2 _1, T3 _2, T4 _3, T5 _4, T6 _6, T7 _7, T8 _8) {}
        public static void Empty<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 _0, T2 _1, T3 _2, T4 _3, T5 _4, T6 _5, T7 _6, T8 _7, T9 _8) {}
        public static void Empty<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 _0, T2 _1, T3 _2, T4 _3, T5 _4, T6 _5, T7 _6, T8 _7, T9 _8, T10 _9) {}

        public static bool Not(bool v) => !v;
        public static bool Or(bool a, bool b) => a || b;
        public static bool And(bool a, bool b) => a && b;
        #endregion

        #region Bind
        public static Action Bind<T0>(this Action<T0> f, T0 v0) => () => f(v0);
        public static Action<T1> Bind<T0, T1>(this Action<T0, T1> f, T0 v0) => t1 => f(v0, t1);
        public static Action<T1, T2> Bind<T0, T1, T2>(this Action<T0, T1, T2> f, T0 v0) => (t1, t2) => f(v0, t1, t2);
        public static Action<T1, T2, T3> Bind<T0, T1, T2, T3>(this Action<T0, T1, T2, T3> f, T0 v0) => (t1, t2, t3) => f(v0, t1, t2, t3);
        public static Action<T1, T2, T3, T4> Bind<T0, T1, T2, T3, T4>(this Action<T0, T1, T2, T3, T4> f, T0 v0) => (t1, t2, t3, t4) => f(v0, t1, t2, t3, t4);
        public static Action<T1, T2, T3, T4, T5> Bind<T0, T1, T2, T3, T4, T5>(this Action<T0, T1, T2, T3, T4, T5> f, T0 v0) => (t1, t2, t3, t4, t5) => f(v0, t1, t2, t3, t4, t5);
        public static Action<T1, T2, T3, T4, T5, T6> Bind<T0, T1, T2, T3, T4, T5, T6>(this Action<T0, T1, T2, T3, T4, T5, T6> f, T0 v0) => (t1, t2, t3, t4, t5, t6) => f(v0, t1, t2, t3, t4, t5, t6);
        public static Action<T1, T2, T3, T4, T5, T6, T7> Bind<T0, T1, T2, T3, T4, T5, T6, T7>(this Action<T0, T1, T2, T3, T4, T5, T6, T7> f, T0 v0) => (t1, t2, t3, t4, t5, t6, t7) => f(v0, t1, t2, t3, t4, t5, t6, t7);
        public static Action<T1, T2, T3, T4, T5, T6, T7, T8> Bind<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> f, T0 v0) => (t1, t2, t3, t4, t5, t6, t7, t8) => f(v0, t1, t2, t3, t4, t5, t6, t7, t8);
        public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> Bind<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> f, T0 v0) => (t1, t2, t3, t4, t5, t6, t7, t8, t9) => f(v0, t1, t2, t3, t4, t5, t6, t7, t8, t9);
        public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Bind<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> f, T0 v0) => (t1, t2, t3, t4, t5, t6, t7, t8, t9, t10) => f(v0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

        public static Action Bind<T1, T2>(this Action<T1, T2> f, T1 t1, T2 t2) => () => f(t1, t2);
        public static Action<T3> Bind<T1, T2, T3>(this Action<T1, T2, T3> f, T1 t1, T2 t2) => t3 => f(t1, t2, t3);
        public static Action<T3, T4> Bind<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> f, T1 t1, T2 t2) => (t3, t4) => f(t1, t2, t3, t4);
        public static Action<T3, T4, T5> Bind<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> f, T1 t1, T2 t2) => (t3, t4, t5) => f(t1, t2, t3, t4, t5);
        public static Action<T3, T4, T5, T6> Bind<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> f, T1 t1, T2 t2) => (t3, t4, t5, t6) => f(t1, t2, t3, t4, t5, t6);
        public static Action<T3, T4, T5, T6, T7> Bind<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> f, T1 t1, T2 t2) => (t3, t4, t5, t6, t7) => f(t1, t2, t3, t4, t5, t6, t7);
        public static Action<T3, T4, T5, T6, T7, T8> Bind<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> f, T1 t1, T2 t2) => (t3, t4, t5, t6, t7, t8) => f(t1, t2, t3, t4, t5, t6, t7, t8);
        public static Action<T3, T4, T5, T6, T7, T8, T9> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> f, T1 t1, T2 t2) => (t3, t4, t5, t6, t7, t8, t9) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        public static Action<T3, T4, T5, T6, T7, T8, T9, T10> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> f, T1 t1, T2 t2) => (t3, t4, t5, t6, t7, t8, t9, t10) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

        public static Action Bind<T1, T2, T3>(this Action<T1, T2, T3> f, T1 t1, T2 t2, T3 t3) => () => f(t1, t2, t3);
        public static Action<T4> Bind<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> f, T1 t1, T2 t2, T3 t3) => t4 => f(t1, t2, t3, t4);
        public static Action<T4, T5> Bind<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> f, T1 t1, T2 t2, T3 t3) => (t4, t5) => f(t1, t2, t3, t4, t5);
        public static Action<T4, T5, T6> Bind<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> f, T1 t1, T2 t2, T3 t3) => (t4, t5, t6) => f(t1, t2, t3, t4, t5, t6);
        public static Action<T4, T5, T6, T7> Bind<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> f, T1 t1, T2 t2, T3 t3) => (t4, t5, t6, t7) => f(t1, t2, t3, t4, t5, t6, t7);
        public static Action<T4, T5, T6, T7, T8> Bind<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> f, T1 t1, T2 t2, T3 t3) => (t4, t5, t6, t7, t8) => f(t1, t2, t3, t4, t5, t6, t7, t8);
        public static Action<T4, T5, T6, T7, T8, T9> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> f, T1 t1, T2 t2, T3 t3) => (t4, t5, t6, t7, t8, t9) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        public static Action<T4, T5, T6, T7, T8, T9, T10> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> f, T1 t1, T2 t2, T3 t3) => (t4, t5, t6, t7, t8, t9, t10) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

        public static Action Bind<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> f, T1 t1, T2 t2, T3 t3, T4 t4) => () => f(t1, t2, t3, t4);
        public static Action<T5> Bind<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> f, T1 t1, T2 t2, T3 t3, T4 t4) => t5 => f(t1, t2, t3, t4, t5);
        public static Action<T5, T6> Bind<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> f, T1 t1, T2 t2, T3 t3, T4 t4) => (t5, t6) => f(t1, t2, t3, t4, t5, t6);
        public static Action<T5, T6, T7> Bind<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> f, T1 t1, T2 t2, T3 t3, T4 t4) => (t5, t6, t7) => f(t1, t2, t3, t4, t5, t6, t7);
        public static Action<T5, T6, T7, T8> Bind<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> f, T1 t1, T2 t2, T3 t3, T4 t4) => (t5, t6, t7, t8) => f(t1, t2, t3, t4, t5, t6, t7, t8);
        public static Action<T5, T6, T7, T8, T9> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> f, T1 t1, T2 t2, T3 t3, T4 t4) => (t5, t6, t7, t8, t9) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        public static Action<T5, T6, T7, T8, T9, T10> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> f, T1 t1, T2 t2, T3 t3, T4 t4) => (t5, t6, t7, t8, t9, t10) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

        public static Action Bind<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => () => f(t1, t2, t3, t4, t5);
        public static Action<T6> Bind<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => t6 => f(t1, t2, t3, t4, t5, t6);
        public static Action<T6, T7> Bind<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => (t6, t7) => f(t1, t2, t3, t4, t5, t6, t7);
        public static Action<T6, T7, T8> Bind<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => (t6, t7, t8) => f(t1, t2, t3, t4, t5, t6, t7, t8);
        public static Action<T6, T7, T8, T9> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => (t6, t7, t8, t9) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        public static Action<T6, T7, T8, T9, T10> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => (t6, t7, t8, t9, t10) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

        public static Action Bind<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => () => f(t1, t2, t3, t4, t5, t6);
        public static Action<T7> Bind<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => t7 => f(t1, t2, t3, t4, t5, t6, t7);
        public static Action<T7, T8> Bind<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => (t7, t8) => f(t1, t2, t3, t4, t5, t6, t7, t8);
        public static Action<T7, T8, T9> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => (t7, t8, t9) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        public static Action<T7, T8, T9, T10> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => (t7, t8, t9, t10) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

        public static Action Bind<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) => () => f(t1, t2, t3, t4, t5, t6, t7);
        public static Action<T8> Bind<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) => t8 => f(t1, t2, t3, t4, t5, t6, t7, t8);
        public static Action<T8, T9> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) => (t8, t9) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        public static Action<T8, T9, T10> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) => (t8, t9, t10) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

        public static Action Bind<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) => () => f(t1, t2, t3, t4, t5, t6, t7, t8);
        public static Action<T9> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) => t9 => f(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        public static Action<T9, T10> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) => (t9, t10) => f(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

        public static Action Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) => () => f(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        public static Action<T10> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) => t10 => f(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

        public static Action Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10) => () => f(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

        public static Func<T2, TR> Bind<T1, T2, TR>(this Func<T1, T2, TR> f, T1 v) => t => f(v, t);
        public static Func<T2, T3, TR> Bind<T1, T2, T3, TR>(this Func<T1, T2, T3, TR> f, T1 v) => (t1, t2) => f(v, t1, t2);
        public static Func<T2, T3, T4, TR> Bind<T1, T2, T3, T4, TR>(this Func<T1, T2, T3, T4, TR> f, T1 v) => (t1, t2, t3) => f(v, t1, t2, t3);
        public static Func<T2, T3, T4, T5, TR> Bind<T1, T2, T3, T4, T5, TR>(this Func<T1, T2, T3, T4, T5, TR> f, T1 v) => (t1, t2, t3, t4) => f(v, t1, t2, t3, t4);
        public static Func<T2, T3, T4, T5, T6, TR> Bind<T1, T2, T3, T4, T5, T6, TR>(this Func<T1, T2, T3, T4, T5, T6, TR> f, T1 v) => (t1, t2, t3, t4, t5) => f(v, t1, t2, t3, t4, t5);
        public static Func<T2, T3, T4, T5, T6, T7, TR> Bind<T1, T2, T3, T4, T5, T6, T7, TR>(this Func<T1, T2, T3, T4, T5, T6, T7, TR> f, T1 v) => (t1, t2, t3, t4, t5, t6) => f(v, t1, t2, t3, t4, t5, t6);
        public static Func<T2, T3, T4, T5, T6, T7, T8, TR> Bind<T1, T2, T3, T4, T5, T6, T7, T8, TR>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, TR> f, T1 v) => (t1, t2, t3, t4, t5, t6, t7) => f(v, t1, t2, t3, t4, t5, t6, t7);
        public static Func<T2, T3, T4, T5, T6, T7, T8, T9, TR> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TR> f, T1 v) => (t1, t2, t3, t4, t5, t6, t7, t8) => f(v, t1, t2, t3, t4, t5, t6, t7, t8);
        public static Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, TR> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TR>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TR> f, T1 v) => (t1, t2, t3, t4, t5, t6, t7, t8, t9) => f(v, t1, t2, t3, t4, t5, t6, t7, t8, t9);
        #endregion
    }
}