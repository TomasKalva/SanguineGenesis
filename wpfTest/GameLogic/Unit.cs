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
        public Vector2 Position { get; set; }
        public override Vector2 Center => Position;
        public Vector2 Vel { get; set; }
        public override float Range { get; }//range of the circle collider
        public bool CanBeMoved { get; set; }//false if the unit has to stand still
        public bool StopMoving { get; set; }//set to true to set WantsToMove to false after Move
        public bool WantsToMove { get; set; }//true if the unit has a target destination
        public bool IsInCollision { get; set; }//true if the unit is colliding with obstacles or other units
        public float MaxSpeed { get; }
        public float Acceleration { get; }
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
            Position = pos;
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
            Abilities.Add(MoveTo.Get);
            Abilities.Add(Attack.Get);
        }
        
        /// <summary>
        /// Unit moves using its velocity.
        /// </summary>
        public void Move(Map map, float deltaT)
        {
            Position = new Vector2(
                Math.Max(Range, Math.Min(Center.X + deltaT * Vel.X, map.Width - Range)),
                Math.Max(Range, Math.Min(Center.Y + deltaT * Vel.Y, map.Height - Range)));
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
        
    }
}
