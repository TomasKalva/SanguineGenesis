using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class Structure : Building
    {
        public Structure(Player player, string buildingType, Node[,] nodes, Node[,] energySources, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange, List<Ability> abilities)
            : base(player, buildingType, nodes, energySources, maxHealth, maxEnergy, maxEnergyIntake, size, physical, biome, terrain, soilQuality, producer, viewRange, abilities)
        {
        }
    }
}
