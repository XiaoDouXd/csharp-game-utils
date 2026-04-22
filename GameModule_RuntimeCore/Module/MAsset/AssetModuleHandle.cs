using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XD.Common.AsyncUtil;
using XD.Common.Log;
using XD.Common.ScopeUtil;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace XD.GameModule.Module.MAsset
{
    public partial class AssetModule
    {
        #region holder
        public interface IAssetHolder : IXDDisposable
        {
            public Type AssetType { get; }
            public object? Asset { get; }
            public bool IsInvalid { get; }
            public bool IsCompleted { get; }
            public float Progress { get; }
        }

        public interface IAssetHolder<out T> :
            IAssetHolder,
            IAwaiterWithoutIsCompleted<T?>,
            IAwaitableWithoutIsCompleted<IAssetHolder<T>, T?>
        {
            public new T? Asset { get; }
        }

        public class EmptyAssetHolder<T> : IAssetHolder<T>
        {
            public static EmptyAssetHolder<T> I { get; } = new();

            public Type AssetType => typeof(T);
            object? IAssetHolder.Asset => null;

            public bool IsInvalid => false;
            public bool IsCompleted => true;
            public float Progress => float.NaN;

            public T? Asset => default;
            public bool IsDisposed => true;

            public void Dispose() {}
            public void AddOnDispose(Action? callback) => callback?.Invoke();
            public void RemoveOnDispose(Action? callback) {}
            public IXDDisposable Bind(IXDDisposable? disposable = null) => this;

            public T? GetResult() => default;
            public IAssetHolder<T> GetAwaiter() => this;
            public void OnCompleted(Action? continuation) => continuation?.Invoke();
            private EmptyAssetHolder() {}
        }

        private class AssetHolderInner<T> : XDDisposableObjectBase, IAssetHolder<T>
        {
            public Type AssetType => typeof(T);
            object? IAssetHolder.Asset => Asset;
            public T? Asset { get; private set; }
            public bool IsInvalid { get; private set; }
            public bool IsCompleted { get; private set; }
            public float Progress => IsCompleted ? IsInvalid ? float.NaN : 1f : _assetHandle?.Progress ?? float.NaN;
            public AssetHolderInner(AssetHandle<T> handle) => _assetHandle = handle;

            #region asset handle
            public void DoCompleteInner(T? asset, bool isSuccess)
            {
                // check dispose
                if (IsDisposed) return;
                if (!isSuccess)
                {
                    Dispose();
                    return;
                }

                // set fin state
                if (IsCompleted) return;
                Asset = asset;
                IsCompleted = true;
                var onComplete = _onComplete;
                _onComplete = null;
                onComplete?.Invoke();
            }

            private AssetHandle<T>? _assetHandle;
            #endregion

            #region awaiter
            public T? GetResult() => Asset;
            public void OnCompleted(Action? continuation)
            {
                if (continuation == null) return;
                if (IsCompleted) continuation();
                _onComplete += continuation;
            }
            public IAssetHolder<T> GetAwaiter() => this;
            private Action? _onComplete;
            #endregion

            #region xd disposable
            public override void Dispose()
            {
                // check dispose
                if (IsDisposed) return;
                _assetHandle?.AntiApply(this);

                // clear asset state
                Asset = default;
                IsInvalid = true;
                _assetHandle = null;

                try { OnDisposed(); }
                catch (Exception e) { Log.Error(e); }

                // clear fin state
                if (IsCompleted) return;
                IsCompleted = true;
                var onComplete = _onComplete;
                _onComplete = null;

                // call fin
                onComplete?.Invoke();
            }
            #endregion
        }
        private static AssetHolderInner<T> NewHolder<TTask, T>(AssetHandle<TTask, T> handle)
            where TTask : IAssetTask<T>
            => new (handle);
        #endregion

        #region handler
        public enum EAutoReleaseType : byte
        {
            // 释放条件逐级宽松
            Immediately = 0,
            Delay,
            Never
        }

        private abstract class AssetHandle
        {
            public readonly struct AutoReleaseConf
            {
                public readonly string Key;
                public readonly float Delay;
                public readonly EAutoReleaseType Type;

                public AutoReleaseConf(EAutoReleaseType type, float delay, string key)
                {
                    Key = key;
                    Type = type;
                    Delay = delay;
                }
            }

            // 引用计数改为 volatile 读 + lock 保护下的写. RefCnt 仅做外部只读展示, 真正的增减均在子类 lock 内完成.
            public int RefCnt { get; protected set; }

            public abstract float Progress { get; }

            /// <summary>
            /// 计算一个释放条件最宽松的配置. 配置改动是 struct 全量替换, 业务上不会跨 Apply 边界并发修改,
            /// 这里不加锁 (若将来有需求可加 Interlocked / lock 保护).
            /// </summary>
            public void RecoverConf(EAutoReleaseType type, float delay)
            {
                type = (EAutoReleaseType)Math.Max((byte)type, (byte)Conf.Type);
                delay = Math.Max(delay, Conf.Delay);
                Conf = new AutoReleaseConf(type, delay, Conf.Key);
            }

            public abstract void Release();
            public abstract void DelayUnload(bool isFromModule = false);

            protected AssetHandle(AssetModule module, in AutoReleaseConf conf)
            {
                Module = module;
                Conf = conf;
            }

            protected AutoReleaseConf Conf;
            protected readonly AssetModule Module;
        }

        private abstract class AssetHandle<T> : AssetHandle
        {
            protected AssetHandle(AssetModule module, in AutoReleaseConf conf) : base(module, conf) {}
            public abstract void Apply(AssetHolderInner<T> holder);
            public abstract void AntiApply(AssetHolderInner<T> holder);
        }

        /// <summary>
        /// 资源句柄. 多线程并发点:
        ///   - Apply / AntiApply / Release 可能被任意线程调用;
        ///   - OnComplete 由底层 IAssetTask 的回调线程调用 (可能是 loader 工作线程);
        ///   - DelayDriver 的 AddDelay / RemoveDelay 本身线程安全 (<see cref="TickDelayDriver"/>).
        /// <para/>
        /// 同步策略:
        ///   - _sync 锁保护 _refCnt / _isReleased / _onComplete / _delayHandle 的状态变更;
        ///   - 回调 (holder.DoCompleteInner) 统一在锁外触发, 避免用户代码在锁内重入死锁;
        ///   - _onComplete 改为 List, 避免多播委托 += 的 O(n²) 复制 (§5.4 性能层面).
        /// </summary>
        private class AssetHandle<TTask, T> : AssetHandle<T> where TTask : IAssetTask<T>
        {
            public override float Progress =>
                // 只读字段快照, 无需锁.
                _isReleased ? 0 : _handle.Progress;

            public AssetHandle(TTask src, AssetModule module, in AutoReleaseConf conf) : base(module, conf)
            {
                _handle = src;
                _isReleased = false;
                if (!src.IsCompleted) src.OnCompleted(OnComplete);
            }

            public override void Apply(AssetHolderInner<T> holder)
            {
                // 下面用 3 个标志位记录: 锁外需要如何通知该 holder.
                var notifySuccess = false;
                var notifyFail = false;
                var alreadyReleased = false;
                T? result = default;

                lock (_sync)
                {
                    if (_isReleased)
                    {
                        alreadyReleased = true;
                    }
                    else if (!_handle.IsCompleted)
                    {
                        // 还在加载中: 订阅完成回调, 增加引用计数, 取消可能的 delay unload.
                        if (_delayHandle != null)
                        {
                            Module._delayDriver.RemoveDelay(_delayHandle);
                            _delayHandle = null;
                        }
                        _onComplete.Add(holder.DoCompleteInner);
                        RefCnt++;
                        return; // 锁内 early-return; 无需锁外通知.
                    }
                    else if (!_handle.IsValid || _handle.IsFailed)
                    {
                        // 已完成但失败: 直接触发失败回调 (锁外), 并推入 Release.
                        // 此处 *不* 增加 RefCnt (§5.5 修复: 原实现先 ++ 再 Release, holder.DoCompleteInner 的后续 Dispose/AntiApply
                        // 因 _isReleased=true 而直接 return, RefCnt 不减, 造成泄漏).
                        result = _handle.GetResult();
                        notifyFail = true;
                        // Release 需要在锁内执行: 它会清理订阅的 _onComplete 列表.
                        ReleaseInnerLocked(out var pendingCbs);
                        // pendingCbs 可能非空 (其他并发 Apply 已订阅), 锁外一并以失败通知.
                        InvokeOutsideLock(pendingCbs, default, false);
                    }
                    else
                    {
                        // 已完成且成功.
                        if (_delayHandle != null)
                        {
                            Module._delayDriver.RemoveDelay(_delayHandle);
                            _delayHandle = null;
                        }
                        RefCnt++;
                        result = _handle.GetResult();
                        notifySuccess = true;
                    }
                }

                // 锁外通知.
                if (alreadyReleased) holder.Dispose();
                else if (notifySuccess) holder.DoCompleteInner(result, true);
                else if (notifyFail)
                {
                    holder.DoCompleteInner(result, false);
                    Log.Error($"ERR AssetModule: load asset failed: {_handle.Exception}");
                }
            }

            public override void AntiApply(AssetHolderInner<T> holder)
            {
                var shouldDelayUnload = false;
                lock (_sync)
                {
                    if (_isReleased) return;
                    if (!_handle.IsCompleted) _onComplete.Remove(holder.DoCompleteInner);

                    if (RefCnt > 0)
                    {
                        RefCnt--;
                        if (RefCnt == 0) shouldDelayUnload = true;
                    }
                }
                if (shouldDelayUnload) DelayUnload();
            }

            public override void DelayUnload(bool isFromModule = false)
            {
                // 为避免在锁内调用 DelayDriver (尽管 driver 自身线程安全), 分两步: 锁内决策, 锁外 AddDelay.
                var scheduleImmediate = false;
                var scheduleDelay = false;
                float delay = 0;

                lock (_sync)
                {
                    if (_isReleased) return;
                    switch (Conf.Type)
                    {
                        case EAutoReleaseType.Immediately:
                            scheduleImmediate = true;
                            break;
                        case EAutoReleaseType.Delay:
                            if (_delayHandle == null)
                            {
                                if (Conf.Delay <= 0) scheduleImmediate = true;
                                else { scheduleDelay = true; delay = Conf.Delay; }
                            }
                            break;
                        default:
                        case EAutoReleaseType.Never:
                            if (isFromModule) scheduleImmediate = true;
                            break;
                    }
                }

                if (scheduleImmediate) Release();
                else if (scheduleDelay)
                {
                    var h = Module._delayDriver.AddDelay(Release, delay);
                    // 仍需在锁内写回 _delayHandle; 如果此时已经被 Release 或者另一路径已经设置过 _delayHandle,
                    // 立即取消这次的 delay handle.
                    lock (_sync)
                    {
                        if (_isReleased || _delayHandle != null)
                        {
                            Module._delayDriver.RemoveDelay(h);
                        }
                        else
                        {
                            _delayHandle = h;
                        }
                    }
                }
            }

            public override void Release()
            {
                List<Action<T?, bool>>? cbs;
                TTask handleSnapshot;
                lock (_sync)
                {
                    if (_isReleased) return;
                    ReleaseInnerLocked(out cbs);
                    handleSnapshot = _handle;
                }

                // 锁外执行: 移除字典条目 / 调用回调 / 释放底层 handle.
                // _assetHandles.TryRemove 本身线程安全.
                Module._assetHandles.TryRemove(Conf.Key, out _);
                InvokeOutsideLock(cbs, default, false);
                try { handleSnapshot.Release(); }
                catch (Exception e) { Log.Error($"ERR AssetModule: IAssetTask.Release failed: {e}"); }
            }

            /// <summary>
            /// 锁内 release: 仅修改状态与取消 delayHandle, 取出待通知列表供锁外调用.
            /// 不触碰 _handle.Release (外部或更底层 loader 线程可能在异步释放), 不触碰 _assetHandles (concurrent 安全).
            /// </summary>
            private void ReleaseInnerLocked(out List<Action<T?, bool>>? pendingCbs)
            {
                _isReleased = true;

                if (_delayHandle != null)
                {
                    Module._delayDriver.RemoveDelay(_delayHandle);
                    _delayHandle = null;
                }

                if (_onComplete.Count == 0) pendingCbs = null;
                else
                {
                    pendingCbs = _onComplete;
                    _onComplete = EmptyCbList; // 之后任何 Apply 都会进 _isReleased 分支.
                }
            }

            private void OnComplete()
            {
                // 由 IAssetTask 回调, 线程未定.
                List<Action<T?, bool>>? cbs;
                bool isSuccess;
                T? result;

                lock (_sync)
                {
                    if (_isReleased) return;

                    if (!_handle.IsCompleted || !_handle.IsValid || _handle.IsFailed)
                    {
                        // 加载失败: Release 同时取出待通知.
                        ReleaseInnerLocked(out cbs);
                        isSuccess = false;
                        result = _handle.GetResult();
                    }
                    else
                    {
                        // 加载成功: 取出待通知清空, 但保留 handle 以便后续 Apply.
                        isSuccess = true;
                        result = _handle.GetResult();
                        if (_onComplete.Count == 0) cbs = null;
                        else
                        {
                            cbs = _onComplete;
                            _onComplete = new List<Action<T?, bool>>();
                        }
                    }
                }

                // 锁外调用.
                if (!isSuccess)
                {
                    Module._assetHandles.TryRemove(Conf.Key, out _);
                    InvokeOutsideLock(cbs, default, false);
                    try { _handle.Release(); }
                    catch (Exception e) { Log.Error($"ERR AssetModule: IAssetTask.Release failed: {e}"); }
                    Log.Error($"ERR AssetModule: load asset failed: {_handle.Exception}");
                    return;
                }
                InvokeOutsideLock(cbs, result, true);
            }

            private static void InvokeOutsideLock(List<Action<T?, bool>>? cbs, T? asset, bool isSuccess)
            {
                if (cbs == null) return;
                for (int i = 0, n = cbs.Count; i < n; i++)
                {
                    try { cbs[i](asset, isSuccess); }
                    catch (Exception e) { Log.Error(e); }
                }
            }

            private static readonly List<Action<T?, bool>> EmptyCbList = new(0);

            // ----- fields (由 _sync 保护) -----
            private readonly object _sync = new();
            private bool _isReleased;
            private object? _delayHandle;
            private List<Action<T?, bool>> _onComplete = new();
            private TTask _handle;
        }

        private TickDelayDriver _delayDriver = new();
        private readonly ConcurrentDictionary<string, AssetHandle> _assetHandles = new();
        #endregion
    }
}