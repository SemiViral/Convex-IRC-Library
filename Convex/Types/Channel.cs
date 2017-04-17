#region usings

using System;
using System.Collections.Generic;
using Convex.Types.References;

#endregion

namespace Convex.Types {
    public class Channel : MarshalByRefObject {
        public Channel(string name) {
            Name = name;
            Inhabitants = new List<string>();
            Modes = new List<IrcMode>();
        }

        public string Name { get; }
        public string Topic { get; set; }
        internal List<string> Inhabitants { get; }
        public List<IrcMode> Modes { get; }

        /// <summary>
        ///     Adds a user to a channel in list
        /// </summary>
        /// <param name="nickname">user to be added to list</param>
        public bool AddUser(string nickname) {
            if (Inhabitants.Contains(nickname))
                return false;

            Inhabitants.Add(nickname);
            return true;
        }

        /// <summary>
        ///     Remove a list of users from a channel's user list
        /// </summary>
        /// <param name="nicknames">list of users to remove from channel</param>
        /// <returns>number of users successfully removed</returns>
        public int RemoveUser(List<string> nicknames) {
            return Inhabitants.RemoveAll(nicknames.Contains);
        }

        /// <summary>
        ///     Remove a single user from a channel's user list
        /// </summary>
        /// <param name="nickname">user to remove</param>
        /// <returns>returns true if removal succeeded</returns>
        public bool RemoveUser(string nickname) {
            return Inhabitants.Remove(nickname);
        }

        // this should likely not be used often
        public static implicit operator string(Channel channel) => channel.Name;
    }
}