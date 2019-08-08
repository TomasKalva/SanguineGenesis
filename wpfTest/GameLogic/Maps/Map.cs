using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using wpfTest.GameLogic;

namespace wpfTest
{
    public class Map:IMap<Node>
    {
        private Node[,] nodes { get; set; }
        public Node this[int i, int j]
        {
            get => nodes[i + 1, j + 1];
            set => nodes[i + 1, j + 1] = value;
        }
        public int Width => nodes.GetLength(0) - 2;
        public int Height => nodes.GetLength(1) - 2;
        //color is in rgb format
        private Dictionary<int, Node> colorToNode;
        public bool MapWasChanged { get; set; }//set to true after building was added/removed etc
        /// <summary>
        /// Obstacle maps for the current map. Is updated by the UpdateObstacleMaps.
        /// </summary>
        public Dictionary<Movement, ObstacleMap> ObstacleMaps { get; }

        public Map(PixelColor[,] map)
        {
            int width = map.GetLength(0) + 2;
            int height = map.GetLength(1) + 2;
            nodes = new Node[width, height];
            InitializeColorToTerrain();
            for(int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if(i<=0 || i>Width || j<=0 || j > Height)
                        //nodes outside of the map have default biome and maximal nutrients
                        nodes[i, j] = new Node(i, j, Node.MAX_NUTRIENTS, Biome.DEFAULT, Terrain.LAND);
                    else
                        nodes[i, j] = colorToNode[map[i - 1, j - 1].RGB].Copy(i, j);
                }
            }
            ObstacleMaps = new Dictionary<Movement, ObstacleMap>();
            InitializeObstacleMaps();
            MovementGenerator mg = MovementGenerator.GetMovementGenerator();
            mg.SetMapChanged(wpfTest.Players.PLAYER0, ObstacleMaps);
            mg.SetMapChanged(wpfTest.Players.PLAYER1, ObstacleMaps);
        }

        /// <summary>
        /// Copy the terrain of already existing map.
        /// </summary>
        public Map(Map map)
        {
            int width = map.Width;
            int height = map.Height;
            nodes = new Node[width + 2, height + 2];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Node n = map[i, j];
                    this[i, j] = new Node(i, j, n.Nutrients, n.Biome, n.Terrain);
                }
            }
            ObstacleMaps = new Dictionary<Movement, ObstacleMap>();
            InitializeObstacleMaps();
        }

        private void InitializeColorToTerrain()
        {
            colorToNode = new Dictionary<int, Node>();
            colorToNode.Add(new PixelColor(168, 142, 78).RGB, new Node(-1,-1, 0,Biome.DEFAULT,Terrain.LAND));
            colorToNode.Add(new PixelColor(0, 155, 255).RGB, new Node(-1, -1, 0, Biome.DEFAULT, Terrain.SHALLOW_WATER));
            colorToNode.Add(new PixelColor(0, 0, 255).RGB, new Node(-1, -1, 0, Biome.DEFAULT, Terrain.DEEP_WATER));
            colorToNode.Add(new PixelColor(0, 255, 0).RGB, new Node(-1, -1, 
                Terrain.LAND.Nutrients(Biome.RAINFOREST,SoilQuality.LOW), Biome.RAINFOREST, Terrain.LAND));
            colorToNode.Add(new PixelColor(0, 160, 0).RGB, new Node(-1, -1,
                Terrain.LAND.Nutrients(Biome.RAINFOREST, SoilQuality.MEDIUM), Biome.RAINFOREST, Terrain.LAND));
            colorToNode.Add(new PixelColor(106, 103, 29).RGB, new Node(-1, -1,
                Terrain.LAND.Nutrients(Biome.RAINFOREST,SoilQuality.HIGH), Biome.RAINFOREST, Terrain.LAND));
            colorToNode.Add(new PixelColor(213, 180, 99).RGB, new Node(-1, -1,
                Terrain.LAND.Nutrients(Biome.SAVANNA,SoilQuality.LOW), Biome.SAVANNA, Terrain.LAND));
            colorToNode.Add(new PixelColor(0, 255, 137).RGB, new Node(-1, -1,
                Terrain.LAND.Nutrients(Biome.SAVANNA,SoilQuality.MEDIUM), Biome.SAVANNA, Terrain.LAND));
        }

        private void InitializeObstacleMaps()
        {
            ObstacleMaps.Add(Movement.LAND, GetObstacleMap(Movement.LAND));
            ObstacleMaps.Add(Movement.WATER, GetObstacleMap(Movement.WATER));
            ObstacleMaps.Add(Movement.LAND_WATER, GetObstacleMap(Movement.LAND_WATER));
        }

        /// <summary>
        /// Updates ObstacleMaps. Should be called only when the map chaneges.
        /// </summary>
        public void UpdateObstacleMaps()
        {
            ObstacleMaps[Movement.LAND] = GetObstacleMap(Movement.LAND);
            ObstacleMaps[Movement.WATER] = GetObstacleMap(Movement.WATER);
            ObstacleMaps[Movement.LAND_WATER] = GetObstacleMap(Movement.LAND_WATER);
            MapWasChanged = false;
        }

        public ObstacleMap GetObstacleMap(Movement movement)
        {
            ObstacleMap om = new ObstacleMap(Width,Height);
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    Terrain ter = this[i, j].Terrain;
                    switch (movement)
                    {
                        case Movement.LAND:
                            om[i, j] = ter == Terrain.DEEP_WATER ||
                                        this[i,j].Blocked;
                            break;
                        case Movement.WATER:
                            om[i, j] = (ter != Terrain.DEEP_WATER &&
                                        ter != Terrain.SHALLOW_WATER) ||
                                        this[i,j].Blocked;
                            break;
                        case Movement.LAND_WATER:
                            om[i, j] = this[i,j].Blocked;
                            break;
                    }
                }
            return om;
        }

        public ObstacleMap GetViewMap(Players player)
        {
            ObstacleMap om = new ObstacleMap(Width, Height);
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    //player can't see through blocked squares that he doesn't own
                    om[i, j] = this[i, j].Blocked && this[i, j].Building.Player.PlayerID != player;
            return om;
        }

        public void AddBuilding(Building building)
        {
            Node[,] nodes = GameQuerying.GetGameQuerying()
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

        public void RemoveBuilding(Building building)
        {
            foreach (Node n in building.Nodes)
            {
                this[n.X,n.Y].Building = null;
            }
            MapWasChanged = true;
        }

        /// <summary>
        /// One step of nodes producing and sharing nutrients.
        /// </summary>
        public void UpdateNutrients()
        {

            //generate nutrients
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    this[i, j].GenerateNutrients();

            //transfer nutrients
            int width = Width + 2;
            int height = Height + 2;
            decimal[,] newNutrients = new decimal[width, height];
            Node[] neighbours = new Node[4];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    Node current = this[i, j];
                    //initialize neighbours
                    neighbours[0] = this[i + 1, j];
                    neighbours[1] = this[i - 1, j];
                    neighbours[2] = this[i, j + 1];
                    neighbours[3] = this[i, j - 1];

                    //find the neighbour with the least amount of nutrients
                    Node lowestNutr = neighbours[0];
                    foreach(Node n in neighbours)
                    {
                        if (n.Nutrients < lowestNutr.Nutrients)
                            lowestNutr = n;
                    }

                    //transfer only if the neighbour has less nutrients
                    decimal dif = current.Nutrients - lowestNutr.Nutrients;
                    if (dif > 0)
                    {
                        //diffuse nutrients, limited by soil transfer capacity

                        //maximum amount of transported nutrients supported by the soil
                        decimal suppTrans = Math.Min(dif / 2m, current.SoilQuality.TransferCapacity());
                        //maximum amount of transported nutrients so the soil doesn't downgrade
                        decimal noDownTrans = Math.Min(suppTrans, current.Nutrients - current.Terrain.Nutrients(current.Biome,current.SoilQuality));
                        newNutrients[i + 1, j + 1] += current.Nutrients - noDownTrans;
                        newNutrients[lowestNutr.X, lowestNutr.Y] += noDownTrans;
                    }
                    else
                    {
                        newNutrients[i + 1, j + 1] += current.Nutrients;
                    }
                }

            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    this[i, j].Nutrients=newNutrients[i + 1, j + 1];
        }

        /// <summary>
        /// Change biome by the amount of nutrients and neighbour biomes.
        /// </summary>
        public void UpdateBiomes()
        {
            //spread biome to neighbours
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

                    //initialize neighbours
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

                    if (this[i, j].Nutrients >= this[i,j].Terrain.Nutrients(Biome.SAVANNA, SoilQuality.LOW))
                    {
                        //node has enough nutrients to become savanna
                        if (rainMsav < 0)
                            //majority of nondefault neighbour biomes is savanna
                            newBiomes[i, j] = Biome.SAVANNA;
                    }
                    if (this[i, j].Nutrients >= this[i, j].Terrain.Nutrients(Biome.RAINFOREST, SoilQuality.LOW))
                    {
                        //node has enough nutrients to become rainforest
                        if (rainMsav > 0)
                            //majority of nondefault neighbour biomes is rainforest
                            newBiomes[i, j] = Biome.RAINFOREST;
                    }
                }
            
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    this[i, j].Biome = newBiomes[i, j];

            //lose biome if the soil quality is too bad
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    Node n;
                    if ((n = this[i, j]).SoilQuality == SoilQuality.BAD)
                        n.Biome = Biome.DEFAULT;
                }
        }
    }
}
