#region usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Convex.ComponentModel;

#endregion

namespace Convex.Resource {
    public sealed class User : INotifyPropertyChanged {
        private int access;
        private int attempts;
        private int id;
        private string nickname;
        private string realname;
        private DateTime seen;

        public User(int id, string nickname, string realname, int access) {
            Id = id;
            Nickname = nickname;
            Realname = realname;
            Access = access;
            Seen = DateTime.Now;
        }

        public int Id {
            get { return id; }
            set {
                id = value;
                NotifyPropertyChanged(value);
            }
        }

        public int Attempts {
            get { return attempts; }
            set {
                attempts = value;
                NotifyPropertyChanged(value);
            }
        }

        public string Nickname {
            get { return nickname; }
            set {
                nickname = value;
                NotifyPropertyChanged(value);
            }
        }

        public string Realname {
            get { return realname; }
            set {
                realname = value;
                NotifyPropertyChanged(value);
            }
        }

        public int Access {
            get { return access; }
            set {
                access = value;
                NotifyPropertyChanged(value);
            }
        }

        public DateTime Seen {
            get { return seen; }
            set {
                seen = value;
                NotifyPropertyChanged(value);
            }
        }

        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();
        public List<string> Channels { get; } = new List<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Discern whether a user has exceeded command-querying limit
        /// </summary>
        /// <returns>true: user timeout</returns>
        public bool GetTimeout() {
            bool doTimeout = false;

            if (Attempts.Equals(4))
                if (Seen.AddMinutes(1) < DateTime.UtcNow)
                    Attempts = 0; // if so, reset their attempts to 0
                else
                    doTimeout = true; // if not, timeout is true
            else if (Access > 1)
                // if user isn't admin/op, increment their attempts
                Attempts++;

            return doTimeout;
        }

        private void NotifyPropertyChanged(object newValue, [CallerMemberName] string memberName = "") {
            OnPropertyChanged(this, new UserPropertyChangedEventArgs(memberName, Realname, newValue));
        }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            PropertyChanged?.Invoke(sender, e);
        }

        /// <summary>
        ///     Adds a Args object to list
        /// </summary>
        /// <param name="user">user object</param>
        /// <param name="message"><see cref="Message" /> to be added</param>
        public void AddMessage(User user, Message message) {
            user.Messages.Add(message);
        }
    }

    public class Message {
        public Message(int id, string sender, string contents, DateTime timestamp) {
            Id = id;
            Sender = sender;
            Contents = contents;
            Date = timestamp;
        }

        public int Id { get; }
        public string Sender { get; }
        public string Contents { get; }
        public DateTime Date { get; }
    }
}