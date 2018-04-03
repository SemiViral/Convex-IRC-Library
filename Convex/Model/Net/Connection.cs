#region usings

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convex.ComponentModel.Event;

#endregion

namespace Convex.Model.Net {
    public sealed class Connection {
        private TcpClient _client;
        private NetworkStream _networkStream;

        private StreamReader _reader;
        private StreamWriter _writer;

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
            await SendDataAsync(message);
        }

        #region dispose

        public void Dispose() {
            _client?.Dispose();
            _networkStream?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();
        }

        #endregion

        public async Task ConnectAsync() {
            _client = new TcpClient();
            await _client.ConnectAsync(Address, Port);

            _networkStream = _client.GetStream();
            _reader = new StreamReader(_networkStream);
            _writer = new StreamWriter(_networkStream);
        }

        public async Task WriteAsync(string writable) {
            if (_writer.BaseStream == null)
                throw new NullReferenceException(nameof(_writer.BaseStream));

            await _writer.WriteLineAsync(writable);
            await _writer.FlushAsync();

            await OnFlushed(this, new BasicEventArgs(writable));
        }

        public async Task<string> ReadAsync() {
            if (_reader.BaseStream == null)
                throw new NullReferenceException(nameof(_reader.BaseStream));

            return await _reader.ReadLineAsync();
        }

        #region events

        public event AsyncEventHandler<BasicEventArgs> Flushed;

        private async Task OnFlushed(object source, BasicEventArgs e) {
            if (Flushed == null)
                return;

            await Flushed.Invoke(source, e);
        }

        #endregion
    }
}