#region usings

using System.Collections.Generic;
using Convex.Resource.Reference;

#endregion

namespace Convex.Resource {
    public class Channel {
        public Channel(string name) {
            Name = name;
            Topic = string.Empty;
            Inhabitants = new List<string>();
            Modes = new List<IrcMode>();
            Messages = new List<ServerMessage>();
            IsPrivate = !Name.StartsWith("#");
        }

        public string Name { get; }
        public string Topic { get; set; }
        public List<string> Inhabitants { get; }
        public List<IrcMode> Modes { get; }
        public List<ServerMessage> Messages { get; set; }
        public bool IsPrivate { get; }
        public bool Connected { get; set; }
    }
}