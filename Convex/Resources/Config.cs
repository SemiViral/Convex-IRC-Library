#region usings

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

#endregion

namespace Convex.Resources {
    public class Config : IDisposable {
        public const string BASE_CONFIG = "{\r\n  \"Nickname\": \"Eve\",\r\n  \"Realname\": \"Evealyn\",\r\n  \"Password\": \"evepass\",\r\n  \"Server\": \"irc.foonetic.net\",\r\n  \"Port\": 6667,\r\n  \"Channels\": [ \"#testgrounds\" ],\r\n  \"IgnoreList\": [],\r\n  \"DatabaseLocation\": \"users.sqlite\",\r\n  \"ApiKeys\": {\r\n    \"YouTube\": \"\",\r\n    \"Dictionary\": \"\"\r\n  }\r\n}";

        private static void WriteConfig(string configString) {
            using (FileStream stream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(configString);
                writer.Flush();
            }
        }

        public static void CheckCreate() {
            if (File.Exists(FilePath))
                return;

            Console.WriteLine("Configuration file not found, creating.");

            WriteConfig(BASE_CONFIG);
        }

        public List<string> IgnoreList { get; set; } = new List<string>();
        public Dictionary<string, string> ApiKeys { get; set; } = new Dictionary<string, string>();

        public static string FilePath { get; set; } = "config.json";

        public bool Identified { get; set; }
        
        public string Server { get; set; }
        public string[] Channels { get; set; }
        public string Realname { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string DatabaseLocation { get; set; }
        public int Port { get; set; }

        private bool disposed;

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose) {
            if (!dispose || disposed)
                return;

            WriteConfig(JsonConvert.SerializeObject(this));

            disposed = true;
        }
    }
}