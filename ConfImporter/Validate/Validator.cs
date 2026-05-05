#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using ConfImporter.Builtin;
using ConfImporter.Builtin.Type;
using ConfImporter.Builtin.Util.Gen;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
namespace ConfImporter.Validate
{
    /// <summary>
    /// 校验问题的严重等级.
    /// </summary>
    public enum ESeverity
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// 校验问题的来源类别.
    /// </summary>
    public static class ValidationCategory
    {
        /// <summary> Text (本地化 key) 类型相关的校验问题. </summary>
        public const string Text = "text";
        /// <summary> 字段被标注为 index, 校验所引用的目标表/主键. </summary>
        public const string Index = "index";
        /// <summary> 字段被标注为 asset, 校验为非空字符串 (具体存在性需外部裁定). </summary>
        public const string Asset = "asset";
        /// <summary> 注解本身格式 / 内容不被识别. </summary>
        public const string Attribute = "attribute";
    }

    /// <summary>
    /// 单条校验问题.
    /// </summary>
    public readonly struct ValidationIssue
    {
        public readonly ESeverity Severity;
        /// <summary> 来源 (例如 <c>World[1].path</c> / <c>G.Common.ProjectName</c>) </summary>
        public readonly string Source;
        /// <summary> 类别, 详见 <see cref="ValidationCategory"/> </summary>
        public readonly string Category;
        /// <summary> 描述信息 </summary>
        public readonly string Message;

        public ValidationIssue(ESeverity severity, string? source, string? category, string? message)
        {
            Severity = severity;
            Source = source ?? string.Empty;
            Category = category ?? string.Empty;
            Message = message ?? string.Empty;
        }
    }

    /// <summary>
    /// 一个待外部裁定的资产候选项 (因为引擎层面才知道资产虚拟文件系统的真实状态).
    /// </summary>
    public readonly struct AssetCandidate
    {
        /// <summary> 来源 (例如 <c>World[1].path</c>) </summary>
        public readonly string Source;
        /// <summary> 字段值原文 </summary>
        public readonly string Path;

        public AssetCandidate(string? source, string? path)
        {
            Source = source ?? string.Empty;
            Path = path ?? string.Empty;
        }
    }

    /// <summary>
    /// 校验报告结果.
    /// </summary>
    public sealed class ValidationReport
    {
        public IReadOnlyList<ValidationIssue> Issues => _issues;
        public IReadOnlyList<AssetCandidate> Assets => _assets;

        public int ErrorCount   { get; private set; }
        public int WarningCount { get; private set; }

        internal void Add(ValidationIssue issue)
        {
            _issues.Add(issue);
            if (issue.Severity == ESeverity.Error)   ErrorCount++;
            if (issue.Severity == ESeverity.Warning) WarningCount++;
        }

        internal void AddAsset(AssetCandidate c) => _assets.Add(c);

        /// <summary>
        /// 转为 JSON, 便于落盘后与 GUI / addon 等异构进程交互.
        /// </summary>
        public string ToJson()
        {
            var sb = new StringBuilder();
            sb.Append('{');
            sb.Append("\"errorCount\":").Append(ErrorCount).Append(',');
            sb.Append("\"warningCount\":").Append(WarningCount).Append(',');
            sb.Append("\"issues\":[");
            for (var i = 0; i < _issues.Count; i++)
            {
                if (i != 0) sb.Append(',');
                var x = _issues[i];
                sb.Append('{')
                    .Append("\"severity\":\"").Append(x.Severity).Append("\",")
                    .Append("\"source\":\"").Append(JsonEscape(x.Source)).Append("\",")
                    .Append("\"category\":\"").Append(JsonEscape(x.Category)).Append("\",")
                    .Append("\"message\":\"").Append(JsonEscape(x.Message)).Append('"')
                    .Append('}');
            }
            sb.Append("],");
            sb.Append("\"assets\":[");
            for (var i = 0; i < _assets.Count; i++)
            {
                if (i != 0) sb.Append(',');
                var a = _assets[i];
                sb.Append('{')
                    .Append("\"source\":\"").Append(JsonEscape(a.Source)).Append("\",")
                    .Append("\"path\":\"").Append(JsonEscape(a.Path)).Append('"')
                    .Append('}');
            }
            sb.Append("]}");
            return sb.ToString();
        }

        public void WriteJsonTo(string path)
        {
            Builtin.Util.TextFile.WriteAllText(path, ToJson());
        }

        private static string JsonEscape(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var sb = new StringBuilder(s.Length + 8);
            foreach (var c in s)
            {
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
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        private readonly List<ValidationIssue>  _issues = new();
        private readonly List<AssetCandidate>   _assets = new();
    }

    /// <summary>
    /// 配置表导出后的校验器.
    /// <para>
    /// 当前实现的校验项目:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>text</c> 类型字段值: 检查格式形如 <c>[TABLE,KEY]</c>, 并且 <c>(TABLE, KEY)</c> 已经在本地化表中注册.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       字段类型注解 <c>&lt;index=TableName&gt;</c>: 检查值是否在目标普通表的主键集合中.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       字段类型注解 <c>&lt;asset&gt;</c>: 校验值非空, 并将路径收集到报告的 <see cref="ValidationReport.Assets"/>
    ///       中, 由具备引擎运行时上下文的外部消费者 (例如 Godot addon) 做存在性裁决.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       未识别的注解 key: 输出 Warning, 不阻止导表.
    ///     </description>
    ///   </item>
    /// </list>
    /// </summary>
    public static class Validator
    {
        // 已知注解 key 集合, 注解里出现其他 key 会被告警.
        private static readonly HashSet<string> KnownAttrKeys = new()
        {
            TypeInfo.AttrKeys.Index,
            TypeInfo.AttrKeys.Asset,
        };

        /// <summary>
        /// 跑一次校验. 调用前必须确保 <paramref name="importer"/> 的
        /// <c>CollectFiles</c> + <c>AnaTable</c> 已经成功完成 (无需先 GenByte / GenCode).
        /// </summary>
        public static ValidationReport Run(Config.ConfImporter? importer)
        {
            var report = new ValidationReport();
            if (importer == null) return report;

            // 收集本地化表与普通表主键集合
            // ReSharper disable once InconsistentNaming
            var i18nKeys = CollectI18nKeys(importer);
            var primaryKeys = CollectPrimaryKeys(importer);

            // 普通表
            var commonTable = FindTypeDec<CommonTable>(importer);
            if (commonTable != null)
            {
                foreach (var row in commonTable.EnumerateForValidation())
                {
                    var src = BuildCommonSource(row);
                    ValidateValue(report, row.Type, row.Value, src, i18nKeys, primaryKeys);
                }
            }

            // 全局表
            var globalTable = FindTypeDec<GlobalTable>(importer);
            // ReSharper disable once InvertIf
            if (globalTable != null)
            {
                foreach (var view in globalTable.EnumerateForValidation())
                {
                    var src = $"G.{view.TableName}.{view.FieldName}";
                    ValidateValue(report, view.Type, view.Value, src, i18nKeys, primaryKeys);
                }
            }
            return report;
        }

        private static T? FindTypeDec<T>(Config.ConfImporter importer) where T : class
        {
            // ReSharper disable once InconsistentlySynchronizedField
            foreach (var dec in importer.TableDec.Type.TypeInfos)
            {
                if (dec is T t) return t;
            }
            return null;
        }

        // ReSharper disable once InconsistentNaming
        private static IReadOnlyDictionary<string, HashSet<string>> CollectI18nKeys(Config.ConfImporter importer)
        {
            // ReSharper disable once InconsistentNaming
            var i18n = FindTypeDec<I18nTable>(importer);
            return i18n != null ? i18n.CollectKeySets() : new Dictionary<string, HashSet<string>>();
        }

        private static IReadOnlyDictionary<string, HashSet<Id>> CollectPrimaryKeys(Config.ConfImporter importer)
        {
            var ct = FindTypeDec<CommonTable>(importer);
            return ct != null ? ct.CollectPrimaryKeySets() : new Dictionary<string, HashSet<Id>>();
        }

        private static string BuildCommonSource(in CommonTableRowView row)
        {
            var key = row.Key.IsNumId ? row.Key.Num.ToString() : row.Key.Str;
            return string.IsNullOrEmpty(row.TableSubId)
                ? $"{row.TableName}[{key}].{row.FieldName}"
                : $"{row.TableName}|{row.TableSubId}[{key}].{row.FieldName}";
        }

        // ─────────────────────────────────────────────────────────────────
        // 数据递归校验
        // ─────────────────────────────────────────────────────────────────

        private static void ValidateValue(
            ValidationReport report,
            in TypeInfo type,
            object? value,
            string source,
            // ReSharper disable once InconsistentNaming
            IReadOnlyDictionary<string, HashSet<string>> i18nKeys,
            IReadOnlyDictionary<string, HashSet<Id>> primaryKeys)
        {
            // 先校验注解本身的合法性 (一次性, 而非每个元素一次)
            ValidateAttribute(report, type, source);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (type.Container)
            {
                case TypeInfo.EContainer.None:
                    ValidateScalarOrCustom(report, type, value, source, i18nKeys, primaryKeys);
                    break;
                case TypeInfo.EContainer.Array:
                    if (value is object?[] arr)
                    {
                        for (var i = 0; i < arr.Length; i++)
                        {
                            var elemSrc = source + $"[{i}]";
                            ValidateScalarOrCustom(report, type, arr[i], elemSrc, i18nKeys, primaryKeys);
                        }
                    }
                    break;
                case TypeInfo.EContainer.Dictionary:
                    if (value is object?[] flat)
                    {
                        // dict 是 [k0, v0, k1, v1, ...] 形式
                        for (var i = 0; i + 1 < flat.Length; i += 2)
                        {
                            var keyStr = flat[i]?.ToString() ?? string.Empty;
                            var elemSrc = source + $"[{keyStr}]";
                            ValidateScalarOrCustom(report, type, flat[i + 1], elemSrc, i18nKeys, primaryKeys);
                        }
                    }
                    break;
            }
        }

        private static void ValidateScalarOrCustom(
            ValidationReport report,
            in TypeInfo type,
            object? value,
            string source,
            // ReSharper disable once InconsistentNaming
            IReadOnlyDictionary<string, HashSet<string>> i18nKeys,
            IReadOnlyDictionary<string, HashSet<Id>> primaryKeys)
        {
            var valType = type.ValType;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (valType)
            {
                case TypeInfo.EBaseType.Custom:
                {
                    if (value is not object?[] structFields) return; // null custom 跳过
                    for (var i = 0; i < type.FieldCount && i < structFields.Length; i++)
                    {
                        var (field, fieldIdx) = type.GetFieldByOrder(i);
                        if (fieldIdx < 0) continue;
                        var fSrc = source + "." + field.Name;
                        var fVal = structFields[fieldIdx];

                        // 自定义结构体内部仅检查 Text 类型, 因为字段级别没有携带注解信息.
                        if (field.Type == TypeInfo.EBaseType.Text)
                            ValidateText(report, fVal, fSrc, i18nKeys);
                    }
                    return;
                }
                // 1. 基础类型 Text 永远校验本地化合法性
                case TypeInfo.EBaseType.Text:
                    ValidateText(report, value, source, i18nKeys);
                    break;
            }

            // 2. 注解校验
            if (type.TryGetAttr(TypeInfo.AttrKeys.Index, out var indexTarget))
                ValidateIndex(report, value, source, indexTarget, primaryKeys);

            if (type.HasAttr(TypeInfo.AttrKeys.Asset))
                ValidateAsset(report, value, source);
        }

        private static void ValidateText(
            ValidationReport report,
            object? value,
            string source,
            // ReSharper disable once InconsistentNaming
            IReadOnlyDictionary<string, HashSet<string>> i18nKeys)
        {
            // null / 空允许 (由 Nullable 决定)
            if (value is null) return;
            var s = value as string ?? value.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(s)) return;

            if (!TryParseTextLiteral(s, out var table, out var key))
            {
                report.Add(new ValidationIssue(
                    ESeverity.Error, source, ValidationCategory.Text,
                    $"text 字段值不符合 [TABLE,KEY] 格式: '{s}'"));
                return;
            }

            if (!i18nKeys.TryGetValue(table, out var keys))
            {
                report.Add(new ValidationIssue(
                    ESeverity.Error, source, ValidationCategory.Text,
                    $"找不到本地化表 '{table}' (text 引用: '{s}')"));
                return;
            }

            if (!keys.Contains(key))
            {
                report.Add(new ValidationIssue(
                    ESeverity.Error, source, ValidationCategory.Text,
                    $"本地化表 '{table}' 中不存在 key '{key}'"));
            }
        }

        private static void ValidateIndex(
            ValidationReport report,
            object? value,
            string source,
            string indexTarget,
            IReadOnlyDictionary<string, HashSet<Id>> primaryKeys)
        {
            if (value is null) return; // 可空字段允许 null
            if (string.IsNullOrWhiteSpace(indexTarget))
            {
                report.Add(new ValidationIssue(
                    ESeverity.Warning, source, ValidationCategory.Index,
                    "<index> 注解未提供目标表名 (期望 <index=TableName>), 跳过校验"));
                return;
            }

            if (!primaryKeys.TryGetValue(indexTarget, out var keys))
            {
                report.Add(new ValidationIssue(
                    ESeverity.Error, source, ValidationCategory.Index,
                    $"<index> 引用的目标表 '{indexTarget}' 不存在"));
                return;
            }

            // 把 value 标准化为 Id, 再比对
            var id = ToId(value);
            if (id.IsNull) return;
            // Id 数值与字符串等价判定: 普通表的 Id 总是来自相同基类型, 直接 contains.
            if (keys.Contains(id)) return;

            // 兼容: 若 value 是数字而表存的是数字串 (反之亦然), 转一次再比.
            if (id.IsNumId)
            {
                if (keys.Contains(new Id(id.Num.ToString()))) return;
            }
            else if (long.TryParse(id.Str, out var n))
            {
                if (keys.Contains(new Id(n))) return;
            }

            report.Add(new ValidationIssue(
                ESeverity.Error, source, ValidationCategory.Index,
                $"<index={indexTarget}> 字段值 '{id.Str}' 不在 '{indexTarget}' 表的主键集合中"));
        }

        private static void ValidateAsset(ValidationReport report, object? value, string source)
        {
            if (value is null) return;
            var s = value as string ?? value.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(s))
            {
                // 可空 asset 字段为空时不报错 (与 nullable 语义一致)
                return;
            }
            report.AddAsset(new AssetCandidate(source, s));
        }

        private static void ValidateAttribute(ValidationReport report, in TypeInfo type, string source)
        {
            if (string.IsNullOrEmpty(type.Attribute)) return;
            foreach (var k in type.AttributeMap.Keys)
            {
                if (KnownAttrKeys.Contains(k)) continue;
                report.Add(new ValidationIssue(
                    ESeverity.Warning, source, ValidationCategory.Attribute,
                    $"未识别的注解 key: '{k}' (原始注解: '{type.Attribute}')"));
            }
        }

        private static Id ToId(object value)
        {
            return value switch
            {
                Id id => id,
                string s => new Id(s),
                sbyte sv => new Id(sv),
                short sv => new Id(sv),
                int sv => new Id(sv),
                long sv => new Id(sv),
                byte sv => new Id(sv),
                ushort sv => new Id(sv),
                uint sv => new Id(sv),
                ulong sv => new Id((long)sv),
                _ => new Id(value.ToString())
            };
        }

        /// <summary>
        /// 解析 [TABLE,KEY] 文本字面量. 失败时返回 false. TABLE / KEY 都将被 trim.
        /// </summary>
        public static bool TryParseTextLiteral(string s, out string table, out string key)
        {
            table = string.Empty;
            key = string.Empty;
            if (string.IsNullOrEmpty(s)) return false;
            s = s.Trim();
            if (s.Length < 4) return false;
            if (s[0] != '[' || s[^1] != ']') return false;

            var inner = s.Substring(1, s.Length - 2);
            var commaIdx = inner.IndexOf(',');
            if (commaIdx < 0) return false;

            table = inner.Substring(0, commaIdx).Trim();
            key = inner.Substring(commaIdx + 1).Trim();
            return Builtin.Util.StringUtil.IsUnicodeIdent(table.AsSpan()) &&
                   Builtin.Util.StringUtil.IsUnicodeIdent(key.AsSpan());
        }
    }
}
