﻿using SC2API.CSharp;
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
        private readonly ProtobufProxy _proxy;
        public uint LastStepId { get; set; }
        public uint MyPlayerId { get; set; }
        public ulong FollowingUnitTag { get; set; }

        public InnerBot(ProtobufProxy proxy, InitData initData)
        {
            _proxy = proxy;
            ResponseGameInfo gameInfo = proxy.GetResponseFromResponseBuf(initData.GameInfo).GameInfo;
            ResponseData data = proxy.GetResponseFromResponseBuf(initData.Data).Data;
            ResponsePing pingResponse = proxy.GetResponseFromResponseBuf(initData.PingResponse).Ping;
            ResponseObservation observation = proxy.GetResponseFromResponseBuf(initData.Observation).Observation;
            uint playerId = initData.PlayerId;

            Console.WriteLine($"PlayerID {playerId}");
            MyPlayerId = playerId;

            Console.WriteLine($"Unit 0 type: {observation.Observation.RawData.Units[0].UnitType}");
            Console.WriteLine($"Unit 0 owner: {observation.Observation.RawData.Units[0].Owner}");
        }

        public List<Action> OnFrame(FrameData frameData)
        {
            ResponseObservation observation = _proxy.GetResponseFromResponseBuf(frameData.Observation).Observation;
            int frameNumber = frameData.FrameNumber;

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

        // probably not needed...
        public void OnEnd(ResponseObservation observation, Result result)
        {
            
        }
    }
}
