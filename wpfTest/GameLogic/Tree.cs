using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class Tree:Building
    {
        public int Air { get; }

        public Tree(Player player, EntityType treeType, Node[,] nodes, Node[,] rootNodes, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool aggressive, float viewRange, int air, List<Ability> abilities)
            : base(player, treeType, nodes, rootNodes, maxHealth, maxEnergy, maxEnergyIntake, size, physical, biome, terrain, soilQuality, aggressive, viewRange, abilities)
        {
            Air = air;
        }
    }
}
