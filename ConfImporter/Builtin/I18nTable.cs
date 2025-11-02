#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ConfImporter.Builtin.Util;
using ConfImporter.Builtin.Util.Gen;
using ConfImporter.Config;
using ConfImporter.Table;
using MessagePack;

#region ReSharper disable

// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeNamespaceBody

#endregion

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin
{
    public class I18nTable : TableTypeDec
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

            var programName = StringUtil.MatchProgramValidName(ref sheetNameSpan);
            return !programName.IsEmpty && fileName.ToLower().StartsWith("i18n-");
        }

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
                    using var fEmpty = File.Create(Conf.ByteOutputTargetDir + "/I18nTable.bytes");
                    using var fWriterEmpty = new StreamWriter(fEmpty);
                    fWriterEmpty.Write("");
                    return;
                }

                var objList = new List<object>();
                foreach (var (lang, tables) in _tables)
                {
                    if (tables.Count <= 0) continue;
                    var tableList = new List<object> { lang };
                    foreach (var table in tables.Values)
                    {
                        var sentences = new List<object>();
                        foreach (var (k, v) in table.Data)
                        {
                            sentences.Add(k);
                            sentences.Add(v);
                        }
                        tableList.Add(table.Name);
                        tableList.Add(sentences);
                    }
                    objList.Add(tableList);
                }
                var bytes = MessagePackSerializer.Serialize(objList);
                using var f = File.Create(Conf.ByteOutputTargetDir + "/I18nTable.bytes");
                f.Write(bytes);
                using var fJson = File.Create(Conf.ByteOutputTargetDir + "/I18nTable.json");
                using var fJsonWriter = new StreamWriter(fJson);
                fJsonWriter.Write(MessagePackSerializer.ConvertToJson(bytes));
            }
        }

        public override void Clear()
        {
            lock (_tables) _tables.Clear();
        }

        private void AddSentence(string table, string  key, string value, ELocaleCode language)
        {
            if (string.IsNullOrWhiteSpace(table) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                return;
            lock (_tables)
            {
                if (!_tables.TryGetValue(language, out var t))
                    _tables.Add(language, t = new Dictionary<string, TableData>());
                if (!t.TryGetValue(table, out var d))
                    t.Add(table, d = new TableData(table));
                d.Data[key] = value;
            }
        }

        public I18nTable(Config.ConfImporter conf) : base(conf) {}

        public override int RowHeadSize => 1;

        private class TableInst : ITableInst
        {
            public TableInst(I18nTable dec, string name)
            {
                _dec = dec;
                _name = name;
            }

            public ITableInst.EFailOp LoadRowHead(IReadOnlyList<string> rowHead, int rowIdx,
                CancellationToken cancellationToken)
            {
                for (var i = 0; i < rowHead.Count; i++)
                {
                    var head = rowHead[i];
                    if (head.Length > 10) continue;

                    head = head.Replace('-', '_').ToLower();
                    if (head == "key")
                    {
                        _keyIdx = i;
                        continue;
                    }

                    if (!Enum.TryParse<ELocaleCode>(head, out var language))
                        continue;
                    _languages[i] = language;
                }

                if (_keyIdx < 0 || _languages.Count <= 0)
                    return ITableInst.EFailOp.BreakTable;
                return ITableInst.EFailOp.Success;
            }

            public ITableInst.EFailOp BeginRow(int rowIdx, CancellationToken cancellationToken)
            {
                _curKey = null;
                _curValue.Clear();
                return ITableInst.EFailOp.Success;
            }

            public ITableInst.EFailOp LoadContent(IReadOnlyList<string> colHead, string content, int idx,
                CancellationToken cancellationToken)
            {
                if (idx == _keyIdx) _curKey = content;
                else if (_languages.ContainsKey(idx)) _curValue[idx] = content;
                return ITableInst.EFailOp.Success;
            }

            public ITableInst.EFailOp EndRow(CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(_curKey) || _curValue.Count <= 0)
                    return ITableInst.EFailOp.BreakLine;
                foreach (var (k, v) in _curValue)
                {
                    var lang = _languages[k];
                    _dec.AddSentence(_name, _curKey!, v, lang);
                }
                return ITableInst.EFailOp.Success;
            }

            private readonly I18nTable _dec;

            private string? _curKey;
            private readonly Dictionary<int, string> _curValue = new();

            private int _keyIdx = -1;
            private readonly string _name;
            private readonly SortedDictionary<int, ELocaleCode> _languages = new();
        }

        private readonly struct TableData
        {
            public readonly string Name;
            public readonly Dictionary<string, string> Data;

            public TableData(string name)
            {
                Name = name;
                Data = new Dictionary<string, string>();
            }
        }
        private readonly Dictionary<ELocaleCode, Dictionary<string, TableData>> _tables = new ();
    }
}