using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using XD.Common.ScopeUtil;

namespace XD.Common.Event
{
    // ReSharper disable UnusedMember.Global
    public partial class EventDispatcher
    {
        public static class EventParams
        {
            public static T? Get<T>(IReadOnlyList<ParamPack> paramPacks, int idx)
            {
                if (idx >= paramPacks.Count || idx < 0) return default;
                return paramPacks[idx].GetValue<T>();
            }
        }

        /// <summary> 参数包 </summary>
        public struct ParamPack : IDisposableWithFlag
        {
            public bool IsDisposed => _type == null;
            private IntPtr _valuePtr;
            private Type? _type;
            private object? _valueObj;
            private bool _isUnmanaged;

            public static ParamPack Create<T>(T data)
            {
                var ret = new ParamPack();
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

        public int MaxTaskListCache
        {
            get => _maxTaskListCache;
            set
            {
                _maxTaskListCache = value;
                _taskListPool.Clear();
            }
        }

        private int _maxTaskListCache = 100;

        protected List<EventHandlerBase> NewTaskList() =>
            _taskListPool.TryPop(out var v) ? v : new List<EventHandlerBase>();

        protected void DelTaskList(List<EventHandlerBase> paramList)
        {
            paramList.Clear();
            if (_taskListPool.Count < _maxTaskListCache) _taskListPool.Push(paramList);
        }

        private readonly ConcurrentStack<List<EventHandlerBase>> _taskListPool = new();

        #endregion

        #region param list

        public int MaxParamListCache
        {
            get => _maxParamListCache;
            set
            {
                _maxParamListCache = value;
                _paramListPool.Clear();
            }
        }

        protected readonly List<ParamPack> EmptyParamList = new();

        private int _maxParamListCache = 100;

        protected List<ParamPack> NewParamList() => _paramListPool.TryPop(out var v) ? v : new List<ParamPack>();

        protected void DelParamList(List<ParamPack> paramList)
        {
            if (ReferenceEquals(paramList, EmptyParamList)) return;
            foreach (var param in paramList) param.Dispose(); // 释放参数包的数据
            paramList.Clear();
            if (_paramListPool.Count < _maxParamListCache) _paramListPool.Push(paramList);
        }

        protected static ParamPack NewValuePack<T>(T? obj) => ParamPack.Create(obj);

        private readonly ConcurrentStack<List<ParamPack>> _paramListPool = new();

        #endregion
    }
}