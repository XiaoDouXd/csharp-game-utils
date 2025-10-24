using System;
using XD.Common.AsyncUtil;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace XD.GameModule.Module.MAsset
{
    public interface IAssetTask<out T> : IAwaiter<T>
    {
        public bool IsValid { get; }
        public bool IsFailed { get; }
        public float Progress { get; }
        public Exception? Exception { get; }

        public void Release();
    }

    public interface IAssetLoader
    {
        public TTask LoadAsset<TTask, T>(string assetPath) where TTask : IAssetTask<T>;
    }

    public static class AssetLoader
    {
        public static IAssetLoader? Loader { internal get; set; }
    }
}