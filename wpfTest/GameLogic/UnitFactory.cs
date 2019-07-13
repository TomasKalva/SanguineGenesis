using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    public class UnitFactory
    {
        public float Range { get; }//range of the circle collider
        public float MaxSpeed { get; }
        public float Acceleration { get; }

        public Unit NewInstance(Players playerID, Vector2 pos)
        {
            return new Unit(playerID, pos, range:Range, maxSpeed:MaxSpeed, acceleration:Acceleration);
        }

        public UnitFactory(float range, float maxSpeed, float acceleration)
        {
            Range = range;
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
        }
    }
}
