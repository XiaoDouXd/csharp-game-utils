using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace XD.Common.Event
{
    /// <summary>
    /// 强类型事件 Key. Key 同时承载 "事件 ID" 和 "参数类型列表", 使广播和订阅在编译期就能对齐签名.
    /// <para>
    /// 推荐用法: 在事件定义附近声明静态只读 Key, 例如:
    /// <code>
    /// public enum EXxxEvent { OnFoo, OnBar, }
    /// public static class XxxEventKeys
    /// {
    ///     public static readonly EventKey&lt;int, string&gt; OnFoo =
    ///         EventKey&lt;int, string&gt;.Of(EXxxEvent.OnFoo);
    /// }
    /// </code>
    /// </para>
    /// </summary>
    public readonly struct EventKey : IEquatable<EventKey>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }

        public static EventKey Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));

        public bool Equals(EventKey o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey a, EventKey b) => a.Equals(b);
        public static bool operator !=(EventKey a, EventKey b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0> : IEquatable<EventKey<T0>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0> a, EventKey<T0> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0> a, EventKey<T0> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1> : IEquatable<EventKey<T0, T1>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1> a, EventKey<T0, T1> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1> a, EventKey<T0, T1> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2> : IEquatable<EventKey<T0, T1, T2>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2> a, EventKey<T0, T1, T2> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2> a, EventKey<T0, T1, T2> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3> : IEquatable<EventKey<T0, T1, T2, T3>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3> a, EventKey<T0, T1, T2, T3> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3> a, EventKey<T0, T1, T2, T3> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4> : IEquatable<EventKey<T0, T1, T2, T3, T4>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4> a, EventKey<T0, T1, T2, T3, T4> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4> a, EventKey<T0, T1, T2, T3, T4> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4, T5> : IEquatable<EventKey<T0, T1, T2, T3, T4, T5>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4, T5> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4, T5> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4, T5> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4, T5> a, EventKey<T0, T1, T2, T3, T4, T5> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4, T5> a, EventKey<T0, T1, T2, T3, T4, T5> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4, T5, T6> : IEquatable<EventKey<T0, T1, T2, T3, T4, T5, T6>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4, T5, T6> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4, T5, T6> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4, T5, T6> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4, T5, T6> a, EventKey<T0, T1, T2, T3, T4, T5, T6> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4, T5, T6> a, EventKey<T0, T1, T2, T3, T4, T5, T6> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4, T5, T6, T7> : IEquatable<EventKey<T0, T1, T2, T3, T4, T5, T6, T7>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4, T5, T6, T7> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4, T5, T6, T7> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4, T5, T6, T7> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4, T5, T6, T7> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4, T5, T6, T7> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> : IEquatable<EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : IEquatable<EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        : IEquatable<EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
        : IEquatable<EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        : IEquatable<EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
        : IEquatable<EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        : IEquatable<EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> b) => !a.Equals(b);
    }

    public readonly struct EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
        : IEquatable<EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>
    {
        public EventDispatcher.EventIdentify Id { get; }
        public EventKey(in EventDispatcher.EventIdentify id) { Id = id; }
        public static EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Of<TEnum>(TEnum enumVal) where TEnum : unmanaged, Enum =>
            new(EventDispatcher.EventIdentify.Get(enumVal));
        public bool Equals(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> o) => o.Id == Id;
        public override bool Equals(object? obj) => obj is EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> v && Equals(v);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> b) => a.Equals(b);
        public static bool operator !=(EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> a, EventKey<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> b) => !a.Equals(b);
    }
}
