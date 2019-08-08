using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public abstract class BuildingFactory
    {
        public EntityType BuildingType { get; }
        public float MaxHealth { get; }
        public float MaxEnergy { get; }
        public decimal MaxEnergyIntake { get; }
        public int Size { get; }
        public bool Physical { get; }
        public float EnergyCost { get; }
        public Biome Biome { get; }
        public Terrain Terrain { get; }
        public SoilQuality SoilQuality { get; }

        public BuildingFactory(EntityType buildingType, float maxHealth, float maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, float energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality)
        {
            BuildingType = buildingType;
            MaxHealth = maxHealth;
            MaxEnergy = maxEnergy;
            MaxEnergyIntake = maxEnergyIntake;
            Size = size;
            Physical = physical;
            EnergyCost = energyCost;
            Biome = biome;
            Terrain = terrain;
            SoilQuality = soilQuality;
        }

        public abstract Building NewInstance(Player player, Node[,] nodesUnder, Node[,] energySources);

        public bool NodeIsValid(Node node)
        {
            return  node.Terrain == Terrain &&
                node.Biome == Biome &&
                node.Nutrients >= Terrain.Nutrients(Biome, SoilQuality);
        }
    }
}
