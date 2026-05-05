#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin.Util
{
    /// <summary>
    /// 轻量的 JSON 输出器, 专供导表工具落盘"友好排版"的 .json 文件用.
    /// <para>
    /// 设计目标:
    /// </para>
    /// <list type="bullet">
    ///   <item>不依赖额外 nuget 包, 只用 BCL.</item>
    ///   <item>支持显式排版: 2 空格缩进, 数组/对象元素另起一行.</item>
    ///   <item>对小型数组 (例如全部为基础类型且元素少于阈值) 提供 inline 形式以减少行数.</item>
    ///   <item>数字一律按 invariant culture 输出, 防止本地化小数点 (',') 污染 JSON.</item>
    /// </list>
    /// </summary>
    internal static class PrettyJson
    {
        public const string DefaultIndent = "  ";
        public const int DefaultInlineMaxLen = 80;

        /// <summary>
        /// 把任意 object 序列化为格式化 JSON 字符串.
        /// 支持的类型: null, bool, sbyte/byte/short/ushort/int/uint/long/ulong, float/double, string,
        ///   IDictionary&lt;string, ?&gt;, IEnumerable, 以及匿名/具名键值序列 IEnumerable&lt;KeyValuePair&lt;string,?&gt;&gt;.
        /// 不支持的类型会调用 ToString().
        /// </summary>
        public static string Serialize(object? root, string indent = DefaultIndent, int inlineMaxLen = DefaultInlineMaxLen)
        {
            var sb = new StringBuilder();
            WriteValue(sb, root, indent, 0, inlineMaxLen);
            return sb.ToString();
        }

        /// <summary>
        /// 写文件 (UTF-8 无 BOM, LF 行尾) - 与项目默认编码约定一致, 避免 Git 工具显示 BOM.
        /// </summary>
        public static void WriteFile(string path, object? root,
            string indent = DefaultIndent, int inlineMaxLen = DefaultInlineMaxLen)
        {
            var json = Serialize(root, indent, inlineMaxLen);
            // 强制 LF 行尾, 与 .gitattributes / 跨平台一致
            if (json.IndexOf('\r') >= 0) json = json.Replace("\r\n", "\n").Replace("\r", "\n");
            TextFile.WriteAllText(path, json);
        }

        // ─────────────────────────────────────────────────────────────────

        private static void WriteValue(StringBuilder sb, object? v, string indent, int depth, int inlineMaxLen)
        {
            switch (v)
            {
                case null:
                    sb.Append("null");
                    return;
                case bool b:
                    sb.Append(b ? "true" : "false");
                    return;
                case string s:
                    WriteString(sb, s);
                    return;
                case sbyte or byte or short or ushort or int or uint or long:
                    sb.Append(Convert.ToInt64(v, CultureInfo.InvariantCulture)
                        .ToString(CultureInfo.InvariantCulture));
                    return;
                case ulong u:
                    sb.Append(u.ToString(CultureInfo.InvariantCulture));
                    return;
                case float f:
                    sb.Append(FormatFloat(f));
                    return;
                case double d:
                    sb.Append(FormatDouble(d));
                    return;
                case IDictionary<string, object?> dict:
                    WriteObject(sb, dict, indent, depth, inlineMaxLen);
                    return;
                case IEnumerable<KeyValuePair<string, object?>> kvs:
                    WriteObject(sb, kvs, indent, depth, inlineMaxLen);
                    return;
                case IEnumerable list:
                    WriteArray(sb, list, indent, depth, inlineMaxLen);
                    return;
                default:
                    WriteString(sb, v.ToString() ?? string.Empty);
                    return;
            }
        }

        private static string FormatFloat(float f)
        {
            // 用 "R" 保证可往返反序列化; 把小数点固定为 "."
            if (float.IsNaN(f) || float.IsInfinity(f)) return "null";
            return f.ToString("R", CultureInfo.InvariantCulture);
        }

        private static string FormatDouble(double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d)) return "null";
            return d.ToString("R", CultureInfo.InvariantCulture);
        }

        private static void WriteObject(StringBuilder sb, IEnumerable<KeyValuePair<string, object?>> kvs,
            string indent, int depth, int inlineMaxLen)
        {
            // 收集到列表以便判断是否空
            List<KeyValuePair<string, object?>> list;
            switch (kvs)
            {
                case List<KeyValuePair<string, object?>> l: list = l; break;
                default: list = kvs.ToList(); break;
            }

            if (list.Count == 0) { sb.Append("{}"); return; }

            sb.Append('{').Append('\n');
            var childIndent = Repeat(indent, depth + 1);
            var thisIndent  = Repeat(indent, depth);
            for (var i = 0; i < list.Count; i++)
            {
                var kv = list[i];
                sb.Append(childIndent);
                WriteString(sb, kv.Key);
                sb.Append(": ");
                WriteValue(sb, kv.Value, indent, depth + 1, inlineMaxLen);
                if (i < list.Count - 1) sb.Append(',');
                sb.Append('\n');
            }
            sb.Append(thisIndent).Append('}');
        }

        private static void WriteArray(StringBuilder sb, IEnumerable enumerable,
            string indent, int depth, int inlineMaxLen)
        {
            // 把 IEnumerable 物化, 避免后面多次枚举
            var items = new List<object?>();
            foreach (var x in enumerable) items.Add(x);

            if (items.Count == 0) { sb.Append("[]"); return; }

            // 尝试 inline (短数组 + 全部为简单值): 用一个临时 sb 试着拼一行, 若超长则换多行.
            if (CanTryInline(items))
            {
                var probe = new StringBuilder();
                probe.Append('[');
                for (var i = 0; i < items.Count; i++)
                {
                    if (i != 0) probe.Append(", ");
                    WriteValue(probe, items[i], indent, depth + 1, inlineMaxLen);
                }
                probe.Append(']');
                if (probe.Length <= inlineMaxLen)
                {
                    sb.Append(probe);
                    return;
                }
            }

            sb.Append('[').Append('\n');
            var childIndent = Repeat(indent, depth + 1);
            var thisIndent  = Repeat(indent, depth);
            for (var i = 0; i < items.Count; i++)
            {
                sb.Append(childIndent);
                WriteValue(sb, items[i], indent, depth + 1, inlineMaxLen);
                if (i < items.Count - 1) sb.Append(',');
                sb.Append('\n');
            }
            sb.Append(thisIndent).Append(']');
        }

        private static bool CanTryInline(IReadOnlyList<object?> items)
        {
            if (items.Count > 8) return false;
            for (var i = 0; i < items.Count; i++)
            {
                var v = items[i];
                switch (v)
                {
                    case null:
                    case bool:
                    case sbyte: case byte: case short: case ushort:
                    case int: case uint: case long: case ulong:
                    case float: case double:
                    case string:
                        continue;
                    default:
                        return false;
                }
            }
            return true;
        }

        private static string Repeat(string unit, int count)
        {
            if (count <= 0) return string.Empty;
            if (count == 1) return unit;
            var sb = new StringBuilder(unit.Length * count);
            for (var i = 0; i < count; i++) sb.Append(unit);
            return sb.ToString();
        }

        private static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"':  sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n");  break;
                    case '\r': sb.Append("\\r");  break;
                    case '\t': sb.Append("\\t");  break;
                    case '\b': sb.Append("\\b");  break;
                    case '\f': sb.Append("\\f");  break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c); // 直接写出 unicode 字符 (UTF-8 编码由 StreamWriter 处理)
                        break;
                }
            }
            sb.Append('"');
        }
    }
}
