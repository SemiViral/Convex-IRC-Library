#region usings

using System;
using System.ComponentModel;

#endregion

namespace Example {
    internal class Program {
        public static IrcBot IrcBot;

        private static void ParseAndDo(object sender, DoWorkEventArgs e) {
            while (IrcBot.CanExecute) {
                string input = Console.ReadLine();
            }
        }

        private static void NonDebugRun() {
            string input = string.Empty;

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += ParseAndDo;
            backgroundWorker.RunWorkerAsync();

            do {
                input = Console.ReadLine();
            } while (!string.IsNullOrEmpty(input) &&
                     !input.ToLower().Equals("exit"));
        }

        private static void DebugRun() {
            while (IrcBot.CanExecute) {
                string input = Console.ReadLine();
            }

            IrcBot.Dispose();
        }

        private static void ExecuteRuntime() {
            using (IrcBot = new IrcBot()) {
#if DEBUG
                DebugRun();
#else
				NonDebugRun();
#endif
            }
        }

        private static void Main() {
            ExecuteRuntime();
        }
    }
}