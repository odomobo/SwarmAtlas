using System.Collections.Generic;
using System.Threading.Tasks;
using SC2APIProtocol;
using System;
using Action = SC2APIProtocol.Action;

namespace SC2API.CSharp
{
    class HumanProxyBot : IBot
    {
        public string BotName => "TerranColony";

        public void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId)
        { }

        public IEnumerable<Action> OnFrame(ResponseObservation observation)
        {
            List<Action> actions = new List<Action>();

            return actions;
        }

        public void OnEnd(ResponseObservation observation, Result result)
        { }

        public async Task<ResponsePing> Ping(ProtobufProxy proxy)
        {
            Request request = new Request();
            request.Ping = new RequestPing();
            Response response = await proxy.SendRequest(request);
            return response.Ping;
        }

        public async Task Run(ProtobufProxy proxy, uint playerId)
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

                if (start)
                {
                    start = false;
                    OnStart(gameInfoResponse.GameInfo, dataResponse.Data, pingResponse, observation, playerId);
                }

                IEnumerable<SC2APIProtocol.Action> actions = OnFrame(observation);

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
