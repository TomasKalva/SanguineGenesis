using GlmNet;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI;
using SanguineGenesis.GameControls;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps.MovementGenerating;
using SanguineGenesis.GameLogic.Maps.VisibilityGenerating;

namespace SanguineGenesis.GUI
{
    /// <summary>
    /// Used for drawing objects in game to the OpenGL control.
    /// </summary>
    static class OpenGLAtlasDrawer
    {
        //the projection, view and model matrices.
        static mat4 projectionMatrix;
        static mat4 viewMatrix;
        static mat4 modelMatrix;

        //vertex shader attribute indices
        const uint attributeIndexPosition = 0;
        const uint attributeIndexUVCoord = 1;
        const uint attributeIndexTexBL = 2;
        
        //the shader program for the vertex and fragment shader
        static private ShaderProgram shaderProgram;

        #region Initialization

        /// <summary>
        /// Initializes the instance of OpenGL.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        /// <param name="width">The width of the drawing scene.</param>
        /// <param name="height">The height of the drawing scene.</param>
        public static void Initialize(OpenGL gl, float width, float height)
        {
            //create the shader program
            shaderProgram = new ShaderProgram();
            string vertexShaderSource;
            string fragmentShaderSource;
            using (var vertReader = new StreamReader("GUI\\OpenGL\\AtlasTexShader.vert"))
            using (var fragReader = new StreamReader("GUI\\OpenGL\\AtlasTexShader.frag"))
            {
                vertexShaderSource = vertReader.ReadToEnd();
                fragmentShaderSource = fragReader.ReadToEnd();
            }
            shaderProgram.Create(gl, vertexShaderSource, fragmentShaderSource, null);
            

            //set indices for the shader program attributes
            shaderProgram.BindAttributeLocation(gl, attributeIndexPosition, "in_Position");
            shaderProgram.BindAttributeLocation(gl, attributeIndexUVCoord, "in_TexCoord");
            shaderProgram.BindAttributeLocation(gl, attributeIndexTexBL, "in_TexLeftBottomWidthHeight");

            //compile the program
            shaderProgram.AssertValid(gl);

            //create projection matrix that maps points directly to screen coordinates
            projectionMatrix = glm.ortho(0f, width, 0f, height, 0f, 100f);

            //create view matrix that translates graphical objects to visible range
            viewMatrix = glm.translate(new mat4(1.0f), new vec3(0.0f, 0.0f, -1.0f));

            //create identity matrix as model matrix
            modelMatrix = new mat4(1.0f);

            //load image used as atlas and pass it to the shader program
            InitializeAtlas(gl, shaderProgram);
            
            //bind the shader program and set its parameters
            shaderProgram.Bind(gl);
            shaderProgram.SetUniformMatrix4(gl, "projectionMatrix", projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "viewMatrix", viewMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "modelMatrix", modelMatrix.to_array());
        }

        /// <summary>
        /// Loads the tile map texture, binds it and sets texture drawing parameters.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        private static void InitializeAtlas(OpenGL gl, ShaderProgram shaderProgram)
        {
            //enable drawing textures
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            //enable alpha channel for textures
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            //loads the texture to gl
            LoadTexture("Images/bigTileMap.png", gl, shaderProgram);

            //depth testing is not needed for drawing 2d images
            gl.Disable(OpenGL.GL_DEPTH);

            //set linear filtering
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
        }

        /// <summary>
        /// Loads the texture from fileName to gl. 
        /// </summary>
        /// <param name="fileName">The name of the file from which the texture will be loaded.</param>
        /// <param name="gl">The instance of OpenGL to which the texture will be loaded.</param>
        private static void LoadTexture(string fileName, OpenGL gl, ShaderProgram shaderProgram)
        {
            //load image and flip it vertically for easier indexing
            Bitmap textureImage = new Bitmap(fileName);
            textureImage.RotateFlip(RotateFlipType.RotateNoneFlipY);

            //generate id for the texture and then bind the image with this id
            uint[] textureIds = new uint[1];
            gl.GenTextures(1, textureIds);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureIds[0]);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA, textureImage.Width, textureImage.Height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE,
                textureImage.LockBits(new Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb).Scan0);

            textureImage.Dispose();
            shaderProgram.Bind(gl);

            int atlas = shaderProgram.GetUniformLocation(gl, "atlas");
            gl.Uniform1(atlas, 0);

            gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureIds[0]);
        }

        #endregion Initialization

        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        public static void Draw(OpenGL gl, GameplayOptions gameplayOptions)
        {
            //clear the scene
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);

            //draw vertex array buffers
            
            //draw map
            map.VertexBufferArray.Bind(gl);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, map.VertexCount);
            
            //draw nutrients map
            if (gameplayOptions.NutrientsVisible)
            {
                nutrientsMap.VertexBufferArray.Bind(gl);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, nutrientsMap.VertexCount);
            }

            //draw flowfield
            if (gameplayOptions.ShowFlowfield)
            {
                flowField.VertexBufferArray.Bind(gl);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, flowField.VertexCount);
            }

            //draw entities
            if (!entitiesEmpty)
            {
                entityCircles.VertexBufferArray.Bind(gl);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, entityCircles.VertexCount);

                entities.VertexBufferArray.Bind(gl);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, entities.VertexCount);
            }
            if (!entityIndicatorsEmpty) 
            { 
                entityIndicators.VertexBufferArray.Bind(gl);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, entityIndicators.VertexCount);
            }
            
            //draw selection frame
            selectionFrame.VertexBufferArray.Bind(gl);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 6);
            
        }


        /// <summary>
        /// Represents data that can be drawn by the shader program.
        /// </summary>
        private class MyBufferArray
        {
            public VertexBufferArray VertexBufferArray { get; set; }
            public VertexBuffer VertexDataBuffer { get; set; }
            public VertexBuffer UVDataBuffer { get; set; }
            public VertexBuffer TexAtlasDataBuffer { get; set; }

            /// <summary>
            /// Fills the array with uv textures when it is created or resized.
            /// </summary>
            public delegate float[] UVFiller(int size);
            public UVFiller UvFiller { get; }

            public float[] vertices;
            public float[] uv;
            public float[] texAtlas;

            /// <summary>
            /// True if the buffers contains no data.
            /// </summary>
            public bool Clear { get; private set; }

            public MyBufferArray(OpenGL gl, UVFiller uvFiller)
            {
                //initialize VertexBufferArray and link it to its VertexBuffers
                VertexBufferArray = new VertexBufferArray();
                VertexBufferArray.Create(gl);
                VertexBufferArray.Bind(gl);

                //initialize empty VertexDataBuffer
                VertexDataBuffer = new VertexBuffer();
                VertexDataBuffer.Create(gl);

                //initialize empty TextureDataBuffer
                UVDataBuffer = new VertexBuffer();
                UVDataBuffer.Create(gl);

                //initialize empty TexBLDataBuffer
                TexAtlasDataBuffer = new VertexBuffer();
                TexAtlasDataBuffer.Create(gl);

                VertexBufferArray.Unbind(gl);

                //create initial arrays so that they are not null
                vertices = new float[0];
                uv = new float[0];
                texAtlas = new float[0];

                UvFiller = uvFiller;
            }

            /// <summary>
            /// Binds the data to this vertex buffer array. Binds vertices, uv and atlas coordinates.
            /// </summary>
            public void BindData(OpenGL gl, int vStride, float[] vValues,
                                            int uvStride, float[] uvValues,
                                            int aStride, float[] aValues)
            {
                VertexBufferArray.Bind(gl);

                VertexDataBuffer.Bind(gl);
                VertexDataBuffer.SetData(gl, attributeIndexPosition, vValues, false, vStride);
                VertexDataBuffer.Unbind(gl);

                UVDataBuffer.Bind(gl);
                UVDataBuffer.SetData(gl, attributeIndexUVCoord, uvValues, false, uvStride);

                TexAtlasDataBuffer.Bind(gl);
                TexAtlasDataBuffer.SetData(gl, attributeIndexTexBL, aValues, false, aStride);

                VertexBufferArray.Unbind(gl);

                Clear = false;
            }

            /// <summary>
            /// Binds the data to this vertex buffer array. Binds vertices and atlas coordinates.
            /// </summary>
            public void BindData(OpenGL gl, int vStride, float[] vValues,
                                            int aStride, float[] aValues)
            {
                VertexBufferArray.Bind(gl);

                VertexDataBuffer.Bind(gl);
                VertexDataBuffer.SetData(gl, attributeIndexPosition, vValues, false, vStride);
                VertexDataBuffer.Unbind(gl);

                TexAtlasDataBuffer.Bind(gl);
                TexAtlasDataBuffer.SetData(gl, attributeIndexTexBL, aValues, false, aStride);

                VertexBufferArray.Unbind(gl);

                Clear = false;
            }

            /// <summary>
            /// Initialize arrays with zeros with the given sizes.
            /// </summary>
            public void InitializeArrays(OpenGL gl, int verticesSize, int uvSize, int texAtlasSize)
            {
                InitArray(ref vertices, verticesSize);
                //InitArray(ref uv, uvSize);
                InitUV(gl, ref uv, uvSize);
                InitArray(ref texAtlas, texAtlasSize);
            }

            /// <summary>
            /// If the array is shorter than the size, create a new array with the size, otherwise clear the array.
            /// </summary>
            private void InitArray(ref float[] array, int size)
            {
                if (array.Length < size)
                    array = new float[size];
                else
                    Array.Clear(array, 0, array.Length);
            }

            private void InitUV(OpenGL gl, ref float[] uvArray, int uvSize)
            {
                if (uv.Length < uvSize)
                {
                    uvArray = UvFiller(uvSize);
                    VertexBufferArray.Bind(gl);
                    UVDataBuffer.Bind(gl);
                    UVDataBuffer.SetData(gl, attributeIndexUVCoord, uvArray, false, 2/*uv stride*/);
                    VertexBufferArray.Unbind(gl);
                }
                else
                {

                }


            }

            public int VertexCount => vertices == null ? 0 : vertices.Length / 3;

            public void ClearBuffers(OpenGL gl)
            {
                BindData(gl, 1, new float[1],
                            1, new float[1],
                            1, new float[1]);
                Clear = true;
            }
        }

        //vertex buffer arrays which contain the buffers for vertex, 
        //texture and bottom left coordinates of textures
        private static MyBufferArray map;
        private static MyBufferArray flowField;
        private static MyBufferArray nutrientsMap;
        private static MyBufferArray entityCircles;
        private static MyBufferArray entities;
        private static MyBufferArray entityIndicators;
        private static MyBufferArray selectionFrame;

        //true if there are no entities to draw
        private static bool entitiesEmpty;
        //true if there are no entity indicators to draw
        private static bool entityIndicatorsEmpty;

        #region Map
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateMap(OpenGL gl)
        {
            map = new MyBufferArray(gl, CreateSquareUVCoordinates);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateMapDataBuffers(OpenGL gl, MapView mapView, Game game)
        {
            //prepare data
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewBottom = mapView.Bottom;

            bool[,] visibleVisibility = mapView.GetVisibleVisibilityMap(game.Players[game.CurrentPlayer.FactionID].VisibilityMap);
            Node[,] visible = mapView.GetVisibleNodes(game.Players[game.CurrentPlayer.FactionID].VisibleMap);
            int width = visible.GetLength(0);
            int height = visible.GetLength(1);

            //extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;

            int verticesPerOne = width * height * 6;
            int verticesSize = verticesPerOne * 3;
            int uvSize = verticesPerOne * 2;
            int textureAtlasSize = verticesPerOne * 4;
            
            map.InitializeArrays(gl, verticesSize, uvSize, textureAtlasSize);
            float[] vertices = map.vertices;
            float[] texAtlas = map.texAtlas;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //buffer indices
                    int coord = (i + j * width) * 6 * 3;
                    int bottomLeftInd = (i + j * width) * 6 * 4;

                    Node current = visible[i, j];
                    if (current == null)
                        continue;

                    bool isVisible = true;
                    if (visibleVisibility != null)
                        isVisible = visibleVisibility[i, j];
                    
                    Rect atlasCoords = ImageAtlas.GetImageAtlas.GetTileCoords(current.Biome,current.SoilQuality,current.Terrain, isVisible);

                    //tile position
                    float bottom = (current.Y - viewBottom) * sqH;
                    float top = (current.Y - viewBottom + 1) * sqH;
                    float left = (current.X - viewLeft) * sqW;
                    float right = (current.X - viewLeft + 1) * sqW;

                    SetRectangleVertices(vertices, bottom, top, left, right, -10, coord);
                    SetAtlasCoordinates(texAtlas, atlasCoords, bottomLeftInd, 6);
                }
            }

            map.BindData(gl, 3, vertices, 4, texAtlas);
        }

        #endregion Map

        #region Nutrients map
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateNutrientsMap(OpenGL gl)
        {
            nutrientsMap = new MyBufferArray(gl, CreateSquareUVCoordinates);
        }

        /// <summary>
        /// Clears all buffers representing nutrients map.
        /// </summary>
        public static void TryClearNutrientsMapDataBuffers(OpenGL gl)
        {
            if (!nutrientsMap.Clear)
                nutrientsMap.ClearBuffers(gl);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateNutrientsMapDataBuffers(OpenGL gl, MapView mapView, Game game)
        {
            //prepare data
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewBottom = mapView.Bottom;

            Node[,] visible = mapView.GetVisibleNodes(game.CurrentPlayer.VisibleMap);
            int width = visible.GetLength(0);
            int height = visible.GetLength(1);

            // extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;

            int numberOfChars = 6;
            int vertexCount = width * height * 6 * numberOfChars;
            int verticesSize = vertexCount * 3;
            int uvSize = vertexCount * 2;
            int textureAtlasSize = vertexCount * 4;

            nutrientsMap.InitializeArrays(gl, verticesSize, uvSize, textureAtlasSize);
            float[] vertices = nutrientsMap.vertices;
            float[] texAtlas = nutrientsMap.texAtlas;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //buffer indices
                    int coord = (i + j * width) * 6 * 3;
                    int bottomLeftInd = (i + j * width) * 6 * 4;

                    Node current = visible[i, j];
                    if (current == null)
                        continue;

                    //active nutrients
                    int activeNutrients = (int)(current.ActiveNutrients * 10);
                    Rect atlasCoordsRight = ImageAtlas.GetImageAtlas.GetNumberedTriangle(activeNutrients);

                    //passive nutrients
                    int passiveNutrients = (int)(current.PassiveNutrients);
                    Rect atlasCoordsLeft = ImageAtlas.GetImageAtlas.GetNumberedTriangle(passiveNutrients);

                    //tile position
                    float bottom = (current.Y - viewBottom) * sqH;
                    float top = (current.Y - viewBottom + 1) * sqH;
                    float left = (current.X - viewLeft) * sqW;
                    float right = (current.X - viewLeft + 1) * sqW;

                    SetRectangleVertices(vertices, bottom, top, left, right, -10, coord);
                    SetAtlasCoordinates(texAtlas, atlasCoordsLeft, bottomLeftInd , 3);
                    SetAtlasCoordinates(texAtlas, atlasCoordsRight, bottomLeftInd + 4 * 3, 3);
                }
            }

            nutrientsMap.BindData(gl, 3, vertices, 4, texAtlas);
        }

        #endregion Nutrients map
        
        #region Entity circles
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateUnitCircles(OpenGL gl)
        {
            entityCircles = new MyBufferArray(gl, CreateSquareUVCoordinates);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateEntityCirclesDataBuffers(OpenGL gl, MapView mapView, Game game)
        {
            //prepare data
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewBottom = mapView.Bottom;

            List<Entity> visEntity = mapView.GetVisibleEntities(game, game.CurrentPlayer);

            int size = visEntity.Count;
            if (size == 0)
            {
                entitiesEmpty = true;
                return;
            }
            else
            {
                entitiesEmpty = false;
            }

            int verticesPerOne = size * 6;
            int verticesSize = verticesPerOne * 3;
            int uvSize = verticesPerOne * 2;
            int textureAtlasSize = verticesPerOne * 4;

            entityCircles.InitializeArrays(gl, verticesSize, uvSize, textureAtlasSize);
            float[] vertices = entityCircles.vertices;
            float[] texAtlas = entityCircles.texAtlas;

            for (int i=0;i<visEntity.Count;i++)
            {                
                Entity current = visEntity[i];
                //buffer indices
                int index = i * 6 * 3;
                int atlasInd = i * 6 * 4;

                if (current == null)
                    continue;

                float entitySize = nodeSize;

                //entity circle
                {
                    //image position
                    float bottom = (current.Bottom - viewBottom) * entitySize;
                    float top = (current.Top - viewBottom) * entitySize;
                    float left = (current.Left - viewLeft) * entitySize;
                    float right = (current.Right - viewLeft) * entitySize;

                    //vertices
                    SetRectangleVertices(vertices, bottom, top, left, right, -8, index);

                    Rect atlasCoords;
                    //colors
                    if (!current.Selected)
                    {
                        //fill the circle with color of the corresponding player
                        switch (current.Faction.FactionID)
                        {
                            case FactionType.PLAYER0:
                                atlasCoords= ImageAtlas.GetImageAtlas.UnitCircleBlue;
                                break;
                            case FactionType.PLAYER1:
                                atlasCoords = ImageAtlas.GetImageAtlas.UnitCircleRed;
                                break;
                            case FactionType.NEUTRAL:
                                atlasCoords = ImageAtlas.GetImageAtlas.UnitCircleGray;
                                break;
                            default:
                                atlasCoords = ImageAtlas.GetImageAtlas.UnitCircleGray;
                                break;
                        }
                    }
                    else
                    {
                        //fill the circle with selected entity circle color
                        atlasCoords = ImageAtlas.GetImageAtlas.UnitCircleYellow;
                    }

                    //atlas coordinates
                    SetAtlasCoordinates(texAtlas, atlasCoords, atlasInd, 6);
                }
            }

            entityCircles.BindData(gl, 3, vertices, 4, texAtlas);
        }
        #endregion Entity circles

        #region Entity
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateEntities(OpenGL gl)
        {
            entities = new MyBufferArray(gl, CreateSquareUVCoordinates);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateEntitiesDataBuffers(OpenGL gl, MapView mapView, Game game)
        {
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewTop = mapView.Top;
            float viewBottom = mapView.Bottom;
            float viewRight = mapView.Right;

            List<Entity> visEntities = mapView.GetVisibleEntities(game, game.CurrentPlayer);

            int size = visEntities.Count;
            if (size == 0)
            {
                entitiesEmpty = true;
                return;
            }
            else
            {
                entitiesEmpty = false;
            }
            
            int verticesPerOne = size * 6;
            int verticesSize = verticesPerOne * 3;
            int textureSize = verticesPerOne * 2;
            int textureAtlasSize = verticesPerOne * 4;

            entities.InitializeArrays(gl, verticesSize, textureSize, textureAtlasSize);
            float[] vertices = entities.vertices;
            float[] uv = entities.uv;
            float[] texAtlas = entities.texAtlas;

            //visible units have to be sorted to draw them properly
            visEntities.Sort((u, v) => Math.Sign(v.Center.Y - u.Center.Y));

            for (int i = 0; i < visEntities.Count; i++)
            {
                Entity current = visEntities[i];
                //buffer indices
                int index = i * 6 * 3;
                int texIndex = i * 6 * 2;
                int atlasInd = i * 6 * 4;

                if (current == null)
                    continue;

                float entitySize = nodeSize;
                
                //entity image
                {
                    Animation anim = current.AnimationState.Animation;

                    //position of the center of the image
                    Vector2 imageCenter = anim.LeftBottom;
                    if (current is Animal && !((Animal)current).FacingLeft)
                        imageCenter = new Vector2(anim.Width - anim.LeftBottom.X, anim.LeftBottom.Y);

                    //image position
                    float bottom = (current.Center.Y - imageCenter.Y - viewBottom) * entitySize;
                    float top = (current.Center.Y - imageCenter.Y - viewBottom + anim.Height) * entitySize;
                    float left = (current.Center.X - imageCenter.X - viewLeft) * entitySize;
                    float right = (current.Center.X - imageCenter.X - viewLeft + anim.Width) * entitySize;

                    //depth is from [4,5]
                    float depth = 4f + current.Center.Y / game.Map.Height;

                    //vertices
                    SetRectangleVertices(vertices, bottom, top, left, right, -depth, index);

                    //uv coordinates
                    if(current is Animal && !((Animal)current).FacingLeft)
                        SetHorizFlipSquareTextureCoordinates(uv, texIndex);
                    else
                       SetSquareTextureCoordinates(uv, texIndex);

                    //atlas coordinates
                    Rect entityImage = current.AnimationState.CurrentImage;
                    SetAtlasCoordinates(texAtlas, entityImage, atlasInd, 6);
                }
            }

            entities.BindData(gl, 3, vertices, 2, uv, 4, texAtlas);
        }
        #endregion Entity

        #region Entity indicators
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateEntitiesIndicators(OpenGL gl)
        {
            entityIndicators = new MyBufferArray(gl, CreateSquareUVCoordinates);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateEntityIndicatorsDataBuffers(OpenGL gl, MapView mapView,
                                                            Player observer, Game game)
        {
            //prepare data
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewTop = mapView.Top;
            float viewBottom = mapView.Bottom;
            float viewRight = mapView.Right;

            //only show indicators for entities that player can directly see = are in visible area of map
            List<Entity> visEntities = game.GetAll<Entity>().Where((e) => observer.CanSee(e))
                .Where(e=>e.Health!=e.Health.MaxValue || e.Energy !=e.Energy.MaxValue).ToList();

            int size = visEntities.Count;
            if (size == 0)
            {
                entityIndicatorsEmpty = true;
                return;
            }
            else
            {
                entityIndicatorsEmpty = false;
            }
            
            int verticesPerOne = size * 24;
            int verticesSize = verticesPerOne * 3;
            int textureSize = verticesPerOne * 2;
            int textureAtlasSize = verticesPerOne * 4;

            entityIndicators.InitializeArrays(gl, verticesSize, textureSize, textureAtlasSize);
            float[] vertices = entityIndicators.vertices;
            float[] uv = entityIndicators.uv;
            float[] texAtlas = entityIndicators.texAtlas;

            //visible units have to be sorted to draw the indicators properly
            visEntities.Sort((u, v) => Math.Sign(v.Center.Y - u.Center.Y));

            for (int i = 0; i < visEntities.Count; i++)
            {
                Entity current = visEntities[i];
                //buffer indices
                int index = i * 24 * 3;
                int atlasInd = i * 24 * 4;

                if (current == null)
                    continue;

                float unitSize = nodeSize;

                float indicatorWidth = current.Range * 1.5f;
                float indicatorHeight = 0.15f;
                {
                    Animation anim = current.AnimationState.Animation;

                    //rectangle
                    float bottom = (current.Center.Y - anim.LeftBottom.Y - viewBottom + anim.Height) * unitSize;
                    float top = (current.Center.Y - anim.LeftBottom.Y - viewBottom + anim.Height + indicatorHeight) * unitSize;
                    float left = (current.Center.X - indicatorWidth/2f - viewLeft) * unitSize;
                    float right = (current.Center.X + indicatorWidth/2f - viewLeft) * unitSize;

                    //depth is from [2,3]
                    float depth = 2f + current.Center.Y / game.Map.Height;

                    Rect blackRect = ImageAtlas.GetImageAtlas.BlackSquare;
                    Rect redRect = ImageAtlas.GetImageAtlas.RedSquare;
                    Rect greenRect = ImageAtlas.GetImageAtlas.GreenSquare;
                    //energy
                    AddRectangle(left, bottom, right, top, blackRect, depth, index, vertices,
                         atlasInd, texAtlas);
                    index += 6 * 3;
                    atlasInd += 6 * 4;
                    float energyRight = Math.Max(left, left + (right - left) * (float)(current.Energy.Percentage));
                    AddRectangle(left, bottom, energyRight, top, greenRect, depth, index, vertices,
                         atlasInd, texAtlas);
                    index += 6 * 3;
                    atlasInd += 6 * 4;

                    //health
                    bottom += indicatorHeight*unitSize;
                    top += indicatorHeight*unitSize;
                    AddRectangle(left, bottom, right, top, blackRect, depth, index, vertices,
                         atlasInd, texAtlas);
                    index += 6 * 3;
                    atlasInd += 6 * 4;
                    float healthRight =Math.Max(left, left + (right - left) * (float)(current.Health.Percentage));
                    AddRectangle(left, bottom, healthRight, top, redRect, depth, index, vertices,
                         atlasInd, texAtlas);
                }
            }

            entityIndicators.BindData(gl, 3, vertices, 4, texAtlas);
        }

        #endregion Entity indicators

        #region Flowfield

        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateFlowField(OpenGL gl)
        {
            flowField = new MyBufferArray(gl, CreateSquareUVCoordinates);
        }

        /// <summary>
        /// Clears all buffers representing flow map.
        /// </summary>
        public static void TryClearFlowFieldDataBuffers(OpenGL gl)
        {
            if (!flowField.Clear)
                flowField.ClearBuffers(gl);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the flow map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateFlowFieldDataBuffers(OpenGL gl, MapView mapView, FlowField flowfield)
        {
            //prepare data
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewBottom = mapView.Bottom;
            float?[,] flowF = mapView.GetVisibleFlowField(flowfield);
            int width = flowF.GetLength(0);
            int height = flowF.GetLength(1);

            //extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;
            
            int verticesPerOne = width * height * 3;
            int verticesSize = verticesPerOne * 3;
            int uvSize = verticesPerOne * 2;
            int textureAtlasSize = verticesPerOne * 4;

            flowField.InitializeArrays(gl, verticesSize, uvSize, textureAtlasSize);
            float[] vertices = flowField.vertices;
            float[] texAtlas = flowField.texAtlas;

            //triangle
            vec2 triLB = new vec2(-0.25f, -0.15f);
            vec2 triLT = new vec2(-0.25f, 0.15f);
            vec2 triRM = new vec2(0.25f, 0f);
            vec2 triOffset = new vec2(0.5f, 0.5f);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //buffer indices
                    int coord = (i + j * width) * 3 * 3;
                    int bottomLeftInd = (i + j * width) * 3 * 4;


                    vec2 leftBottom =new vec2((i - (viewLeft % 1)), (j - (viewBottom % 1)));
                    vec2 squareExt = new vec2(sqW, sqH);


                    int offset = 0;
                    if (flowF[i, j] == null)
                    {
                        vertices[coord + 0] = vertices[coord + 1] = vertices[coord + 2]
                            = vertices[coord + 3] = vertices[coord + 4] = vertices[coord + 5] = 0;
                    }
                    else
                    {
                        float angle = flowF[i, j].Value;

                        vec2 rotTriLB;
                        vec2 rotTriLT;
                        vec2 rotTriRM;
                        if (FlowField.PointToTarget(angle))
                        {
                            //triangle that represents that the animal should move directly to the target
                            rotTriLB = (new vec2(-0.2f, 0.15f) + triOffset + leftBottom) * squareExt;
                            rotTriLT = (new vec2(0.2f, 0.15f) + triOffset + leftBottom) * squareExt;
                            rotTriRM = (new vec2(0, -0.20f) + triOffset + leftBottom) * squareExt;
                        }
                        else
                        {
                            //rotated triangle coordinates
                            rotTriLB = (Rotate(triLB, angle) + triOffset + leftBottom) * squareExt;
                            rotTriLT = (Rotate(triLT, angle) + triOffset + leftBottom) * squareExt;
                            rotTriRM = (Rotate(triRM, angle) + triOffset + leftBottom) * squareExt;
                        }
                        //bottom left
                        vertices[coord + offset + 0] = rotTriLB.x;
                        vertices[coord + offset + 1] = rotTriLB.y;
                        vertices[coord + offset + 2] = -9;

                        offset += 3;

                        //top left
                        vertices[coord + offset + 0] = rotTriLT.x;
                        vertices[coord + offset + 1] = rotTriLT.y;
                        vertices[coord + offset + 2] = -9;

                        offset += 3;

                        //top right
                        vertices[coord + offset + 0] = rotTriRM.x;
                        vertices[coord + offset + 1] = rotTriRM.y;
                        vertices[coord + offset + 2] = -9;
                    }
                    
                    Rect atlasCoords = ImageAtlas.GetImageAtlas.BlackSquare;//the triangles are black
                    SetAtlasCoordinates(texAtlas, atlasCoords, bottomLeftInd, 3);
                }
            }

            flowField.BindData(gl, 3, vertices, 4, texAtlas);
        }

        #endregion Flowfield
        
        #region Selection frame
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateSelectionFrame(OpenGL gl)
        {
            selectionFrame = new MyBufferArray(gl, CreateSquareUVCoordinates);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the flow map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateSelectionFrameDataBuffers(OpenGL gl, MapView mapView, MapSelectorFrame selectorFrame)
        {
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewBottom = mapView.Bottom;

            float[] vertices = new float[6 * 3];
            float[] texBottomLeft = new float[6 * 4];
            
            //draw selector frame only if it exists
            if (selectorFrame != null)
            {
                float bottom = selectorFrame.Bottom - viewBottom;
                float top = selectorFrame.Top - viewBottom;
                float left = selectorFrame.Left - viewLeft;
                float right = selectorFrame.Right - viewLeft;
                SetRectangleVertices(vertices, bottom * nodeSize, top * nodeSize,
                    left * nodeSize, right * nodeSize, -1f, 0);
                Rect atlasCoords = ImageAtlas.GetImageAtlas.UnitsSelector;
                SetAtlasCoordinates(texBottomLeft, atlasCoords, 0, 6);
            }

            selectionFrame.BindData(gl, 3, vertices, 4, texBottomLeft);
        }
        #endregion Selection frame

        #region Array writing methods
        /// <summary>
        /// Write rectangle vertices with the given parameters to the index of the array. The rectangle
        /// is made of two triangles.
        /// </summary>
        /// <param name="vertices">Writing array.</param>
        /// <param name="bottom">Bottom of the rectangle.</param>
        /// <param name="top">Top of the rectangle.</param>
        /// <param name="left">Left of the rectangle.</param>
        /// <param name="right">Right of the rectangle.</param>
        /// <param name="depth">Depth of the vertices.</param>
        /// <param name="index">Index of the first vertex in array.</param>
        private static void SetRectangleVertices(float[] vertices, float bottom, float top,
                                                float left, float right, float depth, int index)
        {
            int offset = 0;

            //bottom left
            vertices[index + offset + 0] = left;
            vertices[index + offset + 1] = bottom;
            vertices[index + offset + 2] = depth;

            offset += 3;

            //top left
            vertices[index + offset + 0] = left;
            vertices[index + offset + 1] = top;
            vertices[index + offset + 2] = depth;

            offset += 3;

            //top right
            vertices[index + offset + 0] = right;
            vertices[index + offset + 1] = top;
            vertices[index + offset + 2] = depth;

            offset += 3;

            //bottom right
            vertices[index + offset + 0] = right;
            vertices[index + offset + 1] = bottom;
            vertices[index + offset + 2] = depth;

            offset += 3;

            //bottom left
            vertices[index + offset + 0] = left;
            vertices[index + offset + 1] = bottom;
            vertices[index + offset + 2] = depth;

            offset += 3;

            //top right
            vertices[index + offset + 0] = right;
            vertices[index + offset + 1] = top;
            vertices[index + offset + 2] = depth;
        }


        /// <summary>
        /// Write texture UV coordinates for a square made of two triangles.
        /// </summary>
        /// <param name="textureCoords">Writing array.</param>
        /// <param name="index">Index of the first UV coordinate in array.</param>
        private static float[] CreateSquareUVCoordinates(int size)
        {
            float[] uv = new float[size];
            for(int i = 0; i < size/12; i++)
            {
                int index = i * 12;
                SetSquareTextureCoordinates(uv, index);
            }
            return uv;
        }

        /// <summary>
        /// Write texture UV coordinates for a square made of two triangles.
        /// </summary>
        /// <param name="textureCoords">Writing array.</param>
        /// <param name="index">Index of the first UV coordinate in array.</param>
        private static void SetSquareTextureCoordinates(float[] textureCoords, int index)
        {
            int offset = 0;

            //bottom left
            textureCoords[index + offset + 0] = 0;
            textureCoords[index + offset + 1] = 0;

            offset += 2;

            //top left
            textureCoords[index + offset + 0] = 0;
            textureCoords[index + offset + 1] = 1;

            offset += 2;

            //top right
            textureCoords[index + offset + 0] = 1;
            textureCoords[index + offset + 1] = 1;

            offset += 2;

            //bottom right
            textureCoords[index + offset + 0] = 1;
            textureCoords[index + offset + 1] = 0;

            offset += 2;

            //bottom left
            textureCoords[index + offset + 0] = 0;
            textureCoords[index + offset + 1] = 0;

            offset += 2;

            //top right
            textureCoords[index + offset + 0] = 1;
            textureCoords[index + offset + 1] = 1;
        }

        /// <summary>
        /// Write texture UV coordinates for a square made of two triangles. The texture is horizontaly flipped.
        /// </summary>
        /// <param name="textureCoords">Writing array.</param>
        /// <param name="index">Index of the first UV coordinate in array.</param>
        private static void SetHorizFlipSquareTextureCoordinates(float[] textureCoords, int index)
        {
            int offset = 0;

            //bottom left
            textureCoords[index + offset + 0] = 1;
            textureCoords[index + offset + 1] = 0;

            offset += 2;

            //top left
            textureCoords[index + offset + 0] = 1;
            textureCoords[index + offset + 1] = 1;

            offset += 2;

            //top right
            textureCoords[index + offset + 0] = 0;
            textureCoords[index + offset + 1] = 1;

            offset += 2;

            //bottom right
            textureCoords[index + offset + 0] = 0;
            textureCoords[index + offset + 1] = 0;

            offset += 2;

            //bottom left
            textureCoords[index + offset + 0] = 1;
            textureCoords[index + offset + 1] = 0;

            offset += 2;

            //top right
            textureCoords[index + offset + 0] = 0;
            textureCoords[index + offset + 1] = 1;
        }

        /// <summary>
        /// Write texture atlas coordinates to the array.
        /// </summary>
        /// <param name="texBotLeftWidthHeight">Writing array.</param>
        /// <param name="imageCoords">Coordinates of the image in the atlas with format: Rect(x, y, withd, height).</param>
        /// <param name="index">Index of the first coordinate in array.</param>
        /// <param name="vertCount">Number of times this coordinates should be repeated (for each vertex once).</param>
        private static void SetAtlasCoordinates(float[] texBotLeftWidthHeight, Rect imageCoords, int index,
            int vertCount)
        {
            int offset = 0;

            for (int i = 0; i < vertCount; i++)
            {
                texBotLeftWidthHeight[index + offset + 0] = imageCoords.Left;
                texBotLeftWidthHeight[index + offset + 1] = imageCoords.Bottom;
                texBotLeftWidthHeight[index + offset + 2] = imageCoords.Width;
                texBotLeftWidthHeight[index + offset + 3] = imageCoords.Height;

                offset += 4;
            }
        }


        /// <summary>
        /// Adds a rectangle with the parameters to the arrays.
        /// </summary>
        public static void AddRectangle(float left, float bottom, float right, float top, Rect image, float depth,
                                            int vInd, float[] vertices,
                                            int aInd, float[] texAtlas)
        {
            //vertices
            SetRectangleVertices(vertices, bottom, top, left, right, -depth, vInd);

            //atlas coordinates
            SetAtlasCoordinates(texAtlas, image, aInd, 6);
        }
        #endregion Array writing methods

        /// <summary>
        /// Rotates the vector by the angle.
        /// </summary>
        private static vec2 Rotate(vec2 vec, float angle)
        {
            float cosA = (float)Math.Cos(angle);
            float sinA = (float)Math.Sin(angle);
            return new vec2(cosA * vec.x - sinA * vec.y,
                            sinA * vec.x + cosA * vec.y);
        }
    }
}
