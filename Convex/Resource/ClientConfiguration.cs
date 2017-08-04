#region usings

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

#endregion

namespace Convex.Resource {
    public class ClientConfiguration : IDisposable {
        private string databaseFilePath;
        private bool disposed;
        private string logFilePath;

        public List<string> IgnoreList { get; } = new List<string>();
        public Dictionary<string, string> ApiKeys { get; } = new Dictionary<string, string>();

        public string FilePath { get; set; }

        public string Realname { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }

        public string LogFilePath {
            get { return string.IsNullOrEmpty(logFilePath) ? DefaultLogFilePath : logFilePath; }
            set { logFilePath = value; }
        }

        public string DatabaseFilePath {
            get { return string.IsNullOrEmpty(databaseFilePath) ? DefualtDatabaseFilePath : databaseFilePath; }
            set { databaseFilePath = value; }
        }

        public void Dispose() {
            Dispose(true);
        }

        private static void WriteConfig(string configString, string path) {
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write)) {
                using (StreamWriter writer = new StreamWriter(stream)) {
                    writer.WriteLine(configString);
                    writer.Flush();
                }
            }
        }

        public static void CheckCreateConfig(string path) {
            if (File.Exists(path)) {
                return;
            }

            Console.WriteLine("Configuration file not found, creating.\n");

            WriteConfig(DEFAULT_CONFIG, path);
        }

        protected virtual void Dispose(bool dispose) {
            if (!dispose || disposed) {
                return;
            }

            WriteConfig(JsonConvert.SerializeObject(this), string.IsNullOrEmpty(FilePath) ? DefaultFilePath : FilePath);

            disposed = true;
        }

        #region defaults

        // I know this isn't readable. Just run the program once and you'll get a much cleaner
        // representation of the default config in the generated config.json
        public const string DEFAULT_CONFIG = "{\r\n\t\"IgnoreList\": [],\r\n\t\"ApiKeys\": { \"YouTube\": \"\", \"Dictionary\": \"\" },\r\n\t\"Realname\": \"Evealyn\",\r\n\t\"Nickname\": \"Eve\",\r\n\t\"Password\": \"evepass\",\r\n\t\"DatabaseFilePath\": \"\",\r\n\t\"LogFilePath\": \"\"\r\n}\r\n";
        public static readonly string DefaultResourceDirectory = $"{AppContext.BaseDirectory}\\Resources";
        public static readonly string DefaultFilePath = DefaultResourceDirectory + "\\config.json";
        public static readonly string DefualtDatabaseFilePath = DefaultResourceDirectory + "\\users.sqlite";
        public static readonly string DefaultLogFilePath = DefaultResourceDirectory + "\\Log.txt";

        #endregion
    }
}