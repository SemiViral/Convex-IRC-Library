#region usings

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

#endregion

namespace Convex.Resources {
    public partial class ClientConfiguration : IDisposable {
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
            get {
                return string.IsNullOrEmpty(logFilePath)
                    ? DefaultLogFilePath
                    : logFilePath;
            }
            set { logFilePath = value; }
        }

        public string DatabaseFilePath {
            get {
                return string.IsNullOrEmpty(databaseFilePath)
                    ? DefualtDatabaseFilePath
                    : databaseFilePath;
            }
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
            if (File.Exists(path))
                return;

            Console.WriteLine("Configuration file not found, creating.\n");

            WriteConfig(DEFAULT_CONFIG, path);
        }

        protected virtual void Dispose(bool dispose) {
            if (!dispose || disposed)
                return;

            WriteConfig(JsonConvert.SerializeObject(this), string.IsNullOrEmpty(FilePath)
                ? DefaultFilePath
                : FilePath);

            disposed = true;
        }
    }
}