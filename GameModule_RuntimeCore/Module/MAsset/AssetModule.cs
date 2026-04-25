using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XD.Common.AsyncUtil;
using XD.Common.Log;
using XD.Common.Procedure;

namespace XD.GameModule.Module.MAsset
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class AssetModule : EngineModule
    {
        private const float DefaultDelayTime = 16;

        #region Interface
        public void Unload(IAssetHolder? handle) => handle?.Dispose();

        public bool Unload(string key, bool forceUnload = false, bool immediately = true)
        {
            if (!_assetHandles.TryGetValue(key, out var handle)) return false;
            if (!forceUnload && handle.RefCnt > 0) return false;
            if (!immediately) handle.DelayUnload(true);
            else handle.Release();
            return true;
        }

        public IAssetHolder<T> Load<TTaskCreator, TTask, T>(string key, EAutoReleaseType relType = EAutoReleaseType.Immediately, float delay = DefaultDelayTime)
            where TTask : IAssetTask<T>
            where TTaskCreator : struct, IAssetTaskCreator<TTask, T>
        {
            if (_assetHandles.TryGetValue(key, out var handle))
            {
                if (handle is not AssetHandle<TTask, T> tHandle)
                {
                    // 同 key 用不同的 T/TTask 再次加载: 这是典型的用法错误, 原实现静默返回空 holder, 难以排查.
                    Log.Error($"ERR AssetModule: key='{key}' was loaded with type '{handle.GetType()}', " +
                              $"but now requested as 'AssetHandle<{typeof(TTask)}, {typeof(T)}>', returning empty holder.");
                    return EmptyAssetHolder<T>.I;
                }
                handle.RecoverConf(relType, delay);
                var holder = NewHolder(tHandle);
                tHandle.Apply(holder);
                return holder;
            }

            {
                var src = default(TTaskCreator).Create(key);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (src == null || !src.IsValid) return EmptyAssetHolder<T>.I;
                var newHandle = new AssetHandle<TTask, T>(src, this, new AssetHandle.AutoReleaseConf(relType, delay, key));

                // 竞态: 同 key 可能并发进入 Load, 只有第一个 TryAdd 成功. 失败者必须把刚创建的 newHandle
                // 走正规的 Release 路径, 否则:
                //   1) newHandle 已经在构造函数里 src.OnCompleted(OnComplete) 订阅了完成回调, 但其
                //      _assetHandles 入表失败, 没有任何外部路径再会调用它的 Release, 造成 task 泄漏;
                //   2) src (IAssetTask) 也会持续持有资源句柄, 成为无主泄漏.
                // Release 内部会把 _isReleased 置位, 这样之后 src 的完成回调 OnComplete 直接 early-return,
                // 且只会按 value 匹配移除 _assetHandles 中属于 self 的条目 (不会误删胜出的 handle).
                if (!_assetHandles.TryAdd(key, newHandle))
                {
                    try { newHandle.Release(); }
                    catch (Exception e) { Log.Error($"ERR AssetModule: release losing handle failed: {e}"); }

                    // 竞态胜出者可能还没完成, 让本次调用去等它 (递归一次, 极端情况下只会多走一层).
                    return Load<TTaskCreator, TTask, T>(key, relType, delay);
                }

                var holder = NewHolder(newHandle);
                newHandle.Apply(holder);
                return holder;
            }
        }
        #endregion

        #region 主线程投递 (给 finalizer / GC 线程使用)
        /// <summary>
        /// 从任意线程 (含 GC finalizer) 向主线程投递一个动作. 该动作会在下一次 Tick 执行.
        /// <para/>
        /// 典型用途: IAssetTask 的 finalizer / 跨线程的资源释放兜底, 避免在 GC 线程上直接触碰 Godot /
        /// 主线程数据结构.
        /// <para/>
        /// 注意: 在 AssetModule 未 Init 或已 Deinit 状态下调用会直接丢弃 (视作 no-op), 不抛异常,
        /// 避免在 finalizer 里炸出二次异常.
        /// </summary>
        public void EnqueueMainThreadAction(Action? act)
        {
            if (act == null) return;
            if (!_isActive) return;
            _mainThreadQueue.Enqueue(act);
        }

        private void FlushMainThreadQueue(float _, float __)
        {
            while (_mainThreadQueue.TryDequeue(out var act))
            {
                try { act(); }
                catch (Exception e) { Log.Error($"ERR AssetModule: main-thread action failed: {e}"); }
            }
        }
        #endregion

        #region Liftcycle

        internal override IProcedure InitProcedure() => new ProcedureSync(() =>
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (E.Tick == null) return (IProcedure.EndType.Abort, new ArgumentNullException(nameof(E.Tick)));
            // 保证 init 幂等: 如果上一轮 deinit 未完全清理 (异常路径), 这里先做一次硬清理.
            HardResetState();

            E.Tick.Register(_delayDriver);
            E.Tick.OnTickDirect += FlushMainThreadQueue;
            _isActive = true;
            return IProcedure.RetInfo.Success;
        });

        internal override IProcedure ReinitProcedure() => new ProcedureSync(() =>
        {
            // 进入 reinit 期间, 先关闭 _isActive, 让 finalizer 路径投递的 action 被直接丢弃,
            // 避免在 reinit 中途的 tick 里触碰半清理状态.
            _isActive = false;

            // 1) 释放所有已注册的 handle. 注意:
            //    - Release 内部会调用 _assetHandles.ICollection.Remove 移除自身, 会修改字典;
            //    - ConcurrentDictionary 的枚举是弱一致的, 不会抛异常, 但为了结果确定性先 snapshot.
            var handlesSnapshot = _assetHandles.Values.ToSnapshot();
            foreach (var handle in handlesSnapshot)
            {
                try { handle.Release(); }
                catch (Exception e) { Log.Error($"ERR AssetModule: handle.Release during reinit failed: {e}"); }
            }
            _assetHandles.Clear();

            // 2) 清空 main-thread queue: 这些 action 的对象可能已经 dead.
            while (_mainThreadQueue.TryDequeue(out _)) { }

            // 3) 重置 delay driver. 旧的 driver 从 tick 反注册, 它内部仍在 pending 队列的任务会随 GC 回收.
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            E.Tick?.Unregister(_delayDriver);
            E.Tick!.OnTickDirect -= FlushMainThreadQueue;
            _delayDriver.Dispose();
            _delayDriver = new TickDelayDriver();
            E.Tick.Register(_delayDriver);
            E.Tick.OnTickDirect += FlushMainThreadQueue;

            _isActive = true;
            return IProcedure.RetInfo.Success;
        });

        internal override IProcedure DeInitProcedure() => new ProcedureSync(() =>
        {
            _isActive = false;

            var handlesSnapshot = _assetHandles.Values.ToSnapshot();
            foreach (var handle in handlesSnapshot)
            {
                try { handle.Release(); }
                catch (Exception e) { Log.Error($"ERR AssetModule: handle.Release during deinit failed: {e}"); }
            }
            _assetHandles.Clear();

            while (_mainThreadQueue.TryDequeue(out _)) { }

            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            E.Tick?.Unregister(_delayDriver);
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            if (E.Tick != null) E.Tick.OnTickDirect -= FlushMainThreadQueue;
            _delayDriver.Dispose();
            _delayDriver = new TickDelayDriver();
            return IProcedure.RetInfo.Success;
        });

        /// <summary>
        /// 在 init 前做一次兜底清理, 防止重复 Init 导致的状态残留 (例如 E.Tick 上已经订阅过 FlushMainThreadQueue).
        /// </summary>
        private void HardResetState()
        {
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            E.Tick?.Unregister(_delayDriver);
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            if (E.Tick != null) E.Tick.OnTickDirect -= FlushMainThreadQueue;

            var handlesSnapshot = _assetHandles.Values.ToSnapshot();
            foreach (var handle in handlesSnapshot)
            {
                try { handle.Release(); }
                catch (Exception e) { Log.Error($"ERR AssetModule: handle.Release during hard-reset failed: {e}"); }
            }
            _assetHandles.Clear();
            while (_mainThreadQueue.TryDequeue(out _)) { }
        }

        #endregion

        // ----- runtime 状态 -----
        private volatile bool _isActive;
        private readonly ConcurrentQueue<Action> _mainThreadQueue = new();
    }

    // 纯工具: ConcurrentDictionary.Values 在枚举过程中可能随并发修改变化, 一次性拷贝成数组便于稳定遍历.
    internal static class AssetModuleSnapshotExt
    {
        public static T[] ToSnapshot<T>(this ICollection<T> src)
        {
            var arr = new T[src.Count];
            src.CopyTo(arr, 0);
            return arr;
        }
    }
}
