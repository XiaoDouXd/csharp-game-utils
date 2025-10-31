#define MSG_PACK_CONFIG
#if MSG_PACK_CONFIG

using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

// ReSharper disable UnassignedField.Global

namespace XD.GameModule.Module.MConfig
{
    public static partial class CfgUtil
    {
        // ReSharper disable once PartialTypeWithSinglePart
        public ref partial struct SerializedData
        {
            public MessagePackReader Reader;
            public MessagePackSerializerOptions Options;
        }

        public class DeserializeMsgPackImpl : IDeserializeMethod
        {
            public bool IsGlobal { get; set; }

            public long BeginScope(ref SerializedData data)
            {
                ref var reader = ref data.Reader;
                var options = data.Options;
                if (reader.TryReadNil())
                {
                    return 0;
                }

                options.Security.DepthStep(ref reader);
                reader.ReadArrayHeader();
                var version = reader.ReadInt64();
                return version;
            }

            public void EndScope(ref SerializedData data)
            {
                data.Reader.Depth--;
            }

            public int BeginTableGroupScope(ref SerializedData data, Type type, string? name = null)
            {
                ref var reader = ref data.Reader;
                var options = data.Options;
                if (reader.TryReadNil()) return -1;
                if (reader.NextMessagePackType != MessagePackType.Integer)
                    return reader.NextMessagePackType is MessagePackType.String or MessagePackType.Array ? 0 : -1;

                var len = reader.ReadInt32();
                options.Security.DepthStep(ref reader);
                var realLen = reader.ReadArrayHeader();
                if (len == realLen && len > 0) return len;
                reader.Depth--;
                return -2;
            }

            public void EndTableGroupScope(ref SerializedData data, int len)
            {
                if (len > 0) data.Reader.Depth--;
            }

            public string BeginTableScope(ref SerializedData data, Type type, string? _)
            {
                ref var reader = ref data.Reader;
                if (IsGlobal || reader.NextMessagePackType == MessagePackType.Array) return string.Empty;
                return reader.ReadString() ?? string.Empty;
            }

            public void EndTableScope(ref SerializedData data) {}

            public bool BeginItemScope(ref SerializedData data, Type type, string? _)
            {
                ref var reader = ref data.Reader;
                var options = data.Options;
                if (reader.TryReadNil()) return false;

                options.Security.DepthStep(ref reader);
                reader.ReadArrayHeader();
                return true;
            }

            public void EndItemScope(ref SerializedData data)
            {
                data.Reader.Depth--;
            }

            public bool BeginStructScope(ref SerializedData data, Type type, string? _)
            {
                ref var reader = ref data.Reader;
                var options = data.Options;
                if (reader.TryReadNil()) return false;

                options.Security.DepthStep(ref reader);
                reader.ReadArrayHeader();
                return true;
            }

            public void EndStructScope(ref SerializedData data)
            {
                if (!IsGlobal)
                {
                    EndTableScope(ref data);
                    return;
                }
                data.Reader.Depth--;
            }

            public string ReadString(ref SerializedData data, string? _)
            {
                return data.Reader.ReadString() ?? string.Empty;
            }

            public string? ReadStringNullable(ref SerializedData data, string? _)
            {
                return data.Reader.ReadString();
            }

            public sbyte ReadI8(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? (sbyte)0 : data.Reader.ReadSByte();
            }

            public sbyte? ReadI8Nullable(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? null : data.Reader.ReadSByte();
            }

            public byte ReadU8(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? (byte)0 : data.Reader.ReadByte();
            }

            public byte? ReadU8Nullable(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? null : data.Reader.ReadByte();
            }

            public short ReadI16(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? (short)0 : data.Reader.ReadInt16();
            }

            public short? ReadI16Nullable(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? null : data.Reader.ReadInt16();
            }

            public ushort ReadU16(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? (ushort)0 : data.Reader.ReadUInt16();
            }

            public ushort? ReadU16Nullable(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? null : data.Reader.ReadUInt16();
            }

            public int ReadI32(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? 0 : data.Reader.ReadInt32();
            }

            public int? ReadI32Nullable(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? null : data.Reader.ReadInt32();
            }

            public uint ReadU32(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? 0 : data.Reader.ReadUInt32();
            }

            public uint? ReadU32Nullable(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? null : data.Reader.ReadUInt32();
            }

            public long ReadI64(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? 0 : data.Reader.ReadInt64();
            }

            public long? ReadI64Nullable(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? null : data.Reader.ReadInt64();
            }

            public ulong ReadU64(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? 0 : data.Reader.ReadUInt64();
            }

            public ulong? ReadU64Nullable(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? null : data.Reader.ReadUInt64();
            }

            public float ReadF32(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? 0 : data.Reader.ReadSingle();
            }

            public float? ReadF32Nullable(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? null : data.Reader.ReadSingle();
            }

            public double ReadF64(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? 0 : data.Reader.ReadDouble();
            }

            public double? ReadF64Nullable(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? null : data.Reader.ReadDouble();
            }

            public bool ReadBool(ref SerializedData data, string? _)
            {
                return !data.Reader.TryReadNil() && data.Reader.ReadBoolean();
            }

            public bool? ReadBoolNullable(ref SerializedData data, string? _)
            {
                return data.Reader.TryReadNil() ? null : data.Reader.ReadBoolean();
            }

            public Id ReadId(ref SerializedData data, string? _)
            {
                ref var reader = ref data.Reader;
                if (reader.TryReadNil()) return default;

                switch (reader.NextMessagePackType)
                {
                    case MessagePackType.Integer:
                        return reader.ReadInt64();
                    case MessagePackType.Float:
                        return (long)reader.ReadSingle();
                    case MessagePackType.String:
                        return reader.ReadString();
                    case MessagePackType.Unknown:
                    case MessagePackType.Nil:
                    case MessagePackType.Boolean:
                    case MessagePackType.Binary:
                    case MessagePackType.Array:
                    case MessagePackType.Map:
                    case MessagePackType.Extension:
                    default:
                        reader.Skip();
                        return default;
                }
            }

            public Link ReadLink(ref SerializedData data, string? _) => ReadLinkNullable(ref data, _) ?? default;

            public Link? ReadLinkNullable(ref SerializedData data, string? _)
            {
                ref var reader = ref data.Reader;
                if (reader.TryReadNil()) return null;
                switch (reader.NextMessagePackType)
                {
                    case MessagePackType.Integer:
                        return reader.ReadInt64();
                    case MessagePackType.Float:
                        return (long)reader.ReadSingle();
                    case MessagePackType.String:
                        return reader.ReadString();
                    case MessagePackType.Array:
                    {
                        data.Options.Security.DepthStep(ref reader);
                        var length = reader.ReadArrayHeader();
                        if (length <= 0)
                        {
                            reader.Depth--;
                            return null;
                        }

                        long? itemLong = null;
                        string? itemString = null;
                        switch (reader.NextMessagePackType)
                        {
                            case MessagePackType.Integer: itemLong = reader.ReadInt64(); break;
                            case MessagePackType.String: itemString = reader.ReadString(); break;
                            case MessagePackType.Boolean:
                            case MessagePackType.Float:
                            case MessagePackType.Binary:
                            case MessagePackType.Nil:
                            case MessagePackType.Map:
                            case MessagePackType.Array:
                            case MessagePackType.Unknown:
                            case MessagePackType.Extension:
                            default:
                                reader.Skip();
                                reader.Depth--;
                                return null;
                        }
                        length--;

                        if (length > 0)
                        {
                            if (reader.NextMessagePackType == MessagePackType.String)
                            {
                                reader.Depth--;
                                return itemLong.HasValue
                                    ? new Link(itemLong.Value, reader.ReadString())
                                    : new Link(itemString, reader.ReadString());
                            }
                            reader.Skip();
                        }

                        reader.Depth--;
                        return itemLong.HasValue ? new Link(itemLong.Value) : new Link(itemString);
                    }
                    case MessagePackType.Unknown:
                    case MessagePackType.Nil:
                    case MessagePackType.Boolean:
                    case MessagePackType.Binary:
                    case MessagePackType.Map:
                    case MessagePackType.Extension:
                    default:
                        reader.Skip();
                        return null;
                }
            }

            public TRet?[]? ReadArray<TRet>(ref SerializedData data, IDeserializeMethod.ConstructorFunc<TRet> constructor, string? name)
            {
                ref var reader = ref data.Reader;
                if (reader.TryReadNil())
                {
                    reader.Depth--;
                    return null;
                }

                data.Options.Security.DepthStep(ref reader);
                var length = reader.ReadArrayHeader();
                if (length <= 0)
                {
                    reader.Depth--;
                    return Array.Empty<TRet>();
                }

                var ret = new TRet[length];
                for (var i = 0; i < length; i++) ret[i] = constructor(ref data, this, name)!;
                reader.Depth--;
                return ret;
            }

            public IReadOnlyDictionary<TKey, TValue>? ReadMap<TKey, TValue>(ref SerializedData data, IDeserializeMethod.ConstructorFunc<TKey> keyConstructor, IDeserializeMethod.ConstructorFunc<TValue> valueConstructor, string? name)
            {
                ref var reader = ref data.Reader;
                if (reader.TryReadNil())
                {
                    reader.Depth--;
                    return null;
                }

                data.Options.Security.DepthStep(ref reader);
                var length = reader.ReadArrayHeader();
                if (length <= 1)
                {
                    reader.Depth--;
                    return new FakeDictionary<TKey, TValue>();
                }
                var ret = new Dictionary<TKey, TValue>(length);
                var i = 1;
                for (; i < length; i += 2) ret[keyConstructor(ref data, this, name)!] = valueConstructor(ref data, this, name)!;
                if (i - 1 < length) reader.Skip();
                reader.Depth--;
                return ret;
            }
        }

        [MessagePackObject]
        public readonly partial struct CommonTableCreateResult{}
        public class CommonTableCreateResultResolver : IMessagePackFormatter<CommonTableCreateResult>
        {
            public void Serialize(ref MessagePackWriter writer, CommonTableCreateResult value, MessagePackSerializerOptions options)
                => throw new NotImplementedException();

            public CommonTableCreateResult Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                if (CommonTableCreateFunction == null) return default;
                var data = new SerializedData
                {
                    Reader = reader,
                    Options = options
                };
                return CommonTableCreateFunction(ref data, new DeserializeMsgPackImpl());
            }
        }

        [MessagePackObject]
        public readonly partial struct GlobalTableCreateResult{}
        public class GlobalTableCreateResultResolver : IMessagePackFormatter<GlobalTableCreateResult>
        {
            public void Serialize(ref MessagePackWriter writer, GlobalTableCreateResult value, MessagePackSerializerOptions options)
                => throw new NotImplementedException();

            public GlobalTableCreateResult Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                if (GlobalTableCreateFunction == null) return default;
                var data = new SerializedData
                {
                    Reader = reader,
                    Options = options
                };
                return GlobalTableCreateFunction(ref data, new DeserializeMsgPackImpl{IsGlobal = true});
            }
        }
    }
}
#endif
