#region usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Convex;
using Convex.Net;
using Convex.Resources.Plugin;
using Convex.Types;
using Convex.Types.References;
using Core.Calculator;
using Newtonsoft.Json.Linq;

#endregion

namespace Core {
    public class Core : MarshalByRefObject, IPlugin {
        private readonly InlineCalculator calculator = new InlineCalculator();
        private readonly Regex youtubeRegex = new Regex(@"(?i)http(?:s?)://(?:www\.)?youtu(?:be\.com/watch\?v=|\.be/)(?<ID>[\w\-]+)(&(amp;)?[\w\?=‌​]*)?", RegexOptions.Compiled);

        public string Name => "Core";
        public string Author => "SemiViral";
        public string Version => "3.1.3";
        public string Id => Guid.NewGuid().ToString();

        public PluginStatus Status { get; private set; } = PluginStatus.Stopped;

        public void Start() {
            calculator.LogEntryEventHandler += Log;

            DoCallback(PluginActionType.RegisterMethod, new MethodRegistrar(Commands.PRIVMSG, Quit, new KeyValuePair<string, string>("quit", "terminates bot execution")));

            DoCallback(PluginActionType.RegisterMethod, new MethodRegistrar(Commands.PRIVMSG, Reload, new KeyValuePair<string, string>("reload", "reloads the plugin domain. bot execution")));

            DoCallback(PluginActionType.RegisterMethod, new MethodRegistrar(Commands.PRIVMSG, Eval, new KeyValuePair<string, string>("eval", "(<expression>) — evaluates given mathematical expression.")));

            DoCallback(PluginActionType.RegisterMethod, new MethodRegistrar(Commands.PRIVMSG, Join, new KeyValuePair<string, string>("join", "(<channel> *<message>) — joins specified channel.")));

            DoCallback(PluginActionType.RegisterMethod, new MethodRegistrar(Commands.PRIVMSG, Part, new KeyValuePair<string, string>("part", "(<channel> *<message>) — parts from specified channel.")));

            DoCallback(PluginActionType.RegisterMethod, new MethodRegistrar(Commands.PRIVMSG, ListChannels, new KeyValuePair<string, string>("channels", "returns a list of connected channels.")));

            DoCallback(PluginActionType.RegisterMethod, new MethodRegistrar(Commands.PRIVMSG, Define, new KeyValuePair<string, string>("define", "(<word> *<part of speech>) — returns definition for given word.")));

            DoCallback(PluginActionType.RegisterMethod, new MethodRegistrar(Commands.PRIVMSG, Lookup, new KeyValuePair<string, string>("lookup", "(<term/phrase>) — returns the wikipedia summary of given term or phrase.")));

            DoCallback(PluginActionType.RegisterMethod, new MethodRegistrar(Commands.PRIVMSG, ListUsers, new KeyValuePair<string, string>("users", "returns a list of stored user realnames.")));

            DoCallback(PluginActionType.RegisterMethod, new MethodRegistrar(Commands.PRIVMSG, YouTubeLinkResponse));

            Log(IrcLogEntryType.System, $"{Name} loaded.");
        }

        public void Stop() {
            if (Status.Equals(PluginStatus.Running) ||
                Status.Equals(PluginStatus.Processing)) {
                Log(IrcLogEntryType.System, $"Stop called but process is running from: {Name}");
            } else {
                Log(IrcLogEntryType.System, $"Stop called from: {Name}");
                Call_Die();
            }
        }

        public void Log(IrcLogEntryType logType, string message) {
            DoCallback(PluginActionType.Log, new LogEntryEventArgs(logType, message));
        }

        public void Call_Die() {
            Status = PluginStatus.Stopped;
            Log(IrcLogEntryType.System, $"Calling die, stopping process, sending unload —— from: {Name}");
        }

        public event EventHandler<ActionEventArgs> CallbackEvent;

        private void Log(object source, LogEntryEventArgs logEntryEventArgs) {
            Log(logEntryEventArgs.EntryType, logEntryEventArgs.Message);
        }

        public void DoCallback(ActionEventArgs e) {
            CallbackEvent?.Invoke(this, e);
        }

        /// <summary>
        ///     Calls back to PluginWrapper
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="result"></param>
        public void DoCallback(PluginActionType actionType, object result = null) {
            CallbackEvent?.Invoke(this, new ActionEventArgs(actionType, result));
        }

        private void Reload(object source, ChannelMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("reload"))
                return;

            Status = PluginStatus.Processing;

            SimpleMessageEventArgs message = new SimpleMessageEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Root.GetUser(e.Message.Realname).Access > 1) {
                message.Message = "Insufficient permissions.";
                DoCallback(PluginActionType.SendMessage, message);
                return;
            }

            Status = PluginStatus.Running;

            message.Message = "Attempting to reload plugins.";
            DoCallback(PluginActionType.SendMessage, message);

            try {
                DoCallback(PluginActionType.Unload);
                DoCallback(PluginActionType.Load);
            } catch (Exception ex) {
                message.Message = "Error occured reloading plugins.";
                DoCallback(PluginActionType.SendMessage, message);
                Log(IrcLogEntryType.Error, $"Error reloading plugins: {ex}");
                return;
            }

            message.Message = "Sucessfully reloaded plugins.";
            DoCallback(PluginActionType.SendMessage, message);
        }

        private void Quit(object source, ChannelMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("quit"))
                return;

            DoCallback(PluginActionType.SendMessage, new SimpleMessageEventArgs(Commands.PRIVMSG, e.Message.Origin, "Shutting down."));
            DoCallback(PluginActionType.SignalTerminate);
        }

        private void Eval(object source, ChannelMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("eval"))
                return;

            Status = PluginStatus.Processing;

            SimpleMessageEventArgs message = new SimpleMessageEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Message.SplitArgs.Count < 3)
                message.Message = "Not enough parameters.";

            Status = PluginStatus.Running;

            if (string.IsNullOrEmpty(message.Message)) {
                Status = PluginStatus.Running;
                string evalArgs = e.Message.SplitArgs.Count > 3
                    ? e.Message.SplitArgs[2] + e.Message.SplitArgs[3]
                    : e.Message.SplitArgs[2];

                try {
                    message.Message = calculator.Evaluate(evalArgs).ToString(CultureInfo.CurrentCulture);
                } catch (Exception ex) {
                    message.Message = ex.Message;
                }
            }

            DoCallback(PluginActionType.SendMessage, message);

            Status = PluginStatus.Stopped;
        }

        private void Join(object source, ChannelMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("join"))
                return;

            Status = PluginStatus.Processing;

            SimpleMessageEventArgs message = new SimpleMessageEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Root.GetUser(e.Message.Realname).Access > 1)
                message.Message = "Insufficient permissions.";
            else if (e.Message.SplitArgs.Count < 3)
                message.Message = "Insufficient parameters. Type 'eve help join' to view command's help index.";
            else if (e.Message.SplitArgs.Count < 2 ||
                     !e.Message.SplitArgs[2].StartsWith("#"))
                message.Message = "Channel name must start with '#'.";
            else if (e.Root.Server.ChannelExists(e.Message.SplitArgs[2].ToLower()))
                message.Message = "I'm already in that channel.";

            Status = PluginStatus.Running;

            if (string.IsNullOrEmpty(message.Message)) {
                DoCallback(PluginActionType.SendMessage, new SimpleMessageEventArgs(Commands.JOIN, string.Empty, e.Message.SplitArgs[2]));
                e.Root.Server.AddChannel(e.Message.SplitArgs[2].ToLower());

                message.Message = $"Successfully joined channel: {e.Message.SplitArgs[2]}.";
            }

            DoCallback(PluginActionType.SendMessage, message);

            Status = PluginStatus.Stopped;
        }

        private void Part(object source, ChannelMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("part"))
                return;

            Status = PluginStatus.Processing;

            SimpleMessageEventArgs message = new SimpleMessageEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Root.GetUser(e.Message.Realname).Access > 1)
                message.Message = "Insufficient permissions.";
            else if (e.Message.SplitArgs.Count < 3)
                message.Message = "Insufficient parameters. Type 'eve help part' to view command's help index.";
            else if (e.Message.SplitArgs.Count < 2 ||
                     !e.Message.SplitArgs[2].StartsWith("#"))
                message.Message = "Channel parameter must be a proper name (starts with '#').";
            else if (e.Message.SplitArgs.Count < 2 ||
                     !e.Root.Server.ChannelExists(e.Message.SplitArgs[2]))
                message.Message = "I'm not in that channel.";

            Status = PluginStatus.Running;

            if (!string.IsNullOrEmpty(message.Message)) {
                DoCallback(PluginActionType.SendMessage, message);
                return;
            }

            string channel = e.Message.SplitArgs[2].ToLower();

            e.Root.Server.RemoveChannel(channel);

            message.Message = $"Successfully parted channel: {channel}";

            DoCallback(PluginActionType.SendMessage, message);
            DoCallback(PluginActionType.SendMessage, new SimpleMessageEventArgs(Commands.PART, string.Empty, $"{channel} Channel part invoked by: {e.Message.Nickname}"));

            Status = PluginStatus.Stopped;
        }

        private void ListChannels(object source, ChannelMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("channels"))
                return;

            Status = PluginStatus.Running;
            DoCallback(PluginActionType.SendMessage, new SimpleMessageEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Join(", ", e.Root.Server.GetAllChannels())));

            Status = PluginStatus.Stopped;
        }

        private void YouTubeLinkResponse(object source, ChannelMessagedEventArgs e) {
            if (!youtubeRegex.IsMatch(e.Message.Args))
                return;

            Status = PluginStatus.Running;

            const int maxDescriptionLength = 100;

            string getResponse = $"https://www.googleapis.com/youtube/v3/videos?part=snippet&id={youtubeRegex.Match(e.Message.Args).Groups["ID"]}&key={e.Root.GetApiKey("YouTube")}".HttpGet();

            JToken video = JObject.Parse(getResponse)["items"][0]["snippet"];
            string channel = (string)video["channelTitle"];
            string title = (string)video["title"];
            string description = video["description"].ToString().Split('\n')[0];
            string[] descArray = description.Split(' ');

            if (description.Length > maxDescriptionLength) {
                description = string.Empty;

                for (int i = 0; description.Length < maxDescriptionLength; i++)
                    description += $" {descArray[i]}";

                if (!description.EndsWith(" "))
                    description.Remove(description.LastIndexOf(' '));

                description += "....";
            }

            DoCallback(PluginActionType.SendMessage, new SimpleMessageEventArgs(Commands.PRIVMSG, e.Message.Origin, $"{title} (by {channel}) — {description}"));

            Status = PluginStatus.Stopped;
        }

        private void Define(object source, ChannelMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("define"))
                return;

            Status = PluginStatus.Processing;

            SimpleMessageEventArgs message = new SimpleMessageEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Message.SplitArgs.Count < 3) {
                message.Message = "Insufficient parameters. Type 'eve help define' to view correct usage.";
                DoCallback(PluginActionType.SendMessage, message);
                return;
            }

            Status = PluginStatus.Running;

            string partOfSpeech = e.Message.SplitArgs.Count > 3
                ? $"&part_of_speech={e.Message.SplitArgs[3]}"
                : string.Empty;

            JObject entry = JObject.Parse($"http://api.pearson.com/v2/dictionaries/laad3/entries?headword={e.Message.SplitArgs[2]}{partOfSpeech}&limit=1".HttpGet());

            if ((int)entry.SelectToken("count") < 1) {
                message.Message = "Query returned no results.";
                DoCallback(PluginActionType.SendMessage, message);
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

            message.Message = returnMessage;
            DoCallback(PluginActionType.SendMessage, message);

            Status = PluginStatus.Stopped;
        }

        private void Lookup(object source, ChannelMessagedEventArgs e) {
            Status = PluginStatus.Processing;

            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("lookup"))
                return;

            SimpleMessageEventArgs message = new SimpleMessageEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Message.SplitArgs.Count < 3) {
                message.Message = "Insufficient parameters. Type 'eve help lookup' to view correct usage.";
                DoCallback(PluginActionType.SendMessage, message);
                return;
            }

            Status = PluginStatus.Running;

            string query = string.Join(" ", e.Message.SplitArgs.Skip(1));
            string response = $"https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles={query}".HttpGet();

            JToken pages = JObject.Parse(response)["query"]["pages"].Values().First();

            if (string.IsNullOrEmpty((string)pages["extract"])) {
                message.Message = "Query failed to return results. Perhaps try a different term?";
                DoCallback(PluginActionType.SendMessage, message);
                return;
            }

            string fullReplyStr = $"\x02{(string)pages["title"]}\x0F — {Regex.Replace((string)pages["extract"], @"\n\n?|\n", " ")}";

            message.Target = e.Message.Nickname;

            foreach (string splitMessage in fullReplyStr.Split(400)) {
                message.Message = splitMessage;
                DoCallback(PluginActionType.SendMessage, message);
            }

            Status = PluginStatus.Stopped;
        }

        private void ListUsers(object source, ChannelMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("users"))
                return;

            Status = PluginStatus.Running;
            DoCallback(PluginActionType.SendMessage, new SimpleMessageEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Join(", ", e.Root.GetAllUsernames())));

            Status = PluginStatus.Stopped;
        }

        private void Set(object source, ChannelMessagedEventArgs e) {
            if (e.Message.SplitArgs.Count < 2 ||
                !e.Message.SplitArgs[1].Equals("set"))
                return;

            Status = PluginStatus.Processing;

            SimpleMessageEventArgs message = new SimpleMessageEventArgs(Commands.PRIVMSG, e.Message.Origin, string.Empty);

            if (e.Message.SplitArgs.Count < 5) {
                message.Message = "Insufficient parameters. Type 'eve help lookup' to view correct usage.";
                DoCallback(PluginActionType.SendMessage, message);
                return;
            }

            if (e.Root.GetUser(e.Message.Nickname).Access > 0)
                message.Message = "Insufficient permissions.";

            //e.Root.GetUser()

            Status = PluginStatus.Stopped;
        }
    }
}