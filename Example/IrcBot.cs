#region usings

using System;
using System.Collections.Generic;
using Convex;
using Convex.Resources.Plugin;
using Convex.Types;
using Convex.Types.References;

#endregion

namespace Example {
    public class IrcBot : MarshalByRefObject, IDisposable {
        private readonly Bot bot;

        /// <summary>
        ///     Initialises class
        /// </summary>
        public IrcBot() {
            bot = new Bot("config.json");
            bot.Initialise();
            RegisterMethods();
        }

        public string BotInfo => $"[Version {bot.Version}] Evealyn is an IRC bot created by SemiViral as a primary learning project for C#.";

        public bool CanExecute => bot.Server.Execute;

        #region register methods

        /// <summary>
        ///     Register all methods
        /// </summary>
        private void RegisterMethods() {
            bot.Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.PRIVMSG, Info, new KeyValuePair<string, string>("info", "returns the basic information about this bot")));
        }

        private void Info(object sender, ChannelMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("info"))
                return;

            bot.Server.Connection.SendData(Commands.PRIVMSG, $"{e.Message.Origin} {BotInfo}");
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