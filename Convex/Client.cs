#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Net;
using Convex.Plugin;
using Convex.Resource;
using Convex.Resource.Reference;
using Newtonsoft.Json;

#endregion

namespace Convex {
    public sealed partial class Client : IDisposable {
        private readonly string friendlyName;
        private readonly PluginWrapper wrapper;

        private bool disposed;

        /// <summary>
        ///     Initialises class. No connections are made at init of class, so call `Initialise()` to begin sending and
        ///     recieiving.
        /// </summary>
        public Client(string address, int port, string friendlyName = "") {
            if (!Directory.Exists(ClientConfiguration.DefaultResourceDirectory))
                Directory.CreateDirectory(ClientConfiguration.DefaultResourceDirectory);

            ClientConfiguration.CheckCreateConfig(ClientConfiguration.DefaultFilePath);
            ClientConfiguration = JsonConvert.DeserializeObject<ClientConfiguration>(File.ReadAllText(ClientConfiguration.DefaultFilePath));

            wrapper = new PluginWrapper();
            MainDatabase = new Database(ClientConfiguration.DatabaseFilePath);

            Connection conn = new Connection(address, port);
            Server = new Server(conn);

            this.friendlyName = friendlyName;
        }

        public ClientConfiguration ClientConfiguration { get; }

        /// <summary>
        ///     Returns a specified command from commands list
        /// </summary>
        /// <param name="command">Command to be returned</param>
        /// <returns></returns>
        public KeyValuePair<string, string> GetCommand(string command) {
            return wrapper.Host.Commands.SingleOrDefault(x => x.Key.Equals(command));
        }

        /// <summary>
        ///     Checks whether specified comamnd exists
        /// </summary>
        /// <param name="command">comamnd name to be checked</param>
        /// <returns>True: exists; false: does not exist</returns>
        public bool CommandExists(string command) {
            return wrapper.Host.Commands.Keys.Contains(command);
        }

        #region runtime

        private async Task Listen(object source, ServerMessagedEventArgs e) {
            if (string.IsNullOrEmpty(e.Message.Command))
                return;

            if (e.Message.Command.Equals(Commands.PRIVMSG)) {
                if (e.Message.Origin.StartsWith("#") &&
                    !Server.Channels.Any(channel => channel.Name.Equals(e.Message.Origin)))
                    Server.Channels.Add(new Channel(e.Message.Origin));

                if (GetUser(e.Message.Realname)
                        ?.GetTimeout() ?? false)
                    return;
            } else if (e.Message.Command.Equals(Commands.ERROR)) {
                Server.Executing = false;
                return;
            }

            if (e.Message.Nickname.Equals(ClientConfiguration.Nickname) ||
                ClientConfiguration.IgnoreList.Contains(e.Message.Realname))
                return;

            if (e.Message.SplitArgs.Count >= 2 &&
                e.Message.SplitArgs[0].Equals(ClientConfiguration.Nickname.ToLower()))
                e.Message.InputCommand = e.Message.SplitArgs[1].ToLower();

            try {
                await wrapper.Host.InvokeAsync(e);
            } catch (Exception ex) {
                await OnFailure(this, new BasicEventArgs(ex.ToString()));
            }
        }

        #endregion

        #region variables

        private Database MainDatabase { get; }

        public Server Server { get; }

        public Dictionary<string, string> LoadedCommands => wrapper.Host.Commands;

        public List<string> IgnoreList => ClientConfiguration.IgnoreList;

        public string GetApiKey(string type) => ClientConfiguration.ApiKeys[type];

        public Version Version => new AssemblyName(GetType()
            .GetTypeInfo()
            .Assembly.FullName).Version;

        public string FriendlyName => string.IsNullOrWhiteSpace(friendlyName)
            ? Server.Connection.Address
            : friendlyName;

        #endregion

        #region events

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

        #region disposing

        /// <summary>
        ///     Dispose of all streams and objects
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }

        private void Dispose(bool dispose) {
            if (!dispose || disposed)
                return;

            Server?.Dispose();
            ClientConfiguration?.Dispose();

            disposed = true;
        }

        private async Task SignalTerminate(object source, EventArgs e) {
            await OnTerminated(source, e);

            wrapper.Host.StopPlugins();

            Dispose();
        }

        #endregion

        #region init

        public async Task<bool> Initialise() {
            await MainDatabase.Initialise();

            wrapper.Initialise();
            wrapper.Log += OnLog;
            wrapper.Terminated += SignalTerminate;
            wrapper.CommandRecieved += Server.Connection.SendDataAsync;
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
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Commands.NICK, Nick));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Commands.JOIN, Join));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Commands.PART, Part));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Commands.CHANNEL_TOPIC, ChannelTopic));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Commands.TOPIC, NewTopic));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Commands.NAMES_REPLY, NamesReply));
        }

        public void RegisterMethod(MethodRegistrar<ServerMessagedEventArgs> methodRegistrar) {
            wrapper.Host.RegisterMethod(methodRegistrar);
        }

        #endregion

        #region general methods

        public IEnumerable<User> GetAllUsers() => MainDatabase.Users;

        /// <summary>
        ///     Gets the user entry by their realname
        ///     note: will return null if user does not exist
        /// </summary>
        /// <param name="realname">realname of user</param>
        public User GetUser(string realname) => MainDatabase.Users.SingleOrDefault(user => user.Realname.Equals(realname));

        public bool UserExists(string userName) => MainDatabase.Users.Any(user => user.Realname.Equals(userName));

        #endregion
    }
}