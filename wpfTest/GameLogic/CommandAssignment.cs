using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Abilities;
using wpfTest.GameLogic.Maps;

namespace wpfTest.GameLogic
{
    public class MoveToCommandAssignment
    {
        /// <summary>
        /// Animals whose commands should be updated.
        /// </summary>
        public List<Animal> Animals { get; }
        /// <summary>
        /// Whose animals are performing commands for this assignment.
        /// </summary>
        public Players Player { get; }
        /// <summary>
        /// Where the animals should go.
        /// </summary>
        public IMovementTarget TargetPoint { get; }
        /// <summary>
        /// Movement type of the animals.
        /// </summary>
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

        public MoveToCommandAssignment(Players player, List<Animal> units, Movement movement, IMovementTarget target, float goalDistance=0.1f, bool interruptable=true)
        {
            Movement = movement;
            Active = false;
            Player = player;
            Animals = units;
            TargetPoint = target;
        }

        /// <summary>
        /// Generate flowmap for the given obstacle map obst.
        /// </summary>
        public void Process(ObstacleMap obst)
        {
            //if there are no more units, cancel this assignment
            if (!Animals.Any())
            {
                Invalid = true;
                return;
            }

            //map used in the pathfinding algorithm
            ObstacleMap forPathfinding = obst;

            Building targAsBuilding = TargetPoint as Building;
            Node targAsNode = TargetPoint as Node;
            if (targAsBuilding!=null || (targAsNode!=null && targAsNode.Building!=null))
            {
                //remove the building that is over the target on this obstacle map
                Building blockingBuilding;
                if (targAsNode != null)
                    blockingBuilding = targAsNode.Building;
                else
                    blockingBuilding = targAsBuilding;


                forPathfinding = new ObstacleMap(obst);
                foreach(Node n in blockingBuilding.Nodes)
                {
                    forPathfinding[n.X, n.Y] = false;
                }
            }
            else
            {
                //if there is an obstacle on the target square, cancel this assignment
                if (obst[(int)TargetPoint.Center.X, (int)TargetPoint.Center.Y] ||
                    !Animals.Any())
                {
                    Invalid = true;
                    return;
                }
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            flowMap = RayPathfinding.GetPathfinding.GenerateFlowMap(forPathfinding, TargetPoint.Center);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Updates the flowmap of the commands.
        /// </summary>
        public virtual void UpdateCommands()
        {
            foreach (Animal a in Animals)
            {
                //find command from this assignment
                foreach(Command c in a.CommandQueue)
                {
                    if (c is MoveToCommand mtpc)
                        //update its flowmap
                        if (mtpc.Assignment == this)
                            mtpc.FlowMap=flowMap;
                }
            }
        }
    }
}
