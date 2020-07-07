using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.AI;
using SanguineGenesis.GameLogic.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
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
        private const int MAX_MAP_WIDTH = 80;
        /// <summary>
        /// Minimum height of the map.
        /// </summary>
        private const int MIN_MAP_HEIGHT = 30;
        /// <summary>
        /// Maximum height of the map.
        /// </summary>
        private const int MAX_MAP_HEIGHT = 80;
        /// <summary>
        /// How many times bigger is the drawing of the map than the map itself.
        /// </summary>
        private int MapScale { get; }

        public MainMenuWindow()
        {
            InitializeComponent();
            heightNUD.Minimum = MIN_MAP_HEIGHT;
            widthNUD.Minimum = MIN_MAP_WIDTH;
            heightNUD.Maximum = MAX_MAP_HEIGHT;
            widthNUD.Maximum = MAX_MAP_WIDTH;

            MapScale = mapPB.Width / MAX_MAP_WIDTH;
            DrawOpt = DrawOption.NO_ACTION;
            CanCreateGame = true;
            LoadNamesOfCreatedMaps();
            LoadAIFactoryNames();
            //load icon
            try { this.Icon = new Icon("Images/Icons/giraffe.ico"); } catch (IOException e)
            {
                ErrorMessage($"Icon can't be loaded: {e}");
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
            ERASE_NUTRIENTS,
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
                drawOpt = value;
                //enable and disable parts of gui
                switch (value)
                {
                    case DrawOption.DRAW_DEEP_WATER:
                    case DrawOption.DRAW_SHALLOW_WATER:
                    case DrawOption.DRAW_LAND:
                        buildingSelectionGB.Enabled = false;
                        brushSizeNUD.Enabled = true;
                        nutrientsRateNUD.Enabled = false;
                        break;
                    case DrawOption.DRAW_NUTRIENTS:
                    case DrawOption.ERASE_NUTRIENTS:
                        buildingSelectionGB.Enabled = false;
                        brushSizeNUD.Enabled = true;
                        nutrientsRateNUD.Enabled = true;
                        break;
                    case DrawOption.ADD_BUILDING:
                        buildingSelectionGB.Enabled = true;
                        brushSizeNUD.Enabled = false;
                        nutrientsRateNUD.Enabled = false;
                        break;
                    case DrawOption.REMOVE_BUILDING:
                        buildingSelectionGB.Enabled = false;
                        brushSizeNUD.Enabled = false;
                        nutrientsRateNUD.Enabled = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Building that will be placed to the map on click while being in ADD_BUILDING mode.
        /// </summary>
        private BuildingType BuildingToPlace { get; set; }

        /// <summary>
        /// Returns true if name can be used as a name of the map. Prints error message.
        /// </summary>
        private bool IsValidName(string name)
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
        /// Returns true if the name of the map already exists.
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
                    if (IsValidName(name))
                        mapNamesCB.Items.Add(name);
                }
            }
            catch(Exception e) when (e is IOException || e is UnauthorizedAccessException ||
                                     e is PathTooLongException || e is DirectoryNotFoundException)
            {
                ErrorMessage($"List of maps can't be loaded: {e}");
            }
        }

        /// <summary>
        /// Loads names of AIFactories.
        /// </summary>
        public void LoadAIFactoryNames()
        {
            foreach(var name in AIs.GetAINames())
            {
                aiCB.Items.Add(name);
            }
            aiCB.Text = "Default AI";
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
            else if (eraseNutrientsRB.Checked)
                DrawOpt = DrawOption.ERASE_NUTRIENTS;
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

        /// <summary>
        /// Creates a new map.
        /// </summary>
        private void NewMapB_Click(object sender, EventArgs e)
        {
            int width = (int)widthNUD.Value;
            int height = (int)heightNUD.Value;
            
            //check map extents
            if (!(width >= MIN_MAP_WIDTH && width <= MAX_MAP_WIDTH))
            {
                ErrorMessage($"Width has to be between {MIN_MAP_WIDTH} and {MAX_MAP_WIDTH}.");
                return;
            }
            if (!(height >= MIN_MAP_HEIGHT && height <= MAX_MAP_HEIGHT))
            {
                ErrorMessage($"Height has to be between {MIN_MAP_HEIGHT} and {MAX_MAP_HEIGHT}.");
                return;
            }

            //check if the name is valid
            string mapName = newNameTB.Text;
            if (IsValidName(mapName))
            {
                //check if the map already exists
                if (MapAlreadyExists(mapName))
                {
                    ErrorMessage($"The name \"{mapName}\" already exits.");
                    return;
                }
                MapDescr = new MapDescription(width, height, newNameTB.Text);
                mapPB.Invalidate();
                mapPB.Refresh();
                EnableEditing();
                mapNameL.Text = MapDescr.Name;
                Message($"The map \"{MapDescr.Name}\" was created.");
            }
        }

        /// <summary>
        /// Selects drawing tool.
        /// </summary>
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
                case "eraseNutrientsRB":
                    DrawOpt = DrawOption.ERASE_NUTRIENTS;
                    break;
                case "addBuildingRB":
                    DrawOpt = DrawOption.ADD_BUILDING;
                    break;
                case "removeBuildingRB":
                    DrawOpt = DrawOption.REMOVE_BUILDING;
                    break;
            }
        }

        /// <summary>
        /// Draws the map.
        /// </summary>
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

        /// <summary>
        /// Selects building to place to the map.
        /// </summary>
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

        /// <summary>
        /// Draws into the map.
        /// </summary>
        private void MapPB_MouseDownAction(object sender, MouseEventArgs e)
        {
            if (MapDescr == null)
                return;

            Point mapPoint = MapCoordinates(e.X, e.Y);
            coordinatesL.Text = $"X = {mapPoint.X} ; Y = {mapPoint.Y}";

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
                        case DrawOption.ERASE_NUTRIENTS:
                            MapDescr.AddNutrients(x, y, size, size, -(int)nutrientsRateNUD.Value);
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

        /// <summary>
        /// Saves MapDescr.
        /// </summary>
        private void SaveB_Click(object sender, EventArgs e)
        {
            if (MapDescr != null)
            {
                MapDescr.RepairMap();
                try
                {
                    MapDescr.Save();
                }
                catch(InvalidOperationException ex)
                {
                    ErrorMessage(ex.Message);
                    return;
                }
                DisableEditing();
                LoadNamesOfCreatedMaps();
                Message($"The map \"{MapDescr.Name}\" was saved.");
            }
            else
                ErrorMessage("No map is loaded.");
        }

        /// <summary>
        /// Loads a map with name given by the mapNamesCB.Text.
        /// </summary>
        private void LoadB_Click(object sender, EventArgs e)
        {
            string mapDirName = mapNamesCB.Text;
            if (!IsValidName(mapDirName))
                return;

            try
            {
                var mapDescr = new MapDescription(mapDirName);
                MapDescr = mapDescr;
                DisableEditing();
                mapNameL.Text = MapDescr.Name;
                Message($"The map \"{mapDirName}\" was loaded.");
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage($"Map \"{mapDirName}\" was not loaded, because it doesn't exist or has incorrect format: {ex.Message}");
            }
            mapPB.Refresh();
        }

        /// <summary>
        /// Deletes a map with name given by the mapNamesCB.Text.
        /// </summary>
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
                    Message("The map \"{mapNamesCB.Text}\" was deleted.");
                }
                else
                {
                    ErrorMessage("No map is loaded.");
                }
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException || ex is UnauthorizedAccessException ||
                                     ex is PathTooLongException || ex is DirectoryNotFoundException)
            {
                ErrorMessage($"The map can't be deleted: {ex}");
            }
        }

        /// <summary>
        /// Makes map editable.
        /// </summary>
        private void EditB_Click(object sender, EventArgs e)
        {
            if (MapDescr != null)
                EnableEditing();
            else
                ErrorMessage("No map is loaded.");
        }

        /// <summary>
        /// Window where the game is played.
        /// </summary>
        private GameWindow gameWindow;
        /// <summary>
        /// If set to false, new game can't be created. 
        /// </summary>
        public bool CanCreateGame { get; set; }

        /// <summary>
        /// Opens a game window.
        /// </summary>
        private void PlayB_Click(object sender, EventArgs e)
        {
            if (CanCreateGame)
            {
                if (MapDescr != null)
                {
                    if (MapDescr.MainBuildingsPresent())
                    {
                        MapDescr.RepairMap();
                        DisableEditing();

                        //create new window only if it wasn't created already
                        if (gameWindow == null)
                            gameWindow = new GameWindow(this);
                        if (CanCreateGame)
                        {
                            gameWindow.Enabled = true;
                            gameWindow.StartNewGame(MapDescr, PlayersBiome, testAnimalsCB.Checked, AIs.GetAIFactory(aiCB.Text));
                            gameWindow.BringToFront();
                            //hide this window
                            Enabled = false;
                            Visible = false;
                            //show game window
                            gameWindow.Show();
                        }
                    }
                    else
                        ErrorMessage("The map doesn't contain the main buildings of both players.");
                }
                else
                    ErrorMessage("Load or create a map before starting the game.");
            }
            else
                ErrorMessage("There was a problem with game window initialization.");
        }

        /// <summary>
        /// Closes the game window.
        /// </summary>
        private void MainMenuWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            //close the game window if it exists
            if (gameWindow != null)
            {
                gameWindow.CloseWindow = true;
                gameWindow.Close();
            }
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
            public static Color Player0MainColor { get; }//is yellow to not coincide with water
            public static Color Player1MainColor { get; }
            /// <summary>
            /// Default value of BuildingsLocations.
            /// </summary>
            public static Color NoBuildingColor { get; }
            /// <summary>
            /// Default value of NutrientsMap.
            /// </summary>
            public static Color NoNutrientsColor { get; }

            static MapDescription()
            {
                LandColor = Color.FromArgb(168, 142, 78);//brown
                DeepWaterColor = Color.FromArgb(0, 0, 200);//dark blue
                ShallowWaterColor = Color.FromArgb(100, 100, 200);//light blue
                RockColor = Color.FromArgb(100, 100, 100);//dark gray
                BigRockColor = Color.FromArgb(200, 200, 200);//light gray
                Player0MainColor = Color.FromArgb(255, 255, 0);//yellow
                Player1MainColor = Color.FromArgb(255, 0, 0);//red
                NoBuildingColor = Color.FromArgb(255, 255, 255);//white
                NoNutrientsColor = Color.FromArgb(255, 255, 255);//white
            }

            /// <summary>
            /// True if the map can't be edited.
            /// </summary>
            private bool Frozen { get; set; }
            /// <summary>
            /// Color of pixel (i,j) represents terrain of Node (i,j). Mapping terrain to colors is in properties LandColor,
            /// DeepWaterColor and ShallowWaterColor.
            /// </summary>
            private Bitmap TerrainMap { get; }
            public Color GetTerrain(int i, int j) { if (ValidCoordinates(i, j)) return TerrainMap.GetPixel(i, j); else return NoBuildingColor; }
            /// <summary>
            /// Color of pixel (i,j) represents amount of passive nutrients of Node (i,j). The color is always gray, brightness 255 corresponds
            /// to the minal number of nutrients, 0 corresponds to the maximal nubmer of nutrients.
            /// </summary>
            private Bitmap NutrientsMap { get; }
            public Color GetNutrients(int i, int j) { if (ValidCoordinates(i, j)) return NutrientsMap.GetPixel(i, j); else return NoNutrientsColor; }
            /// <summary>
            /// Bitmap that highlights pixels taken by buildings.
            /// </summary>
            private Bitmap BuildingsLocations { get; }
            /// <summary>
            /// List of all buildings on this map.
            /// </summary>
            private List<BuildingDescriptor> Buildings { get; }
            /// <summary>
            /// Returns copy of Buildings.
            /// </summary>
            public IEnumerable<BuildingDescriptor> GetBuildings() => Buildings;
            /// <summary>
            /// Name of this map.
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// Width of this map.
            /// </summary>
            public int Width => TerrainMap.Width;
            /// <summary>
            /// Height of this map.
            /// </summary>
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
                        NutrientsMap.SetPixel(i, j, NoNutrientsColor);
                    }

                //init buildings map
                BuildingsLocations = new Bitmap(width, height);
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        BuildingsLocations.SetPixel(i, j, NoBuildingColor);
                    }

                //init buildings
                Buildings = new List<BuildingDescriptor>();
            }

            /// <summary>
            /// Loads map from the directory.
            /// </summary>
            /// <param name="mapDirectoryName">Name of the directory.</param>
            /// <exception cref="InvalidOperationException">Thrown if the directory or some of the required files don't exist.</exception>
            public MapDescription(string mapDirectoryName)
            {
                string dirName = DIRECTORY + mapDirectoryName;

                if (!Directory.Exists(dirName))
                    throw new InvalidOperationException($"Directory {dirName} doesn't exist.");

                try
                {
                    using (var terrainReader = new FileStream(dirName + "/terrain.bmp", FileMode.Open))
                    using (var nutrientsReader = new FileStream(dirName + "/nutrients.bmp", FileMode.Open))
                    using (var sr = new StreamReader(dirName + "/buildings.txt"))
                    {
                        //load terrain and nutrients
                        TerrainMap = new Bitmap(terrainReader);
                        int tWidth = TerrainMap.Width;
                        int tHeight = TerrainMap.Height;
                        NutrientsMap = new Bitmap(nutrientsReader);
                        int nWidth = NutrientsMap.Width;
                        int nHeight = NutrientsMap.Height;

                        //check extents of loaded maps
                        int width = tWidth;
                        int height = tHeight;
                        if (tWidth > MAX_MAP_WIDTH || tWidth < MIN_MAP_HEIGHT || tHeight > MAX_MAP_HEIGHT || tHeight < MIN_MAP_HEIGHT ||
                            nWidth > MAX_MAP_WIDTH || nWidth < MIN_MAP_HEIGHT || nHeight > MAX_MAP_HEIGHT || nHeight < MIN_MAP_HEIGHT ||
                            tWidth != nWidth || tHeight != nHeight)
                        {
                            //synchronize the extents of the maps and put them to the correct range
                            width = Math.Min(MAX_MAP_WIDTH, Math.Max(MIN_MAP_WIDTH, tWidth));
                            height = Math.Min(MAX_MAP_HEIGHT, Math.Max(MIN_MAP_HEIGHT, tWidth));
                            var terrainMap = new Bitmap(width, height);
                            var nutrientsMap = new Bitmap(width, height);

                            //initialize the maps
                            for (int i = 0; i < width; i++)
                                for (int j = 0; j < height; j++)
                                {
                                    //terrain
                                    if (i >= 0 && i < TerrainMap.Width && j >= 0 && j < TerrainMap.Height)
                                        terrainMap.SetPixel(i, j, TerrainMap.GetPixel(i, j));
                                    else
                                        terrainMap.SetPixel(i, j, LandColor);
                                    //nutrients
                                    if (i >= 0 && i < NutrientsMap.Width && j >= 0 && j < NutrientsMap.Height)
                                        nutrientsMap.SetPixel(i, j, NutrientsMap.GetPixel(i, j));
                                    else
                                        nutrientsMap.SetPixel(i, j, NoNutrientsColor);
                                }

                            //set the maps to properties
                            TerrainMap = terrainMap;
                            NutrientsMap = nutrientsMap;
                        }
                        for (int i = 0; i < width; i++)
                            for (int j = 0; j < height; j++)
                            {
                                //set correct to terrain map if the original is incorrect
                                if (!IsTerrainColor(TerrainMap.GetPixel(i, j)))
                                    TerrainMap.SetPixel(i, j, LandColor);
                            }

                        //load buildings
                        BuildingsLocations = new Bitmap(width, height);
                        for (int i = 0; i < width; i++)
                            for (int j = 0; j < height; j++)
                            {
                                BuildingsLocations.SetPixel(i, j, NoBuildingColor);
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
                }
                catch (Exception ex) when (ex is IOException || ex is SecurityException || ex is ArgumentException || ex is FileNotFoundException
                                       || ex is UnauthorizedAccessException || ex is PathTooLongException || ex is DirectoryNotFoundException)
                {
                    throw new InvalidOperationException($"The map can't be loaded: {ex.Message}", ex);
                }

                Name = mapDirectoryName;
                Frozen = true;
            }

            /// <summary>
            /// Returns true if the color is valid color for terrain map.
            /// </summary>
            private bool IsTerrainColor(Color c)
            {
                return LandColor.SameRGB(c) ||
                        ShallowWaterColor.SameRGB(c) ||
                        DeepWaterColor.SameRGB(c);
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
                        // draw buildings
                        if (!NoBuildingColor.SameRGB(BuildingsLocations.GetPixel(i, j))) 
                        { 
                            total.SetPixel(i, j, BuildingsLocations.GetPixel(i, j));
                            continue;
                        }

                        //blend nutrients to terrain
                        Color terC = TerrainMap.GetPixel(i, j);
                        Color nutC = NutrientsMap.GetPixel(i, j);
                        // nutrients are shown as brightness of the map -- less bright => more nutrients
                        // brightness has nonzero default value
                        // so that terrain and buildings can always be seen through it
                        float brightness = 0.5f + nutC.R / 512f;
                        Color newColor = Color.FromArgb((int)(terC.R * brightness),
                            (int)(terC.G * brightness),
                            (int)(terC.B * brightness));
                        total.SetPixel(i, j, newColor);
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
            /// Returns true if the map contains main buildings of both players.
            /// </summary>
            public bool MainBuildingsPresent()
            {
                return Buildings.Where(bd => bd.Type == BuildingType.PLAYER_0_MAIN).Any() &&
                    Buildings.Where(bd => bd.Type == BuildingType.PLAYER_1_MAIN).Any();
            }

            /// <summary>
            /// Returns true if the coordinate on the map is valid.
            /// </summary>
            public bool ValidCoordinates(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

            /// <summary>
            /// Returns true if building of width and height can be placed to coordinates x,y. Returns correct value only
            /// if witdth and height are positive.
            /// </summary>
            private bool CanBePlacedBuilding(int x, int y, int width, int height)
            {
                //check if coordinates of the building are valid
                if (ValidCoordinates(x, y) &&
                    ValidCoordinates(x + width - 1, y + height - 1))
                {
                    for (int i = x; i < x + width; i++)
                        for (int j = y; j < y + height; j++)
                            if (!NoBuildingColor.SameRGB(BuildingsLocations.GetPixel(i, j)) ||
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

                FillRectWithColor(TerrainMap, color, x, y, width, height, 
                    (xx, yy) => NoBuildingColor.SameRGB(BuildingsLocations.GetPixel(xx, yy)));
            }

            /// <summary>
            /// Draws to the nutrients map. Does nothing if this object is frozen.
            /// </summary>
            public void AddNutrients(int x, int y, int width, int height, int value)
            {
                if (Frozen)
                    return;

                AddRectBrightness(NutrientsMap, -value, x, y, width, height);
            }

            /// <summary>
            /// Adds a new building of the given type to the map. Returns error message.
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
                        //choose color of the building
                        Color buildingColor;
                        switch (type)
                        {
                            case BuildingType.PLAYER_0_MAIN:
                                buildingColor = Player0MainColor;
                                break;
                            case BuildingType.PLAYER_1_MAIN:
                                buildingColor = Player1MainColor;
                                break;
                            case BuildingType.ROCK:
                                buildingColor = RockColor;
                                break;
                            default: // BuildingType.BIG_ROCK:
                                buildingColor = BigRockColor;
                                break;
                        }
                        FillRectWithColor(BuildingsLocations, buildingColor, x, y, width, height, (_x, _y) => true);
                        return null;
                    }
                    else
                        return $"The building cannot be placed at the coordinates ({x}, {y}).";
                }
                else
                    return $"The building with a type \"{type}\" doesn't exist.";
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
                if (NoBuildingColor.SameRGB(BuildingsLocations.GetPixel(x, y)))
                    return;

                BuildingDescriptor? toRemove = null;//BuildingDescriptor is struct but we need null value to check if the building exists
                foreach (var bd in Buildings)
                {
                    TryGetBuildingExtents(bd.Type, out int width, out int height);
                    if (bd.X <= x && x < bd.X + width && bd.Y <= y && y < bd.Y + height)
                    {
                        toRemove = bd;
                        FillRectWithColor(BuildingsLocations, NoBuildingColor, bd.X, bd.Y, width, height, (_x, _y) => true);
                        break;
                    }
                }
                if (toRemove.HasValue)
                    Buildings.Remove(toRemove.Value);
            }
            #endregion Public api mutating methods

            /// <summary>
            /// Fills the rectangle in bitmap with color c. Coordiantes can be out of range. The color of the square is changed
            /// only if squareCondition returns true for the square.
            /// </summary>
            private void FillRectWithColor(Bitmap bitmap, Color c, int x, int y, int width, int height, Func<int, int, bool> squareCondition)
            {
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        int xx = x + i;
                        int yy = y + j;
                        if (ValidCoordinates(xx, yy))
                        {
                            if(squareCondition(xx,yy))
                                bitmap.SetPixel(xx, yy, c);
                        }
                    }
            }

            /// <summary>
            /// Adds the rectangle in bitmap with color c. Coordiantes can be out of range. Brightness
            /// is between 0 and 255.
            /// </summary>
            private void AddRectBrightness(Bitmap bitmap, int value, int x, int y, int width, int height)
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
                                    Math.Min(255, Math.Max(color.R + value, 0)),
                                    Math.Min(255, Math.Max(color.G + value, 0)),
                                    Math.Min(255, Math.Max(color.B + value, 0))));
                        }
                    }
            }

            /// <summary>
            /// Removes forbiden patterns from TerrainMap the following way:
            /// 
            /// 1)
            /// x_  =>  x_
            /// _x      xx
            /// 
            /// 2)
            /// _x  =>  _x
            /// x_      xx
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
                            // a_
                            // _a
                            {
                                if (c.SameRGB(b))
                                // ab
                                // ba
                                {
                                    if (TerrainHigherPriority(a, b))
                                    {
                                        //use rule 1) for x=a
                                        TerrainMap.SetPixel(i, j + 1, a);
                                    }
                                    else
                                    {
                                        //use rule 2) for x=b
                                        TerrainMap.SetPixel(i + 1, j + 1, b);
                                    }
                                }
                                else
                                // ab
                                // ca
                                    //use rule 1) for x=a
                                    TerrainMap.SetPixel(i, j + 1, a);

                                changed = true;
                            }
                            else if (c.SameRGB(b) && !c.SameRGB(a) && !c.SameRGB(d))
                            // _b
                            // b_
                            {
                                //the case a=c was already covered in the first branch
                                // ab
                                // bc
                                //use rule 2) for x=b
                                TerrainMap.SetPixel(i, j + 1, a);

                                changed = true;
                            }
                        }
                }

                //the algorithm might put water under buildings
                //put land under buildings
                for (int i = 0; i < Width - 1; i++)
                    for (int j = 0; j < Height - 1; j++)
                    {
                        if (!NoBuildingColor.SameRGB(BuildingsLocations.GetPixel(i, j)) &&
                            !LandColor.SameRGB(TerrainMap.GetPixel(i,j)))
                        {
                            TerrainMap.SetPixel(i, j, LandColor);
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
            /// <exception cref="InvalidOperationException">If data can't be written to the files.</exception>
            public void Save()
            {
                string dirName = DIRECTORY + Name;
                try
                {
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
                catch (Exception ex) when (ex is IOException || ex is SecurityException || ex is ArgumentException || ex is FileNotFoundException
                                        || ex is UnauthorizedAccessException || ex is PathTooLongException || ex is DirectoryNotFoundException)
                {
                    throw new InvalidOperationException($"The map can't be saved: {ex.Message}", ex);
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
                return $"{X} {Y} {Type}";
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
