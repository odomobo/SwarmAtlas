using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = SC2APIProtocol.Action;

namespace SwarmAtlas.Lib
{
    public class GameInfo
    {
        public uint MyPlayerId { get; private set; }
        public uint EnemyPlayerId { get; private set; }
        public uint NeutralPlayerId { get; private set; }

        public int Minerals;
        public int Gas;
        public int FreeSupply;

        public void Init(InitData initData)
        {
            MyPlayerId = initData.PlayerId;
            EnemyPlayerId = 1; // TODO: get this dynamically
            NeutralPlayerId = 16; // this is fine being hardcoded
        }

        public void OnFrame(FrameData frame, Queue<Action> actions)
        {
            var playerCommon = frame.Observation.Observation.PlayerCommon;
            Minerals = (int)playerCommon.Minerals;
            Gas = (int)playerCommon.Vespene;
            FreeSupply = (int)(playerCommon.FoodCap - playerCommon.FoodUsed);
        }
    }
}
