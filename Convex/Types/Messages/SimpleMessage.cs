#region usings

using System;

#endregion

namespace Convex.Types.Messages {
    [Serializable]
    public class SimpleMessage : Message {
        public SimpleMessage(string command, string target, string args) {
            Command = command;
            Target = target;
            Args = args;
        }

        public string Target { get; set; }

        public override string ToString() {
            return $"{Command} {Target} {Args}";
        }
    }
}