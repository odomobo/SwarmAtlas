using System.Collections.Generic;
using SC2APIProtocol;
using System;

using Action = SC2APIProtocol.Action;
using System.Linq;
using LiteDB;
using SC2API.CSharp;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;

namespace ExampleBot
{
    class UnitData
    {
        public List<Unit> Units { get; set; }
    }
    class ExampleBot : IBot
    {
        private readonly GameConfig _gameConfig;
        internal ExampleBot(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        public string BotName => "ExampleBot";

        private LiteDatabase _liteDb = null;
        private InnerBot _innerBot = null;

        public async Task Run(ProtobufProxy proxy, uint playerId, string opponentID)
        {
            int observationsBytesSoFar = 0;
            _innerBot = new InnerBot();
            var dbFilename = GenerateDbFilename();
            OpenDb(dbFilename);
            var botStates = _liteDb.GetCollection<InnerBot>("botStates");
            botStates.EnsureIndex(x => x.LastStepId);

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

            var (dataResponse, dataResponseBuf) = await proxy.SendRequestRaw(gameDataRequest);
            dataResponse = proxy.GetResponseFromResponseBuf(dataResponseBuf);

            ResponsePing pingResponse = await Ping(proxy);

            bool start = true;

            while (true)
            {
                Request observationRequest = new Request();
                observationRequest.Observation = new RequestObservation();
                var (response, responseBuf) = await proxy.SendRequestRaw(observationRequest);
                observationsBytesSoFar += responseBuf.Length;
                Console.WriteLine($"Observations bytes so far: {observationsBytesSoFar}");

                ResponseObservation observation = response.Observation;

                if (observation == null)
                {
                    CloseDb();
                    _innerBot.OnEnd(observation, Result.Unset);
                    break;
                }
                if (response.Status == Status.Ended || response.Status == Status.Quit)
                {
                    CloseDb();
                    _innerBot.OnEnd(observation, observation.PlayerResult[(int)playerId - 1].Result);
                    break;
                }

                List<Action> actions = new List<Action>();

                if (start)
                {
                    start = false;
                    _innerBot.OnStart(gameInfoResponse.GameInfo, dataResponse.Data, pingResponse, observation, playerId, opponentID);
                    //actions.Add(SendDbInfoChat(dbFilename));
                }

                // if same step ID as last time, then we need to skip processing
                var stepId = observation.Observation.GameLoop;
                if (stepId != _innerBot.LastStepId || true) // TODO; get rid of "true" condition
                {
                    // let's see what happens if this gets sent on step 6... I imagine it'll show on step 8
                    if (stepId == 6)
                    {
                        actions.Add(SendDbInfoChat(dbFilename));
                    }

                    var botActions = _innerBot.OnFrame(observation);
                    actions.AddRange(botActions);
                    botStates.Insert(_innerBot);
                    _liteDb.Commit();
                    // TODO: uncomment this:
                    _innerBot = botStates.FindOne(x => x.LastStepId == _innerBot.LastStepId);

                    var dbFilenameFromChat = GetDbInfoFromChat(observation, playerId);
                    if (dbFilenameFromChat != null)
                    {
                        Console.WriteLine($"Got db filename chat message on step {stepId}");
                    }
                }
                else
                {
                    Console.WriteLine($"Skipping duplicate step {stepId}");

                    var dbFilenameFromChat = GetDbInfoFromChat(observation, playerId);
                    if (dbFilenameFromChat != null)
                    {
                        Console.WriteLine($"Got db filename chat message on step {stepId}");
                    }
                }

                Request actionRequest = new Request();
                actionRequest.Action = new RequestAction();
                actionRequest.Action.Actions.AddRange(actions);
                if (actionRequest.Action.Actions.Count > 0)
                    await proxy.SendRequest(actionRequest);

                Request stepRequest = new Request();
                stepRequest.Step = new RequestStep();
                stepRequest.Step.Count = 1;
                await proxy.SendRequest(stepRequest);

                Thread.Sleep(100);
            }
        }

        private string GenerateDbFilename()
        {
            return $"match {DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db";
        }

        public async Task ProcessReplay(ProtobufProxy proxy, uint playerId)
        {
            int observationsBytesSoFar = 0;

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

            var (dataResponse, dataResponseBuf) = await proxy.SendRequestRaw(gameDataRequest);
            dataResponse = proxy.GetResponseFromResponseBuf(dataResponseBuf);

            var units = new Units(dataResponse.Data.Units);

            ResponsePing pingResponse = await Ping(proxy);

            bool start = true;

            while (true)
            {
                Request observationRequest = new Request();
                observationRequest.Observation = new RequestObservation();
                var (response, responseBuf) = await proxy.SendRequestRaw(observationRequest);
                observationsBytesSoFar += responseBuf.Length;
                Console.WriteLine($"Observations bytes so far: {observationsBytesSoFar}");

                ResponseObservation observation = response.Observation;

                if (observation == null)
                {
                    CloseDb();
                    // we don't need to set this state
                    //if (_innerBot != null)
                    //    _innerBot.OnEnd(observation, Result.Unset);
                    Console.WriteLine("observation null");
                    break;
                }
                if (response.Status == Status.Ended || response.Status == Status.Quit)
                {
                    CloseDb();
                    // we don't need to set this state
                    //if (_innerBot != null)
                    //    _innerBot.OnEnd(observation, observation.PlayerResult[(int)playerId - 1].Result);
                    Console.WriteLine($"observation ended or quit: {response.Status}");
                    break;
                }

                OpenDbFromChat(observation, playerId);

                if (start)
                {
                    start = false;
                    // we don't need to set this state
                    //if (_innerBot != null)
                    //    _innerBot.OnStart(gameInfoResponse.GameInfo, dataResponse.Data, pingResponse, observation, playerId, opponentID);
                    Console.WriteLine("starting");
                }

                var stepId = observation.Observation.GameLoop;
                if (_liteDb != null && stepId > 0)
                {
                    var botStates = _liteDb.GetCollection<InnerBot>("botStates");
                    var lastStepId = stepId - 1;
                    _innerBot = botStates.FindOne(x => x.LastStepId == lastStepId);
                }

                List<Action> actions;
                List<DebugCommand> debugCommands = new List<DebugCommand>();
                if (_innerBot != null)
                {
                    actions = _innerBot.OnFrame(observation);
                }
                else
                    Console.WriteLine($"Skipping frame {observation.Observation.GameLoop}; waiting for DB message");

                // don't want to reuse next frame
                _innerBot = null;

                // can't send in replay mode
                //Request actionRequest = new Request();
                //actionRequest.Action = new RequestAction();
                //actionRequest.Action.Actions.AddRange(actions);
                //if (actionRequest.Action.Actions.Count > 0)
                //    await proxy.SendRequest(actionRequest);

                Request stepRequest = new Request();
                stepRequest.Step = new RequestStep();
                stepRequest.Step.Count = 1;
                await proxy.SendRequest(stepRequest);

                Thread.Sleep(45); // TODO: make more sophisticated
            }
        }

        public async Task<ResponsePing> Ping(ProtobufProxy proxy)
        {
            Request request = new Request();
            request.Ping = new RequestPing();
            Response response = await proxy.SendRequest(request);
            return response.Ping;
        }

        private void OpenDb(string dbFilename)
        {
            _liteDb = new LiteDatabase(Path.Combine(_gameConfig.ReplayPath, dbFilename));
        }

        private void CloseDb()
        {
            if (_liteDb != null)
            {
                _liteDb.Dispose();
                _liteDb = null;
            }
        }

        private Action SendDbInfoChat(string dbFilename)
        {
            var chat = new ActionChat();
            chat.Channel = ActionChat.Types.Channel.Broadcast; // TODO: should be team
            chat.Message = $"db:{dbFilename}";
            return new Action { ActionChat = chat };
        }

        private static readonly Regex ChatDbRegex = new Regex(@"^db:(.*)$");
        private string GetDbInfoFromChat(ResponseObservation observation, uint playerId)
        {
            foreach (var chat in observation.Chat)
            {
                if (chat.PlayerId != playerId)
                    continue;

                var match = ChatDbRegex.Match(chat.Message);

                if (!match.Success)
                    continue;

                return match.Groups[1].Value;
            }
            return null;
        }

        private void OpenDbFromChat(ResponseObservation observation, uint playerId)
        {
            // skip if we already got the DB info
            if (_liteDb != null)
                return;

            var dbFilename = GetDbInfoFromChat(observation, playerId);
            if (dbFilename != null)
                OpenDb(dbFilename);
        }
    }
}
