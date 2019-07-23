using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GUI;

namespace wpfTest.GameLogic
{
    public class Unit:Entity
    {
        public override Vector2 Pos { get; set; }
        public Vector2 Vel { get; set; }
        public override float Size => 2 * Range;
        public float Range { get; }//range of the circle collider
        public bool CanBeMoved { get; set; }//false if the unit has to stand still
        public bool StopMoving { get; set; }//set to true to set WantsToMove to false after Move
        public bool WantsToMove { get; set; }//true if the unit has a target destination
        public bool IsInCollision { get; set; }//true if the unit is colliding with obstacles or other units
        public float MaxSpeed { get; }
        public float Acceleration { get; }
        public override UnitView UnitView => new UnitView(Pos, ViewRange);
        public bool HasEnergy { get; }//true if the unit uses energy
        public float MaxEnergy { get; set; }
        public float Energy { get; set; }
        public Vector2 Direction { get; set; }//direction the unit is facing
        public bool FacingLeft => Direction.X <= 0;
        public Movement Movement { get; }//where can the unit walk
        public float AttackDamage { get; }
        public float AttackPeriod { get; }
        public float AttackDistance { get; }

        public Unit(Players owner, EntityType unitType, float maxHealth, float maxEnergy, Vector2 pos, Movement movement = Movement.GROUND, float range = 0.5f, float viewRange = 6.0f, float maxSpeed = 2f, float acceleration = 4f,
               float attackDamage = 10f, float attackPeriod = 0.9f, float attackDistance = 0.2f)
            :base(owner, unitType, maxHealth, viewRange)
        {
            Pos = pos;
            Vel = new Vector2(0f, 0f);
            Range = range;
            CanBeMoved = true;
            IsInCollision = false;
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            Group = null;
            MaxHealth = maxHealth;
            Health = maxHealth;
            if (maxEnergy > 0)
                HasEnergy = true;
            MaxEnergy = maxEnergy;
            Energy = maxEnergy;
            Direction = new Vector2(1f, 0f);
            Movement = movement;
            AttackDamage = attackDamage;
            AttackPeriod = attackPeriod;
            AttackDistance = attackDistance;
            Abilities.Add(AbilityType.MOVE_TO);
            Abilities.Add(AbilityType.ATTACK);
        }
        
        /// <summary>
        /// Unit moves using its velocity.
        /// </summary>
        public void Move(Map map, float deltaT)
        {
            Pos = new Vector2(
                Math.Max(Range, Math.Min(Pos.X + deltaT * Vel.X, map.Width - Range)),
                Math.Max(Range, Math.Min(Pos.Y + deltaT * Vel.Y, map.Height - Range)));
            if (WantsToMove && Vel.Length != 0)
                Direction = Vel;
            if (StopMoving)
            {
                StopMoving = false;
                WantsToMove = false;
            }
        }

        /// <summary>
        /// Add acceleration to units velocity.
        /// </summary>
        /// <param name="acc"></param>
        public void Accelerate(Vector2 acc)
        {
            Vel += acc;
            float l = Vel.Length;
            if (l > MaxSpeed && l != 0)
                Vel = (MaxSpeed / l) * Vel;
        }
        public override float GetActualBottom(float imageBottom)
            => Math.Min(Pos.Y - Range, Pos.Y - imageBottom);
        public override float GetActualTop(float imageHeight, float imageBottom)
            => Math.Max(Pos.Y + Range, Pos.Y - imageBottom + imageHeight);
        public override float GetActualLeft(float imageLeft)
            => Math.Min(Pos.X - Range, Pos.X - imageLeft);
        public override float GetActualRight(float imageWidth, float imageLeft)
            => Math.Max(Pos.X + Range, Pos.X - imageLeft + imageWidth);
        public override Rect GetActualRect(ImageAtlas atlas)
        {
            Animation anim = atlas.GetAnimation(UnitType);
            return new Rect(
                Math.Min(Pos.X - Range, Pos.X - anim.LeftBottom.X),
                Math.Min(Pos.Y - Range, Pos.Y - anim.LeftBottom.Y),
                Math.Max(Pos.X + Range, Pos.X - anim.LeftBottom.X + anim.Width),
                Math.Max(Pos.Y + Range, Pos.Y - anim.LeftBottom.Y + anim.Height));
        }

        public override float Left => Pos.X - Range;
        public override float Right => Pos.X + Range;
        public override float Bottom => Pos.Y - Range;
        public override float Top => Pos.Y + Range;
        
        public override float DistanceTo(Entity e)
        {
            Unit u = e as Unit;
            if (u != null)
                return (this.Pos - u.Pos).Length - this.Range - u.Range;
            else
                throw new NotImplementedException();
        }
    }
}
