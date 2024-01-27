﻿using System.Collections.Generic;
using SC2APIProtocol;

namespace ExampleBot
{
    class Bot : SC2API.CSharp.Bot
    {
        public string BotName => "ExampleBot";

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
