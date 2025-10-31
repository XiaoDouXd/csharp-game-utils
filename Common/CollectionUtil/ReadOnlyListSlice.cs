using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace XD.Common.CollectionUtil
{
    /// <summary>
    /// 列表切片
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TList"></typeparam>
    public struct ReadOnlyListSlice<T, TList> : IReadOnlyList<T>, IEquatable<ReadOnlyListSlice<T, TList>>
        where TList : IReadOnlyList<T>
    {
        public TList Source { get; set; }

        public int Count => Source == null
            ? 0
            : _slice.length < 0
                ? Source.Count - _slice.beginIdx
                : Math.Max(Math.Min(_slice.length, Source.Count - _slice.beginIdx), 0);

        public int BeginIdx
        {
            get => _slice.beginIdx;
            set => _slice.beginIdx = value;
        }

        public int Length
        {
            get => _slice.length;
            set => _slice.length = value;
        }

        public (int beginIdx, int length) Slice
        {
            get => _slice;
            set => _slice = (Math.Max(value.beginIdx, 0), value.length);
        }

        public T this[int index]
        {
            get
            {
                if (Source == null) throw new IndexOutOfRangeException("try to index from a null list");
                if ((_slice.length >= 0 && index >= _slice.length) || index < 0)
                    throw new IndexOutOfRangeException("index must be >= 0 and < this.Count");
                return Source[index + _slice.beginIdx];
            }
        }

        public ReadOnlyListSlice(TList list, (int beginIdx, int length) slice = default)
        {
            Source = list;
            _slice = slice;
        }

        public bool TryGetValue(int index, out T? value)
        {
            if (Source == null)
            {
                value = default;
                return false;
            }

            if ((_slice.length >= 0 && index >= _slice.length) || index < 0)
            {
                value = default;
                return false;
            }

            var realIndex = index + _slice.beginIdx;
            if (realIndex >= Source.Count)
            {
                value = default;
                return false;
            }

            value = Source[realIndex];
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; _slice.length < 0 || i < _slice.length; i++)
            {
                if (!TryGetValue(i, out var v)) yield break;
                yield return v!;
            }
        }

        public T[] ToArray()
        {
            if (Source is not {Count: > 0}) return Array.Empty<T>();
            var cnt = Count;
            var arr = new T[cnt];
            for (var i = _slice.beginIdx; i < cnt; i++) arr[i] = Source[i + _slice.beginIdx];
            return arr;
        }

        public List<T> ToList()
        {
            if (Source is not {Count: > 0}) return new List<T>();
            var cnt = Count;
            var l = new List<T>(cnt);
            for (var i = _slice.beginIdx; i < cnt; i++) l.Add(Source[i + _slice.beginIdx]);
            return l;
        }

        public bool Equals(ReadOnlyListSlice<T, TList> o)
            => ReferenceEquals(o.Source, Source) &&
               o._slice.beginIdx == _slice.beginIdx &&
               ((o._slice.length < 0 && _slice.length < 0) || o._slice.length == _slice.length);
        public override bool Equals(object? obj) => obj is ReadOnlyListSlice<T, TList> list && Equals(list);
        public override int GetHashCode() => HashCode.Combine(_slice.beginIdx, _slice.length, Source);

        public static bool operator ==(ReadOnlyListSlice<T, TList> a, ReadOnlyListSlice<T, TList> b) => a.Equals(b);
        public static bool operator !=(ReadOnlyListSlice<T, TList> a, ReadOnlyListSlice<T, TList> b) => !(a == b);

        private (int beginIdx, int length) _slice;
    }

    public readonly struct SingleEnumerable<T> : IEnumerable<T>
    {
        public SingleEnumerable(T value) => _value = value;

        public SingleEnumerator GetEnumerator() => new(_value);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        public struct SingleEnumerator : IEnumerator<T>
        {
            public SingleEnumerator(T value) => _value = value;
            public bool MoveNext()
            {
                if (_idx != 0) return false;
                return (_idx += 1) == 1;
            }
            public void Reset() => _idx = 0;

            public T Current => _idx == 1 ? _value! : default!;
            object? IEnumerator.Current => Current;
            public void Dispose()
            {
                _idx = -1;
                _value = default!;
            }

            private T _value;
            private sbyte _idx = 0;
        }

        private readonly T _value;
    }
}