#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using ConfImporter.Config;
using OfficeOpenXml;

// ReSharper disable once CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace ConfImporter.Table
{
     public readonly struct SheetReader : ISheetReader
     {
          public SheetReader(ExcelWorksheet sheet) => _sheet = sheet;
          public string Read(int row, int col)
          {
               var cell = _sheet?.Cells[row + 1, col + 1];
               if (cell == null) return string.Empty;
               return cell.Value?.ToString() ?? string.Empty;
          }
          public static implicit operator SheetReader(ExcelWorksheet sheet) => new(sheet);
          private readonly ExcelWorksheet? _sheet;
     }

     public interface ISheetReader
     {
          public string Read(int row, int col);
     }

     internal class Sheet : IDisposable
     {
          public enum EFailOp : byte
          {
               // ReSharper disable once PreferConcreteValueOverDefault
               Success = default,
               BreakImporter,
               BreakTable,
               Cancel
          }

          public TableTypeDec.ITableInst Inst => _inst;

          public double Processing { get; private set; }

          public Sheet(ExcelWorksheet sheet, string fileName, TableTypeDec type, TableTypeDec.ITableInst inst)
          {
               _typeDec = type;
               _fileName = fileName;

               _inst = inst;
               _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
          }

          public EFailOp LoadContent(CancellationToken cts)
          {
               { // begin
                    var op = _inst.Begin(_sheet.Name, _fileName, cts);
                    if (op == TableTypeDec.ITableInst.EFailOp.Cancel) return EFailOp.Cancel;
                    if (op == TableTypeDec.ITableInst.EFailOp.BreakTable) return EFailOp.BreakTable;
                    if (op == TableTypeDec.ITableInst.EFailOp.BreakAnalyze) return EFailOp.BreakImporter;
               }

               var list = new List<string>();

               var rows = _sheet.Rows;
               var cols = _sheet.Columns;
               var cells = _sheet.Cells;

               var iCnt = 0;
               var headRowCnt = 0;
               for (var i = _sheet.Dimension.Start.Row; i <= _sheet.Dimension.End.Row; i++)
               {
                    if (cts.IsCancellationRequested) return EFailOp.Cancel;

                    list.Clear();
                    var row = rows[i];
                    Processing = rows.EndRow - rows.StartRow > 0
                         ? (double)(i - rows.StartRow) / (rows.EndRow - rows.StartRow)
                         : 1;

                    if (row == null || row.Hidden) continue;
                    if (iCnt++ >= _typeDec.RowHeadSize)
                    {
                         var jCnt = 0;
                         var kCnt = 0;
                         var op = _inst.BeginRow(iCnt - headRowCnt - 1, cts);
                         if (op == TableTypeDec.ITableInst.EFailOp.Cancel) return EFailOp.Cancel;
                         if (op == TableTypeDec.ITableInst.EFailOp.BreakLine) continue;
                         if (op == TableTypeDec.ITableInst.EFailOp.BreakAnalyze) return EFailOp.BreakImporter;
                         if (op == TableTypeDec.ITableInst.EFailOp.BreakTable) return EFailOp.BreakTable;

                         for (var j = _sheet.Dimension.Start.Column; j <= _sheet.Dimension.End.Column; j++)
                         {
                              if (cts.IsCancellationRequested) return EFailOp.Cancel;
                              Processing += rows.EndRow - rows.StartRow > 0 && cols.EndColumn - cols.StartColumn > 0
                                   ? (double)(j - cols.StartColumn) / (rows.EndRow - rows.StartRow) *
                                     (cols.EndColumn - cols.StartColumn)
                                   : 0.0;

                              if (++jCnt <= _typeDec.ColHeadSize)
                              {
                                   list.Add(cells[i, j].Text);
                                   continue;
                              }
                              op = _inst.LoadContent(list, cells[i, j].Text, kCnt++, cts);
                              if (op == TableTypeDec.ITableInst.EFailOp.Cancel) return EFailOp.Cancel;
                              if (op == TableTypeDec.ITableInst.EFailOp.BreakLine) break;
                              if (op == TableTypeDec.ITableInst.EFailOp.BreakAnalyze) return EFailOp.BreakImporter;
                              if (op == TableTypeDec.ITableInst.EFailOp.BreakTable) return EFailOp.BreakTable;
                         }
                         op = _inst.EndRow(cts);
                         if (op == TableTypeDec.ITableInst.EFailOp.Cancel) return EFailOp.Cancel;
                         if (op == TableTypeDec.ITableInst.EFailOp.BreakAnalyze) return EFailOp.BreakImporter;
                         if (op == TableTypeDec.ITableInst.EFailOp.BreakTable) return EFailOp.BreakTable;
                         continue;
                    }

                    {
                         // ReSharper disable once LoopCanBeConvertedToQuery
                         for (var idx = cols.StartColumn; idx <= cols.EndColumn; idx++)
                         {
                              list.Add(cells[i, idx].Text);
                         }
                         var op = _inst.LoadRowHead(list, headRowCnt++, cts);
                         if (op == TableTypeDec.ITableInst.EFailOp.Cancel) return EFailOp.Cancel;
                         if (op == TableTypeDec.ITableInst.EFailOp.BreakLine) continue;
                         if (op == TableTypeDec.ITableInst.EFailOp.BreakAnalyze) return EFailOp.BreakImporter;
                         if (op == TableTypeDec.ITableInst.EFailOp.BreakTable) return EFailOp.BreakTable;
                         // ReSharper disable once InvertIf
                         if (_typeDec.RowHeadSize == headRowCnt)
                         {
                              op = _inst.EndLoadRowHead(cts);
                              if (op == TableTypeDec.ITableInst.EFailOp.Cancel) return EFailOp.Cancel;
                              if (op == TableTypeDec.ITableInst.EFailOp.BreakLine) continue;
                              if (op == TableTypeDec.ITableInst.EFailOp.BreakAnalyze) return EFailOp.BreakImporter;
                              if (op == TableTypeDec.ITableInst.EFailOp.BreakTable) return EFailOp.BreakTable;
                         }
                    }
               }

               { // end
                    var op = _inst.End(cts);
                    if (op == TableTypeDec.ITableInst.EFailOp.Cancel) return EFailOp.Cancel;
                    if (op == TableTypeDec.ITableInst.EFailOp.BreakTable) return EFailOp.BreakTable;
                    if (op == TableTypeDec.ITableInst.EFailOp.BreakAnalyze) return EFailOp.BreakImporter;
               }

               return EFailOp.Success;
          }

          public EFailOp Analyze(CancellationToken cts)
          {
               return _inst.Analyze(cts) switch
               {
                    TableTypeDec.ITableInst.EFailOp.BreakAnalyze => EFailOp.BreakImporter,
                    TableTypeDec.ITableInst.EFailOp.BreakTable => EFailOp.BreakTable,
                    TableTypeDec.ITableInst.EFailOp.Cancel => EFailOp.Cancel,
                    _ => EFailOp.Success
               };
          }

          public void Dispose()
          {
               _inst.Dispose();
               _sheet.Dispose();
          }

          private readonly string _fileName;
          private readonly TableTypeDec _typeDec;
          private readonly ExcelWorksheet _sheet;
          private readonly TableTypeDec.ITableInst _inst;
     }
}