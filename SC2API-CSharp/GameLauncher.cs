using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Google.Protobuf.Reflection.UninterpretedOption.Types;

namespace SC2API.CSharp
{
    /// <summary>
    /// Abstraction on top of GameConnection
    /// </summary>
    public class GameLauncher
    {
        private readonly GameConfig _config;
        private readonly ExeLauncher _exeLauncher;

        public GameLauncher(ExeLauncher exeLauncher, GameConfig config)
        {
            _exeLauncher = exeLauncher;
            _config = config;
        }

        public async Task RunSinglePlayer(Bot bot, string map, Race myRace, int gamePort, Race opponentRace, Difficulty opponentDifficulty)
        {
            var connection = new GameConnection(_config);
            _exeLauncher.StartSC2Instance(gamePort);
            await connection.Connect(gamePort);
            await connection.CreateAiGame(map, opponentRace, opponentDifficulty);
            uint playerId = await connection.JoinGame(myRace, bot.BotName);
            await connection.Run(bot, playerId, null);
        }

        public async Task CreateAndRunLadder(Bot bot, string map, Race myRace, int gamePort, int startPort, string opponentID, ManualResetEvent ladderStarted)
        {
            _exeLauncher.StartSC2Instance(gamePort);
            var connection = new GameConnection(_config);
            await connection.Connect(gamePort);
            await connection.CreateLadderGame(map);
            ladderStarted.Set();
            uint playerId = await connection.JoinGameLadder(myRace, bot.BotName, startPort);
            await connection.Run(bot, playerId, opponentID);
        }

        public async Task RunLadder(Bot bot, Race myRace, int gamePort, int startPort, string opponentID)
        {
            var connection = new GameConnection(_config);
            await connection.Connect(gamePort);
            uint playerId = await connection.JoinGameLadder(myRace, bot.BotName, startPort);
            await connection.Run(bot, playerId, opponentID);
        }

        // doesn't seem to work
        public async Task RunVsHumanSimple(Bot bot, string map, Race botRace, int gamePortHuman, int gamePortBot, int startPort, Race humanRace, string humanPlayerName)
        {
            var ladderStarted = new ManualResetEvent(false);
            var humanProxyBot = new HumanProxyBot();
            var humanTask = CreateAndRunLadder(humanProxyBot, map, humanRace, gamePortHuman, startPort, null, ladderStarted);

            ladderStarted.WaitOne();

            var botTask = RunLadder(bot, botRace, gamePortBot, startPort, null);

            await Task.WhenAll(botTask, humanTask);
        }

        public async Task JoinAndRun(GameConnection connection, Bot bot, Race race, string name, int startPort, string opponentId)
        {
            uint playerId = await connection.JoinGameLadder(race, name, startPort);
            await connection.Run(bot, playerId, opponentId);
        }

        public async Task RunVsHuman(Bot bot, string map, Race botRace, int gamePortHuman, int gamePortBot, int startPort, Race humanRace, string humanPlayerName)
        {
            // steps:
            // 1. Launch human SC2 instance
            // 2. Connect human sc2 instance
            // 3. Create a ladder game
            // 4. Launch bot sc2 instance
            // 5. Connect bot sc2 instance
            // 6. join & run human
            // 7. join & run bot

            // 1
            var humanProxyBot = new HumanProxyBot();
            _exeLauncher.StartSC2Instance(gamePortHuman, false); // true if you want fs
            
            // 2
            var humanConnection = new GameConnection(_config);
            await humanConnection.Connect(gamePortHuman);

            // 3.
            await humanConnection.CreateLadderGame(map);

            // 4
            _exeLauncher.StartSC2Instance(gamePortBot);

            // 5
            var botConnection = new GameConnection(_config);
            await botConnection.Connect(gamePortBot);

            // 6
            var humanTask = JoinAndRun(humanConnection, humanProxyBot, humanRace, humanPlayerName, startPort, null);

            // 7
            var botTask = JoinAndRun(botConnection, bot, botRace, bot.BotName, startPort, null);

            await Task.WhenAll(botTask, humanTask);
        }

        public async Task RunLadder(Bot bot, Race myRace, CommandLineArgs commandLineArgs)
        {
            await RunLadder(bot, myRace, commandLineArgs.GamePort, commandLineArgs.StartPort, commandLineArgs.OpponentID);
        }
    }
}
