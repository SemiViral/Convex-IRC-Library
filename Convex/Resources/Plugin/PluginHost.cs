#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Convex.Types;
using Convex.Types.Events;
using Serilog;

#endregion

namespace Convex.Resources.Plugin {
    public class PluginHost {
        private const string PLUGIN_MASK = "Convex.*.dll";
        private static readonly string _pluginsDirectory = $"{AppContext.BaseDirectory}\\Plugins";
        private readonly AsyncEvent<Func<ActionEventArgs, Task>> pluginCallbackEvent = new AsyncEvent<Func<ActionEventArgs, Task>>();

        public List<PluginInstance> Plugins = new List<PluginInstance>();

        public bool ShuttingDown { get; private set; }

        public Dictionary<string, string> Commands { get; } = new Dictionary<string, string>();

        private List<MethodsContainer> MethodContainers { get; } = new List<MethodsContainer>();

        public event Func<ActionEventArgs, Task> PluginCallback {
            add { pluginCallbackEvent.Add(value); }
            remove { pluginCallbackEvent.Add(value); }
        }

        public void StartPlugins() {
            foreach (PluginInstance pluginInstance in Plugins)
                pluginInstance.Instance.Start();
        }

        public void StopPlugins() {
            Log.Warning($"STOP PLUGINS RECIEVED — shutting down.");
            ShuttingDown = true;

            foreach (PluginInstance pluginInstance in Plugins)
                pluginInstance.Instance.Stop();
        }

        public async Task InvokeAsync(ChannelMessagedEventArgs e) {
            if (ContainerByType(e.Message.Type) == null)
                return;

            await ContainerByType(e.Message.Type)
                .InvokeAsync(e);
        }

        public void RegisterMethod(MethodRegistrar methodRegistrar) {
            if (ContainerByType(methodRegistrar.CommandType) == null)
                MethodContainers.Add(new MethodsContainer(methodRegistrar.CommandType));

            // check whether commands exist and add to list
            if (!methodRegistrar.Definition.Equals(default(KeyValuePair<string, string>)))
                if (Commands.ContainsKey(methodRegistrar.Definition.Key))
                    Log.Information($"'{methodRegistrar.Definition.Key}' command already exists, skipping entry.");
                else
                    Commands.Add(methodRegistrar.Definition.Key, methodRegistrar.Definition.Value);

            ContainerByType(methodRegistrar.CommandType)
                .ChannelMessaged += methodRegistrar.Method;
        }

        private MethodsContainer ContainerByType(string type) {
            return MethodContainers.SingleOrDefault(container => container.Command.Equals(type));
        }

        /// <summary>
        ///     Loads all plugins
        /// </summary>
        public void LoadPlugins() {
            if (!Directory.Exists(_pluginsDirectory))
                Directory.CreateDirectory(_pluginsDirectory);

            // array of all filepaths that are found to match the PLUGIN_MASK
            string[] pluginMatchAddresses = Directory.GetFiles(_pluginsDirectory, PLUGIN_MASK, SearchOption.AllDirectories);

            if (pluginMatchAddresses.Length.Equals(0)) {
                Log.Information("No plugins to load.");
                return;
            }

            foreach (string plugin in pluginMatchAddresses)
                try {
                    Assembly.Load(new AssemblyName(plugin));

                    IPlugin instance = GetPluginInstances(plugin);
                    instance.Callback += OnPluginCallback;
                    AddPlugin(instance, false);
                } catch (ReflectionTypeLoadException ex) {
                    foreach (Exception loaderException in ex.LoaderExceptions)
                        Log.Error(loaderException, $"LoaderException occured loading plugin: {plugin}");
                } catch (Exception ex) {
                    Log.Error(ex, $"Error occured loading plugin: {plugin}");
                }
        }

        /// <summary>
        ///     Gets instance of plugin by assembly name
        /// </summary>
        /// <param name="assemblyName">full name of assembly</param>
        /// <returns></returns>
        private static IPlugin GetPluginInstances(string assemblyName) {
            return (IPlugin)Activator.CreateInstance(GetTypeInstance(assemblyName), null, null);
        }

        /// <summary>
        ///     Gets the IPlugin type instance from an assembly name
        /// </summary>
        /// <param name="assemblyName">full name of assembly</param>
        /// <returns></returns>
        private static Type GetTypeInstance(string assemblyName) {
            AssemblyInstanceInfo pluginAssembly = new AssemblyInstanceInfo(Assembly.Load(new AssemblyName(assemblyName)));

            return typeof(IPlugin).GetTypeInfo()
                .IsAssignableFrom(pluginAssembly.Type.GetTypeInfo())
                .GetType();
        }

        /// <summary>
        ///     Adds IPlugin instance to internal list
        /// </summary>
        /// <param name="plugin">plugin instance</param>
        /// <param name="autoStart">start plugin immediately</param>
        public void AddPlugin(IPlugin plugin, bool autoStart) {
            try {
                Plugins.Add(new PluginInstance(plugin, PluginStatus.Stopped));

                if (autoStart)
                    plugin.Start();
            } catch (Exception ex) {
                Log.Error(ex, $"Error adding plugin: {ex.Message}");
            }
        }

        private async Task OnPluginCallback(ActionEventArgs e) {
            await pluginCallbackEvent.InvokeAsync(e);
        }
    }

    internal class MethodsContainer {
        private readonly AsyncEvent<Func<ChannelMessagedEventArgs, Task>> channelMessageEvent;

        public MethodsContainer(string command) {
            Command = command;
            channelMessageEvent = new AsyncEvent<Func<ChannelMessagedEventArgs, Task>>();
        }

        public string Command { get; }

        public event Func<ChannelMessagedEventArgs, Task> ChannelMessaged {
            add { channelMessageEvent.Add(value); }
            remove { channelMessageEvent.Remove(value); }
        }

        public async Task InvokeAsync(ChannelMessagedEventArgs e) {
            await channelMessageEvent.InvokeAsync(e);
        }
    }
}