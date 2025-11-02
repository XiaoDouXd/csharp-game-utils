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

            sbA.Append(baseIndent).Append("public readonly struct ");
            var id = type.GetTypeIdentity(sbA, sbB, true);
            sbA.Append(" : CfgUtil.ICustomStruct\n").Append(baseIndent).Append('{').Append('\n');

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
                sbA.Append(baseIndent)
                    .Append(oneIndent)
                    .Append(oneIndent)
                    .Append(oneIndent)
                    .Append("case nameof(@").Append(field.Name)
                    .Append("): if (this.@").Append(field.Name).Append(" is T) { value = (T)(object)this.@")
                    .Append(field.Name).Append("; return true; } else return false;\n");
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
                sbA.Append(oneIndent).Append("public readonly ");
                sbA.Append(GetFieldType(field.Type, field.Flag, false));
                sbA.Append(" @").Append(field.Name).Append(';');
            }
        }

        public static string GetFieldType(TypeInfo.EBaseType type, TypeInfo.ETypeFlag flag, bool isId)
        {
            if (isId && type.IsTypeCanBeId()) return "CfgUtil.Id";
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
                TypeInfo.EBaseType.Link => "CfgUtil.Link",
                TypeInfo.EBaseType.Enum => "string",
                _ => "_"
            };
            if ((flag & TypeInfo.ETypeFlag.Nullable) != 0)
                return ret + '?';
            return ret;
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
                    TypeInfo.EBaseType.String => "StringNullable",
                    _ => "_",
                };
            }

            if (type is TypeInfo.EBaseType.String or TypeInfo.EBaseType.Enum) return "CfgUtil.BaseConstructorFuncMethods.ConstructorString";
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
