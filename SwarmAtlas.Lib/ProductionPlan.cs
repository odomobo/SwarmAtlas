using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmAtlas.Lib
{
    public class ProductionPlan
    {
        public Queue<ProductionTask> Tasks { get; set; } = new Queue<ProductionTask>();
        public List<ProductionTask.CSpawnUnit> PendingSpawnUnitTasks { get; set; } = new List<ProductionTask.CSpawnUnit>();
    }
}
