using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using XD.Common.CollectionUtil;

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
        #region table
        public static bool IsAsyncLoad { get; set; }
        public static string? TablePath { get; set; }
        public static GlobalTableCreateDelegate? GlobalTableCreateFunction { internal get; set; }
        public static CommonTableCreateDelegate? CommonTableCreateFunction { internal get; set; }
        public static Func<Task<byte[]>?>? GlobalTableReadFunction { internal get; set; }
        public static Func<Task<byte[]>?>? CommonTableReadFunction { internal get; set; }
        public static Func<Task<byte[]>?>? LocalizationTableReadFunction { internal get; set; }

        public abstract class CfgTableItemBase : ICustomStruct
        {
            public abstract Id Id { get; }

            // ReSharper disable once InconsistentNaming
            public virtual string? __Name => null;
            public virtual bool __TryGet<T>(string name, out T value)
            {
                value = default!;
                return false;
            }
        }

        public interface ITable<TV> : IReadOnlyDictionary<Id, TV>
        {
            public string Id { get; }
            public TV[] ToArray();
        }

        public sealed class Table<TItem> : ITable<TItem> where TItem : CfgTableItemBase
        {
            public enum EIdType : byte { None, String, Number }

            #region dict implementations
            public int Count => Items.Count;
            public IEnumerable<Id> Keys => _dict.Keys;
            public IEnumerable<TItem> Values => Items;
            public TItem this[Id key] => _dict[key];
            public bool ContainsKey(Id key) => _dict.ContainsKey(key);
            public bool TryGetValue(Id key, out TItem value) => _dict.TryGetValue(key, out value);
            IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();
            public IEnumerator<KeyValuePair<Id, TItem>> GetEnumerator() => _dict.GetEnumerator();
            #endregion

            public string Id { get; }
            public Type Type => typeof(TItem);
            public string Name => Group.Name;
            public TItem[] ToArray() => Items.ToArray();
            public IReadOnlyList<TItem> Items { get; }
            public EIdType IdType => Items.Count > 0
                ? Items[0].Id.IsNumId
                    ? EIdType.Number
                    : Items[0].Id.IsNull ? EIdType.None : EIdType.String
                : EIdType.None;
            public TableGroup<TItem> Group { get; }

            public Table(TableGroup<TItem> owner)
            {
                Group = owner;
                Id = string.Empty;
                Items = Array.Empty<TItem>();
                _dict = new FakeDictionary<Id, TItem>();
            }

            public Table(TableGroup<TItem> owner, ref SerializedData data, IDeserializeMethod method, IDeserializeMethod.ConstructorFunc<TItem> constructor)
            {
                Group = owner;
                var tableId = method.BeginTableScope(ref data, typeof(TItem), owner.Name);
                if (tableId == null)
                {
                    Id = string.Empty;
                    Items = Array.Empty<TItem>();
                    _dict = new FakeDictionary<Id, TItem>();
                    method.EndTableScope(ref data);
                    return;
                }

                var list = method.ReadArray(ref data, constructor);
                method.EndTableScope(ref data);
                if (list == null)
                {
                    Id = string.Empty;
                    Items = Array.Empty<TItem>();
                    _dict = new FakeDictionary<Id, TItem>();
                    return;
                }

                Id = tableId;
                var dict = new Dictionary<Id, TItem>();
                foreach (var item in list) dict[item!.Id] = item;
                Items = list!;
                _dict = dict;
            }

            private readonly IReadOnlyDictionary<Id, TItem> _dict;
        }

        public abstract class TableGroup
        {
            public abstract Type Type { get; }
            public abstract string Name { get; }
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        public sealed class TableGroup<TItem> : TableGroup, IReadOnlyDictionary<string, Table<TItem>> where TItem : CfgTableItemBase
        {
            public Table<TItem> Default { get; }

            public override string Name { get; }
            public IReadOnlyList<string> Fields { get; }
            public override Type Type => typeof(TItem);

            public TableGroup(ref SerializedData data, IDeserializeMethod method, IDeserializeMethod.ConstructorFunc<TItem> constructor, string name, string[] fields)
            {
                Name = name;
                Fields = fields;
                var len = method.BeginTableGroupScope(ref data, typeof(TItem), name);
                switch (len)
                {
                    case < 0:
                        Default = new Table<TItem>(this);
                        method.EndTableGroupScope(ref data, len);
                        return;
                    case 0:
                        Default = new Table<TItem>(this, ref data, method, constructor);
                        method.EndTableGroupScope(ref data, len);
                        return;
                }

                _tableInstDict = new SortedDictionary<string, Table<TItem>>();
                for (var idx = 0; idx < len; idx++)
                {
                    var table = new Table<TItem>(this, ref data, method, constructor);
                    if (idx == 0) Default = table;
                    _tableInstDict.TryAdd(table.Id, table);
                }
                method.EndTableGroupScope(ref data, len);
                Default ??= _tableInstDict.First().Value;
            }

            public IEnumerator<KeyValuePair<string, Table<TItem>>> GetEnumerator()
            {
                if (_tableInstDict == null)
                {
                    yield return new KeyValuePair<string, Table<TItem>>(Default.Id, Default);
                    yield break;
                }
                foreach (var v in _tableInstDict)
                    yield return v;
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => _tableInstDict?.Count ?? 1;

            public bool ContainsKey(string key)
            {
                if (_tableInstDict == null) return Default.Id == key;
                return _tableInstDict.ContainsKey(key);
            }
            public bool TryGetValue(string key, out Table<TItem> value)
            {
                if (_tableInstDict != null) return _tableInstDict.TryGetValue(key, out value);
                if (Default.Id == key)
                {
                    value = Default;
                    return true;
                }
                value = null!;
                return false;
            }

            public Table<TItem> this[string key] => _tableInstDict == null ? Default : _tableInstDict[key];
            public IEnumerable<string> Keys => _tableInstDict == null
                ? new SingleEnumerable<string>(Default.Id)
                : _tableInstDict.Keys;
            public IEnumerable<Table<TItem>> Values => _tableInstDict == null
                ? new SingleEnumerable<Table<TItem>>()
                : _tableInstDict.Values;
            private readonly SortedDictionary<string, Table<TItem>>? _tableInstDict;
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedTypeParameter
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static class ____tableInfoCache<_> where _ : CfgTableItemBase { public static long Id = 0; }

        // ReSharper disable once UnusedType.Global
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once ClassNeverInstantiated.Global
        public class VERSION : CfgTableItemBase { public override Id Id => default; }
        #endregion

        #region custom struct
        public interface ICustomStruct
        {
            public bool __TryGet<T>(string name, out T value);
        }
        #endregion

        #region base value
        public readonly struct Id : IEquatable<Id>, IComparable<Id>
        {
            public readonly long Num;
            private readonly string? _str;

            #region Props
            public bool IsNull => _str == null;
            public bool IsNumId => _str == string.Empty;
            public string Str => _str == string.Empty ? Num.ToString() : _str ?? string.Empty;
            #endregion

            #region equals
            public bool Equals(Id other)
            {
                if (IsNull) return other.IsNull;

                if (IsNumId != other.IsNumId) return false;
                if (IsNumId) return Num == other.Num;
                return _str == other._str;
            }
            public override bool Equals(object? obj) => obj is Id other && Equals(other);
            public override int GetHashCode() => IsNumId ? Num.GetHashCode() : _str?.GetHashCode() ?? 0;
            public static bool operator ==(Id left, Id right) => left.Equals(right);
            public static bool operator !=(Id left, Id right) => !(left == right);
            #endregion

            #region compare
            public class IdComparer : IComparer<Id> { int IComparer<Id>.Compare(Id x, Id y) => Id.Compare(x, y); }
            public static int Compare(Id x, Id y)
            {
                // null -> number -> string
                // ReSharper disable ConvertIfStatementToSwitchStatement
                if (x.IsNull && y.IsNull) return 0;
                if (x.IsNull) return 1;
                if (y.IsNull) return -1;
                return x.IsNumId switch
                {
                    true when y.IsNumId => x.Num.CompareTo(y.Num),
                    true => -1,
                    _ => y.IsNumId ? 1 : string.Compare(x._str!, y._str!, StringComparison.Ordinal)
                };
                // ReSharper restore ConvertIfStatementToSwitchStatement
            }

            public int CompareTo(Id other) => Compare(other, this);
            public static bool operator <(Id left, Id right) => left.CompareTo(right) < 0;
            public static bool operator >(Id left, Id right) => left.CompareTo(right) > 0;
            public static bool operator <=(Id left, Id right) => left.CompareTo(right) <= 0;
            public static bool operator >=(Id left, Id right) => left.CompareTo(right) >= 0;
            #endregion

            #region create
            public Id(long num)
            {
                Num = num;
                _str = string.Empty;
            }

            public Id(string? str)
            {
                Num = 0;
                _str = str ?? string.Empty;
            }

            public override string ToString() => (string?)this ?? string.Empty;
            public static implicit operator long(Id id) => id.IsNull ? 0
                : id.IsNumId
                    ? id.Num
                    : long.TryParse(id._str ?? string.Empty, out var v) ? v : 0;
            public static implicit operator int(Id id) => id.IsNull ? 0
                : id.IsNumId
                    ? (int)id.Num
                    : int.TryParse(id._str ?? string.Empty, out var v) ? v : 0;
            public static implicit operator string?(Id id) =>
                id is { IsNull: false, IsNumId: true } ? id.Num.ToString() : id._str;
            public static implicit operator Id(int num) => new(num);
            public static implicit operator Id(long num) => new(num);
            public static implicit operator Id(string? str) => new(str);

            public static Id ____constructor(object? data) => new(data);
            private Id(object? data)
            {
                switch (data)
                {
                    case null:
                        Num = 0;
                        _str = null;
                        return;
                    case string s:
                        _str = s;
                        Num = 0;
                        return;
                    default:
                        _str = string.Empty;
                        Num = Convert.ToInt64(data);
                        return;
                }
            }
            #endregion
        }

        public readonly struct Link : IEquatable<Link>
        {
            public readonly long Num;
            public readonly string Str;
            public readonly string? Table;

            public Link(long item, string? table = null)
            {
                Table = table;
                Num = item;
                Str =item.ToString();
            }

            public Link(string? item, string? table = null)
            {
                Table = table;
                Str = item ?? "";
                Num = item == null ? 0 : long.TryParse(Str, out var v) ? v : 0;
            }

            public Id ToId(bool isNum = false) => isNum ? Num : Str;
            public override string ToString() => Str ?? "";
            public override int GetHashCode() => Str.GetHashCode();

            public bool Equals(Link other) => other.Str == Str;
            public override bool Equals([NotNullWhen(true)] object? obj) => obj is Link other && other.Equals(this);
            public static bool operator ==(Link left, Link right) => left.Equals(right);
            public static bool operator !=(Link left, Link right) => !(left == right);

            public static implicit operator Link(long id) => new(id);
            public static implicit operator Link(string? value) => new(value);
            public static implicit operator string(Link self) => self.Str;
            public static implicit operator long(Link self) => self.Num;
        }

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        public static string ToString(object? data) => Convert.ToString(data) ?? "";
        public static string? ToStringNullable(object? data) => data == null ? null : Convert.ToString(data);
        public static byte? ToByteNullable(object? data) => data == null ? null : Convert.ToByte(data);
        public static sbyte? ToSByteNullable(object? data) => data == null ? null : Convert.ToSByte(data);
        public static short? ToShortNullable(object? data) => data == null ? null : Convert.ToInt16(data);
        public static ushort? ToUShortNullable(object? data) => data == null ? null : Convert.ToUInt16(data);
        public static int? ToIntNullable(object? data) => data == null ? null : Convert.ToInt32(data);
        public static uint? ToUIntNullable(object? data) => data == null ? null : Convert.ToUInt32(data);
        public static long? ToLongNullable(object? data) => data == null ? null : Convert.ToInt64(data);
        public static ulong? ToULongNullable(object? data) => data == null ? null : Convert.ToUInt64(data);
        public static float? ToFloatNullable(object? data) => data == null ? null : Convert.ToSingle(data);
        public static double? ToDoubleNullable(object? data) => data == null ? null : Convert.ToDouble(data);
        public static bool? ToBooleanNullable(object? data) => data == null ? null : Convert.ToBoolean(data);

        public static T[]? ToList<T>(object? data, Func<object?, T> constructor)
        {
            if (data == null) return null;
            if (data is not object?[] { Length: > 0 } list) return Array.Empty<T>();
            var arr = new T[list.Length];
            for (var i = 0; i < list.Length; i++)
                arr[i] = constructor(list[i]);
            return arr;
        }

        public static IReadOnlyDictionary<TKey, TValue>? ToDict<TKey, TValue>(object? data, Func<object?, TKey> keyConstructor, Func<object?, TValue> valueConstructor)
            where TKey : notnull
        {
            if (data == null) return null;
            if (data is not object?[] { Length: > 1 } list) return new FakeDictionary<TKey, TValue>();
            var dict = new Dictionary<TKey, TValue>();
            for (var i = 0; i + 1 < list.Length; i += 2)
            {
                var k = keyConstructor(list[i]);
                var value = valueConstructor(list[i + 1]);
                dict.Add(k, value);
            }
            return dict;
        }

        public class FakeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
        {
            public TValue this[TKey key] { get => default!; set { } }
            public ICollection<TKey> Keys => Array.Empty<TKey>();
            public ICollection<TValue> Values => Array.Empty<TValue>();
            IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
            IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

            public int Count => 0;
            public bool IsReadOnly => true;

            public void Add(TKey key, TValue value) { }
            public void Add(KeyValuePair<TKey, TValue> item) { }
            public void Clear() { }

            public bool Contains(KeyValuePair<TKey, TValue> item) => false;
            public bool ContainsKey(TKey key) => false;

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { yield break; }

            public bool Remove(TKey key) => false;
            public bool Remove(KeyValuePair<TKey, TValue> item) => false;

            public bool TryGetValue(TKey key, out TValue value)
            {
                value = default!;
                return false;
            }
        }
        #endregion
    }
}