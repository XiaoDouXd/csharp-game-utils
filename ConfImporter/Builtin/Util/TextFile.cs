#nullable enable

using System.IO;
using System.Text;

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin.Util
{
    /// <summary>
    /// 写文本文件的统一入口, 强制使用 UTF-8 无 BOM (导表工具的所有产物文本都按这一约定输出).
    /// </summary>
    internal static class TextFile
    {
        private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

        public static void WriteAllText(string path, string? content)
        {
            // File.WriteAllText 接受 Encoding 参数, 直接用即可
            File.WriteAllText(path, content ?? string.Empty, Utf8NoBom);
        }

        public static StreamWriter CreateWriter(string path)
        {
            var fs = File.Create(path);
            return new StreamWriter(fs, Utf8NoBom);
        }
    }
}
