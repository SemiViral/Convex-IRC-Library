#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using Convex.Net;
using Convex.Types;

#endregion

namespace Convex.Resources.Plugin {
    public class Host : MarshalByRefObject {
        private const string DOMAIN_NAME_PLUGINS = "DOM_PLUGINS";
        private readonly string pluginsDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}\\Plugins";

        private Bridge bridge;

        private AppDomain pluginDomain;

        public Host() {
            Initialise();
        }

        public Dictionary<string, string> Commands { get; private set; }

        private List<MethodsContainer> MethodContainers { get; set; }

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
            MethodContainers = new List<MethodsContainer>();
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
            bridge = (Bridge)pluginDomain.CreateInstanceAndUnwrap(typeof(Bridge).Assembly.FullName, typeof(Bridge).FullName);

            bridge.PluginsCallback += PluginsCallback;
            bridge.LoadPlugins(pluginsDirectory);
        }

        public void StartPlugins() {
            bridge?.StartPlugins();
        }

        public void StopPlugins() {
            Log(IrcLogEntryType.System, $"<{DOMAIN_NAME_PLUGINS}> UNLOAD ALL RECIEVED — shutting down.");

            if (bridge.Equals(null))
                return;

            bridge.IsShuttingDown = true;
            bridge.StopPlugins();
        }

        public void InvokeMethods(ChannelMessagedEventArgs e) {
            if (ContainerByType(e.Message.Type) == null)
                return;

            ContainerByType(e.Message.Type).Invoke(this, e);
        }

        public void RegisterMethod(MethodRegistrar methodRegistrar) {
            if (ContainerByType(methodRegistrar.CommandType) == null)
                MethodContainers.Add(new MethodsContainer(methodRegistrar.CommandType));

            // check whether commands exist and add to list
            if (!methodRegistrar.Definition.Equals(default(KeyValuePair<string, string>)))
                if (Commands.ContainsKey(methodRegistrar.Definition.Key))
                    Log(IrcLogEntryType.Warning, $"'{methodRegistrar.Definition.Key}' command already exists, skipping entry.");
                else
                    Commands.Add(methodRegistrar.Definition.Key, methodRegistrar.Definition.Value);

            ContainerByType(methodRegistrar.CommandType).Subscribe(methodRegistrar.Method);
        }

        private MethodsContainer ContainerByType(string type) {
            return MethodContainers.SingleOrDefault(container => container.Command.Equals(type));
        }

        private void Log(IrcLogEntryType entryType, string message) {
            PluginsCallback(this, new ActionEventArgs(PluginActionType.Log, new LogEntryEventArgs(entryType, message)));
        }
    }

    internal class MethodsContainer : MarshalByRefObject {
        private readonly List<PluginMethodWrapper> methods;

        public MethodsContainer(string command) {
            Command = command;
            methods = new List<PluginMethodWrapper>();
        }

        public string Command { get; }

        public void Subscribe(PluginMethodWrapper method) {
            methods.Add(method);
        }

        public void Invoke(object sender, ChannelMessagedEventArgs e) {
            methods.ForEach(method => method.Invoke(this, e));
        }
    }
}