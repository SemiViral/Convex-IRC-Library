#region usings

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Registrar;

#endregion

namespace Convex.Plugin {
    internal class MethodsContainer<TEventArgs>
        where TEventArgs : EventArgs {
        public MethodsContainer(string command) {
            Debug.WriteLine($"Created method container for {command}");

            Command = command;
        }

        public string Command { get; }
        private event AsyncEventHandler<TEventArgs> ChannelMessaged;

        public void SubmitRegistrar(IAsyncRegistrar<TEventArgs> registrar) {
            ChannelMessaged += async (source, e) => {
                if (!registrar.CanExecute(e))
                    return;

                await registrar.Composition(e);
            };
        }

        public async Task InvokeAllAsync(object source, TEventArgs e) {
            if (ChannelMessaged == null)
                return;

            await ChannelMessaged.Invoke(source, e);
        }
    }
}