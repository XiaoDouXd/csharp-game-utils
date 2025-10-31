#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ConfImporter.Builtin.Type;
using ConfImporter.Builtin.Util;
using MessagePack;

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin
{
    public partial class CommonTable
    {
        public override void GenCodes(CancellationToken c)
        {
            const string indent = "    ";
            var tableNamespace = Conf.CodeNamespace ?? "XD.A0.Game.Runtime.Config";
            var sb = new StringBuilder();
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
            var sbFunction = new StringBuilder();
            var sbTableCreator = new StringBuilder();
            sbTableCreator.Append($@"{indent}{indent}{indent}var ret = new CfgUtil.TableGroup[]
{indent}{indent}{indent}{{
");
            var sbTempA = new StringBuilder();
            var sbTempB = new StringBuilder();
            var sbTempC = new StringBuilder();

            CombinedTableInst[]? cfgList;
            lock (_cfg) cfgList = _cfg.Values.Where(inst => inst is { IsBreak: false, Fields: { Count: > 0 } }).ToArray();
            if (cfgList is not { Length: > 0 })
            {
                using var fEmpty = File.Create(Conf.CodeOutputTargetDir + "/CfgGenTable.cs");
                using var fWriterEmpty = new StreamWriter(fEmpty);
                fWriterEmpty.Write("");
                return;
            }

            var sbInterface = new StringBuilder();
            var sbDeclared = new StringBuilder();
            var sbConstruct = new StringBuilder();
            var sbConstructParams = new StringBuilder();

            var sbConstructorStruct = new StringBuilder();

            sbFunction.Append(indent)
                .Append($@"public static partial class ____cfgGenFunction
{indent}{{
{indent}{indent}private class ____ : CfgUtil.CfgTableItemBase {{ public override CfgUtil.Id Id => default; }}
{indent}{indent}public static CfgUtil.CommonTableCreateResult CreateCommonTables(ref CfgUtil.SerializedData data, CfgUtil.IDeserializeMethod method)
{indent}{indent}{{
{indent}{indent}{indent}if (method == null) return default;
{indent}{indent}{indent}var version = method.BeginScope(ref data);
{indent}{indent}{indent}if (version == 0) {{ method.EndScope(ref data); return default; }}
{indent}{indent}{indent}if (CfgUtil.____tableInfoCache<____>.Id == 0)
{indent}{indent}{indent}{{
{indent}{indent}{indent}{indent}CfgUtil.____tableInfoCache<CfgUtil.VERSION>.Id = version;
");
            var tableIdx = 0;
            foreach (var inst in cfgList)
            {
                var tableName = '@' + inst.Name;
                sbTempA.Clear();
                sbTempB.Clear();
                sbTempC.Clear();
                sbInterface.Clear();
                sbDeclared.Clear();
                sbConstruct.Clear();
                sbConstructParams.Clear();
                sbConstructorStruct.Clear();

                var fieldIdx = 0;
                if (tableIdx != 0) sb.Append('\n');
                sb.Append(indent)
                    .Append("public class ")
                    .Append(tableName)
                    .Append(" : CfgUtil.CfgTableItemBase\n")
                    .Append(indent).Append("{\n");
                sb.Append($"{indent}{indent}public override string? __Name => \"{inst.Name}\";\n");
                sbInterface.Append($@"{indent}{indent}public override bool __TryGet<T>(string name, out T value)
{indent}{indent}{{
{indent}{indent}{indent}value = default!;
{indent}{indent}{indent}switch (name)
{indent}{indent}{indent}{{
");
                sbConstructorStruct.Append($@"{indent}public readonly struct ____constructor : CfgUtil.IConstructor<____constructor, {tableName}>
{indent}{indent}{{
{indent}{indent}{indent}public {tableName}? Construct(ref CfgUtil.SerializedData data, CfgUtil.IDeserializeMethod method, string? name)
{indent}{indent}{indent}{{
{indent}{indent}{indent}{indent}if (!method.BeginItemScope(ref data, typeof({tableName}), name)) {{ method.EndItemScope(ref data); return null; }}
{indent}{indent}{indent}{indent}var ret = new {tableName}(
");

                foreach (var field in inst.Fields.Values)
                {
                    var name = '@' + field.Name;
                    var defName = "____" + field.Name;
                    var isId = field.Name == "Id";
                    var typeIdentity = GetFieldValueIdentity(field.Type!.Value, sbTempA, sbTempB, isId);
                    var typeStr = GetFieldTypeName(field.Type!.Value, typeIdentity);
                    var container = field.Type!.Value.Container;
                    var type = field.Type!.Value.ValType;
                    var isValNullable = (field.Type!.Value.ValFlag & TypeInfo.ETypeFlag.Nullable) != 0;

                    // 接口
                    sb.Append(indent).Append(indent)
                        .Append((isId ? "public override " : "public ") + typeStr + ' ' + name + " => " + defName + ";\n");
                    sbInterface.Append($"{indent}{indent}{indent}{indent}case nameof(this.{name}): ")
                        .Append($"if (this.{name} is T) {{ value = (T)(object)this.{defName}!; return true; }} else return false;\n");

                    // 构造函数
                    if (fieldIdx != 0) sbConstructParams.Append(", ");
                    sbConstructParams.Append(typeStr).Append(' ').Append(name);
                    sbConstruct.Append(indent).Append(indent).Append(indent);
                    sbConstruct.Append("this.").Append(defName + " = ").Append($"{name};\n");
                    if (fieldIdx != 0) sbConstructorStruct.Append(",\n");
                    sbConstructorStruct.Append(indent).Append(indent).Append(indent).Append(indent).Append(indent);
                    switch (container)
                    {
                        case TypeInfo.EContainer.Array:
                            sbConstructorStruct.Append("method.ReadArray(ref data, ")
                                .Append(type == TypeInfo.EBaseType.Custom
                                    ? CSharpGenUtil.GetCustomTypeCastFunctionName(typeIdentity, isValNullable)
                                    : CSharpGenUtil.GetBaseTypeCastFunctionName(type, isValNullable, false))
                                .Append($", nameof({tableName}.{name}))!");
                            break;
                        case TypeInfo.EContainer.Dictionary:
                        {
                            var keyType = field.Type!.Value.KeyType;
                            sbConstructorStruct.Append("method.ReadMap(ref data, ")
                                .Append(CSharpGenUtil.GetBaseTypeCastFunctionName(keyType, false, false))
                                .Append(", ")
                                .Append(type == TypeInfo.EBaseType.Custom
                                    ? CSharpGenUtil.GetCustomTypeCastFunctionName(typeIdentity, isValNullable)
                                    : CSharpGenUtil.GetBaseTypeCastFunctionName(type, isValNullable, false))
                                .Append($", nameof({tableName}.{name}))!");
                            break;
                        }
                        case TypeInfo.EContainer.None:
                        default:
                            sbConstructorStruct.Append(type == TypeInfo.EBaseType.Custom
                                    ? CSharpGenUtil.GetCustomTypeCastFunctionName(typeIdentity, isValNullable)
                                    : CSharpGenUtil.GetBaseTypeCastFunctionName(type, isValNullable, isId))
                                .Append($"(ref data, method, nameof({tableName}.{name}))");
                            break;
                    }
                    // 字段定义
                    sbDeclared.Append(indent).Append(indent)
                        .Append("private readonly " + typeStr + ' ' + defName + " = default!;\n");
                    if (fieldIdx != 0) sbTempC.Append(", ");
                    sbTempC.Append($"\"{field.Name}\"");

                    // 自增
                    fieldIdx++;
                }
                sbInterface.Append($"{indent}{indent}{indent}{indent}default: return false;\n{indent}{indent}{indent}}}\n{indent}{indent}}}\n");

                // 插入接口
                sb.Append(sbInterface).Append('\n')
                    .Append(indent).Append(indent)
                    .Append("#region data\n")

                    // 插入构造函数
                    .Append(indent).Append(indent)
                    .Append("private ").Append(tableName)
                    .Append('(')
                    .Append(sbConstructParams)
                    .Append(")\n").Append(indent).Append(indent)
                    .Append("{\n")
                    .Append(sbConstruct)
                    .Append(indent).Append(indent)
                    .Append("}\n")

                    // 插入字段定义
                    .Append(sbDeclared)
                    .Append(indent)
                    // 插入构造结构体
                    .Append(sbConstructorStruct)
                    .Append($@"
{indent}{indent}{indent}{indent});
{indent}{indent}{indent}{indent}method.EndItemScope(ref data);
{indent}{indent}{indent}{indent}return ret;
{indent}{indent}{indent}}}
{indent}{indent}}}
{indent}{indent}#endregion
{indent}}}
");


                sbFunction.Append($"{indent}{indent}{indent}{indent}CfgUtil.____tableInfoCache<{tableName}>.Id = {tableIdx + 1};\n");
                sbTableCreator.Append(
                    $"{indent}{indent}{indent}{indent}new CfgUtil.TableGroup<{tableName}>(ref data, method, CfgUtil.StructConstructorFuncUtils.Construct<{tableName}, {tableName}.____constructor>, \"{inst.Name}\", new []{{{sbTempC}}}),\n");
                tableIdx++;
            }

            sb.Append("\n").Append(sbFunction);
            sb.Append($"{indent}{indent}{indent}{indent}CfgUtil.____tableInfoCache<____>.Id = -1;\n{indent}{indent}{indent}}}\n");
            sb.Append(sbTableCreator);
            sb.Append($"{indent}{indent}{indent}}};\n{indent}{indent}{indent}method.EndScope(ref data);\n{indent}{indent}{indent}return new CfgUtil.CommonTableCreateResult(ret);\n{indent}{indent}}}\n{indent}}}\n");
            sb.Append("}\n");
            using var f = File.Create(Conf.CodeOutputTargetDir + "/CfgGenTable.cs");
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
            CombinedTableInst[]? cfgList;
            lock (_cfg) cfgList = _cfg.Values.Where(inst => inst is { IsBreak: false, Fields: { Count: > 0 } }).ToArray();
            if (cfgList is not { Length: > 0 })
            {
                using var fEmpty = File.Create(Conf.ByteOutputTargetDir + "/CommonTable.bytes");
                using var fWriterEmpty = new StreamWriter(fEmpty);
                fWriterEmpty.Write("");
                return;
            }

            var table = new List<object>{ Conf.Version };
            for (var idy = 1; idy <= cfgList.Length; idy++)
            {
                var inst = cfgList[idy - 1];
                var fields = inst.Fields;
                var fieldCnt = FormatFieldIdx(fields);
                if (inst.Tables.Count <= 1)
                {
                    var t = inst.Tables.First();
                    var tableData = new object?[t.Value.DataList.Count];
                    for (var idx = 0; idx < t.Value.DataList.Count; idx++)
                    {
                        if (!string.IsNullOrEmpty(t.Key)) table.Add(t.Key);
                        var rowData = t.Value.DataList[idx];
                        var data = new object?[fieldCnt];
                        foreach (var (fieldName, value) in rowData.Data)
                        {
                            if (!fields.TryGetValue(fieldName, out var v)) continue;
                            data[v.Idx] = value;
                        }
                        tableData[idx] = data;
                    }
                    table.Add(tableData);
                }
                else
                {
                    var idz = 0;
                    var tableGroup = new object[inst.Tables.Count * 2];
                    foreach (var t in inst.Tables)
                    {
                        tableGroup[idz] = t.Key;
                        var tableData = new object?[t.Value.DataList.Count];
                        for (var idx = 0; idx < t.Value.DataList.Count; idx++)
                        {
                            var rowData = t.Value.DataList[idx];
                            var data = new object?[fieldCnt];
                            foreach (var (fieldName, value) in rowData.Data)
                            {
                                if (!fields.TryGetValue(fieldName, out var v)) continue;
                                data[v.Idx] = value;
                            }
                            tableData[idx] = data;
                        }
                        tableGroup[idz + 1] = tableData;
                        idz += 2;
                    }
                    table.Add(tableGroup);
                }
            }
            var bytes = MessagePackSerializer.Serialize(table.ToArray());
            using var f = File.Create(Conf.ByteOutputTargetDir + "/CommonTable.bytes");
            f.Write(bytes);
            using var fJson = File.Create(Conf.ByteOutputTargetDir + "/CommonTable.json");
            using var fJsonWriter = new StreamWriter(fJson);
            fJsonWriter.Write(MessagePackSerializer.ConvertToJson(bytes));
            return;

            static int FormatFieldIdx( SortedDictionary<string,FieldInfo> fields)
            {
                var idx = 0;
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var info in fields.Values)
                {
                    if (info == null) continue;
                    info.Idx = idx;
                    idx++;
                }
                return idx;
            }
        }
    }
}