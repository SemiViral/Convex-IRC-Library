#region usings

using System;
using System.Threading.Tasks;
using Convex.Event;

#endregion

namespace Convex.Plugin.Registrar {
    internal class MethodsContainer<TEventArgs>
        where TEventArgs : EventArgs {
        private event AsyncEventHandler<TEventArgs> ChannelMessaged;

        public void Register(Predicate<TEventArgs> canExecute, Func<TEventArgs, Task> method) {
            ChannelMessaged += async (source, e) => {
                if (!canExecute(e)) {
                    return;
                }

                await method(e);
            };
        }

        public async Task InvokeAsync(object source, TEventArgs args) {
            if (ChannelMessaged == null) {
                return;
            }

            await ChannelMessaged.Invoke(source, args);
        }
    }
}