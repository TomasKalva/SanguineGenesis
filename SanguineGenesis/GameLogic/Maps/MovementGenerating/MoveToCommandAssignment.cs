using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GameLogic.Maps.MovementGenerating;

namespace SanguineGenesis.GameLogic.Maps.MovementGenerating
{
    class MoveToCommandAssignment
    {
        /// <summary>
        /// Animals whose commands should be updated.
        /// </summary>
        public List<Animal> Animals { get; }
        /// <summary>
        /// Whose animals are performing commands for this assignment.
        /// </summary>
        public FactionType Player { get; }
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
        /// True iff there are no animals that are using this assignment. 
        /// If set to true, this command assignment will be removed and all its commands canceled. Can only 
        /// be used from the MovementGenerator and also from the game thread without locking, because it can 
        /// only be set to true and reading an incorrect bool value once has no negative effects.
        /// </summary>
        public bool Invalid { get { lock (this) return invalid; } set { lock (this) invalid = value; } }
        private FlowField flowField;
        private FlowField FlowField { get { lock (this) return flowField; } set { lock (this) flowField = value; } }

        public MoveToCommandAssignment(FactionType player, List<Animal> units, Movement movement, IMovementTarget target)
        {
            Movement = movement;
            Active = false;
            Player = player;
            Animals = units;
            Target = target;
        }

        /// <summary>
        /// Generate flowfield for the given obstacle map obst.
        /// </summary>
        public void Process(ObstacleMap obst)
        {
            //if there are no more animals, cancel this assignment
            if (!Animals.Any())
            {
                Invalid = true;
                return;
            }

            //remove obstacles from the target node
            int targX = ((int)Target.Center.X)%obst.Width;
            int targY = ((int)Target.Center.Y)%obst.Height;
            bool removedObstacle = false;
            if(obst[targX, targY])
            {
                obst[targX, targY] = false;
                removedObstacle = true;
            }
            //remove obstacles created by building on the target node
            Building targAsBuilding = Target as Building;
            if (targAsBuilding != null)
            {
                //remove the building that is over the target on this obstacle map
                foreach (Node n in targAsBuilding.Nodes)
                {
                    //Building's nodes are immutable
                    obst[n.X, n.Y] = false;
                }
            }

            //calculate flow field
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //temporary variable is used because FlowField needs lock on this instance
            FlowField flF = new BfsPathfinding(obst, Target.Center).GenerateFlowField();
            FlowField = flF;
            sw.Stop();
            //simulate work if flow field calculation took too short time
            if (sw.ElapsedMilliseconds < 2)
                Thread.Sleep(10);

            //put removed obstacles back on the map
            if (removedObstacle)
            {
                obst[targX, targY] = true;
            }
            if(targAsBuilding != null)
            {
                foreach (Node n in targAsBuilding.Nodes)
                {
                    obst[n.X, n.Y] = true;
                }
            }
        }

        /// <summary>
        /// Updates the flowfield of the commands. Used with the game locked.
        /// </summary>
        public void UpdateCommands()
        {
            FlowField flF = FlowField;
            foreach (Animal a in Animals)
            {
                //find command from this assignment
                foreach(Command c in a.CommandQueue)
                {
                    if (c is MoveToCommand mtpc)
                        //update its flowfield
                        if (mtpc.Assignment == this)
                            mtpc.FlowField= flF;

                    //update flowfields of follow commands
                    if (c.FollowCommand != null &&
                        c.FollowCommand.Assignment == this)
                        c.FollowCommand.FlowField = flF;
                }
            }
        }
    }
}
