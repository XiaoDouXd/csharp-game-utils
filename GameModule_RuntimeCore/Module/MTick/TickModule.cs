using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XD.Common.Log;
using XD.Common.Procedure;
using XD.Common.ScopeUtil;

// ReSharper disable InvertIf
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ArrangeTrailingCommaInMultilineLists

namespace XD.GameModule.Module.MTick
{
    /// <summary>
    /// Tick 驱动模块.
    /// <para/>
    /// 线程模型:
    ///   - <see cref="Register{T}"/> / <see cref="Unregister{T}"/> / <see cref="Contains"/>
    ///     / <see cref="Tick(System.Action{float,float},float,float,long)"/>
    ///     / <see cref="LateTick(System.Action{float,float},float,float,long)"/>
    ///     / <see cref="PhysicalTick(System.Action{float,float},float,float,long)"/>
    ///     / <see cref="Delay"/>
    ///     可在任意线程调用, 通过并发队列排到主线程处理.
    ///   - <c>OnTick / OnLateTick / OnPhysicalTick</c> 由引擎主线程驱动,
    ///     所有权威状态 (<c>_regState</c>, <c>_instUpd/_instLateUpd/_instFixedUpd</c>) 只被主线程读写.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class TickModule : EngineModule
    {
        public delegate void TickFunc(float dt, float rdt);

        public sealed class TickFuncHandle : XDDisposableObjectBase, IPolling
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            public new bool IsDisposed => E.Tick == null || !E.Tick.Contains(this) || base.IsDisposed;
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            public override void Dispose()
            {
                if (!IsDisposed) E.Tick?.Unregister(this);

                if (base.IsDisposed) return;
                try { OnDisposed(); }
                finally { GC.SuppressFinalize(this); }
            }
        }

        private const int CallbackInfoPoolSize = 2048;

        #region Register

        #region 直接事件注册 (适用于更高效的场合)
        /// <summary> 每个订阅者被独立 try/catch, 单个订阅者异常不会中断整链. </summary>
        public event TickFunc? OnTickDirect;
        public event TickFunc? OnLateTickDirect;
        public event TickFunc? OnPhysicalTickDirect;
        #endregion

        /// <summary>
        /// 对象是否已注册 (可跨线程调用).
        /// 只要 <c>Register</c> 返回成功, 后续 <c>Contains</c> 就会返回 true,
        /// 直到 <c>Unregister</c> 并经过主线程一次 flush 之后.
        /// </summary>
        public bool Contains(IPolling? polling) => polling != null && _registeredObjects.ContainsKey(polling);

        /// <summary>
        /// 注销事件对象 (可跨线程调用).
        /// </summary>
        public bool Unregister<T>(T? unregisterHandle) where T : class, IPolling
        {
            if (unregisterHandle == null) return false;

            // 跨线程状态机:
            //   Registered(1) --Unregister--> PendingUnregister(2) --flush--> removed
            //   PendingRegister(3) --Unregister--> removed (直接从 _registeredObjects 删, 主线程 flush 时会发现对象缺失跳过实装)
            while (true)
            {
                if (!_registeredObjects.TryGetValue(unregisterHandle, out var state)) return false;
                switch (state)
                {
                    case StateRegistered:
                        if (!_registeredObjects.TryUpdate(unregisterHandle, StatePendingUnregister, StateRegistered))
                            continue; // CAS 失败重试
                        _unregisterQueue.Enqueue(unregisterHandle);
                        return true;
                    case StatePendingRegister:
                        // 尚未实装, 直接撤销. netstandard2.1 无 TryRemove(KeyValuePair) 重载,
                        // 用 lock 保护 "CAS 式移除".
                        lock (_registeredObjects)
                        {
                            if (!_registeredObjects.TryGetValue(unregisterHandle, out var cur) ||
                                cur != StatePendingRegister) continue;
                            _registeredObjects.TryRemove(unregisterHandle, out _);
                        }
                        // 注意: 仍然要推入 unregisterQueue, 让主线程清理 DisposeCallback 订阅 + _regInfoDict 中可能已被 flush 的条目.
                        _unregisterQueue.Enqueue(unregisterHandle);
                        return true;
                    default: return false;
                }
            }
        }

        /// <summary>
        /// 注册事件对象 (可跨线程调用).
        /// </summary>
        /// <returns> 注册成功返回 obj 自身, 失败返回 null. </returns>
        public T? Register<T>(T? obj, bool isCheckDisposable = true) where T : class, IPolling
        {
            if (obj == null) return null;

            // 若已经处于 registered / pending-register 状态, 直接幂等返回.
            if (_registeredObjects.TryGetValue(obj, out var curState))
            {
                if (curState is StateRegistered or StatePendingRegister) return obj;
                // PendingUnregister: 还在注销队列里, 在该对象真正被移除前, 我们直接切回 Registered 状态,
                // 这样主线程 flush 时 _unregisterQueue 里的条目会因状态不再是 PendingUnregister 而被跳过.
                if (curState == StatePendingUnregister &&
                    _registeredObjects.TryUpdate(obj, StateRegistered, StatePendingUnregister))
                    return obj;
            }

            Action? disposeCb = null;
            if (isCheckDisposable && obj is XDObject xdObj)
            {
                // 绑定 Dispose 自动 Unregister (xdObj.AddOnDispose 是线程安全的, 见 XDDisposableObjectBase).
                disposeCb = () => Unregister(obj);
                xdObj.AddOnDispose(disposeCb);
            }

            var info = RegInfo.CreateInterface(obj, disposeCb);
            if (!info.IsValid ||
                !_registeredObjects.TryAdd(obj, StatePendingRegister))
            {
                // 并发导致注册失败: 卸掉刚订阅的 Dispose 回调, 避免 Dispose 时调到一个永不生效的 Unregister.
                if (disposeCb != null && obj is XDObject xdObj2) xdObj2.RemoveOnDispose(disposeCb);
                return null;
            }

            _registerQueue.Enqueue(info);
            return obj;
        }

        /// <summary>
        /// 延迟调用函数 (可跨线程调用).
        /// </summary>
        public TickFuncHandle? Delay(Action? cb, float delay = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = RegInfo.CreateAct(obj, (_, _) =>
            {
                cb.Invoke();
                return false;
            }, RegKind.Tick, delay, 0, 1);
            return RegisterActInternal(obj, info);
        }

        public TickFuncHandle? LateTick(Action<float, float>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = RegInfo.CreateAct(obj, (dt, rdt) => { cb(dt, rdt); return true; },
                RegKind.LateTick, delay, interval, maxCallCnt);
            return RegisterActInternal(obj, info);
        }

        public TickFuncHandle? LateTick(Func<float, float, bool>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = RegInfo.CreateAct(obj, cb, RegKind.LateTick, delay, interval, maxCallCnt);
            return RegisterActInternal(obj, info);
        }

        public TickFuncHandle? PhysicalTick(Action<float, float>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = RegInfo.CreateAct(obj, (dt, rdt) => { cb(dt, rdt); return true; },
                RegKind.PhysicalTick, delay, interval, maxCallCnt);
            return RegisterActInternal(obj, info);
        }

        public TickFuncHandle? PhysicalTick(Func<float, float, bool>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = RegInfo.CreateAct(obj, cb, RegKind.PhysicalTick, delay, interval, maxCallCnt);
            return RegisterActInternal(obj, info);
        }

        public TickFuncHandle? Tick(Action<float, float>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = RegInfo.CreateAct(obj, (dt, rdt) => { cb(dt, rdt); return true; },
                RegKind.Tick, delay, interval, maxCallCnt);
            return RegisterActInternal(obj, info);
        }

        public TickFuncHandle? Tick(Func<float, float, bool>? cb, float delay = 0, float interval = 0, long maxCallCnt = 0)
        {
            if (cb == null) return null;
            var obj = new TickFuncHandle();
            var info = RegInfo.CreateAct(obj, cb, RegKind.Tick, delay, interval, maxCallCnt);
            return RegisterActInternal(obj, info);
        }

        private TickFuncHandle? RegisterActInternal(TickFuncHandle obj, in RegInfo info)
        {
            if (!info.IsValid) return null;
            if (!_registeredObjects.TryAdd(obj, StatePendingRegister)) return null;
            _registerQueue.Enqueue(info);
            return obj;
        }

        #endregion

        #region Main-thread state (主线程独占)

        // 权威表 & 实装表都只在主线程 (CheckRegister/CheckUnregister/OnTick/On...Tick 内) 读写.
        private readonly Dictionary<IPolling, CbInfo?[]> _regInfoDict = new(ReferenceComparer.Instance);
        private readonly LinkedList<CbInfo> _instUpd = new();
        private readonly LinkedList<CbInfo> _instLateUpd = new();
        private readonly LinkedList<CbInfo> _instFixedUpd = new();

        private readonly Stack<CbInfo> _infoPool = new();

        #endregion

        #region Cross-thread state (跨线程)

        private const byte StateRegistered = 1;
        private const byte StatePendingUnregister = 2;
        private const byte StatePendingRegister = 3;

        // 对外可见的注册态字典, 支持跨线程 Contains / Register / Unregister 的 CAS.
        private readonly ConcurrentDictionary<IPolling, byte> _registeredObjects = new(ReferenceComparer.Instance);

        // 生产者 (任意线程) -> 消费者 (主线程) 的队列.
        private readonly ConcurrentQueue<RegInfo> _registerQueue = new();
        private readonly ConcurrentQueue<IPolling> _unregisterQueue = new();

        #endregion

        #region Flush (主线程)

        private void CheckRegister()
        {
            while (_registerQueue.TryDequeue(out var info))
            {
                var src = info.Source;
                if (src == null) continue;

                // 状态检查: 对象可能在入队之后又被 Unregister, 此时不应实装.
                if (!_registeredObjects.TryGetValue(src, out var state) ||
                    state == StatePendingUnregister)
                {
                    // 对象被提前撤销; 清除可能的 dispose 回调.
                    UnbindDisposeCallback(info);
                    continue;
                }

                // 实装.
                var cbInfos = RegisterInnerDelay(info);
                if (cbInfos != null)
                {
                    _regInfoDict[src] = cbInfos;
                    // 尝试切换到 Registered (若中途被 Unregister 再 Register 又改过, 不会有问题).
                    _registeredObjects.TryUpdate(src, StateRegistered, StatePendingRegister);
                }
                else
                {
                    // 实装失败, 回滚注册态. 只有仍处于 PendingRegister 才能安全移除.
                    lock (_registeredObjects)
                    {
                        if (_registeredObjects.TryGetValue(src, out var cur) && cur == StatePendingRegister)
                            _registeredObjects.TryRemove(src, out _);
                    }
                    UnbindDisposeCallback(info);
                }
            }
        }

        private void CheckUnregister()
        {
            while (_unregisterQueue.TryDequeue(out var obj))
            {
                // 只有状态确为 PendingUnregister 时才真的卸载. 若期间又 Register 回来,
                // 状态会是 Registered 或 PendingRegister, 此时直接跳过.
                if (!_registeredObjects.TryGetValue(obj, out var state)) continue;
                if (state != StatePendingUnregister)
                {
                    // 对应 Unregister 路径: 若是 PendingRegister -> removed 路径, 需要清理 _regInfoDict (若已被 flush).
                    if (state == StateRegistered) continue;
                    UnregisterInnerDelay(obj, keepCallback: true);
                    continue;
                }

                bool removed;
                lock (_registeredObjects)
                {
                    if (_registeredObjects.TryGetValue(obj, out var cur) && cur == StatePendingUnregister)
                    {
                        _registeredObjects.TryRemove(obj, out _);
                        removed = true;
                    }
                    else removed = false;
                }
                if (removed) UnregisterInnerDelay(obj, keepCallback: false);
            }
        }

        private void UnregisterInnerDelay(IPolling obj, bool keepCallback)
        {
            if (!_regInfoDict.Remove(obj, out var cbInfos)) goto CleanHandle;

            for (int i = 0, n = cbInfos.Length; i < n; i++)
            {
                var cb = cbInfos[i];
                if (cb == null) continue;

                cb.Node.List?.Remove(cb.Node);
                Del(cb);
            }

            if (!keepCallback && cbInfos.Length > 0)
            {
                // Dispose 回调在任意一条 CbInfo 上都一样, 只需卸一次.
                var anyCb = cbInfos[0];
                if (anyCb?.DisposeCallback != null && obj is IXDDisposable xdObj)
                {
                    try { xdObj.RemoveOnDispose(anyCb.DisposeCallback); }
                    catch (Exception e) { Log.Error(e); }
                }
            }

            CleanHandle:
            // ReSharper disable once InvertIf
            if (obj is TickFuncHandle handle && !((XDDisposableObjectBase)handle).IsDisposed)
            {
                // 直接触发 OnDisposed-语义: handle.Dispose() 内部会再查 Contains(this) == false, 于是不会回调到我们.
                try { handle.Dispose(); }
                catch (Exception e) { Log.Error($"TickFuncHandle.Dispose() failed: {e}"); }
            }
        }

        private CbInfo?[]? RegisterInnerDelay(in RegInfo info)
        {
            switch (info.Kind)
            {
                case RegKind.Tick:
                case RegKind.LateTick:
                case RegKind.PhysicalTick:
                {
                    if (info.Act == null) return null;
                    var cb = New();
                    cb.LoadAct(info, info.Kind);
                    AddToList(cb, info.Kind);
                    return new[] { cb };
                }
                case RegKind.Custom:
                {
                    // 同一对象可能同时实现 ITick / ILateTicker / IPhysicalTicker.
                    var t = info.Source as ITick;
                    var l = info.Source as ILateTicker;
                    var p = info.Source as IPhysicalTicker;

                    var cnt = (t != null ? 1 : 0) + (l != null ? 1 : 0) + (p != null ? 1 : 0);
                    if (cnt == 0) return null;

                    var arr = new CbInfo?[cnt];
                    var idx = 0;
                    if (t != null) { var cb = New(); cb.LoadInterfaceTick(info, t);            AddToList(cb, RegKind.Tick);         arr[idx++] = cb; }
                    if (l != null) { var cb = New(); cb.LoadInterfaceLateTick(info, l);        AddToList(cb, RegKind.LateTick);     arr[idx++] = cb; }
                    if (p != null) { var cb = New(); cb.LoadInterfacePhysicalTick(info, p);    AddToList(cb, RegKind.PhysicalTick); arr[idx]   = cb; }
                    return arr;
                }
                default: return null;
            }
        }

        private void AddToList(CbInfo cb, RegKind kind)
        {
            var list = kind switch
            {
                RegKind.Tick         => _instUpd,
                RegKind.LateTick     => _instLateUpd,
                RegKind.PhysicalTick => _instFixedUpd,
                _ => null
            };
            list?.AddLast(cb.Node);
        }

        private static void UnbindDisposeCallback(in RegInfo info)
        {
            if (info.DisposeCallback == null || info.Source is not IXDDisposable xdObj) return;
            try { xdObj.RemoveOnDispose(info.DisposeCallback); }
            catch (Exception e) { Log.Error(e); }
        }

        private CbInfo New() => _infoPool.TryPop(out var v) ? v : new CbInfo();

        private void Del(CbInfo info)
        {
            info.Clear();
            if (_infoPool.Count >= CallbackInfoPoolSize) return;
            _infoPool.Push(info);
        }

        #endregion

        #region Driver (主线程)

        private void OnTick(float dt, float rdt)
        {
            CheckRegister();
            CheckUnregister();

            InvokeDirect(OnTickDirect, dt, rdt);
            TickList(_instUpd, dt, rdt);
        }

        private void OnPhysicalTick(float dt, float rdt)
        {
            CheckRegister();
            CheckUnregister();

            InvokeDirect(OnPhysicalTickDirect, dt, rdt);
            TickList(_instFixedUpd, dt, rdt);
        }

        private void OnLateTick(float dt, float rdt)
        {
            CheckRegister();
            CheckUnregister();

            InvokeDirect(OnLateTickDirect, dt, rdt);
            TickList(_instLateUpd, dt, rdt);
        }

        /// <summary> 每个订阅者独立 try/catch, 单订阅者异常不会影响后续订阅者. </summary>
        private static void InvokeDirect(TickFunc? ev, float dt, float rdt)
        {
            if (ev == null) return;
            var list = ev.GetInvocationList();
            for (int i = 0, n = list.Length; i < n; i++)
            {
                try { ((TickFunc)list[i])(dt, rdt); }
                catch (Exception e) { Log.Error(e); }
            }
        }

        private void TickList(LinkedList<CbInfo> list, float dt, float rdt)
        {
            var node = list.First;
            while (node != null)
            {
                var next = node.Next;
                var info = node.Value;

                // 注销保护: 若该 CbInfo 已经被 Clear (Source == null), 直接移除.
                var source = info.Source;
                if (source == null)
                {
                    list.Remove(node);
                    node = next;
                    continue;
                }

                // delay 检查 (仅首次调用前生效).
                if (info.CallCnt <= 0 && info.Delay > info.LastCallDeltaTime)
                {
                    info.LastCallDeltaTime += dt;
                    node = next;
                    continue;
                }

                // interval 检查.
                if (info.Interval > info.LastCallDeltaTime)
                {
                    info.LastCallDeltaTime += dt;
                    node = next;
                    continue;
                }

                var ret = false;
                try { ret = info.InvokeTick(dt, rdt); }
                catch (Exception e) { Log.Error(e); }

                if (info.CallCnt == ulong.MaxValue) info.CallCnt = 1;
                else info.CallCnt++;
                info.LastCallDeltaTime = 0;

                if (!ret || (info.MaxCallCnt > 0 && info.CallCnt >= (ulong)info.MaxCallCnt))
                {
                    // 异步 Unregister (通过队列), 实际 Remove 发生在下一次 flush.
                    Unregister(source);
                }

                node = next;
            }
        }

        #endregion

        #region Lifecycle

        internal override IProcedure DeInitProcedure() => _deinitProcedure ??= new ProcedureSync(DeInit);
        internal override IProcedure InitProcedure() => _initProcedure ??= new ProcedureSync(Init);
        internal override IProcedure ReinitProcedure() => _reinitProcedure ??= new ProcedureSync(Reinit);

        private IProcedure.RetInfo Init()
        {
            ResetAll();

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
            ResetAll();

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

        private void ResetAll()
        {
            while (_registerQueue.TryDequeue(out _)) { }
            while (_unregisterQueue.TryDequeue(out _)) { }
            _registeredObjects.Clear();

            _instUpd.Clear();
            _instLateUpd.Clear();
            _instFixedUpd.Clear();
            _regInfoDict.Clear();
        }

        private IProcedure? _initProcedure;
        private IProcedure? _reinitProcedure;
        private IProcedure? _deinitProcedure;

        #endregion

        #region Inner Types

        private enum RegKind : byte
        {
            Custom = 0,   // 接口类型
            Tick,
            LateTick,
            PhysicalTick
        }

        /// <summary>
        /// 注册意图. 跨线程传递, 做成 struct 避免堆分配.
        /// </summary>
        private readonly struct RegInfo
        {
            public readonly IPolling? Source;
            public readonly Action? DisposeCallback;

            public readonly RegKind Kind;
            public readonly float Delay;
            public readonly float Interval;
            public readonly long MaxCallCnt;
            public readonly Func<float, float, bool>? Act;

            public bool IsValid => Source != null && (Act != null || Kind == RegKind.Custom);

            private RegInfo(IPolling? source, Action? disposeCb,
                RegKind kind, float delay, float interval, long maxCallCnt,
                Func<float, float, bool>? act)
            {
                Source = source;
                DisposeCallback = disposeCb;
                Kind = kind;
                Delay = delay;
                Interval = interval;
                MaxCallCnt = maxCallCnt;
                Act = act;
            }

            public static RegInfo CreateInterface(IPolling src, Action? disposeCb)
                => new(src, disposeCb, RegKind.Custom, 0, 0, 0, null);

            public static RegInfo CreateAct(IPolling src, Func<float, float, bool> act,
                RegKind kind, float delay, float interval, long maxCallCnt)
                => new(src, null, kind, delay, interval, maxCallCnt, act);
        }

        /// <summary>
        /// 实装后的回调信息. 只被主线程访问, 无需同步.
        /// 相比原版: 去掉 delegate* 间接调用, 内联字段; 节点直接挂在 CbInfo 上, 卸载 O(1).
        /// </summary>
        private sealed class CbInfo
        {
            public LinkedListNode<CbInfo> Node { get; }
            public IPolling? Source { get; private set; }
            public Action? DisposeCallback { get; private set; }

            public ulong CallCnt;
            public float LastCallDeltaTime;

            public float Delay;
            public float Interval;
            public long MaxCallCnt;

            public CbInfo() => Node = new LinkedListNode<CbInfo>(this);

            public void Clear()
            {
                CallCnt = 0;
                LastCallDeltaTime = 0;
                Delay = 0;
                Interval = 0;
                MaxCallCnt = 1;
                Source = null;
                DisposeCallback = null;
                _act = null;
                _tick = null;
                _lateTick = null;
                _physicalTick = null;
                _kind = CbKind.Empty;
            }

            public void LoadAct(in RegInfo info, RegKind kind)
            {
                Source = info.Source;
                DisposeCallback = info.DisposeCallback;
                Delay = info.Delay;
                Interval = info.Interval;
                MaxCallCnt = info.MaxCallCnt;
                _act = info.Act;
                _kind = CbKind.Act;
                _ = kind; // kind 在 AddToList 里决定链表, 不需要再记录.
            }

            public void LoadInterfaceTick(in RegInfo info, ITick inst)
            {
                Source = info.Source;
                DisposeCallback = info.DisposeCallback;
                Delay = 0;
                Interval = inst.TickInterval;
                MaxCallCnt = 0;
                _tick = inst;
                _kind = CbKind.Tick;
            }

            public void LoadInterfaceLateTick(in RegInfo info, ILateTicker inst)
            {
                Source = info.Source;
                DisposeCallback = info.DisposeCallback;
                Delay = 0;
                Interval = inst.LateTickInterval;
                MaxCallCnt = 0;
                _lateTick = inst;
                _kind = CbKind.LateTick;
            }

            public void LoadInterfacePhysicalTick(in RegInfo info, IPhysicalTicker inst)
            {
                Source = info.Source;
                DisposeCallback = info.DisposeCallback;
                Delay = 0;
                Interval = inst.PhysicalTickInterval;
                MaxCallCnt = 0;
                _physicalTick = inst;
                _kind = CbKind.PhysicalTick;
            }

            public bool InvokeTick(float dt, float rdt)
            {
                // Interval 可能来自接口的动态属性, 每次重读.
                switch (_kind)
                {
                    case CbKind.Empty: return false;
                    case CbKind.Act: return _act != null && _act(dt, rdt);
                    case CbKind.Tick:
                        if (_tick == null) return false;
                        Interval = _tick.TickInterval;
                        _tick.OnTick(dt, rdt);
                        return true;
                    case CbKind.LateTick:
                        if (_lateTick == null) return false;
                        Interval = _lateTick.LateTickInterval;
                        _lateTick.OnLateTick(dt, rdt);
                        return true;
                    case CbKind.PhysicalTick:
                        if (_physicalTick == null) return false;
                        Interval = _physicalTick.PhysicalTickInterval;
                        _physicalTick.OnPhysicalTick(dt, rdt);
                        return true;
                    default: return false;
                }
            }

            private enum CbKind : byte { Empty = 0, Act, Tick, LateTick, PhysicalTick }

            private CbKind _kind = CbKind.Empty;
            private Func<float, float, bool>? _act;
            private ITick? _tick;
            private ILateTicker? _lateTick;
            private IPhysicalTicker? _physicalTick;
        }

        private sealed class ReferenceComparer : IEqualityComparer<IPolling>
        {
            public static readonly ReferenceComparer Instance = new();
            public bool Equals(IPolling? x, IPolling? y) => ReferenceEquals(x, y);
            public int GetHashCode(IPolling obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        #endregion
    }
}
