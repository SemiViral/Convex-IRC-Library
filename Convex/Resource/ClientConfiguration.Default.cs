#region usings

using System;

#endregion

namespace Convex.Resource {
    public partial class ClientConfiguration {
        // I know this isn't readable. Just run the program once and you'll get a much cleaner
        // representation of the default config in the generated config.json
        public const string DEFAULT_CONFIG = "{\r\n\t\"IgnoreList\": [],\r\n\t\"ApiKeys\": { \"YouTube\": \"\", \"Dictionary\": \"\" },\r\n\t\"Realname\": \"Evealyn\",\r\n\t\"Nickname\": \"Eve\",\r\n\t\"Password\": \"evepass\",\r\n\t\"DatabaseFilePath\": \"\",\r\n\t\"LogFilePath\": \"\"\r\n}\r\n";
        public static string DefaultResourceDirectory = $"{AppContext.BaseDirectory}\\Resources";
        public static string DefaultFilePath = DefaultResourceDirectory + "\\config.json";
        public static string DefualtDatabaseFilePath = DefaultResourceDirectory + "\\users.sqlite";
        public static string DefaultLogFilePath = DefaultResourceDirectory + "\\Log.txt";
    }
}