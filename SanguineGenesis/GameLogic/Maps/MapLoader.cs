using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Maps
{
    /// <summary>
    /// Used for loading map from files.
    /// </summary>
    class MapLoader
    {
        private Dictionary<Color, Terrain> ColorToTerrain { get; }
        private Dictionary<Color, Building> ColorToBuilding { get; }

        public MapLoader()
        {
            ColorToTerrain = new Dictionary<Color, Terrain>()
            {
                {Color.FromArgb(168, 142, 78), Terrain.LAND },
                {Color.FromArgb(0, 155, 255), Terrain.SHALLOW_WATER },
                {Color.FromArgb(0, 0, 255), Terrain.DEEP_WATER }
            };
        }

        /// <summary>
        /// Loads information about nodes from the files.
        /// </summary>
        public Map LoadMap(string nutrientsMapFileName,
            string terrainFileName, Game game)
        {
            Bitmap nutrients = new Bitmap(nutrientsMapFileName);
            Bitmap terrain = new Bitmap(terrainFileName);

            int width = nutrients.Width + 2;
            int height = nutrients.Height + 2;

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
                        int mapsX = x; int mapsY = height - y - 3;
                        Terrain terr = GetTerrain(terrain.GetPixel(mapsX, mapsY));
                        decimal nutr = (nutrients.GetPixel(mapsX, mapsY).A / 256m) * Node.MAX_NUTRIENTS;

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
        public void LoadBuildings(Game game, string buildingsMapFileName)
        {
            Map map = game.Map;
            Bitmap buildings = new Bitmap(buildingsMapFileName);
            //add buildings
            for (int i = 0; i < map.Width; i++)
                for (int j = map.Height - 1; j >= 0; j--)
                {
                    int mapX = i; int mapY = j;
                    if (buildings.GetPixel(mapX, mapY).A != 0)
                        SetBuilding(buildings.GetPixel(mapX, mapY), map[i, map.Height - (j + 1)], game, buildings);
                }
        }

        /// <summary>
        /// Returns building corresponding to the color c.
        /// </summary>
        private void SetBuilding(Color c, Node n, Game game, Bitmap buildings)
        {
            if (c.Is(Color.Blue))
            {
                //main building of player 0
                Player player = game.Players[FactionType.PLAYER0];
                PlaceMainBuildingOfPlayer(n, game, player);
            }
            else if (c.Is(Color.Red))
            {
                //main building of player 1
                Player player = game.Players[FactionType.PLAYER1];
                PlaceMainBuildingOfPlayer(n, game, player);
            }
            else if (c.Is(Color.Black))
            {
                //rock
                int x = n.X; int y = buildings.Height - (n.Y + 1);
                GameStaticData gsd = game.NeutralFaction.GameStaticData;
                bool bigRock = false;
                //if there is enough space, place big rock instead of the small one
                if ((x + 1 < buildings.Width) && (y - 1 >= 0))
                {
                    if (buildings.GetPixel(x + 1, y).Is(Color.Black) &&
                       buildings.GetPixel(x, y - 1).Is(Color.Black) &&
                       buildings.GetPixel(x + 1, y - 1).Is(Color.Black))
                        bigRock = true;
                }
                if (bigRock)
                    game.Map.PlaceBuilding(gsd.StructureFactories["BIG_ROCK"], game.NeutralFaction, n.X, n.Y);
                else
                    game.Map.PlaceBuilding(gsd.StructureFactories["ROCK"], game.NeutralFaction, n.X, n.Y);

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

    public static class ColorExtensions
    {
        /// <summary>
        /// Returns true if the color is the same. Doesn't compare alpha channel.
        /// </summary>
        public static bool Is(this Color a, Color b) => a.R == b.R && a.G == b.G && a.B == b.B;
    }
}
