#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Net;
using Convex.Resource.Reference;

#endregion

namespace Convex.Resource {
    public class Server : IDisposable {
        public Server(Connection connection) {
            Channels = new List<Channel>();

            Connection = connection;
        }

        public Connection Connection { get; }

        public bool Identified { get; set; }
        public bool Initialised { get; private set; }
        public bool Executing { get; internal set; }

        public List<Channel> Channels { get; }

        public List<string> Inhabitants => Channels.SelectMany(e => e.Inhabitants)
            .ToList();

        #region dispose

        public void Dispose() {
            Connection?.Dispose();

            Identified = false;
            Initialised = false;
            Executing = false;
        }

        #endregion

        #region runtime

        public async Task QueueAsync(Client caller) {
            await WorkItem(caller);
        }

        private async Task WorkItem(Client caller) {
            Executing = true;

            string rawData = await ListenAsync();

            if (string.IsNullOrEmpty(rawData) ||
                await CheckPing(rawData))
                return;

            await OnChannelMessaged(this, new ServerMessagedEventArgs(caller, new ServerMessage(rawData)));
        }

        /// <summary>
        ///     Recieves input from open stream
        /// </summary>
        private async Task<string> ListenAsync() {
            string data = string.Empty;

            try {
                data = await Connection.ReadAsync();
            } catch (NullReferenceException) {
                Debug.WriteLine("Stream disconnected. Attempting to reconnect...");

                await InitializeStream();
            } catch (Exception ex) {
                Debug.WriteLine(ex, "Exception occured while listening on stream");
            }

            return data;
        }

        /// <summary>
        ///     Check whether the data recieved is a ping message and reply
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        private async Task<bool> CheckPing(string rawData) {
            if (!rawData.StartsWith(Commands.PING))
                return false;

            await Connection.SendDataAsync(Commands.PONG, rawData.Remove(0, 5)); // removes 'PING ' from string
            return true;
        }

        #endregion

        #region events

        public event AsyncEventHandler<ServerMessagedEventArgs> ChannelMessaged;

        private async Task OnChannelMessaged(object source, ServerMessagedEventArgs e) {
            if (ChannelMessaged == null)
                return;

            await ChannelMessaged.Invoke(source, e);
        }

        #endregion

        #region init

        public async Task Initialise() {
            Channels.Add(new Channel(Connection.Address));
            ChannelMessaged += SortMessage;

            Initialised = await InitializeStream();
        }

        /// <summary>
        ///     Initialises all stream connection
        /// </summary>
        private async Task<bool> InitializeStream(int maxRetries = 3) {
            int retries = 0;

            while (retries <= maxRetries)
                try {
                    await Connection.ConnectAsync();
                    break;
                } catch (Exception) {
                    Console.WriteLine(retries <= maxRetries
                        ? "Communication error, attempting to connect again..."
                        : "Communication could not be established with address.\n");

                    retries++;
                }

            return retries <= maxRetries;
        }

        /// <summary>
        ///     sends client info to the server
        /// </summary>
        public async Task SendConnectionInfo(string nickname, string realname) {
            await Connection.SendDataAsync(Commands.USER, nickname, "0 *", realname);
            await Connection.SendDataAsync(Commands.NICK, nickname);
        }

        #endregion

        #region channel methods

        private Task SortMessage(object source, ServerMessagedEventArgs e) {
            if (GetChannel(e.Message.Origin) == null)
                GetChannel(Connection.Address)
                    .Messages.Add(e.Message);
            else
                GetChannel(e.Message.Origin)
                    .Messages.Add(e.Message);

            return Task.CompletedTask;
        }

        public Channel GetChannel(string name) {
            return Channels.SingleOrDefault(channel => channel.Name.Equals(name));
        }

        public int RemoveChannel(string name) => Channels.RemoveAll(channel => channel.Name.Equals(name));

        #endregion
    }
}