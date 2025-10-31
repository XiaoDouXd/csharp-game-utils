#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConfImporter.Builtin.Type;
using ConfImporter.Builtin.Util;
using ConfImporter.Builtin.Util.Gen;
using ConfImporter.Config;
using ConfImporter.Table;

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin
{
    public partial class CommonTable : TableTypeDec
    {
        public override bool CheckSheetName(string sheetName, string fileName, in SheetReader reader)
        {
            var sheetNameSpan = sheetName.AsSpan();

            StringUtil.TrimHead(ref sheetNameSpan);
            var idx = StringUtil.FindFirst(sheetNameSpan, '<');
            if (idx < 0) return false;

            sheetNameSpan = sheetNameSpan[(idx + 1)..];
            StringUtil.TrimHead(ref sheetNameSpan);
            idx = StringUtil.FindFirst(sheetNameSpan, '>');
            if (idx <= 0) return false;
            sheetNameSpan = sheetNameSpan[..idx];
            if (sheetNameSpan.Length == 1 && sheetNameSpan[0] == '*')
            {
                sheetNameSpan = reader.Read(0, 0).AsSpan();
                StringUtil.TrimHead(ref sheetNameSpan);
                idx = StringUtil.FindFirst(sheetNameSpan, '<');
                if (idx <= 0) return false;
                sheetNameSpan = sheetNameSpan[(idx + 1)..];
                StringUtil.TrimHead(ref sheetNameSpan);
                idx = StringUtil.FindFirst(sheetNameSpan, '>');
                if (idx <= 0) return false;
                sheetNameSpan = sheetNameSpan[..idx];
            }

            idx = StringUtil.FindFirst(sheetNameSpan, '|');
            if (idx > 0) sheetNameSpan = sheetNameSpan[..idx];
            var programName = StringUtil.MatchProgramValidName(ref sheetNameSpan);
            if (programName.IsEmpty) return false;
            if (programName.StartsWith("__")) return false;

            var lower = fileName.ToLower();
            if (lower.StartsWith("g-")) return false;
            return !lower.StartsWith("i18n-");
        }

        public override int RowHeadSize => 4;

        public override ITableInst? New(string sheetName, string fileName, in SheetReader reader)
        {
            var sheetNameSpan = sheetName.AsSpan();

            StringUtil.TrimHead(ref sheetNameSpan);
            var idx = StringUtil.FindFirst(sheetNameSpan, '<');
            if (idx < 0) return null;

            sheetNameSpan = sheetNameSpan[(idx + 1)..];
            StringUtil.TrimHead(ref sheetNameSpan);
            idx = StringUtil.FindFirst(sheetNameSpan, '>');
            if (idx <= 0) return null;
            sheetNameSpan = sheetNameSpan[..idx];
            if (sheetNameSpan.Length == 1 && sheetNameSpan[0] == '*')
            {
                sheetNameSpan = reader.Read(0, 0).AsSpan();
                StringUtil.TrimHead(ref sheetNameSpan);
                idx = StringUtil.FindFirst(sheetNameSpan, '<');
                if (idx <= 0) return null;
                sheetNameSpan = sheetNameSpan[(idx + 1)..];
                StringUtil.TrimHead(ref sheetNameSpan);
                idx = StringUtil.FindFirst(sheetNameSpan, '>');
                if (idx <= 0) return null;
                sheetNameSpan = sheetNameSpan[..idx];
            }

            idx = StringUtil.FindFirst(sheetNameSpan, '|');
            var fullSheetNameSpan = sheetNameSpan; // 带了 id 的表名
            if (idx > 0) sheetNameSpan = sheetNameSpan[..idx]; // 不带 id 的表名

            var programName = StringUtil.MatchProgramValidName(ref sheetNameSpan);
            var sName = programName.ToString();
            if (sName.StartsWith("__")) return null;
            return string.IsNullOrWhiteSpace(sName) ? null : new TableInst(this, fullSheetNameSpan.ToString(), fileName + ':' + sheetName);
        }

        public override void Clear()
        {
            lock (_cfg) _cfg.Clear();
        }

        private void SetTableBreak(TableInst inst)
        {
            lock (_cfg)
            {
                if (!_cfg.TryGetValue(inst.SheetName, out var table)) return;
                table.IsBreak = true;
            }
        }

        private bool AddCfgField(TableInst inst, IEnumerable<FieldInfo> fields)
        {
            if (string.IsNullOrWhiteSpace(inst.SheetName)) Conf.Logger.Error("错误, 获得了空配置名");
            if (!StringUtil.IsProgramValidName(inst.SheetName)) Conf.Logger.Error($"错误, 获得了非法配置名{inst.SheetName}");

            lock (_cfg)
            {
                var cfgName = ParseSheetName(inst.SheetName);
                if (!_cfg.TryGetValue(cfgName.name, out var cfgInst))
                {
                    var ret = _cfg.TryAdd(cfgName.name, cfgInst = new CombinedTableInst(cfgName.name));
                    if (!ret)
                    {
                        Conf.Logger.Error($"尝试添加配置表失败, 表: {cfgName}");
                        return false;
                    }
                }

                if (cfgInst.IsBreak)
                {
                    Conf.Logger.Warning($"被跳过的表: {cfgName}");
                    return false;
                }

                // 检查 fields 匹配性
                foreach (var info in fields)
                {
                    if (!info.IsValid)
                    {
                        Conf.Logger.Error($"不合法的字段! 表: {cfgName}, 字段: {info.Name}");
                        cfgInst.IsBreak = true;
                        return false;
                    }

                    if (cfgInst.Fields.TryGetValue(info.Name, out var field))
                    {
                        if (field.TypeUniqueStr == info.TypeUniqueStr) continue;
                        Conf.Logger.Error($"字段类型冲突! 表: {cfgName}, 字段: {info.Name}, 类型: {info.TypeUniqueStr}/{field.TypeUniqueStr}");
                        cfgInst.IsBreak = true;
                        return false;
                    }

                    CommonPostGen.AddType(info.Type!.Value, Conf);
                    cfgInst.Fields.Add(info.Name, info);
                }
            }
            return true;
        }

        private bool AddCfgRowData(TableInst inst, IEnumerable<(string Field, object? Value)> obj, CfgUtil.Id key)
        {
            lock (_cfg)
            {
                var cfgName = ParseSheetName(inst.SheetName);
                if (!_cfg.TryGetValue(cfgName.name, out var cfgInst))
                {
                    Conf.Logger.Error($"错误! 插入数据失败, 找不到配置表字段信息: {inst.SheetName}");
                    return false;
                }

                if (!cfgInst.Tables.TryGetValue(cfgName.id, out var tableInst))
                    cfgInst.Tables.Add(cfgName.id, tableInst = new CombinedTableInst.CombinedTableSingleInst(cfgName.id));
                if (tableInst.Data.ContainsKey(key))
                {
                    Conf.Logger.Error($"错误! id 重复: {inst.SheetName}");
                    return false;
                }

                var rowData = new CombinedTableInst.RowData(key);
                rowData.Data.AddRange(obj);
                tableInst.DataList.Add(rowData);
                tableInst.Data.Add(key, rowData);
            }
            return true;
        }

        private static (string name, string id) ParseSheetName(string? sheetName)
        {
            // sheet name format: "SheetTypeName|SheetId", "|SheetId" 可有可无
            if (string.IsNullOrEmpty(sheetName)) return (string.Empty, string.Empty);
            var sheetType = sheetName.AsSpan();
            var name = StringUtil.MatchProgramValidName(ref sheetType);
            return (name.ToString(), sheetType.Length > 1 ? sheetType[1..].ToString() : string.Empty);
        }

        private readonly ConcurrentDictionary<string, CombinedTableInst> _cfg = new();

        private class CombinedTableInst
        {
            // ReSharper disable once RedundantDefaultMemberInitializer
            public bool IsBreak { get; set; } = false;
            public string Name { get; }
            public Dictionary<string, CombinedTableSingleInst> Tables { get; } = new();
            public SortedDictionary<string, FieldInfo> Fields { get; } = new();
            public CombinedTableInst(string name) => Name = name;

            public class RowData
            {
                public CfgUtil.Id Id { get; }
                public List<(string Field, object? Value)> Data { get; } = new();

                public RowData(CfgUtil.Id id) => Id = id;
            }

            public class CombinedTableSingleInst
            {
                public string Id { get; }
                public List<RowData> DataList { get; } = new();
                public Dictionary<CfgUtil.Id, RowData> Data { get; } = new();

                public CombinedTableSingleInst(string id) => Id = id;
            }
        }

        private class TableInst : ITableInst
        {
            public string SheetName { get; }

            public TableInst(CommonTable tableDec, string name, string fullName)
            {
                _dec = tableDec;
                SheetName = name;
                _fullName = fullName;
            }

            #region Row
            public ITableInst.EFailOp BeginRow(int rowIdx, CancellationToken cancellationToken)
            {
                _curRowIdx = rowIdx;
                _curId = default;
                _rowData.Clear();
                return ITableInst.EFailOp.Success;
            }

            public ITableInst.EFailOp LoadContent(IReadOnlyList<string> colHead, string content, int idx, CancellationToken cancellationToken)
            {
                if (idx >= _fields.Count) return ITableInst.EFailOp.Success;
                var field = _fields[idx];
                if (!field.IsValid) return ITableInst.EFailOp.Success;

                var contentSpan = content.AsSpan();
                try
                {
                    var data = Analyzer.EatValueGroups(ref contentSpan, field.Type!.Value);
                    if (!StringUtil.IsEmptyOrWhiteSpace(contentSpan))
                    {
                        _dec.Conf.Logger.Error($"错误! 字段值内容解析失败, 表: {SheetName}, 字段名: {field.Name}-{content}, 字段行号 {_curRowIdx}, 字段列号: {idx}, 字段失配内容: {contentSpan.ToString()}");
                        return ITableInst.EFailOp.BreakTable;
                    }

                    // id 字段
                    if (field.Name == "Id")
                    {
                        if (!_curId.IsNull)
                        {
                            _dec.Conf.Logger.Error($"错误! 出现了不止一个 Id 字段, 表: {SheetName}");
                            _dec.SetTableBreak(this);
                            return ITableInst.EFailOp.BreakTable;
                        }

                        _curId = (CfgUtil.Id)data[0][0];
                        if (data.Length <= 0 || _curId.IsNull)
                        {
                            _dec.Conf.Logger.Error($"错误! Id 为空, 表: {SheetName}, 行: {_curRowIdx}");
                            _dec.SetTableBreak(this);
                            return ITableInst.EFailOp.BreakTable;
                        }
                    }

                    var obj = Analyzer.ToObject(field.Type!.Value, data, _filters);
                    _rowData.Add((field.Name, obj));
                }
                catch (Exception e)
                {
                    _dec.Conf.Logger.Error($"错误! {e}");
                    return ITableInst.EFailOp.BreakTable;
                }
                return ITableInst.EFailOp.Success;
            }

            public ITableInst.EFailOp EndRow(CancellationToken cancellationToken)
            {
                if (_curId.IsNull)
                {
                    _dec.Conf.Logger.Error($"错误! Id 为空, 表: {SheetName}, 行: {_curRowIdx}");
                    _dec.SetTableBreak(this);
                    return ITableInst.EFailOp.BreakTable;
                }

                var ret = _dec.AddCfgRowData(this, _rowData, _curId);
                if (!ret) _dec.Conf.Logger.Error($"添加行数据出错: 表: {SheetName}, 行: {_curRowIdx}, id: {_curId}");
                return !ret ? ITableInst.EFailOp.BreakLine : ITableInst.EFailOp.Success;
            }

            private int _curRowIdx;
            private CfgUtil.Id _curId;

            private readonly HashSet<AnyBase> _filters = new();
            private readonly List<(string Field, object? Value)> _rowData = new();
            #endregion

            #region Fields
            public ITableInst.EFailOp LoadRowHead(IReadOnlyList<string> rowHead, int rowIdx, CancellationToken cancellationToken)
            {
                if (rowIdx == 0) return ITableInst.EFailOp.Success;
                for (var i = 0; i < rowHead.Count; i++)
                {

                    if (cancellationToken.IsCancellationRequested) return ITableInst.EFailOp.Cancel;

                    var head = rowHead[i];
                    if (string.IsNullOrWhiteSpace(head)) continue;

                    switch (rowIdx)
                    {
                        // 第零行标题 和 注释
                        case 1: // 第一行类型
                            try
                            {
                                var type = Analyzer.ToType(head);
                                if (!string.IsNullOrEmpty(type.Alias))
                                    // ReSharper disable once InconsistentlySynchronizedField
                                    _dec.Conf.Logger.Warning("别名不支持");
                                else if (_fields.Count < i)
                                {
                                    AddEmptyFieldTo(_fields, i);
                                    _fields[i].Type = type;
                                }
                                else if (_fields.Count == i) _fields.Add(new FieldInfo { Type = type });
                                else _fields[i].Type = type;
                            }
                            // ReSharper disable once InconsistentlySynchronizedField
                            catch (Exception e) { _dec.Conf.Logger.Warning(e.ToString()); }
                            continue;
                        case 2: // 第二行导出控制 (client 为客户端导出)
                            if (!head.Contains("client")) continue;
                            if (_fields.Count < i)
                            {
                                AddEmptyFieldTo(_fields, i);
                                _fields[i].IsHidden = false;
                            }
                            else if (_fields.Count == i) _fields.Add(new FieldInfo { IsHidden = false });
                            else _fields[i].IsHidden = false;
                            continue;
                        case 3: // 第三行变量名
                            var headSp = head.AsSpan().TrimStart();
                            var name = StringUtil.MatchProgramValidName(ref headSp);
                            if (name.IsEmpty) continue;
                            var nameStr = name.ToString();
                            if (_fields.Count < i)
                            {
                                AddEmptyFieldTo(_fields, i);
                                _fields[i].Name = nameStr;
                            }
                            else if (_fields.Count == i) _fields.Add(new FieldInfo{ Name = nameStr });
                            else _fields[i].Name = nameStr;
                            if (nameStr.StartsWith("__"))
                            {
                                _dec.Conf.Logger.Error("错误! 字段名不允许以 '__' 字符串开头");
                                _fields[i].IsHidden = true;
                            }
                            continue;
                    }
                }

                return ITableInst.EFailOp.Success;

                static void AddEmptyFieldTo(List<FieldInfo> l, int targetLen)
                {
                    while (targetLen >= l.Count) l.Add(new FieldInfo());
                }
            }

            public ITableInst.EFailOp EndLoadRowHead(CancellationToken cancellationToken)
            {
                // 过滤不合法的字段
                var validFields = _fields.Where(f => f.IsValid);
                var isMatchId = false;
                _fieldFilter.Clear();
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var info in validFields)
                {
                    if (!_fieldFilter.Add(info.Name))
                    {
                        _dec.Conf.Logger.Error($"错误! 表中字段名重复, 表: {_fullName}, 重复字段名: {info.Name}");
                        _dec.SetTableBreak(this);
                        return ITableInst.EFailOp.BreakTable;
                    }

                    // 检查 id
                    if (info.Name != "Id") continue;
                    if (!info.Type!.Value.IsIdType)
                    {
                        _dec.Conf.Logger.Error($"错误! 表中 id 字段类型错误! id 字段类型只能为不可空的 整数类型 或 string, 表 {_fullName}");
                        _dec.SetTableBreak(this);
                        return ITableInst.EFailOp.BreakTable;
                    }
                    isMatchId = true;
                }
                if (!isMatchId)
                {
                    _dec.Conf.Logger.Error($"错误! 表中找不到 id 字段: 表 {_fullName}");
                    _dec.SetTableBreak(this);
                    return ITableInst.EFailOp.BreakTable;
                }

                // 处理字段名和字段类型
                // ReSharper disable once PossibleMultipleEnumeration
                var ret = _dec.AddCfgField(this, validFields);
                return !ret ? ITableInst.EFailOp.BreakTable : ITableInst.EFailOp.Success;
            }

            private readonly List<FieldInfo> _fields = new();
            private readonly HashSet<string> _fieldFilter = new();
            #endregion

            /// 父结构
            private readonly CommonTable _dec;
            /// 表全名, debug 用
            private readonly string _fullName;
        }

        // ReSharper disable RedundantDefaultMemberInitializer
        private class FieldInfo
        {
            public int Idx { get; set; } = -1;
            public bool IsHidden { get; set; } = true;
            public TypeInfo? Type
            {
                get => _type;
                set
                {
                    TypeUniqueStr = value?.ToString(true);
                    _type = value;
                }
            }
            public string Name { get; set; } = string.Empty;

            public string? TypeUniqueStr { get; private set; }

            public bool IsValid => !IsHidden && Type != null && !string.IsNullOrWhiteSpace(Name);

            private TypeInfo? _type = null;
        }
        // ReSharper restore RedundantDefaultMemberInitializer

        public CommonTable(Config.ConfImporter conf) : base(conf) {}
    }
}