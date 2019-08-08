using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class TreeFactory:BuildingFactory
    {
        public int RootsDistance { get; }
        public int Air { get; }

        public override Building NewInstance(Player player, Node[,] nodesUnder, Node[,] roots)
        {
            return new Tree(player, BuildingType, maxHealth: MaxHealth, viewRange:10, maxEnergy: MaxEnergy, nodes: nodesUnder, size: Size, soilQuality:SoilQuality);
        }

        public TreeFactory(EntityType buildingType, float maxHealth, float maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, float energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality, int rootsDistance, int air)
            : base(buildingType, maxHealth, maxEnergy, maxEnergyIntake, size, physical, energyCost, biome, terrain, soilQuality)

        {
            RootsDistance = rootsDistance;
            Air = air;
        }
    }

    public class StructureFactory : BuildingFactory
    {
        public override Building NewInstance(Player player, Node[,] nodesUnder, Node[,] energySources)
        {
            return new Structure(player, BuildingType, maxHealth: MaxHealth, viewRange: 10, maxEnergy: MaxEnergy, nodes: nodesUnder, size: Size, soilQuality: SoilQuality);
        }

        public StructureFactory(EntityType buildingType, float maxHealth, float maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, float energyCost, Biome biome, Terrain terrain, SoilQuality soilQuality)
            : base(buildingType, maxHealth, maxEnergy, maxEnergyIntake, size, physical, energyCost, biome, terrain, soilQuality)

        {
        }
    }
}
