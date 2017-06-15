#region usings



#endregion

namespace Convex.Event {
    public class CommandEventArgs : BasicEventArgs {
        public CommandEventArgs(string command, string target, string contents) : base(contents) {
            Command = command;
            Target = target;
        }

        public string Command { get; set; }
        public string Target { get; set; }

        public override string ToString() {
            return $"{Command} {Target} {Contents}";
        }

        public static implicit operator string(CommandEventArgs message) {
            return $"{message.Command} {message.Target} {message.Contents}";
        }
    }
}