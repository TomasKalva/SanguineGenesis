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
        public IMovementTarget Target { get; }
        /// <summary>
        /// Movement type of the animals.
        /// </summary>
        public Movement Movement { get; }
        private bool active;
        /// <summary>
        /// True if any animals are currently using this command.
        /// </summary>
        public bool Active { get { lock (this) return active; } set { lock (this) active = value; } }
        private bool invalid;
        /// <summary>
        /// True if the target is on unreachable square or there are no units that are using this assignment. 
        /// If set to true, this command assignment will be removed and all its commands canceled. Can only 
        /// be used from the MovementGenerator and also from the game thread without locking, because it can 
        /// only be set to true and reading an incorrect value once has no negative effects.
        /// </summary>
        public bool Invalid { get { lock (this) return invalid; } set { lock (this) invalid = value; } }
        private FlowMap flowMap;
        private FlowMap FlowMap { get { lock (this) return flowMap; } set { lock (this) flowMap = value; } }

        public MoveToCommandAssignment(Players player, List<Animal> units, Movement movement, IMovementTarget target, float goalDistance=0.1f, bool interruptable=true)
        {
            Movement = movement;
            Active = false;
            Player = player;
            Animals = units;
            Target = target;
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

            Building targAsBuilding = Target as Building;
            Node targAsNode = Target as Node;
            if (targAsBuilding!=null || (targAsNode!=null/* && targAsNode.Building!=null*/))
            {
                Building blockingBuilding;
                if (targAsNode != null)
                {
                    //remove the obstacle from target node
                    forPathfinding[targAsNode.X, targAsNode.Y] = false;
                    //blockingBuilding = targAsNode.Building;
                }
                else
                {
                    //remove the building that is over the target on this obstacle map
                    blockingBuilding = targAsBuilding;

                    //Building's nodes are immutable
                    forPathfinding = new ObstacleMap(obst);
                    foreach (Node n in blockingBuilding.Nodes)
                    {
                        forPathfinding[n.X, n.Y] = false;
                    }
                }
                    
            }
            /*else
            {
                //if there is an obstacle on the target square, cancel this assignment
                if (obst[(int)Target.Center.X, (int)Target.Center.Y] ||
                    !Animals.Any())
                {
                    Invalid = true;
                    return;
                }
            }*/

            Stopwatch sw = new Stopwatch();
            sw.Start();
            //FlowMap flM= RayPathfinding.GetPathfinding.GenerateFlowMap(forPathfinding, Target.Center);
            FlowMap flM = new BfsPathfinding().GenerateFlowMap(forPathfinding, Target.Center);
            FlowMap = flM;
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Updates the flowmap of the commands. Used with the game locked.
        /// </summary>
        public virtual void UpdateCommands()
        {
            FlowMap flM = FlowMap;
            foreach (Animal a in Animals)
            {
                //find command from this assignment
                foreach(Command c in a.CommandQueue)
                {
                    if (c is MoveToCommand mtpc)
                        //update its flowmap
                        if (mtpc.Assignment == this)
                            mtpc.FlowMap= flM;

                    //update flowmaps of follow commands
                    if (c.FollowCommand != null &&
                        c.FollowCommand.Assignment == this)
                        c.FollowCommand.FlowMap = flM;
                }
            }
            //if there is an obstacle on the target square, cancel this assignment
            if (!Animals.Any())
            {
                Invalid = true;
                return;
            }
        }
    }
}
