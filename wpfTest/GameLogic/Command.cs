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

    public class MoveTowardsCommand : Command
    {
        public Vector2 TargetPoint;
        private float endDistance;//distance where the unit stops moving

        public MoveTowardsCommand(Unit commandedEntity, Vector2 targetPoint, float endDistance = 0.1f) 
            : base(commandedEntity)
        {
            TargetPoint = targetPoint;
            this.endDistance = endDistance;
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            Vector2 direction = CommandedEntity.Pos.UnitDirectionTo(TargetPoint);
            CommandedEntity.Accelerate(CommandedEntity.Acceleration*direction);
            return (TargetPoint - CommandedEntity.Pos).Length < endDistance;
        }
    }
    
    public class MoveToCommand : Command
    {
        private FlowMap flowMap;
        private Vector2 targetPos;
        private float endDistance;//distance where the unit stops moving

        public MoveToCommand(Unit commandedEntity, Vector2 targetPos, FlowMap flowMap, float endDistance=1.42f) 
            : base(commandedEntity)
        {
            this.flowMap = flowMap;
            this.targetPos = targetPos;
            this.endDistance = endDistance;
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            CommandedEntity.Accelerate(flowMap.GetIntensity(CommandedEntity.Pos, CommandedEntity.Acceleration));
            return (CommandedEntity.Pos - targetPos).Length<=endDistance;
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
                return true;

            timeUntilAttack += deltaT;
            if (timeUntilAttack >= CommandedEntity.AttackPeriod)
            {
                timeUntilAttack -= CommandedEntity.AttackPeriod;
                target.Health -= CommandedEntity.AttackDamage;
            }
            return game.Map.Distance(CommandedEntity, target) >= CommandedEntity.AttackDistance;
        }
    }
}
