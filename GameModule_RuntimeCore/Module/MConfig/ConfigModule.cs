using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using XD.Common.AsyncUtil;
using XD.Common.Log;
using XD.Common.Procedure;

namespace XD.GameModule.Module.MConfig
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConfigModule : EngineModule
    {
        #region i18n table
        // ReSharper disable InconsistentNaming
        public string I18n(string table, string key) => I18nInner(table, key) ?? $"InvalidI18n<{table}, {key}>";

        public string I18n<T>(string table, string key, T obj1)
        {
            var ret = I18nInner(table, key);
            return ret == null ? $"InvalidI18n<{table}, {key}>" : string.Format(ret, obj1?.ToString());
        }

        public string I18n<T1, T2>(string table, string key, T1 obj1, T2 obj2)
        {
            var ret = I18nInner(table, key);
            return ret == null ? $"InvalidI18n<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString());
        }

        public string I18n<T1, T2, T3>(string table, string key, T1 obj1, T2 obj2, T3 obj3)
        {
            var ret = I18nInner(table, key);
            return ret == null ? $"InvalidI18n<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString());
        }

        public string I18n<T1, T2, T3, T4>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4)
        {
            var ret = I18nInner(table, key);
            return ret == null ? $"InvalidI18n<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString());
        }

        public string I18n<T1, T2, T3, T4, T5>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5)
        {
            var ret = I18nInner(table, key);
            return ret == null ? $"InvalidI18n<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString());
        }

        public string I18n<T1, T2, T3, T4, T5, T6>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6)
        {
            var ret = I18nInner(table, key);
            return ret == null ? $"InvalidI18n<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString());
        }

        public string I18n<T1, T2, T3, T4, T5, T6, T7>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7)
        {
            var ret = I18nInner(table, key);
            return ret == null ? $"InvalidI18n<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString());
        }

        public string I18n<T1, T2, T3, T4, T5, T6, T7, T8>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8)
        {
            var ret = I18nInner(table, key);
            return ret == null ? $"InvalidI18n<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString());
        }

        public string I18n<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8, T9 obj9)
        {
            var ret = I18nInner(table, key);
            return ret == null ? $"InvalidI18n<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString(), obj9?.ToString());
        }

        public string I18n<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8, T9 obj9, T10 obj10)
        {
            var ret = I18nInner(table, key);
            return ret == null ? $"InvalidI18n<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString(), obj9?.ToString(), obj10?.ToString());
        }

        private string? I18nInner(string table, string key)
        {
            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(key)) return null;
            if (!_i18nTable.TryGetValue(_currentLocaleCode, out var tables)) return null;
            return !tables.TryGetValue(table, out var t) ? null : t.GetValueOrDefault(key);
        }

        // ReSharper disable once ConvertToConstant.Local
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private CfgUtil.ELocaleCode _currentLocaleCode = CfgUtil.ELocaleCode.zh_cn;
        private readonly Dictionary<CfgUtil.ELocaleCode, Dictionary<string, Dictionary<string, string>>> _i18nTable = new();
        // ReSharper restore InconsistentNaming
        #endregion

        #region common config table
        public bool Contains<T>() where T : CfgUtil.CfgTableItemBase
        {
            if (_tableList == null) return false;
            var id = CfgUtil.____tableInfoCache<T>.Id;
            if (id <= 0 || id - 1 >= _tableList.Length) return false;
            return _tableList[id - 1] != null;
        }

        public bool Contains<T>(CfgUtil.Id id) where T : CfgUtil.CfgTableItemBase
        {
            if (_tableList == null) return false;
            var tableId = CfgUtil.____tableInfoCache<T>.Id;
            if (tableId <= 0  || tableId - 1 >= _tableList.Length) return false;
            return _tableList[tableId - 1] is CfgUtil.TableGroup<T> table && table.Default.ContainsKey(id);
        }

        public CfgUtil.TableGroup<T>? GetGroup<T>() where T : CfgUtil.CfgTableItemBase
        {
            if (_tableList == null) return null;
            var id = CfgUtil.____tableInfoCache<T>.Id;
            if (id <= 0 || id - 1 >= _tableList.Length) return null;
            return _tableList[id - 1] as CfgUtil.TableGroup<T>;
        }

        public bool TryGetGroup<T>(out CfgUtil.TableGroup<T>? value) where T : CfgUtil.CfgTableItemBase
        {
            if (_tableList == null)
            {
                value = null;
                return false;
            }

            var id = CfgUtil.____tableInfoCache<T>.Id;
            if (id > 0 && id - 1 < _tableList.Length)
                return (value = _tableList[id - 1] as CfgUtil.TableGroup<T>) != null;
            value = null;
            return false;
        }

        public CfgUtil.Table<T>? Get<T>() where T : CfgUtil.CfgTableItemBase
        {
            if (_tableList == null) return null;
            var id = CfgUtil.____tableInfoCache<T>.Id;
            if (id <= 0 || id - 1 >= _tableList.Length) return null;
            return (_tableList[id - 1] as CfgUtil.TableGroup<T>)?.Default;
        }

        public bool TryGet<T>(out CfgUtil.Table<T>? value) where T : CfgUtil.CfgTableItemBase
        {
            if (_tableList == null)
            {
                value = null;
                return false;
            }

            var id = CfgUtil.____tableInfoCache<T>.Id;
            if (id <= 0 || id - 1 >= _tableList.Length)
            {
                value = null;
                return false;
            }
            var group = _tableList[id - 1] as CfgUtil.TableGroup<T>;
            value = group?.Default;
            return value != null;
        }

        public T? Get<T>(CfgUtil.Id id) where T : CfgUtil.CfgTableItemBase
        {
            if (_tableList == null) return null;
            var tableId = CfgUtil.____tableInfoCache<T>.Id;
            if (tableId <= 0  || tableId - 1 >= _tableList.Length) return null;
            if (_tableList[tableId - 1] is not CfgUtil.TableGroup<T> table || !table.Default.TryGetValue(id, out var value))
                return null;
            return value;
        }

        public bool TryGet<T>(CfgUtil.Id id, out T? value) where T : CfgUtil.CfgTableItemBase
        {
            if (_tableList == null)
            {
                value = null;
                return false;
            }
            var tableId = CfgUtil.____tableInfoCache<T>.Id;
            if (tableId > 0 && tableId - 1 < _tableList.Length && _tableList[tableId - 1] is CfgUtil.TableGroup<T> table)
                return table.Default.TryGetValue(id, out value);
            value = null;
            return false;
        }

        private CfgUtil.TableGroup?[]? _tableList;
        #endregion

        #region init
        internal override IProcedure InitProcedure() => new ProcedureAsync(async token =>
        {
            try
            {
                Task<CfgUtil.GlobalTableCreateResult>? globalLoader = null;
                if (CfgUtil.GlobalTableCreateFunction != null && CfgUtil.GlobalTableReadFunction != null)
                {
                    globalLoader = Task.Run(async () =>
                    {
                        if (!CfgUtil.IsAsyncLoad)
                        {
                            var task = E.Tick?.Register(new TickTask());
                            if (task != null) await task;
                        }
                        using var h = CfgUtil.GlobalTableReadFunction();
                        return h != null
                            ? MessagePackSerializer.Deserialize<CfgUtil.GlobalTableCreateResult>(await h)
                            : default;
                    }, token);
                }

                Task? commonLoader = null;
                if (CfgUtil.CommonTableCreateFunction != null && CfgUtil.CommonTableReadFunction != null)
                {
                    commonLoader = Task.Run((Func<Task>)(async () =>
                    {
                        if (!CfgUtil.IsAsyncLoad)
                        {
                            var task = E.Tick?.Register(new TickTask());
                            if (task != null) await task;
                        }
                        using var h = CfgUtil.CommonTableReadFunction();
                        _tableList = h != null ? MessagePackSerializer.Deserialize<CfgUtil.CommonTableCreateResult>(await h).Value : null;
                    }), token);
                }
                else
                {
                    if (CfgUtil.CommonTableCreateFunction == null)
                    {
                        Log.Error("CommonTableCreateFunction is null.");
                    }
                    if (CfgUtil.CommonTableReadFunction == null)
                    {
                        Log.Error("CommonTableReadFunction is null.");
                    }
                }

                // ReSharper disable InconsistentNaming
                _i18nTable.Clear();
                Task? localizationLoader = null;
                if (CfgUtil.LocalizationTableReadFunction != null)
                {
                    localizationLoader = Task.Run(async () =>
                    {
                        if (!CfgUtil.IsAsyncLoad)
                        {
                            var task = E.Tick?.Register(new TickTask());
                            if (task != null) await task;
                        }
                        using var h = CfgUtil.LocalizationTableReadFunction();
                        var obj = h != null ? MessagePackSerializer.Typeless.Deserialize(await h) : null;
                        if (obj is object[] { Length: > 0 } i18nData)
                        {
                            foreach (var o in i18nData)
                            {
                                if (o is not object[] {Length: >= 3} i18nList) continue;
                                var code = Convert.ToUInt16(i18nList[0]);
                                if (code is <= 0 or >= (ushort)CfgUtil.ELocaleCode.__max) continue;
                                if (i18nList[1] is not string { Length: > 0 } tableName) continue;
                                if (i18nList[2] is not object[] { Length: >= 2 } i18nSrc) continue;

                                var eCode = (CfgUtil.ELocaleCode)code;
                                if (!_i18nTable.TryGetValue(eCode, out var lang))
                                    _i18nTable.Add(eCode, lang = new Dictionary<string, Dictionary<string, string>>());
                                if (!lang.TryGetValue(tableName, out var table))
                                    lang.Add(tableName, table = new Dictionary<string, string>());
                                for (var i = 0; i < i18nSrc.Length - 1; i += 2)
                                {
                                    if (i18nSrc[i] is not string sK || i18nSrc[i + 1] is not string sV) continue;
                                    table[sK] = sV;
                                }
                            }
                        }
                    }, token);
                }
                else
                {
                    Log.Error("CommonTableReadFunction is null.");
                }
                // ReSharper restore InconsistentNaming

                await Task.WhenAll(
                    commonLoader ?? Task.CompletedTask,
                    globalLoader ?? Task.CompletedTask,
                    localizationLoader ?? Task.CompletedTask
                );
            }
            catch (Exception e)
            {
                Log.Error($"conf module init failed: {e}");
                return new IProcedure.RetInfo(IProcedure.EndType.Abort, e);
            }
            return IProcedure.RetInfo.Success;
        });

        internal override IProcedure ReinitProcedure() => E.EmptyProcedure;
        internal override IProcedure DeInitProcedure() => E.EmptyProcedure;
        #endregion
    }
}