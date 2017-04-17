using System;

namespace Convex.Types.Events
{   
    [Serializable]
    public class ChannelMessagedEventArgs : EventArgs {
        public Bot Root { get; }
        public ChannelMessage Message { get; }

        public ChannelMessagedEventArgs(Bot rootBot, ChannelMessage message) {
            Root = rootBot;
            Message = message;
        }
    }
}
