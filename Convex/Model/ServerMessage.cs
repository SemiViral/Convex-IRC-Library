#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Convex.ComponentModel;
using Convex.ComponentModel.Reference;

#endregion

namespace Convex.Model {
    public class ServerMessage {
        // Regex for parsing RawMessage messages
        private static readonly Regex _MessageRegex = new Regex(@"^:(?<Sender>[^\s]+)\s(?<Type>[^\s]+)\s(?<Recipient>[^\s]+)\s?:?(?<Args>.*)", RegexOptions.Compiled);

        private static readonly Regex _SenderRegex = new Regex(@"^(?<Nickname>[^\s]+)!(?<Realname>[^\s]+)@(?<Hostname>[^\s]+)", RegexOptions.Compiled);

        public readonly Dictionary<string, string> Tags = new Dictionary<string, string>();

        public ServerMessage(string rawData) {
            RawMessage = rawData.Trim();

            if (rawData.StartsWith("ERROR")) {
                Command = Commands.ERROR;
                Args = rawData.Substring(rawData.IndexOf(' ') + 1);
                return;
            }
            Parse();
        }

        public bool IsIrCv3Message { get; private set; }

        public string RawMessage { get; }

        public string Nickname { get; private set; }
        public string Realname { get; private set; }
        public string Hostname { get; private set; }
        public string Origin { get; private set; }
        public string Command { get; private set; }
        public string Args { get; private set; }
        public List<string> SplitArgs { get; private set; }

        public DateTime Timestamp { get; private set; }

        public string InputCommand { get; set; } = string.Empty;

        /// <summary>
        ///     For parsing IRCv3 message tags
        /// </summary>
        private void ParseTagsPrefix() {
            if (!RawMessage.StartsWith("@"))
                return;

            IsIrCv3Message = true;

            string fullTagsPrefix = RawMessage.Substring(0, RawMessage.IndexOf(' '));
            string[] primitiveTagsCollection = RawMessage.Split(';');

            foreach (string[] splitPrimitiveTag in primitiveTagsCollection.Select(primitiveTag => primitiveTag.Split('=')))
                Tags.Add(splitPrimitiveTag[0], splitPrimitiveTag[1] ?? string.Empty);
        }

        public void Parse() {
            if (!_MessageRegex.IsMatch(RawMessage))
                return;

            ParseTagsPrefix();

            Timestamp = DateTime.Now;

            // begin parsing message into sections
            Match mVal = _MessageRegex.Match(RawMessage);
            Match sMatch = _SenderRegex.Match(mVal.Groups["Sender"].Value);

            // class property setting
            Nickname = mVal.Groups["Sender"].Value;
            Realname = mVal.Groups["Sender"].Value.ToLower();
            Hostname = mVal.Groups["Sender"].Value;
            Command = mVal.Groups["Type"].Value;
            Origin = mVal.Groups["Recipient"].Value.StartsWith(":")
                ? mVal.Groups["Recipient"].Value.Substring(1)
                : mVal.Groups["Recipient"].Value;

            Args = mVal.Groups["Args"].Value.DeliminateSpaces();

            // splits the first 5 sections of the message for parsing
            SplitArgs = Args.Split(new[] {' '}, 4)
                .Select(arg => arg.Trim())
                .ToList();

            if (!sMatch.Success)
                return;

            string realname = sMatch.Groups["Realname"].Value;
            Nickname = sMatch.Groups["Nickname"].Value;
            Realname = realname.StartsWith("~")
                ? realname.Substring(1)
                : realname;
            Hostname = sMatch.Groups["Hostname"].Value;
        }
    }
}