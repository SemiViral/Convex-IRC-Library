#region usings

using System;

#endregion

namespace Convex.Types.Messages {
    public class Message {
        public Message() {
            Timestamp = DateTime.Now;
        }

        public string Command { get; set; }
        public string Args { get; set; }
        public DateTime Timestamp { get; set; }
    }
}