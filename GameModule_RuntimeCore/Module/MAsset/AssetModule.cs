using System;
using XD.Common.AsyncUtil;
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

        public IAssetHolder<T> Load<TTask, T>(string key, EAutoReleaseType relType = EAutoReleaseType.Immediately, float delay = DefaultDelayTime)
            where TTask : IAssetTask<T>
        {
            if (_assetHandles.TryGetValue(key, out var handle))
            {
                if (handle is not AssetHandle<TTask, T> tHandle) return EmptyAssetHolder<T>.I;
                handle.RecoverConf(relType, delay);
                var holder = NewHolder(tHandle);
                tHandle.Apply(holder);
                return holder;
            }

            {
                if (AssetLoader.Loader == null) return EmptyAssetHolder<T>.I;
                var src = AssetLoader.Loader.LoadAsset<TTask, T>(key);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (src == null || !src.IsValid) return EmptyAssetHolder<T>.I;
                var newHandle = new AssetHandle<TTask, T>(src, this, new AssetHandle.AutoReleaseConf(relType, delay, key));
                if (!_assetHandles.TryAdd(key, newHandle)) return EmptyAssetHolder<T>.I;
                var holder = NewHolder(newHandle);
                newHandle.Apply(holder);
                return holder;
            }
        }
        #endregion

        #region Liftcycle

        internal override IProcedure InitProcedure() => new ProcedureSync(() =>
        {
            if (E.Tick == null) return (IProcedure.EndType.Abort, new ArgumentNullException(nameof(E.Tick)));
            E.Tick.Register(_delayDriver);
            return IProcedure.RetInfo.Success;
        });

        internal override IProcedure ReinitProcedure() => new ProcedureSync(() =>
        {
            foreach (var handle in _assetHandles.Values) handle.Release();
            _assetHandles.Clear();
            _delayDriver.Dispose();
            _delayDriver = new TickDelayDriver();
            E.Tick?.Register(_delayDriver);
            return IProcedure.RetInfo.Success;
        });

        internal override IProcedure DeInitProcedure() => new ProcedureSync(() =>
        {
            foreach (var handle in _assetHandles.Values) handle.Release();
            _assetHandles.Clear();
            _delayDriver.Dispose();
            _delayDriver = new TickDelayDriver();
            return IProcedure.RetInfo.Success;
        });

        #endregion
    }
}