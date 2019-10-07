using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GUI;

namespace SanguineGenesis.GameLogic
{
    /// <summary>
    /// Represent building that doesn't do anything special.
    /// </summary>
    class Structure : Building
    {
        public Structure(Player player, string buildingType, Node[,] nodes, decimal maxHealth, decimal maxEnergy, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange, List<Ability> abilities)
            : base(player, buildingType, nodes, maxHealth, maxEnergy, size, physical, biome, terrain, soilQuality, producer, viewRange, abilities)
        {
        }

        #region IShowable
        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Player", Player.PlayerID.ToString()),
                new Stat( "EntityType", EntityType),
                new Stat( "Health", Health.ToString()),
                new Stat( "Energy", Energy.ToString()),
                new Stat( "Size", Size.ToString()),
                new Stat( "Biome", Biome.ToString()),
                new Stat( "Terrain", Terrain.ToString()),
                new Stat( "Soil quality", SoilQuality.ToString()),
                new Stat( "Physical", Physical.ToString()),
                new Stat( "View range", ViewRange.ToString()),
            };
            return stats;
        }
        #endregion IShowable
    }
}
