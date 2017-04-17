#region usings

using System;
using System.Reflection;

#endregion

namespace Convex.Resources.Plugin {
    /// <summary>
    ///     Interface for hooking a new plugin into Eve
    /// </summary>
    public interface IPlugin {
        string Name { get; }
        string Author { get; }
        string Version { get; }
        string Id { get; }
        PluginStatus Status { get; }

        void Start();
        void Stop();
        void Call_Die();
        void Log(IrcLogEntryType logType, string message);

        event EventHandler<ActionEventArgs> CallbackEvent;
    }

    [Serializable]
    public class SimpleMessageEventArgs : EventArgs {
        public SimpleMessageEventArgs(string command, string target, params string[] args) {
            Command = command;
            Target = target;
            Args = string.Join(" ", args);
        }

        public override string ToString() {
            return $"{Command} {Target} {Args}";
        }

        public string Command { get; set; }
        public string Target { get; set; }
        public string Args { get; set; }
    }

    [Serializable]
    public class ActionEventArgs : EventArgs {
        public PluginActionType ActionType;
        public string ExecutingDomain;
        public string PluginName;
        public object Result;

        public ActionEventArgs(PluginActionType actionType, object result = null) {
            Result = result;
            ActionType = actionType;
            ExecutingDomain = AppDomain.CurrentDomain.FriendlyName;
            PluginName = Assembly.GetExecutingAssembly().GetName().Name;
        }
    }

    public enum PluginActionType {
        Log,
        Load,
        Unload,
        RegisterMethod,
        SendMessage,
        SignalTerminate
    }

    public enum PluginStatus {
        Stopped = 0,
        Running,
        Processing
    }
}