#region usings

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace Convex.Plugin.Registrar {
    public interface IAsyncRegistrar<in T> {
        Predicate<T> CanExecute { get; }
        Func<T, Task> Composition { get; }
        KeyValuePair<string, string> Description { get; }
        string Command { get; }
        bool IsRegistered { get; }

    }
}