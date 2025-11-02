#nullable enable
using System;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin.Util
{
    internal static partial class Analyzer
    {
        // ReSharper disable ConditionIsAlwaysTrueOrFalse
        public static bool ToBool(string str)
            => str is "1" or "true" or "t" or "真" || (str is "0" or "false" or "f" or "假"
                ? false : throw Failure($"failed to cast {str} to type boolean"));
        // ReSharper restore ConditionIsAlwaysTrueOrFalse

        public static byte ToU8(string str)
            => byte.TryParse(str, out var v) ? v : throw Failure($"failed to cast {str} to type uint8");

        public static sbyte ToI8(string str)
            => sbyte.TryParse(str, out var v) ? v : throw Failure($"failed to cast {str} to type int8");

        public static ushort ToU16(string str)
            => ushort.TryParse(str, out var v) ? v : throw Failure($"failed to cast {str} to type uint16");

        public static short ToI16(string str)
            => short.TryParse(str, out var v) ? v : throw Failure($"failed to cast {str} to type int16");

        public static uint ToU32(string str)
            => uint.TryParse(str, out var v) ? v : throw Failure($"failed to cast {str} to type uint32");

        public static int ToI32(string str)
            => int.TryParse(str, out var v) ? v : throw Failure($"failed to cast {str} to type int32");

        public static ulong ToU64(string str)
            => ulong.TryParse(str, out var v) ? v : throw Failure($"failed to cast {str} to type uint64");

        public static long ToI64(string str)
            => long.TryParse(str, out var v) ? v : throw Failure($"failed to cast {str} to type int64");

        public static float ToF32(string str)
            => float.TryParse(str, out var v) ? v : throw Failure($"failed to cast {str} to type float32");

        public static double ToF64(string str)
            => double.TryParse(str, out var v) ? v : throw Failure($"failed to cast {str} to type float64");

        private static ArgumentException Failure(string? msg = null) => new(msg ?? "failure to cast type");
    }
}