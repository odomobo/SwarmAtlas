using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmAtlas.Lib
{
    public class Units
    {
        public UnitTypeData Drone { get; private set; }

        public void Init(InitData initData) {

            var unitDict = initData.Data.Units.Where(u => !string.IsNullOrWhiteSpace(u.Name)).ToDictionary(u => u.Name);
            Drone = unitDict["Drone"];
        }
    }
}
