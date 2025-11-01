#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfImporter.Table;
using OfficeOpenXml;

// ReSharper disable once CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ConfImporter.Config
{
    public class Logger
    {
        public Action<string>? LogInfo { set; private get; }
        public Action<string>? LogError { set; private get; }
        public Action<string>? LogWarning { set; private get; }

        internal void Info(string s) => LogInfo?.Invoke(s);
        internal void Error(string s) => LogError?.Invoke(s);
        internal void Warning(string s) => LogWarning?.Invoke(s);
    }

    public class ConfImporter
    {
        public ConfImporter()
        {
            TableDec = new TableConfig(this);
            _sheets = new SheetEnumerator(this);
        }

        /// <summary>
        /// 当前状态进度
        /// </summary>
        public double Progress => _getProgress?.Invoke() ?? 0;

        /// <summary>
        /// 当前状态
        /// </summary>
        public string State { get; private set; } = string.Empty;

        /// <summary>
        /// Logger
        /// </summary>
        public Logger Logger { get; } = new();

        /// <summary>
        /// 配置表定义管理
        /// </summary>
        public TableConfig TableDec { get; }

        /// <summary>
        /// 数据导出目标
        /// </summary>
        public string ByteOutputTargetDir { get; set; } = "./";

        /// <summary>
        /// 代码导出目标
        /// </summary>
        public string CodeOutputTargetDir { get; set; } = "./";

        /// <summary>
        /// 名空间
        /// </summary>
        public string? CodeNamespace { get; set; } = null;

        /// <summary>
        /// 版本
        /// </summary>
        public long Version { get; set; } = 1;

        /// <summary>
        /// 收集配置表文件
        /// </summary>
        /// <param name="files"> 文件遍历器 </param>
        public async Task CollectFiles(IEnumerable<string> files)
        {
            if (_lock)
            {
                Logger.Error("importer is locked");
                return;
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                _lock = true;
                _isAnalyzed = false;
                _isFileCollected = false;

                await Task.Delay(0);
                _files.Clear();

                foreach (var f in files)
                {
                    if (string.IsNullOrEmpty(f)) continue;
                    if (!File.Exists(f)) continue;
                    _files.Add(new FileInfo(f));
                }

                _progress = 0;
                _getProgress = null;
                State = "Collecting files";

                foreach (var table in _tables.Values) table.Dispose();
                _tables.Clear();

                for (var i = 0; i < _files.Count; i++)
                {
                    _progress = (double)i / _files.Count;
                    var f = _files[i];
                    State = $"Collecting files: {f.FullName}";

                    if (_tables.ContainsKey(f.Name))
                    {
                        Logger.Warning($"try to collect table with same name: {f.Name}");
                        continue;
                    }

                    ExcelPackage? excelPkg;
                    try { excelPkg = new ExcelPackage(f); }
                    catch (Exception) { continue; }

                    var sheets = excelPkg.Workbook.Worksheets;
                    if (sheets == null) continue;

                    var sheetList = (from sheet in sheets
                        where sheet != null
                        from typeDec in TableDec.Type.TypeInfos
                        where typeDec != null
                        where typeDec.CheckSheetName(sheet.Name, f.Name, sheet)
                        let inst = typeDec.New(sheet.Name, f.Name, sheet)
                        where inst != null
                        select new Sheet(sheet, f.Name, typeDec, inst)).ToList();
                    if (sheetList.Count <= 0) continue;
                    _tables.Add(f.Name, new Table.Table(excelPkg, sheetList));
                }
            }
            catch (Exception)
            {
                State = string.Empty;
                _isFileCollected = true;
                _lock = false;
                throw;
            }

            State = string.Empty;
            _isFileCollected = true;
            _lock = false;
        }

        /// <summary>
        /// 分析配置表
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AnaTable()
        {
            if (_lock)
            {
                Logger.Error("importer is locked");
                return false;
            }

            var ret = true;
            _isAnalyzed = false;
            if (!_isFileCollected) return false;
            if (_tables.Count <= 0) return false;

            var cts = new CancellationTokenSource();
            try
            {
                _lock = true;
                await Task.Delay(0, cts.Token);

                foreach (var info in TableDec.Type.TypeInfos) info?.Clear();
                _getProgress = GetProgress;


                _tasks.Clear();
                State = "Analyze table";
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var table in _tables.Values)
                {
                    var task = Task.Run(() => table.LoadContent(cts.Token), cts.Token);
                    _tasks.Add(task);
                }

                _progress = 0;
                var isCompleted = false;
                while (!isCompleted)
                {
                    if (cts.IsCancellationRequested) goto EndPoint;

                    isCompleted = true;
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < _tasks.Count; i++)
                    {
                        var task = _tasks[i];
                        if (!task.IsCompleted)
                        {
                            isCompleted = false;
                            break;
                        }

                        if (task.Result != Sheet.EFailOp.BreakImporter) continue;
                        Logger.Error($"错误! task result: {task.Result}");
                        cts.Cancel();
                        ret = false;
                        goto EndPoint;
                    }
                    await Task.Delay(200, cts.Token);
                }

                _tasks.Clear();
                State = "Post-analyze table";
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var table in _tables.Values)
                {
                    var task = Task.Run(() => table.Analyze(cts.Token), cts.Token);
                    _tasks.Add(task);
                }

                _progress = 0.5;
                isCompleted = false;
                while (!isCompleted)
                {
                    if (cts.IsCancellationRequested) goto EndPoint;

                    isCompleted = true;
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < _tasks.Count; i++)
                    {
                        var task = _tasks[i];
                        if (!task.IsCompleted)
                        {
                            isCompleted = false;
                            break;
                        }

                        if (task.Result != Sheet.EFailOp.BreakImporter) continue;
                        Logger.Error($"错误! task result: {task.Result}");
                        cts.Cancel();
                        ret = false;
                        goto EndPoint;
                    }
                    await Task.Delay(200, cts.Token);
                }
                _tasks.Clear();
            }
            catch (Exception)
            {
                State = string.Empty;
                _isAnalyzed = true;
                _lock = false;
                if (!cts.IsCancellationRequested) cts.Cancel();
                throw;
            }

            EndPoint:
            State = string.Empty;
            _isAnalyzed = true;
            _lock = false;
            return ret;

            double GetProgress() => _tables.Values.Sum(v => v.Processing) * 0.5 / _tables.Count + _progress;
        }

        /// <summary>
        /// 生成 Byte
        /// </summary>
        public async Task GenByte()
        {
            if (!_isAnalyzed) return;
            if (_lock)
            {
                Logger.Error("importer is locked");
                return;
            }
            _lock = true;

            try
            {
                await Task.Delay(0);

                _progress = 0;
                _getProgress = null;
                State = "Gen byte";

                for (var i = 0; i < TableDec.Type.TypeInfos.Count; i++)
                {
                    _progress = (double)(i + _tables.Count) / (_tables.Count + TableDec.Type.TypeInfos.Count);
                    var t = TableDec.Type.TypeInfos[i];
                    t?.GenBytes(CancellationToken.None);
                }

                TableDec.Type.PostGenByte?.Invoke(this, CancellationToken.None, _sheets);
            }
            catch (Exception)
            {
                State = string.Empty;
                _lock = false;
                throw;
            }

            State = string.Empty;
            _lock = false;
        }

        /// <summary>
        /// 生成脚本
        /// </summary>
        public async Task GenCode()
        {
            if (!_isAnalyzed) return;
            if (_lock)
            {
                Logger.Error("importer is locked");
                return;
            }
            _lock = true;

            try
            {
                await Task.Delay(0);

                _progress = 0;
                _getProgress = null;
                State = "Gen code";

                for (var i = 0; i < TableDec.Type.TypeInfos.Count; i++)
                {
                    _progress = (double)(i + _tables.Count) / (_tables.Count + TableDec.Type.TypeInfos.Count);
                    var t = TableDec.Type.TypeInfos[i];
                    t?.GenCodes(CancellationToken.None);
                }

                TableDec.Type.PostGenCode?.Invoke(this, CancellationToken.None, _sheets);
            }
            catch (Exception)
            {
                State = string.Empty;
                _lock = false;
                throw;
            }

            State = string.Empty;
            _lock = false;
        }

        /// <summary>
        /// 清理收集信息
        /// </summary>
        public void Clear()
        {
            if (_lock)
            {
                Logger.Error("importer is locked");
                return;
            }
            _lock = true;

            try
            {
                _progress = 0;
                _getProgress = null;
                State = string.Empty;

                foreach (var table in _tables.Values) table.Dispose();
                _tasks.Clear();
                _files.Clear();
                _tables.Clear();
                TableDec.Type.OnClear?.Invoke(this);
            }
            catch (Exception)
            {
                _lock = false;
                throw;
            }
            _lock = false;
        }

        private double _progress;
        private Func<double>? _getProgress;

        private bool _lock;

        private bool _isAnalyzed;
        private bool _isFileCollected;

        private readonly SheetEnumerator _sheets;
        private readonly List<FileInfo> _files = new();
        private readonly SortedDictionary<string, Table.Table> _tables = new();
        private readonly List<Task<Sheet.EFailOp>> _tasks = new();

        private class SheetEnumerator : IEnumerable<TableTypeDec.ITableInst>
        {
            private readonly ConfImporter _self;

            public SheetEnumerator(ConfImporter self) => _self = self;

            public IEnumerator<TableTypeDec.ITableInst> GetEnumerator()
            {
                // ReSharper disable ForCanBeConvertedToForeach
                foreach (var table in _self._tables.Values )
                {
                    for (var j = 0; j < table.Sheets.Count; j++)
                        yield return table.Sheets[j].Inst;
                }
                // ReSharper restore ForCanBeConvertedToForeach
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}