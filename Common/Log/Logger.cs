using System;

namespace XD.Common.Log
{
    public static class Log
    {
        public enum ELogLevel
        {
            // 允许所有级别的打印
            All = 0,

            Info    = 1,
            Warning = 2,
            Error   = 3,

            // 禁止所有级别的 Log 打印
            None
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static ELogLevel Level { get; set; } =
#if RELEASE_SHIPPING
            ELogLevel.None;
#else
            ELogLevel.Info;
#endif

        public static void Info(object? info)
        {
            if (CheckLevel(ELogLevel.Info)) return;
            Logger?.Invoke(ELogLevel.Info, info ?? "null");
        }

        public static void Warning(object? info)
        {
            if (CheckLevel(ELogLevel.Warning)) return;
            Logger?.Invoke(ELogLevel.Warning, info ?? "null");
        }

        public static void Error(object? info)
        {
            if (CheckLevel(ELogLevel.Error)) return;
            Logger?.Invoke(ELogLevel.Error, info ?? "null");
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public static Action<ELogLevel, object?>? Logger = null;

        private static bool CheckLevel(ELogLevel lv) => lv < Level;
    }
}