#region usings

using System.Threading.Tasks;
using Convex.Resources.Plugin;

#endregion

namespace Convex.Net {
    public class Connection : Stream {
        /// <summary>
        ///     Initiailises the Writer object with an output stream
        /// </summary>
        /// <message name="stream">object to get stream from</message>
        public Connection(string address, int port) : base(address, port) {}

        public void SendData(params string[] args) {
            if (Writer.BaseStream.Equals(null))
                return;

            Write(string.Join(" ", args));
        }

        public void SendData(SimpleMessageEventArgs messageEventArgs) {
            SendData(messageEventArgs.Command, messageEventArgs.Target, messageEventArgs.Message);
        }

        public async Task SendDataAsync(params string[] args) {
            if (Writer.BaseStream.Equals(null))
                return;

            await WriteAsync(string.Join(" ", args));
        }

        public async Task SendDataAsync(SimpleMessageEventArgs messageEventArgs) {
            await SendDataAsync(messageEventArgs.Command, messageEventArgs.Target, messageEventArgs.Message);
        }
    }
}