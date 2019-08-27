using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Data.Abilities;

namespace wpfTest
{
    /// <summary>
    /// Represents one square of the map.
    /// </summary>
    public class Node:ITargetable,IMovementTarget, IHerbivoreFood
    {
        /// <summary>
        /// Maximal number of nutrients a node can have.
        /// </summary>
        public const decimal MAX_NUTRIENTS = 9.9m;

        public int X { get; }
        public int Y { get; }

        public Terrain Terrain { get; set; }
        /// <summary>
        /// Backing field for Nutrients.
        /// </summary>
        private decimal nutrients;
        /// <summary>
        /// Amount of nutrients in this node. Belongs to [0, MAX_NUTRIENTS].
        /// </summary>
        public decimal Nutrients
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
        public Biome Biome
        {
            get => biome;
            set
            {
                biome = value;
                SoilQuality = Terrain.Quality(Biome, Nutrients);
            }
        }
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
        public Building Building
        {
            get { return building; }
            set { building = value; Blocked = building != null; }
        }
        public List<Tree> Roots { get; }
        public Vector2 Center => new Vector2(X + 0.5f, Y + 0.5f);

        public Node(int x, int y, decimal nutrients, Biome biome, Terrain terrain)
        {
            Terrain = terrain;
            Biome = biome;
            Nutrients = nutrients;
            X = x;
            Y = y;
            Blocked = false;
            Roots = new List<Tree>();
        }

        public Node Copy(int x, int y)
        {
            return new Node(x, y, Nutrients, Biome, Terrain);
        }

        public void GenerateNutrients()
        {
            Nutrients += SoilQuality.NutrientsProduction();
        }

        bool IFood.FoodLeft => Nutrients > 0;
        void IFood.EatFood(Animal eater)
        {
            decimal nutrientsToEat = Math.Min(eater.FoodEnergyRegen / 10, Nutrients);
            Nutrients -= nutrientsToEat;
            eater.Energy += nutrientsToEat * 10;
        }

        float IMovementTarget.DistanceTo(Animal animal)
        {
            return (animal.Position - Center).Length;
        }
    }
}
