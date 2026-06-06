#nullable enable

using System.Text;
using ConfImporter.Builtin.Type;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin.Util
{
    internal static class CSharpGenUtil
    {
        public static void GenStructDefined(in TypeInfo type, string baseIndent = "", string oneIndent = "    ", StringBuilder? sbA = null, StringBuilder? sbB = null, bool isAppend = true)
        {
            sbA ??= new StringBuilder();
            sbB ??= new StringBuilder();
            if (!isAppend) sbA.Clear();
            sbB.Clear();

            sbA.Append(baseIndent).Append("public struct ");
            var id = type.GetTypeIdentity(sbA, sbB, true);
            sbA.Append(" : CfgHelper.ICustomStruct\n").Append(baseIndent).Append('{').Append('\n');

            // 写入字段
            for (var i = 0; i < type.FieldCount; i++)
            {
                var field = type[i];
                sbA.Append(baseIndent);
                WriteField(field, sbA, oneIndent);
                sbA.Append('\n');
            }
            sbA.Append('\n');

            // 写入 TryGet
            sbA.Append(baseIndent).Append(oneIndent)
                .Append("public bool __TryGet<T>(string name, out T value)\n")
                .Append(baseIndent).Append(oneIndent).Append("{\n")
                .Append(baseIndent).Append(oneIndent).Append(oneIndent)
                .Append("value = default!;\n")
                .Append(baseIndent).Append(oneIndent).Append(oneIndent)
                .Append("switch (name)\n")
                .Append(baseIndent).Append(oneIndent).Append(oneIndent).Append("{\n");
            for (var i = 0; i < type.FieldCount; i++)
            {
                var field = type[i];
                var isFieldNullable = IsFieldNullableOrReference(field);
                // Unsafe.As 的 TFrom: 引用类型不带 ?, 值类型保留 nullable
                var isReferenceType = field.Type is TypeInfo.EBaseType.String or TypeInfo.EBaseType.Enum or TypeInfo.EBaseType.Text;
                var unsafeFromType = isReferenceType
                    ? GetFieldType(field.Type, TypeInfo.ETypeFlag.None, false)
                    : GetFieldType(field.Type, field.Flag, false);

                sbA.Append(baseIndent)
                    .Append(oneIndent)
                    .Append(oneIndent)
                    .Append(oneIndent)
                    .Append("case nameof(@").Append(field.Name).Append("): ");
                if (isFieldNullable)
                {
                    sbA.Append("if (this.@").Append(field.Name)
                        .Append(" != null && this.@").Append(field.Name)
                        .Append(".GetType() == typeof(T)) { value = Unsafe.As<")
                        .Append(unsafeFromType).Append(", T>(ref this.@").Append(field.Name)
                        .Append(")!; return true; } else return false;\n");
                }
                else
                {
                    sbA.Append("if (this.@").Append(field.Name)
                        .Append(".GetType() == typeof(T)) { value = Unsafe.As<")
                        .Append(unsafeFromType).Append(", T>(ref this.@").Append(field.Name)
                        .Append(")!; return true; } else return false;\n");
                }
            }
            sbA.Append(baseIndent).Append(oneIndent).Append(oneIndent).Append(oneIndent)
                .Append("default: return false;\n")
                .Append(baseIndent).Append(oneIndent).Append(oneIndent)
                .Append("}\n");
            sbA.Append(baseIndent).Append(oneIndent).Append("}\n\n");

            // 写入构造函数
            sbA.Append(baseIndent).Append(oneIndent)
                .Append("private ")
                .Append(id)
                .Append('(');
            for (var i = 0; i < type.FieldCount; i++)
            {
                var field = type[i];
                sbA.Append(GetFieldType(field.Type, field.Flag, false)).Append(" @").Append(field.Name);
                if (i < type.FieldCount - 1) sbA.Append(", ");
            }
            sbA.Append(")\n")
                .Append(baseIndent).Append(oneIndent)
                .Append('{').Append('\n');
            for (var i = 0; i < type.FieldCount; i++)
            {
                var field = type[i];
                sbA.Append(baseIndent).Append(oneIndent).Append(oneIndent);
                sbA.Append("this.@").Append(field.Name).Append(" = @").Append(field.Name).Append(";");
                sbA.Append('\n');
            }
            sbA.Append(baseIndent).Append(oneIndent).Append("}\n\n");

            // 写入构造类
            sbA.Append(baseIndent).Append(oneIndent)
                .Append("public readonly struct ____constructor : CfgUtil.IStructConstructor<____constructor, ")
                .Append(id).Append(">\n")
                .Append(baseIndent).Append(oneIndent).Append("{")
                .Append($@"
{baseIndent}{oneIndent}{oneIndent}public {id} Construct(ref CfgUtil.SerializedData data, CfgUtil.IDeserializeMethod method, string? name)
{baseIndent}{oneIndent}{oneIndent}{oneIndent}=> ConstructNullable(ref data, method, name) ?? default;
{baseIndent}{oneIndent}{oneIndent}public {id}? ConstructNullable(ref CfgUtil.SerializedData data, CfgUtil.IDeserializeMethod method, string? name)
{baseIndent}{oneIndent}{oneIndent}{{
{baseIndent}{oneIndent}{oneIndent}{oneIndent}if (!method.BeginStructScope(ref data, typeof({id}), name)) {{ method.EndStructScope(ref data); return null; }}
{baseIndent}{oneIndent}{oneIndent}{oneIndent}var ret = new {id}(
");
            for (var i = 0; i < type.FieldCount; i++)
            {
                var field = type[i];
                sbA.Append(baseIndent).Append(oneIndent).Append(oneIndent).Append(oneIndent).Append(oneIndent);
                WriteFieldCreate(field, sbA, id);
                if (i < type.FieldCount - 1) sbA.Append(',');
                sbA.Append('\n');
            }

            sbA.Append($@"{baseIndent}{oneIndent}{oneIndent}{oneIndent});
{baseIndent}{oneIndent}{oneIndent}{oneIndent}method.EndStructScope(ref data);
{baseIndent}{oneIndent}{oneIndent}{oneIndent}return ret;
{baseIndent}{oneIndent}{oneIndent}}}
");
            sbA.Append(baseIndent).Append(oneIndent).Append("}\n").Append(baseIndent).Append('}');
            return;

            static void WriteFieldCreate(in TypeInfo.Field field, StringBuilder sbA, string id)
            {
                sbA.Append(GetBaseTypeCastFunctionName(field.Type, (field.Flag & TypeInfo.ETypeFlag.Nullable) != 0, false))
                    .Append($"(ref data, method, nameof({id}.@{field.Name}))");
            }

            static void WriteField(in TypeInfo.Field field, StringBuilder sbA, string oneIndent)
            {
                sbA.Append(oneIndent).Append("public ");
                sbA.Append(GetFieldType(field.Type, field.Flag, false));
                sbA.Append(" @").Append(field.Name).Append(';');
            }

            static bool IsFieldNullableOrReference(in TypeInfo.Field field)
            {
                // string / Enum / Text 是引用类型
                if (field.Type is TypeInfo.EBaseType.String or TypeInfo.EBaseType.Enum or TypeInfo.EBaseType.Text)
                    return true;
                // 有 Nullable 标记 → 可空
                if ((field.Flag & TypeInfo.ETypeFlag.Nullable) != 0)
                    return true;
                return false;
            }
        }

        public static string GetFieldType(TypeInfo.EBaseType type, TypeInfo.ETypeFlag flag, bool isId)
        {
            if (isId && type.IsTypeCanBeId()) return "CfgHelper.Id";
            var ret = type switch
            {
                TypeInfo.EBaseType.Bool => "bool",
                TypeInfo.EBaseType.Int8 => "sbyte",
                TypeInfo.EBaseType.Int16 => "short",
                TypeInfo.EBaseType.Int32 => "int",
                TypeInfo.EBaseType.Int64 => "long",
                TypeInfo.EBaseType.UInt8 => "byte",
                TypeInfo.EBaseType.UInt16 => "ushort",
                TypeInfo.EBaseType.UInt32 => "uint",
                TypeInfo.EBaseType.UInt64 => "ulong",
                TypeInfo.EBaseType.Float32 => "float",
                TypeInfo.EBaseType.Float64 => "double",
                TypeInfo.EBaseType.String => "string",
                TypeInfo.EBaseType.Link => "CfgHelper.Link",
                TypeInfo.EBaseType.Enum => "string",
                TypeInfo.EBaseType.Text => "string",
                _ => "_"
            };
            if ((flag & TypeInfo.ETypeFlag.Nullable) != 0)
                return ret + '?';
            return ret;
        }

        /// <summary>
        /// 判断字段是否为引用类型或可空值类型 (需要在 __TryGet 中判空).
        /// </summary>
        public static bool IsNullableOrReferenceType(in TypeInfo info, bool isId)
        {
            // 容器类型 (IReadOnlyList / IReadOnlyDictionary) 是引用类型
            if (info.Container != TypeInfo.EContainer.None) return true;

            // 外层有 Nullable 标记 → 可空值类型 or 可空引用类型, 都需要判空
            if ((info.Flag & TypeInfo.ETypeFlag.Nullable) != 0) return true;

            // string / Enum / Text 在 C# 中是引用类型
            if (info.ValType is TypeInfo.EBaseType.String or TypeInfo.EBaseType.Enum or TypeInfo.EBaseType.Text)
                return true;

            // Link 是 struct, 不需要判空; Id 也是 struct
            // Custom 不带 nullable 也是 struct
            return false;
        }

        /// <summary>
        /// 获取用于 Unsafe.As&lt;TFrom, TTo&gt; 中 TFrom 的类型名.
        /// 对于引用类型不带 '?' (运行时无区别); 对于值类型保留 nullable 标记 (Nullable&lt;T&gt; 与 T 是不同类型).
        /// </summary>
        public static string GetUnsafeAsTypeName(in TypeInfo info, string typeIdentity, bool isId)
        {
            // 对于容器类型, 需要拼完整的泛型声明 (容器本身是引用类型, 不带尾部 ?)
            switch (info.Container)
            {
                case TypeInfo.EContainer.Array:
                {
                    var valStr = info.ValType == TypeInfo.EBaseType.Custom
                        ? $"CfgGenStruct.{typeIdentity}"
                        : GetFieldType(info.ValType, TypeInfo.ETypeFlag.None, false);
                    if ((info.ValFlag & TypeInfo.ETypeFlag.Nullable) != 0) valStr += '?';
                    return "IReadOnlyList<" + valStr + ">";
                }
                case TypeInfo.EContainer.Dictionary:
                {
                    var valStr = info.ValType == TypeInfo.EBaseType.Custom
                        ? $"CfgGenStruct.{typeIdentity}"
                        : GetFieldType(info.ValType, TypeInfo.ETypeFlag.None, false);
                    if ((info.ValFlag & TypeInfo.ETypeFlag.Nullable) != 0) valStr += '?';
                    var keyStr = GetFieldType(info.KeyType, TypeInfo.ETypeFlag.None, false);
                    return "IReadOnlyDictionary<" + keyStr + ", " + valStr + ">";
                }
            }

            // 非容器, Id 特殊处理
            if (isId && info.ValType.IsTypeCanBeId()) return "CfgHelper.Id";

            // Custom struct — 如果外层 nullable 则带 ?
            if (info.ValType == TypeInfo.EBaseType.Custom)
            {
                var result = $"CfgGenStruct.{typeIdentity}";
                if ((info.Flag & TypeInfo.ETypeFlag.Nullable) != 0) result += '?';
                return result;
            }

            // Link — 值类型, 需保留 nullable
            if (info.ValType == TypeInfo.EBaseType.Link)
                return (info.Flag & TypeInfo.ETypeFlag.Nullable) != 0 ? "CfgHelper.Link?" : "CfgHelper.Link";

            // string / Enum / Text — 引用类型, 不带 ? (运行时同类型)
            if (info.ValType is TypeInfo.EBaseType.String or TypeInfo.EBaseType.Enum or TypeInfo.EBaseType.Text)
                return "string";

            // 基础值类型 — 保留 nullable 标记
            return GetFieldType(info.ValType, info.Flag, false);
        }

        public static string GetCustomTypeCastFunctionName(string typeIdentity, bool isNullable)
        {
            return isNullable
                ? $"CfgUtil.StructConstructorFuncUtils.ConstructNullable<CfgGenStruct.{typeIdentity}, CfgGenStruct.{typeIdentity}.____constructor>"
                : $"CfgUtil.StructConstructorFuncUtils.Construct<CfgGenStruct.{typeIdentity}, CfgGenStruct.{typeIdentity}.____constructor>";
        }

        public static string GetBaseTypeCastFunctionName(TypeInfo.EBaseType type, bool isNullable, bool isId)
        {
            if (isId && type.IsTypeCanBeId()) return "CfgUtil.BaseConstructorFuncMethods.ConstructorId";
            if (type == TypeInfo.EBaseType.Link) return isNullable ? "CfgUtil.BaseConstructorFuncMethods.ConstructorLinkNullable" : "CfgUtil.BaseConstructorFuncMethods.ConstructorLink";
            if (isNullable)
            {
                return "CfgUtil.BaseConstructorFuncMethods.Constructor" + type switch
                {
                    TypeInfo.EBaseType.Bool => "BoolNullable",
                    TypeInfo.EBaseType.Int8 => "I8Nullable",
                    TypeInfo.EBaseType.Int16 => "I16Nullable",
                    TypeInfo.EBaseType.Int32 => "I32Nullable",
                    TypeInfo.EBaseType.Int64 => "I64Nullable",
                    TypeInfo.EBaseType.UInt8 => "U8Nullable",
                    TypeInfo.EBaseType.UInt16 => "U16Nullable",
                    TypeInfo.EBaseType.UInt32 => "U32Nullable",
                    TypeInfo.EBaseType.UInt64 => "U64Nullable",
                    TypeInfo.EBaseType.Float32 => "F32Nullable",
                    TypeInfo.EBaseType.Float64 => "F64Nullable",
                    TypeInfo.EBaseType.Enum => "StringNullable",
                    TypeInfo.EBaseType.Text => "StringNullable",
                    TypeInfo.EBaseType.String => "StringNullable",
                    _ => "_",
                };
            }

            if (type is TypeInfo.EBaseType.String or TypeInfo.EBaseType.Enum or TypeInfo.EBaseType.Text) return "CfgUtil.BaseConstructorFuncMethods.ConstructorString";
            return "CfgUtil.BaseConstructorFuncMethods.Constructor" + type switch
            {
                TypeInfo.EBaseType.Bool => "Bool",
                TypeInfo.EBaseType.Int8 => "I8",
                TypeInfo.EBaseType.Int16 => "I16",
                TypeInfo.EBaseType.Int32 => "I32",
                TypeInfo.EBaseType.Int64 => "I64",
                TypeInfo.EBaseType.UInt8 => "U8",
                TypeInfo.EBaseType.UInt16 => "U16",
                TypeInfo.EBaseType.UInt32 => "U32",
                TypeInfo.EBaseType.UInt64 => "U64",
                TypeInfo.EBaseType.Float32 => "F32",
                TypeInfo.EBaseType.Float64 => "F64",
                _ => "_",
            };
        }
    }
}
