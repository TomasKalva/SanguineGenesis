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
        public bool Aggressive { get; }

        public BuildingFactory(string buildingType, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, decimal energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool aggressive, float viewRange)
            : base(buildingType, maxHealth, maxEnergy, physical, energyCost, viewRange)
        {
            MaxEnergyIntake = maxEnergyIntake;
            Size = size;
            Biome = biome;
            Terrain = terrain;
            SoilQuality = soilQuality;
            Aggressive = aggressive;
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
                Size, Physical, Biome, Terrain, SoilQuality, Aggressive, ViewRange, Air, Abilities.ToList());
        }

        public TreeFactory(string buildingType, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, decimal energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool aggressive, float viewRange, int rootsDistance, int air)
            : base(buildingType, maxHealth, maxEnergy, maxEnergyIntake, size, physical, energyCost, biome, terrain, soilQuality, aggressive, viewRange)

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
                Size, Physical, Biome, Terrain, SoilQuality, Aggressive, ViewRange, Abilities.ToList());
        }

        public StructureFactory(string buildingType, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, decimal energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool aggressive, float viewRange)
            : base(buildingType, maxHealth, maxEnergy, maxEnergyIntake, size, physical, energyCost, biome, terrain, soilQuality, aggressive, viewRange)

        {
        }
    }

    public class UnitFactory : EntityFactory
    {
        public float Range { get; }//range of the circle collider
        public float MaxSpeed { get; }
        public float Acceleration { get; }
        public Movement Movement { get; }
        public float SpawningTime { get; }
        public decimal AttackDamage { get; }
        public float AttackPeriod { get; }
        public float AttackDistance { get; }

        public Unit NewInstance(Player player, Vector2 pos)
        {
            return new Unit(player, EntityType, MaxHealth, MaxEnergy, pos, Movement, Range, ViewRange, MaxSpeed, Acceleration, AttackDamage, AttackPeriod, AttackDistance, Abilities.ToList());
        }

        public UnitFactory(string unitType, decimal maxHealth, decimal maxEnergy, float range, bool physical, decimal energyCost,
            float viewRange, float maxSpeed, float acceleration, Movement movement, float spawningTime, decimal attackDamage, float attackPeriod, float attackDistance)
            : base(unitType, maxHealth, maxEnergy, physical, energyCost, viewRange)
        {
            Range = range;
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            Movement = movement;
            SpawningTime = spawningTime;
            AttackDamage = attackDamage;
            AttackPeriod = attackPeriod;
            AttackDistance = attackDistance;
        }
    }
}
