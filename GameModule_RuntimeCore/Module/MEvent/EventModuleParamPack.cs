using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using XD.Common;
using XD.Common.ScopeUtil;

// ReSharper disable MemberCanBePrivate.Local

namespace XD.GameModule.Module.MEvent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class EventModule
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
    }
}