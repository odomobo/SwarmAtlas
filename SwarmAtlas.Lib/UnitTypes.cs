using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmAtlas.Lib
{
    public class UnitTypes
    {
        public Dictionary<uint, UnitTypeData> UnitTypeLookup { get; private set; }

        // zerg units
        public UnitTypeData Drone { get; private set; }
        public UnitTypeData Larva { get; private set; }
        public UnitTypeData Egg { get; private set; }
        public UnitTypeData Overlord { get; private set; }
        public UnitTypeData Overseer { get; private set; }

        // zerg structures
        public UnitTypeData Hatchery { get; private set; }
        public UnitTypeData SpawningPool { get; private set; }

        // terran units
        public UnitTypeData SCV { get; private set; }
        public UnitTypeData Marine { get; private set; }

        // terran structures
        public UnitTypeData CommandCenter { get; private set; }
        public UnitTypeData SupplyDepot { get; private set; }
        public UnitTypeData Barracks { get; private set; }

        public void Init(InitData initData) {

            var unitDict = initData.Data.Units.Where(u => !string.IsNullOrWhiteSpace(u.Name)).ToDictionary(u => u.Name);
            UnitTypeLookup = initData.Data.Units.Where(u => !string.IsNullOrWhiteSpace(u.Name)).ToDictionary(u => u.UnitId);

            // zerg units
            Drone = unitDict["Drone"];
            Larva = unitDict["Larva"];
            Egg = unitDict["Egg"];
            Overlord = unitDict["Overlord"];
            Overseer = unitDict["Overseer"];

            // zerg structures
            Hatchery = unitDict["Hatchery"];
            SpawningPool = unitDict["SpawningPool"];

            // terran units
            SCV = unitDict["SCV"];
            Marine = unitDict["Marine"];

            // terran structures
            CommandCenter = unitDict["CommandCenter"];
            SupplyDepot = unitDict["SupplyDepot"];
            Barracks = unitDict["Barracks"];
        }

        public bool IsStructure(uint unitTypeId)
        {
            return UnitTypeLookup[unitTypeId].Attributes.Contains(SC2APIProtocol.Attribute.Structure);
        }
    }
}
