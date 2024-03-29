﻿using SanguineGenesis.GameLogic.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Eat a node or plant to regenerate energy.
    /// </summary>
    class HerbivoreEat : Ability<Animal, IHerbivoreFood>
    {
        internal HerbivoreEat()
            : base(0.1f, 0, false, false)
        {
        }

        public override Command NewCommand(Animal user, IHerbivoreFood target)
        {
            return new HerbivoreEatCommand(user, target, this);
        }

        /// <summary>
        /// Make sure that animals don't eat dirt and target has food.
        /// </summary>
        public override bool ValidArguments(Animal user, IHerbivoreFood target, ActionLog actionLog)
        {
            if ((target is Node tNode &&
                tNode.Biome == Biome.DEFAULT) ||
                !target.FoodLeft)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string GetName() => "HERBIVORE_EAT";

        public override string Description()
        {
            return "The commanded herbivore eats plant or node.";
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
    class CarnivoreEat : Ability<Animal, ICarnivoreFood>
    {
        internal CarnivoreEat()
            : base(0.1f, 0, false, false)
        {
        }

        public override Command NewCommand(Animal user, ICarnivoreFood target)
        {
            return new CarnivoreEatCommand(user, target, this);
        }

        public override bool ValidArguments(Animal user, ICarnivoreFood target, ActionLog actionLog)
        {
            //check if there is food left
            if (!target.FoodLeft)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string GetName() => "CARNIVORE_EAT";

        public override string Description()
        {
            return "The commanded carnivore eats plant or node.";
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
