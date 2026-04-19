using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable MemberCanBePrivate.Global

namespace XD.Common.AsyncUtil
{
    public interface IAwaitable<out TAwaiter> where TAwaiter : IAwaiter { TAwaiter GetAwaiter(); }
    public interface IAwaitable<out TAwaiter, out TResult> where TAwaiter : IAwaiter<TResult> { TAwaiter GetAwaiter(); }
    public interface IAwaitableWithoutIsCompleted<out TAwaiter, out TResult> where TAwaiter : IAwaiterWithoutIsCompleted<TResult> { TAwaiter GetAwaiter(); }

    public interface IAwaiter : INotifyCompletion
    {
        bool IsCompleted { get; }
        void GetResult();
    }

    public interface IAwaiter<out TResult> : INotifyCompletion
    {
        bool IsCompleted { get; }
        TResult? GetResult();
    }

    public interface IAwaiterWithoutIsCompleted<out TResult> : INotifyCompletion
    {
        TResult? GetResult();
    }

    public interface ICriticalAwaiter<out TResult> : IAwaiter<TResult>, ICriticalNotifyCompletion {}
    public interface ICriticalAwaiter : IAwaiter, ICriticalNotifyCompletion {}

    public class Awaiter : IAwaiter, IAwaitable<IAwaiter>
    {
        public void OnCompleted(Action continuation)
        {
            if (IsCompleted) continuation();
            else OnCompletedDelegate += continuation;
        }

        public void Complete()
        {
            if (IsCompleted) return;
            IsCompleted = true;
            OnCompletedDelegate?.Invoke();
            OnCompletedDelegate = null;
        }

        public void GetResult() {}
        public bool IsCompleted { get; private set; }
        public IAwaiter GetAwaiter() => this;

        private event Action? OnCompletedDelegate;
    }

    public class Awaiter<T> : IAwaiter<T>, IAwaitable<IAwaiter<T>, T>
    {
        public void OnCompleted(Action continuation)
        {
            if (IsCompleted) continuation();
            else OnCompletedDelegate += continuation;
        }

        public void Complete(T? result)
        {
            if (IsCompleted) return;
            _result = result;
            IsCompleted = true;
            OnCompletedDelegate?.Invoke();
            OnCompletedDelegate = null;
        }

        public T? GetResult() => _result;
        public bool IsCompleted { get; private set; }
        public IAwaiter<T> GetAwaiter() => this;

        private T? _result;
        private event Action? OnCompletedDelegate;
    }

    public static class AsyncUtil
    {
        public static Awaiter All(params IAwaiter[] awaitableList) => All((IReadOnlyCollection<IAwaiter>)awaitableList);
        public static Awaiter All(IReadOnlyCollection<IAwaiter>? awaitableList)
        {
            var result = new Awaiter();
            var count = awaitableList?.Count ?? 0;
            if (count == 0)
            {
                result.Complete();
                return result;
            }

            foreach (var awaitable in awaitableList!)
            {
                awaitable.OnCompleted(() =>
                {
                    if (--count == 0) result.Complete();
                });
            }
            return result;
        }

        public static Awaiter<T> All<T>(params IAwaiter<T>[] awaitableList) => All((IReadOnlyCollection<IAwaiter<T>>)awaitableList);
        public static Awaiter<T> All<T>(IReadOnlyCollection<IAwaiter<T>>? awaitableList)
        {
            var result = new Awaiter<T>();
            var count = awaitableList?.Count ?? 0;
            if (count == 0)
            {
                result.Complete(default);
                return result;
            }

            foreach (var awaitable in awaitableList!)
            {
                awaitable.OnCompleted(() =>
                {
                    if (--count == 0) result.Complete(default);
                });
            }
            return result;
        }

        public static Awaiter Race(params IAwaiter[] awaitableList) => Race((IReadOnlyCollection<IAwaiter>)awaitableList);
        public static Awaiter Race(IReadOnlyCollection<IAwaiter>? awaitableList)
        {
            var result = new Awaiter();
            if (awaitableList is not { Count: > 0 })
            {
                result.Complete();
                return result;
            }

            foreach (var awaitable in awaitableList)
            {
                awaitable.OnCompleted(() =>
                {
                    if (result.IsCompleted) return;
                    result.Complete();
                });
            }
            return result;
        }

        public static Awaiter<T> Race<T>(params IAwaiter<T>[] awaitableList) => Race((IReadOnlyCollection<IAwaiter<T>>)awaitableList);
        public static Awaiter<T> Race<T>(IReadOnlyCollection<IAwaiter<T>>? awaitableList)
        {
            var result = new Awaiter<T>();
            if (awaitableList is not { Count: > 0 })
            {
                result.Complete(default);
                return result;
            }

            foreach (var awaitable in awaitableList)
            {
                awaitable.OnCompleted(() =>
                {
                    if (result.IsCompleted) return;
                    result.Complete(default);
                });
            }
            return result;
        }
    }
}