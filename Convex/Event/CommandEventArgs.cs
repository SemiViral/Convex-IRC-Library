#region usings

using System;

#endregion

namespace Convex.Event {
    public class CommandEventArgs : EventArgs {
        public CommandEventArgs(string command, string target, string args) {
            Command = command;
            Target = target;
            Args = args;
        }

        public string Command { get; set; }
        public string Target { get; set; }
        public string Args { get; set; }

        public override string ToString() {
            return $"{Command} {Target} {Args}";
        }

        public static implicit operator string(CommandEventArgs message) {
            return $"{message.Command} {message.Target} {message.Args}";
        }
    }
}