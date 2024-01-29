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
        public uint MyPlayerId { get; set; }
        public ulong FollowingUnitTag { get; set; }

        public void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentID)
        {
            Console.WriteLine($"PlayerID {playerId}");
            MyPlayerId = playerId;

            Console.WriteLine($"Unit 0 type: {observation.Observation.RawData.Units[0].UnitType}");
            Console.WriteLine($"Unit 0 owner: {observation.Observation.RawData.Units[0].Owner}");
        }

        public List<Action> OnFrame(ResponseObservation observation)
        {
            Console.WriteLine($"Processing step {observation.Observation.GameLoop}; last step ID was {LastStepId}");
            LastStepId = observation.Observation.GameLoop;

            var myUnits = observation.Observation.RawData.Units.Where(u => u.Owner == MyPlayerId).ToList(); ;

            var myDrones = myUnits.Where(u => u.UnitType == 104); // I think drone is 104

            if (FollowingUnitTag == 0)
            {
                var firstDrone = myDrones.First();
                FollowingUnitTag = firstDrone.Tag;
            }

            var followingUnit = myDrones.Where(u => u.Tag == FollowingUnitTag).First();
            
            Console.WriteLine($"Following drone xy: {followingUnit.Pos.X}, {followingUnit.Pos.Y}; facing: {followingUnit.Facing}; Tag: {followingUnit.Tag}");

            List<Action> actions = new List<Action>();

            return actions;
        }

        public void OnEnd(ResponseObservation observation, Result result)
        {
            
        }
    }
}
