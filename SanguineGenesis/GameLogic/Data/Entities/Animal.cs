using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI;

namespace SanguineGenesis.GameLogic
{
    /// <summary>
    /// Represent unit that can move.
    /// </summary>
    class Animal : Unit
    {
        /// <summary>
        /// Velocity vector.
        /// </summary>
        public Vector2 Velocity { get; set; }
        /// <summary>
        /// False if the unit has to stand still.
        /// </summary>
        public bool CanBeMoved { get; set; }
        /// <summary>
        /// Set to true to set WantsToMove to false in Move.
        /// </summary>
        public bool StopMoving { get; set; }
        /// <summary>
        /// True if the unit is performing a MoveToCommand.
        /// </summary>
        public bool WantsToMove { get; set; }
        /// <summary>
        /// Direction the unit is facing.
        /// </summary>
        public Vector2 Direction { get; set; }
        /// <summary>
        /// True iff the unit is facing left.
        /// </summary>
        public bool FacingLeft => Direction.X <= 0;

        /// <summary>
        /// Energy regenerated from eating food.
        /// </summary>
        public float FoodEnergyRegen { get; }
        /// <summary>
        /// Time it takes to eat food once in s.
        /// </summary>
        public float FoodEatingPeriod { get; }
        /// <summary>
        /// Energy required to spawn/create this animal.
        /// </summary>
        public float EnergyCost { get; }
        /// <summary>
        /// Damage this animal deals in one attack.
        /// </summary>
        public float AttackDamage { get; }
        /// <summary>
        /// Time it takes this animal to do one attack.
        /// </summary>
        public float AttackPeriod { get; set; }
        /// <summary>
        /// Minimimal distance required to attack other entity.
        /// </summary>
        public float AttackDistance { get; }
        /// <summary>
        /// True iff the animal deals extra damage to buildings.
        /// </summary>
        public bool MechanicalDamage { get; }
        /// <summary>
        /// Maximal speed on land.
        /// </summary>
        public float MaxSpeedLand { get; set; }
        /// <summary>
        /// Maximal speed in water.
        /// </summary>
        public float MaxSpeedWater { get; }
        /// <summary>
        /// Where the unit can walk.
        /// </summary>
        public Movement Movement { get; }
        /// <summary>
        /// Animals with thick skin take less damage.
        /// </summary>
        public bool ThickSkin { get; set; }
        /// <summary>
        /// Command or status that is manipulating the animal's physical state - changing position,
        /// removing it from the map...
        /// </summary>
        public IAnimalStateManipulator StateChangeLock { get; set; }
        /// <summary>
        /// The food this entity can eat.
        /// </summary>
        public Diet Diet { get; }
        /// <summary>
        /// Time it takes for this animal to spawn.
        /// </summary>
        public float SpawningTime { get; }
        /// <summary>
        /// Amount of air taken by this animal.
        /// </summary>
        public int Air { get; }

        public Animal(
            Faction faction,
            Vector2 position,
            string unitType,
            float maxHealth,
            float maxEnergy,
            float foodEnergyRegen,
            float foodEatingPeriod,
            float range,
            float attackDamage,
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
            float energyCost,
            float viewRange,
            List<Ability> abilities,
            int air)
            : base(faction, unitType, maxHealth, viewRange, maxEnergy, abilities, position, range, physical)
        {
            Velocity = new Vector2(0f, 0f);
            CanBeMoved = true;
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
        #region IShowable
        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Faction", Faction.FactionID.ToString()),
            new Stat( "EntityType", EntityType),
            new Stat( "Health", Health.ToString()),
            new Stat("Energy", Energy.ToString()),
            new Stat( "Food regen", FoodEnergyRegen.ToString()),
            new Stat( "Eating period", FoodEatingPeriod.ToString()),
            new Stat( "Size", (2 * Range).ToString()),
            new Stat( "Att damage", AttackDamage.ToString()),
            new Stat( "Att period", AttackPeriod.ToString()),
            new Stat( "Att Distance", AttackDistance.ToString()),
            new Stat( "Mech dmg", MechanicalDamage.ToString()),
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
        #endregion IShowable

        #region Movement
        /// <summary>
        /// Animal moves using its velocity.
        /// </summary>
        public void Move(Map map, float deltaT)
        {
            if (StopMoving)
            {
                StopMoving = false;
                WantsToMove = false;
                Velocity = new Vector2(0, 0);
            }
            // move the animal
            Position += deltaT * Velocity;
            PushBackToMap(map);
            
            // set direction
            if (WantsToMove && Velocity.Length != 0)
                Direction = Velocity.UnitVector();
        }

        /// <summary>
        /// Add acceleration to units velocity.
        /// </summary>
        public void Accelerate(Vector2 direction, Map map)
        {
            Vector2 vel;
            //determine current max speed
            Terrain underAnimal = map[(int)Position.X, (int)Position.Y].Terrain;
            if (underAnimal == Terrain.LAND)
                vel = MaxSpeedLand * direction;
            else
                vel = MaxSpeedWater * direction;

            Velocity = vel;
        }

        /// <summary>
        /// Returns true if the animal can move on the terrain.
        /// </summary>
        public bool CanMoveOn(Terrain terrain)
        {
            switch (Movement)
            {
                case Movement.LAND:
                    // land animal can only move on land
                    if (terrain == Terrain.LAND)
                        return true;
                    else
                        return false;
                case Movement.WATER:
                    // water animal can not move only on land
                    if (terrain == Terrain.LAND)
                        return false;
                    else
                        return true;
                case Movement.LAND_WATER:
                    // land water animal can move everywhere
                    return true;
                default:
                    return false;
            }
        }
        #endregion Movement

        /// <summary>
        /// Called after this animal dies. Spawns a corpse.
        /// </summary>
        public override void Die(Game game)
        {
            base.Die(game);

            //spawn a corpse of this animal if the animal has any energy left
            if(Energy > 0)
                Faction.Entities.Add(
                    new Corpse(game.NeutralFaction, "CORPSE", Energy, 0, Position, 0.2f));
        }

        /// <summary>
        /// Turns the animal to the point.
        /// </summary>
        public void TurnToPoint(Vector2 point)
        {
            Direction = Center.UnitDirectionTo(point);
        }

        /// <summary>
        /// Deals damage to this animal.
        /// </summary>
        public override void Damage(float damage)
        {
            //thick skin prevents some damage
            if (ThickSkin)
                damage -= 1;

            base.Damage(damage);
        }

        /// <summary>
        /// Pushes animal by displacement. Should be only used in collision handling.
        /// </summary>
        public void Push(Vector2 displacement, Map map)
        {
            Position += displacement;
            PushBackToMap(map);
        }

        public void PushBackToMap(Map map)
        {
            Position = new Vector2(
                Math.Max(Range, Math.Min(Position.X, map.Width - Range)),
                Math.Max(Range, Math.Min(Position.Y, map.Height - Range)));
        }
    }

    /// <summary>
    /// Describes where an animal can move.
    /// </summary>
    public enum Movement
    {
        LAND,
        WATER,
        LAND_WATER
    }

    public static class MovementExtensions
    {
    }

    public enum Diet
    {
        HERBIVORE,
        CARNIVORE
    }
}
