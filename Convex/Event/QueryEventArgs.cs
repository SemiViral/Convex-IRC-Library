#region usings

using System;

#endregion

namespace Convex.Event {
    public class QueryEventArgs : EventArgs {
        public QueryEventArgs(string args) {
            Query = args;
        }

        public string Query { get; }
    }
}