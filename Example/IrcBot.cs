#region usings

using System;
using System.Collections.Generic;
using Convex;
using Convex.Resources.Plugin;
using Convex.Types.Events;
using Convex.Types.References;

#endregion

namespace Example
{
    public class IrcBot : MarshalByRefObject, IDisposable
    {
        private readonly Bot bot;

        /// <summary>
        ///     Initialises class
        /// </summary>
        public IrcBot()
        {
            bot = new Bot("config.json");
            bot.Terminated += OnBotTerminated;

            RegisterMethods();

            CanExecute = true;
        }

        internal bool CanExecute {
            get { return bot.Executing; }
            private set { bot.Executing = value; }
        }

        public static string BotInfo => "Evealyn is an IRC bot created by SemiViral as a primary learning project for C#. Version 4.1.2";

        private void OnBotTerminated(object sender, EventArgs e)
        {
            CanExecute = false;
        }

        /// <summary>
        ///     Register all methods
        /// </summary>
        private void RegisterMethods()
        {
            bot.RegisterMethod(new MethodRegistrar(Commands.PRIVMSG, Info, new KeyValuePair<string, string>("info", "returns the basic information about this bot")));
        }

        public void Run()
        {
            bot.ExecuteRuntime();
        }

        #region register methods

        private void Info(object sender, ChannelMessagedEventArgs e) {
            if (!e.Message.SplitArgs[1].Equals("info"))
                return;

            bot.SendData(Commands.PRIVMSG, $"{e.Message.Origin} {BotInfo}");
        }

        #endregion

        #region dispose

        private void Dispose(bool disposing)
        {
            if (disposing)
                bot?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~IrcBot()
        {
            Dispose(false);
        }

        #endregion
    }
}