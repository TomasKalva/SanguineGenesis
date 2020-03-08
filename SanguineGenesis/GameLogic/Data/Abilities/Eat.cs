using SanguineGenesis.GameLogic.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Eat a node or tree to regenerate energy.
    /// </summary>
    sealed class HerbivoreEat : Ability<Animal, IHerbivoreFood>
    {
        internal HerbivoreEat()
            : base(0.1f, 0, false, false)
        {
        }

        public override Command NewCommand(Animal caster, IHerbivoreFood target)
        {
            return new HerbivoreEatCommand(caster, target, this);
        }

        /// <summary>
        /// Make sure that animals don't eat dirt.
        /// </summary>
        public override bool ValidArguments(Animal caster, IHerbivoreFood target)
        {
            if (target is Node tNode)
            {
                if (tNode.Biome == Biome.DEFAULT)
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string GetName() => "EAT";

        public override string Description()
        {
            return "The commanded herbivore eats tree or node.";
        }
    }

    class HerbivoreEatCommand : Command<Animal, IHerbivoreFood, HerbivoreEat>
    {
        public HerbivoreEatCommand(Animal commandedEntity, IHerbivoreFood target, HerbivoreEat eat)
            : base(commandedEntity, target, eat)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (!Target.FoodLeft)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }

            CommandedEntity.Direction = Target.Center - CommandedEntity.Center;

            CommandedEntity.CanBeMoved = false;
            if (ElapsedTime >= CommandedEntity.FoodEatingPeriod)
            {
                //eat
                Target.EatFood(CommandedEntity);
                //reset timer
                ElapsedTime -= CommandedEntity.FoodEatingPeriod;
            }

            return false;
        }

        public override int Progress => (int)(100 * (ElapsedTime / CommandedEntity.FoodEatingPeriod));
    }

    /// <summary>
    /// Eat a corpse to regenerate energy.
    /// </summary>
    sealed class CarnivoreEat : Ability<Animal, ICarnivoreFood>
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

        public override string GetName() => "EAT";

        public override string Description()
        {
            return "The commanded herbivore eats tree or node.";
        }
    }

    class CarnivoreEatCommand : Command<Animal, ICarnivoreFood, CarnivoreEat>
    {
        public CarnivoreEatCommand(Animal commandedEntity, ICarnivoreFood target, CarnivoreEat eat)
            : base(commandedEntity, target, eat)
        {
            this.ElapsedTime = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (!Target.FoodLeft)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }

            CommandedEntity.Direction = Target.Center - CommandedEntity.Center;

            CommandedEntity.CanBeMoved = false;
            if (ElapsedTime >= CommandedEntity.FoodEatingPeriod)
            {
                //eat
                Target.EatFood(CommandedEntity);
                //reset timer
                ElapsedTime -= CommandedEntity.FoodEatingPeriod;
            }

            return false;
        }

        public override int Progress => (int)(100 * (ElapsedTime / CommandedEntity.FoodEatingPeriod));
    }

    /// <summary>
    /// Marks classes that can be eaten by animals.
    /// </summary>
    interface IFood : ITargetable
    {
        bool FoodLeft { get; }
        void EatFood(Animal eater);
    }

    /// <summary>
    /// Marks classes that can be eaten by herbivores.
    /// </summary>
    interface IHerbivoreFood : IFood
    {
    }
    /// <summary>
    /// Marks classes that can be eaten by herbivores.
    /// </summary>
    interface ICarnivoreFood : IFood
    {
    }
}
