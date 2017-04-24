#region usings

using System;
using System.Collections.Generic;
using Convex.Types.References;

#endregion

namespace Convex.Types {
    public class Channel : MarshalByRefObject {
        public Channel(string name) {
            Name = name;
            Topic = string.Empty;
            Inhabitants = new List<string>();
            Modes = new List<IrcMode>();
        }

        public string Name { get; }
        public string Topic { get; set; }
        public List<string> Inhabitants { get; }
        public List<IrcMode> Modes { get; }

        public bool Connected { get; set; } = false;

        // this should likely not be used often
        public static implicit operator string(Channel channel) => channel.Name;
    }
}