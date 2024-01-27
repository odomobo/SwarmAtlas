using System.Collections.Generic;
using SC2APIProtocol;

namespace SC2API.CSharp
{
    class HumanProxyBot : Bot
    {
        public string BotName => "Human";

        public void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentID)
        { }

        public IEnumerable<Action> OnFrame(ResponseObservation observation)
        {
            List<Action> actions = new List<Action>();

            return actions;
        }

        public void OnEnd(ResponseObservation observation, Result result)
        { }
    }
}
