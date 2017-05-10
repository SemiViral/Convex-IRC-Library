#region usings

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Convex.Types.Messages;
using Convex.Types.References;

#endregion

namespace Convex.Types {
    public class Channel {
        public Channel(string name) {
            Name = name;
            Topic = string.Empty;
            Inhabitants = new List<string>();
            Modes = new List<IrcMode>();
            if (!Name.StartsWith("#"))
                IsPrivate = true;

            LogArchive.CollectionChanged += SizeLogBuffer;
        }

        /// <summary>
        ///     Determines how many messages are externally viewable
        /// </summary>
        public int BufferSize { get; set; } = 150;

        public string Name { get; }
        public string Topic { get; set; }
        public List<string> Inhabitants { get; }
        public List<IrcMode> Modes { get; }
        public bool IsPrivate { get; }

        /// <summary>
        ///     Externally displayable log of messages
        /// </summary>
        public ObservableCollection<SimpleMessage> LogBuffer { get; } = new ObservableCollection<SimpleMessage>();

        /// <summary>
        ///     Internally kept log of messages
        /// </summary>
        private ObservableCollection<SimpleMessage> LogArchive { get; } = new ObservableCollection<SimpleMessage>();

        public bool Connected { get; set; } = false;

        public void LogChannelMessage(SimpleMessage simpleMessage) {
            LogArchive.Add(simpleMessage);
        }

        /// <summary>
        ///     Automatically maintains the size and accuracy of the external LogBuffer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeLogBuffer(object sender, NotifyCollectionChangedEventArgs e) {
            if (LogBuffer.Count + e.NewItems.Count > BufferSize)
                foreach (SimpleMessage message in e.NewItems) {
                    LogBuffer.RemoveAt(0);
                    LogBuffer.Add(message);
                }
            else
                foreach (SimpleMessage message in e.NewItems)
                    LogBuffer.Add(message);
        }
    }
}