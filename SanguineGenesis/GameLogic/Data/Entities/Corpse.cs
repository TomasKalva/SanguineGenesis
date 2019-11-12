using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    /// <summary>
    /// Represents a dead animal. Can be used as food for carnivores.
    /// </summary>
    class Corpse:Unit, ICarnivoreFood
    {
        public Corpse(Faction faction, string corpseType, float maxHealth, float maxEnergy, Vector2 pos, float range)
            : base(faction, corpseType, maxHealth, 0, maxEnergy, new List<Ability>(), pos, range, false)
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
            float nutrientsToEat = Math.Min(eater.FoodEnergyRegen, Health);
            Health -= nutrientsToEat;
            eater.Energy += nutrientsToEat;
        }
        #endregion IFood

        #region IShowable
        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Faction", Faction.FactionID.ToString()),
                new Stat( "EntityType", EntityType),
                new Stat( "Health", Health.ToString()),
            };
            return stats;
        }
        #endregion IShowable
    }
}
