#region usings

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Convex.Resource.Reference;

#endregion

namespace Convex.Plugin.Registrar {
    public class MethodRegistrar<TEventArgs> : IAsyncRegistrar<TEventArgs>
        where TEventArgs : EventArgs {
        /// <summary>
        ///     Creates a new instance of MethodRegistrar
        /// </summary>
        /// <param name="canExecute">defines execution readiness</param>
        /// <param name="command">command to reference composition</param>
        /// <param name="composition">registrable composition to be executed</param>
        /// <param name="description">describes composition</param>
        public MethodRegistrar(Func<TEventArgs, Task> composition, Predicate<TEventArgs> canExecute, string command, KeyValuePair<string, string>? description) {
            IsRegistered = false;

            Method = composition;
            CanExecute = canExecute ?? (obj => true);
            Command = command ?? Commands.DEFAULT;
            Description = description ?? default(KeyValuePair<string, string>);

            IsRegistered = true;
        }

        public Func<TEventArgs, Task> Method { get; }
        public Predicate<TEventArgs> CanExecute { get; }

        public string Command { get; }
        public KeyValuePair<string, string> Description { get; }
        public bool IsRegistered { get; }
    }
}