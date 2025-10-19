using System;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global

namespace XD.Common
{
    public static class StringUtil
    {
        public const char Slash = '/';
        public const char Backslash = '\\';

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        public static void TrimHead(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty) return;
            var i = 0;
            for (; i < str.Length; i++) if (!char.IsWhiteSpace(str[i])) break;
            str = str[i..];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        public static void TrimEnd(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty) return;
            var i = str.Length - 1;
            for (; i >= 0; i--) if (!char.IsWhiteSpace(str[i])) break;
            str = str[..(i + 1)];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        public static void Trim(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty) return;
            TrimEnd(ref str);
            TrimHead(ref str);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int FindFirst(in ReadOnlySpan<char> str, char c)
        {
            for (var i = 0; i < str.Length; i++)
                if (c == str[i]) return i;
            return -1;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEmptyOrWhiteSpace(this in ReadOnlySpan<char> str)
            => str.IsEmpty || str.IsWhiteSpace();

        /// <summary>
        /// 转义
        /// </summary>
        /// <param name="self"></param>
        /// <param name="escapingChar"></param>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static string Escape(this string self, char escapingChar = Backslash, StringBuilder? sb = null)
        {
            if (string.IsNullOrEmpty(self)) return self;
            sb ??= CommonSb;
            sb.Clear();

            var isEscaping = false;
            foreach (var c in self)
            {
                switch (isEscaping)
                {
                    case false when c == escapingChar:
                        isEscaping = true;
                        continue;
                    case true:
                        sb.Append(c switch
                        {
                            '0' => '\0',
                            'a' => '\a',
                            'b' => '\b',
                            'f' => '\f',
                            'n' => '\n',
                            'r' => '\r',
                            't' => '\t',
                            'v' => '\v',
                            _ => c
                        });
                        isEscaping = false;
                        continue;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 逆转义
        /// </summary>
        /// <param name="self"></param>
        /// <param name="escapingChar"></param>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static string AntiEscape(this string self, char escapingChar = Backslash, StringBuilder? sb = null)
        {
            if (string.IsNullOrEmpty(self)) return self;
            sb ??= CommonSb;
            sb.Clear();
            foreach (var c in self)
            {
                if (c == escapingChar)
                {
                    sb.Append(escapingChar).Append(escapingChar);
                    continue;
                }
                sb.Append(c switch
                {
                    '\0' => $"{escapingChar}0",
                    '\a' => $"{escapingChar}a",
                    '\b' => $"{escapingChar}b",
                    '\f' => $"{escapingChar}f",
                    '\n' => $"{escapingChar}n",
                    '\r' => $"{escapingChar}r",
                    '\t' => $"{escapingChar}t",
                    '\v' => $"{escapingChar}v",
                    _ => c
                });
            }
            return sb.ToString();
        }

        private static readonly StringBuilder CommonSb = new();
    }
}
