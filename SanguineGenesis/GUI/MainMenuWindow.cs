using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private const int MAX_MAP_WIDTH = 150;
        /// <summary>
        /// Maximum height of the map.
        /// </summary>
        private const int MAX_MAP_HEIGHT = 150;

        public MainMenuWindow()
        {
            InitializeComponent();

            DrawOpt = DrawOption.NO_ACTION;
        }

        private struct BuildingDescriptor
        {
            public int X { get; }
            public int Y { get; }
            public string Type { get; }
        }

        private class MapDescription
        {
            private Bitmap TerrainMap { get; }
            private Bitmap NutrientsMap { get;  }
            private List<BuildingDescriptor> Buildings { get; }
            public int Width => TerrainMap.Width;
            public int Height => TerrainMap.Height;

            public MapDescription(int width, int height)
            {
                TerrainMap = new Bitmap(width, height);
                for(int i=0;i<width; i++)
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
                foreach(var bd in Buildings)
                {
                    switch (bd.Type)
                    {
                        case "Main0":
                            FillRectWithColor(total, Color.Blue, bd.X, bd.Y, 3, 3);
                            break;
                        case "Main1":
                            FillRectWithColor(total, Color.Red, bd.X, bd.Y, 3, 3);
                            break;
                        case "Rock":
                            FillRectWithColor(total, Color.Gray, bd.X, bd.Y, 1, 1);
                            break;
                        case "BigRock":
                            FillRectWithColor(total, Color.DarkGray, bd.X, bd.Y, 2, 2);
                            break;
                    }
                }
                return total;
            }

            /// <summary>
            /// Fills the rectangle in bitmap with color c. Coordiantes can be out of range.
            /// </summary>
            private void FillRectWithColor(Bitmap bitmap, Color c, int x, int y, int width, int height)
            {
                for(int i=0;i<width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        int xx = x + i;
                        int yy = y + j;
                        if(xx>=0 && xx<bitmap.Width && yy >=0 && yy < bitmap.Height)
                        {
                            bitmap.SetPixel(xx, yy, c);
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
                                    Math.Max(color.R + intensity, 255),
                                    Math.Max(color.G + intensity, 255),
                                    Math.Max(color.B + intensity, 255)));
                        }
                    }
            }

            /// <summary>
            /// Returns true iff the coordinate on the map is valid.
            /// </summary>
            public bool ValidCoordinate(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

            /// <summary>
            /// Draws the color to the terrain map.
            /// </summary>
            public void DrawTerrain(Color color, int x, int y , int width, int height)
            {
                FillRectWithColor(TerrainMap, color, x, y, width, height);
            }

            /// <summary>
            /// Draws to the nutrients map.
            /// </summary>
            public void AddNutrients(int x, int y, int width, int height)
            {
                AddRectIntensity(NutrientsMap, -25, x, y, width, height);
            }
        }

        private MapDescription MapDescr { get; set; }
        private enum DrawOption
        {
            DRAW_LAND,
            DRAW_SHALLOW_WATER,
            DRAW_DEEP_WATER,
            DRAW_NUTRIENTS,
            ADD_BUILDING,
            NO_ACTION
        }
        private DrawOption DrawOpt { get; set; }
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
            if(int.TryParse(widthTB.Text, out int width) &&
                int.TryParse(heightTB.Text, out int height)&&
                width > 0 && width <= MAX_MAP_WIDTH &&
                height > 0 && height <= MAX_MAP_HEIGHT &&
                ValidName(newNameTB.Text))
            {
                MapDescr = new MapDescription(width, height);
                mapPB.Invalidate();
                mapPB.Refresh();
            }
        }

        /// <summary>
        /// Returns true if name can be used as name of the map.
        /// </summary>
        private bool ValidName(string name) => name!="";


        /// <summary>
        /// Returns point in map coordinates. Input coordiantes x and y have to be relative
        /// to the picture box.
        /// </summary>
        private Point MapCoordinates(int x, int y)
        {
            int left = (mapPB.Width - 2 * MapDescr.Width) / 2;
            int top = (mapPB.Height - 2 * MapDescr.Height) / 2;
            return new Point((x - left) / 2, (y - top) / 2);
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
            }
        }

        private void MapPB_MouseDown(object sender, MouseEventArgs e)
        {
            if (MapDescr == null)
                return;

            Point mapPoint = MapCoordinates(e.X, e.Y);

            if (MapDescr.ValidCoordinate(mapPoint.X, mapPoint.Y))
            {
                if (DrawOpt == DrawOption.ADD_BUILDING)
                {
                    //todo:add the building to the map
                }
            }
            mapPB.Refresh();
        }

        private void MapPB_Paint(object sender, PaintEventArgs e)
        {
            if (MapDescr != null)
            {
                Graphics g = e.Graphics;
                int left = (mapPB.Width - 2 * MapDescr.Width) / 2;
                int top = (mapPB.Height - 2 * MapDescr.Height) / 2;
                g.DrawImage(MapDescr.TotalMap(), left, top, 2 * MapDescr.Width, 2 * MapDescr.Height);
            }
        }

        private void BuildingRB_Click(object sender, EventArgs e)
        {
            switch (((RadioButton)sender).Name)
            {
                case "main0RB":
                    DrawOpt = DrawOption.DRAW_DEEP_WATER;
                    break;
                case "main1RB":
                    DrawOpt = DrawOption.DRAW_SHALLOW_WATER;
                    break;
                case "rockRB":
                    DrawOpt = DrawOption.DRAW_LAND;
                    break;
                case "bigRockRB":
                    DrawOpt = DrawOption.DRAW_NUTRIENTS;
                    break;
            }
        }

        private void MapPB_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (MapDescr == null)
                    return;

                Point mapPoint = MapCoordinates(e.X, e.Y);

                if (MapDescr.ValidCoordinate(mapPoint.X, mapPoint.Y))
                {
                    int size = 5;//of brush
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
                            MapDescr.AddNutrients(x, y, size, size);
                            break;
                    }
                }
                mapPB.Refresh();
            }
        }
    }
}
