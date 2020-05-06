using SanguineGenesis.GameLogic.Data;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SanguineGenesis.GUI.MainMenuWindow;

namespace SanguineGenesis.GameLogic.Maps
{
    /// <summary>
    /// Used for loading map from files.
    /// </summary>
    class MapLoader
    {
        /// <summary>
        /// Terrain types corresponding to the colors in the terrain map.
        /// </summary>
        private Dictionary<Color, Terrain> ColorToTerrain { get; }

        /// <summary>
        /// Description of the map used for generating the game map.
        /// </summary>
        private MapDescription MapDescription { get; }

        public MapLoader(MapDescription mapDescription)
        {
            MapDescription = mapDescription;

            ColorToTerrain = new Dictionary<Color, Terrain>()
            {
                {MapDescription.LandColor, Terrain.LAND },
                {MapDescription.ShallowWaterColor, Terrain.SHALLOW_WATER },
                {MapDescription.DeepWaterColor, Terrain.DEEP_WATER }
            };
        }

        /// <summary>
        /// Loads information about nodes from the MapDescription.
        /// </summary>
        public Map LoadMap()
        {
            int width = MapDescription.Width + 2;
            int height = MapDescription.Height + 2;

            //create nodes
            Node[,] mapNodes = new Node[width, height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if (i <= 0 || i > width - 2 || j <= 0 || j > height - 2)
                        //nodes outside of the map have default biome and maximal nutrients
                        mapNodes[i, j] = new Node(i - 1, j - 1, Node.MAX_PASSIVE_NUTRIENTS, Node.MAX_ACTIVE_NUTRIENTS, Biome.DEFAULT, Terrain.LAND);
                    else
                    {
                        int x = i - 1; int y = j - 1;
                        int mapsX = x; int mapsY = y;
                        Terrain terr = GetTerrain(MapDescription.GetTerrain(mapsX, mapsY));
                        float nutr = ((256 - MapDescription.GetNutrients(mapsX, mapsY).R) / 256f) * Node.MAX_PASSIVE_NUTRIENTS;

                        mapNodes[x + 1, y + 1] = new Node(x, y, nutr, 0, Biome.DEFAULT, terr);
                    }
                }

            return new Map(mapNodes);
        }

        /// <summary>
        /// Loads buildings to the game, MapDescription has to have same size
        /// as the game map. If game.Map is null, exception is thrown.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if game.Map is null.</exception>
        public void LoadBuildings(Game game)
        {
            if (game.Map == null)
                throw new ArgumentException("Map has to be loaded before loading buildings.");

            Map map = game.Map;
            List<BuildingDescriptor> buildings = MapDescription.GetBuildings;
            //add buildings
            foreach (BuildingDescriptor bd in buildings)
            {
                SetBuilding(bd.Type, map[bd.X, bd.Y], game);
            }
        }

        /// <summary>
        /// Returns terrain corresponding to the color c. Returns Terrain.LAND for
        /// incorrect color.
        /// </summary>
        private Terrain GetTerrain(Color c)
        {
            if (ColorToTerrain.TryGetValue(c, out Terrain terrain))
                return terrain;
            else
                return Terrain.LAND;
        }

        /// <summary>
        /// Places a building of the given type to the map.
        /// </summary>
        private void SetBuilding(BuildingType type, Node n, Game game)
        {
            if (type == BuildingType.PLAYER_0_MAIN)
            {
                //main building of player 0
                Player player = game.Players[FactionType.PLAYER0];
                PlaceMainBuildingOfPlayer(n, game, player);
            }
            else if (type == BuildingType.PLAYER_1_MAIN)
            {
                //main building of player 1
                Player player = game.Players[FactionType.PLAYER1];
                PlaceMainBuildingOfPlayer(n, game, player);
            }
            else if (type == BuildingType.ROCK)
            {
                //neutral rock
                GameData gd = game.GameData;
                game.Map.PlaceBuilding(gd.StructureFactories["ROCK"], game.NeutralFaction, n.X, n.Y, game);
            }
            else if (type == BuildingType.BIG_ROCK)
            {
                //neutral big rock
                GameData gd = game.GameData;
                game.Map.PlaceBuilding(gd.StructureFactories["BIG_ROCK"], game.NeutralFaction, n.X, n.Y, game);

            }
        }

        /// <summary>
        /// Places main building of player to node.
        /// </summary>
        private void PlaceMainBuildingOfPlayer(Node n, Game game, Player player)
        {
            //main buildings can only stand on land
            if (n.Terrain != Terrain.LAND)
                return;

            BuildingFactory buildingFactory = player.GetMainBuildingFactory(game);

            //set correct biome and number of nutrients
            int size = buildingFactory.Size;
            Node[,] buildNodes = GameQuerying.SelectNodes(game.Map, n.X, n.Y, n.X + (size - 1), n.Y + (size - 1));
            float minNutr = n.Terrain.Nutrients(player.Biome, SoilQuality.LOW);
            for (int i = 0; i < buildNodes.GetLength(0); i++)
                for (int j = 0; j < buildNodes.GetLength(1); j++)
                {
                    buildNodes[i, j].Biome = player.Biome;
                    buildNodes[i, j].ActiveNutrients = minNutr;
                }

            //place the building on the map
            game.Map.PlaceBuilding(buildingFactory, player, n.X, n.Y, game);

            //set it to max energy and set its root nodes to be in the same biome
            var mainBuilding = player.GetAll<Plant>().Where(t => t.EntityType == buildingFactory.EntityType).FirstOrDefault();
            if (mainBuilding != null)
            {
                mainBuilding.Energy = buildingFactory.MaxEnergy;
                float minBiomeNutrients = mainBuilding.Terrain.Nutrients(mainBuilding.Biome, SoilQuality.LOW);
                foreach(var rootNode in mainBuilding.RootNodes)
                {
                    //set biome
                    rootNode.Biome = player.Biome;
                    //set minimum nutrients for the biome
                    rootNode.ActiveNutrients = minBiomeNutrients;
                }
            }
        }
    }
}
