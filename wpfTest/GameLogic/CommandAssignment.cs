using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;

namespace wpfTest.GameLogic
{
    /// <summary>
    /// Creates new instances of commands with parameters specified by the factory
    /// for the units that are using this factory. If the factory gets invalidated by
    /// the state of the game, it removes or replaces the commands for its units.
    /// </summary>
    public abstract class CommandAssignment
    {
        /// <summary>
        /// Units whose commands will be affected by invalidation.
        /// </summary>
        public List<Unit> Units { get; }

        public CommandAssignment(List<Unit> units)
        {
            Units = units;
        }

        /// <summary>
        /// True if the command can no longer be executed.
        /// </summary>
        public bool Invalid { get; }
        /// <summary>
        /// Sets Invalid to true if the invalidation condition is met.
        /// </summary>
        public abstract void CheckInvalidation();
        /// <summary>
        /// Calculates information necessary to assign commands to units.
        /// </summary>
        public abstract void Process(Game game);
        /// <summary>
        /// Assigns commands to the units. All calculations that take long and can be pre-processed
        /// should be put into Process.
        /// </summary>
        public virtual void AssignCommands()
        {
            foreach (Unit u in Units)
            {
                Command com = NewInstance(u);
                com.Creator = this;
                u.AddCommand(com);
            }
        }
        /// <summary>
        /// Creates a new instance of the command with given commanded entity. 
        /// </summary>
        public abstract Command NewInstance(Unit commandedEntity);
    }

    public abstract class MovementCommandAssignment:CommandAssignment
    {
        protected float minStoppingDistance;

        public MovementCommandAssignment(List<Unit> units)
            : base(units)
        {
            float volume = 0f;
            foreach(Unit u in units)
            {
                volume += u.Range * u.Range;
            }
            minStoppingDistance = (float)Math.Sqrt(volume)*1.3f;
        }
    }

    public class MoveTowardsCommandAssignment : MovementCommandAssignment
    {
        private Vector2 TargetPoint { get; }
        private float endDistance;//distance where the unit stops moving

        public MoveTowardsCommandAssignment(List<Unit> units, Vector2 targetPoint, float endDistance = 0.1f)
            : base(units)
        {
            TargetPoint = targetPoint;
            this.endDistance = endDistance;
        }

        public override Command NewInstance(Unit commandedEntity)
        {
            return new MoveTowardsCommand(commandedEntity, TargetPoint, minStoppingDistance, endDistance);
        }

        public override void CheckInvalidation()
        {
            throw new NotImplementedException();
        }

        public override void Process(Game game)
        {
            //nothing needs to be processed
        }
    }

    public class MoveToCommandAssignment : MovementCommandAssignment
    {
        private Vector2 target;
        private float endDistance;//distance where the unit stops moving
        private FlowMap flowMap;

        public MoveToCommandAssignment(List<Unit> units, Vector2 target, float endDistance=1.42f)
            : base(units)
        {
            this.target = target;
            this.endDistance = endDistance;
        }

        public override void CheckInvalidation()
        {
            throw new NotImplementedException();
        }

        public override Command NewInstance(Unit commandedEntity)
        {
            return new MoveToCommand(commandedEntity, target, flowMap, minStoppingDistance, endDistance);
        }

        public override void Process(Game game)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //todo: separete units to different groups by their movement
            flowMap = Pathfinding.GetPathfinding.GenerateFlowMap(game.Map.GetObstacleMap(Movement.GROUND), target);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }

    public class AttackCommandAssignment : CommandAssignment
    {
        private Unit target;

        public AttackCommandAssignment(List<Unit> units, Unit target)
            : base(units)
        {
            this.target = target;
        }

        public override Command NewInstance(Unit commandedEntity)
        {
            return new AttackCommand(commandedEntity, target);
        }

        public override void CheckInvalidation()
        {
            throw new NotImplementedException();
        }

        public override void Process(Game game)
        {
            //nothing needs to be processed
        }
    }
}
