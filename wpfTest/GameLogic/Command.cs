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

    }

    public class MoveTowardsCommand : MovementCommand
    {
        public Vector2 TargetPoint;
        private float endDistance;//distance where the unit stops moving

        public MoveTowardsCommand(Unit commandedEntity, Vector2 targetPoint, float minStoppingDistance, float endDistance = 0.1f) 
            : base(commandedEntity,minStoppingDistance)
        {
            TargetPoint = targetPoint;
            this.endDistance = endDistance;
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            Vector2 direction = CommandedEntity.Pos.UnitDirectionTo(TargetPoint);
            CommandedEntity.Accelerate(CommandedEntity.Acceleration*direction);
            AddToLast4(CommandedEntity.Pos);
            //return (TargetPoint - CommandedEntity.Pos).Length < endDistance; 

            CommandedEntity.WantsToMove = true;

            bool finished = (CommandedEntity.Pos - TargetPoint).Length <= endDistance;
            if (finished || (Last4TooClose(deltaT) && CanStop()))
            {
                CommandedEntity.StopMoving = true;
                return true;
            }
            return false;
        }

        private bool CanStop()
        {
            return (TargetPoint-CommandedEntity.Pos).Length < minStoppingDistance;
        }
    }
    
    public class MoveToCommand : MovementCommand
    {
        private FlowMap flowMap;
        private Vector2 targetPos;
        private float endDistance;//distance where the unit stops moving


        public MoveToCommand(Unit commandedEntity, Vector2 targetPos, FlowMap flowMap, float minStoppingDistance, float endDistance=1.42f) 
            : base(commandedEntity, minStoppingDistance)
        {
            this.flowMap = flowMap;
            this.targetPos = targetPos;
            this.endDistance = endDistance;
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            ((MoveToCommandAssignment)Creator).Active = true;
            //map wasn't set yet
            if (flowMap == null)
                return false;

            CommandedEntity.Accelerate(flowMap.GetIntensity(CommandedEntity.Pos, CommandedEntity.Acceleration));
            AddToLast4(CommandedEntity.Pos);
            CommandedEntity.WantsToMove = true;

            bool finished = (CommandedEntity.Pos - targetPos).Length <= endDistance;
            
            if (finished || (Last4TooClose(deltaT) && CanStop()))
            {
                CommandedEntity.StopMoving = true;
                Creator.Units.Remove(CommandedEntity);
                return true;
            }
            return false;
        }

        private bool CanStop()
        {
            return (targetPos - CommandedEntity.Pos).Length < minStoppingDistance;
        }

        public void UpdateFlowMap(FlowMap flMap)
        {
            this.flowMap = flMap;
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
    }
}
