using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;

namespace wpfTest
{
    public class UnitFactory:EntityFactory
    {
        public float Range { get; }//range of the circle collider
        public float MaxSpeed { get; }
        public float Acceleration { get; }
        public Movement Movement { get; }
        public float SpawningTime { get; }
        public decimal AttackDamage { get; }
        public float AttackPeriod { get; }
        public float AttackDistance { get; }

        public Unit NewInstance(Player player, Vector2 pos)
        {
            return new Unit(player, EntityType, MaxHealth, MaxEnergy, pos, Movement, Range, ViewRange, MaxSpeed, Acceleration, AttackDamage, AttackPeriod, AttackDistance, Abilities.ToList());
        }

        public UnitFactory(EntityType unitType, decimal maxHealth, decimal maxEnergy, float range, bool physical, decimal energyCost,
            float viewRange, float maxSpeed, float acceleration, Movement movement, float spawningTime, decimal attackDamage, float attackPeriod, float attackDistance)
            :base(unitType, maxHealth, maxEnergy, physical, energyCost, viewRange)
        {
            Range = range;
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            Movement = movement;
            SpawningTime = spawningTime;
            AttackDamage = attackDamage;
            AttackPeriod = attackPeriod;
            AttackDistance = attackDistance;
        }
    }
}
