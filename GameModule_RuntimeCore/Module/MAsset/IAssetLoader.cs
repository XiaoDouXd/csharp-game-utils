using System;
using XD.Common.AsyncUtil;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace XD.GameModule.Module.MAsset
{
    public interface IAssetTaskCreator<out TTask, T> where TTask : IAssetTask<T>
    {
        public TTask Create(string assetPath);
    }

    public interface IAssetTask<out T> : IAwaiter<T>
    {
        public bool IsValid { get; }
        public bool IsFailed { get; }
        public float Progress { get; }
        public Exception? Exception { get; }

        public void Release();
    }
}