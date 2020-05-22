using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Statuses;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI;
using SanguineGenesis.GUI.WinFormsControls;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    /// <summary>
    /// Represents a dead animal. Can be used as food for carnivores.
    /// </summary>
    class Corpse:Unit, ICarnivoreFood, IDecayable
    {
        public bool Decayed { get; set; }
        public override bool IsDead => base.IsDead || Decayed;

        public Corpse(Faction faction, string corpseType, float maxHealth, float maxEnergy, Vector2 pos, float range)
            : base(faction, corpseType, maxHealth, 0, maxEnergy, new List<Ability>(), pos, range, false)
        {
            Decayed = false;
            Energy = maxEnergy;
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
                new Stat( "Health", Health.ToString("0.0")),
            };
            return stats;
        }
        #endregion IShowable

        public void Decay(float energyDamage)
        {
            Energy -= energyDamage;
            if (Energy <= 0)
                Decayed = true;
        }
    }

    /// <summary>
    /// Marks entities that can die when their energy drops to 0.
    /// </summary>
    interface IDecayable :IStatusOwner
    {
        /// <summary>
        /// If true, the entity should die.
        /// </summary>
        bool Decayed { get; set; }
        /// <summary>
        /// One step of decaying.
        /// </summary>
        void Decay(float energyDamage);
    }
}
