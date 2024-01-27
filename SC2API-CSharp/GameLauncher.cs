using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        public async Task RunSinglePlayer(Bot bot, string map, Race myRace, Race opponentRace, Difficulty opponentDifficulty)
        {
            var connection = new GameConnection(_config);
            _exeLauncher.StartSC2Instance(_config.GamePort);
            await connection.Connect(_config.GamePort);
            await connection.CreateAiGame(map, opponentRace, opponentDifficulty);
            uint playerId = await connection.JoinGame(myRace, bot.BotName);
            await connection.Run(bot, playerId, null);
        }

        public async Task RunLadder(Bot bot, Race myRace, int gamePort, int startPort, string opponentID)
        {
            var connection = new GameConnection(_config);
            await connection.Connect(gamePort);
            uint playerId = await connection.JoinGameLadder(myRace, bot.BotName, startPort);
            await connection.Run(bot, playerId, opponentID);
        }

        public async Task RunVsHuman(Bot bot, string map, Race botRace, int gamePort, int startPort, Race humanRace, string humanPlayerName)
        {
            // TODO: figure out how to make this work. Maybe we need 2 sc2 instances? How do they talk to each other?
            _exeLauncher.StartSC2Instance(_config.GamePort);
            var humanConnection = new GameConnection(_config);
            await humanConnection.Connect(gamePort);
            await humanConnection.CreateLadderGame(map);


            _exeLauncher.StartSC2Instance(9999);
            var botConnection = new GameConnection(_config);
            await botConnection.Connect(9999);

            // maybe not doing this in the right order? I'm not really sure.......
            uint humanPlayerId = await humanConnection.JoinGameLadder(humanRace, humanPlayerName, startPort);
            uint botPlayerId = await botConnection.JoinGameLadder(botRace, bot.BotName, startPort);
            
            await botConnection.Run(bot, botPlayerId, null);
        }

        public async Task RunLadder(Bot bot, Race myRace, string[] args)
        {
            _config.ParseClArgs(args);
            await RunLadder(bot, myRace, _config.GamePort, _config.StartPort, _config.OpponentID);
        }
    }
}
