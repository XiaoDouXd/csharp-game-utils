using System;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeTrailingCommaInMultilineLists

namespace XD.Common.GenericUtil
{
    [Flags] public enum EnumGroup8 : byte
    {
        Null = 0,
        Count = 8,
        All = 0xff,
        E0 = 1 << 0,
        E1 = 1 << 1,
        E2 = 1 << 2,
        E3 = 1 << 3,
        E4 = 1 << 4,
        E5 = 1 << 5,
        E6 = 1 << 6,
        E7 = 1 << 7,
    }

    [Flags] public enum EnumGroup16 : ushort
    {
        Null = 0,
        Count = 16,
        All = 0xffff,
        E00 = 1 << 00,
        E01 = 1 << 01,
        E02 = 1 << 02,
        E03 = 1 << 03,
        E04 = 1 << 04,
        E05 = 1 << 05,
        E06 = 1 << 06,
        E07 = 1 << 07,
        E08 = 1 << 08,
        E09 = 1 << 09,
        E10 = 1 << 10,
        E11 = 1 << 11,
        E12 = 1 << 12,
        E13 = 1 << 13,
        E14 = 1 << 14,
        E15 = 1 << 15,
    }

    [Flags] public enum EnumGroup32 : uint
    {
        Null = 0,
        Count = 32,
        All = 0xffffffffU,
        E00 = 1u << 00,
        E01 = 1u << 01,
        E02 = 1u << 02,
        E03 = 1u << 03,
        E04 = 1u << 04,
        E05 = 1u << 05,
        E06 = 1u << 06,
        E07 = 1u << 07,
        E08 = 1u << 08,
        E09 = 1u << 09,
        E10 = 1u << 10,
        E11 = 1u << 11,
        E12 = 1u << 12,
        E13 = 1u << 13,
        E14 = 1u << 14,
        E15 = 1u << 15,
        E16 = 1u << 16,
        E17 = 1u << 17,
        E18 = 1u << 18,
        E19 = 1u << 19,
        E20 = 1u << 20,
        E21 = 1u << 21,
        E22 = 1u << 22,
        E23 = 1u << 23,
        E24 = 1u << 24,
        E25 = 1u << 25,
        E26 = 1u << 26,
        E27 = 1u << 27,
        E28 = 1u << 28,
        E29 = 1u << 29,
        E30 = 1u << 30,
        E31 = 1u << 31,
    }

    [Flags] public enum EnumGroup64 : ulong
    {
        Null = 0,
        Count = 64,
        All = 0xfffffffffffffffful,
        E00 = 1ul << 00,
        E01 = 1ul << 01,
        E02 = 1ul << 02,
        E03 = 1ul << 03,
        E04 = 1ul << 04,
        E05 = 1ul << 05,
        E06 = 1ul << 06,
        E07 = 1ul << 07,
        E08 = 1ul << 08,
        E09 = 1ul << 09,
        E10 = 1ul << 10,
        E11 = 1ul << 11,
        E12 = 1ul << 12,
        E13 = 1ul << 13,
        E14 = 1ul << 14,
        E15 = 1ul << 15,
        E16 = 1ul << 16,
        E17 = 1ul << 17,
        E18 = 1ul << 18,
        E19 = 1ul << 19,
        E20 = 1ul << 20,
        E21 = 1ul << 21,
        E22 = 1ul << 22,
        E23 = 1ul << 23,
        E24 = 1ul << 24,
        E25 = 1ul << 25,
        E26 = 1ul << 26,
        E27 = 1ul << 27,
        E28 = 1ul << 28,
        E29 = 1ul << 29,
        E30 = 1ul << 30,
        E31 = 1ul << 31,
        E32 = 1ul << 32,
        E33 = 1ul << 33,
        E34 = 1ul << 34,
        E35 = 1ul << 35,
        E36 = 1ul << 36,
        E37 = 1ul << 37,
        E38 = 1ul << 38,
        E39 = 1ul << 39,
        E40 = 1ul << 40,
        E41 = 1ul << 41,
        E42 = 1ul << 42,
        E43 = 1ul << 43,
        E44 = 1ul << 44,
        E45 = 1ul << 45,
        E46 = 1ul << 46,
        E47 = 1ul << 47,
        E48 = 1ul << 48,
        E49 = 1ul << 49,
        E50 = 1ul << 50,
        E51 = 1ul << 51,
        E52 = 1ul << 52,
        E53 = 1ul << 53,
        E54 = 1ul << 54,
        E55 = 1ul << 55,
        E56 = 1ul << 56,
        E57 = 1ul << 57,
        E58 = 1ul << 58,
        E59 = 1ul << 59,
        E60 = 1ul << 60,
        E61 = 1ul << 61,
        E62 = 1ul << 62,
        E63 = 1ul << 63,
    }

    public static class EnumGroupHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckIndex(this EnumGroup8 group, byte idx) => idx < 8 && group.Check(1ul << idx);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckIndex(this EnumGroup16 group, byte idx) => idx < 16 && group.Check(1ul << idx);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckIndex(this EnumGroup32 group, byte idx) => idx < 32 && group.Check(1ul << idx);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckIndex(this EnumGroup64 group, byte idx) => idx < 64 && group.Check(1ul << idx);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this EnumGroup8 group) => group == EnumGroup8.Null;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this EnumGroup16 group) => group == EnumGroup16.Null;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this EnumGroup32 group) => group == EnumGroup32.Null;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this EnumGroup64 group) => group == EnumGroup64.Null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAll(this EnumGroup8 group) => group == EnumGroup8.All;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAll(this EnumGroup16 group) => group == EnumGroup16.All;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAll(this EnumGroup32 group) => group == EnumGroup32.All;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAll(this EnumGroup64 group) => group == EnumGroup64.All;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup8 group, in EnumGroup8 group8)
            => (group & group8) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup16 group, in EnumGroup16 group16)
            => (group & group16) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup32 group, in EnumGroup32 group32)
            => (group & group32) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup64 group, in EnumGroup64 group64)
            => (group & group64) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup8 group, in long group8)
            => ((byte)group & group8) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup16 group, in long group16)
            => ((ushort)group & group16) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup32 group, in long group32)
            => ((uint)group & group32) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup64 group, in long group64)
            => ((ulong)group & (ulong)group64) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup8 group, in ulong group8)
            => ((byte)group & group8) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup16 group, in ulong group16)
            => ((ushort)group & group16) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup32 group, in ulong group32)
            => ((uint)group & group32) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(this EnumGroup64 group, in ulong group64)
            => ((ulong)group & group64) != 0;
    }
}