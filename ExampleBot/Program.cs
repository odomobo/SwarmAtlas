﻿using SC2API.CSharp;
using SC2APIProtocol;

namespace ExampleBot
{
    public class Program
    {
        // Settings for your bot.
        private static Bot bot = new Bot();
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
            //var gameConnection = new GameConnection(gameConfig);
            var exeLauncher = new ExeLauncher(gameConfig);
            var gameLauncher = new GameLauncher(exeLauncher, gameConfig);

            if (args.Length == 0)
            {
                //gameLauncher.RunSinglePlayer(bot, mapName, race, opponentRace, opponentDifficulty).Wait();
                gameLauncher.RunVsHuman(bot, mapName, botRace, 5678, 6000, Race.Terran, "Human").Wait();
            }
            else
            {
                gameLauncher.RunLadder(bot, botRace, args).Wait();
            }
        }
    }
}
