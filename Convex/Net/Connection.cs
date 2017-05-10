#region usings

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convex.Types.Events;
using Convex.Types.Messages;

#endregion

namespace Convex.Net {
    public class Connection {
        private TcpClient client;
        private NetworkStream networkStream;

        public Connection(string address, int port) {
            Address = address;
            Port = port;
        }

        private StreamReader reader;
        private StreamWriter writer;
        public string Address { get; set; }
        public int Port { get; set; }

        public async Task SendDataAsync(params string[] args) {
            await WriteAsync(string.Join(" ", args));
        }

        public async Task SendDataAsync(SimpleMessage message) {
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

            await OnFlushed(new StreamFlushedEventArgs(writable));
        }

        public async Task<string> ReadAsync() {
            if (reader.BaseStream == null)
                throw new NullReferenceException(nameof(reader.BaseStream));

            return await reader.ReadLineAsync();
        }

        #region events

        public event Func<StreamFlushedEventArgs, Task> Flushed {
            add { flushedEvent.Add(value); }
            remove { flushedEvent.Remove(value); }
        }

        private readonly AsyncEvent<Func<StreamFlushedEventArgs, Task>> flushedEvent = new AsyncEvent<Func<StreamFlushedEventArgs, Task>>();

        protected virtual async Task OnFlushed(StreamFlushedEventArgs e) {
            await flushedEvent.InvokeAsync(e);
        }

        #endregion
    }

    [Serializable]
    public class StreamFlushedEventArgs : EventArgs {
        public StreamFlushedEventArgs(string contents) {
            Timestamp = DateTime.Now;
            Contents = contents;
        }

        public DateTime Timestamp { get; set; }
        public string Contents { get; set; }
    }
}