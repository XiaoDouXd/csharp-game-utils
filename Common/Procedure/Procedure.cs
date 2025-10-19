#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace XD.Common.Procedure
{
    /// <summary>
    /// 程序流
    /// </summary>
    public interface IProcedure
    {
        public enum EndType : byte
        {
            Success = 0,
            Abort,
            Warning
        }

        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool Running { get; }

        /// <summary>
        /// 执行进度 [0, 1]
        /// </summary>
        public float Process { get; }

        /// <summary>
        ///
        /// </summary>
        public RetInfo Init();

        /// <summary>
        /// 运行程序流
        /// </summary>
        public void Do();

        /// <summary>
        /// 中断程序流
        /// </summary>
        public void Abort();

        /// <summary>
        /// 程序流运行完成回调
        /// </summary>
        public event Action<IProcedure, EndType, Exception?>? OnEnd;

        public readonly struct RetInfo
        {
            public EndType EndType { get; }
            public Exception? Exception { get; }

            public static readonly RetInfo Success = new(EndType.Success);

            public RetInfo(EndType endType, Exception? exception = null)
            {
                EndType = endType;
                Exception = exception;
            }
            public static implicit operator RetInfo((EndType, Exception?) info) => new(info.Item1, info.Item2);
        }
    }

    /// <summary>
    /// 同步程序流
    /// </summary>
    public class ProcedureSync : IProcedure
    {
        public bool Running { get; private set; }
        public float Process { get; private set; }

        public ProcedureSync() : this(null) {}
        public ProcedureSync(Func<IProcedure.RetInfo>? onDo) => _do = onDo;

        public IProcedure.RetInfo Init()
        {
            Process = 0;
            return IProcedure.RetInfo.Success;
        }

        public void Do()
        {
            Process = 0;
            Running = true;
            var v = _do?.Invoke() ?? (IProcedure.EndType.Success, null);
            OnEnd?.Invoke(this, v.EndType, v.Exception);
            Process = 1;
            Running = false;
        }

        public void Abort() {}

        public event Action<IProcedure, IProcedure.EndType, Exception?>? OnEnd;

        private readonly Func<IProcedure.RetInfo>? _do;
    }

    /// <summary>
    /// 异步程序流 (使用 Task)
    /// </summary>
    public class ProcedureAsync : IProcedure
    {
        public bool Running { get; private set; }
        public float Process { get; set; }

        public ProcedureAsync() : this((Func<CancellationToken, IProcedure.RetInfo>?)null) {}
        public ProcedureAsync(Func<CancellationToken, IProcedure.RetInfo>? onDo) => _do = onDo;
        public ProcedureAsync(Func<CancellationToken, Task<IProcedure.RetInfo>>? onDo) => _doTask = onDo;

        public IProcedure.RetInfo Init()
        {
            if (_do == null) return IProcedure.RetInfo.Success;
            Abort();
            return IProcedure.RetInfo.Success;
        }

        // ReSharper disable once AsyncVoidMethod
        public async void Do()
        {
            if (_do == null && _doTask == null) return;
            if (_task != null) _cts?.Cancel();

            Running = true;
            _cts = new CancellationTokenSource();
            var task = _task = _do != null ? Task.Run(OnDoInner) : OnDoTaskInner();
            var ret = await task;
            if (task != _task) return;
            if (_task.IsFaulted)
            {
                OnEnd?.Invoke(this, IProcedure.EndType.Abort, _task.Exception);
                return;
            }
            OnEnd?.Invoke(this, ret.EndType, ret.Exception);
            Running = false;
        }

        public void Abort()
        {
            _cts?.Cancel();
            _cts = null;
            _task = null;
            Process = 0;
        }

        private IProcedure.RetInfo OnDoInner()
        {
            if (_do == null) return (IProcedure.EndType.Abort, new NullReferenceException("try to run null procedure"));
            if (_cts != null) return _do.Invoke(_cts.Token);

            var r = (IProcedure.EndType.Abort, new ArgumentNullException(nameof(_cts)));
            OnEnd?.Invoke(this, r.Abort, r.Item2);
            return r;
        }

        private async Task<IProcedure.RetInfo> OnDoTaskInner()
        {
            if (_doTask == null) return (IProcedure.EndType.Abort, new NullReferenceException("try to run null procedure"));
            if (_cts != null) return await _doTask.Invoke(_cts.Token);

            var r = (IProcedure.EndType.Abort, new ArgumentNullException(nameof(_cts)));
            OnEnd?.Invoke(this, r.Abort, r.Item2);
            return r;
        }

        public event Action<IProcedure, IProcedure.EndType, Exception?>? OnEnd;

        private Task<IProcedure.RetInfo>? _task;
        private CancellationTokenSource? _cts;
        private readonly Func<CancellationToken, IProcedure.RetInfo>? _do;
        private readonly Func<CancellationToken, Task<IProcedure.RetInfo>>? _doTask;
    }
}