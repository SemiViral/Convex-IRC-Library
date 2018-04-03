#region usings

using System;

#endregion

namespace Convex.ComponentModel.Event {
    public class BasicEventArgs : EventArgs {
        public BasicEventArgs(string contents) {
            Contents = contents;
        }

        public DateTime Timestamp { get; } = DateTime.Now;
        public string Contents { get; set; }
    }
}