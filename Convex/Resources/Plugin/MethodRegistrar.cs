#region usings

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Convex.Types.Messages;

#endregion

namespace Convex.Resources.Plugin {
    public class MethodRegistrar {
        public readonly Func<ChannelMessagedEventArgs, Task> Method;

        /// <summary>
        ///     This is the constructor for a PluginRegistrarEventArgs
        /// </summary>
        /// <param name="commandType">This represents the IRC command the method is triggered by</param>
        /// <param name="pluginMethod">The method instance itself</param>
        /// <param name="definition">
        ///     This is an optional <see cref="KeyValuePair{TKey,TValue}" /> that is added to the commands
        ///     index
        /// </param>
        public MethodRegistrar(string commandType, Func<ChannelMessagedEventArgs, Task> pluginMethod, KeyValuePair<string, string> definition = default(KeyValuePair<string, string>)) {
            CommandType = commandType;
            Method = pluginMethod;
            Definition = definition;
        }

        public string CommandType { get; }

        public KeyValuePair<string, string> Definition { get; }
    }
}