using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{
    /// <summary>
    /// The animal moves to the target.
    /// </summary>
    public sealed class MoveTo : TargetAbility<Animal, IMovementTarget>
    {
        internal MoveTo(float goalDistance, bool interruptable, bool usesAttackDistance)
            : base(-1, 0, false, false)
        {
            GoalDistance = goalDistance;
            AttackEnemyInstead = interruptable;
            UsesAttackDistance = usesAttackDistance;
        }

        //interface IMovementParametrizing properties
        public float GoalDistance { get; }
        public bool AttackEnemyInstead { get; }
        public bool UsesAttackDistance { get; }

        public override void SetCommands(IEnumerable<Animal> casters, IMovementTarget target, bool resetCommandQueue)
        {
            //if there are no casters do nothing
            if (!casters.Any())
                return;

            if (resetCommandQueue)
                //reset all commands
                foreach (Animal c in casters)
                    c.ResetCommands();

            //player whose units are receiving commands
            Players player = casters.First().Player.PlayerID;

            //separete units to different groups by their movement
            var castersGroups = casters.ToLookup((unit) => unit.Movement);

            //volume of all units' circles /pi
            float volume = casters.Select((e) => e.Range * e.Range).Sum();
            //distance from the target when unit can stop if it gets stuck
            float minStoppingDistance = (float)Math.Sqrt(volume) * 1.3f;

            foreach (Movement m in Enum.GetValues(typeof(Movement)))
            {
                IEnumerable<Animal> castersMov = castersGroups[m];
                //set commands only if any unit can receive it
                if (!castersMov.Any())
                    continue;

                MoveToCommandAssignment mtca = new MoveToCommandAssignment(player, castersMov.Cast<Animal>().ToList(), m, target);
                //give command to each caster and set the command's creator
                foreach (Animal caster in castersMov)
                {
                    IComputable com = new MoveToCommand(caster, target, minStoppingDistance, this);
                    com.Assignment = mtca;

                    caster.AddCommand((Command)com);
                }
                MovementGenerator.GetMovementGenerator().AddNewCommand(player, mtca);
            }
        }

        public override Command NewCommand(Animal caster, IMovementTarget target)
        {
            throw new NotImplementedException("This method is not necessary because the virtual method " + nameof(SetCommands) + " was overriden");
        }

        public override string GetName() => "Move to";

        public override string Description()
        {
            return "The unit moves to the target. If the target is on a terrain " +
                "this unit can't move to, the unit won't do anything. If unit meets an enemy it attacks it instead.";
        }
    }


    public class MoveToCommand : Command<Animal, IMovementTarget, MoveTo>, IComputable
    {
        /// <summary>
        /// If the distance to the target is higher than this, flowfield will be used. 
        /// Otherwise unit will walk straight to the target.
        /// </summary>
        private const float FLOWMAP_DISTANCE = 1.41f;
        /// <summary>
        /// Assignment for generating flowfield in other thread.
        /// </summary>
        public MoveToCommandAssignment Assignment { get; set; }
        /// <summary>
        /// Flowmap used for navigation. It can be set after the command was assigned.
        /// </summary>
        public FlowField FlowMap { get; set; }
        /// <summary>
        /// Distance from the target when unit can stop if it gets stuck.
        /// </summary>
        private float MinStoppingDistance { get; }
        /// <summary>
        /// Detects if the animal got stuck.
        /// </summary>
        private NoMovementDetection NoMovementDetection { get; }

        public MoveToCommand(Animal commandedEntity, IMovementTarget target, float minStoppingDistance, MoveTo ability)
            : base(commandedEntity, target, ability)
        {
            MinStoppingDistance = minStoppingDistance;
            NoMovementDetection = new NoMovementDetection();
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            //command immediately finishes if the assignment was invalidated
            if (Assignment != null && Assignment.Invalid)
                return true;

            //set the command assignment to be active to increase its priority
            if (!Assignment.Active)
                Assignment.Active = true;

            //if an enemy is in attack range, attack it instead of other commands
            if (Ability.AttackEnemyInstead)
            {
                Entity enemy = game.GetAll<Animal>().Where(
                            (a) => a.Player != CommandedEntity.Player
                            && CommandedEntity.DistanceTo(a) <= CommandedEntity.AttackDistance).FirstOrDefault();
                if (enemy != null)
                {
                    //attack the enemy
                    CommandedEntity.StopMoving = true;
                    RemoveFromAssignment();
                    CommandedEntity.SetCommand(new AttackCommand(CommandedEntity, enemy, game.CurrentPlayer.GameStaticData.Abilities.Attack));
                    return false;//new command is already set
                }
            }

            //check if the map was set yet
            if (FlowMap == null)
                return false;

            Vector2 animalPos = CommandedEntity.Position;
            Building blockingBuilding;
            if ((blockingBuilding = game.Map[(int)animalPos.X, (int)animalPos.Y].Building) != null
                && blockingBuilding != Targ
                && blockingBuilding.Physical)
            {
                //go outside of node with building to be able to use flowfield
                CommandedEntity.Accelerate(blockingBuilding.Center.UnitDirectionTo(animalPos), game.Map);
            }
            else if (Targ.DistanceTo(CommandedEntity) > FLOWMAP_DISTANCE)
            {
                //use flowfield
                CommandedEntity.Accelerate(FlowMap.GetIntensity(CommandedEntity.Center, 1), game.Map);
            }
            else
            {
                //go in straight line
                Vector2 direction = CommandedEntity.Center.UnitDirectionTo(Targ.Center);
                CommandedEntity.Accelerate(direction, game.Map);
            }
            //update last four positions
            NoMovementDetection.AddNextPosition(CommandedEntity.Center);

            //set that unit wants to move
            CommandedEntity.WantsToMove = true;
            
            //command is finished if unit reached the goal distance or if it was standing at one
            //place near the target position for a long time
            if (Finished() //unit is close to the target point
                || (NoMovementDetection.NotMovingMuch(CommandedEntity.MaxSpeedLand * deltaT / 2) && CanStop())//unit is stuck
                /*|| game.Players[unit.Player.PlayerID].MapView
                    .GetObstacleMap(unit.Movement)[(int)TargetPoint.Center.X,(int)TargetPoint.Center.Y]*/)//target point is blocked
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true iff the animal is close enough to the target.
        /// </summary>
        public bool Finished()
        {
            if (Ability.UsesAttackDistance)
                //using attack distance
                return Targ.DistanceTo(CommandedEntity) <= CommandedEntity.AttackDistance;
            else
                //using ability distance
                return Targ.DistanceTo(CommandedEntity) <= Ability.GoalDistance;
        }

        /// <summary>
        /// Returns true if the unit should stop because of being stuck.
        /// </summary>
        private bool CanStop()
        {
            return Targ.DistanceTo(CommandedEntity) < MinStoppingDistance;
        }

        /// <summary>
        /// Removes CommandedEntity from CommandAssignment.
        /// </summary>
        public void RemoveFromAssignment()
        {
            Assignment.Animals.Remove(CommandedEntity);
        }

        public override int Progress => 100;

        public override void OnRemove()
        {
            CommandedEntity.StopMoving = true;
            RemoveFromAssignment();
        }
    }
    /*
    public class MoveToLogic
    {
        /// <summary>
        /// If the distance to the target is higher than this, flowfield will be used. 
        /// Otherwise unit will walk straight to the target.
        /// </summary>
        private const float FLOWMAP_DISTANCE = 1.41f;

        /// <summary>
        /// Moving unit.
        /// </summary>
        private Animal unit;
        /// <summary>
        /// Point on the map where the unit should go.
        /// </summary>
        public IMovementTarget TargetPoint { get; }
        /// <summary>
        /// Flowmap used for navigation. It can be set after the command was assigned.
        /// </summary>
        private FlowMap flowField;
        /// <summary>
        /// Distance from the target when unit can stop if it gets stuck.
        /// </summary>
        private float minStoppingDistance;
        private IMovementParametrizing movementParametrizing;
        private NoMovementDetection noMovementDetection;

        public MoveToLogic(Animal unit, FlowMap flowField, float minStoppingDistance, IMovementTarget target, IMovementParametrizing movementParametrizing)
        {
            this.unit = unit;
            this.flowField = flowField;
            this.minStoppingDistance = minStoppingDistance;
            noMovementDetection = new NoMovementDetection();
            this.TargetPoint = target;
            this.movementParametrizing = movementParametrizing;
        }

        public bool Step(Game game, float deltaT, FlowMap flowField, MoveToCommand command)
        {
            //set the command assignment to be active to increase its priority
            if (!command.Assignment.Active)
                command.Assignment.Active = true;

            //if an enemy is in attack range, attack it instead of other commands
            if (movementParametrizing.AttackEnemyInstead)
            {
                Entity enemy = game.GetAll<Animal>().Where(
                            (a) => a.Player != unit.Player
                            && unit.DistanceTo(a) <= unit.AttackDistance).FirstOrDefault();
                if (enemy != null)
                {
                    //attack the enemy
                    unit.StopMoving = true;
                    command.RemoveFromAssignment();
                    unit.SetCommand(new AttackCommand(unit, enemy, game.CurrentPlayer.GameStaticData.Abilities.Attack));
                    return false;//new command is already set
                }
            }

            //check if the map was set yet
            if (flowField == null)
                return false;

            float dist = (unit.Center - TargetPoint.Center).Length;

            Vector2 animalPos = unit.Position;
            Building blockingBuilding;
            if((blockingBuilding = game.Map[(int)animalPos.X, (int)animalPos.Y].Building) != null
                && blockingBuilding!=TargetPoint)
            {
                unit.Accelerate(blockingBuilding.Center.UnitDirectionTo(animalPos), game.Map);
            }
            else if (dist > FLOWMAP_DISTANCE)//todo: if animal is standing on node with building - push it out
            {
                //use flowfield
                unit.Accelerate(flowField.GetIntensity(unit.Center, 1), game.Map);
            }
            else
            {
                //go in straight line
                Vector2 direction = unit.Center.UnitDirectionTo(TargetPoint.Center);
                unit.Accelerate(direction, game.Map);
            }
            //update last four positions
            noMovementDetection.AddNextPosition(unit.Center);
            //set that unit wants to move
            unit.WantsToMove = true;

            bool finished = Finished();

            //command is finished if unit reached the goal distance or if it stayed at one
            //place near the target position for a long time
            if (finished //unit is close to the target point
                || (noMovementDetection.NotMovingMuch(unit.MaxSpeedLand * deltaT / 2) && CanStop())//unit is stuck
                )//target point is blocked
            {
                return true;
            }
            return false;
        }

        public bool Finished()
        {
            if (movementParametrizing.UsesAttackDistance)
                return TargetPoint.DistanceTo(unit) <= unit.AttackDistance;
            else
                return TargetPoint.DistanceTo(unit) <= movementParametrizing.GoalDistance;
        }

        public void UpdateFlowMap(FlowMap flFap)
        {
            this.flowField = flFap;
        }

        /// <summary>
        /// Returns true if the unit should stop because of being stuck.
        /// </summary>
        private bool CanStop()
        {
            return (TargetPoint.Center - unit.Center).Length < minStoppingDistance;
        }
    }*/

    /// <summary>
    /// Detects if the animal is not moving much from positions this animal has recently
    /// been at.
    /// </summary>
    public class NoMovementDetection
    {
        /// <summary>
        /// Last 4 positions of the entity.
        /// </summary>
        private Vector2?[] last4positions;

        public NoMovementDetection()
        {
            last4positions = new Vector2?[4];
        }

        /// <summary>
        /// Adds a next position.
        /// </summary>
        /// <param name="v"></param>
        public void AddNextPosition(Vector2 v)
        {
            last4positions[3] = last4positions[2];
            last4positions[2] = last4positions[1];
            last4positions[1] = last4positions[0];
            last4positions[0] = v;
        }

        /// <summary>
        /// Returns true if the animal hasn't moved at least minDistSum in last 3 moves.
        /// </summary>
        public bool NotMovingMuch(float minDistSum)
        {
            if (last4positions[0] != null &&
                last4positions[1] != null &&
                last4positions[2] != null &&
                last4positions[3] != null)
            {
                float d1 = (last4positions[0] - last4positions[1]).Value.Length;
                float d2 = (last4positions[1] - last4positions[2]).Value.Length;
                float d3 = (last4positions[2] - last4positions[3]).Value.Length;

                if (d1 + d2 + d3 < minDistSum)
                    return true;
            }
            return false;
        }
    }

    public interface IComputable
    {
        MoveToCommandAssignment Assignment { get; set; }
    }
}
