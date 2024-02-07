using NLog;
using SC2API.CSharp;
using SC2APIProtocol;
using SwarmAtlas.Lib.Executives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = SC2APIProtocol.Action;

namespace SwarmAtlas.Lib
{
    public class SwarmAtlas
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // injected
        public required UnitTypes UnitTypes { protected get; init; }
        public required Units Units { protected get; init; }
        public required GameInfo GameInfo { protected get; init; }
        public required ProductionExecutive ProductionExecutive { protected get; init; }
        public required SceneBuilder SceneBuilder { protected get; init; }
        
        public void Init(InitData initData)
        {
            SceneBuilder.Init(initData);
            GameInfo.Init(initData);
            UnitTypes.Init(initData);

            var plan = new ProductionPlan();
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(UnitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(UnitTypes.Overlord));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(UnitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(UnitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(UnitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(UnitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(UnitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(UnitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(UnitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(UnitTypes.Drone));
            ProductionExecutive.SetProductionPlan(plan);
        }

        public void OnFrame(FrameData frameData, Queue<SC2APIProtocol.Action> actions)
        {
            Logger.Info($"Processing step {frameData.Observation.Observation.GameLoop}");

            SceneBuilder.OnFrame(frameData, actions);
            Units.OnFrame(frameData, actions);
            GameInfo.OnFrame(frameData, actions);

            ProductionExecutive.OnFrame(frameData, actions);

            SceneBuilder.Render();
        }

        // just for cleanup; currently unused
        public void OnEnd(Result result)
        {
            
        }
    }
}
