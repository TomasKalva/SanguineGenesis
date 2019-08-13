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
}
