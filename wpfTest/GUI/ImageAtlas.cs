using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GUI
{
    /// <summary>
    /// Describes the contents of the tile map. All images are aligned to a square grid.
    /// </summary>
    public class ImageAtlas
    {
        private static ImageAtlas imageAtlas;

        private const int TILE_SIZE = 64;
        private const int ATLAS_WIDTH = 640;
        private const int ATLAS_HEIGHT = 640;
        private Dictionary<Terrain, Rect> terrainImages;

        public Rect UnitCircle { get; }
        public Rect UnitsSelector { get; }

        public static ImageAtlas GetImageAtlas => imageAtlas;

        static ImageAtlas()
        {
            imageAtlas=new ImageAtlas();
        }

        private ImageAtlas()
        {
            terrainImages = new Dictionary<Terrain, Rect>();
            AddTerrainImage(Terrain.LOW_GRASS, 0, 0, 1, 1);
            AddTerrainImage(Terrain.DEEP_WATER, 1, 0, 1, 1);
            AddTerrainImage(Terrain.DIRT, 4, 0, 1, 1);
            AddTerrainImage(Terrain.SHALLOW_WATER, 5, 0, 1, 1);
            AddTerrainImage(Terrain.HIGH_GRASS, 6, 0, 1, 1);
            AddTerrainImage(Terrain.ENTANGLING_ROOTS, 7, 0, 1, 1);
            AddTerrainImage(Terrain.SAVANA_DIRT, 8, 0, 1, 1);
            AddTerrainImage(Terrain.SAVANA_GRASS, 9, 0, 1, 1);

            UnitCircle = ToRelative(GridToCoordinates(2, 0, 1, 1));
            UnitsSelector = ToRelative(GridToCoordinates(3, 0, 1, 1));
        }

        private void AddTerrainImage(Terrain terrain, int left, int bottom, int width, int height)
        {
            terrainImages.Add(terrain, ToRelative(GridToCoordinates(left, bottom,width,height)));
        }

        /// <summary>
        /// Transforms coordinates of a rectangle in the grid to the coordinates in the atlas.
        /// </summary>
        private Rect GridToCoordinates(int left, int bottom, int width, int height)
        {
            float l = left * TILE_SIZE + 1;
            float b = bottom * TILE_SIZE + 1;
            return new Rect(l, b,
                            l + width * TILE_SIZE - 2,
                            b + height * TILE_SIZE - 2);
        }

        /// <summary>
        /// Makes the rect's coordinates relative to the image atlas.
        /// </summary>
        private Rect ToRelative(Rect rect)
        {
            return new Rect(rect.Left / ATLAS_WIDTH,
                rect.Bottom / ATLAS_HEIGHT,
                rect.Right / ATLAS_WIDTH,
                rect.Top / ATLAS_HEIGHT);
        }

        /// <summary>
        /// Get coordinates, where the image for the terrain is located in the atlas. The coordinates
        /// are relative to the atlas.
        /// </summary>
        public Rect GetTerrainCoords(Terrain terrain)
        {
            return terrainImages[terrain];
        }
    }
}
