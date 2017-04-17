#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Convex.Resources;
using Microsoft.Data.Sqlite;

#endregion

namespace Convex.Types {
    public class Database {
        /// <summary>
        ///     Initialise connections to database and sets properties
        /// </summary>
        public Database(string databaseLocation) {
            Location = databaseLocation;

            if (!File.Exists(Location))
                CreateDatabase();

            Log(IrcLogEntryType.System, "Loaded database.");
        }

        internal static string Location { get; private set; }
        internal static bool Connected { get; private set; }
        internal event EventHandler<LogEntry> LogEntryEventHandler;

        private void CreateDatabase() {
            Log(IrcLogEntryType.System, "MainDatabase not found, creating.");

            SimpleQuery("CREATE TABLE users (id int, nickname string, realname string, access int, seen string)", "CREATE TABLE messages (id int, sender string, message string, datetime string)");

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

            Log(IrcLogEntryType.System, "User list loaded.");

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
                            users.SingleOrDefault(e => e.Id.Equals(Convert.ToInt32(messages["id"])))?.Messages.Add(new Message((string)messages["sender"], (string)messages["message"], DateTime.Parse((string)messages["datetime"])));
                    }

                    transaction.Commit();
                }
            }

            Log(IrcLogEntryType.System, "Messages loaded.");
        }

        public void SimpleQuery(params string[] queries) {
            using (SqliteConnection connection = GetConnection(Location)) {
                connection.Open();

                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    foreach (string query in queries) {
                        SqliteCommand command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        ///     Returns int value of last ID in default database
        /// </summary>
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

        private void Log(IrcLogEntryType entryType, string message) {
            LogEntryEventHandler?.Invoke(this, new LogEntry(entryType, message));
        }
    }
}