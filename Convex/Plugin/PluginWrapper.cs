#region usings

using System;
using System.Threading.Tasks;
using Convex.Event;
using Serilog;

#endregion

namespace Convex.Plugin {
    internal class PluginWrapper {
        internal PluginHost Host;
        public event AsyncEventHandler<CommandEventArgs> CommandRecieved;

        internal event AsyncEventHandler Terminated;

        private async Task Callback(object source, ActionEventArgs e) {
            switch (e.ActionType) {
                case PluginActionType.SignalTerminate:
                    if (Terminated == null)
                        break;

                    await Terminated.Invoke(this, EventArgs.Empty);
                    break;
                case PluginActionType.RegisterMethod:
                    if (!(e.Result is MethodRegistrar<ServerMessagedEventArgs>))
                        break;

                    Host.RegisterMethod((MethodRegistrar<ServerMessagedEventArgs>)e.Result);
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

                    Log.Information((string)e.Result);
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