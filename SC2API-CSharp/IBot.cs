using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace SC2API.CSharp
{
    public interface IBot
    {
        //IEnumerable<SC2APIProtocol.Action> OnFrame(ResponseObservation observation);
        //void OnEnd(ResponseObservation observation, Result result);
        //void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, String opponentId);

        Task Run(ProtobufProxy proxy, uint playerId, string opponentID);

        Task ProcessReplay(ProtobufProxy proxy, uint playerId);

        string BotName { get; }
    }
}
