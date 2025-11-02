using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XD.Common.CollectionUtil;
using XD.Common.Procedure;
using XD.Common.ScopeUtil;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ArrangeTrailingCommaInMultilineLists

namespace XD.GameModule.Module.MTick
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class TickModule : EngineModule
    {
        public delegate void TickFunc(float dt, float rdt);

        public sealed class TickFuncHandle : IPolling, IDisposableWithFlag
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            public bool IsDisposed => E.Tick == null || !E.Tick.Contains(this);
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            public void Dispose() => E.Tick?.Unregister(this);
        }

        private const int CallbackInfoPoolSize = 2048;

        #region Register

        #region 直接事件注册 (适用于更高效的场合)
        public event TickFunc? OnTickDirect;
        public event TickFunc? OnLateTickDirect;
        public event TickFunc? OnPhysicalTickDirect;
        #endregion

        /// <summary>
        /// 对象是否在遍历中
        /// </summary>
        /// <param name="polling"></param>
        /// <returns></returns>
        public bool Contains(IPolling polling)
        {
            if (_unregisterQueue.Contains(polling)) return false;
            if (_registerQueue.ContainsKey(polling)) return true;
            lock(_regInfoDict) if (_regInfoDict.ContainsKey(polling)) return true;
            return false;
        }

        /// <summary>
        /// 注销事件对象
        /// </summary>
        /// <param name="unregisterHandle"></param>
        /// <returns></returns>
        public bool Unregister<T>(T? unregisterHandle) where T : class, IPolling
        {
            if (unregisterHandle == null) return false;
            if (_unregisterQueue.Contains(unregisterHandle)) return false;
            if (_registerQueue.Remove(unregisterHandle, out var info))
            {
                if (info.DisposeCallback != null && unregisterHandle is IXDDisposable xdObj)
                    xdObj.RemoveOnDispose(info.DisposeCallback);
                return true;
            }
            lock(_regInfoDict) if (!_regInfoDict.ContainsKey(unregisterHandle)) return false;
            _unregisterQueue.Add(unregisterHandle);
            return true;
        }

        /// <summary>
        /// 注册事件对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isCheckDisposable"></param>
        /// <returns> unregisterHandle </returns>
        public T? Register<T>(T? obj, bool isCheckDisposable = true) where T : class, IPolling
        {
            if (obj == null || CheckRegisteredObjectAndRemoveUnregisterReq(obj)) return obj;
            bool ret;
            if (isCheckDisposable && obj is XDObject xdObj)
            {
                var act = (Action)(() => Unregister(obj));
                var info = new RegInfo(act, obj);
                ret = RegisterInner(info);
                if (ret) xdObj.AddOnDispose(act);
            }
            else ret = RegisterInner(new RegInfo(null, obj));
            return ret ? obj : null;
        }

        /// <summary>
        /// 延迟调用函数
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public TickFuncHandle? Delay(Action? cb, float delay = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = new RegInfo(null, obj, (_, _) =>
            {
                cb.Invoke();
                return false;
            }, RegInfo.RegType.Tick, delay, 0, 1);
            var ret = RegisterInner(info);
            return ret ? obj : null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cb"> 回调 </param>
        /// <param name="delay"> 延迟触发 </param>
        /// <param name="interval"> 调用间隔 </param>
        /// <param name="maxCallCnt"> 最大调用数量(超过时自动注销) </param>
        /// <returns> 唯一索引对象 </returns>
        public TickFuncHandle? LateTick(Action<float /*delta time*/, float /*real delta time*/>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = new RegInfo(null, obj, (dt, rdt) =>
            {
                cb(dt, rdt);
                return true;
            }, RegInfo.RegType.LateTick, delay, interval, maxCallCnt);
            var success = RegisterInner(info);
            return success ? obj : null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cb"> 回调 </param>
        /// <param name="delay"> 延迟触发 </param>
        /// <param name="interval"> 调用间隔 </param>
        /// <param name="maxCallCnt"> 最大调用数量(超过时自动注销) </param>
        /// <returns> 唯一索引对象 </returns>
        public TickFuncHandle? LateTick(Func<float /*delta time*/, float /*real delta time*/, bool>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = new RegInfo(null, obj, cb, RegInfo.RegType.LateTick, delay, interval, maxCallCnt);
            var success = RegisterInner(info);
            return success ? obj : null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cb"> 回调 </param>
        /// <param name="delay"> 延迟触发 </param>
        /// <param name="interval"> 调用间隔 </param>
        /// <param name="maxCallCnt"> 最大调用数量(超过时自动注销) </param>
        /// <returns> 唯一索引对象 </returns>
        public TickFuncHandle? PhysicalTick(Action<float /*delta time*/, float /*real delta time*/>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = new RegInfo(null, obj, (dt, rdt) =>
            {
                cb(dt, rdt);
                return true;
            }, RegInfo.RegType.PhysicalTick, delay, interval, maxCallCnt);
            var success = RegisterInner(info);
            return success ? obj : null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cb"> 回调 </param>
        /// <param name="delay"> 延迟触发 </param>
        /// <param name="interval"> 调用间隔 </param>
        /// <param name="maxCallCnt"> 最大调用数量(超过时自动注销) </param>
        /// <returns> 唯一索引对象 </returns>
        public TickFuncHandle? PhysicalTick(Func<float /*delta time*/, float /*real delta time*/, bool>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = new RegInfo(null, obj, cb, RegInfo.RegType.PhysicalTick, delay, interval, maxCallCnt);
            var success = RegisterInner(info);
            return success ? obj : null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cb"> 回调 </param>
        /// <param name="delay"> 延迟触发 </param>
        /// <param name="interval"> 调用间隔 </param>
        /// <param name="maxCallCnt"> 最大调用数量(超过时自动注销) </param>
        /// <returns> 唯一索引对象 </returns>
        public TickFuncHandle? Tick(Action<float /*delta time*/, float /*real delta time*/>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = new RegInfo(null, obj, (dt, rdt) =>
            {
                cb(dt, rdt);
                return true;
            }, RegInfo.RegType.Tick, delay, interval, maxCallCnt);
            var success = RegisterInner(info);
            return success ? obj : null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cb"> 回调 </param>
        /// <param name="delay"> 延迟触发 </param>
        /// <param name="interval"> 调用间隔 </param>
        /// <param name="maxCallCnt"> 最大调用数量(超过时自动注销) </param>
        /// <returns> 唯一索引对象 </returns>
        public TickFuncHandle? Tick(Func<float /*delta time*/, float /*real delta time*/, bool>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = new RegInfo(null, obj, cb, RegInfo.RegType.Tick, delay, interval, maxCallCnt);
            var success = RegisterInner(info);
            return success ? obj : null;
        }

        private void UnregisterInnerDelay(IPolling? obj)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (obj == null) return;

            RegInfo info;
            lock (_regInfoDict) if (!_regInfoDict.TryGetValue(obj, out info)) return;
            var id = info.Id;
            if (_instUpd.TryGetValue(id, out var instUpd))
            {
                Del(instUpd);
                _instUpd.Remove(id, out _);
            }

            if (_instFixedUpd.TryGetValue(id, out var instFixedUpd))
            {
                Del(instFixedUpd);
                _instFixedUpd.Remove(id, out _);
            }

            if (_instLateUpd.TryGetValue(id, out var instLateUpd))
            {
                Del(instLateUpd);
                _instLateUpd.Remove(id, out _);
            }

            lock (_regInfoDict) _regInfoDict.Remove(obj, out _);
        }

        private bool CheckRegisteredObjectAndRemoveUnregisterReq(IPolling? obj)
        {
            if (obj == null) return true;
            if (_unregisterQueue.Contains(obj)) _unregisterQueue.Remove(obj);
            lock(_regInfoDict) if (_regInfoDict.ContainsKey(obj)) return true;
            return _registerQueue.ContainsKey(obj);
        }

        private bool RegisterInner(in RegInfo info)
        {
            if (info.Source == null ||
                (info.Act == null &&
                 info.Tick == null &&
                 info.PhysicalTick == null &&
                 info.LateTick == null)) return false;
            return _registerQueue.TryAdd(info.Source, info);
        }

        private void RegisterInnerDelay(in RegInfo info)
        {
            if (info.Act != null)
            {
                var cbInfo = New();
                cbInfo.LoadCb(info);

                switch (info.Type)
                {
                    case RegInfo.RegType.Tick:
                        _instUpd.TryAdd(info.Id, cbInfo);
                        break;
                    case RegInfo.RegType.LateTick:
                        _instLateUpd.TryAdd(info.Id, cbInfo);
                        break;
                    case RegInfo.RegType.PhysicalTick:
                        _instFixedUpd.TryAdd(info.Id, cbInfo);
                        break;
                    case RegInfo.RegType.Custom:
                    default:
                    {
                        Del(cbInfo);
                        return;
                    }
                }
            }
            else
            {
                // ReSharper disable InvertIf
                if (info.Tick != null)
                {
                    var cbInfo = New();
                    cbInfo.LoadCb(info, RegInfo.RegType.Tick);
                    _instUpd.TryAdd(info.Id, cbInfo);
                }

                if (info.LateTick != null)
                {
                    var cbInfo = New();
                    cbInfo.LoadCb(info, RegInfo.RegType.LateTick);
                    _instLateUpd.TryAdd(info.Id, cbInfo);
                }

                if (info.PhysicalTick != null)
                {
                    var cbInfo = New();
                    cbInfo.LoadCb(info, RegInfo.RegType.PhysicalTick);
                    _instFixedUpd.TryAdd(info.Id, cbInfo);
                }
                // ReSharper restore InvertIf
            }

            lock(_regInfoDict) _regInfoDict.TryAdd(info.Source!, info);
        }

        private CbInfo New() => _infoPool.TryPop(out var v) ? v : new CbInfo();

        private void Del(CbInfo info)
        {
            info.Clear();
            if (_infoPool.Count >= CallbackInfoPoolSize) return;
            _infoPool.Push(info);
        }

        private readonly HashSet<IPolling> _unregisterQueue = new(CollectionUtil.ReferenceEqualityComparer);
        private readonly ConcurrentDictionary<IPolling, RegInfo> _registerQueue = new(CollectionUtil.ReferenceEqualityComparer);

        private readonly Dictionary<Guid, CbInfo> _instUpd = new();
        private readonly Dictionary<Guid, CbInfo> _instLateUpd = new();
        private readonly Dictionary<Guid, CbInfo> _instFixedUpd = new();

        private readonly Stack<CbInfo> _infoPool = new();
        private readonly Dictionary<IPolling, RegInfo> _regInfoDict = new(CollectionUtil.ReferenceEqualityComparer);

        private unsafe class CbInfo
        {
            // 该业务不需要多播功能, 这里直接使用函数指针, 不用多播委托
            public delegate*<CbInfo, float> GetDelay { get; private set; } = &CbFloat0;
            public delegate*<CbInfo, float> GetInterval { get; private set; } = &CbFloat0;
            public delegate*<CbInfo, long> GetMaxCallCnt { get; private set; } = &CbLong1;
            public delegate*<CbInfo, float /* dt */, float /* rdt */, bool> Tick { get; private set; } = &CbTickEmpty;

            public ulong CallCnt { get; set; }
            public float LastCallDeltaTime { get; set; }
            public IPolling? Source { get; private set; }

            public void Clear()
            {
                CallCnt = 0;
                LastCallDeltaTime = 0;

                GetDelay = &CbFloat0;
                GetInterval = &CbFloat0;
                GetMaxCallCnt = &CbLong1;
                Tick = &CbTickEmpty;

                Source = null;
                _act = null;
                _actDelay = 0;
                _actInterval = 0;
                _actMaxCallCnt = 1;
                _tickInst = null;
                _lateTickInst = null;
                _physicalTickInst = null;
            }

            public void LoadCb(in RegInfo info, RegInfo.RegType type = RegInfo.RegType.Custom)
            {
                Source = info.Source;
                if (Source == null) goto Failure;
                if (info.Type != RegInfo.RegType.Custom)
                {
                    if (info.Act == null) goto Failure;
                    Tick = &CbActTick;
                    GetDelay = &CbActDelay;
                    GetInterval = &CbActInterval;
                    GetMaxCallCnt = &CbActMaxCnt;

                    _act = info.Act;
                    _actDelay = info.Delay;
                    _actInterval = info.Interval;
                    _actMaxCallCnt = info.MaxCallCnt;
                }
                else
                {
                    GetDelay = &CbFloat0;
                    GetMaxCallCnt = &CbLong0;

                    switch (type)
                    {
                        case RegInfo.RegType.Tick:
                            if (info.Tick == null) goto Failure;
                            _tickInst = info.Tick;
                            Tick = &CbTickTick;
                            GetInterval = &CbTickInterval;
                            break;
                        case RegInfo.RegType.LateTick:
                            if (info.LateTick == null) goto Failure;
                            _lateTickInst = info.LateTick;
                            Tick = &CbLateTickTick;
                            GetInterval = &CbLateTickInterval;
                            break;
                        case RegInfo.RegType.PhysicalTick:
                            if (info.PhysicalTick == null) goto Failure;
                            _physicalTickInst = info.PhysicalTick;
                            Tick = &CbPhysicalTickTick;
                            GetInterval = &CbPhysicalTickInterval;
                            break;
                        case RegInfo.RegType.Custom:
                        default: goto Failure;
                    }
                }

                return;
                Failure:
                Tick = &CbTickEmpty;
                GetDelay = &CbFloat0;
                GetInterval = &CbFloat0;
                GetMaxCallCnt = &CbLong1;
            }

            private ITick? _tickInst;
            private ILateTicker? _lateTickInst;
            private IPhysicalTicker? _physicalTickInst;

            private long _actMaxCallCnt;
            private float _actDelay;
            private float _actInterval;
            private Func<float, float, bool>? _act;

            #region Cb

            private static long CbLong0(CbInfo _) => 0;
            private static long CbLong1(CbInfo _) => 1;
            private static float CbFloat0(CbInfo _) => 0f;
            private static bool CbTickEmpty(CbInfo _, float __, float ___) => false;

            private static float CbActInterval(CbInfo self) => self._actInterval;
            private static float CbTickInterval(CbInfo self) => self._tickInst!.TickInterval;
            private static float CbLateTickInterval(CbInfo self) => self._lateTickInst!.LateTickInterval;
            private static float CbPhysicalTickInterval(CbInfo self) => self._physicalTickInst!.PhysicalTickInterval;

            private static float CbActDelay(CbInfo self) => self._actDelay;
            private static long CbActMaxCnt(CbInfo self) => self._actMaxCallCnt;

            private static bool CbActTick(CbInfo self, float dt, float rdt) => self._act!(dt, rdt);
            private static bool CbTickTick(CbInfo self, float dt, float rdt)
            {
                self._tickInst!.OnTick(dt, rdt);
                return true;
            }

            private static bool CbLateTickTick(CbInfo self, float dt, float rdt)
            {
                self._lateTickInst!.OnLateTick(dt, rdt);
                return true;
            }

            private static bool CbPhysicalTickTick(CbInfo self, float dt, float rdt)
            {
                self._physicalTickInst!.OnPhysicalTick(dt, rdt);
                return true;
            }

            #endregion
        }

        private readonly struct RegInfo
        {
            public enum RegType : byte
            {
                // ReSharper disable once PreferConcreteValueOverDefault
                Custom = default, // 接口类型
                Tick,             // Tick 回调
                LateTick,         // LateTick 回调
                PhysicalTick      // PhysicalTick 回调
            }

            public Guid Id { get; }

            #region Interface

            public IPolling? Source { get; }
            public Action? DisposeCallback { get; }
            public ITick? Tick { get; }
            public ILateTicker? LateTick { get; }
            public IPhysicalTicker? PhysicalTick { get; }

            #endregion

            #region Act

            public RegType Type { get; }
            public float Delay { get; }
            public float Interval { get; }
            public long MaxCallCnt { get; }
            public Func<float, float, bool>? Act { get; }

            #endregion

            public RegInfo(Action? disposeCb, IPolling src, Func<float, float, bool>? act, RegType type, float delay, float interval, long maxCallCnt)
            {
                Id = Guid.NewGuid();

                Act = act;
                Type = type;
                Source = src;
                Delay = delay;
                Tick = null;
                LateTick = null;
                PhysicalTick = null;
                Interval = interval;
                MaxCallCnt = maxCallCnt;
                DisposeCallback = disposeCb;
            }

            public RegInfo(Action? disposeCallback, IPolling? source)
            {
                Id = Guid.NewGuid();

                Act = null;
                Source = source;
                Tick = null;
                LateTick = null;
                PhysicalTick = null;
                Type = RegType.Custom;
                MaxCallCnt = 0;
                Interval = 0;
                Delay = 0;
                DisposeCallback = disposeCallback;

                if (Source == null) return;
                Tick = Source as ITick;
                PhysicalTick = Source as IPhysicalTicker;
                LateTick = Source as ILateTicker;
            }
        }

        #endregion

        #region Driver

        private unsafe void OnTick(float dt, float rdt)
        {
            CheckRegister();
            CheckUnregister();

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var info in _instUpd.Values)
            {
                if ((info.CallCnt <= 0 && info.GetDelay(info) > info.LastCallDeltaTime) ||
                    info.GetInterval(info) > info.LastCallDeltaTime)
                {
                    info.LastCallDeltaTime += dt;
                    continue;
                }

                var ret = info.Tick(info, dt, rdt);
                if (info.CallCnt == ulong.MaxValue) info.CallCnt = 1;
                else info.CallCnt++;
                info.LastCallDeltaTime = 0;

                var maxCallCnt = info.GetMaxCallCnt(info);
                if (!ret || (maxCallCnt > 0 && info.CallCnt >= (ulong)maxCallCnt)) Unregister(info.Source);
            }
            OnTickDirect?.Invoke(dt, rdt);
        }

        private unsafe void OnPhysicalTick(float dt, float rdt)
        {
            CheckRegister();
            CheckUnregister();

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var info in _instFixedUpd.Values)
            {
                if ((info.CallCnt <= 0 && info.GetDelay(info) > info.LastCallDeltaTime) ||
                    info.GetInterval(info) > info.LastCallDeltaTime)
                {
                    info.LastCallDeltaTime += dt;
                    continue;
                }

                var ret = info.Tick(info, dt, rdt);
                if (info.CallCnt == ulong.MaxValue) info.CallCnt = 1;
                else info.CallCnt++;
                info.LastCallDeltaTime = 0;

                var maxCallCnt = info.GetMaxCallCnt(info);
                if (!ret || (maxCallCnt > 0 && info.CallCnt >= (ulong)maxCallCnt)) Unregister(info.Source);
            }
            OnPhysicalTickDirect?.Invoke(dt, rdt);
        }

        private unsafe void OnLateTick(float dt, float rdt)
        {
            CheckRegister();
            CheckUnregister();

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var info in _instLateUpd.Values)
            {
                if ((info.CallCnt <= 0 && info.GetDelay(info) > info.LastCallDeltaTime) ||
                    info.GetInterval(info) > info.LastCallDeltaTime)
                {
                    info.LastCallDeltaTime += dt;
                    continue;
                }

                var ret = info.Tick(info, dt, rdt);
                if (info.CallCnt == ulong.MaxValue) info.CallCnt = 1;
                else info.CallCnt++;
                info.LastCallDeltaTime = 0;

                var maxCallCnt = info.GetMaxCallCnt(info);
                if (!ret || (maxCallCnt > 0 && info.CallCnt >= (ulong)maxCallCnt)) Unregister(info.Source);
            }
            OnLateTickDirect?.Invoke(dt, rdt);
        }

        private void CheckUnregister()
        {
            if (_unregisterQueue.Count <= 0) return;
            foreach (var obj in _unregisterQueue)
                UnregisterInnerDelay(obj);
            _unregisterQueue.Clear();
        }

        private void CheckRegister()
        {
            if (_registerQueue.Count <= 0) return;
            foreach (var info in _registerQueue.Values)
                RegisterInnerDelay(info);
            _registerQueue.Clear();
        }

        #endregion

        #region Lifecycle

        internal override IProcedure DeInitProcedure() => _deinitProcedure ??= new ProcedureSync(DeInit);
        internal override IProcedure InitProcedure() => _initProcedure ??= new ProcedureSync(Init);
        internal override IProcedure ReinitProcedure() => _reinitProcedure ??= new ProcedureSync(Reinit);

        private IProcedure.RetInfo Init()
        {
            _registerQueue.Clear();
            _unregisterQueue.Clear();

            _instUpd.Clear();
            _instLateUpd.Clear();
            _instFixedUpd.Clear();
            lock (_regInfoDict) _regInfoDict.Clear();

            E.TickEvent += OnTick;
            E.LateTickEvent += OnLateTick;
            E.PhysicalTickEvent += OnPhysicalTick;

            OnTickDirect += _coroutineInterface.Tick;
            OnLateTickDirect += _coroutineInterface.LateTick;
            OnPhysicalTickDirect += _coroutineInterface.PhysicalTick;
            return IProcedure.RetInfo.Success;
        }

        private IProcedure.RetInfo Reinit()
        {
            _registerQueue.Clear();
            _unregisterQueue.Clear();

            _instUpd.Clear();
            _instLateUpd.Clear();
            _instFixedUpd.Clear();
            lock (_regInfoDict) _regInfoDict.Clear();

            E.TickEvent -= OnTick;
            E.LateTickEvent -= OnLateTick;
            E.PhysicalTickEvent -= OnPhysicalTick;
            E.TickEvent += OnTick;
            E.LateTickEvent += OnLateTick;
            E.PhysicalTickEvent += OnPhysicalTick;

            return IProcedure.RetInfo.Success;
        }

        private IProcedure.RetInfo DeInit()
        {
            E.TickEvent -= OnTick;
            E.LateTickEvent -= OnLateTick;
            E.PhysicalTickEvent -= OnPhysicalTick;
            return IProcedure.RetInfo.Success;
        }

        private IProcedure? _initProcedure;
        private IProcedure? _reinitProcedure;
        private IProcedure? _deinitProcedure;

        #endregion
    }
}