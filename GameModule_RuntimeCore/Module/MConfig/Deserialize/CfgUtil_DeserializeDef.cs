using System;
using System.Collections.Generic;

// ReSharper disable PartialTypeWithSinglePart

namespace XD.GameModule.Module.MConfig
{
    public static partial class CfgUtil
    {
        #region create delegate

        /// <summary>
        /// 普通配置表初始化回调
        /// </summary>
        public delegate CommonTableCreateResult CommonTableCreateDelegate(ref SerializedData data, IDeserializeMethod method);

        /// <summary>
        /// 全局配置表初始化回调
        /// </summary>
        public delegate GlobalTableCreateResult GlobalTableCreateDelegate(ref SerializedData data, IDeserializeMethod method);

        public readonly partial struct CommonTableCreateResult
        {
            public readonly Table[]? Value;
            public CommonTableCreateResult(Table[]? value) => Value = value;
        }

        public readonly partial struct GlobalTableCreateResult
        {
            public readonly bool Success;
            public GlobalTableCreateResult(bool success) => Success = success;
        }

        #endregion

        #region deserialize method

        /// <summary> 序列化数据, 从序列化库中拿, 根据不同的序列化库做实现 (在 CfgUtil_Deserialize{?}Impl 中) </summary>
        /// <para> 因为 C# 9.0 不支持 ref struct 继承接口, 所以这个就不考虑多态了... </para>
        public ref partial struct SerializedData {}

        /// <summary> 反序列化方法, 根据不同的序列化库做实现 </summary>
        public interface IDeserializeMethod
        {
            public delegate TRet? ConstructorFunc<out TRet>(ref SerializedData data, IDeserializeMethod handle, string? name);

            #region scope
            public long BeginScope(ref SerializedData data);
            public void EndScope(ref SerializedData data);

            public bool BeginTableScope(ref SerializedData data, Type type, string? name = null);
            public void EndTableScope(ref SerializedData data);

            public bool BeginItemScope(ref SerializedData data, Type type, string? name = null);
            public void EndItemScope(ref SerializedData data);

            public bool BeginStructScope(ref SerializedData data, Type type, string? name = null);
            public void EndStructScope(ref SerializedData data);
            #endregion

            #region reader
            public string ReadString(ref SerializedData data, string? name = null);
            public string? ReadStringNullable(ref SerializedData data, string? name = null);

            public sbyte ReadI8(ref SerializedData data, string? name = null);
            public sbyte? ReadI8Nullable(ref SerializedData data, string? name = null);

            public byte ReadU8(ref SerializedData data, string? name = null);
            public byte? ReadU8Nullable(ref SerializedData data, string? name = null);

            public short ReadI16(ref SerializedData data, string? name = null);
            public short? ReadI16Nullable(ref SerializedData data, string? name = null);

            public ushort ReadU16(ref SerializedData data, string? name = null);
            public ushort? ReadU16Nullable(ref SerializedData data, string? name = null);

            public int ReadI32(ref SerializedData data, string? name = null);
            public int? ReadI32Nullable(ref SerializedData data, string? name = null);

            public uint ReadU32(ref SerializedData data, string? name = null);
            public uint? ReadU32Nullable(ref SerializedData data, string? name = null);

            public long ReadI64(ref SerializedData data, string? name = null);
            public long? ReadI64Nullable(ref SerializedData data, string? name = null);

            public ulong ReadU64(ref SerializedData data, string? name = null);
            public ulong? ReadU64Nullable(ref SerializedData data, string? name = null);

            public float ReadF32(ref SerializedData data, string? name = null);
            public float? ReadF32Nullable(ref SerializedData data, string? name = null);

            public double ReadF64(ref SerializedData data, string? name = null);
            public double? ReadF64Nullable(ref SerializedData data, string? name = null);

            public bool ReadBool(ref SerializedData data, string? name = null);
            public bool? ReadBoolNullable(ref SerializedData data, string? name = null);

            public Id ReadId(ref SerializedData data, string? name = null);

            public Link ReadLink(ref SerializedData data, string? name = null);
            public Link? ReadLinkNullable(ref SerializedData data, string? name = null);

            public TRet?[]? ReadArray<TRet>(ref SerializedData data, ConstructorFunc<TRet> constructor, string? name = null);
            public IReadOnlyDictionary<TKey, TValue>? ReadMap<TKey, TValue>(
                ref SerializedData data,
                ConstructorFunc<TKey> keyConstructor,
                ConstructorFunc<TValue> valueConstructor,
                string? name = null);
            #endregion
        }

        public interface IConstructor<TSelf, out TRet>
            where TSelf : unmanaged, IConstructor<TSelf, TRet>
        {
            public TRet? Construct(ref SerializedData data, IDeserializeMethod method, string? name);
        }

        public interface IStructConstructor<TSelf, TRet> : IConstructor<TSelf, TRet>
            where TRet : struct
            where TSelf : unmanaged, IStructConstructor<TSelf, TRet>
        {
            public TRet? ConstructNullable(ref SerializedData data, IDeserializeMethod method, string? name);
        }

        public static class StructConstructorFuncUtils
        {
            public static TRet? Construct<TRet, TConstructor>(ref SerializedData data, IDeserializeMethod method, string? name)
                where TConstructor : unmanaged, IConstructor<TConstructor, TRet>
                => default(TConstructor).Construct(ref data, method, name);

            public static TRet? ConstructNullable<TRet, TStructConstructor>(ref SerializedData data, IDeserializeMethod method, string? name)
                where TRet : struct
                where TStructConstructor : unmanaged, IStructConstructor<TStructConstructor, TRet>
                => default(TStructConstructor).ConstructNullable(ref data, method, name);
        }

        public static class BaseConstructorFuncMethods
        {
            public static string ConstructorString(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadString(ref data, name);

            public static string? ConstructorStringNullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadStringNullable(ref data, name);

            public static sbyte ConstructorI8(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadI8(ref data, name);

            public static sbyte? ConstructorI8Nullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadI8Nullable(ref data, name);

            public static byte ConstructorU8(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadU8(ref data, name);

            public static byte? ConstructorU8Nullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadU8Nullable(ref data, name);

            public static short ConstructorI16(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadI16(ref data, name);

            public static short? ConstructorI16Nullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadI16Nullable(ref data, name);

            public static ushort ConstructorU16(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadU16(ref data, name);

            public static ushort? ConstructorU16Nullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadU16Nullable(ref data, name);

            public static int ConstructorI32(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadI32(ref data, name);

            public static int? ConstructorI32Nullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadI32Nullable(ref data, name);

            public static uint ConstructorU32(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadU32(ref data, name);

            public static uint? ConstructorU32Nullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadU32Nullable(ref data, name);

            public static long ConstructorI64(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadI64(ref data, name);

            public static long? ConstructorI64Nullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadI64Nullable(ref data, name);

            public static ulong ConstructorU64(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadU64(ref data, name);

            public static ulong? ConstructorU64Nullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadU64Nullable(ref data, name);

            public static float ConstructorF32(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadF32(ref data, name);

            public static float? ConstructorF32Nullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadF32Nullable(ref data, name);

            public static double ConstructorF64(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadF64(ref data, name);

            public static double? ConstructorF64Nullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadF64Nullable(ref data, name);

            public static bool ConstructorBool(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadBool(ref data, name);

            public static bool? ConstructorBoolNullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadBoolNullable(ref data, name);

            public static Id ConstructorId(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadId(ref data, name);

            public static Link ConstructorLink(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadLink(ref data, name);

            public static Link? ConstructorLinkNullable(ref SerializedData data, IDeserializeMethod methods, string? name = null)
                => methods.ReadLinkNullable(ref data, name);
        }

        #endregion
    }
}