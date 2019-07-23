using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Maps;

namespace wpfTest
{
    public abstract class Command
    {
        public Unit CommandedEntity { get; }
        /// <summary>
        /// Factory that created this command.
        /// </summary>
        public CommandAssignment Creator { get; set; }
        /// <summary>
        /// Performs one step of the command. Returns true if command is finished.
        /// </summary>
        public abstract bool PerformCommand(Game game, float deltaT);
        public Command(Unit commandedEntity)
        {
            CommandedEntity = commandedEntity;
        }
        public virtual void RemoveFromCreator()
        {
            if (Creator != null)
                Creator.Units.Remove(CommandedEntity);
        }
    }

    public abstract class MovementCommand:Command
    {
        private static Vector2 INVALID { get; }
        protected float minStoppingDistance;
        public override abstract bool PerformCommand(Game game, float deltaT);
        static MovementCommand()
        {
            INVALID = new Vector2(-1, -1);
        }
        public MovementCommand(Unit commandedEntity, float minStoppingDistance)
            :base(commandedEntity)
        {
            this.minStoppingDistance = minStoppingDistance;
            last4positions = new Vector2[4];
            last4positions[0] = INVALID;
            last4positions[1] = INVALID;
            last4positions[2] = INVALID;
            last4positions[3] = INVALID;
        }
        protected Vector2[] last4positions;

        protected void AddToLast4(Vector2 v)
        {
            last4positions[3] = last4positions[2];
            last4positions[2] = last4positions[1];
            last4positions[1] = last4positions[0];
            last4positions[0] = v;
        }

        protected bool Last4TooClose(float deltaT)
        {
            if (last4positions[0] != INVALID &&
                last4positions[1] != INVALID &&
                last4positions[2] != INVALID &&
                last4positions[3] != INVALID)
            {
                float d1 = (last4positions[0] - last4positions[1]).Length;
                float d2 = (last4positions[1] - last4positions[2]).Length;
                float d3 = (last4positions[2] - last4positions[3]).Length;

                if (d1 + d2 + d3 < CommandedEntity.MaxSpeed*deltaT/2)
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "Move";
        }
    }
    
    public abstract class MoveToCommand : MovementCommand
    {
        /// <summary>
        /// If the distance to the target is higher than this, flowmap will be used. 
        /// Otherwise unit will walk straight to the target.
        /// </summary>
        protected const float FLOWMAP_DISTANCE = 1.41f;

        /// <summary>
        /// Flowmap used for navigation. It can be set after the command was assigned.
        /// </summary>
        protected FlowMap flowMap;
        /// <summary>
        /// Distance where the unit stops moving.
        /// </summary>
        protected float goalDistance;
        /// <summary>
        /// Point on the map where the units should go.
        /// </summary>
        public abstract Vector2 TargetPoint { get; }
        /// <summary>
        /// If enemy in range, cancel commands and attack the enemy.
        /// </summary>
        protected bool interruptable;

        public MoveToCommand(Unit commandedEntity, FlowMap flowMap, float minStoppingDistance, float goalDistance=0.1f, bool usesAttackDistance=false, 
            bool interruptable=true) 
            : base(commandedEntity, minStoppingDistance)
        {
            this.flowMap = flowMap;
            this.goalDistance = goalDistance;
            this.interruptable = interruptable;
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            ((MoveToCommandAssignment)Creator).Active = true;

            //if an enemy is in attack range, attack it instead of other commands
            if(interruptable)
            {
                Unit enemy = GameQuerying.GetGameQuerying().SelectUnits(game, 
                    (u) => u.Owner!=CommandedEntity.Owner 
                            && CommandedEntity.DistanceTo(u) <= CommandedEntity.AttackDistance).FirstOrDefault();
                if(enemy!=null)
                {
                    //attack the enemy
                    CommandedEntity.StopMoving = true;
                    RemoveFromCreator();
                    CommandedEntity.SetCommand(new AttackCommand(CommandedEntity, enemy));
                    return false;//new command is already set
                }
            }

            //check if the map was set already
            if (flowMap == null)
                return false;

            float dist = (CommandedEntity.Pos - TargetPoint).Length;
            if (dist > FLOWMAP_DISTANCE)
            {
                //use flowmap
                CommandedEntity.Accelerate(flowMap.GetIntensity(CommandedEntity.Pos, CommandedEntity.Acceleration));
            }
            else
            {
                //go in straight line
                Vector2 direction = CommandedEntity.Pos.UnitDirectionTo(TargetPoint);
                CommandedEntity.Accelerate(CommandedEntity.Acceleration * direction);
            }
            //update last four positions
            AddToLast4(CommandedEntity.Pos);
            //set that entity want to move
            CommandedEntity.WantsToMove = true;

            bool finished=Finished();

            //command is finished if unit reached the goal distance or if it stayed at one
            //place near the target position for a long time
            if (finished || (Last4TooClose(deltaT) && CanStop()))
            {
                CommandedEntity.StopMoving = true;
                RemoveFromCreator();
                return true;
            }
            return false;
        }

        public abstract bool Finished();

        public void UpdateFlowMap(FlowMap flMap)
        {
            this.flowMap = flMap;
        }

        private bool CanStop()
        {
            return (TargetPoint - CommandedEntity.Pos).Length < minStoppingDistance;
        }
    }

    public class MoveToPointCommand:MoveToCommand
    {
        /// <summary>
        /// Point on the map where the units should go.
        /// </summary>
        private Vector2 targetPos;
        public override Vector2 TargetPoint=>targetPos;

        public MoveToPointCommand(Unit commandedEntity, Vector2 targetPos, FlowMap flowMap, float minStoppingDistance, float goalDistance = 0.1f)
            : base(commandedEntity, flowMap, minStoppingDistance, goalDistance)
        {
            this.targetPos = targetPos;
        }

        public override bool Finished()
        {
            return (TargetPoint - CommandedEntity.Pos).Length <= goalDistance;
        }
    }

    public class MoveToUnitCommand : MoveToCommand
    {
        /// <summary>
        /// Unit on the map to which the units should go.
        /// </summary>
        private Unit targetUnit;
        public override Vector2 TargetPoint => targetUnit.Pos;
        /// <summary>
        /// True if instead of goalDistance should be used the units AttackDistance.
        /// </summary>
        public bool UsesAttackDistance { get; }

        public MoveToUnitCommand(Unit commandedEntity, Unit targetUnit, FlowMap flowMap, float minStoppingDistance, float goalDistance = 0.1f, bool usesAttackDistance=false)
            : base(commandedEntity, flowMap, minStoppingDistance, goalDistance)
        {
            this.targetUnit = targetUnit;
            UsesAttackDistance = usesAttackDistance;
        }

        public override bool Finished()
        {
            float dist = (targetUnit.Pos - CommandedEntity.Pos).Length - targetUnit.Range - CommandedEntity.Range;
            if (UsesAttackDistance)
                return dist <= CommandedEntity.AttackDistance;
            else
                return dist <= goalDistance;
        }
    }

    public class AttackCommand : Command
    {
        private Unit target;
        private float timeUntilAttack;//time in s until this unit attacks

        public AttackCommand(Unit commandedEntity, Unit target)
            : base(commandedEntity)
        {
            this.target = target;
            this.timeUntilAttack = 0f;
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            //dead target cannont be attacked
            if (target.IsDead)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }
            CommandedEntity.Direction = target.Pos - CommandedEntity.Pos;

            CommandedEntity.CanBeMoved = false;
            timeUntilAttack += deltaT;
            if (timeUntilAttack >= CommandedEntity.AttackPeriod)
            {
                timeUntilAttack -= CommandedEntity.AttackPeriod;
                target.Health -= CommandedEntity.AttackDamage;
            }

            bool finished= game.Map.Distance(CommandedEntity, target) >= CommandedEntity.AttackDistance;
            if (finished)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "Attack";
        }
    }
}
