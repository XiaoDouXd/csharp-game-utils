#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ConfImporter.Builtin.Type;
using ConfImporter.Builtin.Util;
using ConfImporter.Config;
using MessagePack;

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin
{
    public partial class GlobalTable : TableTypeDec
    {
        public override bool CheckSheetName(string sheetName, string fileName)
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

            var programName = StringUtil.MatchProgramValidName(ref sheetNameSpan);
            return !programName.IsEmpty && fileName.ToLower().StartsWith("g-");
        }

        public override ITableInst? New(string sheetName, string fileName)
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

            var programName = StringUtil.MatchProgramValidName(ref sheetNameSpan);
            var sName = programName.ToString();
            if (sName.StartsWith("__")) return null;
            return string.IsNullOrWhiteSpace(sName) ? null : new TableInst(this, sName);
        }

        public override void GenBytes(CancellationToken c)
        {
            lock (_tables)
            {
                if (_tables.Count <= 0)
                {
                    using var fEmpty = File.Create(Conf.ByteOutputTargetDir + "/GlobalTable.bytes");
                    using var fWriterEmpty = new StreamWriter(fEmpty);
                    fWriterEmpty.Write("");
                    return;
                }

                var objList = new List<object?>();
                foreach (var table in _tables.Values)
                    objList.AddRange(table.Values.Select(field => field.Data));
                var bytes = MessagePackSerializer.Serialize(objList);
                using var f = File.Create(Conf.ByteOutputTargetDir + "/GlobalTable.bytes");
                f.Write(bytes);
                using var fJson = File.Create(Conf.ByteOutputTargetDir + "/GlobalTable.json");
                using var fJsonWriter = new StreamWriter(fJson);
                fJsonWriter.Write(MessagePackSerializer.ConvertToJson(bytes));
            }
        }

        public override void Clear()
        {
            lock (_tables) _tables.Clear();
        }

        private void AddData(string table, string name, FieldData data)
        {
            lock (_tables)
            {
                if (!_tables.TryGetValue(table, out var t))
                    _tables.Add(table, t = new Dictionary<string, FieldData>());
                if (!t.TryAdd(name, data))
                {
                    Conf.Logger.Error($"error: repeated fields: {name}");
                }
            }
        }

        public override int RowHeadSize => 1;

        public GlobalTable(Config.ConfImporter conf) : base(conf) {}

        private class TableInst : ITableInst
        {
            public TableInst(GlobalTable dec, string name)
            {
                _dec = dec;
                _name = name;
            }

            public ITableInst.EFailOp BeginRow(int rowIdx, CancellationToken cancellationToken)
            {
                _field = null;
                return ITableInst.EFailOp.Success;
            }

            public ITableInst.EFailOp LoadContent(IReadOnlyList<string> colHead, string content, int idx,
                CancellationToken cancellationToken)
            {
                if (idx > 1 && _field == null) return ITableInst.EFailOp.BreakLine;
                switch (idx)
                {
                    case 1:
                    {
                        var name = content.Trim();
                        if (!StringUtil.IsProgramValidName(name)) return ITableInst.EFailOp.BreakLine;
                        if (name.StartsWith("__"))
                        {
                            _dec.Conf.Logger.Error($"error: field name cannot start with '__': {name}");
                            return ITableInst.EFailOp.BreakLine;
                        }
                        _field = name;
                        return ITableInst.EFailOp.Success;
                    }
                    case 2:
                    {
                        if (content.ToLower().Contains("client"))
                            return ITableInst.EFailOp.Success;
                        _field = null;
                        return ITableInst.EFailOp.BreakLine;
                    }
                    case 3:
                    {
                        try
                        {
                            _type = Analyzer.ToType(content);
                        }
                        catch (Exception e)
                        {
                            _field = null;
                            _dec.Conf.Logger.Error(e.Message);
                            return ITableInst.EFailOp.BreakLine;
                        }
                        return ITableInst.EFailOp.Success;
                    }
                    case 4:
                    {
                        var ctn = content.AsSpan();
                        var group = Analyzer.EatValueGroups(ref ctn, _type);
                        if (!StringUtil.IsEmptyOrWhiteSpace(ctn))
                        {
                            _field = null;
                            _dec.Conf.Logger.Error($"fail to parse field content, table: {_name}, left: {ctn.ToString()}");
                            return ITableInst.EFailOp.BreakLine;
                        }
                        if (_field == null) return ITableInst.EFailOp.BreakLine;
                        CommonPostGen.AddType(_type, _dec.Conf);
                        _dec.AddData(_name, _field, new FieldData(Analyzer.ToObject(_type, group, _filter), _type));
                        return ITableInst.EFailOp.Success;
                    }
                    default: return ITableInst.EFailOp.Success;
                }
            }

            private string? _field;
            private TypeInfo _type;

            private readonly string _name;
            private readonly GlobalTable _dec;
            private readonly HashSet<AnyBase> _filter = new();
        }

        private readonly struct FieldData
        {
            public readonly object? Data;
            public readonly TypeInfo Type;
            public FieldData(object? data, TypeInfo type)
            {
                Data = data;
                Type = type;
            }
        }
        private readonly Dictionary<string, Dictionary<string, FieldData>> _tables = new();
    }
}