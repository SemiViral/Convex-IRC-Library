#region usings

using System;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Registrar;

#endregion

namespace Convex.Plugin {
    internal class PluginWrapper {
        internal PluginHost Host;

        internal event AsyncEventHandler<CommandEventArgs> CommandRecieved;
        internal event AsyncEventHandler<BasicEventArgs> Log;
        internal event AsyncEventHandler Terminated;

        private async Task Callback(object source, ActionEventArgs e) {
            if (Host.ShuttingDown)
                return;

            switch (e.ActionType) {
                case PluginActionType.SignalTerminate:
                    if (Terminated == null)
                        break;

                    await Terminated.Invoke(this, EventArgs.Empty);
                    break;
                case PluginActionType.RegisterMethod:
                    if (!(e.Result is IAsyncRegistrar<ServerMessagedEventArgs>))
                        break;

                    Host.RegisterMethod((IAsyncRegistrar<ServerMessagedEventArgs>)e.Result);
                    break;
                case PluginActionType.SendMessage:
                    if (!(e.Result is CommandEventArgs) ||
                        CommandRecieved == null)
                        break;

                    await CommandRecieved.Invoke(this, (CommandEventArgs)e.Result);
                    break;
                case PluginActionType.Log:
                    if (!(e.Result is string))
                        break;

                    if (Log == null)
                        return;

                    await Log.Invoke(this, new BasicEventArgs((string)e.Result));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Initialise() {
            if (Host != null)
                return;

            Host = new PluginHost();
            Host.PluginCallback += Callback;
            Host.LoadPlugins();
            Host.StartPlugins();
        }
    }
}