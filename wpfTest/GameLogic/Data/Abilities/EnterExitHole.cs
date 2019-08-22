using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Entities;

namespace wpfTest.GameLogic.Data.Abilities
{

    public sealed class EnterHole : TargetAbility<Animal, Structure>
    {
        public float EnteringTime { get; }

        internal EnterHole(decimal energyCost, float enteringTime)
            : base(0.1f, energyCost, false, false)
        {
            EnteringTime = enteringTime;
        }

        public override bool ValidArguments(Animal caster, Structure target)
        {
            //target has to have underground status
            return target.Statuses.Where((s) => s is Underground).Any();
        }

        public override Command NewCommand(Animal caster, Structure target)
        {
            return new EnterHoleCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal enters the hole.";
        }
    }

    public class EnterHoleCommand : Command<Animal, Structure, EnterHole>, IAnimalStateManipulator
    {
        private float timer;

        public EnterHoleCommand(Animal commandedEntity, Structure target, EnterHole enterHole)
            : base(commandedEntity, target, enterHole)
        {
            timer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            timer += deltaT;
            if (timer >= Ability.EnteringTime)
            {
                Underground underground = (Underground)Targ.Statuses.Where((s) => s is Underground).FirstOrDefault();
                if (underground != null)
                {
                    //put the animal in the target hole
                    CommandedEntity.Player.Entities.Remove(CommandedEntity);
                    underground.AnimalsUnderGround.Add(CommandedEntity);
                    CommandedEntity.StateChangeLock = underground;
                }
                return true;
            }

            return false;
        }
    }

    public sealed class ExitHole : TargetAbility<Structure, Nothing>
    {
        public float ExitingTime { get; }

        internal ExitHole(decimal energyCost, float exitingTime)
            : base(0.1f, energyCost, false, false)
        {
            ExitingTime = exitingTime;
        }

        public override Command NewCommand(Structure caster, Nothing target)
        {
            return new ExitHoleCommand(caster, target, this);
        }

        public override string Description()
        {
            return "All animals exit the hole the hole.";
        }
    }

    public class ExitHoleCommand : Command<Structure, Nothing, ExitHole>, IAnimalStateManipulator
    {
        private float timer;

        public ExitHoleCommand(Structure commandedEntity, Nothing target, ExitHole exitHole)
            : base(commandedEntity, target, exitHole)
        {
            timer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            timer += deltaT;
            if (timer >= Ability.ExitingTime)
            {
                timer -= Ability.ExitingTime;
                Underground underground = (Underground)CommandedEntity.Statuses.Where((s) => s is Underground).FirstOrDefault();
                if (underground != null)
                {
                    //put an animal out of the hole
                    Animal animalInHole = underground.AnimalsUnderGround.FirstOrDefault();
                    if (animalInHole != null)
                    {
                        animalInHole.Player.Entities.Add(animalInHole);
                        animalInHole.Position = new Vector2(CommandedEntity.Center.X, CommandedEntity.Bottom - animalInHole.Range);
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
