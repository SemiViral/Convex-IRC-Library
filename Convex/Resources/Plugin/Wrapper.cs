#region usings

using System;

#endregion

namespace Convex.Resources.Plugin {
    internal class PluginWrapper : MarshalByRefObject {
        internal PluginHost PluginHost;

        internal event EventHandler TerminateBotEvent;
        internal event EventHandler<LogEntry> LogEntryEventHandler;
        internal event EventHandler<SimpleMessageEventArgs> SimpleMessageEventHandler;

        private void PluginsCallback(object source, ActionEventArgs e) {
            switch (e.ActionType) {
                case PluginActionType.Load:
                    PluginHost.LoadPluginDomain();
                    break;
                case PluginActionType.Unload:
                    PluginHost.UnloadPluginDomain();
                    break;
                case PluginActionType.SignalTerminate:
                    PluginHost.StopPlugins();
                    PluginHost.UnloadPluginDomain();

                    TerminateBotEvent?.Invoke(this, EventArgs.Empty);
                    break;
                case PluginActionType.RegisterMethod:
                    if (!(e.Result is MethodRegistrar))
                        break;

                    PluginHost.RegisterMethod((MethodRegistrar)e.Result);
                    break;
                case PluginActionType.SendMessage:
                    if (!(e.Result is SimpleMessageEventArgs))
                        break;

                    SimpleMessageEventHandler?.Invoke(this, (SimpleMessageEventArgs)e.Result);
                    break;
                case PluginActionType.Log:
                    if (!(e.Result is LogEntry))
                        break;

                    LogEntryEventHandler?.Invoke(this, (LogEntry)e.Result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Start() {
            if (PluginHost != null)
                return;

            PluginHost = new PluginHost();
            PluginHost.PluginCallback += PluginsCallback;
            PluginHost.LoadPluginDomain();
            PluginHost.StartPlugins();
        }
    }
}