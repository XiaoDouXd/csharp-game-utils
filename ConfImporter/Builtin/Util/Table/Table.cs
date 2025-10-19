#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OfficeOpenXml;

// ReSharper disable once CheckNamespace
// ReSharper disable MemberCanBePrivate.Global

namespace ConfImporter.Table
{
    internal class Table : IDisposable
    {
        public IReadOnlyList<Sheet> Sheets => _sheet;

        public double Processing => _sheet.Count <= 0
            ? 1
            : _analyzing
                ? _analyzeProcessing
                : _sheet.Sum(s => s.Processing) / _sheet.Count;

        public Table(ExcelPackage table, List<Sheet> sheets)
        {
            _sheet = sheets;
            _table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public Sheet.EFailOp LoadContent(CancellationToken cts)
        {
            _analyzing = false;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var sheet in _sheet)
            {
                var op = sheet.LoadContent(cts);
                if (op is Sheet.EFailOp.BreakImporter or Sheet.EFailOp.Cancel) return op;
            }
            return Sheet.EFailOp.Success;
        }

        public Sheet.EFailOp Analyze(CancellationToken cts)
        {
            _analyzing = true;
            _analyzeProcessing = 0;
            for (var i = 0; i < _sheet.Count; i++)
            {
                if (cts.IsCancellationRequested) return Sheet.EFailOp.Cancel;

                _analyzeProcessing = (double)i / _sheet.Count;
                var sheet = _sheet[i];
                var op = sheet.Analyze(cts);
                if (op is Sheet.EFailOp.BreakImporter or Sheet.EFailOp.Cancel) return op;
            }
            return Sheet.EFailOp.Success;
        }

        public void Dispose()
        {
            foreach (var sheet in _sheet)
                sheet.Dispose();
            _table.Dispose();
        }

        private bool _analyzing;
        private double _analyzeProcessing;

        private readonly List<Sheet> _sheet;
        private readonly ExcelPackage _table;
    }
}