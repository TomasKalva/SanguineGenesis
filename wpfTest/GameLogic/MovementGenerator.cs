using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class MovementGenerator
    {
        private static MovementGenerator movementGenerator;
        public static MovementGenerator GetMovementGenerator() => movementGenerator;
        
        static MovementGenerator()
        {
            movementGenerator = new MovementGenerator();
            movementGenerator.StartThread();
        }

        private Dictionary<Players, MovementGenerating> playersMovementGenerating;
        /// <summary>
        /// The player whose next command will be processed. Players switch after each iteration.
        /// </summary>
        private Players NextPlayer { get; set; }

        private class MovementGenerating
        {
            //commands that were not registered yet
            private List<MoveToCommandAssignment> newCommands;//lock is MovementGenerator
            //all currently active commands
            private List<MoveToCommandAssignment> commands;//isolated
            //true if there are commands in newCommands
            public bool AddedCommands { get; private set; }//lock is MovementGenerator
            //true if the visible part of map was changed
            public bool MapChanged { get; set; }//lock is MovementGenerator

            //inputs
            private List<MoveToCommandAssignment> inputs;//lock is MovementGenerator
            private Dictionary<Movement, ObstacleMap> newObstMaps;//lock is MovementGenerator
            private Dictionary<Movement, ObstacleMap> obstMaps;//isolated

            //outputs
            public List<MoveToCommandAssignment> Output { get; set; }//lock is MovementGenerator
            public bool Finished { get; private set; }//isolated

            public MovementGenerating()
            {
                inputs = new List<MoveToCommandAssignment>();
                Output = new List<MoveToCommandAssignment>();
                Finished = true;
                commands = new List<MoveToCommandAssignment>();
                newCommands = new List<MoveToCommandAssignment>();
                newObstMaps = new Dictionary<Movement, ObstacleMap>();
                newObstMaps.Add(Movement.LAND, null);
                newObstMaps.Add(Movement.WATER, null);
                newObstMaps.Add(Movement.LAND_WATER, null);
                obstMaps = new Dictionary<Movement, ObstacleMap>();
                obstMaps.Add(Movement.LAND, null);
                obstMaps.Add(Movement.WATER, null);
                obstMaps.Add(Movement.LAND_WATER, null);
            }

            public void UpdateInputs(object lockObj)
            {
                lock (lockObj)
                {
                    //update obstacle maps
                    foreach (Movement m in Enum.GetValues(typeof(Movement)))
                        obstMaps[m] = newObstMaps[m];

                    if (AddedCommands)
                    {
                        commands.AddRange(newCommands);
                        inputs.AddRange(newCommands);
                        newCommands.Clear();
                        AddedCommands = false;
                    }

                    //remove commands that don't need to be updated anymore
                    Console.WriteLine("active commands: " + commands.Count);
                    commands.RemoveAll((c) => c.Invalid);
                    Console.WriteLine("active commands: " + commands.Count);

                    if (MapChanged)
                    {
                        inputs = commands.ToList();//we don't want the same reference
                        MapChanged = false;
                    }

                    if (inputs.Any())
                        Finished = false;
                }
            }

            public MoveToCommandAssignment HighestPriorityCommand()
            {
                MoveToCommandAssignment comAss = inputs.FirstOrDefault();
                //highest priority command doesn't exist
                if (comAss == null)
                    return null;

                Priority highest = comAss.Active ? Priority.HIGH : Priority.LOW;
                foreach (MoveToCommandAssignment c in inputs)
                {
                    //no higher priority can be found
                    if (highest == Priority.HIGH)
                        break;

                    Priority p = c.Active ? Priority.HIGH : Priority.LOW;
                    if (p.HigherThan(highest))
                    {
                        highest = p;
                        comAss = c;
                    }
                }
                return comAss;
            }

            public Priority GetHighestPriority()
            {
                MoveToCommandAssignment c = HighestPriorityCommand();
                if (c == null)
                    return Priority.LOWEST;
                else
                    return c.Active ? Priority.HIGH : Priority.LOW;
            }

            /// <summary>
            /// Processes the command with the highest prioirty and puts it to the output.
            /// </summary>
            public void ProcessCommand(object lockObj)
            {
                MoveToCommandAssignment c = HighestPriorityCommand();
                inputs.Remove(c);
                if (!inputs.Any())
                    Finished = true;

                if (obstMaps[c.Movement] == null)
                    return;

                c.Process(obstMaps[c.Movement]);
                lock (lockObj)
                    Output.Add(c);
            }

            /// <summary>
            /// Sets the new obstacle maps.
            /// </summary>
            /// <param name="obstMaps"></param>
            public void SetObstMaps(Dictionary<Movement,ObstacleMap> obstMaps)
            {
                foreach(Movement m in Enum.GetValues(typeof(Movement)))
                    newObstMaps[m] = obstMaps[m];
            }

            public void AddNewCommand(MoveToCommandAssignment command)
            {
                newCommands.Add(command);
                AddedCommands = true;
            }
        }

        /// <summary>
        /// Starts a new thread for creating flow maps.
        /// </summary>
        public MovementGenerator()
        {
            playersMovementGenerating = new Dictionary<Players, MovementGenerating>();
            playersMovementGenerating.Add(Players.PLAYER0, new MovementGenerating());
            playersMovementGenerating.Add(Players.PLAYER1, new MovementGenerating());
        }

        private void StartThread()
        {
            Thread t = new Thread(() => Generate());
            t.IsBackground = true;
            t.Start();
        }
        
        /// <summary>
        /// Sets MapChanged for the player to true, and sets new obstacle maps.
        /// </summary>
        public void SetMapChanged(Players player, Dictionary<Movement,ObstacleMap> obstMaps)
        {
            MovementGenerating mg = playersMovementGenerating[player];
            lock (this)
            {
                mg.SetObstMaps(obstMaps);
                mg.MapChanged = true;
                Monitor.Pulse(this);
            }
        }

        /// <summary>
        /// Sets MapChanged for the player to true, and sets new obstacle maps.
        /// </summary>
        public void AddNewCommand(Players player, MoveToCommandAssignment command)
        {
            MovementGenerating mg = playersMovementGenerating[player];
            lock (this)
            {
                mg.AddNewCommand(command);
                Monitor.Pulse(this);
            }
        }
        /// <summary>
        /// Update commands with the newly created flowmaps.
        /// </summary>
        public void UseProcessedCommands()
        {
            lock (this)
            {
                MovementGenerating mg0 = playersMovementGenerating[Players.PLAYER0];
                MovementGenerating mg1 = playersMovementGenerating[Players.PLAYER1];
                foreach (MoveToCommandAssignment c in mg0.Output)
                {
                    c.UpdateCommands();
                }
                foreach (MoveToCommandAssignment c in mg1.Output)
                {
                    c.UpdateCommands();
                }
            }
        } 

        /// <summary>
        /// Infinite loop for generating movement maps.
        /// </summary>
        public void Generate()
        {
            //movement generatings for players don't change
            MovementGenerating mg0 = playersMovementGenerating[Players.PLAYER0];
            MovementGenerating mg1 = playersMovementGenerating[Players.PLAYER1];
            while (true)
            {
                //wait until at least one of the players needs to recalculate the commands
                lock (this)
                {
                    while (!mg0.MapChanged &&
                             !mg0.AddedCommands &&
                             !mg1.MapChanged &&
                             !mg1.AddedCommands) { Monitor.Wait(this); }
                }
                lock (this)
                {
                    //map doesn't change very often, so we don't check for it in the calculating cycle 
                    mg0.UpdateInputs(this);
                    mg1.UpdateInputs(this);
                }

                while (!mg0.Finished ||
                      !mg1.Finished)
                {
                    //determine which player's command will be processed next
                    Priority nextPr = playersMovementGenerating[NextPlayer].GetHighestPriority();
                    Priority oppPr = playersMovementGenerating[Opposite(NextPlayer)].GetHighestPriority();
                    if (oppPr.HigherThan(nextPr))
                        NextPlayer = Opposite(NextPlayer);

                    //process the command from NextPlayer with the highest priority
                    MovementGenerating nMg = playersMovementGenerating[NextPlayer];
                    nMg.ProcessCommand(this);

                    NextPlayer = Opposite(NextPlayer);
                }
            }
        }

        private Players Opposite(Players p)
        {
            switch (p)
            {
                case Players.PLAYER0: return Players.PLAYER1;
                default: return Players.PLAYER0;
            }
        }
    }

    enum Priority
    {
        HIGH,
        LOW,
        LOWEST
    }

    static class PriorityExtensions
    {
        public static bool HigherThan(this Priority p, Priority q)
        {
            switch (p)
            {
                case Priority.HIGH: return q !=Priority.HIGH;//no priority is higher than high
                case Priority.LOW: return q == Priority.LOWEST;//only high priority is higher than low
                case Priority.LOWEST: return false;//any priority other than lowest is higher than lowest
            }
            throw new NotImplementedException("Method " + nameof(HigherThan) + " is implemented incorrectly!");
        }
    }
}
