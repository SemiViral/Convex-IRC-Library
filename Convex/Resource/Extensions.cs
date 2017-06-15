#region usings

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Convex.Event;
using Microsoft.Data.Sqlite;

#endregion

namespace Convex.Resource {
    public static class Extensions {
        /// <summary>
        ///     Obtain HTTP response from a GET request
        /// </summary>
        /// <returns>GET response</returns>
        public static async Task<string> HttpGet(this string instance) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(instance);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(instance);
                string message = string.Empty;

                if (response.IsSuccessStatusCode)
                    message = await response.Content.ReadAsStringAsync();

                return message;
            }
        }

        /// <summary>
        ///     Splits a string into seperate parts
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="maxLength">max length of individual strings to split</param>
        public static IEnumerable<string> SplitByLength(this string instance, int maxLength) {
            for (int i = 0; i < instance.Length; i += maxLength)
                yield return instance.Substring(i, Math.Min(maxLength, instance.Length - i));
        }

        public static string DeliminateSpaces(this string str) {
            StringBuilder deliminatedSpaces = new StringBuilder();
            bool isSpace = false;

            // using for loop to increase speed
            for (int i = 0; i < str.Length; i++) {
                if (str[i].Equals(' ')) {
                    if (isSpace)
                        continue;

                    isSpace = true;
                } else {
                    isSpace = false;
                }

                deliminatedSpaces.Append(str[i]);
            }

            return deliminatedSpaces.ToString();
        }

        public static async Task QueryAsync(this SqliteConnection source, BasicEventArgs e) {
            await source.OpenAsync();

            using (SqliteTransaction transaction = source.BeginTransaction()) {
                using (SqliteCommand command = source.CreateCommand()) {
                    command.Transaction = transaction;
                    command.CommandText = e.Contents;
                    await command.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }
        }

        public static void Query(this SqliteConnection source, BasicEventArgs e) {
            source.Open();

            using (SqliteTransaction transaction = source.BeginTransaction()) {
                using (SqliteCommand command = source.CreateCommand()) {
                    command.Transaction = transaction;
                    command.CommandText = e.Contents;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public static bool InputEquals(this ServerMessagedEventArgs e, string compareTo) => e.Message.InputCommand.Equals(compareTo);
    }
}