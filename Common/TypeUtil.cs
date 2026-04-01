using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace XD.Common
{
    public static class TypeUtil
    {
        public static unsafe long AsLong<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            var value = sizeof(TEnum) switch
            {
                sizeof(byte) => *(byte*)&enumValue,
                sizeof(short) => *(short*)&enumValue,
                sizeof(int) => *(int*)&enumValue,
                sizeof(long) => *(long*)&enumValue,
                _ => throw new Exception("type mismatch")
            };
            return value;
        }

        public static bool IsUnmanaged(this Type t)
        {
            bool ret;
            if (CachedTypes.TryGetValue(t, out var isManaged))
                return isManaged;

            if (t.IsPrimitive || t.IsPointer || t.IsEnum)
                ret = true;
            else if (t.IsGenericType || !t.IsValueType)
                ret = false;
            else
                ret = t.GetFields(BindingFlags.Public |
                                     BindingFlags.NonPublic | BindingFlags.Instance)
                    .All(x => x.FieldType.IsUnmanaged());
            CachedTypes.Add(t, ret);
            return ret;
        }
        private static readonly Dictionary<Type, bool> CachedTypes = new();

        public static bool IsBuiltinValueTypeBox<TBox>() where TBox : IValueBox
        {
            return typeof(TBox) == typeof(U8Box) ||
                   typeof(TBox) == typeof(I8Box) ||
                   typeof(TBox) == typeof(U16Box) ||
                   typeof(TBox) == typeof(I16Box) ||
                   typeof(TBox) == typeof(U32Box) ||
                   typeof(TBox) == typeof(I32Box) ||
                   typeof(TBox) == typeof(U64Box) ||
                   typeof(TBox) == typeof(I64Box) ||
                   typeof(TBox) == typeof(F32Box) ||
                   typeof(TBox) == typeof(F64Box) ||
                   typeof(TBox) == typeof(StringBox);
        }
    }

    public interface IValueBox
    {
        public Type Type { get; }
        public void FromBool(bool value) {}
        public void FromU8(byte value) {}
        public void FromI8(sbyte value) {}
        public void FromU16(ushort value) {}
        public void FromI16(short value) {}
        public void FromU32(uint value) {}
        public void FromI32(int value) {}
        public void FromU64(ulong value) {}
        public void FromI64(long value) {}
        public void FromF32(float value) {}
        public void FromF64(double value) {}
        public void FromString(string value) {}
        public void FromObject(object value) {}
    }

    public interface IValueBox<out T> : IValueBox
    {
        public T Value { get; }
    }

    public struct ObjectBox<T> : IValueBox<T>
    {
        public Type Type => typeof(T);
        public T Value { get; private set; }
        public ObjectBox(T value) => Value = value;
        public override string ToString() => Value?.ToString() ?? string.Empty;
        public void FromObject(object value) => Value = (T)value;
    }

    public struct StringBox : IValueBox<string>
    {
        public Type Type => typeof(string);
        public StringBox() {}
        public string Value { get; private set; } = string.Empty;
        public override string ToString() => Value;
        public void FromString(string value) => Value = value;
        public void FromObject(object value) => Value = value as string ?? value.ToString();

        public void FromBool(bool value) => Value = value.ToString();
        public void FromU8(byte value) => Value = value.ToString();
        public void FromI8(sbyte value) => Value = value.ToString();
        public void FromU16(ushort value) => Value = value.ToString();
        public void FromI16(short value) => Value = value.ToString();
        public void FromU32(uint value) => Value = value.ToString();
        public void FromI32(int value) => Value = value.ToString();
        public void FromU64(ulong value) => Value = value.ToString();
        public void FromI64(long value) => Value = value.ToString();
        public void FromF32(float value) => Value = value.ToString(CultureInfo.InvariantCulture);
        public void FromF64(double value) => Value = value.ToString(CultureInfo.InvariantCulture);
    }

    public struct U8Box : IValueBox<byte>
    {
        public Type Type => typeof(byte);
        public U8Box() {}
        public byte Value { get; private set; }
        public override string ToString() => Value.ToString();
        public void FromBool(bool value) => Value = (byte)(value ? 1 : 0);
        public void FromU8(byte value) => Value = value;
        public void FromI8(sbyte value) => Value = (byte)value;
        public void FromU16(ushort value) => Value = (byte)value;
        public void FromI16(short value) => Value = (byte)value;
        public void FromU32(uint value) => Value = (byte)value;
        public void FromI32(int value) => Value = (byte)value;
        public void FromU64(ulong value) => Value = (byte)value;
        public void FromI64(long value) => Value = (byte)value;
        public void FromF32(float value) => Value = (byte)value;
        public void FromF64(double value) => Value = (byte)value;
        public void FromString(string value) => Value = byte.Parse(value);
        public void FromObject(object value) => Value = value as byte? ?? 0;
    }

    public struct I8Box : IValueBox<sbyte>
    {
        public Type Type => typeof(sbyte);
        public I8Box() {}
        public sbyte Value { get; private set; }
        public override string ToString() => Value.ToString();
        public void FromBool(bool value) => Value = (sbyte)(value ? 1 : 0);
        public void FromU8(byte value) => Value = (sbyte)value;
        public void FromI8(sbyte value) => Value = value;
        public void FromU16(ushort value) => Value = (sbyte)value;
        public void FromI16(short value) => Value = (sbyte)value;
        public void FromU32(uint value) => Value = (sbyte)value;
        public void FromI32(int value) => Value = (sbyte)value;
        public void FromU64(ulong value) => Value = (sbyte)value;
        public void FromI64(long value) => Value = (sbyte)value;
        public void FromF32(float value) => Value = (sbyte)value;
        public void FromF64(double value) => Value = (sbyte)value;
        public void FromString(string value) => Value = sbyte.Parse(value);
        public void FromObject(object value) => Value = value as sbyte? ?? 0;
    }

    public struct U16Box : IValueBox<ushort>
    {
        public Type Type => typeof(ushort);
        public U16Box() {}
        public ushort Value { get; private set; }
        public override string ToString() => Value.ToString();
        public void FromBool(bool value) => Value = (ushort)(value ? 1 : 0);
        public void FromU8(byte value) => Value = value;
        public void FromI8(sbyte value) => Value = (ushort)value;
        public void FromU16(ushort value) => Value = value;
        public void FromI16(short value) => Value = (ushort)value;
        public void FromU32(uint value) => Value = (ushort)value;
        public void FromI32(int value) => Value = (ushort)value;
        public void FromU64(ulong value) => Value = (ushort)value;
        public void FromI64(long value) => Value = (ushort)value;
        public void FromF32(float value) => Value = (ushort)value;
        public void FromF64(double value) => Value = (ushort)value;
        public void FromString(string value) => Value = ushort.Parse(value);
        public void FromObject(object value) => Value = value as ushort? ?? 0;
    }

    public struct I16Box : IValueBox<short>
    {
        public Type Type => typeof(short);
        public I16Box() {}
        public short Value { get; private set; }
        public override string ToString() => Value.ToString();
        public void FromBool(bool value) => Value = (short)(value ? 1 : 0);
        public void FromU8(byte value) => Value = value;
        public void FromI8(sbyte value) => Value = value;
        public void FromU16(ushort value) => Value = (short)value;
        public void FromI16(short value) => Value = value;
        public void FromU32(uint value) => Value = (short)value;
        public void FromI32(int value) => Value = (short)value;
        public void FromU64(ulong value) => Value = (short)value;
        public void FromI64(long value) => Value = (short)value;
        public void FromF32(float value) => Value = (short)value;
        public void FromF64(double value) => Value = (short)value;
        public void FromString(string value) => Value = short.Parse(value);
        public void FromObject(object value) => Value = value as short? ?? 0;
    }

    public struct U32Box : IValueBox<uint>
    {
        public Type Type => typeof(uint);
        public U32Box() {}
        public uint Value { get; private set; }
        public override string ToString() => Value.ToString();
        public void FromBool(bool value) => Value = (uint)(value ? 1 : 0);
        public void FromU8(byte value) => Value = value;
        public void FromI8(sbyte value) => Value = (uint)value;
        public void FromU16(ushort value) => Value = value;
        public void FromI16(short value) => Value = (uint)value;
        public void FromU32(uint value) => Value = value;
        public void FromI32(int value) => Value = (uint)value;
        public void FromU64(ulong value) => Value = (uint)value;
        public void FromI64(long value) => Value = (uint)value;
        public void FromF32(float value) => Value = (uint)value;
        public void FromF64(double value) => Value = (uint)value;
        public void FromString(string value) => Value = uint.Parse(value);
        public void FromObject(object value) => Value = value as uint? ?? 0;
    }

    public struct I32Box : IValueBox<int>
    {
        public Type Type => typeof(int);
        public I32Box() {}
        public int Value { get; private set; }
        public override string ToString() => Value.ToString();
        public void FromBool(bool value) => Value = value ? 1 : 0;
        public void FromU8(byte value) => Value = value;
        public void FromI8(sbyte value) => Value = value;
        public void FromU16(ushort value) => Value = value;
        public void FromI16(short value) => Value = value;
        public void FromU32(uint value) => Value = (int)value;
        public void FromI32(int value) => Value = value;
        public void FromU64(ulong value) => Value = (int)value;
        public void FromI64(long value) => Value = (int)value;
        public void FromF32(float value) => Value = (int)value;
        public void FromF64(double value) => Value = (int)value;
        public void FromString(string value) => Value = int.Parse(value);
        public void FromObject(object value) => Value = value as int? ?? 0;
    }

    public struct U64Box : IValueBox<ulong>
    {
        public Type Type => typeof(ulong);
        public U64Box() {}
        public ulong Value { get; private set; }
        public override string ToString() => Value.ToString();
        public void FromBool(bool value) => Value = (ulong)(value ? 1 : 0);
        public void FromU8(byte value) => Value = value;
        public void FromI8(sbyte value) => Value = (ulong)value;
        public void FromU16(ushort value) => Value = value;
        public void FromI16(short value) => Value = (ulong)value;
        public void FromU32(uint value) => Value = value;
        public void FromI32(int value) => Value = (ulong)value;
        public void FromU64(ulong value) => Value = value;
        public void FromI64(long value) => Value = (ulong)value;
        public void FromF32(float value) => Value = (ulong)value;
        public void FromF64(double value) => Value = (ulong)value;
        public void FromString(string value) => Value = ulong.Parse(value);
        public void FromObject(object value) => Value = value as ulong? ?? 0;
    }

    public struct I64Box : IValueBox<long>
    {
        public Type Type => typeof(long);
        public I64Box() {}
        public long Value { get; private set; }
        public override string ToString() => Value.ToString();
        public void FromBool(bool value) => Value = value ? 1 : 0;
        public void FromU8(byte value) => Value = value;
        public void FromI8(sbyte value) => Value = value;
        public void FromU16(ushort value) => Value = value;
        public void FromI16(short value) => Value = value;
        public void FromU32(uint value) => Value = value;
        public void FromI32(int value) => Value = value;
        public void FromU64(ulong value) => Value = (long)value;
        public void FromI64(long value) => Value = value;
        public void FromF32(float value) => Value = (long)value;
        public void FromF64(double value) => Value = (long)value;
        public void FromString(string value) => Value = long.Parse(value);
        public void FromObject(object value) => Value = value as long? ?? 0;
    }

    public struct BoolBox : IValueBox<bool>
    {
        public Type Type => typeof(bool);
        public BoolBox() {}
        public bool Value { get; private set; }
        public override string ToString() => Value.ToString();
        public void FromBool(bool value) => Value = value;
        public void FromU8(byte value) => Value = value != 0;
        public void FromI8(sbyte value) => Value = value != 0;
        public void FromU16(ushort value) => Value = value != 0;
        public void FromI16(short value) => Value = value != 0;
        public void FromU32(uint value) => Value = value != 0;
        public void FromI32(int value) => Value = value != 0;
        public void FromU64(ulong value) => Value = value != 0;
        public void FromI64(long value) => Value = value != 0;
        public void FromF32(float value) => Value = value != 0;
        public void FromF64(double value) => Value = value != 0;
        public void FromString(string value) => Value = bool.Parse(value);
        public void FromObject(object value) => Value = value as bool? ?? false;
    }

    public struct F32Box : IValueBox<float>
    {
        public Type Type => typeof(float);
        public F32Box() {}
        public float Value { get; private set; }
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        public void FromBool(bool value) => Value = value ? 1 : 0;
        public void FromU8(byte value) => Value = value;
        public void FromI8(sbyte value) => Value = value;
        public void FromU16(ushort value) => Value = value;
        public void FromI16(short value) => Value = value;
        public void FromU32(uint value) => Value = value;
        public void FromI32(int value) => Value = value;
        public void FromU64(ulong value) => Value = value;
        public void FromI64(long value) => Value = value;
        public void FromF32(float value) => Value = value;
        public void FromF64(double value) => Value = (float)value;
        public void FromString(string value) => Value = float.Parse(value);
        public void FromObject(object value) => Value = value as float? ?? 0;
    }

    public struct F64Box : IValueBox<double>
    {
        public Type Type => typeof(double);
        public F64Box() {}
        public double Value { get; private set; }
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        public void FromBool(bool value) => Value = value ? 1 : 0;
        public void FromU8(byte value) => Value = value;
        public void FromI8(sbyte value) => Value = value;
        public void FromU16(ushort value) => Value = value;
        public void FromI16(short value) => Value = value;
        public void FromU32(uint value) => Value = value;
        public void FromI32(int value) => Value = value;
        public void FromU64(ulong value) => Value = value;
        public void FromI64(long value) => Value = value;
        public void FromF32(float value) => Value = value;
        public void FromF64(double value) => Value = value;
        public void FromString(string value) => Value = double.Parse(value);
        public void FromObject(object value) => Value = value as double? ?? 0;
    }
}