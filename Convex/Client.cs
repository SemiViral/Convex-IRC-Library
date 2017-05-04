﻿#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Convex.Net;
using Convex.Resources;
using Convex.Resources.Plugin;
using Convex.Types;
using Convex.Types.Events;
using Convex.Types.References;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

#endregion

namespace Convex {
    public class Client : IDisposable {
        private readonly Config config;
        private readonly Logger logger;

        private bool disposed;

        /// <summary>
        ///     Initialises class
        /// </summary>
        public Client() {
            if (!Directory.Exists(Config.DefaultResourceDirectory))
                Directory.CreateDirectory(Config.DefaultResourceDirectory);

            Config.CheckCreateConfig(Config.DefaultConfigFilePath);
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Config.DefaultConfigFilePath));

            Wrapper = new PluginWrapper();
            MainDatabase = new Database(config.DatabaseFilePath);
            logger = new LoggerConfiguration().WriteTo.LiterateConsole()
                .WriteTo.Async(sinkConfig => sinkConfig.File(config.LogFilePath))
                .CreateLogger();
            Log.Logger = logger;
        }

        ~Client() {
            Dispose(false);
        }

        /// <summary>
        ///     Returns a specified command from commands list
        /// </summary>
        /// <param name="command">Command to be returned</param>
        /// <returns></returns>
        public KeyValuePair<string, string> GetCommand(string command) {
            return Wrapper.Host.Commands.SingleOrDefault(x => x.Key.Equals(command));
        }

        /// <summary>
        ///     Checks whether specified comamnd exists
        /// </summary>
        /// <param name="command">comamnd name to be checked</param>
        /// <returns>True: exists; false: does not exist</returns>
        public bool CommandExists(string command) {
            return Wrapper.Host.Commands.Keys.Contains(command);
        }

        #region variables

        private Database MainDatabase { get; }
        public PluginWrapper Wrapper { get; }

        internal bool Initialised { get; set; }

        public Server Server => config.Server;

        public List<string> IgnoreList { get; internal set; } = new List<string>();

        public string GetApiKey(string type) => config.ApiKeys[type];

        public string Version => "4.6.1"; //Assembly.GetExecutingAssembly().GetName().Version;

        #endregion

        #region events

        public event Func<QueryEventArgs, Task> Queried {
            add { queryEvent.Add(value); }
            remove { queryEvent.Remove(value); }
        }

        public event Func<EventArgs, Task> Terminated {
            add { terminatedEvent.Add(value); }
            remove { terminatedEvent.Remove(value); }
        }

        public event Func<EventArgs, Task> Initiated {
            add { initiatedEvent.Add(value); }
            remove { initiatedEvent.Remove(value); }
        }

        private readonly AsyncEvent<Func<QueryEventArgs, Task>> queryEvent = new AsyncEvent<Func<QueryEventArgs, Task>>();
        private readonly AsyncEvent<Func<EventArgs, Task>> terminatedEvent = new AsyncEvent<Func<EventArgs, Task>>();
        private readonly AsyncEvent<Func<EventArgs, Task>> initiatedEvent = new AsyncEvent<Func<EventArgs, Task>>();

        private async Task OnQuery(QueryEventArgs e) {
            await queryEvent.InvokeAsync(e);
        }

        private async Task OnTerminated(EventArgs e) {
            await terminatedEvent.InvokeAsync(e);
        }

        private async Task OnInitiate(EventArgs e) {
            await initiatedEvent.InvokeAsync(e);
        }

        #endregion

        #region disposing

        /// <summary>
        ///     Dispose of all streams and objects
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose) {
            if (!dispose || disposed)
                return;

            // dispose config after server to ensure
            // serialized values are default
            logger?.Dispose();
            Server?.Dispose();
            config?.Dispose();

            disposed = true;
        }

        private async Task SignalTerminate(EventArgs e) {
            await OnTerminated(e);

            Wrapper.Host.StopPlugins();

            Dispose();
        }

        #endregion

        #region init

        public async Task<bool> Initialise() {
            await MainDatabase.Initialise();
            logger.Information("Loaded database.");

            Wrapper.Initialise();
            Wrapper.Terminated += SignalTerminate;
            Wrapper.SimpleMessaged += Server.Connection.SendDataAsync;
            RegisterMethods();

            await Server.Initialise();
            Server.Connection.Flushed += LogDataSent;
            Server.ChannelMessaged += Listen;
            await Server.SendConnectionInfo(config.Nickname, config.Realname);

            Initialised = true;
            await OnInitiate(EventArgs.Empty);

            return Initialised && Server.Initialised;
        }

        /// <summary>
        ///     Register all methods
        /// </summary>
        public virtual void RegisterMethods() {
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.MOTD_REPLY_END, MotdReplyEnd));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.NICK, Nick));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.JOIN, Join));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.PART, Part));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.CHANNEL_TOPIC, ChannelTopic));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.TOPIC, NewTopic));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.NAMES_REPLY, NamesReply));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.PRIVMSG, Privmsg));
        }

        #endregion

        #region runtime

        private async Task Listen(ChannelMessagedEventArgs e) {
            if (string.IsNullOrEmpty(e.Message.Type))
                return;

            if (e.Message.Type.Equals(Commands.PRIVMSG)) {
                logger.Information($"<{e.Message.Origin} {e.Message.Nickname}> {e.Message.Args}");

                if (e.Message.Origin.StartsWith("#") &&
                    !Server.ChannelExists(e.Message.Origin))
                    Server.Channels.Add(new Channel(e.Message.Origin));

                if (GetUser(e.Message.Realname)
                    .GetTimeout())
                    return;
            } else if (e.Message.Type.Equals(Commands.ERROR)) {
                logger.Information(e.Message.RawMessage);
                Server.Executing = false;
                return;
            } else {
                logger.Information(e.Message.RawMessage);
            }

            if (e.Message.Nickname.Equals(config.Nickname) ||
                config.IgnoreList.Contains(e.Message.Realname))
                return;

            try {
                await Wrapper.Host.InvokeAsync(e);
            } catch (Exception ex) {
                logger.Warning(ex.ToString());
            }
        }

        private async Task LogDataSent(StreamFlushedEventArgs e) {
            logger.Information($" >> {e.Contents}");
        }

        #endregion

        #region user updates

        public List<User> GetAllUsers() => MainDatabase.Users.ToList();

        public User GetUser(string userName) => MainDatabase.Users.SingleOrDefault(user => user.Realname.Equals(userName));
        public bool UserExists(string userName) => MainDatabase.Users.Any(user => user.Realname.Equals(userName));

        #endregion

        #region register methods

        private async Task MotdReplyEnd(ChannelMessagedEventArgs e) {
            if (Server.Identified)
                return;

            await Server.Connection.SendDataAsync(Commands.PRIVMSG, $"NICKSERV IDENTIFY {config.Password}");
            await Server.Connection.SendDataAsync(Commands.MODE, $"{config.Nickname} +B");

            foreach (Channel channel in Server.Channels.Where(channel => !channel.Connected)) {
                await Server.Connection.SendDataAsync(Commands.JOIN, channel.Name);
                channel.Connected = true;
            }

            Server.Identified = true;
        }

        private async Task Nick(ChannelMessagedEventArgs e) {
            await OnQuery(new QueryEventArgs($"UPDATE users SET nickname='{e.Message.Origin}' WHERE realname='{e.Message.Realname}'"));
        }

        private async Task Join(ChannelMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)
                ?.Inhabitants.Add(e.Message.Nickname);
        }

        private async Task Part(ChannelMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)
                ?.Inhabitants.RemoveAll(x => x.Equals(e.Message.Nickname));
        }

        private async Task ChannelTopic(ChannelMessagedEventArgs e) {
            Server.GetChannel(e.Message.SplitArgs[0])
                .Topic = e.Message.Args.Substring(e.Message.Args.IndexOf(' ') + 2);
        }

        private async Task NewTopic(ChannelMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)
                .Topic = e.Message.Args;
        }

        private async Task NamesReply(ChannelMessagedEventArgs e) {
            string channelName = e.Message.SplitArgs[1];

            // * SplitArgs [2] is always your nickname

            // in this case, Eve is the only one in the channel
            if (e.Message.SplitArgs.Count < 4)
                return;

            foreach (string s in e.Message.SplitArgs[3].Split(' ')) {
                Channel currentChannel = Server.Channels.SingleOrDefault(channel => channel.Name.Equals(channelName));

                if (currentChannel == null ||
                    currentChannel.Inhabitants.Contains(s))
                    continue;

                Server?.Channels.Single(channel => channel.Name.Equals(channelName))
                    .Inhabitants.Add(s);
            }
        }

        private async Task Privmsg(ChannelMessagedEventArgs e) {
            if (IgnoreList.Contains(e.Message.Realname))
                return;

            if (!e.Message.SplitArgs[0].Replace(",", string.Empty)
                .Equals(config.Nickname.ToLower()))
                return;

            if (e.Message.SplitArgs.Count < 2) { // typed only 'eve'
                await Server.Connection.SendDataAsync(Commands.PRIVMSG, $"{e.Message.Origin} Type 'eve help' to view my command list.");
                return;
            }

            // built-in 'help' command
            if (e.Message.SplitArgs[1].ToLower()
                .Equals("help")) {
                if (e.Message.SplitArgs.Count.Equals(2)) { // in this case, 'help' is the only text in the string.
                    Dictionary<string, string> commands = Wrapper.Host.Commands;

                    await Server.Connection.SendDataAsync(Commands.PRIVMSG, commands.Count.Equals(0)
                        ? $"{e.Message.Origin} No commands currently active."
                        : $"{e.Message.Origin} Active commands: {string.Join(", ", Wrapper.Host.Commands.Keys)}");
                    return;
                }

                KeyValuePair<string, string> queriedCommand = GetCommand(e.Message.SplitArgs[2]);

                string valueToSend = queriedCommand.Equals(default(KeyValuePair<string, string>))
                    ? "Command not found."
                    : $"{queriedCommand.Key}: {queriedCommand.Value}";

                await Server.Connection.SendDataAsync(Commands.PRIVMSG, $"{e.Message.Origin} {valueToSend}");

                return;
            }

            if (CommandExists(e.Message.SplitArgs[1].ToLower()))
                return;

            await Server.Connection.SendDataAsync(Commands.PRIVMSG, $"{e.Message.Origin} Invalid command. Type 'eve help' to view my command list.");
        }

        #endregion
    }
}