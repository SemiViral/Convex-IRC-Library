namespace Convex.Resources {
    public partial class Config {
        // I know this isn't readable. Just run the program once and you'll get a much cleaner
        // representation of the default config in the generated config.json

        public const string DEFAULT_CONFIG = "{\r\n\t\"Server\": {\r\n\t\t\"Connection\": {\r\n\t\t\t\"Address\": \"irc.foonetic.net\",\r\n\t\t\t\"Port\": 6667\r\n\t\t},\r\n\t\t\"Initialised\": true,\r\n\t\t\"Execute\": false,\r\n\t\t\"Address\": \"irc.foonetic.net\",\r\n\t\t\"Port\": 6667,\r\n\t\t\"Channels\": [\r\n\t\t\t{\r\n\t\t\t\t\"Name\": \"#testgrounds\",\r\n\t\t\t\t\"Topic\": null,\r\n\t\t\t\t\"Inhabitants\": [],\r\n\t\t\t\t\"Modes\": [],\r\n\t\t\t\t\"Connected\": false\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"Name\": \"#testgrounds2\",\r\n\t\t\t\t\"Topic\": null,\r\n\t\t\t\t\"Inhabitants\": [],\r\n\t\t\t\t\"Modes\": [],\r\n\t\t\t\t\"Connected\": false\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"Inhabitants\": []\r\n\t},\r\n\t\"IgnoreList\": [],\r\n\t\"ApiKeys\": {\r\n\t\t\"YouTube\": \"\",\r\n\t\t\"Dictionary\": \"\"\r\n\t},\r\n\t\"Identified\": false,\r\n\t\"Realname\": \"Evealyn\",\r\n\t\"Nickname\": \"Eve\",\r\n\t\"Password\": \"evepass\",\r\n\t\"DatabaseAddress\": \"users.sqlite\",\r\n\t\"LogAddress\": \"Log.txt\"\r\n}";
    }
}