using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    public abstract class Command
    {
        public Unit CommandedEntity { get; }
        /// <summary>
        /// Performs one step of the command. Returns true if command is finished.
        /// </summary>
        /// <returns></returns>
        public abstract bool PerformCommand();
        public Command(Unit commandedEntity)
        {
            CommandedEntity = commandedEntity;
        }
    }

    public interface ICommandFactory
    {
        Command NewInstance(Unit unit);
    }

    public class MoveTowardsCommandFactory : ICommandFactory
    {
        private Vector2 TargetPoint { get; }

        public MoveTowardsCommandFactory(Vector2 targetPoint)
        {
            TargetPoint = targetPoint;
        }

        public Command NewInstance(Unit unit)
        {
            return new MoveTowardsCommand(unit, TargetPoint);
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
            return (TargetPoint - CommandedEntity.Pos).Length < 2f;
        }
    }
}
