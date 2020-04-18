using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GameLogic.Maps.MovementGenerating;

namespace SanguineGenesis.GameLogic.Maps
{
    class Map:IMap<Node>
    {
        /// <summary>
        /// Data of the map.
        /// </summary>
        private Node[,] Nodes { get; set; }
        public Node this[int i, int j]
        {
            get => Nodes[i + 1, j + 1];
            set => Nodes[i + 1, j + 1] = value;
        }
        public int Width => Nodes.GetLength(0) - 2;
        public int Height => Nodes.GetLength(1) - 2;
        /// <summary>
        /// Set to true after building was added or removed.
        /// </summary>
        public bool MapWasChanged { get; set; }
        /// <summary>
        /// Obstacle maps for the current map. Is updated by the UpdateObstacleMaps.
        /// </summary>
        public Dictionary<Movement, ObstacleMap> ObstacleMaps { get; }
        /// <summary>
        /// Visibility obstacles for the current map. Is updated by UpdateVisibilityObstacleMap.
        /// </summary>
        public ObstacleMap VisibilityObstacles { get; }

        internal Map(Node[,] nodes)
        {
            this.Nodes = nodes;
            ObstacleMaps = new Dictionary<Movement, ObstacleMap>();
            InitializeObstacleMaps();
            MovementGenerating.MovementGenerator mg = MovementGenerating.MovementGenerator.GetMovementGenerator();
            mg.SetMapChanged(FactionType.PLAYER0, ObstacleMaps);
            mg.SetMapChanged(FactionType.PLAYER1, ObstacleMaps);
            VisibilityObstacles = new ObstacleMap(Width, Height);
        }

        /// <summary>
        /// Copy the terrain of already existing map.
        /// </summary>
        public Map(Map map)
        {
            int width = map.Width;
            int height = map.Height;
            Nodes = new Node[width + 2, height + 2];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Node n = map[i, j];
                    this[i, j] = new Node(i, j,n.PassiveNutrients, n.ActiveNutrients, n.Biome, n.Terrain);
                }
            }
            ObstacleMaps = new Dictionary<Movement, ObstacleMap>();
            InitializeObstacleMaps();
            VisibilityObstacles = new ObstacleMap(Width, Height);
        }

        #region Obstacle maps

        /// <summary>
        /// Initializes obstacle maps with values based on the map terrain.
        /// </summary>
        private void InitializeObstacleMaps()
        {
            ObstacleMaps.Add(Movement.LAND, new ObstacleMap(Width, Height));
            ObstacleMaps.Add(Movement.WATER, new ObstacleMap(Width, Height));
            ObstacleMaps.Add(Movement.LAND_WATER, new ObstacleMap(Width, Height));
            UpdateObstacleMap(Movement.LAND);
            UpdateObstacleMap(Movement.WATER);
            UpdateObstacleMap(Movement.LAND_WATER);
        }

        /// <summary>
        /// Updates ObstacleMaps. Should be called only when the map chaneges. Locks movement generator.
        /// </summary>
        public void UpdateObstacleMaps()
        {
            lock (MovementGenerator.GetMovementGenerator())
            {
                UpdateObstacleMap(Movement.LAND);
                UpdateObstacleMap(Movement.WATER);
                UpdateObstacleMap(Movement.LAND_WATER);
                MapWasChanged = false;
            }
        }

        /// <summary>
        /// Updates VisibilityObstacles.
        /// </summary>
        public void UpdateVisibilityObstacleMap()
        {
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    //player can't see through buildings that block vision
                    VisibilityObstacles[i, j] = this[i, j].Building != null && this[i, j].Building.BlocksVision;
        }

        /// <summary>
        /// Updates the obstacle map from this Map for movement.
        /// </summary>
        public void UpdateObstacleMap(Movement movement)
        {
            ObstacleMaps.TryGetValue(movement, out ObstacleMap obstMap);
            //check if the map exists, if it doesn't exist, create a new one
            if (obstMap==null ||
                obstMap.Width != Width ||
                obstMap.Height != Height)
            {
                ObstacleMaps[movement] = new ObstacleMap(Width, Height);
                obstMap = ObstacleMaps[movement];
            }

            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    Terrain ter = this[i, j].Terrain;
                    switch (movement)
                    {
                        case Movement.LAND:
                            obstMap[i, j] = ter == Terrain.DEEP_WATER ||
                                        this[i,j].MovementBlocked;
                            break;
                        case Movement.WATER:
                            obstMap[i, j] = (ter != Terrain.DEEP_WATER &&
                                        ter != Terrain.SHALLOW_WATER) ||
                                        this[i,j].MovementBlocked;
                            break;
                        case Movement.LAND_WATER:
                            obstMap[i, j] = this[i,j].MovementBlocked;
                            break;
                    }
                }
        }

        #endregion Obstacle maps

        #region Buildings
        /// <summary>
        /// Returns true if the building can be placed on the coordinates of this map.
        /// </summary>
        public bool BuildingCanBePlaced(BuildingFactory buildingFactory, int x, int y)
        {
            int size = buildingFactory.Size;
            Node[,] buildNodes = GameQuerying.SelectNodes(this, x, y, x + (size - 1), y + (size - 1));

            if (buildNodes.GetLength(0) == size &&
                buildNodes.GetLength(1) == size)
            {
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                    {
                        Node ijN = buildNodes[i, j];
                        //the building can't be built if the node is blocked or contains
                        //incompatible terrain
                        if (ijN.Blocked || !(buildingFactory.CanBeOn(ijN)))
                            return false;
                    }


                return true;
            }
            else
            {
                //the whole building has to be on the map
                return false;
            }
        }

        /// <summary>
        /// Places the building created by buildingFactory on the coordinates of this map. The building
        /// will be owned by owner.
        /// </summary>
        public void PlaceBuilding(BuildingFactory buildingFactory, Faction owner, int x, int y)
        {
            //don't place the building if it can't be placed
            if (!BuildingCanBePlaced(buildingFactory, x, y))
                return;

            int size = buildingFactory.Size;
            Node[,] buildNodes = GameQuerying.SelectNodes(this, x, y, x + (size - 1), y + (size - 1));
            Building newBuilding;
            if (buildingFactory is TreeFactory trF)
            {
                //find energy source nodes
                Node[,] rootNodes;
                int rDist = trF.RootsDistance;
                rootNodes = GameQuerying.SelectNodes(this, x - rDist, y - rDist, x + (size + rDist - 1), y + (size + rDist - 1));
                newBuilding = trF.NewInstance(owner, buildNodes, rootNodes);
                //make the tree grow
                owner.GameStaticData.Abilities.Grow.SetCommands(new List<Tree>(1) { (Tree)newBuilding }, Nothing.Get, true, ActionLog.ThrowAway);
            }
            else
            {
                StructureFactory stF = buildingFactory as StructureFactory;
                newBuilding = stF.NewInstance(owner, buildNodes);
            }
            //put the new building on the main map
            owner.AddEntity(newBuilding);
            AddBuilding(newBuilding);
            MapWasChanged = true;
        }

        /// <summary>
        /// Adds building to the map. Set MapWasChanged to true.
        /// </summary>
        public void AddBuilding(Building building)
        {
            Node[,] nodes = GameQuerying
                .SelectNodes(this,
                building.NodeLeft,
                building.NodeBottom,
                building.NodeLeft + (building.Size - 1),
                building.NodeBottom + (building.Size - 1));
            foreach (Node n in nodes)
            {
                n.Building = building;
            }
            MapWasChanged = true;
        }

        /// <summary>
        /// Removes building from the map. Set MapWasChanged to true.
        /// </summary>
        public void RemoveBuilding(Building building)
        {
            foreach (Node n in building.Nodes)
            {
                this[n.X,n.Y].Building = null;
            }
            MapWasChanged = true;
        }

        #endregion Buildings

        #region Nutrients and biomes

        /// <summary>
        /// Time between two updates of nutrients.
        /// </summary>
        private const float NUTRIENT_UPDATE_TIME = 1f;
        /// <summary>
        /// Time until the next nutrients update.
        /// </summary>
        private float nutrientUpdateTimer = NUTRIENT_UPDATE_TIME;

        /// <summary>
        /// Trees draing nutrients from nodes and nodes with roots generate nutrients.
        /// </summary>
        public void UpdateNutrientsMap(IEnumerable<Tree> trees, float deltaT)
        {
            nutrientUpdateTimer -= deltaT;
            if (nutrientUpdateTimer <= 0)
            {
                nutrientUpdateTimer = NUTRIENT_UPDATE_TIME;
                //nodes with roots produce nutrients
                ProduceNutrients();
                SpreadBiomes();

                //trees drain energy from nodes
                foreach (Tree t in trees)
                {
                    t.DrainEnergy();
                }
            }


            //nutrients biomes and terrain can't be updated in this step after calling this method
            LoseBiome();
        }

        /// <summary>
        /// Nodes with roots of producers produce nutrients trients. Nutrients can't be produced under
        /// structures.
        /// </summary>
        public void ProduceNutrients()
        {
            //generate nutrients
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    //produce nutrients only in the same biome or in default biome
                    Biome biome = this[i, j].Biome;
                    if (this[i, j].Roots.Where(
                        (t) => t.Producer && (t.Biome == biome || biome==Biome.DEFAULT)).Any() &&
                        !(this[i, j].Building is Structure))
                        this[i, j].GenerateNutrients();
                }
        }

        /// <summary>
        /// Spread biomes to neighbor nodes.
        /// </summary>
        public void SpreadBiomes()
        {
            //spread biome to neighbors
            Biome[,] newBiomes = new Biome[Width, Height];
            Node[] neighbours = new Node[4];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    Node current = this[i, j];
                    //biomes can spread only to the default biome
                    if (current.Biome != Biome.DEFAULT)
                    {
                        newBiomes[i, j] = this[i, j].Biome;
                        continue;
                    }

                    //initialize neighbors
                    neighbours[0] = this[i + 1, j];
                    neighbours[1] = this[i - 1, j];
                    neighbours[2] = this[i, j + 1];
                    neighbours[3] = this[i, j - 1];

                    //rainMsav = #rainforest neighbours - #savanna neighbours
                    int rainMsav = 0;
                    foreach (Node n in neighbours)
                        if (n.Biome == Biome.RAINFOREST)
                            rainMsav++;
                        else if (n.Biome == Biome.SAVANNA)
                            rainMsav--;

                    //building standing on this node
                    var building = this[i, j].Building;
                    if (this[i, j].ActiveNutrients >= this[i,j].Terrain.Nutrients(Biome.SAVANNA, SoilQuality.LOW))
                    {
                        //node has enough nutrients to become savanna
                        if (rainMsav < 0 || //majority of nondefault neighbour biomes is savanna
                            (building != null &&
                            building.Biome == Biome.SAVANNA)) //or savanna building is standing on it
                            newBiomes[i, j] = Biome.SAVANNA;
                    }
                    if (this[i, j].ActiveNutrients >= this[i, j].Terrain.Nutrients(Biome.RAINFOREST, SoilQuality.LOW))
                    {
                        //node has enough nutrients to become rainforest
                        if (rainMsav > 0 || //majority of nondefault neighbour biomes is rainforest
                            (building != null &&
                            building.Biome == Biome.RAINFOREST))//or rainforest building is standing on it
                            newBiomes[i, j] = Biome.RAINFOREST;
                    }
                }
            
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    this[i, j].Biome = newBiomes[i, j];

        }

        /// <summary>
        /// Node loses biome if the soil quality is bad.
        /// </summary>
        public void LoseBiome()
        {
            //lose biome if the soil quality is too bad
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    Node n;
                    if ((n = this[i, j]).SoilQuality == SoilQuality.BAD)
                        n.Biome = Biome.DEFAULT;
                }

        }
        #endregion Nutrients and biomes
    }
}
