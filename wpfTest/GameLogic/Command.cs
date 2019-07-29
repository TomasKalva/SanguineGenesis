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
        /// <summary>
        /// Performs one step of the command. Returns true if command is finished.
        /// </summary>
        public abstract bool PerformCommand(Game game, float deltaT);
        /*public Command(Entity commandedEntity)
        {
            CommandedEntity = commandedEntity;
        }*/
        //public abstract Command NewInstance(Entity caster, ITargetable target);
    }

    public abstract class Command<Caster, Target, Abil> : Command where Caster : Entity
                                                                    where Target : ITargetable
                                                                    where Abil:TargetAbility<Caster,Target>
    {
        public Caster CommandedEntity { get; }
        public Target Targ { get; }

        protected Command(Caster commandedEntity, Target target)
        {
            CommandedEntity = commandedEntity;
            Targ = target;
        }
        /*
        public static Command NewInstance(Entity caster, ITargetable target)
        {
            return NewInstance((Caster)caster, (Target)target);
        }

        public static Command NewInstance(Caster caster, Target target) { throw new NotImplementedException(); }*/
    }

    public class AttackCommand : Command<Unit, Entity, Attack>
    {
        private float timeUntilAttack;//time in s until this unit attacks

        private AttackCommand():base(null,null) => throw new NotImplementedException();
        public AttackCommand(Unit commandedEntity, Entity target)
            : base(commandedEntity, target)
        {
            this.timeUntilAttack = 0f;
        }
        
        public override bool PerformCommand(Game game, float deltaT)
        {
            //dead target cannont be attacked
            if (Targ.IsDead)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }
            CommandedEntity.Direction = ((Unit)Targ).Center - CommandedEntity.Center;

            CommandedEntity.CanBeMoved = false;
            timeUntilAttack += deltaT;
            if (timeUntilAttack >= CommandedEntity.AttackPeriod)
            {
                timeUntilAttack -= CommandedEntity.AttackPeriod;
                Targ.Health -= CommandedEntity.AttackDamage;
            }

            bool finished = CommandedEntity.DistanceTo(Targ) >= CommandedEntity.AttackDistance;
            if (finished)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }
            return false;
        }
    }

    public interface IComputable
    {
        MoveToCommandAssignment Creator { get; set; }
    }

    public class MoveToPointCommand : Command<Unit, Vector2, MoveToPoint>,IComputable
    {
        public MoveToCommandAssignment Creator { get; set; }
        private MoveToLogic moveToLogic;
        /// <summary>
        /// Flowmap used for navigation. It can be set after the command was assigned.
        /// </summary>
        private FlowMap flowMap;

        private MoveToPointCommand() : base(null, default(Vector2)) => throw new NotImplementedException();
        public MoveToPointCommand(Unit commandedEntity, Vector2 target)
            : base(commandedEntity, target)
        {
            moveToLogic = new MoveToLogic(CommandedEntity, null, 0,target);
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            return moveToLogic.Step(game, deltaT, flowMap);
        }

        public void UpdateFlowMap(FlowMap flowMap)
        {
            this.flowMap = flowMap;
        }

        public void RemoveFromCreator()
        {
            Creator.Units.Remove(CommandedEntity);
        }
    }
    /*
    public abstract class MovementCommand:Command
    {
        public Unit CommandedUnit => (Unit)CommandedEntity;
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

                if (d1 + d2 + d3 < CommandedUnit.MaxSpeed*deltaT/2)
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "Move";
        }
    }*/
    
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
        private Unit unit;
        /// <summary>
        /// Flowmap used for navigation. It can be set after the command was assigned.
        /// </summary>
        private FlowMap flowMap;
        /// <summary>
        /// Distance where the unit stops moving.
        /// </summary>
        private float goalDistance;
        /// <summary>
        /// Point on the map where the unit should go.
        /// </summary>
        public ITargetable TargetPoint { get; set; }
        /// <summary>
        /// If enemy in range, cancel commands and attack the enemy.
        /// </summary>
        private bool interruptable;
        private float minStoppingDistance;
        private bool usesAttackDistance;
        private StuckBreaking stuckBreaking;

        public MoveToLogic(Unit unit, FlowMap flowMap, float minStoppingDistance, ITargetable target, float goalDistance = 0.1f, bool usesAttackDistance = false,
            bool interruptable = true)
        {
            this.unit = unit;
            this.flowMap = flowMap;
            this.minStoppingDistance = minStoppingDistance;
            this.goalDistance = goalDistance;
            this.usesAttackDistance = usesAttackDistance;
            this.interruptable = interruptable;
            stuckBreaking = new StuckBreaking();
            this.TargetPoint = target;
        }

        public bool Step(Game game, float deltaT, FlowMap flowMap)
        {
            //((MoveToCommandAssignment)Creator).Active = true;
            
            /*
            //if an enemy is in attack range, attack it instead of other commands
            if(interruptable)
            {
                Entity enemy = GameQuerying.GetGameQuerying().SelectUnits(game, 
                    (u) => u.Owner!=unit.Owner 
                            && unit.DistanceTo(u) <= unit.AttackDistance).FirstOrDefault();
                if(enemy!=null)
                {
                    //attack the enemy
                    unit.StopMoving = true;
                    RemoveFromCreator();
                    unit.SetCommand(new AttackCommand(unit, enemy));
                    return false;//new command is already set
                }
            }*/

            //check if the map was set already
            if (flowMap == null)
                return false;

            float dist = (unit.Center - TargetPoint.Center).Length;
            if (dist > FLOWMAP_DISTANCE)
            {
                //use flowmap
                unit.Accelerate(flowMap.GetIntensity(unit.Center, unit.Acceleration));
            }
            else
            {
                //go in straight line
                Vector2 direction = unit.Center.UnitDirectionTo(TargetPoint.Center);
                unit.Accelerate(unit.Acceleration * direction);
            }
            //update last four positions
            stuckBreaking.AddToLast4(unit.Center);
            //set that entity want to move
            unit.WantsToMove = true;

            bool finished=Finished();

            //command is finished if unit reached the goal distance or if it stayed at one
            //place near the target position for a long time
            if (finished || (stuckBreaking.Last4TooClose(deltaT, unit.MaxSpeed * deltaT / 2) && CanStop()))
            {
                unit.StopMoving = true;
                //RemoveFromCreator();
                return true;
            }
            return false;
        }

        public bool Finished()
        {
            return (TargetPoint.Center - unit.Center).Length <= goalDistance;
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

    public class StuckBreaking
    {
        private static Vector2 INVALID { get; }
        private Vector2[] last4positions;
        static StuckBreaking()
        {
            INVALID = new Vector2(-1, -1);
        }
        public StuckBreaking()
        {
            last4positions = new Vector2[4];
            last4positions[0] = INVALID;
            last4positions[1] = INVALID;
            last4positions[2] = INVALID;
            last4positions[3] = INVALID;
        }

        public void AddToLast4(Vector2 v)
        {
            last4positions[3] = last4positions[2];
            last4positions[2] = last4positions[1];
            last4positions[1] = last4positions[0];
            last4positions[0] = v;
        }

        public bool Last4TooClose(float deltaT, float minDistSum)
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
    /*
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
            return (TargetPoint - CommandedUnit.Center).Length <= goalDistance;
        }
    }*/
    /*
    public class MoveToUnitCommand : MoveToCommand
    {
        /// <summary>
        /// Unit on the map to which the units should go.
        /// </summary>
        private Entity targetUnit;
        public override Vector2 TargetPoint => ((Unit)targetUnit).Center;
        /// <summary>
        /// True if instead of goalDistance should be used the units AttackDistance.
        /// </summary>
        public bool UsesAttackDistance { get; }

        public MoveToUnitCommand(Unit commandedEntity, Entity targetUnit, FlowMap flowMap, float minStoppingDistance, float goalDistance = 0.1f, bool usesAttackDistance=false)
            : base(commandedEntity, flowMap, minStoppingDistance, goalDistance)
        {
            this.targetUnit = targetUnit;
            UsesAttackDistance = usesAttackDistance;
        }

        public override bool Finished()
        {
            float dist = CommandedUnit.DistanceTo(targetUnit);
            if (UsesAttackDistance)
                return dist <= CommandedUnit.AttackDistance;
            else
                return dist <= goalDistance;
        }
    }*/

    /*
    public class AttackCommand : Command
    {
        public Unit CommandedUnit => (Unit)CommandedEntity;
        private Entity target;
        private float timeUntilAttack;//time in s until this unit attacks

        public AttackCommand(Entity commandedEntity, Entity target)
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
                CommandedUnit.CanBeMoved = true;
                return true;
            }
            CommandedUnit.Direction = ((Unit)target).Center - CommandedUnit.Center;

            CommandedUnit.CanBeMoved = false;
            timeUntilAttack += deltaT;
            if (timeUntilAttack >= CommandedUnit.AttackPeriod)
            {
                timeUntilAttack -= CommandedUnit.AttackPeriod;
                target.Health -= CommandedUnit.AttackDamage;
            }

            bool finished= CommandedEntity.DistanceTo(target) >= CommandedUnit.AttackDistance;
            if (finished)
            {
                CommandedUnit.CanBeMoved = true;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "Attack";
        }
    }*/
}
