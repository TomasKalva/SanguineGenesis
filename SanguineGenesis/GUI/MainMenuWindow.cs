using SanguineGenesis.GameLogic;
using SanguineGenesis.GUI.WinFormsComponents;
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
        /// Minimum width of the map.
        /// </summary>
        private const int MIN_MAP_WIDTH = 30;
        /// <summary>
        /// Maximum width of the map.
        /// </summary>
        private const int MAX_MAP_WIDTH = 100;
        /// <summary>
        /// Minimum height of the map.
        /// </summary>
        private const int MIN_MAP_HEIGHT = 30;
        /// <summary>
        /// Maximum height of the map.
        /// </summary>
        private const int MAX_MAP_HEIGHT = 100;
        /// <summary>
        /// How many times bigger is the drawing of the map than the map itself.
        /// </summary>
        private int MapScale { get; }
        /// <summary>
        /// Icons used for the game gui.
        /// </summary>
        private Icons Icons { get; }

        public MainMenuWindow()
        {
            InitializeComponent();
            heightNUD.Minimum = MIN_MAP_HEIGHT;
            widthNUD.Minimum = MIN_MAP_WIDTH;
            heightNUD.Maximum = MAX_MAP_HEIGHT;
            widthNUD.Maximum = MAX_MAP_WIDTH;

            MapScale = mapPB.Width / MAX_MAP_WIDTH;
            DrawOpt = DrawOption.NO_ACTION;
            Icons = new Icons();
            LoadNamesOfCreatedMaps();
            try { this.Icon = new Icon("Images/Icons/giraffe.ico"); } catch (IOException)
            {
                Console.WriteLine("Icon can't be loaded.");
            }
        }

        /// <summary>
        /// Description of map that can be edited and used to start a new game.
        /// </summary>
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
        /// <summary>
        /// Backing field for DrawOpt.
        /// </summary>
        private DrawOption drawOpt;
        /// <summary>
        /// Determines what will be done when player clicks on the map.
        /// </summary>
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

        /// <summary>
        /// Building that will be placed to the map on click while being in ADD_BUILDING mode.
        /// </summary>
        private BuildingType BuildingToPlace { get; set; }

        /// <summary>
        /// Returns true if name can be used as a name of the map. Prints error message.
        /// </summary>
        private bool ValidName(string name)
        {
            //check emptyness
            if (name == "")
            {
                ErrorMessage("The name of the map cannot be empty.");
                return false;
            }
            //check if name contains only alphanumeric characters and '_'
            foreach(char a in name)
                if (!(char.IsLetterOrDigit(a) || a=='_'))
                {
                    ErrorMessage("The name of the map can only contain alphanumeric characters and '_'.");
                    return false;
                }
            return true;
        }

        /// <summary>
        /// Returns true iff the name of the map already exists.
        /// </summary>
        public bool MapAlreadyExists(string name)
        {
            //check if the name already exists
            foreach (string mapName in mapNamesCB.Items)
                if (mapName == name)
                {
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Returns point in map coordinates. Input coordiantes x and y have to be relative
        /// to the picture box.
        /// </summary>
        private Point MapCoordinates(int x, int y)
        {
            int left = (mapPB.Width - MapScale * MapDescr.Width) / 2;
            int bottom = (mapPB.Height - MapScale * MapDescr.Height) / 2;
            return new Point((x - left) / MapScale, MapDescr.Height - (y - bottom) / MapScale - 1);
        }

        /// <summary>
        /// Loads names of maps in MapDescription.DIRECTORY to combo box mapNamesCB.
        /// </summary>
        private void LoadNamesOfCreatedMaps()
        {
            string dirName = MapDescription.DIRECTORY;
            try
            {
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);

                mapNamesCB.Items.Clear();
                foreach (var d in Directory.GetDirectories(dirName))
                {
                    string name = Path.GetFileName(d);
                    if (ValidName(name))
                        mapNamesCB.Items.Add(name);
                }
            }
            catch(IOException)
            {
                ErrorMessage("List of maps can't be loaded.");
            }
            //clear error messages
            Message("");
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

        /// <summary>
        /// Disables editing of the map.
        /// </summary>
        private void DisableEditing()
        {
            drawOptionsGB.Enabled = false;
            DrawOpt = DrawOption.NO_ACTION;

            if (MapDescr != null)
                ((IFreezable)MapDescr).Freeze();
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

            if (MapDescr != null)
                ((IFreezable)MapDescr).Unfreeze();
        }

        /// <summary>
        /// Returns Biome to be played by the player.
        /// </summary>
        private Biome PlayersBiome => savannaRB.Checked ? Biome.SAVANNA : Biome.RAINFOREST;
        
        #region Event handlers

        private void NewMapB_Click(object sender, EventArgs e)
        {
            int width = (int)widthNUD.Value;
            int height = (int)heightNUD.Value;
            if (!(width >= MIN_MAP_WIDTH && width <= MAX_MAP_WIDTH))
            {
                ErrorMessage("Width has to be between " + MIN_MAP_WIDTH + " and " + MAX_MAP_WIDTH + ".");
                return;
            }
            if (!(height >= MIN_MAP_HEIGHT && height <= MAX_MAP_HEIGHT))
            {
                ErrorMessage("Height has to be between " + MIN_MAP_HEIGHT + " and " + MAX_MAP_HEIGHT + ".");
                return;
            }

            string mapName = newNameTB.Text;
            if (ValidName(mapName))
            {
                if (MapAlreadyExists(mapName))
                {
                    ErrorMessage("The name \"" + mapName + "\" already exits.");
                    return;
                }
                MapDescr = new MapDescription(width, height, newNameTB.Text);
                mapPB.Invalidate();
                mapPB.Refresh();
                EnableEditing();
                mapNameL.Text = MapDescr.Name;
                Message("The map \"" + MapDescr.Name + "\" was created.");
            }
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
                int left = (mapPB.Width - MapScale * MapDescr.Width) / 2;
                int bottom = (mapPB.Height - MapScale * MapDescr.Height) / 2;
                Bitmap total = MapDescr.TotalMap();
                g.DrawImage(total, left + MapScale / 2, bottom - MapScale / 2 + MapScale * MapDescr.Height, MapScale * total.Width, -MapScale * total.Height);
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
                            MapDescr.DrawTerrain(MapDescription.DeepWaterColor, x, y, size, size);
                            break;
                        case DrawOption.DRAW_SHALLOW_WATER:
                            MapDescr.DrawTerrain(MapDescription.ShallowWaterColor, x, y, size, size);
                            break;
                        case DrawOption.DRAW_LAND:
                            MapDescr.DrawTerrain(MapDescription.LandColor, x, y, size, size);
                            break;
                        case DrawOption.DRAW_NUTRIENTS:
                            MapDescr.AddNutrients(x, y, size, size, (int)nutrientsRateNUD.Value);
                            break;
                        case DrawOption.ADD_BUILDING:
                            string error = MapDescr.AddBuilding(BuildingToPlace, mapPoint.X, mapPoint.Y);
                            if (error != null)
                                ErrorMessage(error);
                            break;
                        case DrawOption.REMOVE_BUILDING:
                            MapDescr.RemoveBuilding(mapPoint.X, mapPoint.Y);
                            break;
                    }
                }
                MapDescr.RepairMap();

                mapPB.Refresh();
            }
        }

        private void SaveB_Click(object sender, EventArgs e)
        {
            if (MapDescr != null)
            {
                MapDescr.RepairMap();
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
            if (!ValidName(mapDirName))
                return;

            try
            {
                MapDescr = new MapDescription(mapDirName);
                DisableEditing();
                mapNameL.Text = MapDescr.Name;
                Message("The map \"" + mapDirName + "\" was loaded.");
            }
            catch (Exception)
            {
                ErrorMessage("Map \"" + mapDirName + "\" was not loaded, because it doesn't exist or has incorrect format.");
            }
            mapPB.Refresh();
        }

        private void DeleteB_Click(object sender, EventArgs e)
        {
            string deletedName = mapNamesCB.Text;
            string dirName = MapDescription.DIRECTORY + deletedName;
            if (deletedName == "")
            { 
                ErrorMessage("Select a map to delete.");
                return;
            }

            try
            {
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
            catch (IOException)
            {
                ErrorMessage("The map can't be deleted.");
            }
        }

        private void PlayB_Click(object sender, EventArgs e)
        {
            if (MapDescr != null)
            {
                if (MapDescr.MainBuildingsPresent())
                {
                    MapDescr.RepairMap();
                    DisableEditing();

                    var gameWindow = new MainWinformWindow(MapDescr, PlayersBiome, Icons, testAnimalsCB.Checked);
                    gameWindow.ShowDialog();
                    gameWindow.Dispose();
                }
                else
                    ErrorMessage("The map doesn't contain the main buildings of both players.");
            }
            else
                ErrorMessage("Load or create a map before starting the game.");
        }

        private void EditB_Click(object sender, EventArgs e)
        {
            if (MapDescr != null)
                EnableEditing();
            else
                ErrorMessage("No map is loaded.");
        }

        #endregion Event handlers

        /// <summary>
        /// Used for freezing classes to make them immutable.
        /// </summary>
        private interface IFreezable
        {
            void Freeze();
            void Unfreeze();
        }

        /// <summary>
        /// Description of the map. Can be loaded from and saved to file. Can only be
        /// edited inside of the class MainMenuWindow.
        /// </summary>
        public class MapDescription:IFreezable
        {
            /// <summary>
            /// Name of directory where maps are saved.
            /// </summary>
            public const string DIRECTORY = "Maps/";
            public static Color LandColor { get; }
            public static Color DeepWaterColor { get; }
            public static Color ShallowWaterColor { get; }
            public static Color RockColor { get; }
            public static Color BigRockColor { get; }
            public static Color Player0MainColor { get; }//is yellow not to coincide with water
            public static Color Player1MainColor { get; }
            /// <summary>
            /// Color of something that isn't important.
            /// </summary>
            public static Color NothingColor { get; }

            static MapDescription()
            {
                LandColor = Color.FromArgb(168, 142, 78);
                DeepWaterColor = Color.FromArgb(0, 0, 200);
                ShallowWaterColor = Color.FromArgb(100, 100, 200);
                RockColor = Color.FromArgb(100, 100, 100);
                BigRockColor = Color.FromArgb(200, 200, 200);
                Player0MainColor = Color.FromArgb(255, 255, 0);
                Player1MainColor = Color.FromArgb(255, 0, 0);
                NothingColor = Color.FromArgb(255, 255, 255);
            }

            /// <summary>
            /// True iff the map can't be edited.
            /// </summary>
            private bool Frozen { get; set; }
            private Bitmap TerrainMap { get; }
            public Color GetTerrain(int i, int j) => TerrainMap.GetPixel(i, j);
            private Bitmap NutrientsMap { get; }
            public Color GetNutrients(int i, int j) => NutrientsMap.GetPixel(i, j);
            /// <summary>
            /// Bitmap that contains black pixels on squares taken by buildings.
            /// </summary>
            private Bitmap BuildingsLocations { get; }
            private List<BuildingDescriptor> Buildings { get; }
            /// <summary>
            /// Returns copy of Buildings.
            /// </summary>
            public List<BuildingDescriptor> GetBuildings => Buildings.ToList();
            public string Name { get; }
            public int Width => TerrainMap.Width;
            public int Height => TerrainMap.Height;

            /// <summary>
            /// Creates a new empty map with the given extents and name.
            /// </summary>
            public MapDescription(int width, int height, string name)
            {
                Name = name;
                Frozen = false;

                //init terrain map
                TerrainMap = new Bitmap(width, height);
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        TerrainMap.SetPixel(i, j, LandColor);
                    }

                //init nutrients map
                NutrientsMap = new Bitmap(width, height);
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        NutrientsMap.SetPixel(i, j, NothingColor);
                    }

                //init buildings map
                BuildingsLocations = new Bitmap(width, height);
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        BuildingsLocations.SetPixel(i, j, NothingColor);
                    }

                //init buildings
                Buildings = new List<BuildingDescriptor>();
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
                            BuildingsLocations.SetPixel(i, j, NothingColor);
                        }

                    Buildings = new List<BuildingDescriptor>();
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] param = line.Split(' ');
                        int x = int.Parse(param[0]);
                        int y = int.Parse(param[1]);
                        string type = param[2];
                        AddBuilding((BuildingType)Enum.Parse(typeof(BuildingType), type), x, y);
                    }
                }
                Name = mapDirectoryName;
                Frozen = true;
            }

            /// <summary>
            /// Visualization of the map.
            /// </summary>
            public Bitmap TotalMap()
            {
                // draw terrain and nutrients
                Bitmap total = new Bitmap(Width, Height);
                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++)
                    {
                        Color terC = TerrainMap.GetPixel(i, j);
                        Color nutC = NutrientsMap.GetPixel(i, j);
                        // brightness has nonzero default value
                        // so that terrain and buildings can always be seen through it
                        float brightness = 0.5f + nutC.R / 512f;
                        Color newColor = Color.FromArgb((int)(terC.R * brightness),
                            (int)(terC.G * brightness),
                            (int)(terC.B * brightness));
                        total.SetPixel(i, j, newColor);
                    }
                // draw buildings
                foreach (var bd in Buildings)
                {
                    if(!TryGetBuildingExtents(bd.Type, out int width, out int height))
                        continue;

                    switch (bd.Type)
                    {
                        case BuildingType.PLAYER_0_MAIN:
                            FillRectWithColor(total, Player0MainColor, bd.X, bd.Y, width, height);
                            break;
                        case BuildingType.PLAYER_1_MAIN:
                            FillRectWithColor(total, Player1MainColor, bd.X, bd.Y, width, height);
                            break;
                        case BuildingType.ROCK:
                            FillRectWithColor(total, RockColor, bd.X, bd.Y, width, height);
                            break;
                        case BuildingType.BIG_ROCK:
                            FillRectWithColor(total, BigRockColor, bd.X, bd.Y, width, height);
                            break;
                    }
                }
                return total;
            }

            /// <summary>
            /// Returns the extents of a building with the given type.
            /// </summary>
            private bool TryGetBuildingExtents(BuildingType buildingType, out int width, out int height)
            {
                switch (buildingType)
                {
                    case BuildingType.PLAYER_0_MAIN:
                        width = height = 2;
                        return true;
                    case BuildingType.PLAYER_1_MAIN:
                        width = height = 2;
                        return true;
                    case BuildingType.ROCK:
                        width = height = 1;
                        return true;
                    case BuildingType.BIG_ROCK:
                        width = height = 2;
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
                return Buildings.Where(bd => bd.Type == BuildingType.PLAYER_0_MAIN).Any() &&
                    Buildings.Where(bd => bd.Type == BuildingType.PLAYER_1_MAIN).Any();
            }

            /// <summary>
            /// Returns true iff the coordinate on the map is valid.
            /// </summary>
            public bool ValidCoordinates(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

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
                            if (Color.Black.SameRGB(BuildingsLocations.GetPixel(i, j)) ||
                                !LandColor.SameRGB(TerrainMap.GetPixel(i, j)))//buildings can be put only on land
                                return false;
                    return true;
                }
                return false;
            }

            #region Mutating methods

            #region Public api mutating methods
            
            /// <summary>
            /// Draws the color to the terrain map. Does nothing if this object is frozen.
            /// </summary>
            public void DrawTerrain(Color color, int x, int y, int width, int height)
            {
                if (Frozen)
                    return;

                FillTerrainIgnoreBuildings(color, x, y, width, height);
            }

            /// <summary>
            /// Draws to the nutrients map. Does nothing if this object is frozen.
            /// </summary>
            public void AddNutrients(int x, int y, int width, int height, int intensity)
            {
                if (Frozen)
                    return;

                AddRectIntensity(NutrientsMap, -intensity, x, y, width, height);
            }

            /// <summary>
            /// Adds a new building of given type to the map. Returns error message.
            /// Does nothing if this object is frozen.
            /// </summary>
            public string AddBuilding(BuildingType type, int x, int y)
            {
                if (Frozen)
                    return "Can't modify frozen map.";

                if (TryGetBuildingExtents(type, out int width, out int height))
                {
                    if (type == BuildingType.PLAYER_0_MAIN &&
                        Buildings.Where(bd => bd.Type == type).Any())
                        return "Main building for player 0 was already placed.";

                    if (type == BuildingType.PLAYER_1_MAIN &&
                        Buildings.Where(bd => bd.Type == type).Any())
                        return "Main building for player 1 was already placed.";

                    if (CanBePlacedBuilding(x, y, width, height))
                    {
                        Buildings.Add(new BuildingDescriptor(type, x, y));
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
            /// Does nothing if this object is frozen.
            /// </summary>
            public void RemoveBuilding(int x, int y)
            {
                if (Frozen)
                    return;

                if (!ValidCoordinates(x, y))
                    return;

                //the square isn't occupied by any building
                if (NothingColor.SameRGB(BuildingsLocations.GetPixel(x, y)))
                    return;

                BuildingDescriptor? toRemove = null;//BuildingDescriptor is struct but we need null value
                foreach (var bd in Buildings)
                {
                    TryGetBuildingExtents(bd.Type, out int width, out int height);
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
            #endregion Public api mutating methods

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
                            if (NothingColor.SameRGB(BuildingsLocations.GetPixel(xx, yy)))
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
            /// Removes forbiden patterns from TerrainMap the following way:
            /// 
            /// +_  =>  +_
            /// _+      ++
            /// 
            /// _+  =>  _+
            /// +_      ++
            /// 
            /// iterates until all of the forbiden patterns are gone. The operation
            /// is applied with priorities from highest to lowest: DeepWater, ShallowWater, Land.
            /// </summary>
            public void RepairMap()
            {
                if (Frozen)
                    return;

                bool changed = true;
                while (changed)
                {
                    changed = false;
                    for (int i = 0; i < Width - 1; i++)
                        for (int j = 0; j < Height - 1; j++)
                        {
                            Color a = TerrainMap.GetPixel(i, j); Color b = TerrainMap.GetPixel(i + 1, j);
                            Color c = TerrainMap.GetPixel(i, j + 1); Color d = TerrainMap.GetPixel(i + 1, j + 1);

                            if (a.SameRGB(d) && ! a.SameRGB(c) && ! a.SameRGB(b))
                            // +_
                            // _+
                            {
                                if (c.SameRGB(b))
                                // +-
                                // -+
                                {
                                    if (TerrainHigherPriority(a, b))
                                    {
                                        TerrainMap.SetPixel(i, j + 1, a);
                                    }
                                    else
                                    {
                                        TerrainMap.SetPixel(i + 1, j + 1, b);
                                    }
                                }
                                else
                                // +.
                                // -+
                                    TerrainMap.SetPixel(i, j + 1, a);

                                    changed = true;
                            }
                            if (c.SameRGB(b) && !c.SameRGB(a) && !c.SameRGB(d))
                            // _+
                            // +_
                            {
                                if (a.SameRGB(d))
                                // -+
                                // +-
                                {
                                    if (TerrainHigherPriority(a, b))
                                    {
                                        TerrainMap.SetPixel(i, j + 1, a);
                                    }
                                    else
                                    {
                                        TerrainMap.SetPixel(i + 1, j + 1, b);
                                    }
                                }
                                else
                                // .+
                                // +-
                                    TerrainMap.SetPixel(i, j + 1, a);

                                changed = true;
                            }
                        }
                }
            }

            /// <summary>
            /// Returns true if a and b represent terrain and a has greater prioirity when repairing
            /// map than b.
            /// </summary>
            private bool TerrainHigherPriority(Color a, Color b)
            {
                if (a.SameRGB(DeepWaterColor))
                    return true;
                else if (a.SameRGB(LandColor))
                    return false;
                else
                {
                    if (b.SameRGB(DeepWaterColor))
                        return false;
                    else
                        return true;
                }
            }
            
            #endregion Mutating methods

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

            void IFreezable.Freeze()
            {
                Frozen = true;
            }

            void IFreezable.Unfreeze()
            {
                Frozen = false;
            }
        }

        /// <summary>
        /// Location and type of building.
        /// </summary>
        public struct BuildingDescriptor
        {
            public int X { get; }
            public int Y { get; }
            public BuildingType Type { get; }

            public BuildingDescriptor(BuildingType type, int x, int y)
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

        public enum BuildingType
        {
            PLAYER_0_MAIN,
            PLAYER_1_MAIN,
            ROCK,
            BIG_ROCK
        }
    }
}
