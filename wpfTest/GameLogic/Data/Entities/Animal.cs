using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Entities;
using wpfTest.GameLogic.Maps;
using wpfTest.GUI;

namespace wpfTest.GameLogic
{
    public class Animal : Unit
    {
        public Vector2 Vel { get; set; }
        public bool CanBeMoved { get; set; }//false if the unit has to stand still
        public bool StopMoving { get; set; }//set to true to set WantsToMove to false after Move
        public bool WantsToMove { get; set; }//true if the unit has a target destination
        public bool IsInCollision { get; set; }//true if the unit is colliding with obstacles or other units
        public float Acceleration => 4f;
        public Vector2 Direction { get; set; }//direction the unit is facing
        public bool FacingLeft => Direction.X <= 0;

        public decimal FoodEnergyRegen { get; }
        public float FoodEatingPeriod { get; }
        public decimal EnergyCost { get; }
        public decimal AttackDamage { get; }
        public float AttackPeriod { get; }
        public float AttackDistance { get; }
        public bool MechanicalDamage { get; }
        public float MaxSpeedLand { get; }
        public float MaxSpeedWater { get; }
        public Movement Movement { get; }//where can the unit walk
        public bool ThickSkin { get; }
        public Diet Diet { get; }
        public float SpawningTime { get; }

        public Animal(
            Player player, 
            Vector2 position,
            string unitType,
            decimal maxHealth,
            decimal maxEnergy,
            decimal foodEnergyRegen,
            float foodEatingPeriod,
            float range,
            decimal attackDamage,
            float attackPeriod,
            float attackDistance,
            bool mechanicalDamage,
            float maxSpeedLand,
            float maxSpeedWater,
            Movement movement,
            bool thickSkin,
            Diet diet,
            float spawningTime,
            bool physical,
            decimal energyCost,
            float viewRange,
            List<Ability> abilities)
            :base(player, unitType, maxHealth, viewRange, maxEnergy, abilities, position, range, physical)
        {
            Vel = new Vector2(0f, 0f);
            CanBeMoved = true;
            IsInCollision = false;
            Direction = new Vector2(1f, 0f);
            
            FoodEnergyRegen = foodEnergyRegen;
            FoodEatingPeriod = foodEatingPeriod;
            AttackDamage = attackDamage;
            AttackPeriod = attackPeriod;
            AttackDistance = attackDistance;
            MechanicalDamage = mechanicalDamage;
            MaxSpeedLand = maxSpeedLand;
            MaxSpeedWater = maxSpeedWater;
            Movement = movement;
            ThickSkin = thickSkin;
            Diet = diet;
            SpawningTime = spawningTime;
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
            if (l > MaxSpeedLand && l != 0)
                Vel = (MaxSpeedLand / l) * Vel;
        }

        public override void Die()
        {
            base.Die();

            //spawn a corpse of this animal if the animal has any energy left
            if(Energy > 0)
                Player.Entities.Add(
                    new Corpse(Player, "CORPSE", MaxEnergy, 0, Position, 0.2f));
        }
    }

    public enum Diet
    {
        HERBIVORE,
        CARNIVORE,
        OMNIVORE
    }
}
