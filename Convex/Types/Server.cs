#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convex.Net;
using Convex.Types.Events;
using Convex.Types.Messages;
using Convex.Types.References;
using Serilog;

#endregion

namespace Convex.Types {
    public class Server : IDisposable {
        public Connection Connection;

        public Server(string address, int port) {
            Channels = new List<Channel>();

            Connection = new Connection(address, port);
        }

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

            await OnChannelMessaged(new ChannelMessagedEventArgs(caller, new ChannelMessage(rawData)));
        }

        /// <summary>
        ///     Recieves input from open stream
        /// </summary>
        private async Task<string> ListenAsync() {
            string data = string.Empty;

            try {
                data = await Connection.ReadAsync();
            } catch (NullReferenceException) {
                Log.Warning("Stream disconnected. Attempting to reconnect...");

                await InitializeStream();
            } catch (Exception ex) {
                Log.Fatal(ex, "Exception occured while listening on stream");
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

        private readonly AsyncEvent<Func<ChannelMessagedEventArgs, Task>> _channelMessaged = new AsyncEvent<Func<ChannelMessagedEventArgs, Task>>();

        public event Func<ChannelMessagedEventArgs, Task> ChannelMessaged {
            add { _channelMessaged.Add(value); }
            remove { _channelMessaged.Remove(value); }
        }

        private async Task OnChannelMessaged(ChannelMessagedEventArgs e) {
            await _channelMessaged.InvokeAsync(e);
        }

        #endregion

        #region init

        public async Task Initialise() {
            Initialised = await InitializeStream();

            Initialised = true;
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

        public Channel GetChannel(string name) {
            return Channels.Single(channel => channel.Name.Equals(name));
        }

        /// <summary>
        ///     Check if message's channel origin should be added to channel list
        /// </summary>
        public void AddChannel(string name) {
            if (name.StartsWith("#") &&
                !Channels.Any(e => e.Name.Equals(name)))
                Channels.Add(new Channel(name));
        }

        public bool ChannelExists(string channelName) => Channels.Any(channel => channel.Name.Equals(channelName));
        public int RemoveChannel(string name) => Channels.RemoveAll(channel => channel.Name.Equals(name));

        #endregion
    }
}