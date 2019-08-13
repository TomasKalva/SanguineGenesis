using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class Structure : Building
    {
        public Structure(Player player, EntityType buildingType, Node[,] nodes, Node[,] energySources, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool aggressive, float viewRange, List<Ability> abilities)
            : base(player, buildingType, nodes, energySources, maxHealth, maxEnergy, maxEnergyIntake, size, physical, biome, terrain, soilQuality, aggressive, viewRange, abilities)
        {
        }
    }
}
