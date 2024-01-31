using NLog;
using SC2API.CSharp;
using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = SC2APIProtocol.Action;

namespace SwarmAtlas.Lib
{
    public class SwarmAtlas
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public uint LastStepId { get; set; }
        public uint MyPlayerId { get; set; }
        public ulong FollowingUnitTag { get; set; }

        private readonly Units _units;

        public SwarmAtlas(Units units)
        {
            _units = units;
        }

        public void Init(InitData initData)
        {
            _units.Init(initData);

            MyPlayerId = initData.PlayerId;
            Logger.Info($"PlayerID {MyPlayerId}");
            
            Logger.Info($"Unit 0 type: {initData.Observation.Observation.RawData.Units[0].UnitType}");
            Logger.Info($"Unit 0 owner: {initData.Observation.Observation.RawData.Units[0].Owner}");
        }

        public List<Action> OnFrame(FrameData frameData)
        {
            Logger.Info($"Processing step {frameData.Observation.Observation.GameLoop}; last step ID was {LastStepId}");
            LastStepId = frameData.Observation.Observation.GameLoop;

            var myUnits = frameData.Observation.Observation.RawData.Units.Where(u => u.Owner == MyPlayerId).ToList();

            var myDrones = myUnits.Where(u => u.UnitType == _units.Drone.UnitId); // drone is 104

            if (FollowingUnitTag == 0)
            {
                var firstDrone = myDrones.First();
                FollowingUnitTag = firstDrone.Tag;
            }

            var followingUnit = myDrones.Where(u => u.Tag == FollowingUnitTag).First();
            
            Logger.Info($"Following drone xy: {followingUnit.Pos.X}, {followingUnit.Pos.Y}; facing: {followingUnit.Facing}; Tag: {followingUnit.Tag}");

            List<Action> actions = new List<Action>();

            return actions;
        }

        // probably not needed...
        public void OnEnd(ResponseObservation observation, Result result)
        {
            
        }
    }
}
