using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class Tree:Building
    {
        public Node[,] RootNodes { get; }
        public int Air { get; }

        public Tree(Player player, EntityType treeType, float maxHealth, float viewRange, float maxEnergy, Node[,] nodes, SoilQuality soilQuality, int size)
            : base(player, treeType, maxHealth, viewRange, maxEnergy, nodes, soilQuality, size)
        {

        }
    }
}
