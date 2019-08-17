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
        /// Sources from which the building takes energy.
        /// </summary>
        public Node[,] EnergySources { get; }
        /// <summary>
        /// Type of the building.
        /// </summary>
        public string BuildingType { get; }
        /// <summary>
        /// Maximum energy taken from one source per second.
        /// </summary>
        public decimal MaxEnergyIntake { get; }
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

        public Building(Player player, string buildingType, Node[,] nodes, Node[,] energySources, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange, List<Ability> abilities)
            : base(player, buildingType, maxHealth, viewRange, maxEnergy, physical, abilities)
        {
            Nodes = nodes;
            EnergySources = energySources;
            MaxEnergyIntake = maxEnergyIntake;
            Size = size;
            Biome = biome;
            Terrain = terrain;
            SoilQuality = soilQuality;
            Producer = producer;

            NodeLeft = nodes[0, 0].X;
            NodeBottom = nodes[0, 0].Y;
            Center = new Vector2(nodes[0, 0].X + Range, nodes[0, 0].Y + Range);
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
        public void DrainEnergy()
        {
            foreach(Node n in EnergySources)
            {
                //check if building can take nutrients from this node
                if(n.Terrain == Terrain &&
                    n.Biome == Biome)
                {
                    //take nutrients from this node
                    decimal nutrientsTaken;

                    //building can't take amount of nutrients that would reduce soil quality
                    nutrientsTaken = Math.Min(MaxEnergyIntake, n.Nutrients - n.Terrain.Nutrients(n.Biome, n.SoilQuality));

                    //nutrients can't drop below zero
                    nutrientsTaken = Math.Min(nutrientsTaken, n.Nutrients);

                    //building only takes energy it can use
                    nutrientsTaken = Math.Min(nutrientsTaken, Energy.NotFilled);

                    Energy += nutrientsTaken;
                    n.Nutrients -= nutrientsTaken;
                }
            }
        }

        /// <summary>
        /// Puts nutrients to the soil at its roots.
        /// </summary>
        public void ProduceNutrients()
        {
            foreach (Node n in EnergySources)
            {
                n.Nutrients += MaxEnergyIntake * 2;
            }
        }
    }
}
