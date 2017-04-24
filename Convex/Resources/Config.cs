#region usings

using System;
using System.Collections.Generic;
using System.IO;
using Convex.Types;
using Newtonsoft.Json;

#endregion

namespace Convex.Resources {
    public partial class Config : IDisposable {
        private bool disposed;
        
        public List<string> IgnoreList { get; } = new List<string>();
        public Dictionary<string, string> ApiKeys { get; } = new Dictionary<string, string>();

        public static string FilePath { get; set; } = "config.json";

        public Server Server { get; set; }
        public string Realname { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string DatabaseAddress { get; set; }
        public string LogAddress { get; set; }

        public void Dispose() {
            Dispose(true);
        }

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

            WriteConfig(DEFAULT_CONFIG);
        }

        protected virtual void Dispose(bool dispose) {
            if (!dispose || disposed)
                return;

            WriteConfig(JsonConvert.SerializeObject(this));

            disposed = true;
        }
    }
}