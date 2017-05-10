#region usings

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

#endregion

namespace Convex.Types.Events {
    /// <summary>
    ///     Implementation taken from
    ///     https://github.com/RogueException/Discord.Net/blob/dev/src/Discord.Net.Core/Utils/AsyncEvent.cs
    /// </summary>
    public class AsyncEvent<T> where T : class {
        private readonly object subLock = new object();
        private ImmutableArray<T> subscriptions;

        public AsyncEvent() {
            subscriptions = ImmutableArray.Create<T>();
        }

        public bool HasSubscribers => subscriptions.Length != 0;
        public IReadOnlyList<T> Subscriptions => subscriptions;

        public void Add(T subscriber) {
            if (subscriber == null)
                throw new NullReferenceException("Null object cannot be added.");

            lock (subLock) {
                subscriptions = subscriptions.Add(subscriber);
            }
        }

        public void Remove(T subscriber) {
            if (subscriber == null)
                throw new NullReferenceException("Removal value cannot be null.");

            lock (subLock) {
                subscriptions = subscriptions.Remove(subscriber);
            }
        }
    }

    public static class EventExtensions {
        public static async Task InvokeAsync(this AsyncEvent<Func<Task>> eventHandler) {
            IReadOnlyList<Func<Task>> subscribers = eventHandler.Subscriptions;

            foreach (Func<Task> subscriber in subscribers)
                await subscriber.Invoke()
                    .ConfigureAwait(false);
        }

        public static async Task InvokeAsync<T>(this AsyncEvent<Func<T, Task>> eventHandler, T arg) {
            IReadOnlyList<Func<T, Task>> subscribers = eventHandler.Subscriptions;

            foreach (Func<T, Task> subscriber in subscribers)
                await subscriber.Invoke(arg)
                    .ConfigureAwait(false);
        }

        public static async Task InvokeAsync<T1, T2>(this AsyncEvent<Func<T1, T2, Task>> eventHandler, T1 arg1, T2 arg2) {
            IReadOnlyList<Func<T1, T2, Task>> subscribers = eventHandler.Subscriptions;

            foreach (Func<T1, T2, Task> subscriber in subscribers)
                await subscriber.Invoke(arg1, arg2)
                    .ConfigureAwait(false);
        }

        public static async Task InvokeAsync<T1, T2, T3>(this AsyncEvent<Func<T1, T2, T3, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3) {
            IReadOnlyList<Func<T1, T2, T3, Task>> subscribers = eventHandler.Subscriptions;

            foreach (Func<T1, T2, T3, Task> subscriber in subscribers)
                await subscriber.Invoke(arg1, arg2, arg3)
                    .ConfigureAwait(false);
        }

        public static async Task InvokeAsync<T1, T2, T3, T4>(this AsyncEvent<Func<T1, T2, T3, T4, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            IReadOnlyList<Func<T1, T2, T3, T4, Task>> subscribers = eventHandler.Subscriptions;

            foreach (Func<T1, T2, T3, T4, Task> subscriber in subscribers)
                await subscriber.Invoke(arg1, arg2, arg3, arg4)
                    .ConfigureAwait(false);
        }

        public static async Task InvokeAsync<T1, T2, T3, T4, T5>(this AsyncEvent<Func<T1, T2, T3, T4, T5, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            IReadOnlyList<Func<T1, T2, T3, T4, T5, Task>> subscribers = eventHandler.Subscriptions;

            foreach (Func<T1, T2, T3, T4, T5, Task> subscriber in subscribers)
                await subscriber.Invoke(arg1, arg2, arg3, arg4, arg5)
                    .ConfigureAwait(false);
        }
    }
}