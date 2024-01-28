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

namespace ExampleBot
{
    class UnitData
    {
        public List<Unit> Units { get; set; }
    }
    class ExampleBot : IBot
    {
        public string BotName => "ExampleBot";

        private LiteDatabase _liteDb;

        public void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentID)
        {

            Console.WriteLine($"PlayerID {playerId}");
            
            Console.WriteLine($"Unit 0 type: {observation.Observation.RawData.Units[0].UnitType}");
            Console.WriteLine($"Unit 0 owner: {observation.Observation.RawData.Units[0].Owner}");
        }
    
        public List<Action> OnFrame(ResponseObservation observation)
        {
            List<Action> actions = new List<Action>();

            return actions;
        }
        
        public void OnEnd(ResponseObservation observation, Result result)
        {
        }

        public async Task<ResponsePing> Ping(ProtobufProxy proxy)
        {
            Request request = new Request();
            request.Ping = new RequestPing();
            Response response = await proxy.SendRequest(request);
            return response.Ping;
        }

        private Action SendDbInfoChat()
        {
            var chat = new ActionChat();
            chat.Channel = ActionChat.Types.Channel.Broadcast; // TODO: should be team
            chat.Message = "db:Human vs ExampleBot on Blackburn AIE at 2024-01-28_00_09_23";
            return new Action { ActionChat = chat };
        }

        private static readonly Regex ChatDbRegex = new Regex(@"^db:(.*)$");
        private void GetDbInfoFromChat(ResponseObservation observation, uint playerId)
        {
            // TODO: skip if we already got the DB info
            foreach (var chat in observation.Chat)
            {
                if (chat.PlayerId != playerId)
                    continue;

                var match = ChatDbRegex.Match(chat.Message);

                if (!match.Success)
                    continue;

                var dbFilename = match.Groups[1].Value;
                Console.WriteLine($"Got DB filename: {dbFilename}");
            }
        }

        public async Task Run(ProtobufProxy proxy, uint playerId, string opponentID)
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

            ResponsePing pingResponse = await Ping(proxy);

            bool start = true;

            while (true)
            {
                Request observationRequest = new Request();
                observationRequest.Observation = new RequestObservation();
                Response response = await proxy.SendRequest(observationRequest);

                ResponseObservation observation = response.Observation;

                if (observation == null)
                {
                    OnEnd(observation, Result.Unset);
                    break;
                }
                if (response.Status == Status.Ended || response.Status == Status.Quit)
                {
                    OnEnd(observation, observation.PlayerResult[(int)playerId - 1].Result);
                    break;
                }

                List<Action> actions = new List<Action>();

                if (start)
                {
                    start = false;
                    OnStart(gameInfoResponse.GameInfo, dataResponse.Data, pingResponse, observation, playerId, opponentID);
                    actions.Add(SendDbInfoChat());
                }

                actions.AddRange(OnFrame(observation));

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

        public async Task ProcessReplay(ProtobufProxy proxy, uint playerId)
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

            ResponsePing pingResponse = await Ping(proxy);

            bool start = true;

            while (true)
            {
                Request observationRequest = new Request();
                observationRequest.Observation = new RequestObservation();
                Response response = await proxy.SendRequest(observationRequest);

                ResponseObservation observation = response.Observation;

                if (observation == null)
                {
                    //OnEnd(observation, Result.Unset);
                    Console.WriteLine("observation null");
                    break;
                }
                if (response.Status == Status.Ended || response.Status == Status.Quit)
                {
                    //OnEnd(observation, observation.PlayerResult[(int)playerId - 1].Result);
                    Console.WriteLine($"observation ended or quit: {response.Status}");
                    break;
                }

                GetDbInfoFromChat(observation, playerId);

                if (start)
                {
                    start = false;
                    //OnStart(gameInfoResponse.GameInfo, dataResponse.Data, pingResponse, observation, playerId, opponentID);
                    Console.WriteLine("starting");
                }

                //IEnumerable<SC2APIProtocol.Action> actions = OnFrame(observation);
                Console.WriteLine($"Reading frame {observation.Observation.GameLoop}... I think?");

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
            }
        }
    }
}
