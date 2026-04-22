using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global
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
        public static Awaiter Completed { get; } = new() { IsCompleted = true };

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
        public static Awaiter<T> Completed => _completed ??= new Awaiter<T> { IsCompleted = true };

        private T? _result;
        private event Action? OnCompletedDelegate;
        private static Awaiter<T>? _completed;
    }

    public static class AsyncUtil
    {
        public static Awaiter All(params IAwaiter?[] awaitableList) => All((IReadOnlyCollection<IAwaiter?>)awaitableList);
        public static Awaiter All(IReadOnlyCollection<IAwaiter?>? awaitableList)
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
                if (awaitable == null)
                {
                    --count;
                    continue;
                }
                awaitable.OnCompleted(() =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    if (--count == 0) result.Complete();
                });
            }
            if (count == 0) result.Complete();
            return result;
        }

        public static Awaiter<T?[]> All<T>(params IAwaiter<T>?[] awaitableList) => All((IReadOnlyCollection<IAwaiter<T>?>)awaitableList);
        public static Awaiter<T?[]> All<T>(IReadOnlyCollection<IAwaiter<T>?>? awaitableList)
        {
            var result = new Awaiter<T?[]>();
            var count = awaitableList?.Count ?? 0;
            if (count == 0)
            {
                result.Complete(Array.Empty<T>());
                return result;
            }

            var resultList = new T?[count];
            var index = 0;
            foreach (var awaitable in awaitableList!)
            {
                var currIdx = index++;
                if (awaitable == null)
                {
                    resultList[currIdx] = default;
                    --count;
                    continue;
                }

                awaitable.OnCompleted(() =>
                {
                    resultList[currIdx] = awaitable.GetResult();
                    // ReSharper disable once AccessToModifiedClosure
                    if (--count == 0) result.Complete(resultList);
                });
            }
            if (count == 0) result.Complete(resultList);
            return result;
        }

        public static Awaiter<T> AllWithLastResult<T>(IReadOnlyCollection<IAwaiter<T>?>? awaitableList)
        {
            var count = awaitableList?.Count ?? 0;
            if (count == 0) return Awaiter<T>.Completed;

            var result = new Awaiter<T>();
            foreach (var awaitable in awaitableList!)
            {
                if (awaitable == null)
                {
                    --count;
                    continue;
                }

                awaitable.OnCompleted(() =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    if (--count == 0) result.Complete(awaitable.GetResult());
                });
            }
            return count == 0 ? Awaiter<T>.Completed : result;
        }

        public static Awaiter Race(params IAwaiter[] awaitableList) => Race((IReadOnlyCollection<IAwaiter?>)awaitableList);
        public static Awaiter Race(IReadOnlyCollection<IAwaiter?>? awaitableList)
        {
            var result = new Awaiter();
            if (awaitableList is not { Count: > 0 })
            {
                result.Complete();
                return result;
            }

            var notAllNull = false;
            foreach (var awaitable in awaitableList)
            {
                if (awaitable == null) continue;
                notAllNull = true;
                awaitable.OnCompleted(() =>
                {
                    if (result.IsCompleted) return;
                    result.Complete();
                });
            }
            if (notAllNull) result.Complete();
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

            var notAllNull = false;
            foreach (var awaitable in awaitableList)
            {
                if (awaitable == null) continue;
                notAllNull = true;
                awaitable.OnCompleted(() =>
                {
                    if (result.IsCompleted) return;
                    result.Complete(awaitable.GetResult());
                });
            }
            if (notAllNull) result.Complete(default);
            return result;
        }

        public static Awaiter Then(this IAwaiter? awaitable, Action? continuation)
        {
            if (awaitable == null || awaitable.IsCompleted)
            {
                continuation?.Invoke();
                return Awaiter.Completed;
            }

            var result = new Awaiter();
            awaitable.OnCompleted(() =>
            {
                continuation?.Invoke();
                result.Complete();
            });
            return result;
        }
        public static Awaiter<TOut> Then<TOut>(this IAwaiter? awaitable, Func<TOut?>? continuation)
        {
            if (awaitable == null || awaitable.IsCompleted)
            {
                var outResult = continuation == null ? default : continuation.Invoke();
                if (outResult == null || EqualityComparer<TOut>.Default.Equals(outResult, default!)) return Awaiter<TOut>.Completed;

                var tout = new Awaiter<TOut>();
                tout.Complete(outResult);
                return tout;
            }

            var result = new Awaiter<TOut>();
            awaitable.OnCompleted(() =>
            {
                var outResult = continuation == null ? default : continuation.Invoke();
                result.Complete(outResult);
            });
            return result;
        }
        public static Awaiter<TOut> Then<T, TOut>(this IAwaiter<T>? awaitable, Func<T?, TOut?>? continuation)
        {
            if (awaitable == null || awaitable.IsCompleted)
            {
                var inResult = awaitable == null ? default : awaitable.GetResult();
                var outResult = continuation == null ? default : continuation.Invoke(inResult);
                if (outResult == null || EqualityComparer<TOut>.Default.Equals(outResult, default!)) return Awaiter<TOut>.Completed;

                var tout = new Awaiter<TOut>();
                tout.Complete(outResult);
                return tout;
            }

            var result = new Awaiter<TOut>();
            awaitable.OnCompleted(() =>
            {
                var inResult = awaitable.GetResult();
                var outResult = continuation == null ? default : continuation.Invoke(inResult);
                result.Complete(outResult);
            });
            return result;
        }
        public static Awaiter Then<T>(this IAwaiter<T>? awaitable, Action<T?>? continuation)
        {
            if (awaitable == null || awaitable.IsCompleted) return Awaiter.Completed;
            var result = new Awaiter();
            awaitable.OnCompleted(() =>
            {
                continuation?.Invoke(awaitable.GetResult());
                result.Complete();
            });
            return result;
        }
    }
}