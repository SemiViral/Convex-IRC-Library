#region usings

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Convex;
using Convex.Resources.Plugin;
using Convex.Types;
using Convex.Types.References;

#endregion

namespace Example {
    public class IrcBot : IDisposable {
        private readonly Client bot;

        /// <summary>
        ///     Initialises class
        /// </summary>
        public IrcBot() {
            bot = new Client();
        }

        public string BotInfo => $"[Version {bot.Version}] Evealyn is an IRC bot created by SemiViral as a primary learning project for C#.";

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
            bot.Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.PRIVMSG, Info, new KeyValuePair<string, string>("info", "returns the basic information about this bot")));
        }

        private async Task Info(ChannelMessagedEventArgs e) {
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