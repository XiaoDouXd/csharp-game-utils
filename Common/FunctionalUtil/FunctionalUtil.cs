// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace XD.Common.FunctionalUtil
{
    /// <summary>
    /// 空类
    /// </summary>
    public class Empty {}

    public static class F
    {
        #region Default Func

        public static void Empty() {}
        public static void Empty<T>(T _) {}
        public static void Empty<T1, T2>(T1 _0, T2 _1) {}

        public static bool Not(bool v) => !v;
        public static bool Or(bool a, bool b) => a || b;
        public static bool And(bool a, bool b) => a && b;

        #endregion
    }
}