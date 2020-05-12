using SanguineGenesis.GameLogic.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Maps.MovementGenerating
{
    /// <summary>
    /// Generates movement flowfields in other thread.
    /// </summary>
    class MovementGenerator
    {
        private static readonly MovementGenerator movementGenerator;
        public static MovementGenerator GetMovementGenerator() => movementGenerator;
        
        static MovementGenerator()
        {
            movementGenerator = new MovementGenerator();
            movementGenerator.StartThread();
        }

        private readonly Dictionary<FactionType, PlayerMovementGenerator> playersMovementGenerators;
        /// <summary>
        /// The player whose next command will be processed. Players switch after each iteration.
        /// </summary>
        private FactionType NextPlayer { get; set; }
        /// <summary>
        /// If set to true, all command assignments are removed during the next iteration.
        /// </summary>
        private bool reset;
        /// <summary>
        /// Sets reset to true.
        /// </summary>
        public void Reset()
        {
            lock (this)
            {
                reset = true;
            }
        }

        /// <summary>
        /// Calculates movement flowfields for one player.
        /// </summary>
        private class PlayerMovementGenerator
        {
            //Inputs that can be changed from other threads.
            /// <summary>
            /// Commands that were not registered yet. Lock is MovementGenerator.
            /// </summary>
            private readonly List<MoveToCommandAssignment> newCommands;
            /// <summary>
            /// Obstacle maps that were not registered yet. Lock is MovementGenerator.
            /// </summary>
            private readonly Dictionary<Movement, ObstacleMap> newObstMaps;
            /// <summary>
            /// Set to true after assignments are added to newCommands. Lock is MovementGenerator.
            /// </summary>
            public bool AddedCommands { get; private set; }
            /// <summary>
            /// Set to true after newObstMaps was changed. Lock is MovementGenerator.
            /// </summary>
            public bool MapChanged { get; set; }
            /// <summary>
            /// Outputs of the algorithm. Lock is MovementGenerator.
            /// </summary>
            public List<MoveToCommandAssignment> Output { get; set; }

            //These fields are set at the start of new cycle from the input from outside.
            //They are isolated during the algorithm.
            /// <summary>
            /// All valid MoveToCommandAssignments for this player in the game.
            /// </summary>
            private readonly List<MoveToCommandAssignment> commands;
            /// <summary>
            /// MoveToCommandAssignments that will be recalculated in this cycle.
            /// </summary>
            private List<MoveToCommandAssignment> inputs;
            /// <summary>
            /// MoveToCommandAssignments that will be calculated repeatedly in this cycle.
            /// </summary>
            private readonly List<MoveToCommandAssignment> repeatedInputs;
            /// <summary>
            /// Obstacle maps used in this cycle.
            /// </summary>
            private readonly Dictionary<Movement, ObstacleMap> obstMaps;
            /// <summary>
            /// What kind of command assignment is currently calculated.
            /// </summary>
            public Work CurrentWork { get; private set; }

            public PlayerMovementGenerator()
            {
                inputs = new List<MoveToCommandAssignment>();
                repeatedInputs = new List<MoveToCommandAssignment>();
                Output = new List<MoveToCommandAssignment>();
                CurrentWork = Work.NOTHING;
                commands = new List<MoveToCommandAssignment>();
                newCommands = new List<MoveToCommandAssignment>();
                newObstMaps = new Dictionary<Movement, ObstacleMap>
                {
                    { Movement.LAND, null },
                    { Movement.WATER, null },
                    { Movement.LAND_WATER, null }
                };
                obstMaps = new Dictionary<Movement, ObstacleMap>
                {
                    { Movement.LAND, null },
                    { Movement.WATER, null },
                    { Movement.LAND_WATER, null }
                };
            }

            /// <summary>
            /// Is called at the start of new cycle. Sets input using newCommands, resets
            /// newCommands and repeatedInput.
            /// </summary>
            public void StartNewCycle(object movementGenerator)
            {
                lock (movementGenerator)
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
                    commands.RemoveAll((c) => c.Invalid);

                    if (MapChanged)
                    {
                        inputs = commands.ToList();//we don't want the same reference
                        repeatedInputs.Clear();
                        MapChanged = false;
                    }

                    if (inputs.Any())
                        CurrentWork = Work.INPUTS;
                }
            }

            /// <summary>
            /// Resets the command assignment queue.
            /// </summary>
            public void Reset()
            {
                lock (this)
                {
                    Output.Clear();
                    inputs.Clear();
                    repeatedInputs.Clear();
                    commands.Clear();
                    CurrentWork = Work.NOTHING;
                }
            }

            /// <summary>
            /// Returns command assignment with the highest priority.
            /// </summary>
            public MoveToCommandAssignment HighestPriorityAssignment()
            {
                MoveToCommandAssignment comAssignment = GetCommAssWithHighestPriority(inputs);

                if(comAssignment!=null)
                    return comAssignment;
                else
                    return GetCommAssWithHighestPriority(repeatedInputs);
            }

            /// <summary>
            /// Returns command assignment from assignments with the highest priority.
            /// </summary>
            private MoveToCommandAssignment GetCommAssWithHighestPriority(List<MoveToCommandAssignment> assignments)
            {
                MoveToCommandAssignment comAssignment = assignments.FirstOrDefault();
                //highest priority command doesn't exist
                if (comAssignment == null)
                    return null;

                Priority highest = comAssignment.Active ? Priority.HIGH : Priority.LOW;
                foreach (MoveToCommandAssignment c in assignments)
                {
                    //no higher priority can be found
                    if (highest == Priority.HIGH)
                        break;

                    Priority p = c.Active ? Priority.HIGH : Priority.LOW;
                    if (p.HigherThan(highest))
                    {
                        highest = p;
                        comAssignment = c;
                    }
                }
                return comAssignment;
            }

            /// <summary>
            /// Returns priority of the command with the highest priority.
            /// Returns no commands if there are no commands.
            /// </summary>
            public Priority GetHighestPriority()
            {
                MoveToCommandAssignment c = HighestPriorityAssignment();
                if (c == null)
                    return Priority.NO_COMMANDS;
                else
                    return c.Active ? Priority.HIGH : Priority.LOW;
            }

            /// <summary>
            /// Processes the command with the highest prioirty and puts it to the output.
            /// </summary>
            public void ProcessCommand(object movementGenerator)
            {
                MoveToCommandAssignment current = HighestPriorityAssignment();
                //remove command from the corresponding list
                if(inputs.Contains(current))
                    inputs.Remove(current);
                else
                    repeatedInputs.Remove(current);

                //animal targets can move so the flowfield needs to be repeatedly recalculated
                if (current.Target is Animal && current.Animals.Any())
                    repeatedInputs.Add(current);

                //set current state of work
                if (!inputs.Any())
                {
                    if (!repeatedInputs.Any())
                        CurrentWork = Work.NOTHING;
                    else
                        CurrentWork = Work.REPEATED_INPUTS;
                }

                //don't do anything if obstacle map for the movement doesn't exist
                if (obstMaps[current.Movement] == null)
                    return;

                //process current command assignment
                current.Process(obstMaps[current.Movement]);

                //put the command assignment to outputs
                lock (movementGenerator)
                    Output.Add(current);
            }

            /// <summary>
            /// Sets the newObstMaps.
            /// </summary>
            public void SetObstMaps(Dictionary<Movement,ObstacleMap> obstMaps)
            {
                foreach(Movement m in Enum.GetValues(typeof(Movement)))
                    newObstMaps[m] = obstMaps[m];
            }

            /// <summary>
            /// Adds command to newCommands.
            /// </summary>
            public void AddNewCommand(MoveToCommandAssignment command)
            {
                newCommands.Add(command);
                AddedCommands = true;
            }
        }

        public MovementGenerator()
        {
            playersMovementGenerators = new Dictionary<FactionType, PlayerMovementGenerator>
            {
                { FactionType.PLAYER0, new PlayerMovementGenerator() },
                { FactionType.PLAYER1, new PlayerMovementGenerator() }
            };
        }

        /// <summary>
        /// Starts a new thread for creating flow maps.
        /// </summary>
        private void StartThread()
        {
            Thread t = new Thread(() => Generate())
            {
                IsBackground = true
            };
            t.Start();
        }
        
        /// <summary>
        /// Sets MapChanged for the player to true, and sets new obstacle maps.
        /// </summary>
        public void SetMapChanged(FactionType player, Dictionary<Movement,ObstacleMap> obstMaps)
        {
            PlayerMovementGenerator mg = playersMovementGenerators[player];
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
        public void AddNewCommand(FactionType player, MoveToCommandAssignment command)
        {
            PlayerMovementGenerator mg = playersMovementGenerators[player];
            lock (this)
            {
                mg.AddNewCommand(command);
                Monitor.Pulse(this);
            }
        }
        /// <summary>
        /// Update commands with the newly created flowfields.
        /// </summary>
        public void UseProcessedCommands()
        {
            lock (this)
            {
                PlayerMovementGenerator mg0 = playersMovementGenerators[FactionType.PLAYER0];
                PlayerMovementGenerator mg1 = playersMovementGenerators[FactionType.PLAYER1];
                foreach (MoveToCommandAssignment c in mg0.Output)
                {
                    c.UpdateCommands();
                }
                mg0.Output.RemoveAll(ca => ca.Invalid);
                foreach (MoveToCommandAssignment c in mg1.Output)
                {
                    c.UpdateCommands();
                }
                mg1.Output.RemoveAll(ca => ca.Invalid);
            }
        } 

        /// <summary>
        /// Infinite loop for generating movement maps.
        /// </summary>
        public void Generate()
        {
            //movement generatings for players don't change
            PlayerMovementGenerator mg0 = playersMovementGenerators[FactionType.PLAYER0];
            PlayerMovementGenerator mg1 = playersMovementGenerators[FactionType.PLAYER1];
            while (true)
            {
                //wait until at least one of the players needs to recalculate the commands
                lock (this)
                {
                    while (!StartNewCycle()) 
                    {
                        TryReset();
                        Monitor.Wait(this); 
                    }
                }
                lock (this)
                {
                    //map doesn't change very often, so we don't check for the change in the calculation cycle 
                    mg0.StartNewCycle(this);
                    mg1.StartNewCycle(this);
                }

                //calculation cycle
                while (!CycleFinished())
                {
                    //determine which player's command will be processed next
                    Priority nextPr = playersMovementGenerators[NextPlayer].GetHighestPriority();
                    Priority oppPr = playersMovementGenerators[Next(NextPlayer)].GetHighestPriority();
                    if (oppPr.HigherThan(nextPr))
                        NextPlayer = Next(NextPlayer);

                    //process the command from NextPlayer with the highest priority
                    PlayerMovementGenerator nMg = playersMovementGenerators[NextPlayer];
                    nMg.ProcessCommand(this);
                    
                    NextPlayer = Next(NextPlayer);

                    TryReset();
                }
            }
        }

        /// <summary>
        /// Resets this instance, if reset is true.
        /// </summary>
        private void TryReset()
        {
            PlayerMovementGenerator mg0 = playersMovementGenerators[FactionType.PLAYER0];
            PlayerMovementGenerator mg1 = playersMovementGenerators[FactionType.PLAYER1];
            lock (this)
                if (reset)
                {
                    //reset the command assignment queues for both players
                    mg0.Reset();
                    mg1.Reset();
                    reset = false;
                }
        }

        /// <summary>
        /// Returns true iff the calculation cycle should finish.
        /// </summary>
        private bool CycleFinished()
        {
            PlayerMovementGenerator mg0 = playersMovementGenerators[FactionType.PLAYER0];
            PlayerMovementGenerator mg1 = playersMovementGenerators[FactionType.PLAYER1];
            if (mg0.CurrentWork == Work.NOTHING && mg1.CurrentWork == Work.NOTHING)
                //cycle is finished if both mg0 and mg1 are doing nothing
                return true;
            else if (mg0.CurrentWork == Work.INPUTS || mg1.CurrentWork == Work.INPUTS)
                //cycle isn't finished if at least one of mg0 and mg1 is working on inputs
                return false;
            else
            {
                //mg0 and mg1 can only be working on repeated inputs - finish if there are no
                //new regular inputs waiting to be calculated in the next cycle
                if (StartNewCycle())
                    return true;
                else
                    return false;

            }

        }

        /// <summary>
        /// Returns true iff new calculation cycle should begin.
        /// </summary>
        private bool StartNewCycle()
        {
            PlayerMovementGenerator mg0 = playersMovementGenerators[FactionType.PLAYER0];
            PlayerMovementGenerator mg1 = playersMovementGenerators[FactionType.PLAYER1];
            lock (this)
            {
                return (mg0.MapChanged ||
                        mg0.AddedCommands ||
                        mg1.MapChanged ||
                        mg1.AddedCommands);
            }
        }


        /// <summary>
        /// Returns the player that comes after p.
        /// </summary>
        private FactionType Next(FactionType p)
        {
            switch (p)
            {
                case FactionType.PLAYER0: return FactionType.PLAYER1;
                default: return FactionType.PLAYER0;
            }
        }
    }

    /// <summary>
    /// Priority of CommandAssignment and PlayerMovementGenerator.
    /// </summary>
    enum Priority
    {
        HIGH,
        LOW,
        NO_COMMANDS
    }

    /// <summary>
    /// Type of work that PlayerMovementGenerator does.
    /// </summary>
    enum Work
    {
        NOTHING,
        INPUTS,
        REPEATED_INPUTS
    }

    static class PriorityExtensions
    {
        public static bool HigherThan(this Priority p, Priority q)
        {
            switch (p)
            {
                case Priority.HIGH: return q !=Priority.HIGH;//no priority is higher than high
                case Priority.LOW: return q == Priority.NO_COMMANDS;//only high priority is higher than low
                case Priority.NO_COMMANDS: return false;//any priority other than NO_COMMANDS is higher than NO_COMMANDS
            }
            throw new NotImplementedException("Method " + nameof(HigherThan) + " doesn't cover the case where p="+p+" and q=" + q+"!");
        }
    }
}
