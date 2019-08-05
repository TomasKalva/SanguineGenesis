using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;

namespace wpfTest.GameLogic
{
    public class MoveToCommandAssignment
    {
        /// <summary>
        /// Units whose commands should be updated.
        /// </summary>
        public List<Unit> Units { get; }
        /// <summary>
        /// Whose units are performing commands for this assignment.
        /// </summary>
        public Players Player { get; }
        public IMovementTarget TargetPoint { get; }
        public Movement Movement { get; }
        /// <summary>
        /// True if any units are currently using this command.
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// True if the target is on unreachable square or there are no units that are using this assignment. 
        /// If set to true, this command assignment will be removed and all its commands canceled. Can only 
        /// be used from the MovementGenerator and also from the game thread without locking, because it can 
        /// only be set to true and reading an incorrect value once has no negative effects.
        /// </summary>
        public bool Invalid { get; set; }

        private FlowMap flowMap;

        public MoveToCommandAssignment(Players player, List<Unit> units, Movement movement, IMovementTarget target, float goalDistance=0.1f, bool interruptable=true)
        {
            Movement = movement;
            Active = false;
            Player = player;
            Units = units;
            TargetPoint = target;
        }

        /// <summary>
        /// Generate flowmap for the give obstacle map obst.
        /// </summary>
        public void Process(ObstacleMap obst)
        {
            //if there are no more units, cancel this assignment
            if (!Units.Any())
            {
                Invalid = true;
                return;
            }

            //map used in the pathfinding algorithm
            ObstacleMap forPathfinding = obst;

            if (TargetPoint is Building b)
            {
                forPathfinding = new ObstacleMap(obst);
                foreach(Node n in b.Nodes)
                {
                    forPathfinding[n.X, n.Y] = false;
                }
            }
            else
            {
                //if there is an obstacle on the target square, cancel this assignment
                if (obst[(int)TargetPoint.Center.X, (int)TargetPoint.Center.Y] ||
                    !Units.Any())
                {
                    Invalid = true;
                    return;
                }
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            flowMap = Pathfinding.GetPathfinding.GenerateFlowMap(forPathfinding, TargetPoint.Center);
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
                    if (c is MoveToPointCommand mtpc)
                        //update its flowmap
                        if (mtpc.Assignment == this)
                            mtpc.UpdateFlowMap(flowMap);
                }
            }
        }
    }
}
