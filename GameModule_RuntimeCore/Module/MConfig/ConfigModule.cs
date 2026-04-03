using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using XD.Common.AsyncUtil;
using XD.Common.Config;
using XD.Common.Config.Helper;
using XD.Common.Log;
using XD.Common.Procedure;

namespace XD.GameModule.Module.MConfig
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConfigModule : EngineModule
    {
        public Cfg Cfg => _cfg ?? Cfg.Empty;
        public I18N I18N => _i18N ?? I18N.Empty;

        private Cfg? _cfg;
        private I18N? _i18N;

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
                            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
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
                            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                            var task = E.Tick?.Register(new TickTask());
                            if (task != null) await task;
                        }
                        using var h = CfgUtil.CommonTableReadFunction();
                        var tableList = h != null ? MessagePackSerializer.Deserialize<CfgUtil.CommonTableCreateResult>(await h).Value : null;
                        var dict = tableList?.ToDictionary(tableGroup => tableGroup.Type);
                        _cfg = new Cfg(dict);
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
                Task? localizationLoader = null;
                if (CfgUtil.LocalizationTableReadFunction != null)
                {
                    localizationLoader = Task.Run(async () =>
                    {
                        if (!CfgUtil.IsAsyncLoad)
                        {
                            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                            var task = E.Tick?.Register(new TickTask());
                            if (task != null) await task;
                        }
                        using var h = CfgUtil.LocalizationTableReadFunction();
                        var i18nTable =
                            new Dictionary<CfgHelper.ELocaleCode, Dictionary<string, Dictionary<string, string>>>();
                        var obj = h != null ? MessagePackSerializer.Typeless.Deserialize(await h) : null;
                        if (obj is object[] { Length: > 0 } i18nData)
                        {
                            foreach (var o in i18nData)
                            {
                                if (o is not object[] {Length: >= 3} i18nList) continue;
                                var code = Convert.ToUInt16(i18nList[0]);
                                if (code is <= 0 or >= (ushort)CfgHelper.ELocaleCode.__max) continue;
                                if (i18nList[1] is not string { Length: > 0 } tableName) continue;
                                if (i18nList[2] is not object[] { Length: >= 2 } i18nSrc) continue;

                                var eCode = (CfgHelper.ELocaleCode)code;
                                if (!i18nTable.TryGetValue(eCode, out var lang))
                                    i18nTable.Add(eCode, lang = new Dictionary<string, Dictionary<string, string>>());
                                if (!lang.TryGetValue(tableName, out var table))
                                    lang.Add(tableName, table = new Dictionary<string, string>());
                                for (var i = 0; i < i18nSrc.Length - 1; i += 2)
                                {
                                    if (i18nSrc[i] is not string sK || i18nSrc[i + 1] is not string sV) continue;
                                    table[sK] = sV;
                                }
                            }
                        }
                        _i18N = new I18N(i18nTable);
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