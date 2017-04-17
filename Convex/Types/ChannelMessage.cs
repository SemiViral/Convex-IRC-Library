#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Convex.Types {
    [Serializable]
    public class ChannelMessage {
        // Regex for parsing RawMessage messages
        private static readonly Regex _messageRegex = new Regex(@"^:(?<Sender>[^\s]+)\s(?<Type>[^\s]+)\s(?<Recipient>[^\s]+)\s?:?(?<Args>.*)", RegexOptions.Compiled);

        private static readonly Regex _senderRegex = new Regex(@"^(?<Nickname>[^\s]+)!(?<Realname>[^\s]+)@(?<Hostname>[^\s]+)", RegexOptions.Compiled);

        public Dictionary<string, string> Tags = new Dictionary<string, string>();

        public ChannelMessage(string rawData) {
            RawMessage = rawData.Trim();
            Parse();
        }

        public bool IsIRCv3Message { get; private set; }

        public string RawMessage { get; }

        public DateTime Timestamp { get; private set; }

        public string Nickname { get; private set; }
        public string Realname { get; private set; }
        public string Hostname { get; private set; }
        public string Origin { get; private set; }
        public string Type { get; private set; }
        public string Args { get; private set; }
        public List<string> SplitArgs { get; private set; }

        private void ParseTagsPrefix() {
            if (!RawMessage.StartsWith("@"))
                return;

            IsIRCv3Message = true;

            string fullTagsPrefix = RawMessage.Substring(0, RawMessage.IndexOf(' '));
            string[] primitiveTagsCollection = RawMessage.Split(';');

            foreach (string[] splitPrimitiveTag in primitiveTagsCollection.Select(primitiveTag => primitiveTag.Split('=')))
                Tags.Add(splitPrimitiveTag[0], splitPrimitiveTag[1] ?? string.Empty);
        }

        public void Parse() {
            if (!_messageRegex.IsMatch(RawMessage))
                return;

            ParseTagsPrefix();

            Timestamp = DateTime.Now;

            // begin parsing message into sections
            Match mVal = _messageRegex.Match(RawMessage);
            Match sMatch = _senderRegex.Match(mVal.Groups["Sender"].Value);

            // class property setting
            Nickname = mVal.Groups["Sender"].Value;
            Realname = mVal.Groups["Sender"].Value.ToLower();
            Hostname = mVal.Groups["Sender"].Value;
            Type = mVal.Groups["Type"].Value;
            Origin = mVal.Groups["Recipient"].Value.StartsWith(":")
                ? mVal.Groups["Recipient"].Value.Substring(1)
                : mVal.Groups["Recipient"].Value;

            string args = mVal.Groups["Args"].Value;
            Args = mVal.Groups["Args"].Value.DeliminateSpaces();

            // splits the first 5 sections of the message for parsing
            SplitArgs = Args.Split(new[] {' '}, 4).Select(arg => arg.Trim()).ToList();

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