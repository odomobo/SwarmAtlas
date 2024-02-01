using NLog;
using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = SC2APIProtocol.Action;

namespace SwarmAtlas.Lib.Executives
{
    public class ProductionExecutive
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Units _units;
        private readonly UnitTypes _unitTypes;
        private readonly GameInfo _gameInfo;

        private ProductionPlan _plan = new ProductionPlan();

        public ProductionExecutive(Units units, UnitTypes unitTypes, GameInfo gameInfo)
        {
            _units = units;
            _unitTypes = unitTypes;
            _gameInfo = gameInfo;
        }

        public void SetProductionPlan(ProductionPlan plan)
        {
            _plan = plan;
        }

        public void OnFrame(FrameData frame, Queue<Action> actions)
        {
            // TODO: need to account for larva which are building, somehow... if we spent resources last turn, we should have some way of knowing
            throw new NotImplementedException();
            CheckPendingTasks(frame); // account for pending tasks...

            // TODO: keep track of pending macro tasks so that they can be ignored
            bool tryNextTask = true;
            while (tryNextTask && _plan.Tasks.Any())
            {
                var task = _plan.Tasks.First();
                switch (task.Type)
                {
                    case ProductionTask.EType.SpawnUnit:
                        // logic
                        tryNextTask = SpawnUnit(task.SpawnUnit, frame, actions);
                        break;
                    default: throw new Exception("Other types not yet supported");
                }
                // this is because currently we can't keep track of used resources, so we'll just do the next step next frame instead
                // In the future, we'll want to keep track of the larva or drones or buildings being used for stuff
                return;
            }
        }

        private void CheckPendingTasks(FrameData frame)
        {
            // TODO: if the pending task has been around for too long (maybe a second or 2), then remove it entirely I guess?

            for (int i = _plan.PendingSpawnUnitTasks.Count - 1; i >= 0; i--)
            {
                var task = _plan.PendingSpawnUnitTasks[i];
                var larva = _units.MyUnits.FirstOrDefault(u => u.Tag == task.LarvaTag);
                // if tag is no longer found, or at least not a larva
                if (larva == null || larva.BuildProgress != 0)
                {
                    _plan.PendingSpawnUnitTasks.RemoveAt(i);
                }
                else // account for its cost when the larva is finally morphed
                {
                    PayForSpawnUnitTask(task);
                }
            }
        }

        private bool SpawnUnit(ProductionTask.CSpawnUnit task, FrameData frame, Queue<Action> actions)
        {
            var firstLarva = _units.MyUnits.FirstOrDefault(u => u.UnitType == _unitTypes.Larva.UnitId);
            if (firstLarva == null)
            {
                return false;
            }

            var mineralCost = task.UnitType.MineralCost;
            var gasCost = task.UnitType.VespeneCost;
            var supplyCost = (uint)task.UnitType.FoodRequired;

            // check requirements before building
            if (_gameInfo.Minerals < mineralCost ||
                _gameInfo.Gas < gasCost ||
                _gameInfo.FreeSupply < supplyCost)
            {
                return false;
            }

            // mark task as pending
            task.LarvaTag = firstLarva.Tag;
            PayForSpawnUnitTask(task);
            _plan.PendingSpawnUnitTasks.Add(task);

            // TODO: somehow mark the larva as being occupied

            Logger.Info($"Spawning unit {task.UnitType.Name}");
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.UnitTags.Add(firstLarva.Tag);
            action.ActionRaw.UnitCommand.AbilityId = (int)task.UnitType.AbilityId;
            // is this ok???
            //action.ActionRaw.UnitCommand.TargetUnitTag = task.LarvaTag;
            actions.Enqueue(action);

            // since we did the action, remove from the plan
            _plan.Tasks.Dequeue();

            if (_plan.Tasks.Any())
            {
                Logger.Info($"Next task type: {_plan.Tasks.First().Type}");
            }
            else
            {
                Logger.Warn("No more tasks in plan");
            }

            return true;
        }

        void PayForSpawnUnitTask(ProductionTask.CSpawnUnit task)
        {
            var mineralCost = task.UnitType.MineralCost;
            var gasCost = task.UnitType.VespeneCost;
            var supplyCost = (uint)task.UnitType.FoodRequired;

            _gameInfo.Minerals -= (int)mineralCost;
            _gameInfo.Gas -= (int)gasCost;
            _gameInfo.FreeSupply -= (int)supplyCost;
        }
    }
}
