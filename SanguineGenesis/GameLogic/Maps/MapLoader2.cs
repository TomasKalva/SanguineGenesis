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
    class MapLoader2
    {
        private Dictionary<Color, Terrain> ColorToTerrain { get; }
        private Dictionary<Color, Building> ColorToBuilding { get; }

        private MapDescription MapDescription { get; }

        public MapLoader2(MapDescription mapDescription)
        {
            MapDescription = mapDescription;

            ColorToTerrain = new Dictionary<Color, Terrain>()
            {
                {Color.FromArgb(168, 142, 78), Terrain.LAND },
                {Color.FromArgb(0, 155, 255), Terrain.SHALLOW_WATER },
                {Color.FromArgb(0, 0, 255), Terrain.DEEP_WATER }
            };
        }

        /// <summary>
        /// Loads information about nodes from the MapDescription.
        /// </summary>
        public Map LoadMap()
        {
            //Bitmap nutrients = MapDescription.NutrientsMap;
            //Bitmap terrain = MapDescription.TerrainMap;

            int width = MapDescription.Width + 2;
            int height = MapDescription.Height + 2;

            //create nodes
            Node[,] mapNodes = new Node[width, height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if (i <= 0 || i > width - 2 || j <= 0 || j > height - 2)
                        //nodes outside of the map have default biome and maximal nutrients
                        mapNodes[i, j] = new Node(i - 1, j - 1, Node.MAX_NUTRIENTS, Biome.DEFAULT, Terrain.LAND);
                    else
                    {
                        int x = i - 1; int y = j - 1;
                        int mapsX = x; int mapsY = y;
                        Terrain terr = GetTerrain(MapDescription.GetTerrain(mapsX, mapsY));
                        decimal nutr = ((256 - MapDescription.GetNutrients(mapsX, mapsY).R) / 256m) * Node.MAX_NUTRIENTS;

                        mapNodes[x + 1, y + 1] = new Node(x, y, nutr, Biome.DEFAULT, terr);
                    }
                }

            return new Map(mapNodes);
        }

        /// <summary>
        /// Returns terrain corresponding to the color c.
        /// </summary>
        private Terrain GetTerrain(Color c)
        {
            return ColorToTerrain[c];
        }

        /// <summary>
        /// Loads buildings to the game, the image with map of buildings has to have the same size
        /// as the game map.
        /// </summary>
        public void LoadBuildings(Game game)
        {
            Map map = game.Map;
            List<BuildingDescriptor> buildings = MapDescription.GetBuildings;
            //add buildings
            foreach(BuildingDescriptor bd in buildings)
            {
                SetBuilding(bd.Type, map[bd.X, bd.Y], game);
            }
        }

        /// <summary>
        /// Places a building of the given type to the map.
        /// </summary>
        private void SetBuilding(string type, Node n, Game game)
        {
            if (type == "PLAYER_0_MAIN")
            {
                //main building of player 0
                Player player = game.Players[FactionType.PLAYER0];
                PlaceMainBuildingOfPlayer(n, game, player);
            }
            else if (type == "PLAYER_1_MAIN")
            {
                //main building of player 1
                Player player = game.Players[FactionType.PLAYER1];
                PlaceMainBuildingOfPlayer(n, game, player);
            }
            else if (type == "ROCK")
            {
                GameStaticData gsd = game.NeutralFaction.GameStaticData;
                game.Map.PlaceBuilding(gsd.StructureFactories["ROCK"], game.NeutralFaction, n.X, n.Y);
            }
            else if (type == "BIG_ROCK")
            {
                GameStaticData gsd = game.NeutralFaction.GameStaticData;
                game.Map.PlaceBuilding(gsd.StructureFactories["BIG_ROCK"], game.NeutralFaction, n.X, n.Y);

            }
        }

        private void PlaceMainBuildingOfPlayer(Node n, Game game, Player player)
        {
            //main buildings can only stand on land
            if (n.Terrain != Terrain.LAND)
                return;

            BuildingFactory buildingFactory = player.GetMainBuildingFactory();

            int size = buildingFactory.Size;
            Node[,] buildNodes = GameQuerying.SelectNodes(game.Map, n.X, n.Y, n.X + (size - 1), n.Y + (size - 1));
            decimal minNutr = n.Terrain.Nutrients(player.Biome, SoilQuality.LOW);
            for (int i = 0; i < buildNodes.GetLength(0); i++)
                for (int j = 0; j < buildNodes.GetLength(1); j++)
                {
                    buildNodes[i, j].Biome = player.Biome;
                    buildNodes[i, j].Nutrients = minNutr;
                }
            game.Map.PlaceBuilding(buildingFactory, player, n.X, n.Y);
        }
    }
}
