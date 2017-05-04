#region usings

using System;
using System.Reflection;
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
        string Version { get; }
        string Id { get; }
        PluginStatus Status { get; }

        Task Start();
        Task Stop();
        Task Call_Die();

        event Func<ActionEventArgs, Task> Callback;
        AsyncEvent<Func<ActionEventArgs, Task>> CallbackEvent { get; set; }
    }

    public class AssemblyInstanceInfo
    {
        public AssemblyInstanceInfo(Assembly baseAssembly)
        {
            Assembly = baseAssembly;
        }

        public Assembly Assembly { get; }

        public Type Type => Assembly.GetType();
    }

    public class PluginInstance
    {
        public IPlugin Instance;
        public PluginStatus Status;

        public PluginInstance(IPlugin instance, PluginStatus status)
        {
            Instance = instance;
            Status = status;
        }
    }

    [Serializable]
    public class SimpleMessageEventArgs : EventArgs {
        public SimpleMessageEventArgs(string command, string target, string message) {
            Command = command;
            Target = target;
            Message = message;
        }

        public string Address { get; set; }
        public string Command { get; set; }
        public string Target { get; set; }
        public string Message { get; set; }

        public override string ToString() {
            return $"{Command} {Target} {Message}";
        }
    }

    [Serializable]
    public class ActionEventArgs : EventArgs {
        public PluginActionType ActionType;
        //public string ExecutingDomain;
        //public string PluginName;
        public object Result;

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