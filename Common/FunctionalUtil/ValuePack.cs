using System;
using System.Collections.Generic;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace XD.Common.FunctionalUtil
{
    public interface IReadOnlyValuePack<out T>
    {
        public T Value { get; }
        public object ValueObj { get; }
    }

    /// <summary>
    /// 值类型封包成引用类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ValuePack<T> : ValuePack, IReadOnlyValuePack<T> where T : struct
    {
        public T Value { get; set; }
        public override object ValueObj => Value;

        public ValuePack() : base(typeof(T)) {}
        public void SetValue(T val) => Value = val;

        public bool Equals(T o) => EqualityComparer<T>.Default.Equals(o, Value);
        public bool Equals(ValuePack<T> other) => Value.Equals(other.Value);
        public override bool Equals(object? obj) => obj switch
        {
            T val => Equals(val),
            ValuePack<T> pack => Equals(pack.Value),
            _ => false
        };

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => Value.GetHashCode();
        public static implicit operator T(ValuePack<T> self) => self.Value;

        public static bool operator !=(ValuePack<T> a, ValuePack<T> b) => !(a == b);
        public static bool operator ==(ValuePack<T> a, ValuePack<T> b) => EqualityComparer<ValuePack<T>>.Default.Equals(a, b);
    }

    public abstract class ValuePack
    {
        public Type Type { get; }
        public abstract object ValueObj { get; }

        protected ValuePack(Type type) => Type = type;
    }
}