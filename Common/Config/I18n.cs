using System.Collections.Generic;
using XD.Common.Config.Helper;

namespace XD.Common.Config
{
    // ReSharper disable MemberCanBePrivate.Global
    public class I18N
    {
        public static I18N Empty { get; } = new(null);

        public CfgHelper.ELocaleCode CurrentLocaleCode { get; set; } = CfgHelper.ELocaleCode.zh_cn;

        public I18N(IReadOnlyDictionary<CfgHelper.ELocaleCode, Dictionary<string, Dictionary<string, string>>>? data)
        {
            _i18NTable = data ??
                         new CfgHelper.FakeDictionary<CfgHelper.ELocaleCode,
                             Dictionary<string, Dictionary<string, string>>>();
        }

        public string Get(string table, string key) => GetInner(table, key) ?? $"I<{table}, {key}>";

        public string Get<T>(string table, string key, T obj1)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString());
        }

        public string Get<T1, T2>(string table, string key, T1 obj1, T2 obj2)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString());
        }

        public string Get<T1, T2, T3>(string table, string key, T1 obj1, T2 obj2, T3 obj3)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString());
        }

        public string Get<T1, T2, T3, T4>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5, T6>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5, T6, T7>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5, T6, T7, T8>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8, T9 obj9)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString(), obj9?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8, T9 obj9, T10 obj10)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString(), obj9?.ToString(), obj10?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8, T9 obj9, T10 obj10, T11 obj11)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString(), obj9?.ToString(), obj10?.ToString(), obj11?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8, T9 obj9, T10 obj10, T11 obj11, T12 obj12)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString(), obj9?.ToString(), obj10?.ToString(), obj11?.ToString(), obj12?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8, T9 obj9, T10 obj10, T11 obj11, T12 obj12, T13 obj13)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString(), obj9?.ToString(), obj10?.ToString(), obj11?.ToString(), obj12?.ToString(), obj13?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8, T9 obj9, T10 obj10, T11 obj11, T12 obj12, T13 obj13, T14 obj14)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString(), obj9?.ToString(), obj10?.ToString(), obj11?.ToString(), obj12?.ToString(), obj13?.ToString(), obj14?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8, T9 obj9, T10 obj10, T11 obj11, T12 obj12, T13 obj13, T14 obj14, T15 obj15)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString(), obj9?.ToString(), obj10?.ToString(), obj11?.ToString(), obj12?.ToString(), obj13?.ToString(), obj14?.ToString(), obj15?.ToString());
        }

        public string Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string table, string key, T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8, T9 obj9, T10 obj10, T11 obj11, T12 obj12, T13 obj13, T14 obj14, T15 obj15, T16 obj16)
        {
            var ret = GetInner(table, key);
            return ret == null ? $"I<{table}, {key}>" : string.Format(ret, obj1?.ToString(), obj2?.ToString(), obj3?.ToString(), obj4?.ToString(), obj5?.ToString(), obj6?.ToString(), obj7?.ToString(), obj8?.ToString(), obj9?.ToString(), obj10?.ToString(), obj11?.ToString(), obj12?.ToString(), obj13?.ToString(), obj14?.ToString(), obj15?.ToString(), obj16?.ToString());
        }

        private string? GetInner(string table, string key)
        {
            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(key)) return null;
            if (!_i18NTable.TryGetValue(CurrentLocaleCode, out var tables)) return null;
            return !tables.TryGetValue(table, out var t) ? null : t.GetValueOrDefault(key);
        }

        private readonly IReadOnlyDictionary<CfgHelper.ELocaleCode, Dictionary<string, Dictionary<string, string>>> _i18NTable;
        // ReSharper restore InconsistentNaming
    }
}

