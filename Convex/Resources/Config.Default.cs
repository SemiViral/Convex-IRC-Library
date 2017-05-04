using System;

namespace Convex.Resources {
    public partial class Config {
        public static string DefaultResourceDirectory = $"{AppContext.BaseDirectory}\\Resources";
        public static string DefaultConfigFilePath = DefaultResourceDirectory + "\\config.json";
        public static string DefualtDatabaseFilePath = DefaultResourceDirectory + "\\users.sqlite";
        public static string DefaultLogFilePath = DefaultResourceDirectory + "\\Log.txt";

        // I know this isn't readable. Just run the program once and you'll get a much cleaner
        // representation of the default config in the generated config.json
        public const string DEFAULT_CONFIG = "{\r\n\t\"IgnoreList\": [],\r\n\t\"ApiKeys\": { \"YouTube\": \"\", \"Dictionary\": \"\" },\r\n\t\"Server\": {\r\n\t\t\"Connection\": { \"Address\": \"irc.foonetic.net\", \"Port\": 6667 },\r\n\t\t\"Identified\": false,\r\n\t\t\"Initialised\": false,\r\n\t\t\"Execute\": false,\r\n\t\t\"Channels\": [\r\n\t\t\t{ \"Name\": \"#testgrounds\", \"Topic\": null, \"Inhabitants\": [], \"Modes\": [], \"Connected\": false },\r\n\t\t\t{ \"Name\": \"#testgrounds2\", \"Topic\": null, \"Inhabitants\": [], \"Modes\": [], \"Connected\": false }\r\n\t\t],\r\n\t\t\"Inhabitants\": []\r\n\t},\r\n\t\"Realname\": \"Evealyn\",\r\n\t\"Nickname\": \"Eve\",\r\n\t\"Password\": \"evepass\",\r\n\t\"DatabaseFilePath\": \"\",\r\n\t\"LogFilePath\": \"\"\r\n}";
    }
}