#region usings

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace Convex.Plugin {
    public class MethodRegistrar<TEventArgs>
        where TEventArgs : EventArgs {
        public readonly Func<TEventArgs, Task> Method;

        public readonly Predicate<TEventArgs> PreprocessCheck;

        /// <summary>
        ///     Creates a new instance of MethodRegistrar
        /// </summary>
        /// <param name="commandType">This represents the IRC command the method is triggered by</param>
        /// <param name="pluginMethod">The method instance itself</param>
        /// <param name="predicate">
        ///     This condition must be met before the assigned <see cref="Func{T, TResult}" /> can be processed
        /// </param>
        /// <param name="definition">
        ///     This is an optional <see cref="KeyValuePair{TKey,TValue}" /> that is added to the commands index
        /// </param>
        public MethodRegistrar(string commandType, Func<TEventArgs, Task> pluginMethod, Predicate<TEventArgs> predicate = null, KeyValuePair<string, string> definition = default(KeyValuePair<string, string>)) {
            CommandType = commandType;
            Method = pluginMethod;
            PreprocessCheck = predicate ?? (x => true);
            Definition = definition;
        }

        public string CommandType { get; }

        public KeyValuePair<string, string> Definition { get; }
    }
}