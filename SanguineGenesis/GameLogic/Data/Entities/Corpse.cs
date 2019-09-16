﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Abilities;
using wpfTest.GameLogic.Maps;
using wpfTest.GUI;
using static wpfTest.MainWindow;

namespace wpfTest.GameLogic.Data.Entities
{
    /// <summary>
    /// Represents a dead animal. Can be used as food for carnivores.
    /// </summary>
    class Corpse:Unit, ICarnivoreFood
    {
        public Corpse(Player player, string corpseType, decimal maxHealth, decimal maxEnergy, Vector2 pos, float range)
            : base(player, corpseType, maxHealth, 0, maxEnergy, new List<Ability>(), pos, range, false)
        {
        }
        
        /// <summary>
        /// Returns true if the entity is visible on visibilityMap.
        /// </summary>
        public override bool IsVisible(VisibilityMap visibilityMap)
        {
            return true;
        }

        #region IFood
        bool IFood.FoodLeft => !IsDead;
        void IFood.EatFood(Animal eater)
        {
            decimal nutrientsToEat = Math.Min(eater.FoodEnergyRegen, Health);
            Health -= nutrientsToEat;
            eater.Energy += nutrientsToEat;
        }
        #endregion IFood

        #region IShowable
        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "player", Player.ToString()),
                new Stat( "EntityType", EntityType),
                new Stat( "Health", Health.ToString()),
            };
            return stats;
        }
        #endregion IShowable
    }
}