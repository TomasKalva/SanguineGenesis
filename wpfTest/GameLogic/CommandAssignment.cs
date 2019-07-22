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
    /// for the units that are using this factory. If the instace gets invalidated by
    /// the state of the game, it removes the commands from its units.
    /// </summary>
    public abstract class CommandAssignment
    {
        /// <summary>
        /// Units whose commands will be affected by invalidation.
        /// </summary>
        public List<Unit> Units { get; }
        /// <summary>
        /// Whose units are performing this command.
        /// </summary>
        public Players Player { get; }
        public CommandAssignment(Players player, List<Unit> units)
        {
            Player = player;
            Units = units;
        }

        /// <summary>
        /// True if the command can no longer be executed.
        /// </summary>
        public abstract bool Invalid(Game game);
        /// <summary>
        /// Removes all references to this instance if it is no longer valid.
        /// </summary>
        public virtual void DestroyIfInvalidated(Game game)
        {
            if(Invalid(game))
                Units.Clear();
        }
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

        public MovementCommandAssignment(Players player, List<Unit> units)
            : base(player,units)
        {
            float volume = 0f;
            foreach(Unit u in units)
            {
                volume += u.Range * u.Range;
            }
            minStoppingDistance = (float)Math.Sqrt(volume)*1.3f;
        }
    }

    /*public class MoveTowardsCommandAssignment : MovementCommandAssignment
    {
        public override bool Invalid(Game game)=> game.Players[Player].MapView[(int)target.X, (int)target.Y].Blocked;
        private Vector2 target;
        private float endDistance;//distance where the unit stops moving

        public MoveTowardsCommandAssignment(Players player, List<Unit> units, Vector2 targetPoint, float endDistance = 0.1f)
            : base(player, units)
        {
            target = targetPoint;
            this.endDistance = endDistance;
        }

        public override Command NewInstance(Unit commandedEntity)
        {
            return new MoveTowardsCommand(commandedEntity, target, minStoppingDistance, endDistance);
        }
    }*/

    public class MoveToCommandAssignment : MovementCommandAssignment
    {
        public override bool Invalid(Game game) => game.Players[Player].MapView[(int)target.X, (int)target.Y].Blocked;
        private Vector2 target;
        private float goalDistance;//distance where the unit stops moving
        private bool usesAttackDistance;
        private FlowMap flowMap;
        public Movement Movement { get; }
        /// <summary>
        /// True if there are units that are currently using this command.
        /// </summary>
        public bool Active { get; set; }

        public MoveToCommandAssignment(Players player, List<Unit> units, Vector2 target, Movement movement, float goalDistance=0.1f,
            bool usesAttackDistance=false)
            : base(player,units)
        {
            this.target = target;
            this.goalDistance = goalDistance;
            this.usesAttackDistance = usesAttackDistance;
            Movement = movement;
            Active = false;
        }

        public override Command NewInstance(Unit commandedEntity)
        {
            return new MoveToCommand(commandedEntity, target, null, minStoppingDistance, goalDistance);
        }

        public void Process(ObstacleMap obst)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //todo: separete units to different groups by their movement
            flowMap = Pathfinding.GetPathfinding.GenerateFlowMap(obst, target);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Updates the flowmap of the commands.
        /// </summary>
        public virtual void UpdateCommands()
        {
            foreach (Unit u in Units)
            {
                //find command from this assignment
                foreach(Command c in u.CommandQueue)
                {
                    //update its flow map
                    if (c.Creator == this)
                        ((MoveToCommand)c).UpdateFlowMap(flowMap);
                }
            }
        }
    }

    

    public class AttackCommandAssignment : CommandAssignment
    {
        private Unit target;
        public override bool Invalid(Game game) => target.IsDead;//dead units are no longer in the game

        public AttackCommandAssignment(Players player, List<Unit> units, Unit target)
            : base(player, units)
        {
            this.target = target;
        }

        public override Command NewInstance(Unit commandedEntity)
        {
            return new AttackCommand(commandedEntity, target);
        }
    }
}
