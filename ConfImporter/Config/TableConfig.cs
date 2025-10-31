#region ReSharper disable

// ReSharper disable ArrangeNamespaceBody
// ReSharper disable VirtualMemberNeverOverridden.Global

#endregion

#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using ConfImporter.Builtin;
using ConfImporter.Table;

// ReSharper disable once CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace ConfImporter.Config
{
    public abstract class TableTypeDec
    {
        public ConfImporter Conf { get; }

        protected TableTypeDec(ConfImporter conf) => Conf = conf;

        #region head check

        /// <summary>
        /// 表名检查
        /// </summary>
        /// <param name="sheetName"> 工作簿名 </param>
        /// <param name="fileName"> 文件名 </param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public abstract bool CheckSheetName(string sheetName, string fileName, in SheetReader reader);

        /// <summary>
        /// 行首
        /// </summary>
        public virtual int ColHeadSize => 0;

        /// <summary>
        /// 表头
        /// </summary>
        public virtual int RowHeadSize => 0;

        /// <summary>
        /// 全局生成 Byte
        /// </summary>
        /// <param name="c"></param>
        public virtual void GenBytes(CancellationToken c) {}

        /// <summary>
        /// 全局生成代码
        /// </summary>
        /// <param name="c"></param>
        public virtual void GenCodes(CancellationToken c) {}

        #endregion

        #region content analizer

        public interface ITableInst : IDisposable
        {
            public enum EFailOp
            {
                // ReSharper disable UnusedMember.Global
                /// 成功
                Success,
                /// 跳过分析该行
                BreakLine,
                /// 跳过分析该表
                BreakTable,
                /// 跳过该次分析 (逻辑上等同于中断)
                BreakAnalyze,
                /// 错误中断
                Cancel
                // ReSharper restore UnusedMember.Global
            }

            #region Global
            /// <summary>
            /// 开始解析表
            /// </summary>
            /// <param name="sheetName"> 工作簿名 </param>
            /// <param name="fileName"> 文件名 </param>
            /// <param name="c"></param>
            /// <returns></returns>
            public EFailOp Begin(string sheetName, string fileName, CancellationToken c) => EFailOp.Success;

            /// <summary>
            /// 结束解析表
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            public EFailOp End(CancellationToken c) => EFailOp.Success;

            /// <summary>
            /// 解析表内容
            /// 在所有工作簿成功加载后 (this.End 后) 调用
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            public EFailOp Analyze(CancellationToken c) => EFailOp.Success;
            #endregion

            #region Head
            /// <summary>
            /// 加载表头
            /// </summary>
            /// <param name="rowHead"> 一行内容 </param>
            /// <param name="rowIdx"> 表头索引 (行 id) (从 0 开始) </param>
            /// <param name="c"></param>
            /// <returns></returns>
            public EFailOp LoadRowHead(IReadOnlyList<string> rowHead, int rowIdx, CancellationToken c) => EFailOp.Success;

            /// <summary>
            /// 结束加载表头
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            public EFailOp EndLoadRowHead(CancellationToken c) => EFailOp.Success;
            #endregion

            #region Row

            /// <summary>
            /// 开始加载一行
            /// </summary>
            /// <param name="rowIdx"> 当前行索引 (从 0 开始) </param>
            /// <param name="c"></param>
            /// <returns></returns>
            public EFailOp BeginRow(int rowIdx, CancellationToken c) => EFailOp.Success;

            /// <summary>
            /// 加载内容项
            /// </summary>
            /// <param name="colHead"> 当前行项表头 (列) </param>
            /// <param name="content"> 当前项内容 </param>
            /// <param name="colIdx"> 当前列索引 (从 0 开始) </param>
            /// <param name="c"></param>
            /// <returns></returns>
            public EFailOp LoadContent(IReadOnlyList<string> colHead, string content, int colIdx, CancellationToken c)
                => EFailOp.Success;

            /// <summary>
            /// 结束加载一行
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            public EFailOp EndRow(CancellationToken c) => EFailOp.Success;
            #endregion

            /// <summary>
            /// 释放
            /// </summary>
            void IDisposable.Dispose() {}
        }

        /// <summary>
        /// 新建工作簿操作实例
        /// </summary>
        /// <param name="sheetName"> 工作簿名 </param>
        /// <param name="fileName"> 文件名 </param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public abstract ITableInst? New(string sheetName, string fileName, in SheetReader reader);

        /// <summary>
        /// 清理生成数据
        /// </summary>
        public virtual void Clear() {}

        #endregion
    }

    public class TableConfig
    {
        public TypeConf Type { get; }

        internal TableConfig(ConfImporter conf) => Type = new TypeConf(conf);

        public sealed class TypeConf
        {
            internal TypeConf(ConfImporter conf)
            {
                // 内建的默认表定义
                _typeInfos[29] = new CommonTable(conf);
                _typeInfos[30] = new GlobalTable(conf);
                _typeInfos[31] = new I18nTable(conf);

                OnClear += CommonPostGen.CommonClear;
                PostGenByte += CommonPostGen.CommonPostGenByte;
                PostGenCode += CommonPostGen.CommonPostGenCode;
            }

            /// <summary>
            /// 当前所有的配置表定义
            /// </summary>
            internal IReadOnlyList<TableTypeDec?> TypeInfos => _typeInfos;

            /// <summary>
            /// 生成 Byte 完毕后调用
            /// </summary>
            public Action<ConfImporter, CancellationToken, IEnumerable<TableTypeDec.ITableInst>>? PostGenByte { get; set; }

            /// <summary>
            /// 生成代码完毕后调用
            /// </summary>
            public Action<ConfImporter, CancellationToken, IEnumerable<TableTypeDec.ITableInst>>? PostGenCode { get; set; }

            /// <summary>
            /// 清理数据时调用
            /// </summary>
            public Action<ConfImporter>? OnClear { get; set; }

            /// <summary>
            /// 添加配置表定义
            /// </summary>
            /// <param name="config"> 配置表定义 </param>
            /// <param name="id"> 添加到的目标 id </param>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentNullException"></exception>
            public void Add(TableTypeDec config, byte id)
            {
                if (id >= 32)
                    throw new ArgumentException("id must less than 32", nameof(id));
                if (_typeInfos[id] != null)
                    throw new ArgumentException("repeated addition", nameof(config));
                _typeInfos[id] = config ?? throw new ArgumentNullException(nameof(config));
            }

            /// <summary>
            /// 清空所有配置表定义
            /// </summary>
            public void Clear()
            {
                for (var i = 0; i < _typeInfos.Length; i++)
                    _typeInfos[i] = null;
            }

            private readonly TableTypeDec?[] _typeInfos =
            {
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            };
        }
    }
}