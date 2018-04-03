namespace Convex.ComponentModel.Reference {
    public static class Commands {
        public const string DEFAULT = "";
        public const string ABORT = "ABORT";
        public const string USER = "USER";
        public const string NICK = "NICK";
        public const string QUIT = "QUIT";
        public const string JOIN = "JOIN";
        public const string PART = "PART";
        public const string PRIVMSG = "PRIVMSG";
        public const string MODE = "MODE";
        public const string TOPIC = "TOPIC";
        public const string KICK = "KICK";
        public const string NOTICE = "NOTICE";
        public const string NAMES = "NAMES";
        public const string LIST = "LIST";
        public const string MOTD = "MOTD";
        public const string VERSION = "VERSION";
        public const string STATS = "STATS";
        public const string LINKS = "LINKS";
        public const string TIME = "TIME";
        public const string CONNECT = "CONNECT";
        public const string ADMIN = "ADMIN";
        public const string INFO = "INFO";
        public const string SERVLIST = "SERVERLIST";
        public const string WHO = "WHO";
        public const string WHOIS = "WHOIS";
        public const string WHOWAS = "WHOWAS";
        public const string KILL = "KILL";
        public const string PING = "PING";
        public const string PONG = "PONG";
        public const string ERROR = "ERROR";
        public const string AWAY = "AWAY";
        public const string REHASH = "REHASH";
        public const string USERS = "USERS";
        public const string USERHOST = "USERHOST";
        public const string ISON = "ISON";

        /// <summary>
        ///     [nick]![user]@[host]
        /// </summary>
        public const string WELCOME = "001";

        /// <summary>
        ///     Host is [servername]; version [ver]
        /// </summary>
        public const string YOUR_HOST = "002";

        /// <summary>
        ///     Created [date]
        /// </summary>
        public const string CREATION_DATE = "003";

        /// <summary>
        ///     [servername] [version] [available user modes] [available channel modes]
        /// </summary>
        public const string YOUR_INFO = "004";

        /// <summary>
        ///     Try server [server name]; port [port number]
        /// </summary>
        public const string ALT_SERVER = "005";

        /// <summary>
        ///     :*1[reply] *( " " [reply] )
        /// </summary>
        public const string USERHOST_REPLY = "302";

        /// <summary>
        ///     :*1[nick] *( " " [nick] )
        /// </summary>
        public const string ISON_REPLY = "303";

        /// <summary>
        ///     [nick] :[away message]
        /// </summary>
        public const string NICK_AWAY = "301";

        /// <summary>
        ///     :No longer marked away
        /// </summary>
        public const string NOT_AWAY = "305";

        /// <summary>
        ///     :You have been marked as away
        /// </summary>
        public const string SELF_AWAY = "306";

        /// <summary>
        ///     [nick] [user] [host] * :[real name]
        /// </summary>
        public const string WHOIS_REPLY = "311";

        /// <summary>
        ///     [nick] [server] :[server info]
        /// </summary>
        public const string WHOIS_SERVER_REPLY = "312";

        /// <summary>
        ///     [nick] :is an IRC operator
        /// </summary>
        public const string WHOIS_OPERATOR_REPLY = "313";

        /// <summary>
        ///     [nick] [integer] :seconds idle
        /// </summary>
        public const string WHOIS_IDLE_REPLY = "317";

        /// <summary>
        ///     [nick] :End of WHOIS list
        /// </summary>
        public const string WHOIS_END = "318";

        /// <summary>
        ///     [nick] :*( ( "@" / "+" ) [channel] " " )
        /// </summary>
        public const string WHOIS_CHANNELS_REPLY = "319";

        /// <summary>
        ///     [nick] [user] [host] * :[real name]
        /// </summary>
        public const string WHOWAS_USER_REPLY = "314";

        /// <summary>
        ///     [nick] :End of WHOWAS
        /// </summary>
        public const string WHOWAS_USER_END = "315";

        /// <summary>
        ///     [channel] [# visible] :[topic]
        /// </summary>
        public const string LIST_REPLY = "322";

        /// <summary>
        ///     :End of LIST
        /// </summary>
        public const string LIST_END = "323";

        /// <summary>
        ///     [channel] [nickname]
        /// </summary>
        public const string UNIQUE_OP_IS = "325";

        /// <summary>
        ///     [channel] [mode] [mode params]
        /// </summary>
        public const string CHANNEL_MODE_IS = "324";

        /// <summary>
        ///     [channel] :No topic is set
        /// </summary>
        public const string NO_CHANNEL_TOPIC = "331";

        /// <summary>
        ///     [channel] :[topic]
        /// </summary>
        public const string CHANNEL_TOPIC = "332";

        /// <summary>
        ///     [version].[debuglevel] [server] :[comments]
        /// </summary>
        public const string VERSION_REPLY = "351";

        /// <summary>
        ///     [channel] [user] [host] [server] [nick] ( "H" / "G" ] ["*"] [ ("@" / "+")] :[hopcount] [real name]
        /// </summary>
        public const string WHO_REPLY = "352";

        /// <summary>
        ///     [name] :End of WHO list
        /// </summary>
        public const string WHO_REPLY_END = "315";

        /// <summary>
        ///     ( "=" / "*" / "@" ) [channel] :[ "@" / "+" ] [nick] *( " " [ "@" / "+" ] [nick] )
        /// </summary>
        public const string NAMES_REPLY = "353";

        /// <summary>
        ///     [channel] :End of NAMES list
        /// </summary>
        public const string NAME_REPLY_END = "366";

        /// <summary>
        ///     [mask] [server] :[hopcount] [server info]
        /// </summary>
        public const string LINKS_REPLY = "364";

        /// <summary>
        ///     [mask] :End of LINKS list
        /// </summary>
        public const string LINKS_REPLY_END = "365";

        /// <summary>
        ///     [channel] [banmask]
        /// </summary>
        public const string BAN_LIST_REPLY = "367";

        /// <summary>
        ///     [channel] :End of channel ban list
        /// </summary>
        public const string BAN_LIST_REPLY_END = "368";

        /// <summary>
        ///     :[const string]
        /// </summary>
        public const string INFO_REPLY = "371";

        /// <summary>
        ///     :End of INFO list
        /// </summary>
        public const string INFO_REPLY_END = "374";

        /// <summary>
        ///     :- [server] Args of the day -
        /// </summary>
        public const string MOTD_START = "375";

        /// <summary>
        ///     :- [text]
        /// </summary>
        public const string MOTD_REPLY = "372";

        /// <summary>
        ///     :End of MOTD command
        /// </summary>
        public const string MOTD_REPLY_END = "376";

        /// <summary>
        ///     :[username] [ttyline] [hostname]
        /// </summary>
        public const string USERS_REPLY = "393";

        /// <summary>
        ///     :End of users
        /// </summary>
        public const string USERS_REPLY_END = "394";

        /// <summary>
        ///     :Nobody logged in
        /// </summary>
        public const string NO_USERS_REPLY = "395";

        /// <summary>
        ///     [linkname] [sendq] [sent messages] [sent Kbytes] [received messages] [received Kbytes] [time open]
        /// </summary>
        public const string STATS_LINK_INFO_REPLY = "211";

        /// <summary>
        ///     [command] [count] [byte count] [remote count]
        /// </summary>
        public const string STATS_COMMANDS_REPLY = "212";

        /// <summary>
        ///     [stats letter] :End of STATS report
        /// </summary>
        public const string STATS_REPLY_END = "219";

        /// <summary>
        ///     :Server Up %d days %d:%02d:%02d
        /// </summary>
        public const string STATS_UPTIME_REPLY = "242";

        /// <summary>
        ///     O [hostmask] * [name]
        /// </summary>
        public const string STATS_ONLINE = "243";

        /// <summary>
        ///     [user mode const string]
        /// </summary>
        public const string USER_MODE_IS_REPLY = "221";

        /// <summary>
        ///     [name] [server] [mask] [type] [hopcount] [info]
        /// </summary>
        public const string SERVER_LIST_REPLY = "234";

        /// <summary>
        ///     [mask] [type] :End of service listing
        /// </summary>
        public const string SERVER_LIST_REPLY_END = "235";

        /// <summary>
        ///     [command] :Please wait a while and try again.
        /// </summary>
        public const string TRY_AGAIN_REPLY = "263";

        /* Error responses */

        /// <summary>
        ///     [nickname] :No such nick/channel
        /// </summary>
        public const string ERROR_NO_SUCH_NICK = "401";

        /// <summary>
        ///     [server name] :No such server
        /// </summary>
        public const string ERROR_NO_SUCH_SERVER = "402";

        /// <summary>
        ///     [channel name] :No such channel
        /// </summary>
        public const string ERROR_NO_SUCH_CHANNEL = "403";

        /// <summary>
        ///     [channel name] :Cannot send to channel
        /// </summary>
        public const string ERROR_CANNOT_SEND_TO_CHAN = "404";

        /// <summary>
        ///     [channel name] :You have joined too many channels
        /// </summary>
        public const string ERROR_TOO_MANY_CHANNELS = "405";

        /// <summary>
        ///     [nickname] :There was no such nickname
        /// </summary>
        public const string ERROR_WAS_NO_SUCH_NICK = "406";

        /// <summary>
        ///     [target] :[error code] recipients. [abort message]
        /// </summary>
        public const string ERROR_TOO_MANY_TARGETS = "407";

        /// <summary>
        ///     [service name] :No such service
        /// </summary>
        public const string ERROR_NO_SUCH_SERVICE = "408";

        /// <summary>
        ///     :No origin specified - PING or PONG message missing originator parameter
        /// </summary>
        public const string ERROR_NO_ORIGIN = "409";

        /// <summary>
        ///     :No recipient given ([command])
        /// </summary>
        public const string ERROR_NO_RECIPIENT = "411";

        /// <summary>
        ///     :No text to send
        /// </summary>
        public const string ERROR_NO_TEXT_TO_SEND = "412";

        /// <summary>
        ///     [mask] :No toplevel domain specified
        /// </summary>
        public const string ERROR_NO_TOP_LEVEL = "413";

        /// <summary>
        ///     [mask] :Wildcard in toplevel domain
        /// </summary>
        public const string ERROR_WILD_TOP_LEVEL = "414";

        /// <summary>
        ///     [mask] :Bad Server/host mask
        /// </summary>
        public const string ERROR_BAD_MASK = "415";

        /// <summary>
        ///     [command] :Unknown command
        /// </summary>
        public const string ERROR_UNKNOWN_COMMAND = "421";

        /// <summary>
        ///     :MOTD File is missing
        /// </summary>
        public const string ERROR_NO_MOTD = "422";

        /// <summary>
        ///     [server] :No administrative info available
        /// </summary>
        public const string ERROR_NO_ADMIN_INFO = "423";

        /// <summary>
        ///     :File error doing [file op] on [file]
        /// </summary>
        public const string ERROR_FILE_ERROR = "424";

        /// <summary>
        ///     :No nickname given
        /// </summary>
        public const string ERROR_NO_NICKNAME_GIVEN = "431";

        /// <summary>
        ///     [nick] :Erroneous nickname
        /// </summary>
        public const string ERROR_ERREONEOUS_NICKNAME = "432";

        /// <summary>
        ///     [nick] :Nickname is already in use
        /// </summary>
        public const string ERROR_NICK_NAME_IN_USE = "433";

        /// <summary>
        ///     [nick] :Nickname collision KILL from [user]@[host]
        /// </summary>
        public const string ERROR_NICK_COLLISION = "436";

        /// <summary>
        ///     [nick/channel] :Nick/channel is temporarily unavailable
        /// </summary>
        public const string ERROR_UNAVAILABLE_RESOURCE = "437";

        /// <summary>
        ///     [nick] [channel] :They aren't on that channel
        /// </summary>
        public const string ERROR_USER_NOT_IN_CHANNEL = "437";

        /// <summary>
        ///     [channel] :You're not on that channel
        /// </summary>
        public const string ERROR_NOT_ON_CHANNEL = "442";

        /// <summary>
        ///     [user] [channel] :is already on channel
        /// </summary>
        public const string ERROR_USER_ON_CHANNEL = "443";

        /// <summary>
        ///     [user] :User not logged in
        /// </summary>
        public const string ERROR_NO_LOGIN = "444";

        /// <summary>
        ///     :USERS has been disabled
        /// </summary>
        public const string ERROR_USERS_DISABLED = "446";

        /// <summary>
        ///     :You have not registered
        /// </summary>
        public const string ERROR_NOT_REGISTERED = "451";

        /// <summary>
        ///     [command] :Not enough parameters
        /// </summary>
        public const string ERROR_NEED_MORE_PARAMS = "461";

        /// <summary>
        ///     :Unauthorized command (already registered)
        /// </summary>
        public const string ERROR_ALREADY_REGISTERED = "462";

        /// <summary>
        ///     :Your host isn't among the privileged
        /// </summary>
        public const string ERROR_NO_PERM_FOR_HOST = "463";

        /// <summary>
        ///     :Password incorrect
        /// </summary>
        public const string ERROR_PASSWORD_MISMATCH = "464";

        /// <summary>
        ///     :You are banned from this server
        /// </summary>
        public const string ERROR_YOU_ARE_BANNED = "465";

        /// <summary>
        ///     - Sent by a server to a user to inform that access to the server will soon be denied.
        /// </summary>
        public const string ERROR_YOU_WILL_BE_BANNED = "466";

        /// <summary>
        ///     [channel] :Channel key already set
        /// </summary>
        public const string ERROR_KEYSET = "467";

        /// <summary>
        ///     [channel] :Cannot join channel (+l)
        /// </summary>
        public const string ERROR_CHANNEL_IS_FULL = "471";

        /// <summary>
        ///     [char] :is unknown mode char to me for [channel]
        /// </summary>
        public const string ERROR_UNKNOWN_MODE = "472";

        /// <summary>
        ///     [channel] :Cannot join channel (+i)
        /// </summary>
        public const string ERROR_INVITE_ONLY_CHAN = "473";

        /// <summary>
        ///     [channel] :Cannot join channel (+b)
        /// </summary>
        public const string ERROR_BANNED_FROM_CHAN = "747";

        /// <summary>
        ///     [channel] :Cannot join channel (+k)
        /// </summary>
        public const string ERROR_BAD_CHANNEL_KEY = "475";

        /// <summary>
        ///     [channel] :Bad Channel Mask
        /// </summary>
        public const string ERROR_BAD_CHAN_MASK = "476";

        /// <summary>
        ///     [channel] :Channel doesn't support modes
        /// </summary>
        public const string ERROR_NO_CHAN_MODES = "477";

        /// <summary>
        ///     [channel] [char] :Channel list is full
        /// </summary>
        public const string ERROR_BAN_LIST_FULL = "478";

        /// <summary>
        ///     :Permission Denied- You're not an IRC operator
        /// </summary>
        public const string ERROR_NO_PRIVEGEDES = "481";

        /// <summary>
        ///     [channel] :You're not channel operator
        /// </summary>
        public const string ERROR_CHANOP_PRIVS_NEEDED = "482";

        /// <summary>
        ///     :You can't kill a server
        /// </summary>
        public const string ERROR_CANT_KILL_SERVER = "483";

        /// <summary>
        ///     :Your connection is restricted
        /// </summary>
        public const string ERROR_RESTRICTED = "484";

        /// <summary>
        ///     :You're not the original channel operator
        /// </summary>
        public const string ERROR_UNIQOP_PRIVS_NEEDED = "485";

        /// <summary>
        ///     :No O-lines for your host
        /// </summary>
        public const string ERROR_NO_OPER_HOST = "491";

        /// <summary>
        ///     :Unknown MODE flag
        /// </summary>
        public const string ERROR_UNKNOWN_MODE_FLAG = "501";

        /// <summary>
        ///     :Cannot change mode for other user
        /// </summary>
        public const string ERROR_USERS_DONT_MATCH = "502";
    }
}