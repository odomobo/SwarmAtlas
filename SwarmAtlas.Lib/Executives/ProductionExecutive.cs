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
            //throw new NotImplementedException();
            var pendingLarvaTags = CheckPendingTasks(frame); // account for pending tasks...

            // TODO: keep track of pending macro tasks so that they can be ignored
            bool tryNextTask = true;
            while (tryNextTask && _plan.Tasks.Any())
            {
                var task = _plan.Tasks.First();
                switch (task.Type)
                {
                    case ProductionTask.EType.SpawnUnit:
                        tryNextTask = SpawnUnit(task.SpawnUnit, frame, pendingLarvaTags, actions);
                        break;
                    default: throw new Exception("Other types not yet supported");
                }
                // this is because currently we can't keep track of used resources, so we'll just do the next step next frame instead
                // In the future, we'll want to keep track of the larva or drones or buildings being used for stuff
                //return;
            }
        }

        private HashSet<ulong> CheckPendingTasks(FrameData frame)
        {
            var ret = new HashSet<ulong>();
            // TODO: if the pending task has been around for too long (maybe a second or 2), then remove it entirely I guess?

            for (int i = _plan.PendingSpawnUnitTasks.Count - 1; i >= 0; i--)
            {
                var task = _plan.PendingSpawnUnitTasks[i];
                var larva = _units.MyUnits.FirstOrDefault(u => u.Tag == task.LarvaTag);
                // if tag is no longer found, or at least not a larva
                if (larva == null || larva.UnitType != _unitTypes.Larva.UnitId)
                {
                    if (larva == null)
                    {
                        Logger.Warn($"Larva tag {task.LarvaTag} was expected to turn into an egg, but it's no longer found");
                    }
                    else if (larva.UnitType != _unitTypes.Egg.UnitId)
                    {
                        Logger.Warn($"Larva tag {task.LarvaTag} was expected to turn into an egg, but instead turned into {_unitTypes.UnitTypeLookup[larva.UnitType].Name}");
                    }

                    _plan.PendingSpawnUnitTasks.RemoveAt(i);
                }
                else // account for its cost when the larva is finally morphed
                {
                    Logger.Info($"Waiting for larva tag {task.LarvaTag} to morph into egg for unit {task.UnitType.Name}");
                    PayForSpawnUnitTask(task);
                    ret.Add(task.LarvaTag);
                }
            }

            return ret;
        }

        private bool SpawnUnit(ProductionTask.CSpawnUnit task, FrameData frame, HashSet<ulong> pendingLarvaTags, Queue<Action> actions)
        {
            var firstLarva = _units.MyLarva.Where(u => !pendingLarvaTags.Contains(u.Tag)).FirstOrDefault();
            if (firstLarva == null)
            {
                return false;
            }
            var larvaTag = firstLarva.Tag;

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

            Logger.Info($"Spawning unit {task.UnitType.Name} from larva tag {larvaTag}");

            // mark task as pending
            task.LarvaTag = larvaTag;
            PayForSpawnUnitTask(task);
            _plan.PendingSpawnUnitTasks.Add(task);
            pendingLarvaTags.Add(task.LarvaTag);


            // TODO: somehow mark the larva as being occupied
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.UnitTags.Add(larvaTag);
            action.ActionRaw.UnitCommand.AbilityId = (int)task.UnitType.AbilityId;
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
