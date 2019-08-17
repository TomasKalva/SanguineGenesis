using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public abstract class EntityFactory
    {
        public string EntityType { get; }
        public decimal MaxHealth { get; }
        public decimal MaxEnergy { get; }
        public bool Physical { get; }
        public decimal EnergyCost { get; }
        public float ViewRange { get; }
        public List<Ability> Abilities { get; }

        public EntityFactory(string entityType, decimal maxHealth, decimal maxEnergy,
            bool physical, decimal energyCost, float viewRange)
        {
            EntityType = entityType;
            MaxHealth = maxHealth;
            MaxEnergy = maxEnergy;
            Physical = physical;
            EnergyCost = energyCost;
            ViewRange = viewRange;
            Abilities = new List<Ability>();
        }

    }

    public abstract class BuildingFactory : EntityFactory
    {
        public decimal MaxEnergyIntake { get; }
        public int Size { get; }
        public Biome Biome { get; }
        public Terrain Terrain { get; }
        public SoilQuality SoilQuality { get; }
        public bool Producer { get; }

        public BuildingFactory(string buildingType, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, decimal energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange)
            : base(buildingType, maxHealth, maxEnergy, physical, energyCost, viewRange)
        {
            MaxEnergyIntake = maxEnergyIntake;
            Size = size;
            Biome = biome;
            Terrain = terrain;
            SoilQuality = soilQuality;
            Producer = producer;
        }

        public abstract Building NewInstance(Player player, Node[,] nodesUnder, Node[,] energySources);

        /// <summary>
        /// Returns true iff node can be under the building.
        /// </summary>
        public bool CanBeUnder(Node node)
        {
            return node.Terrain == Terrain &&
                node.Biome == Biome &&
                node.Nutrients >= Terrain.Nutrients(Biome, SoilQuality);
        }
    }

    public class TreeFactory : BuildingFactory
    {
        public int RootsDistance { get; }
        public int Air { get; }

        public override Building NewInstance(Player player, Node[,] nodesUnder, Node[,] roots)
        {
            return new Tree(player, EntityType, nodesUnder, roots, MaxHealth, MaxEnergy, MaxEnergyIntake,
                Size, Physical, Biome, Terrain, SoilQuality, Producer, ViewRange, Air, Abilities.ToList());
        }

        public TreeFactory(string buildingType, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, decimal energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange, int rootsDistance, int air)
            : base(buildingType, maxHealth, maxEnergy, maxEnergyIntake, size, physical, energyCost, biome, terrain, soilQuality, producer, viewRange)

        {
            RootsDistance = rootsDistance;
            Air = air;
        }
    }

    public class StructureFactory : BuildingFactory
    {
        public override Building NewInstance(Player player, Node[,] nodesUnder, Node[,] energySources)
        {
            return new Structure(player, EntityType, nodesUnder, energySources, MaxHealth, MaxEnergy, MaxEnergyIntake,
                Size, Physical, Biome, Terrain, SoilQuality, Producer, ViewRange, Abilities.ToList());
        }

        public StructureFactory(string buildingType, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, decimal energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange)
            : base(buildingType, maxHealth, maxEnergy, maxEnergyIntake, size, physical, energyCost, biome, terrain, soilQuality, producer, viewRange)

        {
        }
    }

    public abstract class UnitFactory : EntityFactory
    {
        public float Range { get; }//range of the circle collider

        public UnitFactory(string unitType, decimal maxHealth, decimal maxEnergy, float range, bool physical, decimal energyCost,
            float viewRange)
            : base(unitType, maxHealth, maxEnergy, physical, energyCost, viewRange)
        {
            Range = range;
        }
    }

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

        public Animal NewInstance(Player player, Vector2 pos)
        {
            return new Animal(
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
                abilities:Abilities.ToList());
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
            float viewRange)
            : base(unitType, maxHealth, maxEnergy, range, physical, energyCost, viewRange)
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
        }
    }
}
