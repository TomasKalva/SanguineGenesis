using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static wpfTest.MainWindow;

namespace wpfTest.GameLogic
{
    public class Structure : Building
    {
        public Structure(Player player, string buildingType, Node[,] nodes, decimal maxHealth, decimal maxEnergy, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange, List<Ability> abilities)
            : base(player, buildingType, nodes, maxHealth, maxEnergy, size, physical, biome, terrain, soilQuality, producer, viewRange, abilities)
        {
        }

        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Player", Player.ToString()),
                new Stat( "EntityType", EntityType),
                new Stat( "Health", Health+"/"+MaxHealth),
                new Stat("Energy", Energy + "/" + MaxEnergy),
                new Stat( "Size", Size.ToString()),
                new Stat( "Biome", Biome.ToString()),
                new Stat( "Terrain", Terrain.ToString()),
                new Stat( "Soil quality", SoilQuality.ToString()),
                new Stat( "Physical", Physical.ToString()),
                new Stat( "View range", ViewRange.ToString()),
            };
            return stats;
        }
    }
}
