#region usings

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Convex.Event;

#endregion

namespace Convex.Plugin.Registrar
{
    public class MethodDomain<TEventArgs> where TEventArgs : EventArgs
    {
        private readonly Dictionary<string, MethodsContainer<TEventArgs>> methods = new Dictionary<string, MethodsContainer<TEventArgs>>();

        public void SubmitRegistrar(IAsyncRegistrar<TEventArgs> registrar) {
            if (!methods.ContainsKey(registrar.Command)) {
                methods.Add(registrar.Command, new MethodsContainer<TEventArgs>());
            }

            methods[registrar.Command].Register(registrar.CanExecute, registrar.Method);            
        }

        public async Task InvokeAsync(object source, TEventArgs args) {
            if (args is ServerMessagedEventArgs) {
                ServerMessagedEventArgs _args = args as ServerMessagedEventArgs;

                if (methods.ContainsKey(_args.Message.Command))
                    await methods[_args.Message.Command].InvokeAsync(source, args);
            }
        }
    }
}
