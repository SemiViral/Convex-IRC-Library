#region usings

using System;
using Convex.Resource;

#endregion

namespace Convex.Event {
    public class ServerMessagedEventArgs : EventArgs {
        public ServerMessagedEventArgs(Client bot, ServerMessage message) {
            Caller = bot;
            Message = message;
        }

        public Client Caller { get; }
        public ServerMessage Message { get; }
    }
}