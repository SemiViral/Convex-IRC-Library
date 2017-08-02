#region usings

using System;
using System.Threading.Tasks;

#endregion

namespace Convex.Example {
    internal class Program {
        private static IrcBot bot;

        private static async Task DebugRun() {
            do {
                await bot.Execute();
            } while (bot.Executing);

            bot.Dispose();
        }

        private static async Task InitialiseAndExecute() {
            using (bot = new IrcBot()) {
                await bot.Initialise();
                await DebugRun();
            }
        }

        private static void Main() {
            InitialiseAndExecute()
                .Wait();
        }
    }
}