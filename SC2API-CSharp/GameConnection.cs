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

        public async Task Run(Bot bot, uint playerId, string opponentID)
        {
            Request gameInfoReq = new Request();
            gameInfoReq.GameInfo = new RequestGameInfo();

            Response gameInfoResponse = await proxy.SendRequest(gameInfoReq);

            Request gameDataRequest = new Request();
            gameDataRequest.Data = new RequestData();
            gameDataRequest.Data.UnitTypeId = true;
            gameDataRequest.Data.AbilityId = true;
            gameDataRequest.Data.BuffId = true;
            gameDataRequest.Data.EffectId = true;
            gameDataRequest.Data.UpgradeId = true;

            Response dataResponse = await proxy.SendRequest(gameDataRequest);

            ResponsePing pingResponse = await Ping();

            bool start = true;

            while (true)
            {
                Request observationRequest = new Request();
                observationRequest.Observation = new RequestObservation();
                Response response = await proxy.SendRequest(observationRequest);

                ResponseObservation observation = response.Observation;

                if (observation == null)
                {
                    bot.OnEnd(observation, Result.Unset);
                    break;
                }
                if (response.Status == Status.Ended || response.Status == Status.Quit)
                {
                    bot.OnEnd(observation, observation.PlayerResult[(int)playerId - 1].Result);
                    break;
                }

                if (start)
                {
                    start = false;
                    bot.OnStart(gameInfoResponse.GameInfo, dataResponse.Data, pingResponse, observation, playerId, opponentID);
                }

                IEnumerable<SC2APIProtocol.Action> actions = bot.OnFrame(observation);

                Request actionRequest = new Request();
                actionRequest.Action = new RequestAction();
                actionRequest.Action.Actions.AddRange(actions);
                if (actionRequest.Action.Actions.Count > 0)
                    await proxy.SendRequest(actionRequest);

                Request stepRequest = new Request();
                stepRequest.Step = new RequestStep();
                stepRequest.Step.Count = 1;
                await proxy.SendRequest(stepRequest);
            }
        }
    }
}
