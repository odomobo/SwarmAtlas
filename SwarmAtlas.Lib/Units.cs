using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = SC2APIProtocol.Action;

namespace SwarmAtlas.Lib
{
    public class Units
    {
        public List<Unit> MyUnits { get; set; }
        public List<Unit> MyLarva { get; set; }
        public List<Unit> MyStructures { get; set; }
        public List<Unit> EnemyUnits { get; set; }
        public List<Unit> EnemyStructures { get; set; }
        public List<Unit> NeutralStructures { get; set; }

        private readonly GameInfo _gameInfo;
        private readonly UnitTypes _unitTypes;

        public Units(GameInfo gameInfo, UnitTypes unitTypes)
        {
            _gameInfo = gameInfo;
            _unitTypes = unitTypes;
        }

        public void OnFrame(FrameData frame, Queue<Action> actions)
        {
            var allUnits = frame.Observation.Observation.RawData.Units.ToList();
            MyUnits = allUnits.Where(u => u.Owner == _gameInfo.MyPlayerId && !_unitTypes.IsStructure(u.UnitType)).ToList();
            // we make sure to list the larva in tag order, because that's the order they are actually selected
            MyLarva = MyUnits.Where(u => u.UnitType == _unitTypes.Larva.UnitId).OrderBy(u => u.Tag).ToList();
            MyStructures = allUnits.Where(u => u.Owner == _gameInfo.MyPlayerId && _unitTypes.IsStructure(u.UnitType)).ToList();
            EnemyUnits = allUnits.Where(u => u.Owner == _gameInfo.EnemyPlayerId && !_unitTypes.IsStructure(u.UnitType)).ToList();
            EnemyStructures = allUnits.Where(u => u.Owner == _gameInfo.EnemyPlayerId && _unitTypes.IsStructure(u.UnitType)).ToList();
            NeutralStructures = allUnits.Where(u => u.Owner == _gameInfo.NeutralPlayerId && _unitTypes.IsStructure(u.UnitType)).ToList();
        }
    }
}
