using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;

namespace SanguineGenesis
{
    /// <summary>
    /// Represents one square of the map.
    /// </summary>
    class Node:ITargetable,IMovementTarget, IHerbivoreFood
    {
        /// <summary>
        /// Maximal number of nutrients a node can have.
        /// </summary>
        public const float MAX_NUTRIENTS = 9.9f 
            + 0.0001f; // it is easier to get correct digits of 9.9f if the number is slightly larger
        
        /// <summary>
        /// X coordinate on the map.
        /// </summary>
        public int X { get; }
        /// <summary>
        /// Y coordinate on the map.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Terrain of this node.
        /// </summary>
        public Terrain Terrain { get; set; }
        /// <summary>
        /// Backing field for Nutrients.
        /// </summary>
        private float nutrients;
        /// <summary>
        /// Amount of nutrients in this node. Belongs to [0, MAX_NUTRIENTS].
        /// </summary>
        public float Nutrients
        {
            get => nutrients;
            set
            {
                nutrients = Math.Min(MAX_NUTRIENTS, Math.Max(0, value));
                SoilQuality = Terrain.Quality(Biome, Nutrients);
            }
        }
        /// <summary>
        /// Backing field for Biome.
        /// </summary>
        private Biome biome;
        /// <summary>
        /// Biome of this node.
        /// </summary>
        public Biome Biome
        {
            get => biome;
            set
            {
                biome = value;
                SoilQuality = Terrain.Quality(Biome, Nutrients);
            }
        }
        /// <summary>
        /// Soil quality of this node.
        /// </summary>
        public SoilQuality SoilQuality { get; set; }
        /// <summary>
        /// True iff a building or structure is standing on this node.
        /// </summary>
        public bool Blocked { get; private set; }
        /// <summary>
        /// True iff physical building is standing on this node.
        /// </summary>
        public bool MovementBlocked => Building != null && Building.Physical;
        private Building building;
        /// <summary>
        /// Building standing on this node.
        /// </summary>
        public Building Building
        {
            get { return building; }
            set { building = value; Blocked = building != null; }
        }
        /// <summary>
        /// List of trees whose roots are on this node.
        /// </summary>
        public List<Tree> Roots { get; }
        /// <summary>
        /// Center of this node on the map.
        /// </summary>
        public Vector2 Center => new Vector2(X + 0.5f, Y + 0.5f);

        public Node(int x, int y, float nutrients, Biome biome, Terrain terrain)
        {
            Terrain = terrain;
            Biome = biome;
            Nutrients = nutrients;
            X = x;
            Y = y;
            Blocked = false;
            Roots = new List<Tree>();
        }

        /// <summary>
        /// Copy this node. The new node has coordinates x, y.
        /// </summary>
        public Node Copy(int x, int y)
        {
            return new Node(x, y, Nutrients, Biome, Terrain);
        }

        /// <summary>
        /// Generates nutrients. The amount depends on the soil quality.
        /// </summary>
        public void GenerateNutrients()
        {
            Nutrients += SoilQuality.NutrientsProduction();
        }

        #region IFood
        bool IFood.FoodLeft => Nutrients > 0;
        void IFood.EatFood(Animal eater)
        {
            float nutrientsToEat = Math.Min(eater.FoodEnergyRegen / 10, Nutrients);
            Nutrients -= nutrientsToEat;
            eater.Energy += nutrientsToEat * 10;
        }
        #endregion IFood

        /// <summary>
        /// Distance to animal.
        /// </summary>
        float IMovementTarget.DistanceTo(Animal animal)
        {
            return (animal.Position - Center).Length;
        }
    }
}
