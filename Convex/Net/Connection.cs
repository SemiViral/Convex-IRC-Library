#region usings

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convex.Resources.Plugin;
using Convex.Types.Events;

#endregion

namespace Convex.Net {
    public class Connection {
        private TcpClient client;
        private NetworkStream networkStream;
        protected StreamReader Reader { get; set; }
        protected StreamWriter Writer { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        public Connection(string address, int port) {
            Address = address;
            Port = port;
        }

        public async Task SendDataAsync(params string[] args) {
            await WriteAsync(string.Join(" ", args));
        }

        public async Task SendDataAsync(SimpleMessageEventArgs messageEventArgs) {
            await SendDataAsync(messageEventArgs.Command, messageEventArgs.Target, messageEventArgs.Message);
        }

        #region dispose

        public void Dispose() {
            client?.Dispose();
            networkStream?.Dispose();
            Reader?.Dispose();
            Writer?.Dispose();
        }

        #endregion

        public async Task ConnectAsync() {
            client = new TcpClient();
            await client.ConnectAsync(Address, Port);

            networkStream = client.GetStream();
            Reader = new StreamReader(networkStream);
            Writer = new StreamWriter(networkStream);
        }

        public async Task WriteAsync(string writable) {
            if (Writer.BaseStream == null)
                throw new NullReferenceException(nameof(Writer.BaseStream));

            await Writer.WriteLineAsync(writable);
            await Writer.FlushAsync();

            await OnFlushed(new StreamFlushedEventArgs(writable));
        }

        public async Task<string> ReadAsync() {
            if (Reader.BaseStream == null)
                throw new NullReferenceException(nameof(Reader.BaseStream));

            return await Reader.ReadLineAsync();
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