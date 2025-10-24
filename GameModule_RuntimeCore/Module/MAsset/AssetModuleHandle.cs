using System;
using System.Collections.Concurrent;
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
            public void Bind(IXDDisposable? disposable = null) {}

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
                _onComplete?.Invoke();
                _onComplete = null;
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
                OnDisposed();

                // clear asset state
                Asset = default;
                IsInvalid = true;
                _assetHandle = null;

                // clear fin state
                if (IsCompleted) return;
                IsCompleted = true;
                _onComplete?.Invoke();
                _onComplete = null;
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

            public int RefCnt { get; protected set; }
            public abstract float Progress { get; }

            /// <summary>
            /// 计算一个释放条件最宽松的配置
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

        private class AssetHandle<TTask, T> : AssetHandle<T> where TTask : IAssetTask<T>
        {
            public override float Progress => _isReleased ? 0 : _handle.Progress;

            public AssetHandle(TTask src, AssetModule module, in AutoReleaseConf conf) : base(module, conf)
            {
                _handle = src;
                _isReleased = false;
                if (!src.IsCompleted) src.OnCompleted(OnComplete);
            }

            public override void Apply(AssetHolderInner<T> holder)
            {
                if (_isReleased)
                {
                    holder.Dispose();
                    return;
                }

                RefCnt++;
                if (!_handle.IsCompleted)
                {
                    if (_delayHandle != null) Module._delayDriver.RemoveDelay(_delayHandle);
                    _onComplete += holder.DoCompleteInner;
                    return;
                }

                if (!_handle.IsValid || _handle.IsFailed)
                {
                    Release();
                    holder.DoCompleteInner(_handle.GetResult(), false);
                    Log.Error($"ERR AssetModule: load asset failed: {_handle.Exception}");
                    return;
                }

                if (_delayHandle != null) Module._delayDriver.RemoveDelay(_delayHandle);
                holder.DoCompleteInner(_handle.GetResult(), true);
            }

            public override void AntiApply(AssetHolderInner<T> holder)
            {
                if (_isReleased) return;
                if (!_handle.IsCompleted) _onComplete -= holder.DoCompleteInner;

                RefCnt--;
                if (RefCnt != 0) return;
                DelayUnload();
            }

            public override void DelayUnload(bool isFromModule = false)
            {
                if (_isReleased) return;
                switch (Conf.Type)
                {
                    case EAutoReleaseType.Immediately:
                        Release();
                        break;
                    case EAutoReleaseType.Delay:
                        if (_delayHandle == null)
                        {
                            if (Conf.Delay <= 0)
                            {
                                Release();
                                break;
                            }
                            _delayHandle = Module._delayDriver.AddDelay(Release, Conf.Delay);
                        }
                        break;
                    default:
                    case EAutoReleaseType.Never:
                        if (isFromModule) Release();
                        break;
                }
            }

            public override void Release()
            {
                if (_isReleased) return;
                _isReleased = true;
                _delayHandle = null;
                Module._assetHandles.TryRemove(Conf.Key, out _);

                _onComplete?.Invoke(default!, false);
                _onComplete = null;
                _handle.Release();
            }

            private void OnComplete()
            {
                if (_isReleased) return;
                if (!_handle.IsCompleted || !_handle.IsValid || _handle.IsFailed)
                {
                    Release();
                    _onComplete?.Invoke(_handle.GetResult(), false);
                    Log.Error($"ERR AssetModule: load asset failed: {_handle.Exception}");
                    return;
                }

                _onComplete?.Invoke(_handle.GetResult(), true);
                _onComplete = null;
            }

            private bool _isReleased;

            private object? _delayHandle;
            private Action<T?, bool>? _onComplete;
            private TTask _handle;
        }

        private TickDelayDriver _delayDriver = new();
        private readonly ConcurrentDictionary<string, AssetHandle> _assetHandles = new();
        #endregion
    }
}