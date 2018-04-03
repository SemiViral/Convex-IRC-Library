#region usings

using System;
using Convex.Plugin;

#endregion

namespace Convex.ComponentModel.Event {
    public class ActionEventArgs : EventArgs {
        public ActionEventArgs(PluginActionType actionType, object result = null, string pluginName = "") {
            Result = result;
            ActionType = actionType;
            PluginName = pluginName;
        }

        public PluginActionType ActionType { get; }
        public object Result { get; }
        public string PluginName { get; set; }
    }
}