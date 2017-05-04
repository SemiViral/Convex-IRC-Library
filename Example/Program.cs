#region usings

using System;
using System.ComponentModel;
using System.Threading.Tasks;

#endregion

namespace Example {
    internal class Program {
        public static IrcBot IrcBot;

        //private static void ParseAndDo(object sender, DoWorkEventArgs e) {
        //    while (IrcBot.CanExecute) {
        //        string input = Console.ReadLine();
        //    }
        //}

        //private static void NonDebugRun() {
        //    string input = string.Empty;

        //    BackgroundWorker backgroundWorker = new BackgroundWorker();
        //    backgroundWorker.DoWork += ParseAndDo;
        //    backgroundWorker.RunWorkerAsync();

        //    do {
        //        input = Console.ReadLine();
        //    } while (!string.IsNullOrEmpty(input) &&
        //             !input.ToLower().Equals("exit"));
        //}

        private static async Task DebugRun() {
            do {
                await IrcBot.Execute();
            } while (IrcBot.Executing);

            IrcBot.Dispose();
        }

        private static async Task InitialiseAndExecute() {
            using (IrcBot = new IrcBot()) {
                await IrcBot.Initialise();
                
#if DEBUG
                await DebugRun();
#else
				NonDebugRun();
#endif
            }
        }

        private static void Main() {
            InitialiseAndExecute()
                .Wait();

            Console.ReadLine();
        }
    }
}