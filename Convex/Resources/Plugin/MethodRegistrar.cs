#region usings

using System;
using System.Collections.Generic;
using Convex.Types;

#endregion

namespace Convex.Resources.Plugin {
    public class MethodRegistrar : MarshalByRefObject {
        public readonly PluginMethodWrapper Method;

        /// <summary>
        ///     This is the constructor for a PluginRegistrarEventArgs
        /// </summary>
        /// <param name="commandType">This represents the IRC command the method is triggered by</param>
        /// <param name="pluginMethod">The method instance itself</param>
        /// <param name="definition">
        ///     This is an optional <see cref="KeyValuePair{TKey,TValue}" /> that is added to the commands
        ///     index
        /// </param>
        public MethodRegistrar(string commandType, Action<object, ChannelMessagedEventArgs> pluginMethod, KeyValuePair<string, string> definition = default(KeyValuePair<string, string>)) {
            CommandType = commandType;
            Method = new PluginMethodWrapper(pluginMethod);
            Definition = definition;
        }

        public string CommandType { get; }

        public KeyValuePair<string, string> Definition { get; }
    }

    /// <summary>
    ///     This class is used to wrap the plugin method instances in an object type that can be marshalled across the
    ///     app-domain boundary.
    ///     This is necessary due to <see cref="List{T}" /> not being marhallable, thusly the contents of the list must be
    ///     instead.
    /// </summary>
    public sealed class PluginMethodWrapper : MarshalByRefObject {
        private readonly Action<object, ChannelMessagedEventArgs> internalDelegate;

        public PluginMethodWrapper(Action<object, ChannelMessagedEventArgs> method) {
            internalDelegate = method;
        }

        public void Invoke(object source, ChannelMessagedEventArgs channelMessage) {
            internalDelegate.Invoke(source, channelMessage);
        }
    }
}