using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace SC2API.CSharp
{
    public interface IBot
    {
        Task Run(ProtobufProxy proxy, uint playerId);

        // TODO: remove this
        string BotName { get; }
    }
}
