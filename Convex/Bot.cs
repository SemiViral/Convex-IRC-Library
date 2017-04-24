#region usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Convex.ComponentModel;
using Convex.Net;
using Convex.Resources;
using Convex.Resources.Plugin;
using Convex.Types;
using Convex.Types.References;
using Newtonsoft.Json;

#endregion

namespace Convex {
    public class Bot : MarshalByRefObject, IDisposable {
        private readonly Config config;
        private readonly Logger logger;

        private bool disposed;

        /// <summary>
        ///     Initialises class
        /// </summary>
        public Bot(string configName) {
            Wrapper = new Wrapper();
            users = new ObservableCollection<User>();
            users.CollectionChanged += UserAdded;

            Config.CheckCreate();
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configName));

            logger = new Logger(config.LogAddress);
            LogEventHandler += logger.Log;
        }

        ~Bot() {
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
            return Wrapper.Host.GetCommands().Keys.Contains(command);
        }

        #region variabless

        private Database MainDatabase { get; set; }
        public Wrapper Wrapper { get; }

        private readonly ObservableCollection<User> users;

        private bool initialised;

        internal bool Initialised {
            get { return initialised; }
            set {
                initialised = value;
                OnInitiate(EventArgs.Empty);
            }
        }

        public Server Server => config.Server;

        public List<string> IgnoreList { get; internal set; } = new List<string>();

        public string GetApiKey(string type) => config.ApiKeys[type];

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        #endregion

        #region events

        public event EventHandler<QueryEventArgs> QueryEventHandler;
        public event EventHandler<LogEntryEventArgs> LogEventHandler;
        public event EventHandler TerminatedEventHandler;
        public event EventHandler InitiatedEventHandler;

        private void OnQuery(QueryEventArgs e) {
            QueryEventHandler?.Invoke(this, e);
        }

        private void OnLog(LogEntryEventArgs e) {
            LogEventHandler?.Invoke(this, e);
        }

        private void OnTerminated(EventArgs e) {
            TerminatedEventHandler?.Invoke(this, e);
        }

        private void OnInitiate(EventArgs e) {
            InitiatedEventHandler?.Invoke(this, e);
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

        private void SignalTerminate(object source, EventArgs e) {
            OnTerminated(e);

            Wrapper.Host.StopPlugins();
            Wrapper.Host.UnloadPluginDomain();

            Dispose();

            OnLog(new LogEntryEventArgs(IrcLogEntryType.System, "Bot has shutdown. Press any key to exit program."));
            Console.ReadKey();
        }

        #endregion

        #region init

        public bool Initialise() {
            InitializeDatabase();
            InitailisePluginWrapper();
            InitialiseServer();

            return Server.Initialised;
        }

        private void InitialiseServer() {
            Server.Connection.FlushedEvent += LogDataSent;
            Server.ChannelMessagedEvent += ListenEvent;
            Server.Initialise(config.Nickname, config.Realname);

            Thread serverThread = new Thread(() => {
                while (Server.Execute)
                    Server.Queue(this);
            });
            serverThread.Start();
        }

        private void InitializeDatabase() {
            try {
                MainDatabase = new Database(config.DatabaseAddress);
                MainDatabase.LogEntryEventHandler += logger.Log;

                foreach (User user in MainDatabase.GetAllUsers())
                    users.Add(user);

                OnLog(new LogEntryEventArgs(IrcLogEntryType.System, "Loaded database."));
            } catch (Exception ex) {
                OnLog(new LogEntryEventArgs(IrcLogEntryType.Error, ex.ToString()));

                Server.Execute = false;
            }
        }

        private void InitailisePluginWrapper() {
            Wrapper.TerminateBotEvent += SignalTerminate;
            Wrapper.LogEntryEventHandler += logger.Log;
            Wrapper.SimpleMessageEventHandler += (source, e) => Server.Connection.SendData(e);
            Wrapper.Start();

            RegisterMethods();
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

        private void ListenEvent(object source, ChannelMessagedEventArgs e) {
            if (string.IsNullOrEmpty(e.Message.Type))
                return;

            if (e.Message.Type.Equals(Commands.PRIVMSG)) {
                OnLog(new LogEntryEventArgs(IrcLogEntryType.Message, $"<{e.Message.Origin} {e.Message.Nickname}> {e.Message.Args}"));

                // todo AddChannel(channelMessage.Origin);

                if (GetUser(e.Message.Realname).GetTimeout())
                    return;
            } else if (e.Message.Type.Equals(Commands.ERROR)) {
                OnLog(new LogEntryEventArgs(IrcLogEntryType.Message, e.Message.RawMessage));
                Server.Execute = false;
                return;
            } else {
                OnLog(new LogEntryEventArgs(IrcLogEntryType.Message, e.Message.RawMessage));
            }

            if (e.Message.Nickname.Equals(config.Nickname) ||
                config.IgnoreList.Contains(e.Message.Realname))
                return;

            try {
                Wrapper.Host.InvokeMethods(e);
            } catch (Exception ex) {
                OnLog(new LogEntryEventArgs(IrcLogEntryType.Warning, ex.ToString()));
            }
        }

        private void LogDataSent(object source, StreamFlushedEventArgs e) {
            OnLog(new LogEntryEventArgs(IrcLogEntryType.Message, $" >> {e.Contents}"));
        }

        #endregion

        #region user updates

        protected virtual void UserAdded(object source, NotifyCollectionChangedEventArgs e) {
            if (!e.Action.Equals(NotifyCollectionChangedAction.Add))
                return;

            foreach (object item in e.NewItems) {
                if (!(item is User))
                    continue;

                if (!MainDatabase.UserExists(((User)item).Realname))
                    MainDatabase.CreateUser((User)item);

                ((User)item).PropertyChanged += AutoUpdateUsers;
                ((User)item).Messages.CollectionChanged += MessageAdded;
            }
        }

        protected virtual void MessageAdded(object source, NotifyCollectionChangedEventArgs e) {
            if (!e.Action.Equals(NotifyCollectionChangedAction.Add))
                return;

            foreach (object item in e.NewItems) {
                if (!(item is Message))
                    continue;

                Message message = (Message)item;

                OnQuery(new QueryEventArgs($"INSERT INTO messages VALUES ({message.Id}, '{message.Sender}', '{message.Contents}', '{message.Date}')"));
            }
        }

        private void AutoUpdateUsers(object source, PropertyChangedEventArgs e) {
            if (!(e is SpecialPropertyChangedEventArgs))
                return;

            SpecialPropertyChangedEventArgs castedArgs = (SpecialPropertyChangedEventArgs)e;

            OnQuery(new QueryEventArgs($"UPDATE users SET {castedArgs.PropertyName}='{castedArgs.NewValue}' WHERE realname='{castedArgs.Name}'"));
        }

        public List<string> GetAllUsernames() => users.Select(user => user.Realname).ToList();
        public User GetUser(string userName) => users.SingleOrDefault(user => user.Realname.Equals(userName));
        public bool UserExists(string userName) => users.Any(user => user.Realname.Equals(userName));

        #endregion

        #region register methods

        private void MotdReplyEnd(object source, ChannelMessagedEventArgs e) {
            if (Server.Identified)
                return;

            Server?.Connection.SendData(Commands.PRIVMSG, $"NICKSERV IDENTIFY {config.Password}");
            Server?.Connection.SendData(Commands.MODE, $"{config.Nickname} +B");

            foreach (Channel channel in Server.Channels.Where(channel => !channel.Connected)) {
                Server.Connection.SendData(Commands.JOIN, channel.Name);
                channel.Connected = true;
            }

            Server.Identified = true;
        }

        private void Nick(object source, ChannelMessagedEventArgs e) {
            OnQuery(new QueryEventArgs($"UPDATE users SET nickname='{e.Message.Origin}' WHERE realname='{e.Message.Realname}'"));
        }

        private void Join(object source, ChannelMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)?.Inhabitants.Add(e.Message.Nickname);
        }

        private void Part(object source, ChannelMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)?.Inhabitants.RemoveAll(x => x.Equals(e.Message.Nickname));
        }

        private void ChannelTopic(object source, ChannelMessagedEventArgs e) {
            Server.GetChannel(e.Message.SplitArgs[0]).Topic = e.Message.Args.Substring(e.Message.Args.IndexOf(' ') + 2);
        }

        private void NewTopic(object source, ChannelMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin).Topic = e.Message.Args;
        }

        private void NamesReply(object source, ChannelMessagedEventArgs e) {
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

                Server?.Channels.Single(channel => channel.Name.Equals(channelName)).Inhabitants.Add(s);
            }
        }

        private void Privmsg(object source, ChannelMessagedEventArgs e) {
            if (IgnoreList.Contains(e.Message.Realname))
                return;

            if (!e.Message.SplitArgs[0].Replace(",", string.Empty).Equals(config.Nickname.ToLower()))
                return;

            if (e.Message.SplitArgs.Count < 2) { // typed only 'eve'
                Server?.Connection.SendData(Commands.PRIVMSG, $"{e.Message.Origin} Type 'eve help' to view my command list.");
                return;
            }

            // built-in 'help' command
            if (e.Message.SplitArgs[1].ToLower().Equals("help")) {
                if (e.Message.SplitArgs.Count.Equals(2)) { // in this case, 'help' is the only text in the string.
                    Dictionary<string, string> commands = Wrapper.Host.GetCommands();

                    Server?.Connection.SendData(Commands.PRIVMSG, commands.Count.Equals(0)
                        ? $"{e.Message.Origin} No commands currently active."
                        : $"{e.Message.Origin} Active commands: {string.Join(", ", Wrapper.Host.GetCommands().Keys)}");
                    return;
                }

                KeyValuePair<string, string> queriedCommand = GetCommand(e.Message.SplitArgs[2]);

                string valueToSend = queriedCommand.Equals(default(KeyValuePair<string, string>))
                    ? "Command not found."
                    : $"{queriedCommand.Key}: {queriedCommand.Value}";

                Server?.Connection.SendData(Commands.PRIVMSG, $"{e.Message.Origin} {valueToSend}");

                return;
            }

            if (CommandExists(e.Message.SplitArgs[1].ToLower()))
                return;
            Server?.Connection.SendData(Commands.PRIVMSG, $"{e.Message.Origin} Invalid command. Type 'eve help' to view my command list.");
        }

        #endregion
    }
}