using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmAtlas.Lib
{
    public class ProductionTask
    {
        public enum EType
        {
            SpawnUnit,
            SpawnStructure,
            UpgradeStructure,
            StartUpgrade,
        }

        public class CSpawnUnit
        {
            public UnitTypeData UnitType { get; set; }
            public ulong LarvaTag { get; set; }
        }

        public class CSpawnStructure
        {

        }

        public class CUpgradeStructure
        {

        }

        public class CStartUpgrade
        {

        }

        public EType Type { get; set; }
        public CSpawnUnit SpawnUnit { get; set; }
        public CSpawnStructure SpawnStructure { get; set; }
        public CUpgradeStructure UpgradeStructure { get; set; }
        public CStartUpgrade StartUpgrade { get; set; }

        public static ProductionTask MakeSpawnUnit(UnitTypeData unitType)
        {
            return new ProductionTask
            {
                Type = EType.SpawnUnit,
                SpawnUnit = new CSpawnUnit { UnitType = unitType }
            };
        }
    }
}
