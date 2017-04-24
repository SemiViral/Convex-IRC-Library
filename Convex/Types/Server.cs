#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Convex.Net;
using Convex.Types.References;

#endregion

namespace Convex.Types {
    public class Server : MarshalByRefObject, IDisposable {
        public Connection Connection;

        public Server(string address, int port) {
            Channels = new List<Channel>();

            Connection = new Connection(address, port);
        }

        public bool Identified { get; set; }
        public bool Initialised { get; private set; }
        public bool Execute { get; internal set; }

        public List<Channel> Channels { get; }

        public List<string> Inhabitants => Channels.SelectMany(e => e.Inhabitants).ToList();

        #region dispose

        public void Dispose() {
            Connection?.Dispose();

            Identified = false;
            Initialised = false;
            Execute = false;
        }

        #endregion

        #region runtime

        public void Queue(Bot caller) {
            WorkItem(caller);
        }

        private void WorkItem(Bot caller) {
            string rawData = ListenToStream();

            if (string.IsNullOrEmpty(rawData) ||
                CheckPing(rawData))
                return;

            Debug.WriteLine(rawData);
            OnChannelMessage(new ChannelMessagedEventArgs(caller, new ChannelMessage(rawData)));
        }

        /// <summary>
        ///     Recieves input from open stream
        /// </summary>
        private string ListenToStream() {
            string data = string.Empty;

            try {
                data = Connection.Read();
            } catch (NullReferenceException) {
                OnLog(new LogEntryEventArgs(IrcLogEntryType.Error, "Stream disconnected. Attempting to reconnect..."));

                InitializeStream();
            } catch (Exception ex) {
                OnLog(new LogEntryEventArgs(IrcLogEntryType.Error, ex.ToString()));
            }

            return data;
        }

        /// <summary>
        ///     Check whether the data recieved is a ping message and reply
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        private bool CheckPing(string rawData) {
            if (!rawData.StartsWith(Commands.PING))
                return false;

            Connection.SendData(Commands.PONG, rawData.Remove(0, 5)); // removes 'PING ' from string
            return true;
        }

        #endregion

        #region events 

        public event EventHandler<LogEntryEventArgs> LogEntryEvent;
        public event EventHandler<ChannelMessagedEventArgs> ChannelMessagedEvent;

        private void OnLog(LogEntryEventArgs e) {
            LogEntryEvent?.Invoke(this, e);
        }

        private void OnChannelMessage(ChannelMessagedEventArgs e) {
            ChannelMessagedEvent?.Invoke(this, e);
        }

        #endregion

        #region init

        public void Initialise(string nickname, string realname) {
            if (!(Initialised = InitializeStream()))
                return;

            SendConnectionInfo(nickname, realname);

            Execute = true;
        }

        /// <summary>
        ///     Initialises all stream connection
        /// </summary>
        private bool InitializeStream(int maxRetries = 3) {
            int retries = 0;

            while (retries <= maxRetries)
                try {
                    Connection.Connect();
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
        private void SendConnectionInfo(string nickname, string realname) {
            Connection.SendData(Commands.USER, nickname, "0 *", realname);
            Connection.SendData(Commands.NICK, nickname);
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
        public List<string> GetAllChannels() => Channels.Select(channel => channel.Name).ToList();

        #endregion
    }
}