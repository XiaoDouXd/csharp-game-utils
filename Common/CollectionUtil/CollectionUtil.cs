using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace XD.Common.CollectionUtil
{
    public static class CollectionUtil
    {
        /// <summary>
        /// 对象引用比较器
        /// </summary>
        public static readonly IEqualityComparer<object> ReferenceEqualityComparer = new ReferenceEqualityComparerCls();
        private sealed class ReferenceEqualityComparerCls : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => obj.GetHashCode();
        }

        /// <summary>
        /// 有序插入
        /// </summary>
        /// <param name="self"></param>
        /// <param name="item"></param>
        /// <param name="comparer"></param>
        /// <typeparam name="T"></typeparam>
        public static void SortedInsert<T>(this IList<T>? self, T item, IComparer<T>? comparer)
            => self.SortedInsert(item, comparer != null ? comparer.Compare : null);

        /// <summary>
        /// 比较方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public delegate int FCompare<in T>(T x, T y);

        /// <summary>
        /// (二分插入) 有序插入, 默认 self 是有序的
        /// </summary>
        /// <param name="self"></param>
        /// <param name="item"></param>
        /// <param name="comparer"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentException"></exception>
        public static int SortedInsert<T>(this IList<T>? self, T item, FCompare<T>? comparer = null)
        {
            if (self == null) return -1;
            if (self.Count <= 0)
            {
                self.Add(item);
                return 0;
            }

            comparer ??= Comparer<T>.Default.Compare;
            int i = 0, j = self.Count - 1, ret;
            while ((ret = TryAdd(self, item, i, j, comparer)) < 0)
            {
                var p = (i + j) / 2;
                var v = self[p];
                var r = comparer(item, v);
                if (r >= 0) i = p + 1;
                else j = p;
            }
            return ret;

            static int TryAdd(IList<T> self, T item, int i, int j, FCompare<T> comparer)
            {
                if (i < j) return -1;

                var v = self[i];
                var r = comparer(item, v);
                if (r >= 0)
                {
                    self.Insert(i + 1, item);
                    return i + 1;
                }

                self.Insert(i, item);
                return i;
            }
        }

        /// <summary>
        /// 筛选方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public delegate bool FCondition<in T>(T data);

        /// <summary>
        ///
        /// </summary>
        /// <param name="self"></param>
        /// <param name="func"></param>
        /// <param name="target"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<T>? ToListWhere<T>(this IList<T>? self, FCondition<T> func, IList<T>? target = null)
        {
            if (self == null) return null;
            target ??= new List<T>();
            foreach (var v in self)
            {
                if (func != null && func(v)) continue;
                target.Add(v);
            }
            return target;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] Copy<T>(this IReadOnlyList<T>? self)
        {
            if (self is not { Count: > 0 }) return Array.Empty<T>();
            var copy = new T[self.Count];
            switch (self)
            {
                case T[] arr:
                    Array.Copy(arr, 0, copy, 0, self.Count);
                    return copy;
                case List<T> list: return list.ToArray();
                default: return self.ToArray();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Intersects<T>(this ICollection<T>? self, in ICollection<T>? other) =>
            self.Intersects<ICollection<T>, ICollection<T>, T>(other);

        /// <summary>
        ///
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static bool Intersects<T1, T2, T>(this T1? self, in T2? other)
            where T1 : ICollection<T>
            where T2 : ICollection<T>
        {
            if (other == null) return true;
            if (self == null) return true;
            if (self.Count <= 0 || other.Count <= 0) return true;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var i in other) if (self.Contains(i)) return true;
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool SetEqual<T>(this ICollection<T>? self, in ICollection<T>? other) =>
            self.Contains<ICollection<T>, ICollection<T>, T>(other) &&
            other.Contains<ICollection<T>, ICollection<T>, T>(self);

        /// <summary>
        ///
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Contains<T>(this ICollection<T>? self, in ICollection<T>? other) =>
            self.Contains<ICollection<T>, ICollection<T>, T>(other);

        /// <summary>
        ///
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static bool Contains<T1, T2, T>(this T1? self, in T2? other)
            where T1 : ICollection<T>
            where T2 : ICollection<T>
        {
            if (other == null) return false;
            if (self == null) return false;
            if (other.Count <= 0) return true;
            if (self.Count <= 0) return false;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var i in other) if (!self.Contains(i)) return false;
            return true;
        }

        /// <summary>
        /// 切片
        /// </summary>
        /// <param name="self"></param>
        /// <param name="beginIdx"></param>
        /// <param name="len"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TList"></typeparam>
        /// <returns></returns>
        public static ReadOnlyListSlice<T, TList> AsSlice<TList, T>(this TList self, int beginIdx = 0, int len = -1)
            where TList : IReadOnlyList<T> => new(self, (beginIdx, len));

        /// <summary>
        /// 转换为安全列表, 若越界则返回 defaultValue
        /// </summary>
        /// <param name="self"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SafeReadOnlyList<T> ToSafe<T>(this IReadOnlyList<T> self, T? defaultValue = default) =>
            new(self, defaultValue);

        /// <summary>
        ///
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        public static void Reverse<T>(this IList<T> self)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            for (var i = 0; i < self.Count / 2; i++) // 双指针交换元素
                (self[i], self[^(i + 1)]) = (self[^(i + 1)], self[i]);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        public static void Reverse<T>(this T[] self) => Array.Reverse(self);
    }

    public readonly struct SafeReadOnlyList<T> : IReadOnlyList<T>
    {
        public SafeReadOnlyList(IReadOnlyList<T> list, T? defaultValue) => (_defaultValue, _list) = (defaultValue!, list);
        IEnumerator IEnumerable.GetEnumerator() => _list?.GetEnumerator() ?? Array.Empty<T>().GetEnumerator();
        public IEnumerator<T> GetEnumerator() => _list?.GetEnumerator() ?? ((IReadOnlyList<T>)Array.Empty<T>()).GetEnumerator();

        public int Count => _list?.Count ?? 0;
        public T this[int index] => _list == null || index < 0 || index >= _list.Count ? _defaultValue : _list[index];

        private readonly T _defaultValue;
        private readonly IReadOnlyList<T>? _list;

        public static implicit operator SafeReadOnlyList<T>(T[] self) => new(self, default);
        public static implicit operator SafeReadOnlyList<T>(List<T> self) => new(self, default);
    }
}
