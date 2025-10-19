using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

// ReSharper disable MemberCanBePrivate.Global
namespace XD.Common.Guid
{
    /// <summary>
    /// 16 byte 结构体, 储存 guid
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    // ReSharper disable once InconsistentNaming
    public readonly struct guid : IEquatable<guid>, IComparable<guid>
    {
        // 显式内存布局
        [FieldOffset(0)]  private readonly byte _8u0;
        [FieldOffset(1)]  private readonly byte _8u1;
        [FieldOffset(2)]  private readonly byte _8u2;
        [FieldOffset(3)]  private readonly byte _8u3;
        [FieldOffset(4)]  private readonly byte _8u4;
        [FieldOffset(5)]  private readonly byte _8u5;
        [FieldOffset(6)]  private readonly byte _8u6;
        [FieldOffset(7)]  private readonly byte _8u7;
        [FieldOffset(8)]  private readonly byte _8u8;
        [FieldOffset(9)]  private readonly byte _8u9;
        [FieldOffset(10)] private readonly byte _8uA;
        [FieldOffset(11)] private readonly byte _8uB;
        [FieldOffset(12)] private readonly byte _8uC;
        [FieldOffset(13)] private readonly byte _8uD;
        [FieldOffset(14)] private readonly byte _8uE;
        [FieldOffset(15)] private readonly byte _8uF;

        // 64 位视图用于快速比较
        [FieldOffset(0)] [IgnoreDataMember] private readonly ulong _64u1;
        [FieldOffset(8)] [IgnoreDataMember] private readonly ulong _64u2;

        // 32 位视图
        [FieldOffset(0)]  [IgnoreDataMember] private readonly uint _32u1;
        [FieldOffset(4)]  [IgnoreDataMember] private readonly uint _32u2;
        [FieldOffset(8)]  [IgnoreDataMember] private readonly uint _32u3;
        [FieldOffset(12)] [IgnoreDataMember] private readonly uint _32u4;

        public static guid Empty() => new();

        public static guid New() => System.Guid.NewGuid();

        public static unsafe guid New(in ReadOnlySpan<byte> sp)
        {
            if (sp.Length <= 0) return Empty();
            var ret = new guid();
            var pDest = &ret._8u0;
            fixed (byte* pSrc = &sp[0])
                Unsafe.CopyBlock(pDest, pSrc, sp.Length < 16 ? (uint)sp.Length : 16);
            return ret;
        }

        public ReadOnlySpan<byte> AsSpan()
        {
            ref var self = ref Unsafe.AsRef(in this);
            return MemoryMarshal.CreateSpan(ref Unsafe.As<guid, byte>(ref self), 16);
        }
        public bool Equals(guid other) => _64u1 == other._64u1 && _64u2 == other._64u2;
        public int CompareTo(guid other)
        {
            var cmp = _64u1.CompareTo(other._64u1);
            return cmp != 0 ? cmp : _64u2.CompareTo(other._64u2);
        }
        public override bool Equals(object? obj) => obj is guid other && Equals(other);

        public override int GetHashCode() =>
            (int)(((uint)(_64u1 ^ (_64u1 >> 32)) * 397) ^ (uint)(_64u2 ^ (_64u2 >> 32)));
        public override string ToString() =>
            $"{_8u0:X2}{_8u1:X2}{_8u2:X2}{_8u3:X2}-{_8u4:X2}{_8u5:X2}-{_8u6:X2}{_8u7:X2}-{_8u8:X2}{_8u9:X2}-{_8uA:X2}{_8uB:X2}{_8uC:X2}{_8uD:X2}{_8uE:X2}{_8uF:X2}";

        public static bool operator ==(in guid left, in guid right) => left.Equals(right);
        public static bool operator !=(in guid left, in guid right) => !left.Equals(right);
        public static bool operator ==(in System.Guid left, in guid right) => left.Equals(right);
        public static bool operator !=(in System.Guid left, in guid right) => !left.Equals(right);
        public static bool operator ==(in guid left, in System.Guid right) => ((System.Guid)left).Equals(right);
        public static bool operator !=(in guid left, in System.Guid right) => !((System.Guid)left).Equals(right);
        public static implicit operator System.Guid(guid g) => new(g.AsSpan());
        public static implicit operator guid(System.Guid g)
        {
            Span<byte> sp = stackalloc byte[16];
            return g.TryWriteBytes(sp) ? New(sp) : Empty();
        }

        public static implicit operator bool(guid g) => g is not { _64u1: 0, _64u2: 0 };
    }
}