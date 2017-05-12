#region usings

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convex.Event;

#endregion

namespace Convex.Net {
    public sealed class Connection {
        private TcpClient client;
        private NetworkStream networkStream;

        private StreamReader reader;
        private StreamWriter writer;

        public Connection(string address, int port) {
            Address = address;
            Port = port;
        }

        public string Address { get; }
        public int Port { get; }

        public async Task SendDataAsync(params string[] args) {
            await WriteAsync(string.Join(" ", args));
        }

        public async Task SendDataAsync(object source, CommandEventArgs message) {
            await SendDataAsync(message.Command, message.Target, message.Args);
        }

        #region dispose

        public void Dispose() {
            client?.Dispose();
            networkStream?.Dispose();
            reader?.Dispose();
            writer?.Dispose();
        }

        #endregion

        public async Task ConnectAsync() {
            client = new TcpClient();
            await client.ConnectAsync(Address, Port);

            networkStream = client.GetStream();
            reader = new StreamReader(networkStream);
            writer = new StreamWriter(networkStream);
        }

        public async Task WriteAsync(string writable) {
            if (writer.BaseStream == null)
                throw new NullReferenceException(nameof(writer.BaseStream));

            await writer.WriteLineAsync(writable);
            await writer.FlushAsync();

            await OnFlushed(this, new StreamFlushedEventArgs(writable));
        }

        public async Task<string> ReadAsync() {
            if (reader.BaseStream == null)
                throw new NullReferenceException(nameof(reader.BaseStream));

            return await reader.ReadLineAsync();
        }

        #region events

        public event AsyncEventHandler<StreamFlushedEventArgs> Flushed;

        private async Task OnFlushed(object source, StreamFlushedEventArgs e) {
            if (Flushed == null)
                return;

            await Flushed.Invoke(source, e);
        }

        #endregion
    }

    public class StreamFlushedEventArgs : EventArgs {
        public StreamFlushedEventArgs(string contents) {
            Timestamp = DateTime.Now;
            Contents = contents;
        }

        public DateTime Timestamp { get; set; }
        public string Contents { get; set; }
    }
}