using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SanguineGenesis.GUI
{
    public partial class MainMenuWindow : Form
    {
        /// <summary>
        /// Maximum width of the map.
        /// </summary>
        private const int MAX_MAP_WIDTH = 100;
        /// <summary>
        /// Maximum height of the map.
        /// </summary>
        private const int MAX_MAP_HEIGHT = 100;
        /// <summary>
        /// How many times bigger is the drawing of the map than the map itself.
        /// </summary>
        private int scale;

        public MainMenuWindow()
        {
            InitializeComponent();

            scale = mapPB.Width / MAX_MAP_WIDTH;
            DrawOpt = DrawOption.NO_ACTION;
            LoadNamesOfCreatedMaps();
        }


        private MapDescription MapDescr { get; set; }
        private enum DrawOption
        {
            DRAW_LAND,
            DRAW_SHALLOW_WATER,
            DRAW_DEEP_WATER,
            DRAW_NUTRIENTS,
            ADD_BUILDING,
            REMOVE_BUILDING,
            NO_ACTION
        }
        private DrawOption drawOpt;
        private DrawOption DrawOpt
        {
            get => drawOpt;
            set
            {
                if (value == DrawOption.ADD_BUILDING)
                {
                    buildingSelectionGB.Enabled = true;
                    brushSizeNUD.Enabled = false;
                    nutrientsRateNUD.Enabled = false;
                }
                else
                {
                    buildingSelectionGB.Enabled = false;
                    brushSizeNUD.Enabled = true;
                    if (value == DrawOption.DRAW_NUTRIENTS)
                        nutrientsRateNUD.Enabled = true;
                }
                drawOpt = value;
            }
        }
        private enum BuildingType
        {
            PLAYER_0_MAIN,
            PLAYER_1_MAIN,
            ROCK,
            BIG_ROCK
        }
        private BuildingType BuildingToPlace { get; set; }

        private void NewMapB_Click(object sender, EventArgs e)
        {
            if(!int.TryParse(widthTB.Text, out int width))
            {
                ErrorMessage("The width value \""+widthTB.Text + "\" is not a number.");
                return;
            }
            if(!int.TryParse(heightTB.Text, out int height))
            {
                ErrorMessage("The height value \"" + heightTB.Text + "\" is not a valid number.");
                return;
            }
            if (!(width > 0 && width <= MAX_MAP_WIDTH))
            {

                ErrorMessage("Width has to be between " + 0 + " and " + MAX_MAP_WIDTH + ".");
                return;
            }
            if(!(height > 0 && height <= MAX_MAP_HEIGHT))
            {
                ErrorMessage("Height has to be between " + 0 + " and " + MAX_MAP_HEIGHT + ".");
                return;
            }
            if(ValidName(newNameTB.Text))
            {
                MapDescr = new MapDescription(width, height, newNameTB.Text);
                mapPB.Invalidate();
                mapPB.Refresh();
                EnableEditing();
                mapNameL.Text = MapDescr.Name;
                Message("The map \"" + MapDescr.Name + "\" was created.");
            }
        }

        /// <summary>
        /// Returns true if name can be used as a name of the map. Prints error message.
        /// </summary>
        private bool ValidName(string name)
        {
            if (name == "")
            {
                ErrorMessage("The name of the map cannot be empty.");
                return false;
            }
            foreach(string mapName in mapNamesCB.Items)
                if(mapName == name)
                {
                    ErrorMessage("The name \"" + name + "\" already exits.");
                    return false;
                }
            return true;
        }

        /// <summary>
        /// Returns point in map coordinates. Input coordiantes x and y have to be relative
        /// to the picture box.
        /// </summary>
        private Point MapCoordinates(int x, int y)
        {
            int left = (mapPB.Width - scale * MapDescr.Width) / 2;
            int bottom = (mapPB.Height - scale * MapDescr.Height) / 2;
            return new Point((x - left) / scale, MapDescr.Height - (y - bottom) / scale - 1);
        }

        private void DrawOptionsRB_Click(object sender, EventArgs e)
        {
            switch (((RadioButton)sender).Name)
            {
                case "deepWaterRB":
                    DrawOpt = DrawOption.DRAW_DEEP_WATER;
                    break;
                case "shallowWaterRB":
                    DrawOpt = DrawOption.DRAW_SHALLOW_WATER;
                    break;
                case "landRB":
                    DrawOpt = DrawOption.DRAW_LAND;
                    break;
                case "nutrientsRB":
                    DrawOpt = DrawOption.DRAW_NUTRIENTS;
                    break;
                case "addBuildingRB":
                    DrawOpt = DrawOption.ADD_BUILDING;
                    break;
                case "removeBuildingRB":
                    DrawOpt = DrawOption.REMOVE_BUILDING;
                    break;
            }
        }

        private void MapPB_Paint(object sender, PaintEventArgs e)
        {
            if (MapDescr != null)
            {
                Graphics g = e.Graphics;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                int left = (mapPB.Width - scale * MapDescr.Width) / 2;
                int bottom = (mapPB.Height - scale * MapDescr.Height) / 2;
                Bitmap total = MapDescr.TotalMap();
                g.DrawImage(total, left + scale/2 , bottom - scale/2 + scale * MapDescr.Height, scale * total.Width, -scale * total.Height);
            }
        }

        private void BuildingRB_Click(object sender, EventArgs e)
        {
            switch (((RadioButton)sender).Name)
            {
                case "main0RB":
                    BuildingToPlace = BuildingType.PLAYER_0_MAIN;
                    break;
                case "main1RB":
                    BuildingToPlace = BuildingType.PLAYER_1_MAIN;
                    break;
                case "rockRB":
                    BuildingToPlace = BuildingType.ROCK;
                    break;
                case "bigRockRB":
                    BuildingToPlace = BuildingType.BIG_ROCK;
                    break;
            }
        }

        private void MapPB_MouseDownAction(object sender, MouseEventArgs e)
        {
            if (MapDescr == null)
                return;

            Point mapPoint = MapCoordinates(e.X, e.Y);
            coordinatesL.Text = "X = " + mapPoint.X + " ; Y = " + mapPoint.Y;

            if (e.Button == MouseButtons.Left)
            {
                if (MapDescr.ValidCoordinates(mapPoint.X, mapPoint.Y))
                {
                    int size = (int)brushSizeNUD.Value;//of brush
                    int x = mapPoint.X - size / 2;
                    int y = mapPoint.Y - size / 2;
                    switch (DrawOpt)
                    {
                        case DrawOption.DRAW_DEEP_WATER:
                            MapDescr.DrawTerrain(Color.FromArgb(0, 0, 255), x, y, size, size);
                            break;
                        case DrawOption.DRAW_SHALLOW_WATER:
                            MapDescr.DrawTerrain(Color.FromArgb(0, 155, 255), x, y, size, size);
                            break;
                        case DrawOption.DRAW_LAND:
                            MapDescr.DrawTerrain(Color.FromArgb(168, 142, 78), x, y, size, size);
                            break;
                        case DrawOption.DRAW_NUTRIENTS:
                            MapDescr.AddNutrients(x, y, size, size, (int)nutrientsRateNUD.Value);
                            break;
                        case DrawOption.ADD_BUILDING:
                            string error = MapDescr.AddBuilding(BuildingToPlace.ToString(), mapPoint.X, mapPoint.Y);
                            if (error != null)
                                ErrorMessage(error);
                            break;
                        case DrawOption.REMOVE_BUILDING:
                            MapDescr.RemoveBuilding(mapPoint.X, mapPoint.Y);
                            break;
                    }
                }
                mapPB.Refresh();
            }
        }

        private void SaveB_Click(object sender, EventArgs e)
        {
            if (MapDescr != null)
            {
                MapDescr.Save();
                DisableEditing();
                LoadNamesOfCreatedMaps();
                Message("The map \"" + MapDescr.Name + "\" was saved.");
            }
            else
                ErrorMessage("No map is loaded.");
        }

        private void LoadB_Click(object sender, EventArgs e)
        {
            string mapDirName = mapNamesCB.Text;
            if (mapDirName == "")
            {
                ErrorMessage("Empty string can't be a name of a map.");
                return;
            }

            try
            {
                MapDescr = new MapDescription(mapDirName);
                DisableEditing();
                mapNameL.Text = MapDescr.Name;
                Message("The map \"" + mapDirName + "\" was loaded.");
            }
            catch(Exception)
            {
                ErrorMessage("Map \"" + mapDirName + "\" was not loaded, because it doesn't exist or has incorrect format.");
            }
            mapPB.Refresh();
        }

        private void DeleteB_Click(object sender, EventArgs e)
        {
            string dirName = MapDescription.DIRECTORY + mapNamesCB.Text;
            if (Directory.Exists(dirName))
            {
                Directory.Delete(dirName, true);
                LoadNamesOfCreatedMaps();
                Message("The map \"" + mapNamesCB.Text + "\" was deleted.");
            }
            else
            {
                ErrorMessage("No map is loaded.");
            }
        }

        /// <summary>
        /// Loads names of maps in MapDescription.DIRECTORY to combo box mapNamesCB.
        /// </summary>
        private void LoadNamesOfCreatedMaps()
        {
            string dirName = MapDescription.DIRECTORY;
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            mapNamesCB.Items.Clear();
            foreach (var d in Directory.GetDirectories(dirName))
            {
                mapNamesCB.Items.Add(Path.GetFileName(d));
            }
        }

        /// <summary>
        /// Prints error message.
        /// </summary>
        private void ErrorMessage(string message)
        {
            errorMessageL.ForeColor = Color.Red;
            errorMessageL.Text = message;
        }

        /// <summary>
        /// Prints message.
        /// </summary>
        private void Message(string message)
        {
            errorMessageL.ForeColor = Color.Green;
            errorMessageL.Text = message;
        }

        private void EditB_Click(object sender, EventArgs e)
        {
            if (MapDescr != null)
                EnableEditing();
            else
                ErrorMessage("No map is loaded.");
        }

        /// <summary>
        /// Disables editing of the map.
        /// </summary>
        private void DisableEditing()
        {
            drawOptionsGB.Enabled = false;
            DrawOpt = DrawOption.NO_ACTION;
        }

        /// <summary>
        /// Enables editing of the map.
        /// </summary>
        private void EnableEditing()
        {
            drawOptionsGB.Enabled = true;
            if (deepWaterRB.Checked)
                DrawOpt = DrawOption.DRAW_DEEP_WATER;
            else if (shallowWaterRB.Checked)
                DrawOpt = DrawOption.DRAW_SHALLOW_WATER;
            else if (landRB.Checked)
                DrawOpt = DrawOption.DRAW_LAND;
            else if (nutrientsRB.Checked)
                DrawOpt = DrawOption.DRAW_NUTRIENTS;
            else if (addBuildingRB.Checked)
                DrawOpt = DrawOption.ADD_BUILDING;
            else if (removeBuildingRB.Checked)
                DrawOpt = DrawOption.REMOVE_BUILDING;
        }

        /// <summary>
        /// Returns Biome to be played by the player.
        /// </summary>
        private Biome PlayersBiome => savannaRB.Checked ? Biome.SAVANNA : Biome.RAINFOREST;

        private void PlayB_Click(object sender, EventArgs e)
        {
            if (MapDescr != null)
            {
                if (MapDescr.MainBuildingsPresent())
                {
                    var gameWindow = new MainWinformWindow(MapDescr, PlayersBiome);
                    gameWindow.ShowDialog();
                }
                else
                    ErrorMessage("The map doesn't contain the main buildings of both players.");
            }
            else
                ErrorMessage("Load or create a map before starting the game.");
        }
    }

    struct BuildingDescriptor
    {
        public int X { get; }
        public int Y { get; }
        public string Type { get; }

        public BuildingDescriptor(string type, int x, int y)
        {
            Type = type;
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return X + " " + Y + " " + Type;
        }
    }

    class MapDescription
    {
        /// <summary>
        /// Name of directory where maps are saved.
        /// </summary>
        public const string DIRECTORY = "Maps/";
        public Bitmap TerrainMap { get; }
        public Bitmap NutrientsMap { get; }
        private Bitmap BuildingsLocations { get; }
        public List<BuildingDescriptor> Buildings { get; }
        public string Name { get; }
        public int Width => TerrainMap.Width;
        public int Height => TerrainMap.Height;

        public MapDescription(int width, int height, string name)
        {
            Name = name;
            TerrainMap = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    TerrainMap.SetPixel(i, j, Color.FromArgb(168, 142, 78));
                }
            NutrientsMap = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    NutrientsMap.SetPixel(i, j, Color.White);
                }
            BuildingsLocations = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    BuildingsLocations.SetPixel(i, j, Color.White);
                }
            Buildings = new List<BuildingDescriptor>();
        }

        /// <summary>
        /// Visualization of the map.
        /// </summary>
        public Bitmap TotalMap()
        {
            Bitmap total = new Bitmap(Width, Height);
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    Color terC = TerrainMap.GetPixel(i, j);
                    Color nutC = NutrientsMap.GetPixel(i, j);
                    float brightness = nutC.R / 256f;
                    Color newColor = Color.FromArgb((int)(terC.R * brightness),
                        (int)(terC.G * brightness),
                        (int)(terC.B * brightness));
                    total.SetPixel(i, j, newColor);
                }
            foreach (var bd in Buildings)
            {
                switch (bd.Type)
                {
                    case "PLAYER_0_MAIN":
                        FillRectWithColor(total, Color.Blue, bd.X, bd.Y, 3, 3);
                        break;
                    case "PLAYER_1_MAIN":
                        FillRectWithColor(total, Color.Red, bd.X, bd.Y, 3, 3);
                        break;
                    case "ROCK":
                        FillRectWithColor(total, Color.Gray, bd.X, bd.Y, 1, 1);
                        break;
                    case "BIG_ROCK":
                        FillRectWithColor(total, Color.DarkGray, bd.X, bd.Y, 2, 2);
                        break;
                }
            }
            return total;
        }

        private bool TryGetBuildingSize(string buildingType, out int width, out int height)
        {
            switch (buildingType)
            {
                case "PLAYER_0_MAIN":
                    width = 3;
                    height = 3;
                    return true;
                case "PLAYER_1_MAIN":
                    width = 3;
                    height = 3;
                    return true;
                case "ROCK":
                    width = 1;
                    height = 1;
                    return true;
                case "BIG_ROCK":
                    width = 2;
                    height = 2;
                    return true;
            }
            width = height = 0;
            return false;
        }

        /// <summary>
        /// Returns true iff the map contains main buildings of both players.
        /// </summary>
        public bool MainBuildingsPresent()
        {
            return Buildings.Where(bd => bd.Type == "PLAYER_0_MAIN").Any() &&
                Buildings.Where(bd => bd.Type == "PLAYER_1_MAIN").Any();
        }

        /// <summary>
        /// Fills the rectangle in bitmap with color c. Coordiantes can be out of range.
        /// </summary>
        private void FillRectWithColor(Bitmap bitmap, Color c, int x, int y, int width, int height)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    int xx = x + i;
                    int yy = y + j;
                    if (ValidCoordinates(xx, yy))
                    {
                        bitmap.SetPixel(xx, yy, c);
                    }
                }
        }

        /// <summary>
        /// Fills the terrain map in the given rectangle with the color c but 
        /// ignores the squares with buildings. Coordiantes can be out of range.
        /// </summary>
        private void FillTerrainIgnoreBuildings(Color c, int x, int y, int width, int height)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    int xx = x + i;
                    int yy = y + j;
                    if (ValidCoordinates(xx, yy))
                    {
                        if (ColorsEqual(BuildingsLocations.GetPixel(xx, yy), Color.White))
                            TerrainMap.SetPixel(xx, yy, c);
                    }
                }
        }

        /// <summary>
        /// Adds the rectangle in bitmap with color c. Coordiantes can be out of range. Intensity
        /// is between 0 and 255.
        /// </summary>
        private void AddRectIntensity(Bitmap bitmap, int intensity, int x, int y, int width, int height)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    int xx = x + i;
                    int yy = y + j;
                    if (xx >= 0 && xx < bitmap.Width && yy >= 0 && yy < bitmap.Height)
                    {
                        Color color = bitmap.GetPixel(xx, yy);
                        bitmap.SetPixel(xx, yy,
                            Color.FromArgb(
                                Math.Max(color.R + intensity, 0),
                                Math.Max(color.G + intensity, 0),
                                Math.Max(color.B + intensity, 0)));
                    }
                }
        }

        /// <summary>
        /// Returns true iff the coordinate on the map is valid.
        /// </summary>
        public bool ValidCoordinates(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        /// <summary>
        /// Draws the color to the terrain map.
        /// </summary>
        public void DrawTerrain(Color color, int x, int y, int width, int height)
        {
            FillTerrainIgnoreBuildings(color, x, y, width, height);
        }

        /// <summary>
        /// Draws to the nutrients map.
        /// </summary>
        public void AddNutrients(int x, int y, int width, int height, int intensity)
        {
            AddRectIntensity(NutrientsMap, -intensity, x, y, width, height);
        }

        /// <summary>
        /// Returns true iff building of width and height can be placed to coordinates x,y.
        /// </summary>
        private bool CanBePlacedBuilding(int x, int y, int width, int height)
        {
            if (ValidCoordinates(x, y) &&
                ValidCoordinates(x + width - 1, y + height - 1))
            {
                for (int i = x; i < x + width; i++)
                    for (int j = y; j < y + height; j++)
                        if (ColorsEqual(BuildingsLocations.GetPixel(i, j), Color.Black) ||
                            !ColorsEqual(TerrainMap.GetPixel(i, j), Color.FromArgb(168, 142, 78)))
                            return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a new building of given type to the map. Returns error message.
        /// </summary>
        public string AddBuilding(string type, int x, int y)
        {
            if (TryGetBuildingSize(type, out int width, out int height))
            {
                if (type == "PLAYER_0_MAIN" &&
                    Buildings.Where(bd => bd.Type == type).Any())
                    return "Main building for player 0 was already placed.";

                if (type == "PLAYER_1_MAIN" &&
                    Buildings.Where(bd => bd.Type == type).Any())
                    return "Main building for player 1 was already placed.";

                if (CanBePlacedBuilding(x, y, width, height))
                {
                    Buildings.Add(new BuildingDescriptor(type.ToString(), x, y));
                    FillRectWithColor(BuildingsLocations, Color.Black, x, y, width, height);
                    return null;
                }
                else
                    return "The building cannot be placed at the coordinates (" + x + ", " + y + ").";
            }
            else
                return "The building with a type \"" + type + "\" doesn't exist.";
        }

        /// <summary>
        /// Removes the building that lies on position (x,y).
        /// </summary>
        public void RemoveBuilding(int x, int y)
        {
            if (!ValidCoordinates(x, y))
                return;

            //the square isn't occupied by any building
            if (ColorsEqual(BuildingsLocations.GetPixel(x, y), Color.White))
                return;

            BuildingDescriptor? toRemove = null;
            foreach (var bd in Buildings)
            {
                TryGetBuildingSize(bd.Type, out int width, out int height);
                if (bd.X <= x && x < bd.X + width && bd.Y <= y && y < bd.Y + height)
                {
                    toRemove = bd;
                    FillRectWithColor(BuildingsLocations, Color.White, bd.X, bd.Y, width, height);
                    break;
                }
            }
            if (toRemove.HasValue)
                Buildings.Remove(toRemove.Value);
        }

        /// <summary>
        /// Creates a new directory with this map's name and puts there this map's data.
        /// </summary>
        public void Save()
        {
            string dirName = DIRECTORY + Name;
            if (Directory.Exists(dirName))
                foreach (var f in Directory.GetFiles(dirName))
                    File.Delete(f);

            Directory.CreateDirectory(dirName);
            using (var terrainWriter = new FileStream(dirName + "/terrain.bmp", FileMode.OpenOrCreate))
            using (var nutrientsWriter = new FileStream(dirName + "/nutrients.bmp", FileMode.OpenOrCreate))
            using (var sw = new StreamWriter(dirName + "/buildings.txt"))
            {
                ImageConverter ic = new ImageConverter();
                var terBytes = (byte[])ic.ConvertTo(TerrainMap, typeof(byte[]));
                terrainWriter.Write(terBytes, 0, terBytes.Length);
                var nutBytes = (byte[])ic.ConvertTo(NutrientsMap, typeof(byte[]));
                nutrientsWriter.Write(nutBytes, 0, nutBytes.Length);
                foreach (var building in Buildings)
                    sw.WriteLine(building);
            }
        }

        /// <summary>
        /// Loads map from the directory.
        /// </summary>
        /// <param name="mapDirectoryName">Name of the directory.</param>
        /// <exception cref="IOException">Thrown if the directory or some of the required files don't exist.</exception>
        public MapDescription(string mapDirectoryName)
        {
            string dirName = DIRECTORY + mapDirectoryName;

            if (!Directory.Exists(dirName))
                throw new IOException("Directory " + dirName + " doesn't exist!");

            using (var terrainReader = new FileStream(dirName + "/terrain.bmp", FileMode.Open))
            using (var nutrientsReader = new FileStream(dirName + "/nutrients.bmp", FileMode.Open))
            using (var sr = new StreamReader(dirName + "/buildings.txt"))
            {
                TerrainMap = new Bitmap(terrainReader);
                NutrientsMap = new Bitmap(nutrientsReader);
                BuildingsLocations = new Bitmap(TerrainMap.Width, TerrainMap.Height);
                for (int i = 0; i < BuildingsLocations.Width; i++)
                    for (int j = 0; j < BuildingsLocations.Height; j++)
                    {
                        BuildingsLocations.SetPixel(i, j, Color.White);
                    }

                Buildings = new List<BuildingDescriptor>();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] param = line.Split(' ');
                    int x = int.Parse(param[0]);
                    int y = int.Parse(param[1]);
                    string type = param[2];
                    AddBuilding(type, x, y);
                }
            }
            Name = mapDirectoryName;
        }

        private static bool ColorsEqual(Color a, Color b) => a.R == b.R && a.G == b.G && a.B == b.B;
    }
}
