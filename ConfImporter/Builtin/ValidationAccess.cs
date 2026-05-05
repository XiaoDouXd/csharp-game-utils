#nullable enable

using System.Collections.Generic;
using ConfImporter.Builtin.Type;
using ConfImporter.Builtin.Util.Gen;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin
{
    /// <summary>
    /// 校验阶段消费普通表数据所用的只读视图, 由 <see cref="CommonTable"/> 暴露.
    /// </summary>
    internal readonly struct CommonTableRowView
    {
        /// <summary> 配置表名 (合并后的最终名) </summary>
        public readonly string TableName;
        /// <summary> 表 id, 无 id 时为空串 </summary>
        public readonly string TableSubId;
        /// <summary> 主键值 </summary>
        public readonly Id Key;
        /// <summary> 字段名 </summary>
        public readonly string FieldName;
        /// <summary> 字段类型信息 </summary>
        public readonly TypeInfo Type;
        /// <summary> 字段值. 具体结构参考 <c>Analyzer.ToObject</c> 的文档说明 </summary>
        public readonly object? Value;

        public CommonTableRowView(string tableName, string tableSubId, Id key, string fieldName, TypeInfo type, object? value)
        {
            TableName = tableName;
            TableSubId = tableSubId;
            Key = key;
            FieldName = fieldName;
            Type = type;
            Value = value;
        }
    }

    /// <summary>
    /// 校验阶段消费全局表数据所用的只读视图, 由 <see cref="GlobalTable"/> 暴露.
    /// </summary>
    internal readonly struct GlobalTableFieldView
    {
        public readonly string TableName;
        public readonly string FieldName;
        public readonly TypeInfo Type;
        public readonly object? Value;

        public GlobalTableFieldView(string tableName, string fieldName, TypeInfo type, object? value)
        {
            TableName = tableName;
            FieldName = fieldName;
            Type = type;
            Value = value;
        }
    }

    public partial class CommonTable
    {
        /// <summary>
        /// 枚举所有有效的行 x 字段, 供校验阶段消费.
        /// </summary>
        internal IEnumerable<CommonTableRowView> EnumerateForValidation()
        {
            lock (_cfg)
            {
                // 为避免迭代过程中被外部改动, 先拷贝一份
                var cfg = new List<CombinedTableInst>(_cfg.Values);
                foreach (var inst in cfg)
                {
                    if (inst.IsBreak) continue;
                    foreach (var single in inst.Tables.Values)
                    {
                        foreach (var row in single.DataList)
                        {
                            foreach (var (fname, value) in row.Data)
                            {
                                if (!inst.Fields.TryGetValue(fname, out var fi) || !fi.IsValid) continue;
                                if (fi.Type is not { } t) continue;
                                yield return new CommonTableRowView(inst.Name, single.Id, row.Id, fname, t, value);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 枚举所有普通表的主键集合, 供 <c>index</c> 注解校验使用.
        /// </summary>
        internal IReadOnlyDictionary<string, HashSet<Id>> CollectPrimaryKeySets()
        {
            var ret = new Dictionary<string, HashSet<Id>>();
            lock (_cfg)
            {
                foreach (var inst in _cfg.Values)
                {
                    if (inst.IsBreak) continue;
                    if (!ret.TryGetValue(inst.Name, out var set))
                        ret[inst.Name] = set = new HashSet<Id>();
                    foreach (var single in inst.Tables.Values)
                    foreach (var row in single.DataList)
                        set.Add(row.Id);
                }
            }
            return ret;
        }

        /// <summary>
        /// 枚举所有普通表的字段定义 (表名, 字段名, 类型). 用于代码 / meta 生成阶段.
        /// </summary>
        internal IEnumerable<(string tableName, string fieldName, TypeInfo type)> EnumerateFieldDefs()
        {
            lock (_cfg)
            {
                var snapshot = new List<(string, string, TypeInfo)>();
                foreach (var inst in _cfg.Values)
                {
                    if (inst.IsBreak) continue;
                    foreach (var fi in inst.Fields.Values)
                    {
                        if (!fi.IsValid || fi.Type is not { } t) continue;
                        snapshot.Add((inst.Name, fi.Name, t));
                    }
                }
                foreach (var item in snapshot) yield return item;
            }
        }
    }

    public partial class GlobalTable
    {
        internal IEnumerable<GlobalTableFieldView> EnumerateForValidation()
        {
            lock (_tables)
            {
                var snapshot = new List<(string tableName, string fieldName, TypeInfo type, object? data)>();
                foreach (var (tableName, fields) in _tables)
                foreach (var (fname, fdata) in fields)
                    snapshot.Add((tableName, fname, fdata.Type, fdata.Data));
                foreach (var item in snapshot)
                    yield return new GlobalTableFieldView(item.tableName, item.fieldName, item.type, item.data);
            }
        }

        /// <summary>
        /// 枚举所有全局表的字段定义 (表名, 字段名, 类型). 用于代码 / meta 生成阶段.
        /// </summary>
        internal IEnumerable<(string tableName, string fieldName, TypeInfo type)> EnumerateFieldDefs()
        {
            lock (_tables)
            {
                var snapshot = new List<(string, string, TypeInfo)>();
                foreach (var (tableName, fields) in _tables)
                foreach (var (fname, fdata) in fields)
                    snapshot.Add((tableName, fname, fdata.Type));
                foreach (var item in snapshot) yield return item;
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    public partial class I18nTable
    {
        /// <summary>
        /// 返回本地化表中所有已登记的 (表名, key) 集合 (合并所有语言).
        /// 任一语言登记过即算存在.
        /// </summary>
        internal IReadOnlyDictionary<string, HashSet<string>> CollectKeySets()
        {
            var ret = new Dictionary<string, HashSet<string>>();
            lock (_tables)
            {
                foreach (var langTables in _tables.Values)
                foreach (var (name, data) in langTables)
                {
                    if (!ret.TryGetValue(name, out var set))
                        ret[name] = set = new HashSet<string>();
                    foreach (var k in data.Data.Keys) set.Add(k);
                }
            }
            return ret;
        }
    }
}
