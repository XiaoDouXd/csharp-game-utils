#nullable enable

using System;
using System.Collections.Generic;
using ConfImporter.Builtin.Type;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin.Util
{
    internal static partial class Analyzer
    {
        public const char EscapingChar = '\\';

        #region Eat functions

        /// <summary>
        /// 根据 type 吞下 string
        /// </summary>
        /// <param name="str"> 列表字符串 </param>
        /// <param name="info"> 类型信息 </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ValueGroup[] EatValueGroups(ref ReadOnlySpan<char> str, in TypeInfo info)
        {
            if (info.Container == TypeInfo.EContainer.None) return new[] { EatValueGroup(ref str, in info) };
            if (str.IsWhiteSpace()) return Array.Empty<ValueGroup>();

            StringUtil.TrimHead(ref str);
            var valueGroupList = new List<ValueGroup>();

            // 循环吞下所有的列表元素
            while (str.Length > 0)
            {
                var keyAnyBase = new AnyBase();

                // 如果是 dictionary, 读取第一个值和 ':' 作为 key
                // 若没有冒号, 则其 key 为 default (此处不是 c# 语境下的 default, string 的 default 为 "")
                if (info.Container == TypeInfo.EContainer.Dictionary)
                {
                    var keyType = info.KeyType;
                    var newStr = str;
                    var s = EatBaseTypeValue(ref newStr, keyType);

                    StringUtil.TrimHead(ref newStr);
                    if (newStr.Length > 0 && IsDictKeySplitChar(newStr[0]))
                    {
                        var a = ToAnyBase(keyType, TypeInfo.ETypeFlag.None, s);
                        keyAnyBase = a;
                        newStr = newStr[1..];
                        str = newStr;
                    }
                    else keyAnyBase = GetDefaultBaseValue(keyType);
                }

                StringUtil.TrimHead(ref str);
                var v = EatValueGroup(ref str, info);
                v.Key = keyAnyBase;
                valueGroupList.Add(v);

                StringUtil.TrimHead(ref str);
                if (str.Length <= 0) continue;
                if (IsArrSplitChar(str[0]))
                {
                    str = str[1..];
                    StringUtil.TrimHead(ref str);
                    if (str.Length <= 0) valueGroupList.Add(new ValueGroup());
                }
                else return valueGroupList.ToArray();
            }
            return valueGroupList.ToArray();
        }

        /// <summary>
        /// 吞下自定义类型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static ValueGroup EatValueGroup(ref ReadOnlySpan<char> str, in TypeInfo info)
        {
            if (info.ValType != TypeInfo.EBaseType.Custom)
            {
                var type = info.ValType;
                var s = EatBaseTypeValue(ref str, type);
                var a = ToAnyBase(type, info.Flag, s);
                return new ValueGroup() { [0] = a };
            }
            if (info.FieldCount <= 0 || info.ValType == TypeInfo.EBaseType.Null) return new ValueGroup();

            var valGroup = new ValueGroup() { IsNull = (info.ValFlag & TypeInfo.ETypeFlag.Nullable) == 0 };
            var i = 0;
            var isInterrupt = false;
            for (; i < info.FieldCount; i++)
            {
                var (field, fieldIdx) = info.GetFieldByOrder(i);
                if (fieldIdx < 0) throw new ArgumentOutOfRangeException(nameof(info), "field count err");

                var type = field.Type;
                var flag = field.Flag;
                StringUtil.TrimHead(ref str);
                if (isInterrupt || str.Length <= 0 || IsArrSplitChar(str[0]) || IsDictKeySplitChar(str[0]))
                {
                    valGroup[i] = ToAnyBase(type, flag, ReadOnlySpan<char>.Empty);
                    continue;
                }

                if (IsCustomFieldSplitChar(str[0]))
                {
                    valGroup[i] = ToAnyBase(type, flag, ReadOnlySpan<char>.Empty);
                    valGroup.IsNull = false;
                    str = str[1..]; // 去除 ','
                    continue;
                }

                var ss = EatBaseTypeValue(ref str, type);
                StringUtil.TrimHead(ref str);
                valGroup[i] = ToAnyBase(type, flag, ss);
                valGroup.IsNull = false;

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (str.Length > 0 &&  IsCustomFieldSplitChar(str[0])) str = str[1..]; // 去除 ','
                else if (str.Length > 0) isInterrupt = true; // 中断 (如果字段中途失配则认为中断)
            }
            valGroup.ValueCount = Math.Min(Math.Max(i, 0), ValueGroup.MaxCount);
            return valGroup;
        }

        public static ReadOnlySpan<char> EatBaseTypeValue(ref ReadOnlySpan<char> str, TypeInfo.EBaseType type)
        {
            switch (type)
            {
                case TypeInfo.EBaseType.Bool: return EatBool(ref str);
                case TypeInfo.EBaseType.Int8:
                case TypeInfo.EBaseType.Int16:
                case TypeInfo.EBaseType.Int32:
                case TypeInfo.EBaseType.Int64:
                case TypeInfo.EBaseType.UInt8:
                case TypeInfo.EBaseType.UInt16:
                case TypeInfo.EBaseType.UInt32:
                case TypeInfo.EBaseType.UInt64:
                case TypeInfo.EBaseType.Float32:
                case TypeInfo.EBaseType.Float64: return EatNumber(ref str);
                case TypeInfo.EBaseType.Link: return EatLink(ref str);
                case TypeInfo.EBaseType.String: return EatString(ref str);
                case TypeInfo.EBaseType.Enum: return EatSymbol(ref str);
                case TypeInfo.EBaseType.Null:
                case TypeInfo.EBaseType.Custom:
                default: return ReadOnlySpan<char>.Empty;
            }
        }

        public static ReadOnlySpan<char> EatSymbol(ref ReadOnlySpan<char> str, bool acceptHeadNumber = false)
        {
            if (str.IsEmpty) return str;

            for (var i = 0; i < str.Length; i++)
            {
                if (!acceptHeadNumber && i == 0 && IsDigit(str[i])) return ReadOnlySpan<char>.Empty;
                if (IsLetterOrUnderLine(str[i]) || IsDigit(str[i])) continue;
                return StringUtil.Divide(ref str, i);
            }

            var slice = str;
            str = ReadOnlySpan<char>.Empty;
            return slice;
        }

        public static ReadOnlySpan<char> EatString(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty) return str;
            var isEscaping = false;
            var isSurrounded = IsStringSurroundedChar(str[0]);
            for (var i = isSurrounded ? 1 : 0; i < str.Length; i++)
            {
                var c = str[i];
                if (isEscaping)
                {
                    isEscaping = false;
                }
                else if (c == EscapingChar)
                {
                    isEscaping = true;
                }
                else
                {
                    switch (isSurrounded)
                    {
                        case true when IsStringSurroundedChar(c):
                            return StringUtil.Divide(ref str, i + 1)[1..^1];
                        case false when IsArrSplitChar(c) || IsDictKeySplitChar(c) || IsCustomFieldSplitChar(c):
                            return StringUtil.Divide(ref str, i);
                    }
                }
            }

            if (isSurrounded) return ReadOnlySpan<char>.Empty;
            var slice = str;
            str = ReadOnlySpan<char>.Empty;
            return slice;
        }

        public static ReadOnlySpan<char> EatBool(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty) return str;
            foreach (var keyword in BoolKeyWord)
            {
                var s = EatKeyword(ref str, keyword);
                if (!s.IsEmpty) return s;
            }
            return ReadOnlySpan<char>.Empty;
        }

        public static ReadOnlySpan<char> EatKeyword(ref ReadOnlySpan<char> str, string? keyword)
        {
            keyword = (keyword ?? string.Empty).Trim();
            if (str.Length < keyword.Length) return ReadOnlySpan<char>.Empty;

            var i = 0;
            for (; i < keyword.Length; i++)
            {
                var c = char.ToLower(str[i]);
                var cKeyword = keyword[i];
                if (c != cKeyword) return ReadOnlySpan<char>.Empty;
            }

            // ReSharper disable once InvertIf
            if (i < str.Length)
            {
                var c = str[i];
                if (StringUtil.IsDigit(c) || StringUtil.IsLetterOrUnderLine(c)) return ReadOnlySpan<char>.Empty;
            }
            return StringUtil.Divide(ref str, i);
        }

        public static ReadOnlySpan<char> EatNumber(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty) return str;

            var i = 0;
            var isDot = false;
            for (; i < str.Length; i++)
            {
                var c = str[i];
                switch (c)
                {
                    case >= '0' and <= '9':
                        continue;
                    case '.' when !isDot:
                        isDot = true;
                        continue;
                }
                break;
            }
            return StringUtil.Divide(ref str, i);
        }

        /// <summary>
        /// 吞下 id, id 的结构为 ITEM_INDEX-TABLE_INDEX, 其中 ITEM_INDEX 为表项主键, TABLE_INDEX 为表名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ReadOnlySpan<char> EatLink(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty) return str;
            var newContent = str;
            var item = EatSymbol(ref newContent, true);
            if (item.IsEmpty) return ReadOnlySpan<char>.Empty;
            StringUtil.TrimHead(ref newContent);
            if (newContent.Length <= 0 || newContent[0] != '-')
                return StringUtil.Divide(ref str, item.Length);

            newContent = newContent[1..];
            StringUtil.TrimHead(ref newContent);
            var table = EatSymbol(ref newContent, true);
            return table.IsEmpty
                ? StringUtil.Divide(ref str, item.Length)
                : StringUtil.Divide(ref str, str.Length - newContent.Length);
        }

        public static bool IsCustomFieldSplitChar(char c) => c is '，' or ',';

        public static bool IsArrSplitChar(char c) => c is '|';

        public static bool IsDictKeySplitChar(char c) => c is ':';

        public static bool IsStringSurroundedChar(char c) => c is '“' or '”' or '"';

        private static readonly string[] BoolKeyWord = { "1", "true", "t", "真", "0", "false", "f", "假" };

        #endregion

        #region Value Parse

        /// <summary>
        /// 转换 string 为 AnyBase
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flag"></param>
        /// <param name="fieldStr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static AnyBase ToAnyBase(TypeInfo.EBaseType type, TypeInfo.ETypeFlag flag, in ReadOnlySpan<char> fieldStr)
        {
            switch (type)
            {
                case TypeInfo.EBaseType.Bool:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(!fieldStr.IsWhiteSpace() && ToBool(fieldStr.ToString().ToLower()));
                case TypeInfo.EBaseType.Int8:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? 0 : ToI8(fieldStr.ToString()));
                case TypeInfo.EBaseType.Int16:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? 0 : ToI16(fieldStr.ToString()));
                case TypeInfo.EBaseType.Int32:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? 0 : ToI32(fieldStr.ToString()));
                case TypeInfo.EBaseType.Int64:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? 0 : ToI64(fieldStr.ToString()));
                case TypeInfo.EBaseType.UInt8:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? 0 : ToU8(fieldStr.ToString()));
                case TypeInfo.EBaseType.UInt16:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? 0 : ToU16(fieldStr.ToString()));
                case TypeInfo.EBaseType.UInt32:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? 0 : ToU32(fieldStr.ToString()));
                case TypeInfo.EBaseType.UInt64:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? 0 : ToU64(fieldStr.ToString()));
                case TypeInfo.EBaseType.Float32:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? 0 : ToF32(fieldStr.ToString()));
                case TypeInfo.EBaseType.Float64:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? 0 : ToF64(fieldStr.ToString()));
                case TypeInfo.EBaseType.String:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? "" : fieldStr.ToString());
                case TypeInfo.EBaseType.Link:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? "" : fieldStr.ToString());
                case TypeInfo.EBaseType.Enum:
                    if ((flag & TypeInfo.ETypeFlag.Nullable) != 0 && fieldStr.IsWhiteSpace())
                        return new AnyBase();
                    return new AnyBase(fieldStr.IsWhiteSpace() ? "" : fieldStr.ToString());
                case TypeInfo.EBaseType.Null:
                case TypeInfo.EBaseType.Custom:
                default: return new AnyBase();
            }
        }

        /// <summary>
        /// 获得默认值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static AnyBase GetDefaultBaseValue(TypeInfo.EBaseType type, TypeInfo.ETypeFlag flag = TypeInfo.ETypeFlag.None)
        {
            return type switch
            {
                // ReSharper disable RedundantCast
                TypeInfo.EBaseType.Bool => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase(false),
                TypeInfo.EBaseType.Int8 => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase((sbyte)0),
                TypeInfo.EBaseType.Int16 => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase((short)0),
                TypeInfo.EBaseType.Int32 => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase((int)0),
                TypeInfo.EBaseType.Int64 => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase((long)0),
                TypeInfo.EBaseType.UInt8 => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase((byte)0),
                TypeInfo.EBaseType.UInt16 => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase((ushort)0),
                TypeInfo.EBaseType.UInt32 => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase((uint)0),
                TypeInfo.EBaseType.UInt64 => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase((ulong)0),
                TypeInfo.EBaseType.Float32 => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase((float)0),
                TypeInfo.EBaseType.Float64 => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase((double)0),
                TypeInfo.EBaseType.String => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase(""),
                TypeInfo.EBaseType.Link => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase(""),
                TypeInfo.EBaseType.Enum => (flag & TypeInfo.ETypeFlag.Nullable) != 0 ? new AnyBase() : new AnyBase(""),
                // ReSharper restore RedundantCast
                _ => new AnyBase(),
            };
        }

        /// <summary>
        /// 转换为嵌套列表, 格式:
        /// <para>
        /// <para>
        ///     - 非容器形如以下结构, 结构体的值按字段定义位置排列, 基础类型直接为值:
        ///     [true, 4, string, ENUM_TYPE_A, ...] 或 5 或 2.4 或 ...
        /// </para>
        /// <para>
        ///     - 容器形如以下结构, 若为字典, 则每一项前一个元素为 key:
        ///     [0, [1, bool, string], 1, [2.4, false, null], 2, null, ...]
        /// </para>
        /// </para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="group"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static object? ToObject(in TypeInfo type, in ValueGroup[] group, HashSet<AnyBase>? filter = null)
        {
            if (group is not { Length: > 0 })
            {
                if (type.Container == TypeInfo.EContainer.None)
                    return (type.Flag & TypeInfo.ETypeFlag.Nullable) != 0 ? null : GetDefaultBaseValue(type.ValType, type.Flag);
                return (type.Flag & TypeInfo.ETypeFlag.Nullable) != 0 ? null : Array.Empty<object>();
            }

            switch (type.Container)
            {
                // 非容器
                case TypeInfo.EContainer.None:
                {
                    var item = group[0];
                    if (item.IsNull) return null;
                    if (item.ValueCount <= 0) // 基础类型
                        return ToBaseObject(type.ValType, item[0]);

                    // 复合类型
                    {
                        var arr = new object?[item.ValueCount];
                        for (var i = 0; i < item.ValueCount; i++)
                        {
                            var (field, fieldIdx) = type.GetFieldByOrder(i);
                            if (fieldIdx < 0 || fieldIdx > arr.Length)
                            {
                                arr[i] = null;
                                continue;
                            }
                            var value = item[i];
                            arr[fieldIdx] = ToBaseObject(field.Type, value);
                        }
                        return arr;
                    }
                }
                // 列表容器
                case TypeInfo.EContainer.Array:
                {
                    var ret = new object?[group.Length];
                    for (var j = 0; j < group.Length; j++)
                    {
                        var item = group[j];
                        if (!item.IsNull)
                        {
                            if (item.ValueCount <= 0) // 基础类型
                            {
                                ret[j] = ToBaseObject(type.ValType, item[0]);
                            }
                            else // 复合类型
                            {
                                var arr = new object?[item.ValueCount];
                                for (var i = 0; i < item.ValueCount; i++)
                                {
                                    var (field, fieldIdx) = type.GetFieldByOrder(i);
                                    if (fieldIdx < 0 || fieldIdx > arr.Length)
                                    {
                                        arr[i] = null;
                                        continue;
                                    }
                                    var value = item[i];
                                    arr[fieldIdx] = ToBaseObject(field.Type, value);
                                }
                                ret[j] = arr;
                            }
                        }
                        else ret[j] = null;
                    }
                    return ret;
                }
                // 字典容器
                case TypeInfo.EContainer.Dictionary:
                {
                    filter ??= new HashSet<AnyBase>();
                    filter.Clear();

                    var ret = new List<object?>();
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var j = 0; j < group.Length; j++)
                    {
                        var item = group[j];
                        var key = item.Key;
                        if (key.TypeCode == AnyBase.ETypeCode.Null || filter.Contains(item.Key)) continue;

                        if (!item.IsNull)
                        {
                            if (item.ValueCount <= 0) // 基础类型
                            {
                                ret.Add(ToBaseObject(type.KeyType, key));
                                ret.Add(ToBaseObject(type.ValType, item[0]));
                            }
                            else // 复合类型
                            {
                                var arr = new object?[item.ValueCount];
                                for (var i = 0; i < item.ValueCount; i++)
                                {
                                    var (field, fieldIdx) = type.GetFieldByOrder(i);
                                    if (fieldIdx < 0 || fieldIdx > arr.Length)
                                    {
                                        arr[i] = null;
                                        continue;
                                    }
                                    var value = item[i];
                                    arr[fieldIdx] = ToBaseObject(field.Type, value);
                                }
                                ret.Add(ToBaseObject(type.KeyType, key));
                                ret.Add(arr);
                            }
                        }
                        else
                        {
                            ret.Add(ToBaseObject(type.KeyType, key));
                            ret.Add(null);
                        }
                        filter.Add(item.Key);
                    }

                    filter.Clear();
                    return ret.ToArray();
                }
                default: return null;
            }

            static object? ToBaseObject(TypeInfo.EBaseType type, in AnyBase any)
            {
                // id 类型特殊处理
                if (type != TypeInfo.EBaseType.Link) return any.ToObject();
                if (any.TypeCode == AnyBase.ETypeCode.Null) return null;
                var str = (string?)any;
                if (str == null) return null;

                var strArr = str.Split('-');
                if (strArr is not { Length: > 0 }) return new[] { "", "" };
                return strArr.Length < 2 ? new[] { strArr[0].Trim(), "" } : new[] { strArr[0].Trim(), strArr[1].Trim() };
            }
        }

        public struct ValueGroup
        {
            public const int MaxCount = 16;

            public bool IsNull;
            public AnyBase Key;
            public int ValueCount;

            private AnyBase _v00;
            private AnyBase _v01;
            private AnyBase _v02;
            private AnyBase _v03;
            private AnyBase _v04;
            private AnyBase _v05;
            private AnyBase _v06;
            private AnyBase _v07;
            private AnyBase _v08;
            private AnyBase _v09;
            private AnyBase _v10;
            private AnyBase _v11;
            private AnyBase _v12;
            private AnyBase _v13;
            private AnyBase _v14;
            private AnyBase _v15;

            public AnyBase this[int i]
            {
                readonly get => i switch
                {
                    0  => _v00,
                    1  => _v01,
                    2  => _v02,
                    3  => _v03,
                    4  => _v04,
                    5  => _v05,
                    6  => _v06,
                    7  => _v07,
                    8  => _v08,
                    9  => _v09,
                    10 => _v10,
                    11 => _v11,
                    12 => _v12,
                    13 => _v13,
                    14 => _v14,
                    15 => _v15,
                    _ => default
                };

                set
                {
                    switch (i)
                    {
                        case 0 :  _v00 = value; return;
                        case 1 :  _v01 = value; return;
                        case 2 :  _v02 = value; return;
                        case 3 :  _v03 = value; return;
                        case 4 :  _v04 = value; return;
                        case 5 :  _v05 = value; return;
                        case 6 :  _v06 = value; return;
                        case 7 :  _v07 = value; return;
                        case 8 :  _v08 = value; return;
                        case 9 :  _v09 = value; return;
                        case 10:  _v10 = value; return;
                        case 11:  _v11 = value; return;
                        case 12:  _v12 = value; return;
                        case 13:  _v13 = value; return;
                        case 14:  _v14 = value; return;
                        case 15:  _v15 = value; return;
                        default: return;
                    }
                }
            }
        }

        #endregion
    }
}