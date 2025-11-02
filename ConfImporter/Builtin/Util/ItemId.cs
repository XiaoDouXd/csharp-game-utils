#nullable enable

using System;
using System.Collections.Generic;

// ReSharper disable All
// ReSharper disable InconsistentNaming
#pragma warning disable CS8618, CS9264

// ReSharper disable once CheckNamespace
namespace ConfImporter.Builtin.Util.Gen
{
    public readonly struct Id : IEquatable<Id>, IComparable<Id>
    {
        public readonly long Num;
        private readonly string? _str;

        #region Props
        public bool IsNull => _str == null;
        public bool IsNumId => _str == string.Empty;
        public string Str => _str == string.Empty ? Num.ToString() : _str ?? string.Empty;
        #endregion

        #region equals
        public bool Equals(Id other)
        {
            if (IsNull) return other.IsNull;

            if (IsNumId != other.IsNumId) return false;
            if (IsNumId) return Num == other.Num;
            return _str == other._str;
        }
        public override bool Equals(object? obj) => obj is Id other && Equals(other);
        public override int GetHashCode() => IsNumId ? Num.GetHashCode() : _str?.GetHashCode() ?? 0;
        public static bool operator ==(Id left, Id right) => left.Equals(right);
        public static bool operator !=(Id left, Id right) => !(left == right);
        #endregion

        #region compare
        public class IdComparer : IComparer<Id> { int IComparer<Id>.Compare(Id x, Id y) => Id.Compare(x, y); }
        public static int Compare(Id x, Id y)
        {
            // null -> number -> string
            // ReSharper disable ConvertIfStatementToSwitchStatement
            if (x.IsNull && y.IsNull) return 0;
            if (x.IsNull) return 1;
            if (y.IsNull) return -1;
            return x.IsNumId switch
            {
                true when y.IsNumId => x.Num.CompareTo(y.Num),
                true => -1,
                _ => y.IsNumId ? 1 : string.Compare(x._str!, y._str!, StringComparison.Ordinal)
            };
            // ReSharper restore ConvertIfStatementToSwitchStatement
        }

        public int CompareTo(Id other) => Compare(other, this);
        public static bool operator <(Id left, Id right) => left.CompareTo(right) < 0;
        public static bool operator >(Id left, Id right) => left.CompareTo(right) > 0;
        public static bool operator <=(Id left, Id right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Id left, Id right) => left.CompareTo(right) >= 0;
        #endregion

        #region create
        public Id(long num)
        {
            Num = num;
            _str = string.Empty;
        }

        public Id(string? str)
        {
            Num = 0;
            _str = str ?? string.Empty;
        }

        public override string ToString() => (string?)this ?? string.Empty;
        public static implicit operator long(Id id) => id.IsNumId
            ? id.Num
            : long.TryParse(id._str ?? string.Empty, out var v) ? v : 0;
        public static implicit operator string?(Id id) => id.IsNumId ? id.Num.ToString() : id._str;
        public static implicit operator Id(long num) => new(num);
        public static implicit operator Id(string? str) => new(str);

        public static Id ____constructor(object? data) => new(data);
        private Id(object? data)
        {
            switch (data)
            {
                case null:
                    Num = 0;
                    _str = null;
                    return;
                case string s:
                    _str = s;
                    Num = 0;
                    return;
                default:
                    _str = string.Empty;
                    Num = Convert.ToInt64(data);
                    return;
            }
        }
        #endregion
    }
}