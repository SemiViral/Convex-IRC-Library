#region usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Convex.ComponentModel;
using Convex.Resources;
using Convex.Resources.Plugin;
using Convex.Types;
using Convex.Types.Events;
using Convex.Types.References;
using Newtonsoft.Json;

#endregion

namespace Convex {
    public class Bot : MarshalByRefObject, IDisposable {
        private readonly Config config;
        private readonly Writer writer;
        private TcpClient connection;
        private bool disposed;
        private NetworkStream networkStream;
        private StreamReader streamWriter;

        /// <summary>
        ///     Initialises class
        /// </summary>
        public Bot(string configName) {
            Wrapper = new Wrapper();

            Config.CheckCreate();
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configName));

            // check if connection is established, don't execute if not
            if (!(Executing = InitializeNetworkStream()))
                return;

            writer = new Writer(networkStream);

            InitializeDatabase();
            InitailisePluginWrapper();

            channels = new List<Channel>();
            users = new ObservableCollection<User>(MainDatabase.GetAllUsers());
            users.CollectionChanged += UserAdded;

            writer.SendData(Commands.USER, $"{config.Nickname} 0 * {config.Realname}");
            writer.SendData(Commands.NICK, config.Nickname);

            RegisterMethods();

            Initialised = true;
            Executing = true;
        }

        public bool Executing { get; set; }

        ~Bot() {
            Dispose(false);
        }

        #region non-critical variables

        private Database MainDatabase { get; set; }
        private Wrapper Wrapper { get; }

        private readonly ObservableCollection<User> users;
        private readonly List<Channel> channels;

        private bool initialised;

        internal bool Initialised {
            get { return initialised; }
            set {
                initialised = value;
                OnInitiate(EventArgs.Empty);
            }
        }

        public List<string> IgnoreList { get; internal set; } = new List<string>();
        public List<string> Inhabitants => channels.SelectMany(e => e.Inhabitants).ToList();

        public string GetApiKey(string type) => config.ApiKeys[type];

        #endregion

        #region events

        public event EventHandler Terminated;
        public event EventHandler Initiated;

        private void OnTerminated(EventArgs e) {
            Terminated?.Invoke(this, e);
        }

        private void OnInitiate(EventArgs e) {
            Initiated?.Invoke(this, e);
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

            networkStream.Dispose();
            streamWriter.Dispose();
            writer.Dispose();
            config.Dispose();

            disposed = true;
            Executing = false;
        }

        private void SignalTerminate(object source, EventArgs e) {
            OnTerminated(e);

            Wrapper.Host.StopPlugins();
            Wrapper.Host.UnloadPluginDomain();

            Dispose();

            Log(IrcLogEntryType.System, "Bot has shutdown. Press any key to exit program.");
            Console.ReadKey();
        }

        #endregion

        #region initializations

        /// <summary>
        ///     Initialises all data streams
        /// </summary>
        private bool InitializeNetworkStream(int maxRetries = 3) {
            int retries = 0;

            while (retries <= maxRetries)
                try {
                    connection = new TcpClient(config.Server, config.Port);
                    networkStream = connection.GetStream();
                    streamWriter = new StreamReader(networkStream);
                    break;
                } catch (Exception) {
                    Console.WriteLine(retries <= maxRetries
                        ? "Communication error, attempting to connect again..."
                        : "Communication could not be established with address.\n");

                    retries++;
                }

            return retries <= maxRetries;
        }

        private void InitializeDatabase() {
            try {
                MainDatabase = new Database(config.DatabaseLocation);
                MainDatabase.LogEntryEventHandler += writer.LogEvent;
            } catch (Exception ex) {
                Log(IrcLogEntryType.Error, ex.ToString());

                Executing = false;
            }
        }

        private void InitailisePluginWrapper() {
            Wrapper.TerminateBotEvent += SignalTerminate;
            Wrapper.LogEntryEventHandler += writer.LogEvent;
            Wrapper.SimpleMessageEventHandler += writer.SendData;
            Wrapper.Start();
        }

        /// <summary>
        ///     Register all methods
        /// </summary>
        private void RegisterMethods() {
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.MOTD_REPLY_END, MotdReplyEnd));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.NICK, Nick));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.JOIN, Join));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.PART, Part));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.NAMES_REPLY, NamesReply));
            Wrapper.Host.RegisterMethod(new MethodRegistrar(Commands.PRIVMSG, Privmsg));
        }

        #endregion

        #region runtime

        /// <summary>
        ///     Recieves input from open stream
        /// </summary>
        private string ListenToStream() {
            string data = string.Empty;

            try {
                data = streamWriter.ReadLine();
            } catch (NullReferenceException) {
                Log(IrcLogEntryType.Error, "Stream disconnected. Attempting to reconnect...");

                InitializeNetworkStream();
            } catch (Exception ex) {
                Log(IrcLogEntryType.Error, ex.ToString());

                InitializeNetworkStream();
            }

            return data;
        }

        /// <summary>
        ///     Default method to execute bot functions
        /// </summary>
        public void ExecuteRuntime() {
            if (!Executing)
                return;

            string rawData = ListenToStream();

            if (string.IsNullOrEmpty(rawData) ||
                CheckIfIsPing(rawData))
                return;

            ChannelMessage channelMessage = new ChannelMessage(rawData);

            if (string.IsNullOrEmpty(channelMessage.Type) ||
                channelMessage.Realname.Equals(config.Realname) ||
                channelMessage.Type.Equals(Commands.ABORT) ||
                config.IgnoreList.Contains(channelMessage.Realname))
                return;

            if (channelMessage.Type.Equals(Commands.PRIVMSG)) {
                Log(IrcLogEntryType.Message, $"<{channelMessage.Origin} {channelMessage.Nickname}> {channelMessage.Args}");

                AddChannel(channelMessage.Origin);
                CheckAddUser(channelMessage);

                if (GetUser(channelMessage.Realname).GetTimeout())
                    return;
            } else {
                Log(IrcLogEntryType.Message, rawData);
            }

            try {
                Wrapper.Host.InvokeMethods(new ChannelMessagedEventArgs(this, channelMessage));
            } catch (Exception ex) {
                Log(IrcLogEntryType.Warning, ex.ToString());
            }
        }

        #endregion

        #region general methods

        private void Log(IrcLogEntryType entryType, string message) {
            writer.LogEvent(this, new LogEntry(entryType, message));
        }

        public void SendData(params string[] args) {
            writer.SendData(args);
        }

        public void RegisterMethod(MethodRegistrar registrar) {
            Wrapper.Host.RegisterMethod(registrar);
        }

        public int RemoveChannel(string name) => channels.RemoveAll(channel => channel.Name.Equals(name));

        /// <summary>
        ///     Returns a specified command from commands list
        /// </summary>
        /// <param name="command">Command to be returned</param>
        /// <returns></returns>
        public KeyValuePair<string, string> GetCommand(string command) {
            return Wrapper.Host.GetCommands().SingleOrDefault(x => x.Key.Equals(command));
        }

        /// <summary>
        ///     Checks whether specified comamnd exists
        /// </summary>
        /// <param name="command">comamnd name to be checked</param>
        /// <returns>True: exists; false: does not exist</returns>
        public bool HasCommand(string command) {
            return Wrapper.Host.GetCommands().Keys.Contains(command);
        }

        /// <summary>
        ///     Check whether the data recieved is a ping message and reply
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        private bool CheckIfIsPing(string rawData) {
            if (!rawData.StartsWith(Commands.PING))
                return false;

            writer.SendData(Commands.PONG, rawData.Remove(0, 5)); // removes 'PING ' from string
            return true;
        }

        #endregion

        #region channel methods

        /// <summary>
        ///     Check if message's channel origin should be added to channel list
        /// </summary>
        public void AddChannel(string name) {
            if (name.StartsWith("#") &&
                !channels.Any(e => e.Name.Equals(name)))
                channels.Add(new Channel(name));
        }

        public bool ChannelExists(string channelName) => channels.Any(channel => channel.Name.Equals(channelName));

        public List<string> GetAllChannels() => channels.Select(channel => channel.Name).ToList();

        #endregion

        #region user methods

        protected virtual void UserAdded(object source, NotifyCollectionChangedEventArgs e) {
            if (!e.Action.Equals(NotifyCollectionChangedAction.Add))
                return;

            foreach (object item in e.NewItems) {
                if (!(item is User))
                    continue;

                ((User)item).PropertyChanged += AutoUpdateUsers;
            }
        }

        private void AutoUpdateUsers(object source, PropertyChangedEventArgs e) {
            if (!(e is SpecialPropertyChangedEventArgs))
                return;

            SpecialPropertyChangedEventArgs castedArgs = (SpecialPropertyChangedEventArgs)e;

            MainDatabase.SimpleQuery($"UPDATE users SET {castedArgs.PropertyName}='{castedArgs.NewValue}' WHERE realname='{castedArgs.Name}'");
        }

        private void CheckAddUser(ChannelMessage channelMessage) {
            if (GetUser(channelMessage.Realname) != null)
                return;

            CreateUser(3, channelMessage.Nickname, channelMessage.Realname, channelMessage.Timestamp);
        }

        /// <summary>
        ///     Creates a new user and updates the users & userTimeouts collections
        /// </summary>
        /// <param name="access">access level of user</param>
        /// <param name="nickname">nickname of user</param>
        /// <param name="realname">realname of user</param>
        /// <param name="seen">last time user was seen</param>
        private void CreateUser(int access, string nickname, string realname, DateTime seen) {
            if (users.Any(e => e.Realname.Equals(realname)))
                return;

            Log(IrcLogEntryType.System, $"Creating database entry for {realname}.");

            int id = MainDatabase.GetLastDatabaseId() + 1;

            users.Add(new User(id, nickname, realname, access, seen));
            MainDatabase.SimpleQuery($"INSERT INTO users VALUES ({id}, '{nickname}', '{realname}', {access}, '{seen}')");
        }

        public List<string> GetAllUsernames() => users.Select(user => user.Realname).ToList();
        public User GetUser(string userName) => users.SingleOrDefault(user => user.Realname.Equals(userName));
        public bool UserExists(string userName) => users.Any(user => user.Realname.Equals(userName));

        /// <summary>
        ///     Adds a Args object to list
        /// </summary>
        /// <param name="user">user object</param>
        /// <param name="message"><see cref="Message" /> to be added</param>
        public void AddMessage(User user, Message message) {
            if (!users.Contains(user))
                return;

            MainDatabase.SimpleQuery($"INSERT INTO messages VALUES ({user.Id}, '{message.Sender}', '{message.Contents}', '{message.Date}')");
            user.Messages.Add(message);
        }

        #endregion

        #region register methods

        private void MotdReplyEnd(object source, ChannelMessagedEventArgs e) {
            if (config.Identified)
                return;

            writer.SendData(Commands.PRIVMSG, $"NICKSERV IDENTIFY {config.Password}");
            writer.SendData(Commands.MODE, $"{config.Nickname} +B");

            foreach (string channel in config.Channels) {
                writer.SendData(Commands.JOIN, channel);
                channels.Add(new Channel(channel));
            }

            config.Identified = true;
        }

        private void Nick(object source, ChannelMessagedEventArgs e) {
            MainDatabase.SimpleQuery($"UPDATE users SET nickname='{e.Message.Origin}' WHERE realname='{e.Message.Realname}'");
        }

        private void Join(object source, ChannelMessagedEventArgs e) {
            channels.SingleOrDefault(channel => !channel.Name.Equals(e.Message.Origin))?.AddUser(e.Message.Origin);
        }

        private void Part(object source, ChannelMessagedEventArgs e) {
            channels.SingleOrDefault(channel => channel.Name.Equals(e.Message.Origin))?.Inhabitants.RemoveAll(x => x.Equals(e.Message.Nickname));
        }

        private void NamesReply(object source, ChannelMessagedEventArgs e) {
            string channelName = e.Message.SplitArgs[1];

            // * SplitArgs [2] is always your nickname

            // in this case, Eve is the only one in the channel
            if (e.Message.SplitArgs.Count < 4)
                return;

            foreach (string s in e.Message.SplitArgs[3].Split(' ')) {
                Channel currentChannel = channels.SingleOrDefault(channel => channel.Name.Equals(channelName));

                if (currentChannel == null ||
                    currentChannel.Inhabitants.Contains(s))
                    continue;

                channels.Single(channel => channel.Name.Equals(channelName)).Inhabitants.Add(s);
            }
        }

        private void Privmsg(object source, ChannelMessagedEventArgs e) {
            if (IgnoreList.Contains(e.Message.Realname))
                return;

            if (!e.Message.SplitArgs[0].Replace(",", string.Empty).Equals(config.Nickname.ToLower()))
                return;

            if (e.Message.SplitArgs.Count < 2) { // typed only 'eve'
                writer.SendData(Commands.PRIVMSG, $"{e.Message.Origin} Type 'eve help' to view my command list.");
                return;
            }

            // built-in 'help' command
            if (e.Message.SplitArgs[1].ToLower().Equals("help")) {
                if (e.Message.SplitArgs.Count.Equals(2)) { // in this case, 'help' is the only text in the string.
                    Dictionary<string, string> commands = Wrapper.Host.GetCommands();

                    writer.SendData(Commands.PRIVMSG, commands.Count.Equals(0)
                        ? $"{e.Message.Origin} No commands currently active."
                        : $"{e.Message.Origin} Active commands: {string.Join(", ", Wrapper.Host.GetCommands().Keys)}");
                    return;
                }

                KeyValuePair<string, string> queriedCommand = GetCommand(e.Message.SplitArgs[2]);

                string valueToSend = queriedCommand.Equals(default(KeyValuePair<string, string>))
                    ? "Command not found."
                    : $"{queriedCommand.Key}: {queriedCommand.Value}";

                writer.SendData(Commands.PRIVMSG, $"{e.Message.Origin} {valueToSend}");

                return;
            }

            if (HasCommand(e.Message.SplitArgs[1].ToLower()))
                return;
            writer.SendData(Commands.PRIVMSG, $"{e.Message.Origin} Invalid command. Type 'eve help' to view my command list.");
        }

        #endregion
    }
}