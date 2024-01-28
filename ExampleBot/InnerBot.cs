using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Action = SC2APIProtocol.Action;

namespace ExampleBot
{
    internal class InnerBot
    {
        public uint LastStepId { get; set; }

        public void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentID)
        {
            Console.WriteLine($"PlayerID {playerId}");

            Console.WriteLine($"Unit 0 type: {observation.Observation.RawData.Units[0].UnitType}");
            Console.WriteLine($"Unit 0 owner: {observation.Observation.RawData.Units[0].Owner}");
        }

        public List<Action> OnFrame(ResponseObservation observation)
        {
            Console.WriteLine($"Processing step {observation.Observation.GameLoop}; last step ID was {LastStepId}");
            LastStepId = observation.Observation.GameLoop;

            List<Action> actions = new List<Action>();

            return actions;
        }

        public void OnEnd(ResponseObservation observation, Result result)
        {
            
        }
    }
}
