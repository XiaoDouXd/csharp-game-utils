using System;
using System.Collections.Generic;
using ConfImporter.Builtin.Type;

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin.Util
{
    internal static partial class Analyzer
    {
        // ReSharper disable GrammarMistakeInComment
        /// <summary>
        /// 解析类型:
        /// <para>
        ///     复合类型:
        ///         int[] | {int a, float b}[] -> 数组;
        ///         int[int] | {int a, float b}[string] -> 字典
        /// </para>
        /// </summary>
        /// <param name="typeStr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        // ReSharper restore GrammarMistakeInComment
        public static TypeInfo ToType(in ReadOnlySpan<char> typeStr)
        {
            var str = typeStr;
            StringUtil.TrimHead(ref str);
            var type = MatchId(ref str);
            StringUtil.TrimHead(ref str);
            var field = MatchFieldBlock(ref str);
            StringUtil.TrimHead(ref str);
            var attr = MatchAttributeBlock(ref str);
            StringUtil.TrimHead(ref str);
            var typeFlag = MatchFlag(ref str);
            StringUtil.TrimHead(ref str);
            var container = MatchContainerBlock(ref str);
            StringUtil.TrimHead(ref str);
            var containerFlag = container.IsEmpty ? ReadOnlySpan<char>.Empty : MatchFlag(ref str);
            StringUtil.TrimHead(ref str);

            if (!str.IsEmpty) throw new ArgumentException("invalid type str");

            if (container.IsEmpty)
            {
                var bType = ToBaseType(type);
                if (bType.Type != TypeInfo.EBaseType.Null)
                    return new TypeInfo(bType.Type, string.Empty, typeFlag.IsEmpty ? bType.Flag : ToFlag(typeFlag))
                    { Attribute = attr.IsEmpty ? string.Empty : attr.ToString() };

                var info = new TypeInfo(TypeInfo.EBaseType.Custom, type.IsEmpty ? string.Empty : type.ToString(),
                    typeFlag.IsEmpty ? bType.Flag : ToFlag(typeFlag))
                    { Attribute = attr.IsEmpty ? string.Empty : attr.ToString() };
                AddFields(ref field, ref info);
                return info;
            }

            if (container[0] == ArrayPlaceholder)
            {
                var bType = ToBaseType(type);
                var info = new TypeInfo(TypeInfo.EContainer.Array, TypeInfo.EBaseType.Int32,
                    typeFlag.IsEmpty ? bType.Flag : ToFlag(typeFlag),
                    containerFlag.IsEmpty ? TypeInfo.ETypeFlag.Nullable : ToFlag(containerFlag))
                    { Attribute = attr.IsEmpty ? string.Empty : attr.ToString() };

                if (bType.Type != TypeInfo.EBaseType.Null)
                {
                    info.SetContainerValTypeBase(bType.Type, typeFlag.IsEmpty ? bType.Flag : ToFlag(typeFlag));
                    return info;
                }

                if (!type.IsEmpty) info.SetContainerValTypeAlias(type.ToString(), typeFlag.IsEmpty ? bType.Flag : ToFlag(typeFlag));
                AddFields(ref field, ref info);
                return info;
            }
            else
            {
                StringUtil.TrimHead(ref container);
                var keyTypeStr = MatchId(ref container);
                StringUtil.TrimHead(ref container);
                if (!container.IsEmpty) throw new ArgumentException("invalid key type str");

                var kType = ToBaseType(keyTypeStr);
                if (kType.Type == TypeInfo.EBaseType.Null) throw new ArgumentException("type of key must be base type with no attr and nullable flag");

                var bType = ToBaseType(type);
                var info = new TypeInfo(TypeInfo.EContainer.Dictionary, kType.Type,
                    typeFlag.IsEmpty ? bType.Flag : ToFlag(typeFlag),
                    containerFlag.IsEmpty ? TypeInfo.ETypeFlag.Nullable : ToFlag(containerFlag))
                    { Attribute = attr.IsEmpty ? string.Empty : attr.ToString() };

                if (bType.Type != TypeInfo.EBaseType.Null)
                {
                    info.SetContainerValTypeBase(bType.Type, typeFlag.IsEmpty ? bType.Flag : ToFlag(typeFlag));
                    return info;
                }

                if (!type.IsEmpty) info.SetContainerValTypeAlias(type.ToString(), typeFlag.IsEmpty ? bType.Flag : ToFlag(typeFlag));
                AddFields(ref field, ref info);
                return info;
            }

            void AddFields(ref ReadOnlySpan<char> fieldStr, ref TypeInfo info)
            {
                StringUtil.TrimHead(ref fieldStr);
                var order = (sbyte)0;
                while (MatchField(ref fieldStr, out var fieldType, out var fieldFlag, out var fieldName))
                {
                    var bFieldType = ToBaseType(fieldType);
                    if (bFieldType.Type == TypeInfo.EBaseType.Null) throw new ArgumentException("type of field must be basic");
                    info.Add(order, bFieldType.Type, fieldName.ToString(), fieldFlag.IsEmpty ? bFieldType.Flag : ToFlag(fieldFlag));
                    if (order++ > 15) break;

                    StringUtil.TrimHead(ref fieldStr);
                    if (fieldStr.Length > 1 && fieldStr[0] is ',' or '，') fieldStr = fieldStr[1..];
                    else if (!StringUtil.IsEmptyOrWhiteSpace(fieldStr)) throw new ArgumentException("invalid field str");
                    else break;
                }
                if (string.IsNullOrEmpty(info.Alias) && info.FieldCount <= 0) throw new ArgumentException("invalid custom type dec");
            }
        }

        private static ReadOnlySpan<char> MatchFieldBlock(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty || str[0] != '{') return ReadOnlySpan<char>.Empty;

            var i = 0;
            var isEnter = false;
            var tStr = str[1..];
            for (; i < tStr.Length; i++)
            {
                isEnter = true;
                var c = tStr[i];
                if (c == '}')
                {
                    isEnter = false;
                    break;
                }

                if (IsFlag(c)) continue;
                if (IsDigit(c)) continue;
                if (char.IsWhiteSpace(c)) continue;
                if (IsLetterOrUnderLine(c)) continue;
                if (c is ',' or '，') continue;
                break;
            }
            if (isEnter) return ReadOnlySpan<char>.Empty;

            str = tStr;
            var slice = str[..i];
            str = str[(i + 1)..];
            return slice;
        }

        private static ReadOnlySpan<char> MatchAttributeBlock(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty || str[0] != '<') return ReadOnlySpan<char>.Empty;

            var i = 0;
            var isEnter = false;
            var tStr = str[1..];
            for (; i < tStr.Length; i++)
            {
                isEnter = true;
                var c = tStr[i];
                if (c == '>')
                {
                    isEnter = false;
                    break;
                }

                if (IsFlag(c)) continue;
                if (IsDigit(c)) continue;
                if (char.IsWhiteSpace(c)) continue;
                if (IsLetterOrUnderLine(c)) continue;
                if (c == '|') continue;
                break;
            }
            if (isEnter) return ReadOnlySpan<char>.Empty;

            str = tStr;
            var slice = str[..i];
            str = str[(i + 1)..];
            return slice;
        }

        private static ReadOnlySpan<char> MatchContainerBlock(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty || str[0] != '[') return ReadOnlySpan<char>.Empty;

            var i = 0;
            var isEnter = false;
            var tStr = str[1..];
            for (; i < tStr.Length; i++)
            {
                isEnter = true;
                var c = tStr[i];
                if (c == ']')
                {
                    isEnter = false;
                    break;
                }

                if (IsFlag(c)) continue;
                if (IsDigit(c)) continue;
                if (char.IsWhiteSpace(c)) continue;
                if (IsLetterOrUnderLine(c)) continue;
                break;
            }
            if (isEnter) return ReadOnlySpan<char>.Empty;

            str = tStr;
            var slice = str[..i];
            str = str[(i + 1)..];
            return slice.IsEmpty ? ArrayPlaceholder.ToString().AsSpan() : slice;
        }

        private static ReadOnlySpan<char> MatchFlag(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty) return ReadOnlySpan<char>.Empty;

            var i = 0;
            var lastFlagIdx = -1;
            var isEnter = false;
            for (; i < str.Length; i++)
            {
                if (isEnter && char.IsWhiteSpace(str[i])) continue;
                if (IsFlag(str[i]))
                {
                    isEnter = true;
                    lastFlagIdx = i;
                    continue;
                }
                break;
            }

            var slice = lastFlagIdx < 0 ? ReadOnlySpan<char>.Empty : str[..(lastFlagIdx + 1)];
            str = str[(lastFlagIdx + 1)..];
            return slice;
        }

        private static ReadOnlySpan<char> MatchId(ref ReadOnlySpan<char> str)
        {
            if (str.IsEmpty) return ReadOnlySpan<char>.Empty;

            for (var i = 0; i < str.Length; i++)
            {
                if (i == 0 && IsDigit(str[i])) return ReadOnlySpan<char>.Empty;
                if (IsLetterOrUnderLine(str[i]) || IsDigit(str[i])) continue;

                var ret = str[..i];
                str = str[i..];
                return ret;
            }

            var slice = str;
            str = ReadOnlySpan<char>.Empty;
            return slice;
        }

        private static bool MatchField(ref ReadOnlySpan<char> str, out ReadOnlySpan<char> type, out ReadOnlySpan<char> flag, out ReadOnlySpan<char> name)
        {
            StringUtil.TrimHead(ref str);
            if (str.IsEmpty)
            {
                type = ReadOnlySpan<char>.Empty;
                name = ReadOnlySpan<char>.Empty;
                flag = ReadOnlySpan<char>.Empty;
                return false;
            }

            var tStr = str;
            type = MatchId(ref tStr);
            if (ToBaseType(type).Type == TypeInfo.EBaseType.Null)
                throw new ArgumentException($"type of field must be basic, unknown type: {type.ToString()}");

            StringUtil.TrimHead(ref tStr);
            flag = MatchFlag(ref tStr);
            StringUtil.TrimHead(ref tStr);
            name = MatchId(ref tStr);
            // 复合类型字段名禁止使用下划线
            if (StringUtil.Contains(name, '_'))
                throw new ArgumentException("can't use '_' as field name in custom type");

            str = tStr;
            return true;
        }

        private static bool IsFlag(char c) => c is '?' or '？' or '!' or '！';
        private static bool IsDigit(char c) => c is >= '0' and <= '9';
        private static bool IsLetterOrUnderLine(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';

        private static TypeInfo.ETypeFlag ToFlag(in ReadOnlySpan<char> str)
        {
            if (str.IsEmpty) return TypeInfo.ETypeFlag.None;
            for (var i = str.Length - 1; i >= 0; i--)
            {
                switch (str[i])
                {
                    case '?': case '？': return TypeInfo.ETypeFlag.Nullable;
                    case '!': case '！': return TypeInfo.ETypeFlag.None;
                }
            }
            return TypeInfo.ETypeFlag.None;
        }

        private static TypeDefaultSetting ToBaseType(in ReadOnlySpan<char> str)
            => TypeMap.TryGetValue(str.ToString().ToLower(), out var v)
                ? v : new TypeDefaultSetting(TypeInfo.EBaseType.Null, TypeInfo.ETypeFlag.None);

        private const char ArrayPlaceholder = '\u200b';

        private readonly struct TypeDefaultSetting
        {
            public readonly TypeInfo.EBaseType Type;
            public readonly TypeInfo.ETypeFlag Flag;
            public TypeDefaultSetting(TypeInfo.EBaseType t, TypeInfo.ETypeFlag f) { Type = t; Flag = f; }
        }

        private static readonly SortedList<string, TypeDefaultSetting> TypeMap = new()
        {
            ["bool"] = new TypeDefaultSetting(TypeInfo.EBaseType.Bool, TypeInfo.ETypeFlag.None),
            ["boolean"] = new TypeDefaultSetting(TypeInfo.EBaseType.Bool, TypeInfo.ETypeFlag.None),

            ["int8"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int8, TypeInfo.ETypeFlag.None),
            ["sbyte"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int8, TypeInfo.ETypeFlag.None),
            ["uint8"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt8, TypeInfo.ETypeFlag.None),
            ["byte"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt8, TypeInfo.ETypeFlag.None),

            ["int16"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int16, TypeInfo.ETypeFlag.None),
            ["short"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int16, TypeInfo.ETypeFlag.None),
            ["uint16"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt16, TypeInfo.ETypeFlag.None),
            ["ushort"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt16, TypeInfo.ETypeFlag.None),

            ["int32"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int32, TypeInfo.ETypeFlag.None),
            ["int"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int32, TypeInfo.ETypeFlag.None),
            ["uint32"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt32, TypeInfo.ETypeFlag.None),
            ["uint"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt32, TypeInfo.ETypeFlag.None),

            ["int64"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int64, TypeInfo.ETypeFlag.None),
            ["long"] = new TypeDefaultSetting(TypeInfo.EBaseType.Int64, TypeInfo.ETypeFlag.None),
            ["uint64"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt64, TypeInfo.ETypeFlag.None),
            ["ulong"] = new TypeDefaultSetting(TypeInfo.EBaseType.UInt64, TypeInfo.ETypeFlag.None),

            ["float32"] = new TypeDefaultSetting(TypeInfo.EBaseType.Float32, TypeInfo.ETypeFlag.None),
            ["float"] = new TypeDefaultSetting(TypeInfo.EBaseType.Float32, TypeInfo.ETypeFlag.None),
            ["float64"] = new TypeDefaultSetting(TypeInfo.EBaseType.Float64, TypeInfo.ETypeFlag.None),
            ["double"] = new TypeDefaultSetting(TypeInfo.EBaseType.Float64, TypeInfo.ETypeFlag.None),

            ["link"] = new TypeDefaultSetting(TypeInfo.EBaseType.Link, TypeInfo.ETypeFlag.None),

            ["string"] = new TypeDefaultSetting(TypeInfo.EBaseType.String, TypeInfo.ETypeFlag.Nullable),
            ["str"] = new TypeDefaultSetting(TypeInfo.EBaseType.String, TypeInfo.ETypeFlag.Nullable),

            ["enum"] = new TypeDefaultSetting(TypeInfo.EBaseType.Enum, TypeInfo.ETypeFlag.Nullable)
        };
    }
}