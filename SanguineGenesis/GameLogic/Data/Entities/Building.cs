using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    /// <summary>
    /// Represent entity that is bound to the square grid.
    /// </summary>
    abstract class Building : Entity
    {
        /// <summary>
        /// X coordinate of bottom left node.
        /// </summary>
        public int NodeLeft { get; }
        /// <summary>
        /// Y coordinate of bottom left node.
        /// </summary>
        public int NodeBottom { get; }
        /// <summary>
        /// How many nodes horizontally and also vertically it takes.
        /// </summary>
        public int Size { get; }
        /// <summary>
        /// Center of this building on the map.
        /// </summary>
        public override Vector2 Center { get; }
        /// <summary>
        /// Radius of the circle collider.
        /// </summary>
        public override float Radius => Size / 2f;
        /// <summary>
        /// True if building of this building was finished.
        /// </summary>
        public bool Built { get; set; }
        /// <summary>
        /// Nodes under the building.
        /// </summary>
        public Node[,] Nodes { get; }
        /// <summary>
        /// Biome required under the building. Energy is taken only from nodes with this biome.
        /// </summary>
        public Biome Biome { get; }
        /// <summary>
        /// Terrain required under the building. Energy is taken only from nodes with this terrain.
        /// </summary>
        public Terrain Terrain { get; }
        /// <summary>
        /// Minimal soil quality required under the building.
        /// </summary>
        public SoilQuality SoilQuality { get; }
        /// <summary>
        /// True if the building blocks vision.
        /// </summary>
        public bool BlocksVision { get; set; }
        /// <summary>
        /// Point to which created units go after they spawn.
        /// </summary>
        public Vector2? RallyPoint { get; set; }
        // Map extents
        public new int Left => NodeLeft;
        public new int Right => Left + Nodes.GetLength(0);
        public new int Bottom => NodeBottom;
        public new int Top => Bottom + Nodes.GetLength(1);
        public new int Width => Right - Left;
        public new int Height => Top - Bottom;

        public Building(Faction faction, string buildingType, Node[,] nodes, float maxHealth, float maxEnergy, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, float viewRange, bool blocksVision, List<Ability> abilities)
            : base(faction, buildingType, maxHealth, viewRange, maxEnergy, physical, abilities)
        {
            Nodes = nodes;
            Size = size;
            Biome = biome;
            Terrain = terrain;
            SoilQuality = soilQuality;
            BlocksVision = blocksVision;

            NodeLeft = nodes[0, 0].X;
            NodeBottom = nodes[0, 0].Y;
            Center = new Vector2(nodes[0, 0].X + Radius, nodes[0, 0].Y + Radius);
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
        public override void Die(Game game)
        {
            base.Die(game);

            RemoveFromMap();
        }
    }
}
