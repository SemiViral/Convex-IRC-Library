#region usings

using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

#endregion

namespace Convex.Net {
    public class Logger : Stream {
        public Logger(string address) : base(address) {
            Backlog = new StringBuilder();

            Timer recursiveBacklogTrigger = new Timer(5000);
            recursiveBacklogTrigger.Elapsed += async (source, e) => await LogBacklog(source, e);
            recursiveBacklogTrigger.Start();
        }

        private StringBuilder Backlog { get; }

        private async Task LogBacklog(object source, ElapsedEventArgs e) {
            if (Backlog.Length.Equals(0))
                return;

            await WriteAsync(Backlog.ToString().Trim());
            Backlog.Clear();
        }

        private void Log(IrcLogEntryType logType, string message, string memberName = "", int lineNumber = 0) {
            string timestamp = DateTime.Now.ToString("dd/MM hh:mm");

            string _out = $"[{timestamp} {Enum.GetName(typeof(IrcLogEntryType), logType)}]";

            switch (logType) {
                case IrcLogEntryType.System:
                    _out += $" {message}";

                    Console.WriteLine(_out);
                    break;
                case IrcLogEntryType.Warning:
                    _out += $" from `{memberName}' at line {lineNumber}: {message}";

                    Console.WriteLine(_out);
                    break;
                case IrcLogEntryType.Error:
                    _out += $" from `{memberName}' at line {lineNumber}";

                    Console.WriteLine(_out);

                    _out = $"\n{_out}\n{message}\n";

                    break;
                case IrcLogEntryType.Message:
                    _out += $" {message}";

                    Console.WriteLine(_out);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, "Use of undefined EventLogEntryType.");
            }

            if (!_out.EndsWith(Environment.NewLine))
                _out += Environment.NewLine;

            Backlog.Append(_out);
        }

        public void Log(object source, LogEntryEventArgs logEntryEventArgs) {
            Log(logEntryEventArgs.EntryType, logEntryEventArgs.Message, logEntryEventArgs.MemberName, logEntryEventArgs.LineNumber);
        }
    }

    [Serializable]
    public class LogEntryEventArgs : EventArgs {
        public LogEntryEventArgs(IrcLogEntryType entryType, string logMessage, [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0) {
            EntryType = entryType;
            Message = logMessage;
            MemberName = memberName;
            LineNumber = lineNumber;
        }

        public IrcLogEntryType EntryType { get; }
        public string Message { get; }
        public string MemberName { get; }
        public int LineNumber { get; }
    }

    public enum IrcLogEntryType {
        Error = 0,
        System,
        Message,
        Warning
    }
}