#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Convex.ComponentModel.Event;
using Convex.ComponentModel.Reference;
using Convex.Model;
using Convex.Model.Config;
using Convex.Model.Net;
using Convex.Plugin;
using Convex.Plugin.Registrar;
using Newtonsoft.Json;

#endregion

namespace Convex {
    public sealed partial class Client : IDisposable {
        private readonly string _friendlyName;
        private readonly PluginWrapper _wrapper;

        private bool _disposed;

        /// <summary>
        ///     Initialises class. No connections are made at init of class, so call `Initialise()` to begin sending and
        ///     recieiving.
        /// </summary>
        public Client(string address, int port, string friendlyName = "") {
            if (!Directory.Exists(Configuration.DefaultResourceDirectory))
                Directory.CreateDirectory(Configuration.DefaultResourceDirectory);

            Configuration.CheckCreateConfig(Configuration.DefaultFilePath);
            ClientConfiguration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Configuration.DefaultFilePath));

            _wrapper = new PluginWrapper();
            MainDatabase = new Database(ClientConfiguration.DatabaseFilePath);

            Connection conn = new Connection(address, port);
            Server = new Server(conn);

            _friendlyName = friendlyName;
        }

        public Configuration ClientConfiguration { get; }

        /// <summary>
        ///     Returns a specified command from commands list
        /// </summary>
        /// <param name="command">Command to be returned</param>
        /// <returns></returns>
        public Tuple<string, string> GetCommand(string command) {
            return _wrapper.Host.DescriptionRegistry.Values.SingleOrDefault(x => x != null && x.Item1.Equals(command, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        ///     Checks whether specified comamnd exists
        /// </summary>
        /// <param name="command">comamnd name to be checked</param>
        /// <returns>True: exists; false: does not exist</returns>
        public bool CommandExists(string command) {
            return !GetCommand(command).Equals(default(Tuple<string, string>));
        }

        #region RUNTIME

        private async Task Listen(object source, ServerMessagedEventArgs e) {
            if (string.IsNullOrEmpty(e.Message.Command))
                return;

            if (e.Message.Command.Equals(Commands.PRIVMSG)) {
                if (e.Message.Origin.StartsWith("#") && !Server.Channels.Any(channel => channel.Name.Equals(e.Message.Origin)))
                    Server.Channels.Add(new Channel(e.Message.Origin));

                if (GetUser(e.Message.Realname)?.GetTimeout() ?? false)
                    return;
            } else if (e.Message.Command.Equals(Commands.ERROR)) {
                Server.Executing = false;
                return;
            }

            if (e.Message.Nickname.Equals(ClientConfiguration.Nickname) || ClientConfiguration.IgnoreList.Contains(e.Message.Realname))
                return;

            if (e.Message.SplitArgs.Count >= 2 && e.Message.SplitArgs[0].Equals(ClientConfiguration.Nickname.ToLower()))
                e.Message.InputCommand = e.Message.SplitArgs[1].ToLower();

            try {
                await _wrapper.Host.InvokeAsync(e);
            } catch (Exception ex) {
                await OnFailure(this, new BasicEventArgs(ex.ToString()));
            }
        }

        #endregion

        #region MEMBERS

        private Database MainDatabase { get; }

        public Server Server { get; }

        public Dictionary<string, Tuple<string, string>> LoadedCommands => _wrapper.Host.DescriptionRegistry;

        public List<string> IgnoreList => ClientConfiguration.IgnoreList;

        public string GetApiKey(string type) {
            return ClientConfiguration.ApiKeys[type];
        }

        public Version Version => new AssemblyName(GetType().GetTypeInfo().Assembly.FullName).Version;

        public string FriendlyName => string.IsNullOrWhiteSpace(_friendlyName) ? Server.Connection.Address : _friendlyName;

        #endregion

        #region EVENTS

        public event AsyncEventHandler Queried;
        public event AsyncEventHandler Terminated;
        public event AsyncEventHandler Initialized;

        public event AsyncEventHandler<BasicEventArgs> Failure;
        public event AsyncEventHandler<BasicEventArgs> Log;

        private async Task OnQuery(object source, EventArgs e) {
            if (Queried == null)
                return;

            await Queried.Invoke(source, e);
        }

        private async Task OnTerminated(object source, EventArgs e) {
            if (Terminated == null)
                return;

            await Terminated?.Invoke(source, e);
        }

        private async Task OnInitialized(object source, EventArgs e) {
            if (Initialized == null)
                return;

            await Initialized.Invoke(source, e);
        }

        private async Task OnFailure(object source, BasicEventArgs e) {
            if (Failure == null)
                return;

            await Failure.Invoke(this, e);
        }

        private async Task OnLog(object source, BasicEventArgs e) {
            if (Log == null)
                return;

            await Log.Invoke(this, e);
        }

        #endregion

        #region DISPOSE

        /// <summary>
        ///     Dispose of all streams and objects
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }

        private void Dispose(bool dispose) {
            if (!dispose || _disposed)
                return;

            Server?.Dispose();
            ClientConfiguration?.Dispose();

            _disposed = true;
        }

        private async Task SignalTerminate(object source, EventArgs e) {
            await OnTerminated(source, e);

            _wrapper.Host.StopPlugins();

            Dispose();
        }

        #endregion

        #region INIT

        public async Task<bool> Initialise() {
            await MainDatabase.Initialise();

            _wrapper.Initialise();
            _wrapper.Log += OnLog;
            _wrapper.Terminated += SignalTerminate;
            _wrapper.CommandRecieved += Server.Connection.SendDataAsync;
            RegisterMethods();

            await Server.Initialise();
            Server.ChannelMessaged += Listen;
            await Server.SendConnectionInfo(ClientConfiguration.Nickname, ClientConfiguration.Realname);

            await OnInitialized(this, EventArgs.Empty);

            return Server.Initialised;
        }

        /// <summary>
        ///     Register all methods
        /// </summary>
        private void RegisterMethods() {
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Nick, null, Commands.NICK, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Join, null, Commands.JOIN, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Part, null, Commands.PART, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(ChannelTopic, null, Commands.CHANNEL_TOPIC, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(NewTopic, null, Commands.TOPIC, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(NamesReply, null, Commands.NAMES_REPLY, null));
        }

        public void RegisterMethod(IAsyncRegistrar<ServerMessagedEventArgs> methodRegistrar) {
            _wrapper.Host.RegisterMethod(methodRegistrar);
        }

        #endregion

        #region METHODS

        public IEnumerable<User> GetAllUsers() {
            return MainDatabase.Users;
        }

        /// <summary>
        ///     Gets the user entry by their realname
        ///     note: will return null if user does not exist
        /// </summary>
        /// <param name="realname">realname of user</param>
        public User GetUser(string realname) {
            return MainDatabase.Users.SingleOrDefault(user => user.Realname.Equals(realname));
        }

        public bool UserExists(string userName) {
            return MainDatabase.Users.Any(user => user.Realname.Equals(userName));
        }

        #endregion

        #region REGISTRABLE METHODS

        private async Task Nick(ServerMessagedEventArgs e) {
            await OnQuery(this, new BasicEventArgs($"UPDATE users SET nickname='{e.Message.Origin}' WHERE realname='{e.Message.Realname}'"));
        }

        private Task Join(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)?.Inhabitants.Add(e.Message.Nickname);

            return Task.CompletedTask;
        }

        private Task Part(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)?.Inhabitants.RemoveAll(x => x.Equals(e.Message.Nickname));

            return Task.CompletedTask;
        }

        private Task ChannelTopic(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.SplitArgs[0]).Topic = e.Message.Args.Substring(e.Message.Args.IndexOf(' ') + 2);

            return Task.CompletedTask;
        }

        private Task NewTopic(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin).Topic = e.Message.Args;

            return Task.CompletedTask;
        }

        private Task NamesReply(ServerMessagedEventArgs e) {
            string channelName = e.Message.SplitArgs[1];

            // * SplitArgs [2] is always your nickname

            // in this case, Eve is the only one in the channel
            if (e.Message.SplitArgs.Count < 4)
                return Task.CompletedTask;

            foreach (string s in e.Message.SplitArgs[3].Split(' ')) {
                Channel currentChannel = Server.Channels.SingleOrDefault(channel => channel.Name.Equals(channelName));

                if (currentChannel == null || currentChannel.Inhabitants.Contains(s))
                    continue;

                Server?.Channels.Single(channel => channel.Name.Equals(channelName)).Inhabitants.Add(s);
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
