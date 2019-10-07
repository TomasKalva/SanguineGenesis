using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace SanguineGenesis.GUI
{
    /// <summary>
    /// Describes the contents of the tile map. All images are aligned to a square grid.
    /// </summary>
    class ImageAtlas
    {
        /// <summary>
        /// Size of tile in the atlas.
        /// </summary>
        private const int TILE_SIZE = 64;
        /// <summary>
        /// Width of the atlas.
        /// </summary>
        private const int ATLAS_WIDTH = 2048;
        /// <summary>
        /// Height of the atlas.
        /// </summary>
        private const int ATLAS_HEIGHT = 2048;

        /// <summary>
        /// Contains animations for entity types.
        /// </summary>
        private Dictionary<string, Animation> entitiesAnimations;
        /// <summary>
        /// For each number 0-9 contains corresponding glyph. At the index
        /// -1 is glyph for '.'.
        /// </summary>
        private Dictionary<int, Rect> glyphs;

        /// <summary>
        /// Position of unit circle in the atlas.
        /// </summary>
        public Rect UnitCircle { get; }
        /// <summary>
        /// Position of transparent square in the atlas.
        /// </summary>
        public Rect UnitsSelector { get; }
        /// <summary>
        /// Position of blank white square in the atlas.
        /// </summary>
        public Rect BlankWhite { get; }

        private static ImageAtlas imageAtlas;
        public static ImageAtlas GetImageAtlas => imageAtlas;
        static ImageAtlas()
        {
            imageAtlas=new ImageAtlas();
        }

        private ImageAtlas()
        {
            InitializeDigitImages();
            LoadEntitiesAnimations("Images/atlas0.xml");

            UnitCircle = ToRelative(GridToCoordinates(2, 0, 1, 1));
            UnitsSelector = ToRelative(GridToCoordinates(3, 0, 1, 1));
            BlankWhite = ToRelative(GridToCoordinates(0, 1, 1, 1));

        }

        /// <summary>
        /// Initialize glyphs with images of digits and '.'.
        /// </summary>
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

        /// <summary>
        /// Add image for the glyph number.
        /// </summary>
        private void AddGlyphImage(int glyph, float left, float bottom, float width, float height)
        {
            glyphs.Add(glyph, ToRelative(GridToCoordinates(new Rect(left, bottom, left + width, bottom + height))));
        }

        /// <summary>
        /// Loads animations from the file.
        /// </summary>
        /// <param name="animationDescriptionFileName">The file name.</param>
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
                    float centerX = float.Parse(animation.GetAttribute("CenterX"), CultureInfo.InvariantCulture);
                    float centerY = float.Parse(animation.GetAttribute("CenterY"), CultureInfo.InvariantCulture);
                    float animWidth = float.Parse(firstImage.GetAttribute("Width"), CultureInfo.InvariantCulture);
                    float animHeight = float.Parse(firstImage.GetAttribute("Height"), CultureInfo.InvariantCulture);
                    string action = animation.GetAttribute("Action");

                    //iterate over all images of the animation
                    var animationImages = new List<Rect>();
                    var animChangeTime = new List<float>(animationImages.Count);
                    foreach (XmlElement image in animation.ChildNodes)
                    {
                        //load extents of the image
                        float x = float.Parse(image.GetAttribute("X"), CultureInfo.InvariantCulture);
                        float y = float.Parse(image.GetAttribute("Y"), CultureInfo.InvariantCulture);
                        float width = float.Parse(image.GetAttribute("Width"), CultureInfo.InvariantCulture);
                        float height = float.Parse(image.GetAttribute("Height"), CultureInfo.InvariantCulture);
                        animationImages.Add(
                            ToRelative(
                                GridToCoordinates(x, y, width, height)));
                        
                        //load duration of the image
                        string dur = image.GetAttribute("Duration");
                        if (dur != "")
                        {
                            float duration = float.Parse(dur, CultureInfo.InvariantCulture);
                            animChangeTime.Add(duration);
                        }
                        else
                        {
                            animChangeTime.Add(0.5f);
                        }
                    }

                    string animationName = AnimationName(entityType, action);

                    //add new animation to the animation dictionary
                    AddEntitiesAnimation(animationName, action, new Vector2(centerX, centerY), animWidth, animHeight, animChangeTime, animationImages);
                }
            }
            }catch(Exception e)
            {
                ;
            }


        }

        private string AnimationName(string entityType, string action)  => entityType + "__" + action;

        /// <summary>
        /// Creates new Animation for the entity with the given parameters and adds it to entitiesAnimations.
        /// </summary>
        private void AddEntitiesAnimation(string animationName, string action, Vector2 leftBottom,float width, float height, List<float> animChangeTimes, List<Rect> images)
        {
            entitiesAnimations.Add(animationName, new Animation(action, leftBottom,width, height, animChangeTimes, images));
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
        /// Get animation for the entity.
        /// </summary>
        public Animation GetAnimation(string entityType, string action)
        {
            string animationName = AnimationName(entityType, action);
            if (entitiesAnimations.TryGetValue(animationName, out Animation anim))
                return anim;
            else
                return GetAnimation("NO_ENTITY","NO_ACTION");
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

    /// <summary>
    /// Represents animation and location of its images in the atlas.
    /// </summary>
    class Animation
    {
        /// <summary>
        /// Left bottom position in the atlas. In grid coordinates.
        /// </summary>
        public Vector2 LeftBottom { get; }
        /// <summary>
        /// Rectangles describe position and extens of the images of animation in the atlas.
        /// In coordinates relative to the atlas extents.
        /// Rect(x, y, width, height)
        /// </summary>
        public List<Rect> Images { get; }
        /// <summary>
        /// Width in grid coordinates.
        /// </summary>
        public float Width { get; }
        /// <summary>
        /// Height in grid coordinates.
        /// </summary>
        public float Height { get; }
        /// <summary>
        /// The time in seconds it takes for images to change.
        /// </summary>
        public List<float> ChangeTimes { get; }
        /// <summary>
        /// Number of images in animation.
        /// </summary>
        public int Length => Images.Count;
        /// <summary>
        /// Action that this animation represents.
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Creates new animation.
        /// </summary>
        /// <param name="leftBottom">Left bottom coordinate in the atlas.</param>
        /// <param name="width">Width of the images in animation.</param>
        /// <param name="height">Height of the images in animation.</param>
        /// <param name="animChangeTimes">Time it takes to change images.</param>
        /// <param name="images">First two numbers represent left bottom position of the image, second two represent width and height.</param>
        public Animation(string action, Vector2 leftBottom, float width, float height, List<float> animChangeTimes, List<Rect> images)
        {
            Action = action;
            LeftBottom = leftBottom;
            Images = images;
            Width = width;
            Height = height;
            ChangeTimes = animChangeTimes;
        }
    }
}
