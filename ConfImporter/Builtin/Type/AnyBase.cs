#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using ConfImporter.Builtin.Util.Gen;

// ReSharper disable once CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PreferConcreteValueOverDefault

namespace ConfImporter.Builtin.Type
{
    public struct AnyBase : IEquatable<AnyBase>
    {
        public System.Type? Type => TypeCode switch
        {
            ETypeCode.Int8 => typeof(sbyte),
            ETypeCode.UInt8 => typeof(byte),
            ETypeCode.Int16 => typeof(short),
            ETypeCode.UInt16 => typeof(ushort),
            ETypeCode.Int32 => typeof(int),
            ETypeCode.UInt32 => typeof(uint),
            ETypeCode.Int64 => typeof(long),
            ETypeCode.UInt64 => typeof(ulong),
            ETypeCode.Bool => typeof(bool),
            ETypeCode.String => typeof(string),
            _ => null
        };

        public ETypeCode TypeCode { get; private set; }

        public bool HasValue => TypeCode is not ETypeCode.Null;

        public bool IsIdType => TypeCode is
            ETypeCode.Int8 or
            ETypeCode.UInt8 or
            ETypeCode.Int16 or
            ETypeCode.UInt16 or
            ETypeCode.Int32 or
            ETypeCode.UInt32 or
            ETypeCode.Int64 or
            ETypeCode.UInt64 or
            ETypeCode.String;

        public void Set()
        {
            _i = 0;
            _d = 0;
            _f = 0f;
            _s = null;
            TypeCode = ETypeCode.Null;
        }

        public AnyBase(bool v)
        {
            TypeCode = ETypeCode.Bool;
            _i = v ? 1 : 0;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public void Set(bool v)
        {
            TypeCode = ETypeCode.Bool;
            _i = v ? 1 : 0;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public AnyBase(byte v)
        {
            TypeCode = ETypeCode.UInt8;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public void Set(byte v)
        {
            TypeCode = ETypeCode.UInt8;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public AnyBase(sbyte v)
        {
            TypeCode = ETypeCode.Int8;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public void Set(sbyte v)
        {
            TypeCode = ETypeCode.Int8;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public AnyBase(short v)
        {
            TypeCode = ETypeCode.Int16;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public void Set(short v)
        {
            TypeCode = ETypeCode.Int16;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public AnyBase(ushort v)
        {
            TypeCode = ETypeCode.UInt16;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public void Set(ushort v)
        {
            TypeCode = ETypeCode.UInt16;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public AnyBase(int v)
        {
            TypeCode = ETypeCode.Int32;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public void Set(int v)
        {
            TypeCode = ETypeCode.Int32;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public AnyBase(uint v)
        {
            TypeCode = ETypeCode.UInt32;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public void Set(uint v)
        {
            TypeCode = ETypeCode.UInt32;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public AnyBase(long v)
        {
            TypeCode = ETypeCode.Int64;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public void Set(long v)
        {
            TypeCode = ETypeCode.Int64;
            _i = v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public AnyBase(ulong v)
        {
            TypeCode = ETypeCode.UInt64;
            _i = (long)v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public void Set(ulong v)
        {
            TypeCode = ETypeCode.UInt64;
            _i = (long)v;

            _d = 0;
            _f = 0f;
            _s = null;
        }

        public AnyBase(float v)
        {
            TypeCode = ETypeCode.Float32;
            _f = v;

            _d = 0;
            _i = 0;
            _s = null;
        }

        public void Set(float v)
        {
            TypeCode = ETypeCode.Float32;
            _f = v;

            _d = 0;
            _i = 0;
            _s = null;
        }

        public AnyBase(double v)
        {
            TypeCode = ETypeCode.Float64;
            _d = v;

            _i = 0;
            _f = 0f;
            _s = null;
        }

        public void Set(double v)
        {
            TypeCode = ETypeCode.Float64;
            _d = v;

            _i = 0;
            _f = 0f;
            _s = null;
        }

        public AnyBase(string? v)
        {
            TypeCode = ETypeCode.String;
            _s = v;

            _i = 0;
            _d = 0;
            _f = 0f;
        }

        public void Set(string? v)
        {
            TypeCode = ETypeCode.String;
            _s = v;

            _i = 0;
            _d = 0;
            _f = 0f;
        }

        public static explicit operator AnyBase(bool v) => new(v);
        public static explicit operator AnyBase(byte v) => new(v);
        public static explicit operator AnyBase(sbyte v) => new(v);
        public static explicit operator AnyBase(short v) => new(v);
        public static explicit operator AnyBase(ushort v) => new(v);
        public static explicit operator AnyBase(int v) => new(v);
        public static explicit operator AnyBase(uint v) => new(v);
        public static explicit operator AnyBase(long v) => new(v);
        public static explicit operator AnyBase(ulong v) => new(v);
        public static explicit operator AnyBase(string? v) => new(v);

        public static explicit operator bool(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => false,
            ETypeCode.String => self._s is "true" or "t" or "T" or "真" or "1",
            ETypeCode.Float64 => self._d != 0,
            ETypeCode.Float32 => self._f != 0f,
            _ => self._i != 0
        };

        public static explicit operator byte(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => self._s == null ? default : byte.TryParse(self._s, out var v) ? v : default,
            ETypeCode.Float64 => (byte)self._d,
            ETypeCode.Float32 => (byte)self._f,
            _ => (byte)self._i
        };

        public static explicit operator sbyte(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => self._s == null ? default : sbyte.TryParse(self._s, out var v) ? v : default,
            ETypeCode.Float64 => (sbyte)self._d,
            ETypeCode.Float32 => (sbyte)self._f,
            _ => (sbyte)self._i
        };

        public static explicit operator short(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => self._s == null ? default : short.TryParse(self._s, out var v) ? v : default,
            ETypeCode.Float64 => (short)self._d,
            ETypeCode.Float32 => (short)self._f,
            _ => (short)self._i
        };

        public static explicit operator ushort(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => self._s == null ? default : ushort.TryParse(self._s, out var v) ? v : default,
            ETypeCode.Float64 => (ushort)self._d,
            ETypeCode.Float32 => (ushort)self._f,
            _ => (ushort)self._i
        };

        public static explicit operator int(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => self._s == null ? default : int.TryParse(self._s, out var v) ? v : default,
            ETypeCode.Float64 => (int)self._d,
            ETypeCode.Float32 => (int)self._f,
            _ => (int)self._i
        };

        public static explicit operator uint(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => self._s == null ? default : uint.TryParse(self._s, out var v) ? v : default,
            ETypeCode.Float64 => (uint)self._d,
            ETypeCode.Float32 => (uint)self._f,
            _ => (uint)self._i
        };

        public static explicit operator long(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => self._s == null ? default : long.TryParse(self._s, out var v) ? v : default,
            ETypeCode.Float64 => (long)self._d,
            ETypeCode.Float32 => (long)self._f,
            _ => self._i
        };

        public static explicit operator ulong(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => self._s == null ? default : ulong.TryParse(self._s, out var v) ? v : default,
            ETypeCode.Float64 => (ulong)self._d,
            ETypeCode.Float32 => (ulong)self._f,
            _ => (ulong)self._i
        };

        public static explicit operator float(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => self._s == null ? default : float.TryParse(self._s, out var v) ? v : default,
            ETypeCode.Float64 => (float)self._d,
            ETypeCode.Float32 => self._f,
            _ => self._i
        };

        public static explicit operator double(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => self._s == null ? default : double.TryParse(self._s, out var v) ? v : default,
            ETypeCode.Float64 => self._d,
            ETypeCode.Float32 => self._f,
            _ => self._i
        };

        public static explicit operator string?(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => null,
            ETypeCode.String => self._s,
            // ReSharper disable SpecifyACultureInStringConversionExplicitly
            ETypeCode.Float32 => self._f.ToString(),
            ETypeCode.Float64 => self._d.ToString(),
            // ReSharper restore SpecifyACultureInStringConversionExplicitly
            _ => self._i.ToString()
        };

        public static explicit operator CfgUtil.Link(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => new CfgUtil.Link(self._s),
            // ReSharper disable SpecifyACultureInStringConversionExplicitly
            ETypeCode.Float32 => new CfgUtil.Link(self._f.ToString()),
            ETypeCode.Float64 => new CfgUtil.Link(self._d.ToString()),
            // ReSharper restore SpecifyACultureInStringConversionExplicitly
            ETypeCode.UInt64 => new CfgUtil.Link(self._i),
            _ => new CfgUtil.Link(self._i)
        };

        public static explicit operator CfgUtil.Id(AnyBase self) => self.TypeCode switch
        {
            ETypeCode.Null => default,
            ETypeCode.String => new CfgUtil.Id(self._s),
            // ReSharper disable SpecifyACultureInStringConversionExplicitly
            ETypeCode.Float32 => default,
            ETypeCode.Float64 => default,
            // ReSharper restore SpecifyACultureInStringConversionExplicitly
            ETypeCode.UInt64 => new CfgUtil.Id(self._i),
            _ => new CfgUtil.Id(self._i)
        };

        [Pure]
        public object? ToObject()
        {
            if (TypeCode == ETypeCode.Null) return null;
            return TypeCode switch
            {
                ETypeCode.Bool => (bool)this,
                ETypeCode.Int8 => (sbyte)this,
                ETypeCode.Int16 => (short)this,
                ETypeCode.Int32 => (int)this,
                ETypeCode.Int64 => (long)this,
                ETypeCode.UInt8 => (byte)this,
                ETypeCode.UInt16 => (ushort)this,
                ETypeCode.UInt32 => (uint)this,
                ETypeCode.UInt64 => (ulong)this,
                ETypeCode.Float32 => (float)this,
                ETypeCode.Float64 => (double)this,
                ETypeCode.String => (string?)this,
                _ => null,
            };
        }

        [Pure]
        public bool Equals(in AnyBase other)
        {
            if (other.TypeCode != TypeCode) return false;
            if (TypeCode == ETypeCode.Null) return true;
            return TypeCode switch
            {
                ETypeCode.Bool => _i == other._i,
                ETypeCode.Int8 => _i == other._i,
                ETypeCode.Int16 => _i == other._i,
                ETypeCode.Int32 => _i == other._i,
                ETypeCode.Int64 => _i == other._i,

                ETypeCode.UInt8 => _i == other._i,
                ETypeCode.UInt16 => _i == other._i,
                ETypeCode.UInt32 => _i == other._i,
                ETypeCode.UInt64 => _i == other._i,

                // ReSharper disable CompareOfFloatsByEqualityOperator
                ETypeCode.Float32 => _f == other._f,
                ETypeCode.Float64 => _d == other._d,
                // ReSharper restore CompareOfFloatsByEqualityOperator
                ETypeCode.String => _s == other._s,
                _ => true,
            };
        }

        bool IEquatable<AnyBase>.Equals(AnyBase other) => Equals(in other);
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is AnyBase other && Equals(in other);

        public static bool operator !=(AnyBase left, AnyBase right) => !(left == right);
        public static bool operator ==(AnyBase left, AnyBase right) => left.Equals(right);

        public override int GetHashCode()
        {
            return TypeCode switch
            {
                ETypeCode.Bool => _i.GetHashCode(),
                ETypeCode.Int8 => _i.GetHashCode(),
                ETypeCode.Int16 => _i.GetHashCode(),
                ETypeCode.Int32 => _i.GetHashCode(),
                ETypeCode.Int64 => _i.GetHashCode(),
                ETypeCode.UInt8 => _i.GetHashCode(),
                ETypeCode.UInt16 => _i.GetHashCode(),
                ETypeCode.UInt32 => _i.GetHashCode(),
                ETypeCode.UInt64 => _i.GetHashCode(),
                ETypeCode.Float32 => _f.GetHashCode(),
                ETypeCode.Float64 => _d.GetHashCode(),
                ETypeCode.String => _s?.GetHashCode() ?? 0,
                _ => 0,
            };
        }

        [Pure]
        public override string ToString() => (string?)this ?? string.Empty;

        [Pure]
        public string ToString(string format)=> TypeCode switch
        {
            ETypeCode.Null => string.Empty,
            ETypeCode.String => _s ?? string.Empty,
            ETypeCode.Float32 => _f.ToString(format),
            ETypeCode.Float64 => _d.ToString(format),
            _ => _i.ToString()
        };

        private long _i;
        private float _f;
        private double _d;
        private string? _s;

        public enum ETypeCode : byte
        {
            Null    = default,

            Bool    = 1,

            Int8    = 2,
            Int16   = 3,
            Int32   = 4,
            Int64   = 5,

            UInt8   = 6,
            UInt16  = 7,
            UInt32  = 8,
            UInt64  = 9,

            Float32 = 10,
            Float64 = 11,
            String  = 12
        }
    }
}