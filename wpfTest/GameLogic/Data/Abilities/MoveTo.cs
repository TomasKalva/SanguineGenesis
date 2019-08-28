using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{
    public sealed class MoveTo : TargetAbility<Animal, IMovementTarget>, IMovementParametrizing
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
        public MoveToCommandAssignment Assignment { get; set; }
        private MoveToLogic moveToLogic;
        /// <summary>
        /// Flowmap used for navigation. It can be set after the command was assigned.
        /// </summary>
        private FlowMap flowMap;
        
        public MoveToCommand(Animal commandedEntity, IMovementTarget target, float minStoppingDistance, MoveTo ability)
            : base(commandedEntity, target, ability)
        {
            moveToLogic = new MoveToLogic(CommandedEntity, null, minStoppingDistance, target, Ability);
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (!CanBeUsed())
                //finish if the target is invalid
                return true;

            bool finished = false;
            //command immediately finishes if the assignment was invalidated
            if (Assignment != null && Assignment.Invalid)
                finished = true;
            else
                finished = moveToLogic.Step(game, deltaT, flowMap, this);
            
            return finished;
        }


        public void UpdateFlowMap(FlowMap flowMap)
        {
            this.flowMap = flowMap;
        }

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

    public class MoveToLogic
    {
        /// <summary>
        /// If the distance to the target is higher than this, flowmap will be used. 
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
        public ITargetable TargetPoint { get; }
        /// <summary>
        /// Flowmap used for navigation. It can be set after the command was assigned.
        /// </summary>
        private FlowMap flowMap;
        /// <summary>
        /// Distance from the target when unit can stop if it gets stuck.
        /// </summary>
        private float minStoppingDistance;
        private IMovementParametrizing movementParametrizing;
        private NoMovementDetection noMovementDetection;

        public MoveToLogic(Animal unit, FlowMap flowMap, float minStoppingDistance, IMovementTarget target, IMovementParametrizing movementParametrizing)
        {
            this.unit = unit;
            this.flowMap = flowMap;
            this.minStoppingDistance = minStoppingDistance;
            noMovementDetection = new NoMovementDetection();
            this.TargetPoint = target;
            this.movementParametrizing = movementParametrizing;
        }

        public bool Step(Game game, float deltaT, FlowMap flowMap, MoveToCommand command)
        {
            //set the command assignment to be active to increase its priority
            if (!command.Assignment.Active)
                command.Assignment.Active = true;

            //if an enemy is in attack range, attack it instead of other commands
            if (movementParametrizing.AttackEnemyInstead)
            {
                Entity enemy = GameQuerying.GetGameQuerying().SelectEntities(game,
                    (u) => u.Player != unit.Player
                            && unit.DistanceTo(u) <= unit.AttackDistance).FirstOrDefault();
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
            if (flowMap == null)
                return false;

            float dist = (unit.Center - TargetPoint.Center).Length;
            if (dist > FLOWMAP_DISTANCE)//todo: if animal is standing on node with building - push it out
            {
                //use flowmap
                unit.Accelerate(flowMap.GetIntensity(unit.Center, 1), game.Map);
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
                || (noMovementDetection.NotMovingMuch(deltaT, unit.MaxSpeedLand * deltaT / 2) && CanStop())//unit is stuck
                                                                                                           /*|| game.Players[unit.Player.PlayerID].MapView
                                                                                                               .GetObstacleMap(unit.Movement)[(int)TargetPoint.Center.X,(int)TargetPoint.Center.Y]*/)//target point is blocked
            {
                return true;
            }
            return false;
        }

        public bool Finished()
        {
            if (TargetPoint is Entity entity)
            {
                //target is entity
                //use distance between closest points of the unit and the target
                if (movementParametrizing.UsesAttackDistance)
                    return unit.DistanceTo(entity) <= unit.AttackDistance;
                else
                    return unit.DistanceTo(entity) <= movementParametrizing.GoalDistance;
            }
            else if (TargetPoint is Vector2 vector)
            {
                //target is vector
                //use distance between center of the unit and the point
                return (vector - unit.Center).Length <= movementParametrizing.GoalDistance;
            }
            else
            {
                Node node = TargetPoint as Node;
                return unit.DistanceTo(node) <= movementParametrizing.GoalDistance;
            }
        }

        public void UpdateFlowMap(FlowMap flMap)
        {
            this.flowMap = flMap;
        }

        /// <summary>
        /// Returns true if the unit can stop because of being stuck.
        /// </summary>
        private bool CanStop()
        {
            return (TargetPoint.Center - unit.Center).Length < minStoppingDistance;
        }
    }

    /// <summary>
    /// Detects if the unit is not moving much.
    /// </summary>
    public class NoMovementDetection
    {
        private static Vector2 INVALID { get; }
        private Vector2[] last4positions;
        static NoMovementDetection()
        {
            INVALID = new Vector2(-1, -1);
        }
        public NoMovementDetection()
        {
            last4positions = new Vector2[4];
            last4positions[0] = INVALID;
            last4positions[1] = INVALID;
            last4positions[2] = INVALID;
            last4positions[3] = INVALID;
        }

        public void AddNextPosition(Vector2 v)
        {
            last4positions[3] = last4positions[2];
            last4positions[2] = last4positions[1];
            last4positions[1] = last4positions[0];
            last4positions[0] = v;
        }

        public bool NotMovingMuch(float deltaT, float minDistSum)
        {
            if (last4positions[0] != INVALID &&
                last4positions[1] != INVALID &&
                last4positions[2] != INVALID &&
                last4positions[3] != INVALID)
            {
                float d1 = (last4positions[0] - last4positions[1]).Length;
                float d2 = (last4positions[1] - last4positions[2]).Length;
                float d3 = (last4positions[2] - last4positions[3]).Length;

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
