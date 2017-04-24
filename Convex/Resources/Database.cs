#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Convex.Net;
using Convex.Types;
using Microsoft.Data.Sqlite;

#endregion

namespace Convex.Resources {
    public class Database {
        /// <summary>
        ///     Initialise connections to database and sets properties
        /// </summary>
        public Database(string databaseLocation) {
            Location = databaseLocation;

            if (!File.Exists(Location))
                CreateDatabase();
        }

        internal static string Location { get; private set; }
        internal static bool Connected { get; private set; }
        internal event EventHandler<LogEntryEventArgs> LogEntryEventHandler;

        private void CreateDatabase() {
            Log(new LogEntryEventArgs(IrcLogEntryType.System, "Main database not found, creating."));

            Query(this, new QueryEventArgs("CREATE TABLE users (id int, nickname string, realname string, access int, seen string)"));
            Query(this, new QueryEventArgs("CREATE TABLE messages (id int, sender string, message string, datetime string)"));

            Connected = true;
        }

        private static SqliteConnection GetConnection(string source) {
            return new SqliteConnection(new SqliteConnectionStringBuilder {
                DataSource = source
            }.ToString());
        }

        internal ICollection<User> GetAllUsers() {
            List<User> users = new List<User>();

            using (SqliteConnection connection = GetConnection(Location)) {
                connection.Open();

                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    SqliteCommand getUsers = connection.CreateCommand();
                    getUsers.Transaction = transaction;
                    getUsers.CommandText = "SELECT * FROM users";

                    using (SqliteDataReader userEntries = getUsers.ExecuteReader()) {
                        while (userEntries.Read()) {
                            int id = Convert.ToInt32(userEntries.GetValue(0));
                            string nickname = userEntries.GetValue(1).ToString();
                            string realname = (string)userEntries.GetValue(2);
                            int access = Convert.ToInt32(userEntries.GetValue(3));
                            DateTime seen = DateTime.Parse((string)userEntries.GetValue(4));

                            users.Add(new User(id, nickname, realname, access, seen));
                        }
                    }

                    transaction.Commit();
                }
            }

            ReadMessagesIntoUsers(users);
            
            return users;
        }

        private void ReadMessagesIntoUsers(ICollection<User> users) {
            if (users.Count.Equals(0))
                return;
            using (SqliteConnection connection = GetConnection(Location)) {
                connection.Open();

                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    SqliteCommand getMessages = connection.CreateCommand();
                    getMessages.Transaction = transaction;
                    getMessages.CommandText = "SELECT * FROM messages";

                    using (SqliteDataReader messages = getMessages.ExecuteReader()) {
                        while (messages.Read())
                            users.SingleOrDefault(e => e.Id.Equals(Convert.ToInt32(messages["id"])))?.Messages.Add(new Message((int)messages["id"], (string)messages["sender"], (string)messages["message"], DateTime.Parse((string)messages["datetime"])));
                    }

                    transaction.Commit();
                }
            }
            }

        private static void Query(object source, QueryEventArgs e) {
            using (SqliteConnection connection = GetConnection(Location)) {
                connection.Open();

                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    SqliteCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = e.Query;
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
        }

        internal int GetLastDatabaseId() {
            int id;

            using (SqliteConnection connection = GetConnection(Location)) {
                connection.Open();

                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    SqliteCommand getId = connection.CreateCommand();
                    getId.Transaction = transaction;
                    getId.CommandText = "SELECT MAX(id) FROM users";
                    id = (int)getId.ExecuteScalar();

                    transaction.Commit();
                }
            }

            return id;
        }

        public bool UserExists(string realname) {
            bool exists;

            using (SqliteConnection connection = GetConnection(Location)) {
                connection.Open();

                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    SqliteCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.Parameters.AddWithValue("@user", realname);
                    command.CommandText = "SELECT COUNT(*) FROM users WHERE ([realname] = @user)";

                    exists = Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }

            return exists;
        }

        internal void CreateUser(User user) {
            CreateUser(user.Id, user.Nickname, user.Realname, user.Seen);
        }

        /// <summary>
        ///     Creates a new user and updates the users & userTimeouts collections
        /// </summary>
        /// <param name="access">access level of user</param>
        /// <param name="nickname">nickname of user</param>
        /// <param name="realname">realname of user</param>
        /// <param name="seen">last time user was seen</param>
        internal void CreateUser(int access, string nickname, string realname, DateTime seen) {
            Log(new LogEntryEventArgs(IrcLogEntryType.System, $"Creating database entry for {realname}."));

            Query(this, new QueryEventArgs($"INSERT INTO users VALUES ({GetLastDatabaseId() + 1}, '{nickname}', '{realname}', {access}, '{seen}')"));
        }

        private void Log(LogEntryEventArgs e) {
            LogEntryEventHandler?.Invoke(this, e);
        }
    }

    public class QueryEventArgs : EventArgs {
        public QueryEventArgs(string args) {
            Query = args;
        }

        public string Query { get; }
    }
}