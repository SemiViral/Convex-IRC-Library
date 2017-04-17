namespace Convex.Types.References {
    public static class Modes {
        // Several modes mean different things on
        // seperate servers, so this may require some
        // tinkering when connecting to a different server.
        //
        // You can probably find your server's MODE specs by
        // Googling them or asking the admin for documentation.
        //
        // Eve will probably only use general mode specs,
        // +v, +o, etc. that are the same across most servers.
        // 
        // Notes on mode
        // =============
        //
        // A lot of general and important modes are missing.
        // I'll add them... eventually.
        //
        // Mode functionality beyond per-basis user recognition 
        // isn't very important.
        //
        // todo add modes that are missing

        public static IrcMode Ban = new IrcMode('b', "MODE %c +b %n!%i@%h");

        public static IrcMode BanException = new IrcMode('e', "MODE %c +e %n!%i@%h");

        public static IrcMode NoColor = new IrcMode('c', "MODE %c +e");

        public static IrcMode FloodLimit = new IrcMode('f', "MODE %c +e %p");

        public static IrcMode JoinThrottle = new IrcMode('j', "MODE %c +e %p:%p2");

        // Sets limit to number of users in a channel
        public static IrcMode Limit = new IrcMode('l', "MODE %c +e %p");

        public static IrcMode ModeratedChannel = new IrcMode('m', "MODE %c +e");

        // Considered obsolete, alt: SECRET
        public static IrcMode Private = new IrcMode('p', "MODE %c +p");

        public static IrcMode Secret = new IrcMode('s', "MODE %c +s");

        public static IrcMode SecuredOnly = new IrcMode('z', "MODE %channel +z");

        public static IrcMode NoCtcp = new IrcMode('c', "MODE %channel +C");

        public static IrcMode StripBadWords = new IrcMode('G', "MODE %c +G");

        // Allows user to talk in +M channels
        public static IrcMode Voice = new IrcMode('v', "MODE %c +v %n");

        // Blocks users from using /KNOCK to to try and acces a keyword locked channel
        public static IrcMode NoKnock = new IrcMode('K', "MODE %c +K");

        public static IrcMode NoNickChange = new IrcMode('N', "MODE %c +N");

        public static IrcMode NoKicks = new IrcMode('Q', "MODE %c +Q");

        // Only registered users may join channel
        public static IrcMode RegOnly = new IrcMode('R', "MODE %c +R");

        // This channel mode will remove all client color codes from messages in your channel.
        public static IrcMode Strip = new IrcMode('S', "MODE %c +S");

        // This blocks users from sending NOTICE's to the channel
        public static IrcMode NoNotice = new IrcMode('T', "MODE %c +T");

        // This mode prevents users from sending channel invites to users outside the channel.
        public static IrcMode NoInvites = new IrcMode('V', "MODE %c +VarManagement");
    }

    public class IrcMode {
        public IrcMode(char identifier, string syntax) {
            Identifier = identifier;
            Syntax = syntax;
        }

        public char Identifier { get; }
        // public string Translation { get; }
        public string Syntax { get; }
    }
}