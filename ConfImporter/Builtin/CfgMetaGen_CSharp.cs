#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Text;
using ConfImporter.Builtin.Type;
using ConfImporter.Builtin.Util;
using MessagePack;
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin
{
    /// <summary>
    /// 类型注解元数据生成器:
    /// <list type="bullet">
    ///   <item>
    ///     <c>CfgGenMeta.bytes</c> (与 <c>CommonTable.bytes</c> 同源, MessagePack 数组结构),
    ///     运行时按需热刷反序列化, 不写死.
    ///   </item>
    ///   <item>
    ///     <c>CfgGenMeta.json</c> 友好排版供人查阅.
    ///   </item>
    ///   <item>
    ///     <c>CfgGenMeta.g.cs</c> 反序列化器: 通过 <see cref="ConfImporter"/> 项目无法引用的运行时
    ///     接口 <c>CfgUtil.IDeserializeMethod</c> 把 bytes 还原成静态字典, 然后 <c>CfgMeta</c>
    ///     提供查询 helper (运行时只读, 但每次 init 流程都会被重新填充).
    ///   </item>
    /// </list>
    /// </summary>
    internal static class CfgMetaGen
    {
        public const string FileBaseName = "CfgGenMeta";

        public static long Version { get; set; } = 1;

        public static void GenerateBytes(Config.ConfImporter conf)
        {
            var commonMeta = CollectMeta(conf, isCommon: true);
            var globalMeta = CollectMeta(conf, isCommon: false);

            // bytes 结构 (扁平数组, 不带 schema 头, 与 GlobalTable 一致的 [version, ...] 风格):
            // [
            //   version: long,
            //   commonSection: [
            //     [tableName, [ [fieldName, [k1, v1, k2, v2, ...]], ... ] ],
            //     ...
            //   ],
            //   globalSection: 同上
            // ]
            var root = new List<object>
            {
                conf.Version,
                BuildSection(commonMeta),
                BuildSection(globalMeta),
            };

            var bytes = MessagePackSerializer.Serialize(root);
            using (var f = File.Create(conf.ByteOutputTargetDir + "/" + FileBaseName + ".bytes"))
                f.Write(bytes, 0, bytes.Length);

            // 友好 JSON
            var jsonRoot = new Dictionary<string, object?>
            {
                ["version"] = conf.Version,
                ["common"]  = BuildJsonSection(commonMeta),
                ["global"]  = BuildJsonSection(globalMeta),
            };
            var jsonDir = conf.ResolveJsonOutputDir();
            Directory.CreateDirectory(jsonDir);
            PrettyJson.WriteFile(jsonDir + "/" + FileBaseName + ".json", jsonRoot);
        }

        public static void GenerateCode(Config.ConfImporter conf)
        {
            var ns = conf.CodeNamespace ?? "XD.A0.Game.Runtime.Config";
            var sb = new StringBuilder();
            sb.Append(@"// ReSharper disable UnusedNullableDirective
// ReSharper disable RedundantUsingDirective

#nullable enable

using System.Collections.Generic;
using XD.Common.Config.Helper;
using XD.GameModule.Module.MConfig;

// ReSharper disable All
// ReSharper disable InconsistentNaming
#pragma warning disable CS8618, CS9264

// ReSharper disable once CheckNamespace
");
            sb.Append("namespace ").Append(ns).Append("\n{\n");

            // 静态查询入口. 数据由 ____cfgGenFunction.CreateCfgMeta 在运行时 init 阶段填充.
            sb.Append(
@"    /// <summary>
    /// 由导表工具写入 CfgGenMeta.bytes 的字段类型注解元数据.
    /// 通过 <c>CommonTable</c> / <c>GlobalTable</c> 找字段的 (key -> value) 注解集.
    /// 数据在 <see cref=""ConfigModule""/> 初始化阶段从 bytes 反序列化得到, 支持热刷.
    /// </summary>
    public static partial class CfgMeta
    {
        // 注意: 这些 Empty* 必须放在使用它们作为初值的成员之前 (C# 静态字段按文本顺序初始化).
        private static readonly IReadOnlyDictionary<string, string> EmptyInner = new Dictionary<string, string>();
        private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> EmptyOuter = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>>();

        /// <summary> 普通表的字段注解元数据 (TableName -> FieldName -> AttrKey -> AttrValue). </summary>
        public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> CommonTable { get; private set; }
            = EmptyOuter!;

        /// <summary> 全局表的字段注解元数据. </summary>
        public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> GlobalTable { get; private set; }
            = EmptyOuter!;

        /// <summary> 当前已加载的 meta 版本号. </summary>
        public static long Version { get; private set; }

        public static IReadOnlyDictionary<string, string> GetCommonFieldAttrs(string tableName, string fieldName)
            => CommonTable.TryGetValue(tableName, out var fields) && fields.TryGetValue(fieldName, out var attrs)
                ? attrs : EmptyInner;

        public static IReadOnlyDictionary<string, string> GetGlobalFieldAttrs(string tableName, string fieldName)
            => GlobalTable.TryGetValue(tableName, out var fields) && fields.TryGetValue(fieldName, out var attrs)
                ? attrs : EmptyInner;

        public static bool TryGetCommonAttr(string tableName, string fieldName, string attrKey, out string value)
        {
            value = string.Empty;
            return GetCommonFieldAttrs(tableName, fieldName).TryGetValue(attrKey, out value!);
        }

        public static bool TryGetGlobalAttr(string tableName, string fieldName, string attrKey, out string value)
        {
            value = string.Empty;
            return GetGlobalFieldAttrs(tableName, fieldName).TryGetValue(attrKey, out value!);
        }

        /// <summary> 由生成的反序列化函数写入运行时数据 (热刷友好). 业务侧不应直接调用. </summary>
        internal static void __SetData(
            long version,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> common,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> global)
        {
            Version = version;
            CommonTable = common;
            GlobalTable = global;
        }
    }

    public static partial class ____cfgGenFunction
    {
        /// <summary>
        /// 反序列化 <c>CfgGenMeta.bytes</c>, 把数据塞进 <see cref=""CfgMeta""/> 的静态字段.
        /// 返回值 <see cref=""CfgUtil.CfgMetaCreateResult""/> 仅用于让 formatter 完成调用链, 不携带数据.
        /// </summary>
        public static CfgUtil.CfgMetaCreateResult CreateCfgMeta(ref CfgUtil.SerializedData data, CfgUtil.IDeserializeMethod method)
        {
            if (method == null) return default;
            var version = method.BeginScope(ref data);
            if (version == 0) { method.EndScope(ref data); return default; }

            var common = ReadSection(ref data, method);
            var global = ReadSection(ref data, method);

            method.EndScope(ref data);
            CfgMeta.__SetData(version, common, global);
            return new CfgUtil.CfgMetaCreateResult(true, version);
        }

        // ─── 反序列化辅助 (避开 ReadArray<TRet> 的泛型构造器要求, 因为这里都是动态 string) ───
        // 所有读取严格遵循 IDeserializeMethod 的 Begin/End 配对约定, 与 Common/Global 写法对齐.

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> ReadSection(
            ref CfgUtil.SerializedData data, CfgUtil.IDeserializeMethod method)
        {
            // section 是一个数组, 每项形如 [tableName, [ [fieldName, [k1,v1,...]], ... ] ]
            var ret = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>>();
            ref var reader = ref data.Reader;
            if (reader.TryReadNil()) return ret;

            data.Options.Security.DepthStep(ref reader);
            var len = reader.ReadArrayHeader();
            for (var i = 0; i < len; i++)
            {
                if (reader.TryReadNil()) continue;
                data.Options.Security.DepthStep(ref reader);
                var entryLen = reader.ReadArrayHeader();
                if (entryLen < 2)
                {
                    for (var k = 0; k < entryLen; k++) reader.Skip();
                    reader.Depth--;
                    continue;
                }
                var tableName = reader.ReadString() ?? string.Empty;
                var fields = ReadFieldList(ref data, method);
                // 跳过未来可能新增的 entry 项
                for (var k = 2; k < entryLen; k++) reader.Skip();
                reader.Depth--;

                ret[tableName] = fields;
            }
            reader.Depth--;
            return ret;
        }

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ReadFieldList(
            ref CfgUtil.SerializedData data, CfgUtil.IDeserializeMethod method)
        {
            var ret = new Dictionary<string, IReadOnlyDictionary<string, string>>();
            ref var reader = ref data.Reader;
            if (reader.TryReadNil()) return ret;

            data.Options.Security.DepthStep(ref reader);
            var len = reader.ReadArrayHeader();
            for (var i = 0; i < len; i++)
            {
                if (reader.TryReadNil()) continue;
                data.Options.Security.DepthStep(ref reader);
                var entryLen = reader.ReadArrayHeader();
                if (entryLen < 2)
                {
                    for (var k = 0; k < entryLen; k++) reader.Skip();
                    reader.Depth--;
                    continue;
                }
                var fieldName = reader.ReadString() ?? string.Empty;
                var attrs = ReadAttrMap(ref data, method);
                for (var k = 2; k < entryLen; k++) reader.Skip();
                reader.Depth--;

                ret[fieldName] = attrs;
            }
            reader.Depth--;
            return ret;
        }

        private static IReadOnlyDictionary<string, string> ReadAttrMap(ref CfgUtil.SerializedData data, CfgUtil.IDeserializeMethod method)
        {
            var ret = new Dictionary<string, string>();
            ref var reader = ref data.Reader;
            if (reader.TryReadNil()) return ret;

            data.Options.Security.DepthStep(ref reader);
            var len = reader.ReadArrayHeader();
            // [k1, v1, k2, v2, ...] 扁平结构
            for (var i = 0; i + 1 < len; i += 2)
            {
                var k = reader.ReadString() ?? string.Empty;
                var v = reader.ReadString() ?? string.Empty;
                ret[k] = v;
            }
            // 奇数尾项跳过
            if (len % 2 == 1) reader.Skip();
            reader.Depth--;
            return ret;
        }
    }
}
");
            TextFile.WriteAllText(conf.CodeOutputTargetDir + "/" + FileBaseName + ".g.cs", sb.ToString());
        }

        // ─────────────────────────────────────────────────────────────────
        // 数据收集
        // ─────────────────────────────────────────────────────────────────

        private static SortedDictionary<string, SortedDictionary<string, IReadOnlyDictionary<string, string>>> CollectMeta(
            Config.ConfImporter conf, bool isCommon)
        {
            var meta = new SortedDictionary<string, SortedDictionary<string, IReadOnlyDictionary<string, string>>>();
            foreach (var dec in conf.TableDec.Type.TypeInfos)
            {
                if (isCommon && dec is CommonTable ct)
                {
                    foreach (var (table, field, type) in ct.EnumerateFieldDefs())
                        Add(meta, table, field, type);
                }
                else if (!isCommon && dec is GlobalTable gt)
                {
                    foreach (var (table, field, type) in gt.EnumerateFieldDefs())
                        Add(meta, table, field, type);
                }
            }
            return meta;

            static void Add(
                SortedDictionary<string, SortedDictionary<string, IReadOnlyDictionary<string, string>>> meta,
                string tableName, string fieldName, in TypeInfo type)
            {
                var attrs = type.AttributeMap;
                if (attrs.Count == 0) return; // 无注解的字段不入表, 减小 bytes 体积
                if (!meta.TryGetValue(tableName, out var fields))
                    meta[tableName] = fields = new SortedDictionary<string, IReadOnlyDictionary<string, string>>();
                fields[fieldName] = attrs;
            }
        }

        private static List<object> BuildSection(
            SortedDictionary<string, SortedDictionary<string, IReadOnlyDictionary<string, string>>> meta)
        {
            var ret = new List<object>();
            foreach (var (tableName, fields) in meta)
            {
                var fieldList = new List<object>();
                foreach (var (fname, attrs) in fields)
                {
                    var flat = new List<object>();
                    foreach (var (k, v) in attrs)
                    {
                        flat.Add(k);
                        flat.Add(v ?? string.Empty);
                    }
                    fieldList.Add(new object[] { fname, flat });
                }
                ret.Add(new object[] { tableName, fieldList });
            }
            return ret;
        }

        private static List<object?> BuildJsonSection(
            SortedDictionary<string, SortedDictionary<string, IReadOnlyDictionary<string, string>>> meta)
        {
            var ret = new List<object?>();
            foreach (var (tableName, fields) in meta)
            {
                var fieldsJson = new List<object?>();
                foreach (var (fname, attrs) in fields)
                {
                    var attrEntries = new List<KeyValuePair<string, object?>>();
                    foreach (var (k, v) in attrs)
                        attrEntries.Add(new KeyValuePair<string, object?>(k, (v ?? string.Empty)));
                    fieldsJson.Add(new Dictionary<string, object?>
                    {
                        ["name"]  = fname,
                        ["attrs"] = attrEntries,
                    });
                }
                ret.Add(new Dictionary<string, object?>
                {
                    ["table"]  = tableName,
                    ["fields"] = fieldsJson,
                });
            }
            return ret;
        }
    }
}
