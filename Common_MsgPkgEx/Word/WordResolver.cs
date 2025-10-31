using System;
using System.Text;
using MsgPack = MessagePack;

namespace XD.Common.Word
{
    public sealed class WordFormatter : MsgPack::Formatters.IMessagePackFormatter<Word>
    {
        public void Serialize(ref MsgPack::MessagePackWriter writer, Word value, MsgPack::MessagePackSerializerOptions options)
        {
            if (value.Length <= 0)
            {
                writer.WriteString(new ReadOnlySpan<byte>());
                return;
            }
            writer.WriteString(Encoding.UTF8.GetBytes(value.ToString()));
        }

        public Word Deserialize(ref MsgPack::MessagePackReader reader, MsgPack::MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil()) throw new InvalidOperationException("typecode is null, struct not supported");

            options.Security.DepthStep(ref reader);
            var seq = reader.ReadStringSequence();
            if (seq.HasValue)
            {
                var src = seq.Value.FirstSpan;
                Span<char> buffer = stackalloc char[Encoding.UTF8.GetCharCount(src)];
                var ret = new Word(buffer);
                reader.Depth--;
                return ret;
            }
            reader.Depth--;
            return default;
        }
    }

    internal sealed class LongWordFormatter : MsgPack::Formatters.IMessagePackFormatter<LongWord>
    {
        public void Serialize(ref MsgPack::MessagePackWriter writer, LongWord value, MsgPack::MessagePackSerializerOptions options)
        {
            if (value.Length <= 0)
            {
                writer.WriteString(new ReadOnlySpan<byte>());
                return;
            }
            writer.WriteString(Encoding.UTF8.GetBytes(value.ToString()));
        }

        public LongWord Deserialize(ref MsgPack::MessagePackReader reader, MsgPack::MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil()) throw new InvalidOperationException("typecode is null, struct not supported");

            options.Security.DepthStep(ref reader);
            var seq = reader.ReadStringSequence();
            if (seq.HasValue)
            {
                var src = seq.Value.FirstSpan;
                Span<char> buffer = stackalloc char[Encoding.UTF8.GetCharCount(src)];
                var ret = new LongWord(buffer);
                reader.Depth--;
                return ret;
            }
            reader.Depth--;
            return default;
        }
    }
}