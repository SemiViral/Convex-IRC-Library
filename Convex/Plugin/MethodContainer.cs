#region usings

using System;
using System.Threading.Tasks;
using Convex.Event;

#endregion

namespace Convex.Plugin {
    internal class MethodsContainer<TEventArgs>
        where TEventArgs : EventArgs {
        public MethodsContainer(string command) {
            Command = command;
        }

        public string Command { get; }
        private event AsyncEventHandler<TEventArgs> ChannelMessaged;

        public void SubmitRegistrar(MethodRegistrar<TEventArgs> registrar) {
            ChannelMessaged += async (source, e) => {
                if (!registrar.PreprocessCheck(e))
                    return;
                await registrar.Method(e);
            };
        }

        public async Task InvokeAllAsync(object source, TEventArgs e) {
            if (ChannelMessaged == null)
                return;

            await ChannelMessaged.Invoke(source, e);
        }
    }
}