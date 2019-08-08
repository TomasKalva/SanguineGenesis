using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;
using wpfTest.GUI;

namespace wpfTest.GameLogic
{
    public abstract class Building : Entity
    {
        public override Vector2 Center { get; }
        public override float Range => Size / 2f;
        public int Size { get; }
        public SoilQuality SoilQuality { get; }
        /// <summary>
        /// True iff the building was built.
        /// </summary>
        public bool Built { get; private set; }
        /// <summary>
        /// X coordinate of bottom left node.
        /// </summary>
        public int NodeLeft { get; }
        /// <summary>
        /// Y coordinate of bottom left node.
        /// </summary>
        public int NodeBottom { get; }
        public Node[,] Nodes { get; }

        public Building(Player player, EntityType bulidingType, float maxHealth, float viewRange, float maxEnergy, Node[,] nodes, SoilQuality soilQuality, int size) 
            : base(player, bulidingType, maxHealth, viewRange, maxEnergy)
        {
            Size = size;
            Center = new Vector2(nodes[0, 0].X + Range, nodes[0, 0].Y + Range);
            SoilQuality = soilQuality;
            Nodes = nodes;
            NodeLeft = nodes[0,0].X;
            NodeBottom = nodes[0,0].Y;
        }

        /// <summary>
        /// Returns true if at least one of the building's nodes is visible.
        /// </summary>
        public override bool IsVisible(VisibilityMap visibilityMap)
        {
            if (visibilityMap == null)
                return false;

            foreach (Node n in Nodes)
            {
                if (visibilityMap[n.X, n.Y])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes this building from the main map.
        /// </summary>
        public void RemoveFromMap()
        {
            foreach (Node n in Nodes)
            {
                n.Building = null;
            }
        }

        /// <summary>
        /// Transforms nutrients from energy sources to energy.
        /// </summary>
        public void DrainEnergy(float deltaT)
        {
            throw new NotImplementedException();
        }
    }
}
