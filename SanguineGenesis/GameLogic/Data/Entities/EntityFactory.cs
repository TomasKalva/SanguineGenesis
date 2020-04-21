using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;
using SanguineGenesis.GameLogic.Maps;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    /// <summary>
    /// Used for creating Entities.
    /// </summary>
    abstract class EntityFactory
    {
        public string EntityType { get; }
        public float MaxHealth { get; }
        public float MaxEnergy { get; }
        public bool Physical { get; }
        public float EnergyCost { get; }
        public float ViewRange { get; }
        public List<Ability> Abilities { get; }
        public List<StatusFactory> StatusFactories { get; }

        public EntityFactory(string entityType, float maxHealth, float maxEnergy,
            bool physical, float energyCost, float viewRange, List<StatusFactory> statusFactories)
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
                statFac.ApplyToStatusOwner(entity);
            return entity;
        }
    }

    /// <summary>
    /// Used for creating Buildings.
    /// </summary>
    class BuildingFactory : EntityFactory
    {
        public int Size { get; }
        public Biome Biome { get; }
        public Terrain Terrain { get; }
        public SoilQuality SoilQuality { get; }
        public bool Producer { get; }
        public float BuildingDistance { get; }
        public bool BlocksVision { get; }

        public BuildingFactory(string buildingType, float maxHealth, float maxEnergy, int size,
            bool physical, float energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float buildingDistance, float viewRange, bool blocksVision, List<StatusFactory> statusFactories)
            : base(buildingType, maxHealth, maxEnergy, physical, energyCost, viewRange, statusFactories)
        {
            Size = size;
            Biome = biome;
            Terrain = terrain;
            SoilQuality = soilQuality;
            Producer = producer;
            BuildingDistance = buildingDistance;
            BlocksVision = blocksVision;
        }

        /// <summary>
        /// Returns true iff this building can be on the node.
        /// </summary>
        public bool CanBeOn(Node node)
        {
            //any node has at least BAD terrain
            if (node.Terrain == Terrain &&
                SoilQuality == SoilQuality.BAD)
                return true;

            return node.Terrain == Terrain &&
                node.Biome == Biome &&
                node.ActiveNutrients >= Terrain.Nutrients(Biome, SoilQuality);
        }
    }

    /// <summary>
    /// Used for creating Plants.
    /// </summary>
    class PlantFactory : BuildingFactory
    {
        public float MaxEnergyIntake { get; }
        public int RootsDistance { get; }
        public int Air { get; }

        /// <summary>
        /// Creates a new Plant for the player.
        /// </summary>
        public Plant NewInstance(Faction faction, Node[,] nodesUnder, Node[,] roots)
        {
            return (Plant)SetStatuses(new Plant(faction, EntityType, nodesUnder, roots, RootsDistance, MaxHealth, MaxEnergy, MaxEnergyIntake,
                Size, Physical, Biome, Terrain, SoilQuality, Producer, BuildingDistance, ViewRange, BlocksVision, Air, Abilities.ToList()));
        }

        public PlantFactory(string buildingType, float maxHealth, float maxEnergy, float maxEnergyIntake, int size,
            bool physical, float energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float buildingDistance, float viewRange, bool blocksVision, int rootsDistance, int air, List<StatusFactory> statusFactories)
            : base(buildingType, maxHealth, maxEnergy, size, physical, energyCost, biome, terrain, soilQuality, producer, buildingDistance, viewRange, blocksVision, statusFactories)

        {
            MaxEnergyIntake = maxEnergyIntake;
            RootsDistance = rootsDistance;
            Air = air;
        }
    }

    /// <summary>
    /// Used for creating Structures.
    /// </summary>
    class StructureFactory : BuildingFactory
    {
        /// <summary>
        /// Creates a new Structure for the player.
        /// </summary>
        public Structure NewInstance(Faction faction, Node[,] nodesUnder)
        {
            return (Structure)SetStatuses(new Structure(faction, EntityType, nodesUnder, MaxHealth, MaxEnergy,
                Size, Physical, Biome, Terrain, SoilQuality, Producer, BuildingDistance, ViewRange, BlocksVision, Abilities.ToList()));
        }

        public StructureFactory(string buildingType, float maxHealth, float maxEnergy, int size,
            bool physical, float energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float buildingDistance, float viewRange, bool blocksVision, List<StatusFactory> statusFactories)
            : base(buildingType, maxHealth, maxEnergy, size, physical, energyCost, biome, terrain, soilQuality, producer, buildingDistance, viewRange, blocksVision, statusFactories)

        {
        }
    }

    /// <summary>
    /// Used for creating Units.
    /// </summary>
    class UnitFactory : EntityFactory
    {
        public float Radius { get; }//randius of the circle collider

        public UnitFactory(string unitType, float maxHealth, float maxEnergy, float radius, bool physical, float energyCost,
            float viewRange, List<StatusFactory> statusFactories)
            : base(unitType, maxHealth, maxEnergy, physical, energyCost, viewRange, statusFactories)
        {
            Radius = radius;
        }
    }

    /// <summary>
    /// Used for creating Animals.
    /// </summary>
    class AnimalFactory : UnitFactory
    {
        public float FoodEnergyRegen { get; }
        public float FoodEatingPeriod { get; }
        public float AttackDamage { get; }
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
        public Animal NewInstance(Faction faction, Vector2 pos)
        {
            return (Animal)SetStatuses(new Animal(
                faction: faction,
                position: pos,
                unitType: EntityType,
                maxHealth: MaxHealth,
                maxEnergy:MaxEnergy,
                foodEnergyRegen:FoodEnergyRegen,
                foodEatingPeriod:FoodEatingPeriod,
                radius:Radius,
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
            float maxHealth,
            float maxEnergy,
            float foodEnergyRegen,
            float foodEatingPeriod,
            float radius,
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
            List<StatusFactory> statusFactories,
            int air)
            : base(unitType, maxHealth, maxEnergy, radius, physical, energyCost, viewRange, statusFactories)
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
