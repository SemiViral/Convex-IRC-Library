#region usings

using System;
using System.Threading.Tasks;
using Convex.ComponentModel.Reference;

#endregion

namespace Convex.Plugin.Registrar {
    public class MethodRegistrar<TEventArgs> : IAsyncRegistrar<TEventArgs> where TEventArgs : EventArgs {
        /// <summary>
        ///     Creates a new instance of MethodRegistrar
        /// </summary>
        /// <param name="canExecute">defines execution readiness</param>
        /// <param name="command">command to reference composition</param>
        /// <param name="composition">registrable composition to be executed</param>
        /// <param name="description">describes composition</param>
        public MethodRegistrar(Func<TEventArgs, Task> composition, Predicate<TEventArgs> canExecute, string command, Tuple<string, string> description) {
            IsRegistered = false;

            Composition = composition;
            CanExecute = canExecute ?? (obj => true);
            Command = command ?? Commands.DEFAULT;
            Description = description;

            IsRegistered = true;
        }

        public Func<TEventArgs, Task> Composition { get; }
        public Predicate<TEventArgs> CanExecute { get; }

        public string Command { get; }
        public Tuple<string, string> Description { get; }
        public bool IsRegistered { get; }

        public string UniqueId { get; } = Guid.NewGuid().ToString();
    }
}
