using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;
using SanguineGenesis.GameLogic.Maps;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Enter hole.
    /// </summary>
    class EnterHole : Ability<Animal, Structure>
    {
        internal EnterHole(float energyCost, float enteringTime)
            : base(0.1f, energyCost, false, false, duration:enteringTime)
        {
        }

        public override bool ValidArguments(Animal user, Structure target, ActionLog actionLog)
        {
            //target has to have hole system status
            if(!target.Statuses.Where((s) => s is HoleSystem).Any())
            {
                actionLog.LogError(user, this, $"target doesn't have status {nameof(HoleSystem)}");
                return false;
            }
            return true;
        }

        public override Command NewCommand(Animal user, Structure target)
        {
            return new EnterHoleCommand(user, target, this);
        }

        public override string GetName() => "ENTER_HOLE";

        public override string Description()
        {
            return "The animal enters HOLE.";
        }
    }

    class EnterHoleCommand : Command<Animal, Structure, EnterHole>, IAnimalStateManipulator
    {
        public EnterHoleCommand(Animal commandedEntity, Structure target, EnterHole enterHole)
            : base(commandedEntity, target, enterHole)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (ElapsedTime >= Ability.Duration)
            {
                HoleSystem holeSystem = (HoleSystem)Target.Statuses.Where((s) => s is HoleSystem).FirstOrDefault();
                if (holeSystem != null)
                {
                    //put the animal in the target hole
                    CommandedEntity.Faction.RemoveEntity(CommandedEntity);
                    holeSystem.AnimalsInHole.Add(CommandedEntity);
                    CommandedEntity.StateChangeLock = holeSystem;
                }
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Exit hole.
    /// </summary>
    class ExitHole : Ability<Structure, Nothing>
    {
        internal ExitHole(float energyCost, float exitingTime)
            : base(0.1f, energyCost, false, false, duration:exitingTime)
        {
        }

        public override Command NewCommand(Structure user, Nothing target)
        {
            return new ExitHoleCommand(user, target, this);
        }

        public override string GetName() => "EXIT_HOLE";

        public override string Description()
        {
            return "All animals exit the hole.";
        }
    }

    class ExitHoleCommand : Command<Structure, Nothing, ExitHole>, IAnimalStateManipulator
    {
        public ExitHoleCommand(Structure commandedEntity, Nothing target, ExitHole exitHole)
            : base(commandedEntity, target, exitHole)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (ElapsedTime >= Ability.Duration)
            {
                ElapsedTime -= Ability.Duration;
                HoleSystem holeSystem = (HoleSystem)CommandedEntity.Statuses.Where((s) => s is HoleSystem).FirstOrDefault();
                if (holeSystem != null)
                {
                    //put an animal out of the hole
                    Animal animalInHole = holeSystem.AnimalsInHole.FirstOrDefault();
                    if (animalInHole != null)
                    {
                        animalInHole.Faction.AddEntity(animalInHole);
                        animalInHole.Position = new Vector2(CommandedEntity.Center.X, CommandedEntity.Bottom - animalInHole.Radius);
                        holeSystem.AnimalsInHole.Remove(animalInHole);
                        animalInHole.StateChangeLock = null;
                        return false;
                    }
                    else
                        //no more animals to put out of holes
                        return true;
                }
            }

            return false;
        }
    }
}
