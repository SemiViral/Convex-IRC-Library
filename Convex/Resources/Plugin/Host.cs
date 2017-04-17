#region usings

using System;
using System.Collections.Generic;
using Convex.Types.Events;

#endregion

namespace Convex.Resources.Plugin {
    internal class PluginHost : MarshalByRefObject {
        private const string DOMAIN_NAME_PLUGINS = "DOM_PLUGINS";
        private readonly string pluginsDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}\\Plugins";

        private PluginController pluginController;

        private AppDomain pluginDomain;

        public PluginHost() {
            Initialise();
        }

        private Dictionary<string, string> Commands { get; set; }

        private Dictionary<string, List<PluginMethodWrapper>> PluginEvents { get; set; }

        public event EventHandler<ActionEventArgs> PluginCallback;

        public Dictionary<string, string> GetCommands() => Commands;

        /// <summary>
        ///     Intermediary method for activating PluginCallback
        /// </summary>
        private void PluginsCallback(object sender, ActionEventArgs e) {
            PluginCallback?.Invoke(this, e);
        }

        public void Initialise() {
            if (pluginDomain != null)
                return;

            pluginDomain = AppDomain.CreateDomain(DOMAIN_NAME_PLUGINS);
            PluginEvents = new Dictionary<string, List<PluginMethodWrapper>>();
            Commands = new Dictionary<string, string>();
        }

        public void LoadPluginDomain() {
            Initialise();
            InitialiseController();
        }

        public void UnloadPluginDomain() {
            if (pluginDomain.Equals(null))
                return;

            AppDomain.Unload(pluginDomain);
        }

        private void InitialiseController() {
            pluginController = (PluginController)pluginDomain.CreateInstanceAndUnwrap(typeof(PluginController).Assembly.FullName, typeof(PluginController).FullName);

            pluginController.PluginsCallback += PluginsCallback;
            pluginController.LoadPlugins(pluginsDirectory);
        }

        public void StartPlugins() {
            pluginController?.StartPlugins();
        }

        public void StopPlugins() {
            Log(IrcLogEntryType.System, $"<{DOMAIN_NAME_PLUGINS}> UNLOAD ALL RECIEVED — shutting down.");

            if (pluginController.Equals(null))
                return;

            pluginController.IsShuttingDown = true;
            pluginController.StopPlugins();
        }

        public void InvokeMethods(ChannelMessagedEventArgs e) {
            if (!PluginEvents.ContainsKey(e.Message.Type))
                return;

            foreach (PluginMethodWrapper pluginRegistrar in PluginEvents[e.Message.Type])
                pluginRegistrar.Invoke(this, e);
        }

        public void RegisterMethod(MethodRegistrar methodRegistrar) {
            if (!PluginEvents.ContainsKey(methodRegistrar.CommandType))
                PluginEvents.Add(methodRegistrar.CommandType, new List<PluginMethodWrapper>());

            // check whether commands exist and add to list
            if (!methodRegistrar.Definition.Equals(default(KeyValuePair<string, string>)))
                if (Commands.ContainsKey(methodRegistrar.Definition.Key))
                    Log(IrcLogEntryType.Warning, $"'{methodRegistrar.Definition.Key}' command already exists, skipping entry.");
                else
                    Commands.Add(methodRegistrar.Definition.Key, methodRegistrar.Definition.Value);

            PluginEvents[methodRegistrar.CommandType].Add(methodRegistrar.Method);
        }

        private void Log(IrcLogEntryType entryType, string message) {
            PluginsCallback(this, new ActionEventArgs(PluginActionType.Log, new LogEntry(entryType, message)));
        }
    }
}