#region usings

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

#endregion

namespace Convex.Net {
    public class Stream : MarshalByRefObject, IDisposable {
        private TcpClient client;
        private NetworkStream networkStream;

        public Stream(string address, bool append = false) {
            Address = address;

            Writer = new StreamWriter(address, append);
        }

        public Stream(string address, int port) {
            Address = address;
            Port = port;
        }

        protected StreamReader Reader { get; set; }
        protected StreamWriter Writer { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        #region dispose

        public void Dispose() {
            client?.Dispose();
            networkStream?.Dispose();
            Reader?.Dispose();
            Writer?.Dispose();
        }

        #endregion

        public void Connect() {
            client = new TcpClient(Address, Port);
            networkStream = client.GetStream();
            Reader = new StreamReader(networkStream);
            Writer = new StreamWriter(networkStream);
        }

        public async Task WriteAsync(string writable) {
            await Writer.WriteLineAsync(writable);
            await Writer.FlushAsync();

            OnFlushed(new StreamFlushedEventArgs(writable));
        }

        public void Write(string writable) {
            Writer.WriteLine(writable);
            Writer.Flush();

            OnFlushed(new StreamFlushedEventArgs(writable));
        }

        public string Read() {
            return Reader.ReadLine();
        }

        public async Task<string> ReadAsync() {
            return await Reader.ReadLineAsync();
        }

        #region events

        public event EventHandler<StreamFlushedEventArgs> FlushedEvent;

        protected virtual void OnFlushed(StreamFlushedEventArgs e) {
            EventHandler<StreamFlushedEventArgs> handler = FlushedEvent;
            handler?.Invoke(this, e);
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