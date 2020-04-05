using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;

namespace SanguineGenesis.GameLogic
{
    /// <summary>
    /// Represents one square of the map.
    /// </summary>
    class Node:ITargetable,IMovementTarget, IHerbivoreFood
    {
        /// <summary>
        /// Maximal number of active nutrients a node can have.
        /// </summary>
        public const float MAX_ACTIVE_NUTRIENTS = 9.9f 
            + 0.0001f; // it is easier to get correct digits of 9.9f if the number is slightly larger
        /// <summary>
        /// Maximal number of passive nutrients a node can have.
        /// </summary>
        public const int MAX_PASSIVE_NUTRIENTS = 30;

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
        private float activeNutrients;
        /// <summary>
        /// Amount of nutrients in this node. Belongs to [0, MAX_NUTRIENTS].
        /// </summary>
        public float ActiveNutrients
        {
            get => activeNutrients;
            set
            {
                activeNutrients = Math.Min(MAX_ACTIVE_NUTRIENTS, Math.Max(0, value));
                SoilQuality = Terrain.Quality(Biome, ActiveNutrients);
            }
        }
        /// <summary>
        /// The amount of nutrients that can be extracted from this node.
        /// </summary>
        public FloatRange PassiveNutrients { get; set; }
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
                SoilQuality = Terrain.Quality(Biome, ActiveNutrients);
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

        public Node(int x, int y, float passiveNutrients, float activeNutrients, Biome biome, Terrain terrain)
        {
            Terrain = terrain;
            Biome = biome;
            ActiveNutrients = activeNutrients;
            PassiveNutrients = new FloatRange(MAX_PASSIVE_NUTRIENTS, passiveNutrients);
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
            return new Node(x, y, PassiveNutrients, ActiveNutrients, Biome, Terrain);
        }

        /// <summary>
        /// Generates nutrients. The amount depends on the soil quality.
        /// </summary>
        public void GenerateNutrients()
        {
            //the transaction conserves nutrients
            float residue = Node.MAX_ACTIVE_NUTRIENTS - ActiveNutrients;
            float extracted = Math.Min(residue,
                Math.Min(PassiveNutrients, SoilQuality.NutrientsProduction()));
            ActiveNutrients += extracted;
            PassiveNutrients -= extracted;
        }

        #region IFood
        bool IFood.FoodLeft => ActiveNutrients > 0;
        void IFood.EatFood(Animal eater)
        {
            float nutrientsToEat = Math.Min(eater.FoodEnergyRegen / 10, ActiveNutrients);
            ActiveNutrients -= nutrientsToEat;
            eater.Energy += nutrientsToEat * 10;
        }
        #endregion IFood

        /// <summary>
        /// Distance to entity.
        /// </summary>
        float ITargetable.DistanceTo(Entity entity)
        {
            return Math.Max(0, (entity.Center - Center).Length - 0.5f);
        }
    }
}
