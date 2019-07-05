using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    public class Unit
    {
        public Vector2 Pos { get; set; }
        public Vector2 Vel { get; set; }
        public float Range { get; }//range of the circle collider
        public bool WantsToMove => false;//true if the unit has a target destination
        public bool IsInCollision { get; set; }//true if the unit is colliding with obstacles or other units
        public float MaxSpeed { get; }
        public CommandsGroup Group { get; set; }

        public Unit(float x, float y, float size=0.5f)
        {
            Pos = new Vector2(x, y);
            Range = size;
            IsInCollision = false;
            MaxSpeed = 2f;
            Group = null;
        }

        public void Move(Map map, float deltaT)
        {
            Pos = new Vector2( 
                Math.Max(Range, Math.Min(Pos.X + deltaT * Vel.X,map.Width-Range)),
                Math.Max(Range, Math.Min(Pos.Y + deltaT * Vel.Y, map.Height-Range)));
        }
        
        public float GetActualBottom(float imageBottom)
            => Math.Min(Pos.Y - Range, Pos.Y + imageBottom);
        public float GetActualTop(float imageHeight, float imageBottom)
            => Math.Max(Pos.Y + Range, Pos.Y + imageBottom+imageHeight);
        public float GetActualLeft(float imageLeft)
            => Math.Min(Pos.X - Range, Pos.X + imageLeft);
        public float GetActualRight(float imageWidth, float imageLeft)
            => Math.Max(Pos.X + Range, Pos.X + imageLeft + imageWidth);

        public float Left => Pos.X - Range;
        public float Right => Pos.X + Range;
        public float Bottom => Pos.Y - Range;
        public float Top => Pos.Y + Range;

        public void Accelerate(Vector2 acc)
        {
            Vel += acc;
            float l;
            if ((l=Vel.Length)>MaxSpeed)
                Vel = (MaxSpeed / l)*Vel;
        }
    }
}
