using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
        private Dictionary<string, Animation> entitiesAnimations;
        private Dictionary<int, Rect> glyphs;

        public Rect UnitCircle { get; }
        public Rect UnitsSelector { get; }
        public Rect BlankWhite { get; }

        private struct Square
        {
            Biome Biome { get; }
            SoilQuality SoilQuality { get; }
            Terrain Terrain { get; }

            public Square(Biome biome, SoilQuality soilQuality, Terrain terrain)
            {
                Biome = biome;
                SoilQuality = soilQuality;
                Terrain = terrain;
            }
        }

        public static ImageAtlas GetImageAtlas => imageAtlas;

        static ImageAtlas()
        {
            imageAtlas=new ImageAtlas();
        }

        private ImageAtlas()
        {
            InitializeDigitImages();
            //InitializeUnitsAnimations();
            LoadEntitiesAnimations("Images/atlas0.xml");

            UnitCircle = ToRelative(GridToCoordinates(2, 0, 1, 1));
            UnitsSelector = ToRelative(GridToCoordinates(3, 0, 1, 1));
            BlankWhite = ToRelative(GridToCoordinates(0, 1, 1, 1));

        }

        private void InitializeDigitImages()
        {
            glyphs = new Dictionary<int, Rect>();
            float offset = 0;
            AddGlyphImage(0, offset, 3, 0.5f, 1);
            offset += 0.5f;
            AddGlyphImage(1, offset, 3, 0.5f, 1);
            offset += 0.5f;
            AddGlyphImage(2, offset, 3, 0.5f, 1);
            offset += 0.5f;
            AddGlyphImage(3, offset, 3, 0.5f, 1);
            offset += 0.5f;
            AddGlyphImage(4, offset, 3, 0.5f, 1);
            offset += 0.5f;
            AddGlyphImage(5, offset, 3, 0.5f, 1);
            offset += 0.5f;
            AddGlyphImage(6, offset, 3, 0.5f, 1);
            offset += 0.5f;
            AddGlyphImage(7, offset, 3, 0.5f, 1);
            offset += 0.5f;
            AddGlyphImage(8, offset, 3, 0.5f, 1);
            offset += 0.5f;
            AddGlyphImage(9, offset, 3, 0.5f, 1);
            offset += 0.5f;
            AddGlyphImage(-1, offset, 3, 0.5f, 1);
        }

        private void AddGlyphImage(int glyph, float left, float bottom, float width, float height)
        {
            glyphs.Add(glyph, ToRelative(GridToCoordinates(new Rect(left, bottom, left + width, bottom + height))));
        }

        private void InitializeUnitsAnimations()
        {
            entitiesAnimations = new Dictionary<string, Animation>();
            AddEntitiesAnimation("TIGER",
                new Vector2(0.75f, 0.2f),
                1.5f, 1f, 0.5f,
                new List<Rect>()
                { ToRelative(GridToCoordinates(0,2,1.5f,1)),
                  ToRelative(GridToCoordinates(1.5f,2,1.5f,1))});
            AddEntitiesAnimation("BAOBAB",
                new Vector2(2.5f, 1.5f),
                5, 6, 0.8f,
                new List<Rect>()
                { ToRelative(GridToCoordinates(10,0,5,6)),
                  ToRelative(GridToCoordinates(15,0,5,6)),
                  ToRelative(GridToCoordinates(10,0,5,6)),
                  ToRelative(GridToCoordinates(20,0,5,6)),});
        }

        private void LoadEntitiesAnimations(string animationDescriptionFileName)
        {
            //try block used ONLY for easier debugging
            try
            {
            XmlDocument doc = new XmlDocument();
            doc.Load(animationDescriptionFileName);

            //iterate over all entities
            entitiesAnimations = new Dictionary<string, Animation>();
            foreach (XmlElement entity in doc.GetElementsByTagName("Entities")[0].ChildNodes)
            {
                string entityType = entity.GetAttribute("EntityType");
                //iterate over all animations of the entity
                foreach(XmlElement animation in entity.ChildNodes)
                {
                    XmlElement firstImage = (XmlElement)animation.FirstChild;
                    float centerX = float.Parse(animation.GetAttribute("CenterX"));
                    float centerY = float.Parse(animation.GetAttribute("CenterY"));
                    float animWidth = float.Parse(firstImage.GetAttribute("Width"));
                    float animHeight = float.Parse(firstImage.GetAttribute("Height"));
                    
                    //iterate over all images of the animation
                    var animationImages = new List<Rect>();
                    foreach (XmlElement image in animation.ChildNodes)
                    {
                        float x = float.Parse(image.GetAttribute("X"));
                        float y = float.Parse(image.GetAttribute("Y"));
                        float width = float.Parse(image.GetAttribute("Width"));
                        float height = float.Parse(image.GetAttribute("Height"));
                        animationImages.Add(
                            ToRelative(
                                GridToCoordinates(x, y, width, height)));
                    }

                    float animChangeTime = 0.5f;

                    //add new animation to the animation dictionary
                    AddEntitiesAnimation(entityType, new Vector2(centerX, centerY), animWidth, animHeight, animChangeTime, animationImages);
                }
            }
            }catch(Exception e)
            {
                ;
            }


        }

        private void AddEntitiesAnimation(string unit, Vector2 leftBottom,float width, float height, float animChangeTimeS, List<Rect> images)
        {
            entitiesAnimations.Add(unit, new Animation(leftBottom,width, height, animChangeTimeS, images));
        }

        /// <summary>
        /// Transforms coordinates of a rectangle in the grid to the coordinates in the atlas.
        /// </summary>
        private Rect GridToCoordinates(float left, float bottom, float width, float height)
        {
            float l = (int)(left * TILE_SIZE) + 1;
            float b = (int)(bottom * TILE_SIZE) + 1;
            return new Rect(l, b,
                            l + (int)(width * TILE_SIZE) - 2,
                            b + (int)(height * TILE_SIZE) - 2);
        }

        /// <summary>
        /// Transforms coordinates of a rectangle in the grid to the coordinates in the atlas.
        /// </summary>
        private Rect GridToCoordinates(Rect rect)
        {
            return GridToCoordinates(rect.Left, rect.Bottom, rect.Width, rect.Height);
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
        public Rect GetTileCoords(Biome biome, SoilQuality soilQuality, Terrain terrain)
        {
            Rect coords=default(Rect);
            if(terrain==Terrain.SHALLOW_WATER)
                switch (biome)
                {
                    case Biome.DEFAULT:
                        coords = new Rect(5, 0, 6f, 1f);
                        break;
                    case Biome.SAVANNA:
                        coords = new Rect(3, 1, 4, 2);
                        break;
                    case Biome.RAINFOREST:
                        coords = new Rect(1, 1, 2, 2);
                        break;
                }
            else if(terrain==Terrain.DEEP_WATER)
                switch (biome)
                {
                    case Biome.DEFAULT:
                        coords = new Rect(1, 0, 2, 1);
                        break;
                    case Biome.SAVANNA:
                        coords = new Rect(4, 1, 5, 2);
                        break;
                    case Biome.RAINFOREST:
                        coords = new Rect(2, 1, 3, 2);
                        break;
                }
            else
            {
                //terrain is land
                switch (biome)
                {
                    case Biome.RAINFOREST:
                        switch (soilQuality)
                        {
                            case SoilQuality.LOW:
                                coords =  new Rect (0, 0, 1, 1);
                                break;
                            case SoilQuality.MEDIUM:
                                coords =  new Rect(6, 0, 7, 1);
                                break;
                            case SoilQuality.HIGH:
                                coords =  new Rect(7, 0, 8, 1);
                                break;
                        }
                        break;
                    case Biome.SAVANNA:
                        switch (soilQuality)
                        {
                            case SoilQuality.LOW:
                                coords =  new Rect(8, 0, 9, 1);
                                break;
                            case SoilQuality.MEDIUM:
                                coords =  new Rect(9, 0, 10, 1);
                                break;
                            case SoilQuality.HIGH:
                                throw new ArgumentException("Savanna doesn't have high quality soil!");
                        }
                        break;
                    default:
                        coords =  new Rect(4, 0, 5, 1);
                        break;
                }
            }
            if(coords.Equals(default(Rect)))
                throw new ArgumentException("Combination " + terrain + ", " + biome + ", " + soilQuality + " isn't valid");

            return ToRelative(GridToCoordinates(coords));
        }

        /// <summary>
        /// Get animation for the unit.
        /// </summary>
        public Animation GetAnimation(string unitType)
        {
            if (entitiesAnimations.TryGetValue(unitType, out Animation anim))
                return anim;
            else
                return GetAnimation("BAOBAB");
        }

        /// <summary>
        /// Returns image for the char. If it doesn't exist, argument exception is thrown.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the image for the char doesn't exist.</exception>
        public Rect GetGlyph(int glyph)
        {
            if (glyphs.TryGetValue(glyph, out Rect rect))
                return rect;
            else
                throw new ArgumentException(glyph + " is not a valid glyph!");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leftBottom"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="animChangeTimeS"></param>
        /// <param name="images">First two numbers represent left bottom position of the image, second two represent width and height.</param>
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
