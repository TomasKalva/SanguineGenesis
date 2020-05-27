using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Maps;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SanguineGenesis.GUI
{
    /// <summary>
    /// Describes the contents of the texture atlas. All images are aligned to a square grid.
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
        /// For each digit 0-9 contains corresponding symbol. At the index
        /// 10 is symbol for '.', 11 is '/'.
        /// </summary>
        private Dictionary<int, Rect> digits;
        /// <summary>
        /// Contains triangles with numbers from 0 to 99.
        /// </summary>
        private Rect[] numberedTriangles;
        /// <summary>
        /// Contains textures for nodes.
        /// </summary>
        private Dictionary<NodeDescription, Rect> nodeTextures;

        /// <summary>
        /// Position of white unit circle in the atlas.
        /// </summary>
        public Rect UnitCircleGray { get; }
        /// <summary>
        /// Position of red unit circle in the atlas.
        /// </summary>
        public Rect UnitCircleRed { get; }
        /// <summary>
        /// Position of blue unit circle in the atlas.
        /// </summary>
        public Rect UnitCircleBlue { get; }
        /// <summary>
        /// Position of yellow unit circle in the atlas.
        /// </summary>
        public Rect UnitCircleYellow { get; }
        /// <summary>
        /// Position of white unit circle in the atlas.
        /// </summary>
        public Rect UnitCircleWhite { get; }
        /// <summary>
        /// Position of transparent square in the atlas.
        /// </summary>
        public Rect UnitsSelector { get; }

        /// <summary>
        /// Position of black square in the atlas.
        /// </summary>
        public Rect BlackSquare { get; }
        /// <summary>
        /// Position of red square in the atlas.
        /// </summary>
        public Rect RedSquare { get; }
        /// <summary>
        /// Position of blue square in the atlas.
        /// </summary>
        public Rect BlueSquare { get; }
        /// <summary>
        /// Position of green square in the atlas.
        /// </summary>
        public Rect GreenSquare { get; }

        /// <summary>
        /// Composite index for nodeTextures.
        /// </summary>
        class NodeDescription
        {
            public Biome Biome { get; }
            public Terrain Terrain { get; }
            public SoilQuality SoilQuality { get; }
            public bool Visible { get; }
            
            public NodeDescription(Biome biome, Terrain terrain, SoilQuality soilQuality, bool visible)
            {
                Biome = biome;
                Terrain = terrain;
                SoilQuality = soilQuality;
                Visible = visible;
            }

            public static bool operator ==(NodeDescription a, NodeDescription b) =>
                a.Biome == b.Biome && a.Terrain == b.Terrain &&
                a.SoilQuality == b.SoilQuality && a.Visible == b.Visible;

            public static bool operator !=(NodeDescription a, NodeDescription b) => !(a == b);

            public override bool Equals(object obj)
            {
                if (obj is NodeDescription nd)
                    return this == nd;
                else
                    return false;
            }

            public override int GetHashCode()
            {
                return Biome.GetHashCode() * 7 + Terrain.GetHashCode() * 13 + SoilQuality.GetHashCode() * 23 + Visible.GetHashCode() * 37;
            }
        }

        /// <summary>
        /// Singleton instance of this class.
        /// </summary>
        public static ImageAtlas GetImageAtlas { get; private set; }

        /// <summary>
        /// Initializes this class with new instance.
        /// </summary>
        public static void Init() 
        {
            if(GetImageAtlas==null)
                GetImageAtlas = new ImageAtlas();
        }

        private ImageAtlas()
        {
            XDocument doc = XDocument.Load("Images/atlas.xml");

            //returns child element of root with name=elemName
            XElement elWithName(string elemName) => (from el in doc.Root.Elements()
                                                     where el.Name.LocalName == elemName
                                                     select el).First();

            InitializeDigitImages(elWithName("Digits"));
            InitializeEntitiesAnimations(elWithName("Entities"));
            InitializeNumberedTriangles(elWithName("NumbersArray"));
            InitializeNodes(elWithName("Nodes"));

            //finds subelement with attribute name=attrName
            XElement shapesImgElWithName(string attrName) => (from el in (from el in doc.Root.Elements() where el.Name.LocalName == "Shapes" select el).First().Elements()
                                                              where el.Attribute("Name").Value == attrName
                                                              select (XElement)el.FirstNode).First();

            //circles
            UnitCircleGray = LoadImage(shapesImgElWithName("UnitCircleGray"));
            UnitCircleRed = LoadImage(shapesImgElWithName("UnitCircleRed"));
            UnitCircleBlue = LoadImage(shapesImgElWithName("UnitCircleBlue"));
            UnitCircleYellow = LoadImage(shapesImgElWithName("UnitCircleYellow"));
            UnitCircleWhite = LoadImage(shapesImgElWithName("UnitCircleWhite"));

            UnitsSelector = LoadImage(shapesImgElWithName("UnitsSelector"));

            //squares
            BlackSquare = LoadImage(shapesImgElWithName("BlackSquare"));
            RedSquare = LoadImage(shapesImgElWithName("RedSquare"));
            GreenSquare = LoadImage(shapesImgElWithName("GreenSquare"));
            BlueSquare = LoadImage(shapesImgElWithName("BlueSquare"));
        }

        /// <summary>
        /// Initialize digits with images of digits, '.' and '/'.
        /// </summary>
        private void InitializeDigitImages(XElement digits)
        {
            this.digits = new Dictionary<int, Rect>();

            foreach(XElement digit in digits.Elements())
            {
                int value = int.Parse(digit.Attribute("Value").Value, CultureInfo.InvariantCulture);
                this.digits.Add(value, LoadImage((XElement)digit.FirstNode));
            }
        }

        /// <summary>
        /// Initialize numbered triangles, numbersArray determines number of the triangles.
        /// </summary>
        private void InitializeNumberedTriangles(XElement numbersArray)
        {
            int tableTop = int.Parse(numbersArray.Attribute("Y").Value, CultureInfo.InvariantCulture);
            int tableLeft = int.Parse(numbersArray.Attribute("X").Value, CultureInfo.InvariantCulture);
            int width = int.Parse(numbersArray.Attribute("Width").Value, CultureInfo.InvariantCulture);
            int height = int.Parse(numbersArray.Attribute("Height").Value, CultureInfo.InvariantCulture);

            //initialize numbered triangles with the image extents
            int arraySize = width * height;
            numberedTriangles = new Rect[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                int x = tableLeft + (i / width);
                int y = tableTop - (i % height);
                numberedTriangles[i] = ToRelative(GridToPixels(x, y, 1, 1));
            }
        }

        /// <summary>
        /// Loads entities.
        /// </summary>
        /// <param name="animationDescriptionFileName">Element whose subelements are entities.</param>
        private void InitializeEntitiesAnimations(XElement entities)
        {
            //iterate over all entities
            entitiesAnimations = new Dictionary<string, Animation>();
            foreach (XElement entity in entities.Elements())
            {
                string entityType = entity.Attribute("EntityType").Value;
                //iterate over all animations of the entity
                foreach (XElement animation in entity.Elements())
                {
                    XElement firstImage = (XElement)animation.FirstNode;
                    float centerX = float.Parse(animation.Attribute("CenterX").Value, CultureInfo.InvariantCulture);
                    float centerY = float.Parse(animation.Attribute("CenterY").Value, CultureInfo.InvariantCulture);
                    float animWidth = float.Parse(firstImage.Attribute("Width").Value, CultureInfo.InvariantCulture);
                    float animHeight = float.Parse(firstImage.Attribute("Height").Value, CultureInfo.InvariantCulture);
                    string action = animation.Attribute("Action").Value;

                    //iterate over all images of the animation
                    var animationImages = new List<Rect>();
                    var animChangeTime = new List<float>(animationImages.Count);
                    foreach (XElement image in animation.Elements())
                    {
                        //load extents of the image
                        animationImages.Add(LoadImage(image));

                        //load duration of the image
                        var dur = image.Attribute("Duration");
                        if (dur != null)
                        {
                            float duration = float.Parse(dur.Value, CultureInfo.InvariantCulture);
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
        }

        /// <summary>
        /// Loads nodes images.
        /// </summary>
        /// <param name="animationDescriptionFileName">Element whose subelements are nodes.</param>
        private void InitializeNodes(XElement nodes)
        {
            //iterate over all nodes
            nodeTextures = new Dictionary<NodeDescription, Rect>();
            foreach (XElement node in nodes.Elements())
            {
                Biome biome = (Biome)Enum.Parse(typeof(Biome), node.Attribute("Biome").Value);
                Terrain terrain = (Terrain)Enum.Parse(typeof(Terrain), node.Attribute("Terrain").Value);
                SoilQuality soilQuality = (SoilQuality)Enum.Parse(typeof(SoilQuality), node.Attribute("SoilQuality").Value);
                bool visible = bool.Parse(node.Attribute("Visible").Value);
                XElement image = (XElement)node.FirstNode;
                nodeTextures.Add(new NodeDescription(biome, terrain, soilQuality, visible), LoadImage(image));

            }
        }

        private string AnimationName(string entityType, string action)  => entityType + "__" + action;

        /// <summary>
        /// Creates rectangle from imageElement. Extents of the rectangle are relative to the atlas extents.
        /// </summary>
        /// <param name="imageElement">Element that describes the image.</param>
        /// <returns></returns>
        private Rect LoadImage(XElement imageElement)
        {
            if (imageElement == null || imageElement.Name.LocalName != "Image")
                return default;

            //load extents of the image
            float x = float.Parse(imageElement.Attribute("X").Value, CultureInfo.InvariantCulture);
            float y = float.Parse(imageElement.Attribute("Y").Value, CultureInfo.InvariantCulture);
            float width = float.Parse(imageElement.Attribute("Width").Value, CultureInfo.InvariantCulture);
            float height = float.Parse(imageElement.Attribute("Height").Value, CultureInfo.InvariantCulture);

            return ToRelative(GridToPixels(x, y, width, height));
        }

        /// <summary>
        /// Creates new Animation for the entity with the given parameters and adds it to entitiesAnimations.
        /// </summary>
        private void AddEntitiesAnimation(string animationName, string action, Vector2 center,float width, float height, List<float> animChangeTimes, List<Rect> images)
        {
            entitiesAnimations.Add(animationName, new Animation(action, center,width, height, animChangeTimes, images));
        }

        /// <summary>
        /// Transforms coordinates of a rectangle in the grid to the coordinates in the atlas.
        /// </summary>
        private Rect GridToPixels(float left, float bottom, float width, float height)
        {
            float l = (int)(left * TILE_SIZE) + 1;
            float b = (int)(bottom * TILE_SIZE) + 1;
            return new Rect(l, b,
                            l + (int)(width * TILE_SIZE) - 2,
                            b + (int)(height * TILE_SIZE) - 2);
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
        /// Get coordinates, where the image for the node is located in the atlas. Returns darker
        /// copy of the texture, if visible is false. The coordinates are relative to the atlas.
        /// </summary>
        public Rect GetNodeTexture(Biome biome, SoilQuality soilQuality, Terrain terrain, bool visible)
        {
            if (nodeTextures.TryGetValue(new NodeDescription(biome, terrain, soilQuality, visible), out Rect value))
                return value;
            else
                return default;
        }

        /// <summary>
        /// Get animation for the entity.
        /// </summary>
        public Animation GetEntityAnimation(string entityType, string action)
        {
            string animationName = AnimationName(entityType, action);
            if (entitiesAnimations.TryGetValue(animationName, out Animation anim))
                return anim;
            else
                return GetEntityAnimation("NO_ENTITY","NO_ACTION");
        }

        /// <summary>
        /// Returns image for the int. Digits correspond to numbers 0-9, 10 is '.', 11 is '/'.
        /// For other values argument exception is thrown.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the image for the char doesn't exist.</exception>
        public Rect GetDigit(int digit)
        {
            if (digits.TryGetValue(digit, out Rect rect))
                return rect;
            else
                return default;
        }

        /// <summary>
        /// Returns triangle with number between 0 and 99. Returns triangle with 
        /// number 0 if number is out of 
        /// .
        /// </summary>
        public Rect GetNumberedTriangle(int number)
        {
            if(number>= 0 && number < numberedTriangles.Length)
                return numberedTriangles[number];
            else
                return default;
        }
    }

    /// <summary>
    /// Represents animation and location of its images in the atlas.
    /// </summary>
    class Animation
    {
        /// <summary>
        /// Center of the image (on the map). In grid coordinates.
        /// </summary>
        public Vector2 Center { get; }
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
        /// <param name="center">Left bottom coordinate in the atlas.</param>
        /// <param name="width">Width of the images in animation.</param>
        /// <param name="height">Height of the images in animation.</param>
        /// <param name="animChangeTimes">Time it takes to change images.</param>
        /// <param name="images">First two numbers represent left bottom position of the image, second two represent width and height.</param>
        public Animation(string action, Vector2 center, float width, float height, List<float> animChangeTimes, List<Rect> images)
        {
            Action = action;
            Center = center;
            Images = images;
            Width = width;
            Height = height;
            ChangeTimes = animChangeTimes;
        }
    }

    /// <summary>
    /// Animation with current image and timer.
    /// </summary>
    class AnimationState
    {
        /// <summary>
        /// Animation whose images are used.
        /// </summary>
        public Animation Animation { get; }
        /// <summary>
        /// Time the current image was shown for.
        /// </summary>
        private float timeOfImage;
        /// <summary>
        /// Index of the current image.
        /// </summary>
        private int image;

        /// <summary>
        /// Location of the current image in atlas.
        /// </summary>
        public Rect CurrentImage => Animation.Images[image];

        public AnimationState(Animation anim)
        {
            Animation = anim;
            timeOfImage = 0;
        }

        /// <summary>
        /// Update the state.
        /// </summary>
        public void Step(float deltaT)
        {
            timeOfImage += deltaT;
            if (timeOfImage >= Animation.ChangeTimes[image])
            {
                //move to the next image
                timeOfImage -= Animation.ChangeTimes[image];
                image = (image + 1) % Animation.Length;
            }
        }
    }
}
