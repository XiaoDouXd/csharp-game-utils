using System;
using System.Collections.Generic;
using XD.Common.Config.Helper;

namespace XD.Common.Config
{
    public class Cfg
    {
        public static Cfg Empty { get; } = new(null);

        public Cfg(IReadOnlyDictionary<Type, CfgHelper.TableGroup>? data)
        {
            if (data == null)
            {
                _data = new CfgHelper.FakeDictionary<Type, CfgHelper.TableGroup>();
                return;
            }
            _data = data;
        }

        public bool Contains<T>() where T : CfgHelper.CfgTableItemBase => _data.ContainsKey(typeof(T));
        public bool Contains<T>(CfgHelper.Id id) where T : CfgHelper.CfgTableItemBase =>
            _data.TryGetValue(typeof(T), out var table) && table is CfgHelper.TableGroup<T> tableGroup &&
            tableGroup.Default.ContainsKey(id);

        public CfgHelper.TableGroup<T>? GetGroup<T>() where T : CfgHelper.CfgTableItemBase
        {
            if (!_data.TryGetValue(typeof(T), out var table)) return null;
            return table as CfgHelper.TableGroup<T>;
        }
        public bool TryGetGroup<T>(out CfgHelper.TableGroup<T>? value) where T : CfgHelper.CfgTableItemBase
        {
            value = null;
            if (!_data.TryGetValue(typeof(T), out var table)) return false;
            if (table is not CfgHelper.TableGroup<T> tableGroup) return false;
            value = tableGroup;
            return true;
        }

        public CfgHelper.Table<T>? Get<T>() where T : CfgHelper.CfgTableItemBase
            => !_data.TryGetValue(typeof(T), out var table) ? null : (table as CfgHelper.TableGroup<T>)?.Default;
        public bool TryGet<T>(out CfgHelper.Table<T>? value) where T : CfgHelper.CfgTableItemBase
        {
            value = null;
            if (!_data.TryGetValue(typeof(T), out var table)) return false;
            if (table is not CfgHelper.TableGroup<T> tableGroup) return false;
            value = tableGroup.Default;
            return true;
        }

        public T? Get<T>(CfgHelper.Id id) where T : CfgHelper.CfgTableItemBase
        {
            if (!_data.TryGetValue(typeof(T), out var table)) return null;
            return table is not CfgHelper.TableGroup<T> tableGroup ? null : tableGroup.Default.GetValueOrDefault(id);
        }
        public bool TryGet<T>(CfgHelper.Id id, out T? value) where T : CfgHelper.CfgTableItemBase
        {
            value = null;
            if (!_data.TryGetValue(typeof(T), out var table)) return false;
            return table is CfgHelper.TableGroup<T> tableGroup && tableGroup.Default.TryGetValue(id, out value);
        }

        private readonly IReadOnlyDictionary<Type, CfgHelper.TableGroup> _data;
    }
}