using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{
    public sealed class HerbivoreEat : TargetAbility<Animal, IHerbivoreFood>
    {
        internal HerbivoreEat()
            : base(0.1f, 0, false, false)
        {
        }

        public override Command NewCommand(Animal caster, IHerbivoreFood target)
        {
            return new HerbivoreEatCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string Description()
        {
            return "The commanded herbivore eats tree or node.";
        }
    }

    public class HerbivoreEatCommand : Command<Animal, IHerbivoreFood, HerbivoreEat>
    {
        /// <summary>
        /// Time in s until this animal eats.
        /// </summary>
        private float timeUntilEating;

        public HerbivoreEatCommand(Animal commandedEntity, IHerbivoreFood target, HerbivoreEat eat)
            : base(commandedEntity, target, eat)
        {
            this.timeUntilEating = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (!Targ.FoodLeft)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }

            CommandedEntity.Direction = Targ.Center - CommandedEntity.Center;

            CommandedEntity.CanBeMoved = false;
            timeUntilEating += deltaT;
            if (timeUntilEating >= CommandedEntity.FoodEatingPeriod)
            {
                //eat
                Targ.EatFood(CommandedEntity);
                //reset timer
                timeUntilEating -= CommandedEntity.FoodEatingPeriod;
            }

            return false;
        }
    }

    public sealed class CarnivoreEat : TargetAbility<Animal, ICarnivoreFood>
    {
        internal CarnivoreEat()
            : base(0.1f, 0, false, false)
        {
        }

        public override Command NewCommand(Animal caster, ICarnivoreFood target)
        {
            return new CarnivoreEatCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string Description()
        {
            return "The commanded herbivore eats tree or node.";
        }
    }

    public class CarnivoreEatCommand : Command<Animal, ICarnivoreFood, CarnivoreEat>
    {
        /// <summary>
        /// Time in s until this animal eats.
        /// </summary>
        private float timeUntilEating;

        public CarnivoreEatCommand(Animal commandedEntity, ICarnivoreFood target, CarnivoreEat eat)
            : base(commandedEntity, target, eat)
        {
            this.timeUntilEating = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (!Targ.FoodLeft)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }

            CommandedEntity.Direction = Targ.Center - CommandedEntity.Center;

            CommandedEntity.CanBeMoved = false;
            timeUntilEating += deltaT;
            if (timeUntilEating >= CommandedEntity.FoodEatingPeriod)
            {
                //eat
                Targ.EatFood(CommandedEntity);
                //reset timer
                timeUntilEating -= CommandedEntity.FoodEatingPeriod;
            }

            return false;
        }
    }

    /// <summary>
    /// Marks classes that can be eaten by animals.
    /// </summary>
    public interface IFood : ITargetable
    {
        bool FoodLeft { get; }
        void EatFood(Animal eater);
    }

    /// <summary>
    /// Marks classes that can be eaten by herbivores.
    /// </summary>
    public interface IHerbivoreFood : IFood
    {
    }
    /// <summary>
    /// Marks classes that can be eaten by herbivores.
    /// </summary>
    public interface ICarnivoreFood : IFood
    {
    }
}
