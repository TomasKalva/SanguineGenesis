using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;

namespace SanguineGenesis.GameLogic
{
    /// <summary>
    /// Used for creating Entities.
    /// </summary>
    public abstract class EntityFactory
    {
        public string EntityType { get; }
        public decimal MaxHealth { get; }
        public decimal MaxEnergy { get; }
        public bool Physical { get; }
        public decimal EnergyCost { get; }
        public float ViewRange { get; }
        public List<Ability> Abilities { get; }
        public List<StatusFactory> StatusFactories { get; }

        public EntityFactory(string entityType, decimal maxHealth, decimal maxEnergy,
            bool physical, decimal energyCost, float viewRange, List<StatusFactory> statusFactories)
        {
            EntityType = entityType;
            MaxHealth = maxHealth;
            MaxEnergy = maxEnergy;
            Physical = physical;
            EnergyCost = energyCost;
            ViewRange = viewRange;
            Abilities = new List<Ability>();
            StatusFactories = statusFactories;
        }

        /// <summary>
        /// Sets statuses of this factory to the entity.
        /// </summary>
        public Entity SetStatuses(Entity entity)
        {
            foreach(StatusFactory statFac in StatusFactories)
                statFac.ApplyToEntity(entity);
            return entity;
        }
    }

    /// <summary>
    /// Used for creating Buildings.
    /// </summary>
    public abstract class BuildingFactory : EntityFactory
    {
        public int Size { get; }
        public Biome Biome { get; }
        public Terrain Terrain { get; }
        public SoilQuality SoilQuality { get; }
        public bool Producer { get; }

        public BuildingFactory(string buildingType, decimal maxHealth, decimal maxEnergy, int size,
            bool physical, decimal energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange, List<StatusFactory> statusFactories)
            : base(buildingType, maxHealth, maxEnergy, physical, energyCost, viewRange, statusFactories)
        {
            Size = size;
            Biome = biome;
            Terrain = terrain;
            SoilQuality = soilQuality;
            Producer = producer;
        }

        /// <summary>
        /// Returns true iff this building can be on the node.
        /// </summary>
        public bool CanBeOn(Node node)
        {
            return node.Terrain == Terrain &&
                node.Biome == Biome &&
                node.Nutrients >= Terrain.Nutrients(Biome, SoilQuality);
        }
    }

    /// <summary>
    /// Used for creating Trees.
    /// </summary>
    public class TreeFactory : BuildingFactory
    {
        public decimal MaxEnergyIntake { get; }
        public int RootsDistance { get; }
        public int Air { get; }

        /// <summary>
        /// Creates a new Tree for the player.
        /// </summary>
        public Tree NewInstance(Player player, Node[,] nodesUnder, Node[,] roots)
        {
            return (Tree)SetStatuses(new Tree(player, EntityType, nodesUnder, roots, MaxHealth, MaxEnergy, MaxEnergyIntake,
                Size, Physical, Biome, Terrain, SoilQuality, Producer, ViewRange, Air, Abilities.ToList()));
        }

        public TreeFactory(string buildingType, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, decimal energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange, int rootsDistance, int air, List<StatusFactory> statusFactories)
            : base(buildingType, maxHealth, maxEnergy, size, physical, energyCost, biome, terrain, soilQuality, producer, viewRange, statusFactories)

        {
            MaxEnergyIntake = maxEnergyIntake;
            RootsDistance = rootsDistance;
            Air = air;
        }
    }

    /// <summary>
    /// Used for creating Structures.
    /// </summary>
    public class StructureFactory : BuildingFactory
    {
        /// <summary>
        /// Creates a new Structure for the player.
        /// </summary>
        public Structure NewInstance(Player player, Node[,] nodesUnder)
        {
            return (Structure)SetStatuses(new Structure(player, EntityType, nodesUnder, MaxHealth, MaxEnergy,
                Size, Physical, Biome, Terrain, SoilQuality, Producer, ViewRange, Abilities.ToList()));
        }

        public StructureFactory(string buildingType, decimal maxHealth, decimal maxEnergy, int size,
            bool physical, decimal energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange, List<StatusFactory> statusFactories)
            : base(buildingType, maxHealth, maxEnergy, size, physical, energyCost, biome, terrain, soilQuality, producer, viewRange, statusFactories)

        {
        }
    }

    /// <summary>
    /// Used for creating Units.
    /// </summary>
    public abstract class UnitFactory : EntityFactory
    {
        public float Range { get; }//range of the circle collider

        public UnitFactory(string unitType, decimal maxHealth, decimal maxEnergy, float range, bool physical, decimal energyCost,
            float viewRange, List<StatusFactory> statusFactories)
            : base(unitType, maxHealth, maxEnergy, physical, energyCost, viewRange, statusFactories)
        {
            Range = range;
        }
    }

    /// <summary>
    /// Used for creating Animals.
    /// </summary>
    public class AnimalFactory : UnitFactory
    {
        public decimal FoodEnergyRegen { get; }
        public float FoodEatingPeriod { get; }
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
        public int Air { get; }

        /// <summary>
        /// Creates a new Animal for the player.
        /// </summary>
        public Animal NewInstance(Player player, Vector2 pos)
        {
            return (Animal)SetStatuses(new Animal(
                player: player,
                position: pos,
                unitType: EntityType,
                maxHealth: MaxHealth,
                maxEnergy:MaxEnergy,
                foodEnergyRegen:FoodEnergyRegen,
                foodEatingPeriod:FoodEatingPeriod,
                range:Range,
                attackDamage:AttackDamage,
                attackPeriod:AttackPeriod,
                attackDistance:AttackDistance,
                mechanicalDamage:MechanicalDamage,
                maxSpeedLand:MaxSpeedLand,
                maxSpeedWater:MaxSpeedWater,
                movement:Movement,
                thickSkin:ThickSkin,
                diet:Diet,
                spawningTime:SpawningTime,
                physical:Physical,
                energyCost:EnergyCost,
                viewRange:ViewRange,
                abilities:Abilities.ToList(),
                air:Air));
        }

        public AnimalFactory(
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
            List<StatusFactory> statusFactories,
            int air)
            : base(unitType, maxHealth, maxEnergy, range, physical, energyCost, viewRange, statusFactories)
        {
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
    }
}
