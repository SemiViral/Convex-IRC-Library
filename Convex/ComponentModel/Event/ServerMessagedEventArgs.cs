#region usings

using System;
using Convex.Model;

#endregion

namespace Convex.ComponentModel.Event {
    public class ServerMessagedEventArgs : EventArgs {
        public ServerMessagedEventArgs(Client bot, ServerMessage message) {
            Caller = bot;
            Message = message;
        }

        public Client Caller { get; }
        public ServerMessage Message { get; }
    }
}