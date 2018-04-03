#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Convex.ComponentModel.Event;
using Convex.Plugin.Registrar;

#endregion

namespace Convex.Plugin {
    internal class PluginHost {
        private const string _PluginMask = "Convex.*.dll";
        private static readonly string _PluginsDirectory = $"{AppContext.BaseDirectory}\\Plugins";

        private Dictionary<string, AsyncEventHandler<ServerMessagedEventArgs>> CompositionHandlers { get; } = new Dictionary<string, AsyncEventHandler<ServerMessagedEventArgs>>();
        private List<PluginInstance> Plugins { get; } = new List<PluginInstance>();

        public bool ShuttingDown { get; private set; }

        public Dictionary<string, Tuple<string, string>> DescriptionRegistry { get; } = new Dictionary<string, Tuple<string, string>>();

        public event AsyncEventHandler<ActionEventArgs> PluginCallback;

        public void StartPlugins() {
            foreach (PluginInstance pluginInstance in Plugins)
                pluginInstance.Instance.Start();
        }

        public void StopPlugins() {
            Debug.WriteLine("STOP PLUGINS RECIEVED — shutting down.");
            ShuttingDown = true;

            foreach (PluginInstance pluginInstance in Plugins)
                pluginInstance.Instance.Stop();
        }

        public async Task InvokeAsync(ServerMessagedEventArgs args) {
            if (!CompositionHandlers.ContainsKey(args.Message.Command))
                return;

            await CompositionHandlers[args.Message.Command].Invoke(this, args);
        }

        public void RegisterMethod(IAsyncRegistrar<ServerMessagedEventArgs> registrar) {
            AddComposition(registrar);

            if (DescriptionRegistry.Keys.Contains(registrar.UniqueId))
                Debug.WriteLine($"'{registrar.UniqueId}' description already exists, skipping entry.");
            else
                DescriptionRegistry.Add(registrar.UniqueId, registrar.Description);
        }

        private void AddComposition(IAsyncRegistrar<ServerMessagedEventArgs> registrar) {
            if (!CompositionHandlers.ContainsKey(registrar.Command))
                CompositionHandlers.Add(registrar.Command, null);

            CompositionHandlers[registrar.Command] += async (source, e) => {
                if (!registrar.CanExecute(e))
                    return;

                await registrar.Composition(e);
            };
        }


        /// <summary>
        ///     Loads all plugins
        /// </summary>
        public void LoadPlugins() {
            if (!Directory.Exists(_PluginsDirectory))
                Directory.CreateDirectory(_PluginsDirectory);

            try {
                // array of all filepaths that are found to match the PLUGIN_MASK

#if DEBUG
                IEnumerable<IPlugin> pluginInstances = Directory.GetFiles("C:\\Users\\semiv\\OneDrive\\Documents\\GitHub\\Convex-IRC-Library\\Convex.Plugin.Core\\bin\\Debug\\netstandard2.0", _PluginMask, SearchOption.AllDirectories).SelectMany(GetPluginInstances);
#else
                IEnumerable<IPlugin> pluginInstances = Directory.GetFiles(_PluginsDirectory, _PluginMask, SearchOption.AllDirectories).SelectMany(GetPluginInstances);
#endif

                foreach (IPlugin plugin in pluginInstances) {
                    plugin.Callback += OnPluginCallback;
                    AddPlugin(plugin, false);
                }
            } catch (ReflectionTypeLoadException ex) {
                foreach (Exception loaderException in ex.LoaderExceptions)
                    Debug.WriteLine(loaderException, "LoaderException occured loading a plugin");
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
            return GetTypeInstances(GetAssembly(assemblyName)).Select(type => (IPlugin)Activator.CreateInstance(type));
        }

        /// <summary>
        ///     Gets the IPlugin type instance from an assembly name
        /// </summary>
        /// <param name="assembly">assembly instance</param>
        /// <returns></returns>
        private static IEnumerable<Type> GetTypeInstances(Assembly assembly) {
            return assembly.GetTypes().Where(type => type.GetTypeInfo().GetInterfaces().Contains(typeof(IPlugin)));
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
                Plugins.Add(new PluginInstance(plugin, PluginStatus.Stopped));

                if (autoStart)
                    plugin.Start();
            } catch (Exception ex) {
                Debug.WriteLine(ex, $"Error adding plugin: {ex.Message}");
            }
        }

        private async Task OnPluginCallback(object source, ActionEventArgs e) {
            if (PluginCallback == null)
                return;

            await PluginCallback.Invoke(source, e);
        }
    }
}
