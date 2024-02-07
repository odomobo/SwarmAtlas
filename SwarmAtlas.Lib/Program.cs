using Autofac;
using NLog;
using SC2API.CSharp;
using SC2APIProtocol;

namespace SwarmAtlas.Lib
{
    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Settings for your bot. Maybe put this elsewhere
        private const Race botRace = Race.Zerg;

        // Settings for single player mode.
        private const string mapName = @"BlackburnAIE.SC2Map";
        private const LaunchMode Mode = LaunchMode.Simulate;
        private const string ReplayDbFilename = @"match 2024-02-01_16-23-53.db";

        private enum LaunchMode
        {
            Run,
            Simulate,
        }

        /* The main entry point for the bot.
         * This will start the Stacraft 2 instance and connect to it.
         * The program can run in single player mode against the standard Blizzard AI, or it can be run against other bots through the ladder.
         */
        public static void Run(string[] args)
        {
            //IConfigurationRoot config = new ConfigurationBuilder()
            //    .AddJsonFile("appsettings.json")
            //    .AddEnvironmentVariables()
            //    .Build();

            Logger.Info("Starting application");

            //var renderer = new Renderer();
            //var scene = new Scene();
            //scene.MapObjects.Add(new CenteredSquare(new Vector2(50, 2), 1, Microsoft.Xna.Framework.Color.Purple, 0.1f));
            //renderer.UpdateScene(scene);
            //
            //Thread.Sleep(60000);
            //renderer.Stop();
            //
            //return;

            var container = AutofacRegistration.BuildContainer(args);
            using (var scope = container.BeginLifetimeScope())
            {
                var bot = scope.Resolve<SwarmAtlasRunner>();
                var gameLauncher = scope.Resolve<GameLauncher>();

                if (args.Length == 0)
                {
                    //gameLauncher.RunSinglePlayer(bot, mapName, botRace, 5678, opponentRace, opponentDifficulty).Wait();
                    switch (Mode)
                    {
                        case LaunchMode.Run:
                            RunVsHuman(bot, gameLauncher);
                            break;
                        case LaunchMode.Simulate:
                            SimulateBot(bot, ReplayDbFilename);
                            break;
                    }
                }
                else
                {
                    var commandLineArgs = new CommandLineArgs(args);
                    gameLauncher.RunLadder(bot, botRace, commandLineArgs.GamePort, commandLineArgs.StartPort, commandLineArgs.OpponentID).Wait();
                }
            }
        }

        private static void RunVsHuman(IBot bot, GameLauncher gameLauncher)
        {
            Logger.Info("Running game with bot against human");
            gameLauncher.RunVsHuman(bot, mapName, botRace, 5678, 5679, 6000, Race.Terran, "Human").Wait();
        }

        private static void SimulateBot(SwarmAtlasRunner bot, string replayDbFilename)
        {
            Logger.Info($"Simulating replay for \"{replayDbFilename}\"");
            var proxy = new ProtobufProxy(); // this doesn't actually have to be connected; only used for parsing buffers
            bot.Simulate(proxy, replayDbFilename);
        }
    }
}
