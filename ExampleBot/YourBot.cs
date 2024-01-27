using System.Collections.Generic;
using SC2APIProtocol;
using System;

using Action = SC2APIProtocol.Action;
using System.Linq;
using LiteDB;

namespace ExampleBot
{
    class UnitData
    {
        public List<Unit> Units { get; set; }
    }
    class ExampleBot : SC2API.CSharp.Bot
    {
        public string BotName => "ExampleBot";

        //private LiteDatabase _liteDb;

        public void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentID)
        {

            Console.WriteLine($"PlayerID {playerId}");
            
            Console.WriteLine($"Unit 0 type: {observation.Observation.RawData.Units[0].UnitType}");
            Console.WriteLine($"Unit 0 owner: {observation.Observation.RawData.Units[0].Owner}");
        }
    
        public IEnumerable<Action> OnFrame(ResponseObservation observation)
        {
            List<Action> actions = new List<Action>();

            

            return actions;
        }
        
        public void OnEnd(ResponseObservation observation, Result result)
        { }
    }
}
