using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ConfImporter.Builtin.Type;
using ConfImporter.Builtin.Util;
using System.Threading;
using MessagePack;

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin
{
    partial class GlobalTable
    {
        public override void GenCodes(CancellationToken c)
        {
            lock (_tables)
            {
                if (_tables is not { Count: > 0 })
                {
                    using var fEmpty = File.Create(Conf.CodeOutputTargetDir + "/CfgGenGlobalTable.cs");
                    using var fWriterEmpty = new StreamWriter(fEmpty);
                    fWriterEmpty.Write("");
                    return;
                }
            }

            const string indent = "    ";
            var tableNamespace = Conf.CodeNamespace ?? "XD.A0.Game.Runtime.Config";
            var sb = new StringBuilder();
            var sbDef = new StringBuilder();
            var sbConstruct = new StringBuilder();
            var sbFunction = new StringBuilder();
            var sbTempA = new StringBuilder();
            var sbTempB = new StringBuilder();
            sb.Append($@"// ReSharper disable UnusedNullableDirective
// ReSharper disable RedundantUsingDirective

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using XD.GameModule.Module.MConfig;

// ReSharper disable All
// ReSharper disable InconsistentNaming
#pragma warning disable CS8618, CS9264

// ReSharper disable once CheckNamespace
namespace {tableNamespace}
{{
");
            var fieldIdx = 0;
            lock (_tables)
            {
                sb.Append($"{indent}public static class G\n{indent}{{\n");
                foreach (var (nameSrc, fields) in _tables)
                {
                    var name = "@" + nameSrc;
                    if (fieldIdx != 0) sb.Append("\n");
                    sb.Append($"{indent}{indent}public static class {name}\n{indent}{indent}{{\n");
                    sbDef.Clear();
                    sbConstruct.Clear();
                    sbTempA.Clear();
                    sbTempB.Clear();

                    foreach (var (field, data) in fields)
                    {
                        var n = '@' + field;
                        var defN = "____" + field;
                        var typeIdentity = GetFieldValueIdentity(data.Type, sbTempA, sbTempB, false);
                        var typeStr = GetFieldTypeName(data.Type, typeIdentity);
                        var container = data.Type.Container;
                        var type = data.Type.ValType;
                        var isValNullable = (data.Type.ValFlag & TypeInfo.ETypeFlag.Nullable) != 0;
                        sb.Append($"{indent}{indent}{indent}public static {typeStr} {n} => {defN};\n");
                        sbDef.Append($"{indent}{indent}{indent}private static {typeStr} {defN} = default!;\n");
                        sbConstruct.Append(indent).Append(indent).Append(indent).Append(indent);
                        sbConstruct.Append(defN + " = ");
                        switch (container)
                        {
                            case TypeInfo.EContainer.Array:
                                sbConstruct.Append("method.ReadArray(ref data, ")
                                    .Append(type == TypeInfo.EBaseType.Custom
                                        ? CSharpGenUtil.GetCustomTypeCastFunctionName(typeIdentity, isValNullable)
                                        : CSharpGenUtil.GetBaseTypeCastFunctionName(type, isValNullable, false))
                                    .Append($", nameof({name}.{n}))!;\n");
                                break;
                            case TypeInfo.EContainer.Dictionary:
                            {
                                var keyType = data.Type.KeyType;
                                sbConstruct.Append("method.ReadMap(ref data, ")
                                    .Append(CSharpGenUtil.GetBaseTypeCastFunctionName(keyType, false, false))
                                    .Append(", ")
                                    .Append(type == TypeInfo.EBaseType.Custom
                                        ? CSharpGenUtil.GetCustomTypeCastFunctionName(typeIdentity, isValNullable)
                                        : CSharpGenUtil.GetBaseTypeCastFunctionName(type, isValNullable, false))
                                    .Append($", nameof({name}.{n}))!;\n");
                                break;
                            }
                            case TypeInfo.EContainer.None:
                            default:
                                sbConstruct.Append(type == TypeInfo.EBaseType.Custom
                                        ? CSharpGenUtil.GetCustomTypeCastFunctionName(typeIdentity, isValNullable)
                                        : CSharpGenUtil.GetBaseTypeCastFunctionName(type, isValNullable, false))
                                    .Append($"(ref data, method, nameof({name}.{n}));\n");
                                break;
                        }
                        fieldIdx += 1;
                    }
                    sb.Append('\n');
                    sb.Append(sbDef);
                    sb.Append($"{indent}{indent}{indent}internal static void ____constructor(ref CfgUtil.SerializedData data, CfgUtil.IDeserializeMethod method)\n{indent}{indent}{indent}{{\n");
                    sb.Append($"{indent}{indent}{indent}{indent}if (method.BeginTableScope(ref data, typeof({name}), nameof({name})) != null) {{ method.EndTableScope(ref data); return; }}\n");
                    sb.Append(sbConstruct);
                    sb.Append($"{indent}{indent}{indent}{indent}method.EndTableScope(ref data);\n{indent}{indent}{indent}}}\n{indent}{indent}}}\n");
                    sbFunction.Append($"{indent}{indent}{indent}G.{name}.____constructor(ref data, method);\n");
                }

            }

            sb.Append($"{indent}}}\n\n");
            sb.Append(indent).Append($@"public static partial class ____cfgGenFunction
{indent}{{
{indent}{indent}public static CfgUtil.GlobalTableCreateResult CreateGlobalTables(ref CfgUtil.SerializedData data, CfgUtil.IDeserializeMethod method)
{indent}{indent}{{
{indent}{indent}{indent}if (method == null) return default;
{indent}{indent}{indent}var version = method.BeginScope(ref data);
{indent}{indent}{indent}if (version == 0) {{ method.EndScope(ref data); return default; }}
");
            sb.Append(sbFunction);
            sb.Append($"{indent}{indent}{indent}method.EndScope(ref data);\n{indent}{indent}{indent}return new CfgUtil.GlobalTableCreateResult(true);\n");
            sb.Append($"{indent}{indent}}}\n{indent}}}\n}}\n");
            using var f = File.Create(Conf.CodeOutputTargetDir + "/CfgGenGlobalTable.cs");
            using var fWriter = new StreamWriter(f);
            fWriter.Write(sb.ToString());
            return;

            static string GetFieldTypeName(in TypeInfo info, string typeStr)
            {
                if (info.ValType == TypeInfo.EBaseType.Custom)
                    typeStr = $"CfgGenStruct.{typeStr}";

                switch (info.Container)
                {
                    case TypeInfo.EContainer.Array:
                        if ((info.ValFlag & TypeInfo.ETypeFlag.Nullable) != 0) typeStr += '?';
                        return "IReadOnlyList<" + typeStr + ">" + ((info.Flag & TypeInfo.ETypeFlag.Nullable) == 0 ? "" : "?");
                    case TypeInfo.EContainer.Dictionary:
                        if ((info.ValFlag & TypeInfo.ETypeFlag.Nullable) != 0) typeStr += '?';
                        return "IReadOnlyDictionary<" +
                               CSharpGenUtil.GetFieldType(info.KeyType, TypeInfo.ETypeFlag.None, false) +
                               ", " + typeStr + ">" + ((info.Flag & TypeInfo.ETypeFlag.Nullable) == 0 ? "" : "?");
                    case TypeInfo.EContainer.None:
                    default:
                        return typeStr + ((info.Flag & TypeInfo.ETypeFlag.Nullable) == 0 ? "" : "?");
                }
            }

            static string GetFieldValueIdentity(in TypeInfo info, StringBuilder sbA, StringBuilder sbB, bool isId)
            {
                if (info.ValType != TypeInfo.EBaseType.Custom)
                    return CSharpGenUtil.GetFieldType(info.ValType, TypeInfo.ETypeFlag.None, isId);
                var ret = info.GetTypeIdentity(sbA, sbB);
                return ret;
            }
        }

        public override void GenBytes(CancellationToken c)
        {
            lock (_tables)
            {
                if (_tables.Count <= 0)
                {
                    using var fEmpty = File.Create(Conf.ByteOutputTargetDir + "/GlobalTable.bytes");
                    using var fWriterEmpty = new StreamWriter(fEmpty);
                    fWriterEmpty.Write("");
                    return;
                }

                var objList = new List<object>{ Conf.Version };
                foreach (var table in _tables.Values)
                    objList.AddRange(table.Values.Select(field => field.Data));
                var bytes = MessagePackSerializer.Serialize(objList);
                using var f = File.Create(Conf.ByteOutputTargetDir + "/GlobalTable.bytes");
                f.Write(bytes);
                using var fJson = File.Create(Conf.ByteOutputTargetDir + "/GlobalTable.json");
                using var fJsonWriter = new StreamWriter(fJson);
                fJsonWriter.Write(MessagePackSerializer.ConvertToJson(bytes));
            }
        }
    }
}