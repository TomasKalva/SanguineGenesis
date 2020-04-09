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
    sealed class EnterHole : Ability<Animal, Structure>
    {
        internal EnterHole(float energyCost, float enteringTime)
            : base(0.1f, energyCost, false, false, duration:enteringTime)
        {
        }

        public override bool ValidArguments(Animal caster, Structure target, ActionLog actionLog)
        {
            //target has to have underground status
            if(!target.Statuses.Where((s) => s is Underground).Any())
            {
                actionLog.LogError(caster, this, $"target doesn't have status {nameof(Underground)}");
                return false;
            }
            return true;
        }

        public override Command NewCommand(Animal caster, Structure target)
        {
            return new EnterHoleCommand(caster, target, this);
        }

        public override string GetName() => "ENTER_HOLE";

        public override string Description()
        {
            return "The animal enters the hole.";
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
                Underground underground = (Underground)Target.Statuses.Where((s) => s is Underground).FirstOrDefault();
                if (underground != null)
                {
                    //put the animal in the target hole
                    CommandedEntity.Faction.RemoveEntity(CommandedEntity);
                    underground.AnimalsUnderGround.Add(CommandedEntity);
                    CommandedEntity.StateChangeLock = underground;
                }
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Exit hole.
    /// </summary>
    sealed class ExitHole : Ability<Structure, Nothing>
    {
        internal ExitHole(float energyCost, float exitingTime)
            : base(0.1f, energyCost, false, false, duration:exitingTime)
        {
        }

        public override Command NewCommand(Structure caster, Nothing target)
        {
            return new ExitHoleCommand(caster, target, this);
        }

        public override string GetName() => "EXIT_HOLE";

        public override string Description()
        {
            return "All animals exit the hole the hole.";
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
                Underground underground = (Underground)CommandedEntity.Statuses.Where((s) => s is Underground).FirstOrDefault();
                if (underground != null)
                {
                    //put an animal out of the hole
                    Animal animalInHole = underground.AnimalsUnderGround.FirstOrDefault();
                    if (animalInHole != null)
                    {
                        animalInHole.Faction.AddEntity(animalInHole);
                        animalInHole.Position = new Vector2(CommandedEntity.Center.X, CommandedEntity.Bottom - animalInHole.Radius);
                        underground.AnimalsUnderGround.Remove(animalInHole);
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
