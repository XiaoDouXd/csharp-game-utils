using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XD.Common.Config.Helper;

// ReSharper disable UnusedMember.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace XD.GameModule.Module.MConfig
{
    public static partial class CfgUtil
    {
        public static bool IsAsyncLoad { get; set; }
        public static GlobalTableCreateDelegate? GlobalTableCreateFunction { internal get; set; }
        public static CommonTableCreateDelegate? CommonTableCreateFunction { internal get; set; }
        public static Func<Task<byte[]>?>? GlobalTableReadFunction { internal get; set; }
        public static Func<Task<byte[]>?>? CommonTableReadFunction { internal get; set; }
        public static Func<Task<byte[]>?>? LocalizationTableReadFunction { internal get; set; }

        // ReSharper disable once ClassNeverInstantiated.Global
        public static class TableGroupConstructor<TItem>  where TItem : CfgHelper.CfgTableItemBase
        {
            public static CfgHelper.TableGroup<TItem> New(ref SerializedData data, IDeserializeMethod method, IDeserializeMethod.ConstructorFunc<TItem> constructor, string name, string[] fields)
            {
                var retConstructor = new CfgHelper.TableGroup<TItem>.ConstructData
                {
                    Name = name,
                    Fields = fields
                };
                var len = method.BeginTableGroupScope(ref data, typeof(TItem), name);
                switch (len)
                {
                    case < 0:
                        method.EndTableGroupScope(ref data, len);
                        return new CfgHelper.TableGroup<TItem>(retConstructor);
                    case 0:
                        var group = new CfgHelper.TableGroup<TItem>(retConstructor);
                        var defaultTable = NewTable(group, ref data, method, constructor);
                        group.SetDefault(defaultTable);
                        method.EndTableGroupScope(ref data, len);
                        return group;
                }

                var dict = new SortedDictionary<string, CfgHelper.Table<TItem>>();
                retConstructor.TableInstDict = dict;
                var retGroup = new CfgHelper.TableGroup<TItem>(retConstructor);
                CfgHelper.Table<TItem>? defaultId = null;
                for (var idx = 0; idx < len; idx++)
                {
                    var table = NewTable(retGroup, ref data, method, constructor);
                    if (idx == 0) defaultId = table;
                    dict.TryAdd(table.Id, table);
                }
                retGroup.SetDefault(defaultId);
                method.EndTableGroupScope(ref data, len);
                return retGroup;
            }

            private static CfgHelper.Table<TItem> NewTable(CfgHelper.TableGroup<TItem> owner, ref SerializedData data, IDeserializeMethod method, IDeserializeMethod.ConstructorFunc<TItem> constructor)
            {
                var retConstructor = new CfgHelper.Table<TItem>.ConstructData();
                var tableId = method.BeginTableScope(ref data, typeof(TItem), owner.Name);
                if (tableId == null)
                {
                    retConstructor.Id = string.Empty;
                    retConstructor.Items = Array.Empty<TItem>();
                    retConstructor.Dict = new CfgHelper.FakeDictionary<CfgHelper.Id, TItem>();
                    method.EndTableScope(ref data);
                    return new CfgHelper.Table<TItem>(owner, retConstructor);
                }

                var list = method.ReadArray(ref data, constructor);
                method.EndTableScope(ref data);
                if (list == null)
                {
                    retConstructor.Id = string.Empty;
                    retConstructor.Items = Array.Empty<TItem>();
                    retConstructor.Dict = new CfgHelper.FakeDictionary<CfgHelper.Id, TItem>();
                    return new CfgHelper.Table<TItem>(owner, retConstructor);
                }

                retConstructor.Id = tableId;
                var dict = new Dictionary<CfgHelper.Id, TItem>();
                foreach (var item in list) dict[item!.Id] = item;
                retConstructor.Items = list!;
                retConstructor.Dict = dict;
                return new CfgHelper.Table<TItem>(owner, retConstructor);
            }
        }
    }
}