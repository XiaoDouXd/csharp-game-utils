using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using XD.Common.ScopeUtil;

namespace XD.Common.Event
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable VirtualMemberNeverOverridden.Global
    public partial class EventDispatcher
    {
        protected virtual T? UnpackParam<T>(IReadOnlyList<ParamPack> paramPacks, int idx)
        {
            if (idx >= paramPacks.Count || idx < 0) return default;
            return paramPacks[idx].GetValue<T>();
        }
        protected virtual ParamPack PackParam<T>(T? obj) => ParamPack.Create(obj);
        protected virtual void DisposeParamPack(ref ParamPack paramPack) => paramPack.Dispose();

        /// <summary> 参数包 </summary>
        public struct ParamPack : IDisposableWithFlag
        {
            public bool IsDisposed => _type == null;
            public string ExtInfo => _extInfo ?? string.Empty;

            private IntPtr _valuePtr;
            private bool _isUnmanaged;

            private Type? _type;
            private object? _valueObj;
            private string? _extInfo;

            public static ParamPack Create<T>(T data, string? extInfo = null)
            {
                var ret = new ParamPack();
                ret._extInfo = extInfo;
                if (data == null) return ret;

                // 确定是否为非托管类型
                ret._isUnmanaged = (ret._type = typeof(T)).IsUnmanaged();
                if (ret._isUnmanaged)
                {   // 若为非托管类型则申请一块内存, 其生命周期交由 Pack 管理
                    ret._valuePtr = Marshal.AllocHGlobal(Marshal.SizeOf(data));
                    Marshal.StructureToPtr(data, ret._valuePtr, false);
                }
                else ret._valueObj = data;
                return ret;
            }

            [Pure] public T GetValue<T>()
            {
                if (IsDisposed) return default!;
                if (!_isUnmanaged) return _valueObj is T obj ? obj : default!;

                // 非托管类型从指针获取数据
                if (_valuePtr == IntPtr.Zero) return default!;
                return typeof(T) == _type ? Marshal.PtrToStructure<T>(_valuePtr) : default!;
            }

            public void Dispose()
            {
                if (IsDisposed) return;
                _type = null;
                _isUnmanaged = false;

                // 手动释放申请的内存
                if (_valuePtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_valuePtr);
                    _valuePtr = IntPtr.Zero;
                }
                _valueObj = null;
            }
        }

        #region task list

        public static int MaxTaskListCache
        {
            get => _maxTaskListCache;
            set
            {
                _maxTaskListCache = value;
                TaskListPool.Clear();
            }
        }

        private static int _maxTaskListCache = 100;

        protected static List<EventHandlerBase> NewTaskList() =>
            TaskListPool.TryPop(out var v) ? v : new List<EventHandlerBase>();

        protected static void DelTaskList(List<EventHandlerBase> paramList)
        {
            paramList.Clear();
            if (TaskListPool.Count < _maxTaskListCache) TaskListPool.Push(paramList);
        }

        private static readonly ConcurrentStack<List<EventHandlerBase>> TaskListPool = new();

        #endregion

        #region param list

        public static int MaxParamListCache
        {
            get => _maxParamListCache;
            set
            {
                _maxParamListCache = value;
                ParamListPool.Clear();
            }
        }

        protected static readonly List<ParamPack> EmptyParamList = new();

        private static int _maxParamListCache = 100;

        protected static List<ParamPack> NewParamList() => ParamListPool.TryPop(out var v) ? v : new List<ParamPack>();

        protected void DelParamList(List<ParamPack> paramList)
        {
            if (ReferenceEquals(paramList, EmptyParamList)) return;
            foreach (var pack in paramList)
            {
                var packCopy = pack;
                DisposeParamPack(ref packCopy); // 释放参数包的数据
            }
            paramList.Clear();
            if (ParamListPool.Count < _maxParamListCache) ParamListPool.Push(paramList);
        }

        private static readonly ConcurrentStack<List<ParamPack>> ParamListPool = new();

        #endregion
    }
}