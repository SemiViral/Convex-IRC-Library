﻿#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Convex.Event;
using Serilog;

#endregion

namespace Convex.Plugin {
    internal class PluginHost {
        private const string PLUGIN_MASK = "Convex.*.dll";
        private static readonly string _pluginsDirectory = $"{AppContext.BaseDirectory}\\Plugins";

        private readonly List<PluginInstance> plugins = new List<PluginInstance>();

        public bool ShuttingDown { get; private set; }

        public Dictionary<string, string> Commands { get; } = new Dictionary<string, string>();

        private List<MethodsContainer<ServerMessagedEventArgs>> MethodContainers { get; } = new List<MethodsContainer<ServerMessagedEventArgs>>();
        public event AsyncEventHandler<ActionEventArgs> PluginCallback;

        public void StartPlugins() {
            foreach (PluginInstance pluginInstance in plugins)
                pluginInstance.Instance.Start();
        }

        public void StopPlugins() {
            Log.Warning("STOP PLUGINS RECIEVED — shutting down.");
            ShuttingDown = true;

            foreach (PluginInstance pluginInstance in plugins)
                pluginInstance.Instance.Stop();
        }

        public async Task InvokeAsync(ServerMessagedEventArgs e) {
            if (ContainerByType(e.Message.Command) == null)
                return;

            await ContainerByType(e.Message.Command)
                .InvokeAllAsync(this, e);
        }

        public void RegisterMethod(MethodRegistrar<ServerMessagedEventArgs> methodRegistrar) {
            if (ContainerByType(methodRegistrar.CommandType) == null)
                MethodContainers.Add(new MethodsContainer<ServerMessagedEventArgs>(methodRegistrar.CommandType));

            // check whether commands exist and add to list
            if (!methodRegistrar.Definition.Equals(default(KeyValuePair<string, string>)))
                if (Commands.ContainsKey(methodRegistrar.Definition.Key))
                    Log.Information($"'{methodRegistrar.Definition.Key}' command already exists, skipping entry.");
                else
                    Commands.Add(methodRegistrar.Definition.Key, methodRegistrar.Definition.Value);

            ContainerByType(methodRegistrar.CommandType)
                .SubmitRegistrar(methodRegistrar);
        }

        private MethodsContainer<ServerMessagedEventArgs> ContainerByType(string type) {
            return MethodContainers.SingleOrDefault(container => container.Command.Equals(type));
        }

        /// <summary>
        ///     Loads all plugins
        /// </summary>
        public void LoadPlugins() {
            if (!Directory.Exists(_pluginsDirectory))
                Directory.CreateDirectory(_pluginsDirectory);

            try {
                // array of all filepaths that are found to match the PLUGIN_MASK
                IEnumerable<IPlugin> pluginInstances = Directory.GetFiles(_pluginsDirectory, PLUGIN_MASK, SearchOption.AllDirectories)
                    .SelectMany(GetPluginInstances);

                foreach (IPlugin plugin in pluginInstances) {
                    plugin.Callback += OnPluginCallback;
                    AddPlugin(plugin, false);
                }
            } catch (ReflectionTypeLoadException ex) {
                foreach (Exception loaderException in ex.LoaderExceptions)
                    Log.Error(loaderException, "LoaderException occured loading a plugin");
            } catch (Exception ex) {
                Log.Error(ex, "Error occured loading a plugin");
            }
        }

        /// <summary>
        ///     Gets instance of plugin by assembly name
        /// </summary>
        /// <param name="assemblyName">full name of assembly</param>
        /// <returns></returns>
        private static IEnumerable<IPlugin> GetPluginInstances(string assemblyName) {
            return GetTypeInstances(GetAssembly(assemblyName))
                .Select(type => (IPlugin)Activator.CreateInstance(type));
        }

        /// <summary>
        ///     Gets the IPlugin type instance from an assembly name
        /// </summary>
        /// <param name="assembly">assembly instance</param>
        /// <returns></returns>
        private static IEnumerable<Type> GetTypeInstances(Assembly assembly) {
            return assembly.GetTypes()
                .Where(type => type.GetTypeInfo()
                    .GetInterfaces()
                    .Contains(typeof(IPlugin)));
        }

        private static Assembly GetAssembly(string assemblyName) {
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyName);
        }

        /// <summary>
        ///     Adds IPlugin instance to internal list
        /// </summary>
        /// <param name="plugin">plugin instance</param>
        /// <param name="autoStart">start plugin immediately</param>
        public void AddPlugin(IPlugin plugin, bool autoStart) {
            try {
                plugins.Add(new PluginInstance(plugin, PluginStatus.Stopped));

                if (autoStart)
                    plugin.Start();
            } catch (Exception ex) {
                Log.Error(ex, $"Error adding plugin: {ex.Message}");
            }
        }

        private async Task OnPluginCallback(object source, ActionEventArgs e) {
            if (PluginCallback == null)
                return;

            await PluginCallback.Invoke(source, e);
        }
    }
}