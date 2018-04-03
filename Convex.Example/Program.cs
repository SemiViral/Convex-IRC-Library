#region usings

using System;
using System.Threading.Tasks;

#endregion

namespace Convex.Example {
    internal static class Program {
        private static IrcBot _bot;

        private static async Task DebugRun() {
            do {
                await _bot.Execute();
            } while (_bot.Executing);

            _bot.Dispose();
        }

        private static async Task InitialiseAndExecute() {
            using (_bot = new IrcBot()) {
                await _bot.Initialise();
                await DebugRun();
            }
        }

        private static void Main() {
            InitialiseAndExecute()
                .Wait();

            Console.ReadLine();
        }
    }
}