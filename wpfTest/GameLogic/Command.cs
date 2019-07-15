using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;

namespace wpfTest
{
    public abstract class Command
    {
        public Unit CommandedEntity { get; }
        /// <summary>
        /// Factory that created this command.
        /// </summary>
        public ICommandFactory Creator { get; }
        /// <summary>
        /// Performs one step of the command. Returns true if command is finished.
        /// </summary>
        public abstract bool PerformCommand();
        public Command(Unit commandedEntity)
        {
            CommandedEntity = commandedEntity;
        }
    }

    /// <summary>
    /// Creates new instances of commands with parameters specified by the factory
    /// for the units that are using this factory. If the factory gets invalidated by
    /// the state of the game, it removes or replaces the commands for its units.
    /// </summary>
    public interface ICommandFactory
    {
        /// <summary>
        /// Units whose commands will be affected by invalidation.
        /// </summary>
        List<Unit> Units { get; }
        /// <summary>
        /// True if the command can no longer be executed.
        /// </summary>
        bool Invalid { get; }
        /// <summary>
        /// Sets Invalid to true if the invalidation condition is met.
        /// </summary>
        void CheckInvalidation();
        /// <summary>
        /// Replaces the invalidated commands.
        /// </summary>
        void ReplaceCommands();
        /// <summary>
        /// Creates a new instance of the command with given commanded entity. 
        /// </summary>
        Command NewInstance(Unit commandedEntity);
    }

    public class MoveTowardsCommandFactory : ICommandFactory
    {
        private Vector2 TargetPoint { get; }

        public List<Unit> Units { get; }

        public bool Invalid { get; private set; }

        public MoveTowardsCommandFactory(Vector2 targetPoint)
        {
            TargetPoint = targetPoint;
        }

        public Command NewInstance(Unit commandedEntity)
        {
            return new MoveTowardsCommand(commandedEntity, TargetPoint);
        }

        public void CheckInvalidation()
        {
            throw new NotImplementedException();
        }

        public void ReplaceCommands()
        {
            throw new NotImplementedException();
        }
    }

    public class MoveTowardsCommand : Command
    {
        public Vector2 TargetPoint;

        public MoveTowardsCommand(Unit commandedEntity, Vector2 targetPoint):base(commandedEntity)
        {
            TargetPoint = targetPoint;
        }

        public override bool PerformCommand()
        {
            Vector2 direction = CommandedEntity.Pos.UnitDirectionTo(TargetPoint);
            CommandedEntity.Accelerate(CommandedEntity.Acceleration*direction);
            return (TargetPoint - CommandedEntity.Pos).Length < 0.1f;
        }
    }

    public class MoveToCommandFactory : ICommandFactory
    {
        public List<Unit> Units { get; }

        public bool Invalid { get; }

        private Vector2 targetPos;
        private FlowMap flowMap;

        public MoveToCommandFactory(Vector2 target, Game game)
        {
            this.targetPos = target;
            flowMap = Pathfinding.GetPathfinding.GenerateFlowMap(game.Map.GetObstacleMap(),  target);
        }

        public void CheckInvalidation()
        {
            throw new NotImplementedException();
        }

        public Command NewInstance(Unit commandedEntity)
        {
            return new MoveToCommand(commandedEntity, targetPos, flowMap);
        }

        public void ReplaceCommands()
        {
            throw new NotImplementedException();
        }
    }

    public class MoveToCommand : Command
    {
        private FlowMap flowMap;
        private Vector2 targetPos;

        public MoveToCommand(Unit commandedEntity, Vector2 targetPos, FlowMap flowMap) : base(commandedEntity)
        {
            this.flowMap = flowMap;
            this.targetPos = targetPos;
        }

        public override bool PerformCommand()
        {
            CommandedEntity.Accelerate(flowMap.GetIntensity(CommandedEntity.Pos, CommandedEntity.Acceleration));
            return (CommandedEntity.Pos - targetPos).Length<0.5f;
        }
    }
}
