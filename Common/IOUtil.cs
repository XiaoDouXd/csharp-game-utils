using System;
using System.Collections.Generic;
using System.Text;
using XD.Common.CollectionUtil;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace XD.Common
{
    // ReSharper disable once InconsistentNaming
    public static class IOUtil
    {
        /// <summary>
        /// 目录分隔符
        /// </summary>
        public const char DirSeparatorChar = '/';

        /// <summary>
        /// 翻转目录分隔符
        /// </summary>
        public const char AltDirSeparatorChar = '\\';

        /// <summary>
        /// 盘符
        /// </summary>
        public const char VolumeSeparatorChar = ':';

        /// <summary>
        /// 目录留置字符
        /// </summary>
        public const char DirLienChar = '.';

        /// <summary>
        /// 路径去除相同首部
        /// </summary>
        /// <param name="fullPath"> 被减数 </param>
        /// <param name="head"> 减数 </param>
        /// <returns></returns>
        public static ReadOnlyListSlice<string, TList> SplitPathTrimHead<TList>(in TList fullPath, in TList head)
            where TList : IReadOnlyList<string>
        {
            var slice = new ReadOnlyListSlice<string, TList>(fullPath);
            if (fullPath is not { Count: > 0 }) return slice;

            int i = 0, j = 0;
            while (true)
            {
                while (i < fullPath.Count && string.IsNullOrWhiteSpace(fullPath[i])) i++;
                while (j < head.Count && string.IsNullOrWhiteSpace(head[j])) j++;
                if (i >= fullPath.Count) break;
                if (j >= head.Count) break;
                if (fullPath[i++] != head[j++]) break;
            }

            slice.Slice = (i, fullPath.Count - i);
            return slice;
        }

        /// <summary>
        /// 串路径转换为合法文件名
        /// </summary>
        /// <param name="path"></param>
        /// <param name="suffix"></param>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static string SplitPathToValidFileName<TList>(in TList path, string suffix = "", StringBuilder? sb = null)
            where TList : IReadOnlyList<string>
        {
            const char escapingChar = '!';
            const char splitChar = ';';

            sb ??= CommonSb;
            sb.Clear();

            if (path is not { Count: > 0 }) return "." + suffix;

            // 避免拆装箱
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < path.Count; i++)
            {
                var part = path[i];
                if (string.IsNullOrWhiteSpace(part)) continue;
                if (sb.Length > 0) sb.Append(splitChar);
                sb.Append(part.AntiEscape(escapingChar));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 串路径转换为路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="suffix"></param>
        /// <param name="delFileExtension"></param>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static string SplitPathToPath<TList>(in TList path, string? suffix = "", bool delFileExtension = false, StringBuilder? sb = null)
            where TList : IReadOnlyList<string>
        {
            const char splitChar = '/';

            sb ??= CommonSb;
            sb.Clear();

            if (path is not { Count: > 0 }) return suffix ?? "";

            // 避免拆装箱
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < path.Count; i++)
            {
                var part = path[i];
                if (string.IsNullOrWhiteSpace(part)) continue;
                if (sb.Length > 0) sb.Append(splitChar);
                if (delFileExtension && i == path.Count - 1)
                {
                    var dot = part.LastIndexOf('.');
                    if (dot >= 0) part = part[..dot];
                }
                sb.Append(part);
            }
            return suffix is {Length: > 0} ? sb.ToString() + splitChar + suffix : sb.ToString();
        }

        /// <summary>
        /// 串路径是否等价
        /// </summary>
        /// <param name="pathA"></param>
        /// <param name="pathB"></param>
        /// <returns></returns>
        public static bool IsSameSplitPath<T>(in T pathA, in T pathB)
            where T : IReadOnlyList<string>
        {
            if (pathA is not { Count: > 0 }) return pathB is not { Count: > 0 };
            if (pathB is not { Count: > 0 }) return false;

            int i = 0, j = 0;
            while (true)
            {
                while (i < pathA.Count && string.IsNullOrWhiteSpace(pathA[i])) i++;
                while (j < pathB.Count && string.IsNullOrWhiteSpace(pathB[j])) j++;
                if (i >= pathA.Count) return j >= pathB.Count;
                if (j >= pathB.Count) return false;
                if (pathA[i++] != pathB[j++]) return false;
            }
        }

        /// <summary>
        /// 串路径是否包含
        /// </summary>
        /// <param name="subPath"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsSplitPathContains<T>(in T path, in T subPath)
            where T : IReadOnlyList<string>
        {
            if (subPath is not { Count: > 0 }) return path is not { Count: > 0 };
            if (path is not { Count: > 0 }) return true;

            int i = 0, j = 0;
            while (true)
            {
                while (i < subPath.Count && string.IsNullOrWhiteSpace(subPath[i])) i++;
                while (j < path.Count && string.IsNullOrWhiteSpace(path[j])) j++;
                if (i >= subPath.Count) return j >= path.Count;
                if (j >= path.Count) return true;
                if (subPath[i++] != path[j++]) return false;
            }
        }

        /// <summary>
        /// 串路径是否包含
        /// </summary>
        /// <param name="subPath"></param>
        /// <param name="path"></param>
        /// <param name="left"></param>
        /// <returns></returns>
        public static bool IsSplitPathContains<TList>(in TList path, TList subPath, out ReadOnlyListSlice<string, TList> left)
            where TList : IReadOnlyList<string>
        {
            if (subPath is not { Count: > 0 })
            {
                left = new ReadOnlyListSlice<string, TList>(path);
                return path is not { Count: > 0 };
            }

            if (path is not { Count: > 0 })
            {
                left = new ReadOnlyListSlice<string, TList>(path);
                return true;
            }

            int i = 0, j = 0;
            while (true)
            {
                while (i < subPath.Count && string.IsNullOrWhiteSpace(subPath[i])) i++;
                while (j < path.Count && string.IsNullOrWhiteSpace(path[j])) j++;
                if (i >= subPath.Count)
                {
                    if (j >= path.Count)
                    {
                        left = subPath.AsSlice<TList, string>(i);
                        return true;
                    }
                    left = path.AsSlice<TList, string>(0, 0);
                    return false;
                }

                if (j >= path.Count)
                {
                    left = subPath.AsSlice<TList, string>(i);
                    return true;
                }

                if (subPath[i++] == path[j++]) continue;
                left = new ReadOnlyListSlice<string, TList>(subPath);
                return false;
            }
        }

        /// <summary>
        /// 路径转串路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static List<string> SplitPath(in ReadOnlySpan<char> path, List<string>? list)
        {
            list ??= new List<string>();
            list.Clear();

            // 把字符串分段
            Span<int> part = stackalloc int[path.Length];
            var idx = 0;
            var isFrom = true;
            for (var i = 0; i < path.Length; i++)
            {
                if (path[i] is DirSeparatorChar or VolumeSeparatorChar or AltDirSeparatorChar)
                {
                    if (!isFrom)
                    {
                        part[idx++] = i - 1;
                        if (idx > 1)
                        {
                            var to = part[idx - 1];
                            var from = part[idx - 2];
                            var partStr = path.Slice(from, to - from + 1);
                            StringUtil.Trim(ref partStr);
                            switch (partStr.Length)
                            {
                                case 1 when partStr[0] == DirLienChar:
                                    idx -= 2;
                                    break;
                                case 2 when partStr[0] == DirLienChar && partStr[1] == DirLienChar:
                                    idx -= 4;
                                    break;
                            }
                        }
                    }
                    isFrom = true;
                    continue;
                }
                if (!isFrom) continue;

                part[idx++] = i;
                isFrom = false;
            }

            // ReSharper disable ConvertIfStatementToSwitchStatement
            if (idx == 0) return list;
            if (idx % 2 != 0) part[idx++] = path.Length - 1;
            if (idx > 1)
            {
                var to = part[idx - 1];
                var from = part[idx - 2];
                var partStr = path.Slice(from, to - from + 1);
                StringUtil.Trim(ref partStr);
                switch (partStr.Length)
                {
                    case 1 when partStr[0] == DirLienChar:
                        idx -= 2;
                        break;
                    case 2 when partStr[0] == DirLienChar && partStr[1] == DirLienChar:
                        idx -= 4;
                        break;
                }
            }
            if (idx < 0) throw new IndexOutOfRangeException();
            if (idx == 0) return list;
            // ReSharper restore ConvertIfStatementToSwitchStatement

            for (var i = 0; i < idx; i += 2)
            {
                var from = part[i];
                var to = part[i + 1];
                var partStr = path.Slice(from, to - from + 1);
                if (partStr.IsEmptyOrWhiteSpace()) continue;
                StringUtil.Trim(ref partStr);
                list.Add(partStr.ToString());
            }
            return list;
        }

        /// <summary>
        /// 路径转串路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] SplitPath(in ReadOnlySpan<char> path)
        {
            if (path.Length == 0) return Array.Empty<string>();

            // 把字符串分段
            Span<int> part = stackalloc int[path.Length];
            var idx = 0;
            var isFrom = true;
            for (var i = 0; i < path.Length; i++)
            {
                if (path[i] is DirSeparatorChar or VolumeSeparatorChar or AltDirSeparatorChar)
                {
                    if (!isFrom)
                    {
                        part[idx++] = i - 1;
                        if (idx > 1)
                        {
                            var to = part[idx - 1];
                            var from = part[idx - 2];
                            var partStr = path.Slice(from, to - from + 1);
                            StringUtil.Trim(ref partStr);
                            switch (partStr.Length)
                            {
                                case 1 when partStr[0] == DirLienChar:
                                    idx -= 2;
                                    break;
                                case 2 when partStr[0] == DirLienChar && partStr[1] == DirLienChar:
                                    idx -= 4;
                                    break;
                            }
                        }
                    }
                    isFrom = true;
                    continue;
                }
                if (!isFrom) continue;

                part[idx++] = i;
                isFrom = false;
            }

            // ReSharper disable ConvertIfStatementToSwitchStatement
            if (idx == 0) return Array.Empty<string>();
            if (idx % 2 != 0) part[idx++] = path.Length - 1;
            if (idx > 1)
            {
                var to = part[idx - 1];
                var from = part[idx - 2];
                var partStr = path.Slice(from, to - from + 1);
                StringUtil.Trim(ref partStr);
                switch (partStr.Length)
                {
                    case 1 when partStr[0] == DirLienChar:
                        idx -= 2;
                        break;
                    case 2 when partStr[0] == DirLienChar && partStr[1] == DirLienChar:
                        idx -= 4;
                        break;
                }
            }
            if (idx < 0) throw new IndexOutOfRangeException();
            if (idx == 0) return Array.Empty<string>();
            // ReSharper restore ConvertIfStatementToSwitchStatement

            var cnt = idx / 2;
            for (var i = 0; i < idx; i += 2)
            {
                var from = part[i];
                if (path.Slice(from, part[i + 1] - from + 1).IsEmptyOrWhiteSpace()) cnt--;
            }

            var j = 0;
            var list = new string[cnt];
            for (var i = 0; i < idx && j < cnt; i += 2)
            {
                var from = part[i];
                var to = part[i + 1];
                var partStr = path.Slice(from, to - from + 1);
                if (partStr.IsEmptyOrWhiteSpace()) continue;
                StringUtil.Trim(ref partStr);
                list[j++] = partStr.ToString();
            }
            return list;
        }

        private static readonly StringBuilder CommonSb = new();
    }
}