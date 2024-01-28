using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Action = SC2APIProtocol.Action;

namespace ExampleBot
{
    internal interface IInnerBot
    {

        void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentID);

        List<Action> OnFrame(ResponseObservation observation);

        void OnEnd(ResponseObservation observation, Result result);
    }
    internal class InnerBot : IInnerBot
    {
        public uint LastProcessedStep { get; set; }

        public void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentID)
        {
            Console.WriteLine($"PlayerID {playerId}");

            Console.WriteLine($"Unit 0 type: {observation.Observation.RawData.Units[0].UnitType}");
            Console.WriteLine($"Unit 0 owner: {observation.Observation.RawData.Units[0].Owner}");
        }

        public List<Action> OnFrame(ResponseObservation observation)
        {
            LastProcessedStep = observation.Observation.GameLoop;

            List<Action> actions = new List<Action>();

            return actions;
        }

        public void OnEnd(ResponseObservation observation, Result result)
        {
            
        }
    }
}
