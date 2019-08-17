using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;

namespace wpfTest
{
    public class Node:ITargetable,IMovementTarget
    {
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
        public bool Blocked { get; private set; }
        private Building building;
        public Building Building
        {
            get { return building; }
            set { building = value; Blocked = building != null; }
        }
        public List<Tree> Roots { get; }
        Vector2 ITargetable.Center => new Vector2(X + 0.5f, Y + 0.5f);

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
    }
}
