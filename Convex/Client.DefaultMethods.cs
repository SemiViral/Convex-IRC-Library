#region usings

using System.Linq;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Resource;

#endregion

namespace Convex {
    public sealed partial class Client {
        #region register methods

        private async Task Nick(ServerMessagedEventArgs e) {
            await OnQuery(this, new BasicEventArgs($"UPDATE users SET nickname='{e.Message.Origin}' WHERE realname='{e.Message.Realname}'"));
        }

        private Task Join(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)
                ?.Inhabitants.Add(e.Message.Nickname);

            return Task.CompletedTask;
        }

        private Task Part(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)
                ?.Inhabitants.RemoveAll(x => x.Equals(e.Message.Nickname));

            return Task.CompletedTask;
        }

        private Task ChannelTopic(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.SplitArgs[0])
                .Topic = e.Message.Args.Substring(e.Message.Args.IndexOf(' ') + 2);

            return Task.CompletedTask;
        }

        private Task NewTopic(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)
                .Topic = e.Message.Args;

            return Task.CompletedTask;
        }

        private Task NamesReply(ServerMessagedEventArgs e) {
            string channelName = e.Message.SplitArgs[1];

            // * SplitArgs [2] is always your nickname

            // in this case, Eve is the only one in the channel
            if (e.Message.SplitArgs.Count < 4)
                return Task.CompletedTask;

            foreach (string s in e.Message.SplitArgs[3].Split(' ')) {
                Channel currentChannel = Server.Channels.SingleOrDefault(channel => channel.Name.Equals(channelName));

                if (currentChannel == null ||
                    currentChannel.Inhabitants.Contains(s))
                    continue;

                Server?.Channels.Single(channel => channel.Name.Equals(channelName))
                    .Inhabitants.Add(s);
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}