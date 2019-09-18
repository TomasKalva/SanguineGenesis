using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI;

namespace SanguineGenesis.GameLogic
{
    /// <summary>
    /// Represent entity that is bound to the square grid.
    /// </summary>
    public abstract class Building : Entity
    {
        /// <summary>
        /// Center of this building on the map.
        /// </summary>
        public override Vector2 Center { get; }
        /// <summary>
        /// Range of the circle collider.
        /// </summary>
        public override float Range => Size / 2f;
        /// <summary>
        /// True iff the building was built.
        /// </summary>
        public bool Built { get; set; }
        /// <summary>
        /// X coordinate of bottom left node.
        /// </summary>
        public int NodeLeft { get; }
        /// <summary>
        /// Y coordinate of bottom left node.
        /// </summary>
        public int NodeBottom { get; }
        /// <summary>
        /// Nodes under the building.
        /// </summary>
        public Node[,] Nodes { get; }
        /// <summary>
        /// Type of the building.
        /// </summary>
        public string BuildingType { get; }
        /// <summary>
        /// How many nodes horizontally and also vertically it takes.
        /// </summary>
        public int Size { get; }
        /// <summary>
        /// Biome required under the building. Energy is taken only from nodes with this biome.
        /// </summary>
        public Biome Biome { get; }
        /// <summary>
        /// Terrain required under the building. Energy is taken only from nodes with this terrain.
        /// </summary>
        public Terrain Terrain { get; }
        /// <summary>
        /// Minimal soil quality required under the building. Energy is taken only from nodes with
        /// at least this soil quality.
        /// </summary>
        public SoilQuality SoilQuality { get; }
        /// <summary>
        /// Produces energy for nodes around it.
        /// </summary>
        public bool Producer { get; }
        /// <summary>
        /// Point to which created units go after they spawn.
        /// </summary>
        public Vector2 RallyPoint { get; set; }
        // Map extents
        public new int Left => NodeLeft;
        public new int Right => Left + Nodes.GetLength(0);
        public new int Bottom => NodeBottom;
        public new int Top => Bottom + Nodes.GetLength(1);
        public new int Width => Right - Left;
        public new int Height => Top - Bottom;

        public Building(Player player, string buildingType, Node[,] nodes, decimal maxHealth, decimal maxEnergy, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange, List<Ability> abilities)
            : base(player, buildingType, maxHealth, viewRange, maxEnergy, physical, abilities)
        {
            Nodes = nodes;
            Size = size;
            Biome = biome;
            Terrain = terrain;
            SoilQuality = soilQuality;
            Producer = producer;

            NodeLeft = nodes[0, 0].X;
            NodeBottom = nodes[0, 0].Y;
            Center = new Vector2(nodes[0, 0].X + Range, nodes[0, 0].Y + Range);
            RallyPoint = new Vector2(Center.X, Bottom - 1);
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
        /// Called after this building dies.
        /// </summary>
        public override void Die()
        {
            base.Die();

            RemoveFromMap();
        }
    }
}
