using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// ReSharper disable MemberCanBePrivate.Global
namespace XD.Common.Word
{
    public interface IWord
    {
        public int Length { get; }
        public int Capacity { get; }
        public bool TryWriteBytes(Span<byte> span);
        public bool TryWriteBytes(Span<char> span);

        /// <summary>
        /// 索引器, 返回值域为 0~63
        /// </summary>
        /// <param name="index"> 索引 </param>
        public byte this[int index] { get; }
    }

    public interface IWord<T> : IWord, IEquatable<T>, IComparable<T> where T : unmanaged, IWord<T>
    {
        public T Slice(int start, int length);
        public T Create(in ReadOnlySpan<char> text);
        public T Create(in ReadOnlySpan<byte> text);
    }

    /// <summary>
    /// [16 byte] 仅支持 0-9 a-z A-Z '_' ' ' 且不超过 20 个字符的短字符串
    /// <para> 编码: 0: ' ', 1-9:'1'-'9', 10: '0', 11-36:'a'-'z', 37-62:'A'-'Z', 63:'_' </para>
    /// <para> 使用 6-bit 表达一个字符, 3byte 表达四个字符: 000000 | 001111 | 111122 | 222222 </para>
    /// <para> 20 个字符可以由 15 个 byte 表达, 剩余一个 byte 为 head, 表达长度 </para>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Word : IWord<Word>
    {
        public int Capacity => MaxLength;
        public int Length => BitConverter.IsLittleEndian ? _headLittleEndian : _headBigEndian;

        #region head
        // 受到大小端的影响, head 的 byte 位置需要调整
        private const byte MaxLength = 20;
        [FieldOffset(0)] private readonly byte _headLittleEndian;
        [FieldOffset(15)] private readonly byte _headBigEndian;
        #endregion

        #region 8-byte view
        // 用于比对字符串值
        [FieldOffset(0)] private readonly ulong _0;
        [FieldOffset(8)] private readonly ulong _1;
        #endregion

        public unsafe Word(in ReadOnlySpan<char> name)
        {
            { _0 = 0; _1 = 0; _headBigEndian = 0; _headLittleEndian = 0; }
            var len = name.Length > MaxLength ? MaxLength : name.Length;

            var groupInt = 0u;
            var i = 0;
            for (; i < len; i++)
            {
                var c = (uint)Trans(name[i]);

                // 四个字符一组, 最多 5 组
                var subIdx = i % 4;
                var charOffset = 26 - subIdx * 6; // 偏移
                if (subIdx <= 0)
                {
                    if (i != 0) fixed(ulong* ptr = &_0) WriteGroup(groupInt, i, ptr);
                    groupInt = 0;
                }
                groupInt |= c << charOffset;
            }
            fixed(ulong* ptr = &_0) WriteGroup(groupInt, i, ptr);
            if (BitConverter.IsLittleEndian) _headLittleEndian = (byte)len;
            else _headBigEndian = (byte)len;
            return;

            // 写入一组数据
            static void WriteGroup(uint groupInt, int i, void* self)
            {
                var group = (i - 1) / 4;
                var uintPtr = (uint*)((byte*)self + group * 3);
                *uintPtr |= groupInt;
            }

            // 将 char 翻译到内部编码
            static byte Trans(char c) => c switch
            {
                '_' => 63,
                '0' => 10,
                >= '1' and <= '9' => (byte)(c - '1' + 1),
                >= 'a' and <= 'z' => (byte)(c - 'a' + 11),
                >= 'A' and <= 'Z' => (byte)(c - 'A' + 37),
                _ => 0
            };
        }

        public unsafe Word(in ReadOnlySpan<byte> name)
        {
            { _0 = 0; _1 = 0; _headBigEndian = 0; _headLittleEndian = 0; }
            var len = name.Length > MaxLength ? MaxLength : name.Length;

            var groupInt = 0u;
            var i = 0;
            for (; i < len; i++)
            {
                var c = (uint)name[i];
                if (c > 63) c = 0u;

                // 四个字符一组, 最多 5 组
                var subIdx = i % 4;
                var charOffset = 26 - subIdx * 6; // 偏移
                if (subIdx <= 0)
                {
                    if (i != 0) fixed(ulong* ptr = &_0) WriteGroup(groupInt, i, ptr);
                    groupInt = 0;
                }
                groupInt |= c << charOffset;
            }
            fixed(ulong* ptr = &_0) WriteGroup(groupInt, i, ptr);
            if (BitConverter.IsLittleEndian) _headLittleEndian = (byte)len;
            else _headBigEndian = (byte)len;
            return;

            // 写入一组数据
            static void WriteGroup(uint groupInt, int i, void* self)
            {
                var group = (i - 1) / 4;
                var uintPtr = (uint*)((byte*)self + group * 3);
                *uintPtr |= groupInt;
            }
        }

        public Word Create(in ReadOnlySpan<char> text) => new(text);
        public Word Create(in ReadOnlySpan<byte> text) => new(text);

        public unsafe byte this[int index]
        {
            get
            {
                var len = Length;
                if (index < 0 || index >= len) throw new IndexOutOfRangeException(nameof(index));

                uint charBit;
                var group = index / 4;
                var subIdx = index % 4;
                var charOffset = 26 - subIdx * 6; // 偏移
                fixed (ulong* ptr = &_0) charBit = (byte)(*(uint*)((byte*)ptr + group * 3) >> charOffset) & 0b111111u;
                return (byte)charBit;
            }
        }

        public Word Slice(int start, int length)
        {
            var len = Length;
            if (start < 0 || start >= len) return default;
            if (start + length > len) length = len - start;

            Span<char> span = stackalloc char[len];
            TryWriteBytes(span);
            return new Word(span.Slice(start, length));
        }

        public unsafe bool TryWriteBytes(Span<char> span)
        {
            var len = Length;
            if (len > span.Length) len = span.Length;
            for (var i = 0; i < len; i++)
            {
                uint charBit;
                var group = i / 4;
                var subIdx = i % 4;
                var charOffset = 26 - subIdx * 6; // 偏移
                fixed (ulong* ptr = &_0) charBit = (byte)(*(uint*)((byte*)ptr + group * 3) >> charOffset) & 0b111111u;
                span[i] = Trans(charBit);
            }
            return true;

            static char Trans(uint c) => c switch
            {
                0 => ' ',
                63 => '_',
                10 => '0',
                <= 9 => (char)('1' + c - 1),
                <= 36 => (char)('a' + c - 11),
                <= 62 => (char)('A' + c - 37),
                _ => ' '
            };
        }

        public unsafe bool TryWriteBytes(Span<byte> span)
        {
            var len = Length;
            if (len > span.Length) len = span.Length;
            for (var i = 0; i < len; i++)
            {
                uint charBit;
                var group = i / 4;
                var subIdx = i % 4;
                var charOffset = 26 - subIdx * 6; // 偏移
                fixed (ulong* ptr = &_0) charBit = (byte)(*(uint*)((byte*)ptr + group * 3) >> charOffset) & 0b111111u;
                span[i] = Trans(charBit);
            }
            return true;

            static byte Trans(uint c) => c switch
            {
                0 => (byte)' ',
                63 => (byte)'_',
                10 => (byte)'0',
                <= 9 => (byte)('1' + c - 1),
                <= 36 => (byte)('a' + c - 11),
                <= 62 => (byte)('A' + c - 37),
                _ => (byte)' '
            };
        }

        public override string ToString()
        {
            var len = Length;
            Span<char> span = stackalloc char[len];
            return TryWriteBytes(span).ToString();
        }

        public bool Equals(Word other) => _0 == other._0 && _1 == other._1;
        public int CompareTo(Word other)
        {
            var ret = _0.CompareTo(other._0);
            return ret != 0 ? ret : _1.CompareTo(other._1);
        }

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Word other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_0, _1);

        public static bool operator ==(Word left, Word right) => left.Equals(right);
        public static bool operator !=(Word left, Word right) => !(left == right);

        public static implicit operator string(Word word) => word.ToString();
        public static implicit operator Word(string text) => new(text);

        public static (ECheckResult result, int value) Check(string? text)
        {
            if (string.IsNullOrEmpty(text)) return (ECheckResult.Success, 0);
            if (text.Length > MaxLength) return (ECheckResult.InvalidLength, MaxLength);
            for (var i = 0; i < text.Length; i++)
            {
                var t = text[i];
                var c = t switch
                {
                    '_' => 0,
                    '0' => 0,
                    >= '1' and <= '9' => 0,
                    >= 'a' and <= 'z' => 0,
                    >= 'A' and <= 'Z' => 0,
                    _ => -1
                };
                if (c < 0) return (ECheckResult.InvalidCharacter, i);
            }
            return (ECheckResult.Success, 0);
        }
    }

    /// <summary>
    /// [32 byte] 仅支持 0-9 a-z A-Z '_' ' ' 且不超过 40 个字符的短字符串
    /// <para> 编码: 0: ' ', 1-9:'1'-'9', 10: '0', 11-36:'a'-'z', 37-62:'A'-'Z', 63:'_' </para>
    /// <para> 使用 6-bit 表达一个字符, 3byte 表达四个字符: 000000 | 001111 | 111122 | 222222 </para>
    /// <para> 20 个字符可以由 15 个 byte 表达, 剩余一个 byte 为 head, 表达长度 </para>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct LongWord : IWord<LongWord>
    {
        public int Capacity => MaxLength;
        public int Length => BitConverter.IsLittleEndian ? _headLittleEndian : _headBigEndian;

        #region head
        // 受到大小端的影响, head 的 byte 位置需要调整
        private const byte MaxLength = 40;
        [FieldOffset(0)] private readonly byte _headLittleEndian;
        [FieldOffset(31)] private readonly byte _headBigEndian;
        #endregion

        #region 8-byte view
        // 用于比对字符串值
        [FieldOffset(0)]  private readonly ulong _0;
        [FieldOffset(8)]  private readonly ulong _1;
        [FieldOffset(16)] private readonly ulong _2;
        [FieldOffset(24)] private readonly ulong _3;
        #endregion

        public unsafe LongWord(in ReadOnlySpan<char> name)
        {
            { _0 = 0; _1 = 0; _2 = 0; _3 = 0; _headBigEndian = 0; _headLittleEndian = 0; }
            var len = name.Length > MaxLength ? MaxLength : name.Length;

            var groupInt = 0u;
            var i = 0;
            for (; i < len; i++)
            {
                var c = (uint)Trans(name[i]);

                // 四个字符一组, 最多 5 组
                var subIdx = i % 4;
                var charOffset = 26 - subIdx * 6; // 偏移
                if (subIdx <= 0)
                {
                    if (i != 0) fixed(ulong* ptr = &_0) WriteGroup(groupInt, i, ptr);
                    groupInt = 0;
                }
                groupInt |= c << charOffset;
            }
            fixed(ulong* ptr = &_0) WriteGroup(groupInt, i, ptr);
            if (BitConverter.IsLittleEndian) _headLittleEndian = (byte)len;
            else _headBigEndian = (byte)len;
            return;

            // 写入一组数据
            static void WriteGroup(uint groupInt, int i, void* self)
            {
                var group = (i - 1) / 4;
                var uintPtr = (uint*)((byte*)self + group * 3);
                *uintPtr |= groupInt;
            }

            // 将 char 翻译到内部编码
            static byte Trans(char c) => c switch
            {
                '_' => 63,
                '0' => 10,
                >= '1' and <= '9' => (byte)(c - '1' + 1),
                >= 'a' and <= 'z' => (byte)(c - 'a' + 11),
                >= 'A' and <= 'Z' => (byte)(c - 'A' + 37),
                _ => 0
            };
        }

        public unsafe LongWord(in ReadOnlySpan<byte> name)
        {
            { _0 = 0; _1 = 0; _2 = 0; _3 = 0; _headBigEndian = 0; _headLittleEndian = 0; }
            var len = name.Length > MaxLength ? MaxLength : name.Length;

            var groupInt = 0u;
            var i = 0;
            for (; i < len; i++)
            {
                var c = (uint)name[i];
                if (c > 63) c = 0u;

                // 四个字符一组, 最多 5 组
                var subIdx = i % 4;
                var charOffset = 26 - subIdx * 6; // 偏移
                if (subIdx <= 0)
                {
                    if (i != 0) fixed(ulong* ptr = &_0) WriteGroup(groupInt, i, ptr);
                    groupInt = 0;
                }
                groupInt |= c << charOffset;
            }
            fixed(ulong* ptr = &_0) WriteGroup(groupInt, i, ptr);
            if (BitConverter.IsLittleEndian) _headLittleEndian = (byte)len;
            else _headBigEndian = (byte)len;
            return;

            // 写入一组数据
            static void WriteGroup(uint groupInt, int i, void* self)
            {
                var group = (i - 1) / 4;
                var uintPtr = (uint*)((byte*)self + group * 3);
                *uintPtr |= groupInt;
            }
        }

        public LongWord Create(in ReadOnlySpan<char> text) => new(text);
        public LongWord Create(in ReadOnlySpan<byte> text) => new(text);

        public unsafe byte this[int index]
        {
            get
            {
                var len = Length;
                if (index < 0 || index >= len) throw new IndexOutOfRangeException(nameof(index));

                uint charBit;
                var group = index / 4;
                var subIdx = index % 4;
                var charOffset = 26 - subIdx * 6; // 偏移
                fixed (ulong* ptr = &_0) charBit = (byte)(*(uint*)((byte*)ptr + group * 3) >> charOffset) & 0b111111u;
                return (byte)charBit;
            }
        }

        public LongWord Slice(int start, int length)
        {
            var len = Length;
            if (start < 0 || start >= len) return default;
            if (start + length > len) length = len - start;

            Span<char> span = stackalloc char[len];
            TryWriteBytes(span);
            return new LongWord(span.Slice(start, length));
        }

        public unsafe bool TryWriteBytes(Span<char> span)
        {
            var len = Length;
            if (len > span.Length) len = span.Length;
            for (var i = 0; i < len; i++)
            {
                uint charBit;
                var group = i / 4;
                var subIdx = i % 4;
                var charOffset = 26 - subIdx * 6; // 偏移
                fixed (ulong* ptr = &_0) charBit = (byte)(*(uint*)((byte*)ptr + group * 3) >> charOffset) & 0b111111u;
                span[i] = Trans(charBit);
            }
            return true;

            static char Trans(uint c) => c switch
            {
                0 => ' ',
                63 => '_',
                10 => '0',
                <= 9 => (char)('1' + c - 1),
                <= 36 => (char)('a' + c - 11),
                <= 62 => (char)('A' + c - 37),
                _ => ' '
            };
        }

        public unsafe bool TryWriteBytes(Span<byte> span)
        {
            var len = Length;
            if (len > span.Length) len = span.Length;
            for (var i = 0; i < len; i++)
            {
                uint charBit;
                var group = i / 4;
                var subIdx = i % 4;
                var charOffset = 26 - subIdx * 6; // 偏移
                fixed (ulong* ptr = &_0) charBit = (byte)(*(uint*)((byte*)ptr + group * 3) >> charOffset) & 0b111111u;
                span[i] = Trans(charBit);
            }
            return true;

            static byte Trans(uint c) => c switch
            {
                0 => (byte)' ',
                63 => (byte)'_',
                10 => (byte)'0',
                <= 9 => (byte)('1' + c - 1),
                <= 36 => (byte)('a' + c - 11),
                <= 62 => (byte)('A' + c - 37),
                _ => (byte)' '
            };
        }

        public override string ToString()
        {
            var len = Length;
            Span<char> span = stackalloc char[len];
            return TryWriteBytes(span).ToString();
        }

        public bool Equals(LongWord other) => _0 == other._0 && _1 == other._1 && _2 == other._2 && _3 == other._3;
        public int CompareTo(LongWord other)
        {
            var ret = _0.CompareTo(other._0);
            if (0 != ret) return ret;
            ret = _1.CompareTo(other._1);
            if (0 != ret) return ret;
            ret = _2.CompareTo(other._2);
            return ret != 0 ? ret : _3.CompareTo(other._3);
        }

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is LongWord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_0, _1, _2, _3);

        public static bool operator ==(LongWord left, LongWord right) => left.Equals(right);
        public static bool operator !=(LongWord left, LongWord right) => !(left == right);

        public static implicit operator LongWord(in Word word)
        {
            Span<char> span = stackalloc char[word.Length];
            word.TryWriteBytes(span);
            return new LongWord(span);
        }
        public static implicit operator string(LongWord word) => word.ToString();
        public static implicit operator LongWord(string text) => new(text);

        public static (ECheckResult result, int value) Check(string? text)
        {
            if (string.IsNullOrEmpty(text)) return (ECheckResult.Success, 0);
            if (text.Length > MaxLength) return (ECheckResult.InvalidLength, MaxLength);
            for (var i = 0; i < text.Length; i++)
            {
                var t = text[i];
                var c = t switch
                {
                    '_' => 0,
                    '0' => 0,
                    >= '1' and <= '9' => 0,
                    >= 'a' and <= 'z' => 0,
                    >= 'A' and <= 'Z' => 0,
                    _ => -1
                };
                if (c < 0) return (ECheckResult.InvalidCharacter, i);
            }
            return (ECheckResult.Success, 0);
        }
    }

    public enum ECheckResult : byte
    {
        Success,
        InvalidCharacter,
        InvalidLength
    }
}