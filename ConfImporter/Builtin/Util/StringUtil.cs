using System;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin.Util
{
    internal static class StringUtil
    {
        public static bool Contains(in ReadOnlySpan<char> str, char c)
        {
            foreach (var t in str) if (t == c) return true;
            return false;
        }
        
        public static ReadOnlySpan<char> MatchProgramValidName(ref ReadOnlySpan<char> str)
        {
            var slice = ReadOnlySpan<char>.Empty;
            if (str.IsEmpty) return slice;

            for (var i = 0; i < str.Length; i++)
            {
                if (i == 0 && IsDigit(str[i])) return slice;
                if (IsLetterOrUnderLine(str[i]) || IsDigit(str[i])) continue;

                slice = str[..i];
                str = str[i..];
                return slice;
            }

            slice = str;
            str = ReadOnlySpan<char>.Empty;
            return slice;
        }

        public static bool IsProgramValidName(in ReadOnlySpan<char> str)
        {
            for (var i = 0; i < str.Length; i++)
            {
                if (i == 0 && IsDigit(str[i])) return false;
                if (IsLetterOrUnderLine(str[i]) || IsDigit(str[i])) continue;
                return false;
            }
            return true;
        }

        public static bool IsDigit(char c) => c is >= '0' and <= '9';

        public static bool IsLetterOrUnderLine(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';

        public static int FindFirst(in ReadOnlySpan<char> str, char c)
        {
            for (var i = 0; i < str.Length; i++)
                if (c == str[i]) return i;
            return -1;
        }

        public static void TrimHead(ref ReadOnlySpan<char> str)
        {
            var i = 0;
            for (; i < str.Length; i++) if (!char.IsWhiteSpace(str[i])) break;
            str = str[i..];
        }

        public static void TrimEnd(ref ReadOnlySpan<char> str)
        {
            var i = str.Length - 1;
            for (; i >= 0; i--) if (!char.IsWhiteSpace(str[i])) break;
            str = str[..(i + 1)];
        }

        public static void Trim(ref ReadOnlySpan<char> str)
        {
            TrimEnd(ref str);
            TrimHead(ref str);
        }

        public static bool IsEmptyOrWhiteSpace(in ReadOnlySpan<char> str)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < str.Length; i++)
                if (!char.IsWhiteSpace(str[i]))
                    return false;
            return true;
        }

        /// <summary>
        /// 分割 span, 如: Divide("abcd", 2) => return: "ab", src: "cd"
        /// </summary>
        /// <param name="str"> 要分割的字符串 </param>
        /// <param name="length"> 分割长度 </param>
        /// <returns></returns>
        public static ReadOnlySpan<char> Divide(ref ReadOnlySpan<char> str, int length)
        {
            if (length < 0 || length > str.Length) return ReadOnlySpan<char>.Empty;
            if (length == str.Length)
            {
                var ss = str;
                str = ReadOnlySpan<char>.Empty;
                return ss;
            }
            var slice = str[..length];
            str = str[length..];
            return slice;
        }

        public static bool IsContainsInFilter(string tag, string filter)
        {
            for (var i = 0; i < filter.Length; i++)
            {
                if (char.IsWhiteSpace(filter[i])) continue;

                var j = i;
                for (; j < filter.Length; j++) if (filter[j] == '/') break;
                var l = j - i;
                var isEnter = false;
                for (var k = j - 1; k >= i; k--)
                {
                    if (!isEnter && char.IsWhiteSpace(filter[k]))
                    {
                        l--;
                        isEnter = true;
                        continue;
                    }

                    if (l != tag.Length) break;
                    if (tag[--l] != filter[k]) break;
                }

                if (l == 0) return true;
                i = j;
            }

            return false;
        }
    }
}