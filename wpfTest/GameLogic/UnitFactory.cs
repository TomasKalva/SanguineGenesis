using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;

namespace wpfTest
{
    public class UnitFactory
    {
        public EntityType UnitType { get; }
        public float Range { get; }//range of the circle collider
        public float MaxSpeed { get; }
        public float Acceleration { get; }
        public float MaxHealth { get; }
        public float MaxEnergy { get; }
        public Movement Movement { get; }
        public float SpawningTime { get; }

        public Unit NewInstance(Player player, Vector2 pos)
        {
            return new Unit(player, UnitType, maxHealth:MaxHealth, maxEnergy:MaxEnergy, pos:pos, range:Range, maxSpeed:MaxSpeed, acceleration:Acceleration,movement:Movement);
        }

        public UnitFactory(EntityType unitType, float range, float maxSpeed, float acceleration, float maxHealth, float maxEnergy, Movement movement, float spawningTime)
        {
            Range = range;
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            MaxHealth = maxHealth;
            MaxEnergy = maxEnergy;
            UnitType = unitType;
            Movement = movement;
            SpawningTime = spawningTime;
        }
    }
}
