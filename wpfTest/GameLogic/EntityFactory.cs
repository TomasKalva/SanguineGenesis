using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public abstract class EntityFactory
    {
        public EntityType EntityType { get; }
        public decimal MaxHealth { get; }
        public decimal MaxEnergy { get; }
        public bool Physical { get; }
        public decimal EnergyCost { get; }
        public float ViewRange { get; }
        public List<Ability> Abilities { get; }

        public EntityFactory(EntityType entityType, decimal maxHealth, decimal maxEnergy,
            bool physical, decimal energyCost, float viewRange)
        {
            EntityType = entityType;
            MaxHealth = maxHealth;
            MaxEnergy = maxEnergy;
            Physical = physical;
            EnergyCost = energyCost;
            ViewRange = viewRange;
            Abilities = new List<Ability>();
        }

    }
}
