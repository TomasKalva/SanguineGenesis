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

        public Entity NewInstance(Players playerID, Vector2 pos)
        {
            return new Unit(playerID, UnitType, maxHealth:MaxHealth, maxEnergy:MaxEnergy, pos:pos, range:Range, maxSpeed:MaxSpeed, acceleration:Acceleration);
        }

        public UnitFactory(EntityType unitType, float range, float maxSpeed, float acceleration, float maxHealth, float maxEnergy)
        {
            Range = range;
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            MaxHealth = maxHealth;
            MaxEnergy = maxEnergy;
            UnitType = unitType;
        }
    }
}
