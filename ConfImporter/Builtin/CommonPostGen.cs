
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using ConfImporter.Builtin.Type;
using ConfImporter.Builtin.Util;
using ConfImporter.Config;

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin
{
    public static class CommonPostGen
    {
        public static void CommonPostGenByte(Config.ConfImporter conf, CancellationToken ct, IEnumerable<TableTypeDec.ITableInst> instList)
        {
            // nothing to be done
        }

        public static void CommonPostGenCode(Config.ConfImporter conf, CancellationToken ct, IEnumerable<TableTypeDec.ITableInst> instList)
        {
            // 写入自定义类型信息
            lock (Types)
            {
                // ReSharper disable once InconsistentNaming
                const string Namespace = "XD.A0.Game.Runtime.Config";

                var sb = new StringBuilder();
                sb.Append(@"#nullable enable

using XD.A0.Engine.Runtime.Module.MConfig;

// ReSharper disable All
// ReSharper disable InconsistentNaming
#pragma warning disable CS8618, CS9264

// ReSharper disable once CheckNamespace
");
                sb.Append($"namespace {Namespace}.CfgGenStruct\n{{\n");
                var sbTemp = new StringBuilder();

                var cnt = 0;
                // 正文
                foreach (var type in Types.Values)
                {
                    if (cnt != 0) sb.Append('\n');
                    CSharpGenUtil.GenStructDefined(type, "    ", "    ", sb, sbTemp);
                    sb.Append('\n');
                    cnt++;
                }
                sb.Append("}\n");

                using var f = File.Create(conf.CodeOutputTargetDir + "/CfgGenStruct.cs");
                using var fWriter = new StreamWriter(f);
                fWriter.Write(sb.ToString());
            }
        }

        public static void CommonClear(Config.ConfImporter conf)
        {
            lock (Types) Types.Clear();
        }

        internal static void AddType(in TypeInfo info, Config.ConfImporter conf)
        {
            if (info.ValType != TypeInfo.EBaseType.Custom) return;
            var str = info.ToString(false, false);

            lock (Types)
            {
                if (Types.ContainsKey(str)) return;
                Types.Add(str, info);
            }
        }

        private static readonly Dictionary<string, TypeInfo> Types = new();
    }
}