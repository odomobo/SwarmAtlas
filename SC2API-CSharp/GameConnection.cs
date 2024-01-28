using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace SC2API.CSharp
{
    public class GameConnection
    {
        ProtobufProxy proxy = new ProtobufProxy();

        private readonly GameConfig _gameConfig;

        public GameConnection(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        public async Task Connect(int port)
        {
            for (int i = 0; i < 40; i++)
            {
                try
                {
                    await proxy.Connect(_gameConfig.Address, port);
                    return;
                }
                catch (WebSocketException) { }
                Thread.Sleep(2000);
            }
            throw new Exception("Unable to make a connection.");
        }

        public async Task CreateAiGame(string mapName, Race opponentRace, Difficulty opponentDifficulty)
        {
            RequestCreateGame createGame = new RequestCreateGame();
            createGame.Realtime = _gameConfig.Realtime;

            string mapPath = Path.Combine(_gameConfig.StarcraftDir, "Maps", mapName);
            if (!File.Exists(mapPath))
                throw new Exception("Could not find map at " + mapPath);
            createGame.LocalMap = new LocalMap();
            createGame.LocalMap.MapPath = mapPath;

            PlayerSetup player1 = new PlayerSetup();
            createGame.PlayerSetup.Add(player1);
            player1.Type = PlayerType.Participant;

            PlayerSetup player2 = new PlayerSetup();
            createGame.PlayerSetup.Add(player2);
            player2.Type = PlayerType.Computer;

            // these settings are only applicable to builtin AI
            player2.Race = opponentRace;
            player2.Difficulty = opponentDifficulty;

            Request request = new Request();
            request.CreateGame = createGame;
            Response response = await proxy.SendRequest(request);
        }

        public async Task CreateLadderGame(string mapName)
        {
            RequestCreateGame createGame = new RequestCreateGame();
            createGame.Realtime = _gameConfig.Realtime;

            string mapPath = Path.Combine(_gameConfig.StarcraftDir, "Maps", mapName);
            if (!File.Exists(mapPath))
                throw new Exception("Could not find map at " + mapPath);
            createGame.LocalMap = new LocalMap();
            createGame.LocalMap.MapPath = mapPath;

            PlayerSetup player1 = new PlayerSetup();
            createGame.PlayerSetup.Add(player1);
            player1.Type = PlayerType.Participant;

            PlayerSetup player2 = new PlayerSetup();
            createGame.PlayerSetup.Add(player2);
            player2.Type = PlayerType.Participant;

            Request request = new Request();
            request.CreateGame = createGame;
            Response response = await proxy.SendRequest(request);
        }

        public async Task<uint> JoinGame(Race race, string playerName)
        {
            RequestJoinGame joinGame = new RequestJoinGame();
            joinGame.Race = race;
            joinGame.PlayerName = playerName;

            joinGame.Options = new InterfaceOptions();
            joinGame.Options.Raw = true;
            joinGame.Options.Score = true;

            Request request = new Request();
            request.JoinGame = joinGame;
            Response response = await proxy.SendRequest(request);
            return response.JoinGame.PlayerId;
        }

        public async Task<uint> JoinGameLadder(Race race, string playerName, int startPort)
        {
            RequestJoinGame joinGame = new RequestJoinGame();
            joinGame.Race = race;
            joinGame.PlayerName = playerName;

            joinGame.SharedPort = startPort + 1;
            joinGame.ServerPorts = new PortSet();
            joinGame.ServerPorts.GamePort = startPort + 2;
            joinGame.ServerPorts.BasePort = startPort + 3;

            joinGame.ClientPorts.Add(new PortSet());
            joinGame.ClientPorts[0].GamePort = startPort + 4;
            joinGame.ClientPorts[0].BasePort = startPort + 5;

            joinGame.Options = new InterfaceOptions();
            joinGame.Options.Raw = true;
            joinGame.Options.ShowCloaked = true;
            joinGame.Options.Score = true;

            Request request = new Request();
            request.JoinGame = joinGame;

            Response response = await proxy.SendRequest(request);
            return response.JoinGame.PlayerId;
        }

        public async Task<ResponsePing> Ping()
        {
            Request request = new Request();
            request.Ping = new RequestPing();
            Response response = await proxy.SendRequest(request);
            return response.Ping;
        }

        public async Task RequestLeaveGame()
        {
            Request requestLeaveGame = new Request();
            requestLeaveGame.LeaveGame = new RequestLeaveGame();
            await proxy.SendRequest(requestLeaveGame);
        }

        public async Task SendRequest(Request request)
        {
            await proxy.SendRequest(request);
        }

        public async Task<ResponseQuery> SendQuery(RequestQuery query)
        {
            Request request = new Request();
            request.Query = query;
            Response response = await proxy.SendRequest(request);
            return response.Query;
        }

        public async Task SaveReplay()
        {
            var request = new Request();
            request.SaveReplay = new RequestSaveReplay();
            var response = await proxy.SendRequest(request);
        }

        public async Task Run(IBot bot, uint playerId, string opponentID)
        {
            await bot.Run(proxy, playerId, opponentID);
        }

        public async Task StartReplay(string replayFilename, uint playerId)
        {
            var request = new Request();
            request.StartReplay = new RequestStartReplay();
            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            request.StartReplay.ReplayPath = Path.Combine(myDocuments, "Starcraft II", "Replays", "Multiplayer", replayFilename);
            request.StartReplay.ObservedPlayerId = (int)playerId; // bot is always 2, I think?
            var options = new InterfaceOptions();
            options.Raw = true;
            options.ShowCloaked = true;
            options.Score = true;
            request.StartReplay.Options = options;
            request.StartReplay.Realtime = false;

            var response = await proxy.SendRequest(request);
        }

        public async Task ProcessReplay(IBot bot, uint playerId)
        {
            await bot.ProcessReplay(proxy, playerId);
        }
    }
}
