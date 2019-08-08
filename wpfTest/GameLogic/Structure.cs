using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class Structure : Building
    {
        public Structure(Player player, EntityType bulidingType, float maxHealth, float viewRange, float maxEnergy, Node[,] nodes, SoilQuality soilQuality, int size)
            : base(player, bulidingType, maxHealth, viewRange, maxEnergy, nodes, soilQuality, size)
        {
        }
    }
}
