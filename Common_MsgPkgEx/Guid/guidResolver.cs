using System;
using MessagePack.Formatters;
using MsgPack = MessagePack;

namespace XD.Common.Guid {
	internal sealed class GuidFormatter : IMessagePackFormatter<guid>
	{
		public void Serialize(ref MsgPack.MessagePackWriter writer, guid value, MsgPack.MessagePackSerializerOptions options)
			=> writer.WriteString(value.AsSpan());

		public guid Deserialize(ref MsgPack.MessagePackReader reader, MsgPack.MessagePackSerializerOptions options)
		{
			if (reader.TryReadNil()) throw new InvalidOperationException("typecode is null, struct not supported");

			options.Security.DepthStep(ref reader);
			var seq = reader.ReadStringSequence();
			if (seq.HasValue)
			{
				var ret = guid.New(seq.Value.FirstSpan);
				reader.Depth--;
				return ret;
			}
			reader.Depth--;
			return default;
		}
	}
}