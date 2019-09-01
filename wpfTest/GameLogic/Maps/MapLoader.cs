using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Maps
{
    /// <summary>
    /// Used for loading map from files.
    /// </summary>
    class MapLoader
    {
        private Dictionary<Color, Biome> ColorToBiome { get; }
        private Dictionary<Color, Terrain> ColorToTerrain { get; }

        public MapLoader()
        {
            ColorToBiome = new Dictionary<Color, Biome>()
            {
                {Color.FromArgb(0, 255, 137), Biome.SAVANNA },
                {Color.FromArgb(0, 160, 0), Biome.RAINFOREST }
            };
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
        public Map LoadMap(string nutrientsMapFileName, string biomesMapFileName,
            string terrainFileName, Game game)
        {
            Bitmap nutrients = new Bitmap(nutrientsMapFileName);
            Bitmap biomes = new Bitmap(biomesMapFileName);
            Bitmap terrain = new Bitmap(terrainFileName);

            int width = nutrients.Width + 2;
            int height = nutrients.Height + 2;

            //create nodes
            Node[,] mapNodes = new Node[width, height];
            for(int i = 0; i<width; i++)
                for(int j=0;j<height; j++)
                {
                    if (i <= 0 || i > width - 2 || j <= 0 || j > height - 2)
                        //nodes outside of the map have default biome and maximal nutrients
                        mapNodes[i, j] = new Node(i - 1, j - 1, Node.MAX_NUTRIENTS, Biome.DEFAULT, Terrain.LAND);
                    else
                    {
                        int x = i - 1; int y = j - 1;
                        int mapsX = x; int mapsY = height - y - 3;
                        Biome biome = GetBiome(biomes.GetPixel(mapsX, mapsY));
                        Terrain terr = GetTerrain(terrain.GetPixel(mapsX, mapsY));
                        decimal nutr = (nutrients.GetPixel(mapsX, mapsY).A/256m) * Node.MAX_NUTRIENTS;
                        decimal minNutr = terr.Nutrients(biome, SoilQuality.LOW);
                        nutr = Math.Max(nutr, minNutr);

                        mapNodes[x + 1, y + 1] = new Node(x, y, nutr, biome, terr);
                    }
                }
            
            return new Map(mapNodes);
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
            for (int i = 0; i < map.Width ; i++)
                for (int j = 0; j < map.Height ; j++)
                {
                    int mapX = i; int mapY = map.Height - (j + 1); 
                    if (buildings.GetPixel(mapX, mapY).A != 0)
                        SetBuilding(buildings.GetPixel(mapX, mapY), map[i, j], game);
                }
        }

        /// <summary>
        /// Returns biome corresponding to the color c.
        /// </summary>
        private Biome GetBiome(Color c)
        {
            if (ColorToBiome.TryGetValue(c, out Biome biome))
                return biome;
            return Biome.DEFAULT;
        }

        /// <summary>
        /// Returns terrain corresponding to the color c.
        /// </summary>
        private Terrain GetTerrain(Color c)
        {
            return ColorToTerrain[c];
        }
        
        /// <summary>
        /// Returns building corresponding to the color c.
        /// </summary>
        private void SetBuilding(Color c, Node n, Game game)
        {
            Color blue = Color.Blue;
            Color red = Color.Red;
            if (c.R == blue.R && c.G==blue.G && c.B==blue.B)
            {
                GameStaticData gsd = game.Players[Players.PLAYER0].GameStaticData;
                //create a new builder that will be used to build the building
                Entity builder = gsd.AnimalFactories.Factorys["TIGER"].NewInstance(game.Players[Players.PLAYER0], new Vector2(0,0));
                gsd.Abilities.BuildBuilding("BAOBAB").NewCommand(builder, n).PerformCommand(game, 0);
            }
            else if (c.R == red.R && c.G == red.G && c.B == red.B)
            {
                GameStaticData gsd = game.Players[Players.PLAYER1].GameStaticData;
                //create a new builder that will be used to build the building
                Entity builder = gsd.AnimalFactories.Factorys["TIGER"].NewInstance(game.Players[Players.PLAYER1], new Vector2(0, 0));
                gsd.Abilities.BuildBuilding("KAPOC").NewCommand(builder, n).PerformCommand(game, 0);
            }
        }
    }
}
