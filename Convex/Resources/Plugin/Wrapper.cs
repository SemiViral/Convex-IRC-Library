#region usings

using System;
using Convex.Net;

#endregion

namespace Convex.Resources.Plugin {
    public class Wrapper : MarshalByRefObject {
        public Host Host;

        internal event EventHandler TerminateBotEvent;
        internal event EventHandler<LogEntryEventArgs> LogEntryEventHandler;
        internal event EventHandler<SimpleMessageEventArgs> SimpleMessageEventHandler;

        private void Callback(object source, ActionEventArgs e) {
            switch (e.ActionType) {
                case PluginActionType.Load:
                    Host.LoadPluginDomain();
                    break;
                case PluginActionType.Unload:
                    Host.UnloadPluginDomain();
                    break;
                case PluginActionType.SignalTerminate:
                    TerminateBotEvent?.Invoke(this, EventArgs.Empty);
                    break;
                case PluginActionType.RegisterMethod:
                    if (!(e.Result is MethodRegistrar))
                        break;

                    Host.RegisterMethod((MethodRegistrar)e.Result);
                    break;
                case PluginActionType.SendMessage:
                    if (!(e.Result is SimpleMessageEventArgs))
                        break;

                    SimpleMessageEventHandler?.Invoke(this, (SimpleMessageEventArgs)e.Result);
                    break;
                case PluginActionType.Log:
                    if (!(e.Result is LogEntryEventArgs))
                        break;

                    LogEntryEventHandler?.Invoke(this, (LogEntryEventArgs)e.Result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Start() {
            if (Host != null)
                return;

            Host = new Host();
            Host.PluginCallback += Callback;
            Host.LoadPluginDomain();
            Host.StartPlugins();
        }
    }
}