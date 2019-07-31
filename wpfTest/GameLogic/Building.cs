using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GUI;

namespace wpfTest.GameLogic
{
    public class Building : Entity
    {
        public override Vector2 Center { get; }
        public override float Range => Size / 2f;
        public int Size { get; }
        public List<Terrain> Soil { get; }
        public float BuildingTime { get; }
        public Node[,] Nodes { get; }

        public Building(Player player, EntityType bulidingType, float maxHealth, float viewRange, float maxEnergy, Node[,] nodes, List<Terrain> soil, int size, float buildingTime) 
            : base(player, bulidingType, maxHealth, viewRange, maxEnergy)
        {
            Size = size;
            Center = new Vector2(nodes[0, 0].X + Range, nodes[0, 0].Y + Range);
            Soil = soil;
            BuildingTime = buildingTime;
            Nodes = nodes;
            foreach(Node n in nodes)
            {
                n.Building = this;
            }
        }
    }
}
