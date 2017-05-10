#region usings

using System;
using System.Threading.Tasks;
using Convex.Types.Events;

#endregion

namespace Convex.Resources.Plugin {
    /// <summary>
    ///     Interface for hooking a new plugin into Eve
    /// </summary>
    public interface IPlugin {
        string Name { get; }
        string Author { get; }
        Version Version { get; }
        string Id { get; }
        PluginStatus Status { get; }
        AsyncEvent<Func<ActionEventArgs, Task>> CallbackEvent { get; set; }

        Task Start();
        Task Stop();
        Task Call_Die();

        event Func<ActionEventArgs, Task> Callback;
    }

    public class PluginInstance {
        public readonly IPlugin Instance;
        public PluginStatus Status;

        public PluginInstance(IPlugin instance, PluginStatus status) {
            Instance = instance;
            Status = status;
        }
    }

    [Serializable]
    public class ActionEventArgs : EventArgs {
        public readonly PluginActionType ActionType;
        //public string ExecutingDomain;
        //public string PluginName;
        public readonly object Result;

        public ActionEventArgs(PluginActionType actionType, object result = null) {
            Result = result;
            ActionType = actionType;
            //ExecutingDomain = AppDomain.CurrentDomain.FriendlyName;
            //PluginName = Assembly.GetExecutingAssembly().GetName().Name;
        }
    }

    public enum PluginActionType {
        Log,
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