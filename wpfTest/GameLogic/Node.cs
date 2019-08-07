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
        public int X { get; }
        public int Y { get; }

        public Terrain Terrain { get; set; }
        /// <summary>
        /// Backing field for Nutrients.
        /// </summary>
        private float nutrients;
        /// <summary>
        /// Amount of nutrients in this node. Belongs to [0, 9.9].
        /// </summary>
        public float Nutrients
        {
            get => nutrients;
            set
            {
                nutrients = Math.Min(9.9f, Math.Max(0, value));
                SoilQuality = Biome.Quality(nutrients);
            }
        }
        public Biome Biome { get; set; }
        public SoilQuality SoilQuality { get; set; }
        public bool Blocked { get; private set; }
        private Building building;
        public Building Building
        {
            get { return building; }
            set { building = value; Blocked = building != null; }
        }
        Vector2 ITargetable.Center => new Vector2(X + 0.5f, Y + 0.5f);

        public Node(int x, int y, float nutrients, Biome biome, Terrain terrain)
        {
            Terrain = terrain;
            Biome = biome;
            Nutrients = nutrients;
            X = x;
            Y = y;
            Blocked = false;
        }

        public Node Copy(int x, int y)
        {
            return new Node(x, y, Nutrients, Biome, Terrain);
        }
    }
}
