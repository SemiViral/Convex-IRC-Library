#region usings

using System;
using System.Threading.Tasks;
using Convex.ComponentModel.Event;

#endregion

namespace Convex.Plugin {
    /// <summary>
    ///     Interface for hooking a new plugin into Eve
    /// </summary>
    public interface IPlugin {
        string Name { get; }
        string Author { get; }
        Version Version { get; }
        string Id { get; }
        PluginStatus Status { get; }

        /// <summary>
        ///     Often used to register methods
        /// </summary>
        Task Start();

        Task Stop();
        Task Call_Die();

        event AsyncEventHandler<ActionEventArgs> Callback;
    }

    public class PluginInstance {
        public readonly IPlugin Instance;
        public PluginStatus Status;

        public PluginInstance(IPlugin instance, PluginStatus status) {
            Instance = instance;
            Status = status;
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