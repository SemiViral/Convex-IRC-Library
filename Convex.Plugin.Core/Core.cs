#region usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Calculator;
using Convex.Resource;
using Convex.Resource.Reference;
using Newtonsoft.Json.Linq;

#endregion

namespace Convex.Plugin {
    public class Core : IPlugin {
        private readonly InlineCalculator calculator = new InlineCalculator();
        private readonly Regex youtubeRegex = new Regex(@"(?i)http(?:s?)://(?:www\.)?youtu(?:be\.com/watch\?v=|\.be/)(?<ID>[\w\-]+)(&(amp;)?[\w\?=‌​]*)?", RegexOptions.Compiled);

        public string Name => "Core";
        public string Author => "SemiViral";

        public Version Version => new AssemblyName(GetType()
            .GetTypeInfo()
            .Assembly.FullName).Version;

        public string Id => Guid.NewGuid()
            .ToString();

        public PluginStatus Status { get; private set; } = PluginStatus.Stopped;

        public event AsyncEventHandler<ActionEventArgs> Callback;

        public async Task Start() {
            await DoCallback(this, new ActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(Commands.MOTD_REPLY_END, MotdReplyEnd)));

            await DoCallback(this, new ActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(Commands.PRIVMSG, Default)));

            await DoCallback(this, new ActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(Commands.PRIVMSG, YouTubeLinkResponse, e => youtubeRegex.IsMatch(e.Message.Args))));

            await DoCallback(this, new ActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(Commands.PRIVMSG, Quit, e => e.InputEquals("quit"), new KeyValuePair<string, string>("quit", "terminates bot execution"))));

            await DoCallback(this, new ActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(Commands.PRIVMSG, Eval, e => e.InputEquals("eval"), new KeyValuePair<string, string>("eval", "(<expression>) — evaluates given mathematical expression."))));

            await DoCallback(this, new ActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(Commands.PRIVMSG, Join, e => e.InputEquals("join"), new KeyValuePair<string, string>("join", "(<channel> *<message>) — joins specified channel."))));

            await DoCallback(this, new ActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(Commands.PRIVMSG, Part, e => e.InputEquals("part"), new KeyValuePair<string, string>("part", "(<channel> *<message>) — parts from specified channel."))));

            await DoCallback(this, new ActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(Commands.PRIVMSG, ListChannels, e => e.InputEquals("channels"), new KeyValuePair<string, string>("channels", "returns a list of connected channels."))));

            await DoCallback(this, new ActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(Commands.PRIVMSG, Define, e => e.InputEquals("define"), new KeyValuePair<string, string>("define", "(<word> *<part of speech>) — returns definition for given word."))));

            await DoCallback(this, new ActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(Commands.PRIVMSG, Lookup, e => e.InputEquals("lookup"), new KeyValuePair<string, string>("lookup", "(<term/phrase>) — returns the wikipedia summary of given term or phrase."))));

            await DoCallback(this, new ActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(Commands.PRIVMSG, ListUsers, e => e.InputEquals("users"), new KeyValuePair<string, string>("users", "returns a list of stored user realnames."))));

            await Log($"{Name} loaded.");
        }

        public async Task Stop() {
            if (Status.Equals(PluginStatus.Running) ||
                Status.Equals(PluginStatus.Processing)) {
                await Log($"Stop called but process is running from: {Name}");
            } else {
                await Log($"Stop called from: {Name}");
                await Call_Die();
            }
        }

        public async Task Call_Die() {
            Status = PluginStatus.Stopped;
            await Log($"Calling die, stopping process, sending unload —— from: {Name}");
        }

        private async Task Log(params string[] args) {
            await DoCallback(this, new ActionEventArgs(PluginActionType.Log, string.Join(" ", args)));
        }

        private async Task DoCallback(object source, ActionEventArgs e) {
            if (Callback == null)
                return;

            e.PluginName = Name;

            await Callback.Invoke(source, e);
        }

        private static async Task MotdReplyEnd(ServerMessagedEventArgs e) {
            if (e.Caller.Server.Identified)
                return;

            await e.Caller.Server.Connection.SendDataAsync(Commands.PRIVMSG, $"NICKSERV IDENTIFY {e.Caller.ClientConfiguration.Password}");
            await e.Caller.Server.Connection.SendDataAsync(Commands.MODE, $"{e.Caller.ClientConfiguration.Nickname} +B");

            foreach (Channel channel in e.Caller.Server.Channels.Where(channel => !channel.Connected && !channel.IsPrivate)) {
                await e.Caller.Server.Connection.SendDataAsync(Commands.JOIN, channel.Name);
                channel.Connected = true;
            }

            e.Caller.Server.Identified = true;
        }

        private static async Task Default(ServerMessagedEventArgs e) {
            if (e.Caller.IgnoreList.Contains(e.Message.Realname))
                return;

            if (!e.Message.SplitArgs[0].Replace(",", string.Empty)
                .Equals(e.Caller.ClientConfiguration.Nickname.ToLower()))
                return;

            if (e.Message.SplitArgs.Count < 2) { // typed only 'eve'
                await e.Caller.Server.Connection.SendDataAsync(Commands.PRIVMSG, $"{e.Message.Origin} Type 'eve help' to view my command list.");
                return;
            }

            // built-in 'help' command
            if (e.Message.SplitArgs[1].ToLower()
                .Equals("help")) {
                if (e.Message.SplitArgs.Count.Equals(2)) { // in this case, 'help' is the only text in the string.
                    Dictionary<string, string> commands = e.Caller.LoadedCommands;

                    await e.Caller.Server.Connection.SendDataAsync(Commands.PRIVMSG, commands.Count.Equals(0)
                        ? $"{e.Message.Origin} No commands currently active."
                        : $"{e.Message.Origin} Active commands: {string.Join(", ", e.Caller.LoadedCommands.Keys)}");
                    return;
                }

                KeyValuePair<string, string> queriedCommand = e.Caller.GetCommand(e.Message.SplitArgs[2]);

                string valueToSend = queriedCommand.Equals(default(KeyValuePair<string, string>))
                    ? "Command not found."
                    : $"{queriedCommand.Key}: {queriedCommand.Value}";

                await e.Caller.Server.Connection.SendDataAsync(Commands.PRIVMSG, $"{e.Message.Origin} {valueToSend}");

                return;
            }

            if (e.Caller.CommandExists(e.Message.SplitArgs[1].ToLower()))
                return;

            await e.Caller.Server.Connection.SendDataAsync(Commands.PRIVMSG, $"{e.Message.Origin} Invalid command. Type 'eve help' to view my command list.");
        }

        private async Task Quit(ServerMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("quit"))
                return;

            await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, new CommandEventArgs(Commands.PRIVMSG, e.Message.Origin, "Shutting down.")));
            await DoCallback(this, new ActionEventArgs(PluginActionType.SignalTerminate));
        }

        private async Task Eval(ServerMessagedEventArgs e) {
            Status = PluginStatus.Processing;

            CommandEventArgs message = new CommandEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Message.SplitArgs.Count < 3)
                message.Contents = "Not enough parameters.";

            Status = PluginStatus.Running;

            if (string.IsNullOrEmpty(message.Contents)) {
                Status = PluginStatus.Running;
                string evalArgs = e.Message.SplitArgs.Count > 3
                    ? e.Message.SplitArgs[2] + e.Message.SplitArgs[3]
                    : e.Message.SplitArgs[2];

                try {
                    message.Contents = calculator.Evaluate(evalArgs)
                        .ToString(CultureInfo.CurrentCulture);
                } catch (Exception ex) {
                    message.Contents = ex.Message;
                }
            }

            await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, message));

            Status = PluginStatus.Stopped;
        }

        private async Task Join(ServerMessagedEventArgs e) {
            Status = PluginStatus.Processing;

            string message = string.Empty;

            if (e.Caller.GetUser(e.Message.Realname)
                    ?.Access > 1)
                message = "Insufficient permissions.";
            else if (e.Message.SplitArgs.Count < 3)
                message = "Insufficient parameters. Type 'eve help join' to view command's help index.";
            else if (e.Message.SplitArgs.Count < 2 ||
                     !e.Message.SplitArgs[2].StartsWith("#"))
                message = "Channel name must start with '#'.";
            else if (e.Caller.Server.GetChannel(e.Message.SplitArgs[2].ToLower()) != null)
                message = "I'm already in that channel.";

            Status = PluginStatus.Running;

            if (string.IsNullOrEmpty(message)) {
                await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, new CommandEventArgs(Commands.JOIN, string.Empty, e.Message.SplitArgs[2])));
                e.Caller.Server.Channels.Add(new Channel(e.Message.SplitArgs[2].ToLower()));

                message = $"Successfully joined channel: {e.Message.SplitArgs[2]}.";
            }

            await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, new CommandEventArgs(Commands.PRIVMSG, e.Message.Origin, message)));

            Status = PluginStatus.Stopped;
        }

        private async Task Part(ServerMessagedEventArgs e) {
            Status = PluginStatus.Processing;

            CommandEventArgs message = new CommandEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Caller.GetUser(e.Message.Realname)
                    ?.Access > 1)
                message.Contents = "Insufficient permissions.";
            else if (e.Message.SplitArgs.Count < 3)
                message.Contents = "Insufficient parameters. Type 'eve help part' to view command's help index.";
            else if (e.Message.SplitArgs.Count < 2 ||
                     !e.Message.SplitArgs[2].StartsWith("#"))
                message.Contents = "Channel parameter must be a proper name (starts with '#').";
            else if (e.Message.SplitArgs.Count < 2 ||
                     e.Caller.Server.GetChannel(e.Message.SplitArgs[2]) != null)
                message.Contents = "I'm not in that channel.";

            Status = PluginStatus.Running;

            if (!string.IsNullOrEmpty(message.Contents)) {
                await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, message));
                return;
            }

            string channel = e.Message.SplitArgs[2].ToLower();

            e.Caller.Server.RemoveChannel(channel);

            message.Contents = $"Successfully parted channel: {channel}";

            await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, message));
            await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, new CommandEventArgs(Commands.PART, string.Empty, $"{channel} Channel part invoked by: {e.Message.Nickname}")));

            Status = PluginStatus.Stopped;
        }

        private async Task ListChannels(ServerMessagedEventArgs e) {
            Status = PluginStatus.Running;
            await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, new CommandEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Join(", ", e.Caller.Server.Channels.Select(channel => channel.Name)))));

            Status = PluginStatus.Stopped;
        }

        private async Task YouTubeLinkResponse(ServerMessagedEventArgs e) {
            Status = PluginStatus.Running;

            const int maxDescriptionLength = 100;

            string getResponse = await $"https://www.googleapis.com/youtube/v3/videos?part=snippet&id={youtubeRegex.Match(e.Message.Args) .Groups["ID"]}&key={e.Caller.GetApiKey("YouTube")}".HttpGet();

            JToken video = JObject.Parse(getResponse)["items"][0]["snippet"];
            string channel = (string)video["channelTitle"];
            string title = (string)video["title"];
            string description = video["description"].ToString()
                .Split('\n')[0];
            string[] descArray = description.Split(' ');

            if (description.Length > maxDescriptionLength) {
                description = string.Empty;

                for (int i = 0; description.Length < maxDescriptionLength; i++)
                    description += $" {descArray[i]}";

                if (!description.EndsWith(" "))
                    description.Remove(description.LastIndexOf(' '));

                description += "....";
            }

            await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, new CommandEventArgs(Commands.PRIVMSG, e.Message.Origin, $"{title} (by {channel}) — {description}")));

            Status = PluginStatus.Stopped;
        }

        private async Task Define(ServerMessagedEventArgs e) {
            Status = PluginStatus.Processing;

            CommandEventArgs message = new CommandEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Message.SplitArgs.Count < 3) {
                message.Contents = "Insufficient parameters. Type 'eve help define' to view correct usage.";
                await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, message));
                return;
            }

            Status = PluginStatus.Running;

            string partOfSpeech = e.Message.SplitArgs.Count > 3
                ? $"&part_of_speech={e.Message.SplitArgs[3]}"
                : string.Empty;

            JObject entry = JObject.Parse(await $"http://api.pearson.com/v2/dictionaries/laad3/entries?headword={e.Message.SplitArgs[2]}{partOfSpeech}&limit=1".HttpGet());

            if ((int)entry.SelectToken("count") < 1) {
                message.Contents = "Query returned no results.";
                await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, message));
                return;
            }

            Dictionary<string, string> _out = new Dictionary<string, string> {
                {"word", (string)entry["results"][0]["headword"]},
                {"pos", (string)entry["results"][0]["part_of_speech"]}
            };

            // this 'if' block seems messy and unoptimised.
            // I'll likely change it in the future.
            if (entry["results"][0]["senses"][0]["subsenses"] != null) {
                _out.Add("definition", (string)entry["results"][0]["senses"][0]["subsenses"][0]["definition"]);

                if (entry["results"][0]["senses"][0]["subsenses"][0]["examples"] != null)
                    _out.Add("example", (string)entry["results"][0]["senses"][0]["subsenses"][0]["examples"][0]["text"]);
            } else {
                _out.Add("definition", (string)entry["results"][0]["senses"][0]["definition"]);

                if (entry["results"][0]["senses"][0]["examples"] != null)
                    _out.Add("example", (string)entry["results"][0]["senses"][0]["examples"][0]["text"]);
            }

            string returnMessage = $"{_out["word"]} [{_out["pos"]}] — {_out["definition"]}";

            if (_out.ContainsKey("example"))
                returnMessage += $" (ex. {_out["example"]})";

            message.Contents = returnMessage;
            await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, message));

            Status = PluginStatus.Stopped;
        }

        private async Task Lookup(ServerMessagedEventArgs e) {
            Status = PluginStatus.Processing;

            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("lookup"))
                return;

            CommandEventArgs message = new CommandEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Message.SplitArgs.Count < 3) {
                message.Contents = "Insufficient parameters. Type 'eve help lookup' to view correct usage.";
                await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, message));
                return;
            }

            Status = PluginStatus.Running;

            string query = string.Join(" ", e.Message.SplitArgs.Skip(1));
            string response = await $"https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles={query}".HttpGet();

            JToken pages = JObject.Parse(response)["query"]["pages"].Values()
                .First();

            if (string.IsNullOrEmpty((string)pages["extract"])) {
                message.Contents = "Query failed to return results. Perhaps try a different term?";
                await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, message));
                return;
            }

            string fullReplyStr = $"\x02{(string)pages["title"]}\x0F — {Regex.Replace((string)pages["extract"], @"\n\n?|\n", " ")}";

            message.Target = e.Message.Nickname;

            foreach (string splitMessage in fullReplyStr.SplitByLength(400)) {
                message.Contents = splitMessage;
                await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, message));
            }

            Status = PluginStatus.Stopped;
        }

        private async Task ListUsers(ServerMessagedEventArgs e) {
            Status = PluginStatus.Running;

            await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, new CommandEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Join(", ", e.Caller.GetAllUsers()))));

            Status = PluginStatus.Stopped;
        }

        private async Task Set(ServerMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("set"))
                return;

            Status = PluginStatus.Processing;

            CommandEventArgs message = new CommandEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Message.SplitArgs.Count < 5) {
                message.Contents = "Insufficient parameters. Type 'eve help lookup' to view correct usage.";
                await DoCallback(this, new ActionEventArgs(PluginActionType.SendMessage, message));
                return;
            }

            if (e.Caller.GetUser(e.Message.Nickname)
                    ?.Access > 0)
                message.Contents = "Insufficient permissions.";

            //e.Root.GetUser()

            Status = PluginStatus.Stopped;
        }
    }
}