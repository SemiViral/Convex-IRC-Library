#region usings

using System;
using System.Threading.Tasks;
using Convex.ComponentModel.Event;
using Convex.ComponentModel.Reference;
using Convex.Model;
using Convex.Plugin.Registrar;
using Serilog;
using Serilog.Events;

#endregion

namespace Convex.Example {
    public class IrcBot : IDisposable {
        private readonly Client _bot;

        /// <summary>
        ///     Initialises class
        /// </summary>
        public IrcBot() {
            _bot = new Client("irc.foonetic.net", 6667);
            _bot.Server.Channels.Add(new Channel("#testgrounds"));

            Log.Logger = new LoggerConfiguration().WriteTo.RollingFile(_bot.ClientConfiguration.LogFilePath).WriteTo.LiterateConsole().CreateLogger();
        }

        private string BotInfo => $"[Version {_bot.Version}] Evealyn is an IRC bot created by SemiViral as a primary learning project for C#.";

        public bool Executing => _bot.Server.Executing;

        public async Task Initialise() {
            await _bot.Initialise();
            RegisterMethods();

            _bot.Initialized += (source, e) => OnLog(source, new LogEventArgs(LogEventLevel.Information, "Client initialized."));
            _bot.Log += (source, e) => OnLog(source, new LogEventArgs(LogEventLevel.Information, e.Contents));
            _bot.Server.Connection.Flushed += (source, e) => OnLog(source, new LogEventArgs(LogEventLevel.Information, $" >> {e.Contents}"));
            _bot.Server.ChannelMessaged += LogChannelMessage;
        }

        private static Task OnLog(object source, LogEventArgs e) {
            switch (e.Level) {
                case LogEventLevel.Verbose:
                    Log.Verbose(e.Contents);
                    break;
                case LogEventLevel.Debug:
                    Log.Debug(e.Contents);
                    break;
                case LogEventLevel.Information:
                    Log.Information(e.Contents);
                    break;
                case LogEventLevel.Warning:
                    Log.Warning(e.Contents);
                    break;
                case LogEventLevel.Error:
                    Log.Error(e.Contents);
                    break;
                case LogEventLevel.Fatal:
                    Log.Fatal(e.Contents);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }

        private Task LogChannelMessage(object source, ServerMessagedEventArgs e) {
            if (e.Message.Command.Equals(Commands.PRIVMSG))
                OnLog(this, new LogEventArgs(LogEventLevel.Information, $"<{e.Message.Origin} {e.Message.Nickname}> {e.Message.Args}"));
            else if (e.Message.Command.Equals(Commands.ERROR))
                OnLog(this, new LogEventArgs(LogEventLevel.Error, e.Message.RawMessage));
            else
                OnLog(this, new LogEventArgs(LogEventLevel.Information, e.Message.RawMessage));

            return Task.CompletedTask;
        }

        public async Task Execute() {
            await _bot.Server.QueueAsync(_bot);
        }

        #region register methods

        /// <summary>
        ///     Register all methods
        /// </summary>
        private void RegisterMethods() {
            _bot.RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Info, e => e.Message.InputCommand.Equals(nameof(Info).ToLower()), Commands.PRIVMSG, new Tuple<string, string>(nameof(Info), "returns the basic information about this bot")));
        }

        private async Task Info(ServerMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 || !e.Message.SplitArgs[1].Equals("info"))
                return;

            await _bot.Server.Connection.SendDataAsync(Commands.PRIVMSG, $"{e.Message.Origin} {BotInfo}");
        }

        #endregion

        #region dispose

        private void Dispose(bool disposing) {
            if (disposing)
                _bot?.Dispose();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~IrcBot() {
            Dispose(false);
        }

        #endregion
    }

    internal class LogEventArgs : BasicEventArgs {
        public LogEventArgs(LogEventLevel logLevel, string contents) : base(contents) {
            Level = logLevel;
            Contents = contents;
        }

        public LogEventLevel Level { get; }
    }
}
