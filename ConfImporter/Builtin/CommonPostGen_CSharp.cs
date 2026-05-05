
using System.Collections.Generic;
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
            // 注解元数据 bytes (与 Common/Global/I18n 同源, 支持运行时热刷)
            CfgMetaGen.GenerateBytes(conf);
        }

        public static void CommonPostGenCode(Config.ConfImporter conf, CancellationToken ct, IEnumerable<TableTypeDec.ITableInst> instList)
        {
            // 写入自定义类型信息
            lock (Types)
            {
                // ReSharper disable once InconsistentNaming
                var Namespace = conf.CodeNamespace ?? "XD.A0.Game.Runtime.Config";

                var sb = new StringBuilder();
                sb.Append(@"#nullable enable

using XD.Common.Config.Helper;
using XD.GameModule.Module.MConfig;

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

                TextFile.WriteAllText(conf.CodeOutputTargetDir + "/CfgGenStruct.g.cs", sb.ToString());
            }

            // 生成 meta (类型注解) 反序列化器代码 - 不依赖 Types 字典, 在锁外做.
            CfgMetaGen.GenerateCode(conf);
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
