using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// ReSharper disable MemberCanBePrivate.Global

namespace XD.Common.CollectionUtil
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Arr32<T> where T : unmanaged
    {
        public const byte Capacity = 32;
        public int Count => _count;

        private T _00;
        private T _01;
        private T _02;
        private T _03;
        private T _04;
        private T _05;
        private T _06;
        private T _07;
        private T _08;
        private T _09;
        private T _10;
        private T _11;
        private T _12;
        private T _13;
        private T _14;
        private T _15;
        private T _16;
        private T _17;
        private T _18;
        private T _19;
        private T _20;
        private T _21;
        private T _22;
        private T _23;
        private T _24;
        private T _25;
        private T _26;
        private T _27;
        private T _28;
        private T _29;
        private T _30;
        private T _31;
        private byte _count;

        public unsafe T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) return default;
                fixed(T* ptr = &_00) return ptr[index];
            }

            set
            {
                if (index < 0 || index >= _count) return;
                fixed (T* ptr = &_00) ptr[index] = value;
            }
        }

        public override string ToString()
        {
            var s = "{ ";
            for (var i = 0; i < _count; i++)
            {
                var ss = this[i].ToString();
                s += ss ?? "";
                if (i < _count - 1) s += ", ";
            }
            return s + '}';
        }

        public unsafe bool Push(in T item)
        {
            if (_count >= Capacity) return false;
            fixed(T* ptr = &_00) { ptr[_count++] = item; }
            return true;
        }

        public bool Pop()
        {
            if (_count <= 0) return false;
            _count--;
            return true;
        }

        public bool Pop(out T item)
        {
            if (_count <= 0)
            {
                item = default;
                return false;
            }
            item = this[_count - 1];
            _count--;
            return true;
        }

        public unsafe bool RemoveAt(int index)
        {
            if (index < 0 || index >= _count) return false;
            if (index < _count - 1)
            {
                fixed(T* ptr = &_00)
                {
                    Buffer.MemoryCopy(
                        source: ptr + index + 1,
                        destination: ptr + index,
                        destinationSizeInBytes: (_count - index) * sizeof(T),
                        sourceBytesToCopy: (_count - index - 1) * sizeof(T));
                }
            }
            _count--;
            return true;
        }

        public bool Remove(in T item)
        {
            for (var i = 0; i < _count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(this[i], item)) continue;
                RemoveAt(i);
                return true;
            }
            return false;
        }

        public ReadOnlySpan<T> AsSpan() => MemoryMarshal.CreateSpan(ref _00, _count);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Arr16<T> where T : unmanaged
    {
        public const byte Capacity = 16;
        public int Count => _count;

        private T _00;
        private T _01;
        private T _02;
        private T _03;
        private T _04;
        private T _05;
        private T _06;
        private T _07;
        private T _08;
        private T _09;
        private T _10;
        private T _11;
        private T _12;
        private T _13;
        private T _14;
        private T _15;
        private byte _count;

        public unsafe T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) return default;
                fixed(T* ptr = &_00) return ptr[index];
            }

            set
            {
                if (index < 0 || index >= _count) return;
                fixed (T* ptr = &_00) ptr[index] = value;
            }
        }

        public override string ToString()
        {
            var s = "{ ";
            for (var i = 0; i < _count; i++)
            {
                var ss = this[i].ToString();
                s += ss ?? "";
                if (i < _count - 1) s += ", ";
            }
            return s + '}';
        }

        public unsafe bool Push(in T item)
        {
            if (_count >= Capacity) return false;
            fixed(T* ptr = &_00) { ptr[_count++] = item; }
            return true;
        }

        public bool Pop()
        {
            if (_count <= 0) return false;
            _count--;
            return true;
        }

        public bool Pop(out T item)
        {
            if (_count <= 0)
            {
                item = default;
                return false;
            }
            item = this[_count - 1];
            _count--;
            return true;
        }

        public unsafe bool RemoveAt(int index)
        {
            if (index < 0 || index >= _count) return false;
            if (index < _count - 1)
            {
                fixed(T* ptr = &_00)
                {
                    Buffer.MemoryCopy(
                        source: ptr + index + 1,
                        destination: ptr + index,
                        destinationSizeInBytes: (_count - index) * sizeof(T),
                        sourceBytesToCopy: (_count - index - 1) * sizeof(T));
                }
            }
            _count--;
            return true;
        }

        public bool Remove(in T item)
        {
            for (var i = 0; i < _count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(this[i], item)) continue;
                RemoveAt(i);
                return true;
            }
            return false;
        }

        public ReadOnlySpan<T> AsSpan() => MemoryMarshal.CreateSpan(ref _00, _count);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Arr8<T> where T : unmanaged
    {
        public const byte Capacity = 8;
        public int Count => _count;

        private T _0;
        private T _1;
        private T _2;
        private T _3;
        private T _4;
        private T _5;
        private T _6;
        private T _7;
        private byte _count;

        public unsafe T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) return default;
                fixed(T* ptr = &_0) return ptr[index];
            }

            set
            {
                if (index < 0 || index >= _count) return;
                fixed (T* ptr = &_0) ptr[index] = value;
            }
        }

        public override string ToString()
        {
            var s = "{ ";
            for (var i = 0; i < _count; i++)
            {
                var ss = this[i].ToString();
                s += ss ?? "";
                if (i < _count - 1) s += ", ";
            }
            return s + '}';
        }

        public unsafe bool Push(in T item)
        {
            if (_count >= Capacity) return false;
            fixed(T* ptr = &_0) { ptr[_count++] = item; }
            return true;
        }

        public bool Pop()
        {
            if (_count <= 0) return false;
            _count--;
            return true;
        }

        public bool Pop(out T item)
        {
            if (_count <= 0)
            {
                item = default;
                return false;
            }
            item = this[_count - 1];
            _count--;
            return true;
        }

        public unsafe bool RemoveAt(int index)
        {
            if (index < 0 || index >= _count) return false;
            if (index < _count - 1)
            {
                fixed(T* ptr = &_0)
                {
                    Buffer.MemoryCopy(
                        source: ptr + index + 1,
                        destination: ptr + index,
                        destinationSizeInBytes: (_count - index) * sizeof(T),
                        sourceBytesToCopy: (_count - index - 1) * sizeof(T));
                }
            }
            _count--;
            return true;
        }

        public bool Remove(in T item)
        {
            for (var i = 0; i < _count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(this[i], item)) continue;
                RemoveAt(i);
                return true;
            }
            return false;
        }

        public ReadOnlySpan<T> AsSpan() => MemoryMarshal.CreateSpan(ref _0, _count);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Que8<T> where T : unmanaged
    {
        public const sbyte Capacity = 8;

        private T _0;
        private T _1;
        private T _2;
        private T _3;
        private T _4;
        private T _5;
        private T _6;
        private T _7;

        private sbyte _front;
        private sbyte _count;

        public int Count => _count;
        public bool IsEmpty => _count == 0;
        public bool IsFull => _count == Capacity;

        public unsafe bool Enqueue(in T item)
        {
            if (IsFull) return false;
            var index = (_front + _count) & (Capacity - 1); // 取模
            fixed(T* ptr = &_0) { ptr[index] = item; }
            _count++;
            return true;
        }

        public unsafe T Dequeue()
        {
            if (IsEmpty) return default;
            fixed(T* ptr = &_0)
            {
                var item = ptr[_front];
                _front = (sbyte)((_front + 1) & (Capacity - 1)); // 取模
                _count--;
                return item;
            }
        }

        public unsafe T Peek()
        {
            if (IsEmpty) return default;
            fixed(T* ptr = &_0) { return ptr[_front]; }
        }

        public void Clear()
        {
            _front = 0;
            _count = 0;
        }

        public unsafe T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) return default;
                var physicalIndex = (_front + index) & (Capacity - 1);
                fixed(T* ptr = &_0) { return ptr[physicalIndex]; }
            }
        }
    }
}