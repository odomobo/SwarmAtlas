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

        private readonly UnitTypes _unitTypes;
        private readonly Units _units;
        private readonly GameInfo _gameInfo;
        private readonly ProductionExecutive _productionExecutive;

        public SwarmAtlas(UnitTypes unitTypes, Units units, GameInfo gameInfo, ProductionExecutive productionExecutive)
        {
            _unitTypes = unitTypes;
            _units = units;
            _gameInfo = gameInfo;
            _productionExecutive = productionExecutive;
        }

        public void Init(InitData initData)
        {
            _gameInfo.Init(initData);
            _unitTypes.Init(initData);

            var plan = new ProductionPlan();
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(_unitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(_unitTypes.Overlord));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(_unitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(_unitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(_unitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(_unitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(_unitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(_unitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(_unitTypes.Drone));
            plan.Tasks.Enqueue(ProductionTask.MakeSpawnUnit(_unitTypes.Drone));
            _productionExecutive.SetProductionPlan(plan);
        }

        public void OnFrame(FrameData frameData, Queue<Action> actions)
        {
            Logger.Info($"Processing step {frameData.Observation.Observation.GameLoop}");

            _units.OnFrame(frameData, actions);
            _gameInfo.OnFrame(frameData, actions);

            _productionExecutive.OnFrame(frameData, actions);
        }

        // just for cleanup; currently unused
        public void OnEnd(Result result)
        {
            
        }
    }
}
