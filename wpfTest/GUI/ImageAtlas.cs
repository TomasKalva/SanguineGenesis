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
        private const int ATLAS_WIDTH = 2048;
        private const int ATLAS_HEIGHT = 2048;
        private Dictionary<Terrain, Rect> terrainImages;
        private Dictionary<EntityType, Animation> unitsAnimations;

        public Rect UnitCircle { get; }
        public Rect UnitsSelector { get; }
        public Rect BlankWhite { get; }

        public static ImageAtlas GetImageAtlas => imageAtlas;

        static ImageAtlas()
        {
            imageAtlas=new ImageAtlas();
        }

        private ImageAtlas()
        {
            InitializeTerrainImages();
            InitializeUnitsAnimations();

            UnitCircle = ToRelative(GridToCoordinates(2, 0, 1, 1));
            UnitsSelector = ToRelative(GridToCoordinates(3, 0, 1, 1));
            BlankWhite = ToRelative(GridToCoordinates(0, 1, 1, 1));

        }

        private void InitializeTerrainImages()
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
        }

        private void InitializeUnitsAnimations()
        {
            unitsAnimations = new Dictionary<EntityType, Animation>();
            AddUnitsAnimation(EntityType.TIGER,
                new Vector2(0.75f, 0.2f),
                1.5f, 1f, 0.5f,
                new List<Rect>()
                { ToRelative(GridToCoordinates(0,2,1.5f,1)),
                  ToRelative(GridToCoordinates(1.5f,2,1.5f,1))});
            AddUnitsAnimation(EntityType.BAOBAB,
                new Vector2(2.5f, 1.5f),
                5, 6, 0.8f,
                new List<Rect>()
                { ToRelative(GridToCoordinates(10,0,5,6)),
                  ToRelative(GridToCoordinates(15,0,5,6)),
                  ToRelative(GridToCoordinates(10,0,5,6)),
                  ToRelative(GridToCoordinates(20,0,5,6)),});
        }

        private void AddTerrainImage(Terrain terrain, float left, float bottom, float width, float height)
        {
            terrainImages.Add(terrain, ToRelative(GridToCoordinates(left, bottom,width,height)));
        }

        private void AddUnitsAnimation(EntityType unit, Vector2 leftBottom,float width, float height, float animChangeTimeS, List<Rect> images)
        {
            unitsAnimations.Add(unit, new Animation(leftBottom,width, height, animChangeTimeS, images));
        }

        /// <summary>
        /// Transforms coordinates of a rectangle in the grid to the coordinates in the atlas.
        /// </summary>
        private Rect GridToCoordinates(float left, float bottom, float width, float height)
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

        /// <summary>
        /// Get animation for the unit.
        /// </summary>
        public Animation GetAnimation(EntityType unitType)
        {
            return unitsAnimations[unitType];
        }
    }

    public class Animation
    {
        public Vector2 LeftBottom { get; }
        public List<Rect> Images { get; }
        public float Width { get; }
        public float Height { get; }
        public float ChangeTimeS { get; }
        public int Length => Images.Count;

        public Animation(Vector2 leftBottom, float width, float height, float animChangeTimeS, List<Rect> images)
        {
            LeftBottom = leftBottom;
            Images = images;
            Width = width;
            Height = height;
            ChangeTimeS = animChangeTimeS;
        }
    }
}
