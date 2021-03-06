﻿#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Registrar;

#endregion

namespace Convex.Plugin {
    internal class PluginHost {
        private const string PLUGIN_MASK = "Convex.*.dll";
        private static readonly string pluginsDirectory = $"{AppContext.BaseDirectory}\\Plugins";

        private readonly MethodDomain<ServerMessagedEventArgs> methods = new MethodDomain<ServerMessagedEventArgs>();
        private readonly List<PluginInstance> plugins = new List<PluginInstance>();

        public bool ShuttingDown { get; private set; }

        public Dictionary<string, string> DescriptionRegistry { get; } = new Dictionary<string, string>();

        public event AsyncEventHandler<ActionEventArgs> PluginCallback;

        public void StartPlugins() {
            foreach (PluginInstance pluginInstance in plugins) {
                pluginInstance.Instance.Start();
            }
        }

        public void StopPlugins() {
            Debug.WriteLine("STOP PLUGINS RECEIVED — shutting down.");
            ShuttingDown = true;

            foreach (PluginInstance pluginInstance in plugins) {
                pluginInstance.Instance.Stop();
            }
        }

        public async Task InvokeAsync(object source, ServerMessagedEventArgs e) {
            await methods.InvokeAsync(source, e);
        }

        public void RegisterMethod(IAsyncRegistrar<ServerMessagedEventArgs> methodRegistrar) {
            if (methodRegistrar.Identifier == null) {
                return;
            }

            if (!methodRegistrar.Description.Equals(default(KeyValuePair<string, string>))) {
                if (DescriptionRegistry.Keys.Contains(methodRegistrar.Description.Key)) {
                    Debug.WriteLine($"'{methodRegistrar.Description.Key}' description already exists, skipping entry.");
                } else {
                    DescriptionRegistry.Add(methodRegistrar.Description.Key, methodRegistrar.Description.Value);
                }
            }

            methods.SubmitRegistrar(methodRegistrar);

            methodRegistrar.IsRegistered = true;
        }

        /// <summary>
        ///     Loads all plugins
        /// </summary>
        public void LoadPlugins() {
            if (!Directory.Exists(pluginsDirectory)) {
                Directory.CreateDirectory(pluginsDirectory);
            }

            try {
                // array of all filepaths that are found to match the PLUGIN_MASK
                IEnumerable<IPlugin> pluginInstances = Directory.GetFiles(pluginsDirectory, PLUGIN_MASK, SearchOption.AllDirectories).
                    SelectMany(GetPluginInstances);

                foreach (IPlugin plugin in pluginInstances) {
                    plugin.Callback += OnPluginCallback;
                    AddPlugin(plugin, false);
                }
            } catch (ReflectionTypeLoadException ex) {
                foreach (Exception loaderException in ex.LoaderExceptions) {
                    Debug.WriteLine(loaderException, "LoaderException occured loading a plugin");
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex, "Error occured loading a plugin");
            }
        }

        /// <summary>
        ///     Gets instance of plugin by assembly name
        /// </summary>
        /// <param name="assemblyName">full name of assembly</param>
        /// <returns></returns>
        private static IEnumerable<IPlugin> GetPluginInstances(string assemblyName) {
            return GetTypeInstances(GetAssembly(assemblyName)).
                Select(type => (IPlugin)Activator.CreateInstance(type));
        }

        /// <summary>
        ///     Gets the IPlugin type instance from an assembly name
        /// </summary>
        /// <param name="assembly">assembly instance</param>
        /// <returns></returns>
        private static IEnumerable<Type> GetTypeInstances(Assembly assembly) {
            return assembly.GetTypes().
                Where(type => type.GetTypeInfo().
                    GetInterfaces().
                    Contains(typeof(IPlugin)));
        }

        private static Assembly GetAssembly(string assemblyName) {
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyName);
        }

        /// <summary>
        ///     Adds IPlugin instance to internal list
        /// </summary>
        /// <param name="plugin">plugin instance</param>
        /// <param name="autoStart">start plugin immediately</param>
        private void AddPlugin(IPlugin plugin, bool autoStart) {
            try {
                plugins.Add(new PluginInstance(plugin, PluginStatus.Stopped));

                if (autoStart) {
                    plugin.Start();
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex, $"Error adding plugin: {ex.Message}");
            }
        }

        private async Task OnPluginCallback(object source, ActionEventArgs e) {
            if (PluginCallback == null) {
                return;
            }

            await PluginCallback.Invoke(source, e);
        }
    }
}