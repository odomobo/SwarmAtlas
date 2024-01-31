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
        public UnitTypeData Drone { get; }
        public Units(IEnumerable<UnitTypeData> unitTypesIEnumerable) {
            var unitTypes = unitTypesIEnumerable.ToList();
            Drone = unitTypes.First(u => u.Name == "Drone");
        }
    }
}
