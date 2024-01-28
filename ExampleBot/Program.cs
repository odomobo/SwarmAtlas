using SC2API.CSharp;
using SC2APIProtocol;

namespace ExampleBot
{
    public class Program
    {
        // Settings for your bot.
        private static Race botRace = Race.Zerg;

        // Settings for single player mode.
        private static string mapName = @"BlackburnAIE.SC2Map";
        private static Race opponentRace = Race.Random;
        private static Difficulty opponentDifficulty = Difficulty.VeryEasy;

        /* The main entry point for the bot.
         * This will start the Stacraft 2 instance and connect to it.
         * The program can run in single player mode against the standard Blizzard AI, or it can be run against other bots through the ladder.
         */
        public static void Run(string[] args)
        {
            var gameConfig = new GameConfig();
            var bot = new ExampleBot(gameConfig);
            var exeLauncher = new ExeLauncher(gameConfig);
            var gameLauncher = new GameLauncher(exeLauncher, gameConfig);

            if (args.Length == 0)
            {
                //gameLauncher.RunSinglePlayer(bot, mapName, botRace, 5678, opponentRace, opponentDifficulty).Wait();
                switch (1)
                {
                    case 1:
                        RunVsHuman(bot, gameLauncher);
                        break;
                    case 2:
                        ProcessReplay(bot);
                        break;
                }
            }
            else
            {
                var commandLineArgs = new CommandLineArgs(args);
                gameLauncher.RunLadder(bot, botRace, commandLineArgs.GamePort, commandLineArgs.StartPort, commandLineArgs.OpponentID).Wait();
            }
        }

        private static void RunVsHuman(IBot bot, GameLauncher gameLauncher)
        {
            gameLauncher.RunVsHuman(bot, mapName, botRace, 5678, 5679, 6000, Race.Terran, "Human").Wait();
        }

        private static void ProcessReplay(IBot bot)
        {
            var gameConfig = new GameConfig();
            gameConfig.Realtime = false; // we want to process replays fast... I think
            var exeLauncher = new ExeLauncher(gameConfig);
            var gameLauncher = new GameLauncher(exeLauncher, gameConfig);

            gameLauncher.ProcessReplay(bot, 2, 5678, @"Blackburn AIE (85).SC2Replay").Wait();
        }
    }
}
