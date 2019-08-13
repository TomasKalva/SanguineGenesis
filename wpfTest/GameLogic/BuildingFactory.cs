using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public abstract class BuildingFactory:EntityFactory
    {
        public decimal MaxEnergyIntake { get; }
        public int Size { get; }
        public Biome Biome { get; }
        public Terrain Terrain { get; }
        public SoilQuality SoilQuality { get; }
        public bool Aggressive { get; }

        public BuildingFactory(string buildingType, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, decimal energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, bool aggressive, float viewRange)
            :base(buildingType,maxHealth, maxEnergy, physical, energyCost, viewRange)
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
            return  node.Terrain == Terrain &&
                node.Biome == Biome &&
                node.Nutrients >= Terrain.Nutrients(Biome, SoilQuality);
        }
    }
}
