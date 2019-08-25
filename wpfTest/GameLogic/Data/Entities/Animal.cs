﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Entities;
using wpfTest.GameLogic.Maps;
using wpfTest.GUI;
using static wpfTest.MainWindow;

namespace wpfTest.GameLogic
{
    public class Animal : Unit
    {
        public Vector2 Velocity { get; set; }
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
        public float AttackPeriod { get; set; }
        public float AttackDistance { get; }
        public bool MechanicalDamage { get; }
        public float MaxSpeedLand { get; set; }
        public float MaxSpeedWater { get; }
        public Movement Movement { get; }//where can the unit walk
        /// <summary>
        /// Animals with thick skin take less damage.
        /// </summary>
        public bool ThickSkin { get; set; }
        /// <summary>
        /// Command or status that is manipulating the animal's physical state - changing position,
        /// removing it from the map...
        /// </summary>
        public IAnimalStateManipulator StateChangeLock { get; set; }
        public Diet Diet { get; }
        public float SpawningTime { get; }
        /// <summary>
        /// Amount of air taken by this animal.
        /// </summary>
        public int Air { get; }

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
            List<Ability> abilities,
            int air)
            : base(player, unitType, maxHealth, viewRange, maxEnergy, abilities, position, range, physical)
        {
            Velocity = new Vector2(0f, 0f);
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
            Air = air;
        }

        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Player", Player.ToString()),
            new Stat( "EntityType", EntityType),
            new Stat( "Health", Health+"/"+MaxHealth),
            new Stat("Energy", Energy + "/" + MaxEnergy),
            new Stat( "Food regen", FoodEnergyRegen.ToString()),
            new Stat( "Eating period", FoodEatingPeriod.ToString()),
            new Stat( "Size", (2 * Range).ToString()),
            new Stat( "Att damage", AttackDamage.ToString()),
            new Stat( "Att period", AttackPeriod.ToString()),
            new Stat( "Att Distance", AttackDistance.ToString()),
            new Stat( "Mechanical damage", MechanicalDamage.ToString()),
            new Stat( "Speed land", MaxSpeedLand.ToString()),
            new Stat( "Speed water", MaxSpeedWater.ToString()),
            new Stat( "Movement", Movement.ToString()),
            new Stat( "Thick skin", ThickSkin.ToString()),
            new Stat( "Diet", Diet.ToString()),
            new Stat( "Spawning time", SpawningTime.ToString()),
            new Stat( "Physical", Physical.ToString()),
            new Stat( "Energy cost", EnergyCost.ToString()),
            new Stat( "View range", ViewRange.ToString()),
            };
            return stats;
        }
        
        /// <summary>
        /// Unit moves using its velocity.
        /// </summary>
        public void Move(Map map, float deltaT)
        {
            Position = new Vector2(
                Math.Max(Range, Math.Min(Center.X + deltaT * Velocity.X, map.Width - Range)),
                Math.Max(Range, Math.Min(Center.Y + deltaT * Velocity.Y, map.Height - Range)));
            if (WantsToMove && Velocity.Length != 0)
                Direction = Velocity.UnitVector();
            if (StopMoving)
            {
                StopMoving = false;
                WantsToMove = false;
            }
        }

        /// <summary>
        /// Add acceleration to units velocity.
        /// </summary>
        public void Accelerate(Vector2 acc, Map map)
        {
            //add acceleration to the velocity
            Velocity += acc;

            //determine current max speed
            float maxSpeed;
            Terrain underAnimal = map[(int)Position.X, (int)Position.Y].Terrain;
            if (underAnimal == Terrain.LAND)
                maxSpeed = MaxSpeedLand;
            else
                maxSpeed = MaxSpeedWater;

            //scale the velocity down if it exceeds max speed
            float l = Velocity.Length;
            if (l > maxSpeed && l != 0)
                Velocity = (maxSpeed / l) * Velocity;
        }

        public override void Die()
        {
            base.Die();

            //spawn a corpse of this animal if the animal has any energy left
            if(Energy > 0)
                Player.Entities.Add(
                    new Corpse(Player, "CORPSE", MaxEnergy, 0, Position, 0.2f));
        }

        /// <summary>
        /// Turns the animal to the point.
        /// </summary>
        public void TurnToPoint(Vector2 point)
        {
            Direction = Center.UnitDirectionTo(point);
        }

        public override void Damage(decimal damage)
        {
            //thick skin prevents some damage
            if (ThickSkin)
                damage -= 1;

            base.Damage(damage);
        }
    }

    public enum Diet
    {
        HERBIVORE,
        CARNIVORE,
        OMNIVORE
    }
}
