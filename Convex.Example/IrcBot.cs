#region usings

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin;
using Convex.Resource;
using Convex.Resource.Reference;

#endregion

namespace Convex.Example {
    public class IrcBot : IDisposable {
        private readonly Client bot;

        /// <summary>
        ///     Initialises class
        /// </summary>
        public IrcBot() {
            bot = new Client("irc.foonetic.net", 6667);
            bot.Server.Channels.Add(new Channel("#testgrounds"));
        }

        private string BotInfo => $"[Version {bot.Version}] Evealyn is an IRC bot created by SemiViral as a primary learning project for C#.";

        public bool Executing => bot.Server.Executing;

        public async Task Initialise() {
            await bot.Initialise();
            RegisterMethods();
        }

        public async Task Execute() {
            await bot.Server.QueueAsync(bot);
        }

        #region register methods

        /// <summary>
        ///     Register all methods
        /// </summary>
        private void RegisterMethods() {
            bot.RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Commands.PRIVMSG, Info, e => e.Message.InputCommand.Equals("info"), new KeyValuePair<string, string>("info", "returns the basic information about this bot")));
        }

        private async Task Info(ServerMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("info"))
                return;

            await bot.Server.Connection.SendDataAsync(Commands.PRIVMSG, $"{e.Message.Origin} {BotInfo}");
        }

        #endregion

        #region dispose

        private void Dispose(bool disposing) {
            if (disposing)
                bot?.Dispose();
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
}