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

namespace SanguineGenesis
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
        const uint attributeIndexcolor = 1;
        const uint attributeIndexTexCoord = 2;
        const uint attributeIndexTexBL = 3;
        
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
            shaderProgram.BindAttributeLocation(gl, attributeIndexcolor, "in_Color");
            shaderProgram.BindAttributeLocation(gl, attributeIndexTexCoord, "in_TexCoord");
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
        private static void LoadTexture(String fileName, OpenGL gl, ShaderProgram shaderProgram)
        {
            //load image and flip it vertically
            Bitmap textureImage = new Bitmap(fileName);
            Bitmap test = (Bitmap)textureImage.Clone();
            textureImage.RotateFlip(RotateFlipType.RotateNoneFlipY);


            //generate id for the texture and then bind the image with this id
            uint[] textureIds = new uint[2];
            gl.GenTextures(2, textureIds);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureIds[0]);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA, test.Width, test.Height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE,
                test.LockBits(new Rectangle(0, 0, test.Width, test.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb).Scan0);

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureIds[1]);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA, textureImage.Width, textureImage.Height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE,
                textureImage.LockBits(new Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb).Scan0);

            shaderProgram.Bind(gl);

            int atlas0 = shaderProgram.GetUniformLocation(gl, "atlas0");
            int atlas1 = shaderProgram.GetUniformLocation(gl, "atlas1");
            gl.Uniform1(atlas0, 0);
            gl.Uniform1(atlas1, 1);

            gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureIds[0]);
            gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureIds[1]);

            /*shaderProgram.SetUniform1(gl, "atlas0", textureIds[0]);
            shaderProgram.SetUniform1(gl, "atlas1", textureIds[1]);*/


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
            //if (gameplayOptions.NutrientsVisible)
            {
                nutrientsMap.VertexBufferArray.Bind(gl);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, nutrientsMap.VertexCount);
            }

            //draw flowfield
            //if (gameplayOptions.ShowFlowfield)
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
            public VertexBuffer ColorDataBuffer { get; set; }
            public VertexBuffer TextureDataBuffer { get; set; }
            public VertexBuffer TexAtlasDataBuffer { get; set; }

            public float[] vertices;
            public float[] colors;
            public float[] texture;
            public float[] texAtlas;

            /// <summary>
            /// True if the buffers contains no data.
            /// </summary>
            public bool Clear { get; private set; }

            public MyBufferArray(OpenGL gl)
            {
                //initialize VertexBufferArray and link it to its VertexBuffers
                VertexBufferArray = new VertexBufferArray();
                VertexBufferArray.Create(gl);
                VertexBufferArray.Bind(gl);

                //initialize empty VertexDataBuffer
                VertexDataBuffer = new VertexBuffer();
                VertexDataBuffer.Create(gl);

                //initialize empty ColorDataBuffer
                ColorDataBuffer = new VertexBuffer();
                ColorDataBuffer.Create(gl);

                //initialize empty TextureDataBuffer
                TextureDataBuffer = new VertexBuffer();
                TextureDataBuffer.Create(gl);

                //initialize empty TexBLDataBuffer
                TexAtlasDataBuffer = new VertexBuffer();
                TexAtlasDataBuffer.Create(gl);

                VertexBufferArray.Unbind(gl);

                //create initial arrays so that they are not null
                vertices = new float[0];
                colors = new float[0];
                texture = new float[0];
                texAtlas = new float[0];
            }

            /// <summary>
            /// Binds the data to this vertex buffer array.
            /// </summary>
            public void BindData(OpenGL gl, int vStride, float[] vValues,
                                            int cStride, float[] cValues,
                                            int tStride, float[] tValues,
                                            int aStride, float[] aValues)
            {
                VertexBufferArray.Bind(gl);

                VertexDataBuffer.Bind(gl);
                VertexDataBuffer.SetData(gl, attributeIndexPosition, vValues, false, vStride);
                //gl.BufferData(OpenGL.GL_ARRAY_BUFFER, vValues, OpenGL.GL_STREAM_DRAW);
                VertexDataBuffer.Unbind(gl);

                ColorDataBuffer.Bind(gl);
                ColorDataBuffer.SetData(gl, attributeIndexcolor, cValues, false, cStride);

                TextureDataBuffer.Bind(gl);
                TextureDataBuffer.SetData(gl, attributeIndexTexCoord, tValues, false, tStride);

                TexAtlasDataBuffer.Bind(gl);
                TexAtlasDataBuffer.SetData(gl, attributeIndexTexBL, aValues, false, aStride);

                VertexBufferArray.Unbind(gl);

                Clear = false;
            }

            /// <summary>
            /// Initialize arrays with zeros with the given sizes.
            /// </summary>
            public void InitializeArrays(int verticesSize, int colorsSize, int textureSize, int texAtlasSize)
            {
                InitArray(ref vertices, verticesSize);
                InitArray(ref colors, colorsSize);
                InitArray(ref texture, textureSize);
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

            public int VertexCount => vertices == null ? 0 : vertices.Length / 3;

            public void ClearBuffers(OpenGL gl)
            {
                BindData(gl, 1, new float[1],
                            1, new float[1],
                            1, new float[1],
                            1, new float[1]);
                Clear = true;
            }
        }

        //vertex buffer arrays which contain the buffers for vertex, 
        //color, texture and bottom left coordinates of textures
        private static MyBufferArray map;
        private static MyBufferArray flowField;
        private static MyBufferArray nutrientsMap;
        private static MyBufferArray entityCircles;
        private static MyBufferArray entities;
        private static MyBufferArray entityIndicators;
        private static MyBufferArray selectionFrame;

        //true if there are no units to draw
        private static bool entitiesEmpty;

        #region Map
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateMap(OpenGL gl)
        {
            map = new MyBufferArray(gl);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateMapDataBuffers(OpenGL gl, MapView mapView, Game game)
        {
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewTop = mapView.Top;
            float viewBottom = mapView.Bottom;
            float viewRight = mapView.Right;

            bool[,] visibleVisibility = mapView.GetVisibleVisibilityMap(game.Players[game.CurrentPlayer.PlayerID].VisibilityMap);
            Node[,] visible = mapView.GetVisibleNodes(game.Players[game.CurrentPlayer.PlayerID].VisibleMap);
            int width = visible.GetLength(0);
            int height = visible.GetLength(1);

            //extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;

            int verticesPerOne = width * height * 6;
            int verticesSize = verticesPerOne * 3;
            int colorsSize = verticesPerOne * 3;
            int textureSize = verticesPerOne * 2;
            int textureAtlasSize = verticesPerOne * 4;
            
            map.InitializeArrays(verticesSize, colorsSize, textureSize, textureAtlasSize);
            float[] vertices = map.vertices;
            float[] colors = map.colors;
            float[] texture = map.texture;
            float[] texAtlas = map.texAtlas;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //buffer indices
                    int coord = (i + j * width) * 6 * 3;
                    int texCoord = (i + j * width) * 6 * 2;
                    int bottomLeftInd = (i + j * width) * 6 * 4;

                    Node current = visible[i, j];
                    if (current == null)
                        continue;

                    bool isVisible = true;
                    if (visibleVisibility != null)
                        isVisible = visibleVisibility[i, j];
                    
                    Rect atlasCoords = ImageAtlas.GetImageAtlas.GetTileCoords(current.Biome,current.SoilQuality,current.Terrain);

                    //tile position
                    float bottom = (current.Y - viewBottom) * sqH;
                    float top = (current.Y - viewBottom + 1) * sqH;
                    float left = (current.X - viewLeft) * sqW;
                    float right = (current.X - viewLeft + 1) * sqW;

                    SetRectangleVertices(vertices, bottom, top, left, right, -10, coord);
                    if(isVisible)
                        SetColor(colors, 1f, 1f, 1f, coord,6);
                    else
                        SetColor(colors, .8f, .8f, .8f, coord, 6);
                    SetSquareTextureCoordinates(texture, texCoord);
                    SetAtlasCoordinates(texAtlas, atlasCoords, bottomLeftInd, 6);
                }
            }

            map.BindData(gl, 3, vertices, 3, colors, 2, texture, 4, texAtlas);
        }

        #endregion Map

        #region Nutrients map
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateNutrientsMap(OpenGL gl)
        {
            nutrientsMap = new MyBufferArray(gl);
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
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewTop = mapView.Top;
            float viewBottom = mapView.Bottom;
            float viewRight = mapView.Right;

            bool[,] visibleVisibility = mapView.GetVisibleVisibilityMap(game.Players[game.CurrentPlayer.PlayerID].VisibilityMap);
            Node[,] visible = mapView.GetVisibleNodes(game.CurrentPlayer.VisibleMap);
            int width = visible.GetLength(0);
            int height = visible.GetLength(1);

            //extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;

            int verticesPerOne = width * height * 6 * 3;
            int verticesSize = verticesPerOne * 3;
            int colorsSize = verticesPerOne * 3;
            int textureSize = verticesPerOne * 2;
            int textureAtlasSize = verticesPerOne * 4;

            nutrientsMap.InitializeArrays(verticesSize, colorsSize, textureSize, textureAtlasSize);
            float[] vertices = nutrientsMap.vertices;
            float[] colors = nutrientsMap.colors;
            float[] texture = nutrientsMap.texture;
            float[] texAtlas = nutrientsMap.texAtlas;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //buffer indices
                    int coord = (i + j * width) * 6 * 3 * 3;
                    int texCoord = (i + j * width) * 6 * 2 * 3;
                    int bottomLeftInd = (i + j * width) * 6 * 4 * 3;

                    Node current = visible[i, j];
                    if (current == null)
                        continue;

                    //find digits and their glyphs
                    int leftDig = (int)current.Nutrients;
                    int rightDig = (int)((current.Nutrients - leftDig) * 10);//(int)((((1000*current.Nutrients))%1000)/100);
                    Rect leftDigAtlasCoords = ImageAtlas.GetImageAtlas.GetGlyph(leftDig);
                    Rect rightDigAtlasCoords = ImageAtlas.GetImageAtlas.GetGlyph(rightDig);
                    Rect decPointAtlasCoords = ImageAtlas.GetImageAtlas.GetGlyph(-1);

                    //numbers y position
                    float bottom = (current.Y - viewBottom + 0.25f) * sqH;
                    float top = (current.Y - viewBottom + 0.75f) * sqH;
                    
                    //left digit position
                    float left = (current.X - viewLeft + 0.2f) * sqW;
                    float right = (current.X - viewLeft + 0.45f) * sqW;

                    SetRectangleVertices(vertices, bottom, top, left, right, -10, coord);
                    SetColor(colors, 1f, 1f, 1f, coord, 6);
                    SetSquareTextureCoordinates(texture, texCoord);
                    SetAtlasCoordinates(texAtlas, leftDigAtlasCoords, bottomLeftInd, 6);

                    coord += 6 * 3;
                    texCoord +=  6 * 2;
                    bottomLeftInd += 6 * 4;

                    //decimal point position
                    left = (current.X - viewLeft + 0.4f) * sqW;
                    right = (current.X - viewLeft + 0.6f) * sqW;

                    SetRectangleVertices(vertices, bottom, top, left, right, -10, coord);
                    SetColor(colors, 1f, 1f, 1f, coord, 6);
                    SetSquareTextureCoordinates(texture, texCoord);
                    SetAtlasCoordinates(texAtlas, decPointAtlasCoords, bottomLeftInd, 6);

                    coord += 6 * 3;
                    texCoord += 6 * 2;
                    bottomLeftInd += 6 * 4;
                    
                    //left digit position
                    left = (current.X - viewLeft + 0.55f) * sqW;
                    right = (current.X - viewLeft + 0.8f) * sqW;

                    SetRectangleVertices(vertices, bottom, top, left, right, -10, coord);
                    SetColor(colors, 1f, 1f, 1f, coord, 6);
                    SetSquareTextureCoordinates(texture, texCoord);
                    SetAtlasCoordinates(texAtlas, rightDigAtlasCoords, bottomLeftInd, 6);
                }
            }

            nutrientsMap.BindData(gl, 3, vertices, 3, colors, 2, texture, 4, texAtlas);
        }

        #endregion Nutrients map
        
        #region Entity circles
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateUnitCircles(OpenGL gl)
        {
            entityCircles = new MyBufferArray(gl);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateEntityCirclesDataBuffers(OpenGL gl, MapView mapView, Game game)
        {
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewTop = mapView.Top;
            float viewBottom = mapView.Bottom;
            float viewRight = mapView.Right;

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
            int colorsSize = verticesPerOne * 3;
            int textureSize = verticesPerOne * 2;
            int textureAtlasSize = verticesPerOne * 4;

            entityCircles.InitializeArrays(verticesSize, colorsSize, textureSize, textureAtlasSize);
            float[] vertices = entityCircles.vertices;
            float[] colors = entityCircles.colors;
            float[] texture = entityCircles.texture;
            float[] texAtlas = entityCircles.texAtlas;

            for (int i=0;i<visEntity.Count;i++)
            {                
                Entity current = visEntity[i];
                //buffer indices
                int index = i * 6 * 3;
                int texIndex = i * 6 * 2;
                int atlasInd = i * 6 * 4;

                if (current == null)
                    continue;

                float entitySize = nodeSize;

                //entity circle
                {
                    //tile position
                    float bottom = (current.Bottom - viewBottom) * entitySize;
                    float top = (current.Top - viewBottom) * entitySize;
                    float left = (current.Left - viewLeft) * entitySize;
                    float right = (current.Right - viewLeft) * entitySize;

                    //vertices
                    SetRectangleVertices(vertices, bottom, top, left, right, -8, index);

                    //colors
                    if (!current.Selected)
                    {
                        //fill the circle with color of the corresponding player
                        switch (current.Player.PlayerID)
                        {
                            case Players.PLAYER0:
                                SetColor(colors, 0f, 0f, 1f, index, 6);
                                break;
                            case Players.PLAYER1:
                                SetColor(colors, 1f, 0f, 0f, index, 6);
                                break;
                        }
                    }
                    else
                        //fill the circle with selected entity circle color
                        SetColor(colors, 1f, 1f, 0f, index, 6);

                    //texture coordinates
                    SetSquareTextureCoordinates(texture, texIndex);

                    //atlas coordinates
                    Rect atlasCoords = ImageAtlas.GetImageAtlas.UnitCircle;
                    SetAtlasCoordinates(texAtlas, atlasCoords, atlasInd, 6);
                }
            }

            entityCircles.BindData(gl, 3, vertices, 3, colors, 2, texture, 4, texAtlas);
        }
        #endregion Entity circles

        #region Entity
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateEntities(OpenGL gl)
        {
            entities = new MyBufferArray(gl);
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
            int colorsSize = verticesPerOne * 3;
            int textureSize = verticesPerOne * 2;
            int textureAtlasSize = verticesPerOne * 4;

            entities.InitializeArrays(verticesSize, colorsSize, textureSize, textureAtlasSize);
            float[] vertices = entities.vertices;
            float[] colors = entities.colors;
            float[] texture = entities.texture;
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

                    //tile position
                    float bottom = (current.Center.Y - imageCenter.Y - viewBottom) * entitySize;
                    float top = (current.Center.Y - imageCenter.Y - viewBottom + anim.Height) * entitySize;
                    float left = (current.Center.X - imageCenter.X - viewLeft) * entitySize;
                    float right = (current.Center.X - imageCenter.X - viewLeft + anim.Width) * entitySize;

                    //depth is from [4,5]
                    float depth = 4f + current.Center.Y / game.Map.Height;
                    //vertices
                    SetRectangleVertices(vertices, bottom, top, left, right, -depth, index);

                    //colors
                    SetColor(colors, 1f, 1f, 1f, index, 6);

                    //texture coordinates
                    if(current is Animal && !((Animal)current).FacingLeft)
                        SetHorizFlipSquareTextureCoordinates(texture, texIndex);
                    else
                        SetSquareTextureCoordinates(texture, texIndex);

                    //atlas coordinates
                    Rect entityImage = current.AnimationState.CurrentImage;
                    SetAtlasCoordinates(texAtlas, entityImage, atlasInd, 6);
                }
            }

            entities.BindData(gl, 3, vertices, 3, colors, 2, texture, 4, texAtlas);
        }
        #endregion Entity

        #region Entity indicators
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateEntitiesIndicators(OpenGL gl)
        {
            entityIndicators = new MyBufferArray(gl);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateEntityIndicatorsDataBuffers(OpenGL gl, MapView mapView, Game game)
        {
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewTop = mapView.Top;
            float viewBottom = mapView.Bottom;
            float viewRight = mapView.Right;

            List<Entity> visUnits = mapView.GetVisibleEntities(game, game.CurrentPlayer);

            int size = visUnits.Count;
            if (size == 0)
            {
                entitiesEmpty = true;
                return;
            }
            else
            {
                entitiesEmpty = false;
            }
            
            int verticesPerOne = size * 24;
            int verticesSize = verticesPerOne * 3;
            int colorsSize = verticesPerOne * 3;
            int textureSize = verticesPerOne * 2;
            int textureAtlasSize = verticesPerOne * 4;

            entityIndicators.InitializeArrays(verticesSize, colorsSize, textureSize, textureAtlasSize);
            float[] vertices = entityIndicators.vertices;
            float[] colors = entityIndicators.colors;
            float[] texture = entityIndicators.texture;
            float[] texAtlas = entityIndicators.texAtlas;

            //visible units have to be sorted to draw the indicators properly
            visUnits.Sort((u, v) => Math.Sign(v.Center.Y - u.Center.Y));

            for (int i = 0; i < visUnits.Count; i++)
            {
                Entity current = visUnits[i];
                //buffer indices
                int index = i * 24 * 3;
                int texIndex = i * 24 * 2;
                int atlasInd = i * 24 * 4;

                if (current == null)
                    continue;

                Rect indicatorImage = ImageAtlas.GetImageAtlas.BlankWhite;

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
                    
                    //energy
                    AddRectangle(left, bottom, right, top, indicatorImage, depth, index, vertices,
                        index, colors, texIndex, texture, atlasInd, texAtlas, 0f, 0f, 0f);
                    index += 6 * 3;
                    texIndex += 6 * 2;
                    atlasInd += 6 * 4;
                    float energyRight = Math.Max(left, left + (right - left) * (float)(current.Energy.Percentage));
                    AddRectangle(left, bottom, energyRight, top, indicatorImage, depth, index, vertices,
                        index, colors, texIndex, texture, atlasInd, texAtlas, 0f, 1f, 0f);
                    index += 6 * 3;
                    texIndex += 6 * 2;
                    atlasInd += 6 * 4;

                    //health
                    bottom += indicatorHeight*unitSize;
                    top += indicatorHeight*unitSize;
                    AddRectangle(left, bottom, right, top, indicatorImage, depth, index, vertices,
                        index, colors, texIndex, texture, atlasInd, texAtlas, 0f, 0f, 0f);
                    index += 6 * 3;
                    texIndex += 6 * 2;
                    atlasInd += 6 * 4;
                    float healthRight =Math.Max(left, left + (right - left) * (float)(current.Health.Percentage));
                    AddRectangle(left, bottom, healthRight, top, indicatorImage, depth, index, vertices,
                        index, colors, texIndex, texture, atlasInd, texAtlas, 1f, 0f, 0f);
                }
            }

            entityIndicators.BindData(gl, 3, vertices, 3, colors, 2, texture, 4, texAtlas);
        }

        #endregion Entity indicators

        #region Flowfield

        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateFlowField(OpenGL gl)
        {
            flowField = new MyBufferArray(gl);
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
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewTop = mapView.Top;
            float viewBottom = mapView.Bottom;
            float viewRight = mapView.Right;
            float?[,] flowF = mapView.GetVisibleFlowField(flowfield);
            int width = flowF.GetLength(0);
            int height = flowF.GetLength(1);

            //extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;
            
            int verticesPerOne = width * height * 3;
            int verticesSize = verticesPerOne * 3;
            int colorsSize = verticesPerOne * 3;
            int textureSize = verticesPerOne * 2;
            int textureAtlasSize = verticesPerOne * 4;

            flowField.InitializeArrays(verticesSize, colorsSize, textureSize, textureAtlasSize);
            float[] vertices = flowField.vertices;
            float[] colors = flowField.colors;
            float[] texture = flowField.texture;
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
                    int texCoord = (i + j * width) * 3 * 2;
                    int bottomLeftInd = (i + j * width) * 3 * 4;


                    vec2 leftBottom =new vec2((i - (viewLeft % 1)), (j - (viewBottom % 1)));
                    vec2 squareExt = new vec2(sqW, sqH);


                    int offset = 0;
                    int texOffset = 0;
                    if (flowF[i, j] == null)
                    {
                        vertices[coord + 0] = vertices[coord + 1] = vertices[coord + 2]
                            = vertices[coord + 3] = vertices[coord + 4] = vertices[coord + 5] = 0;
                    }
                    else
                    {
                        float angle = flowF[i, j].Value;

                        //rotated triangle coordinates
                        vec2 rotTriLB = (Rotate(triLB, angle) + triOffset + leftBottom) * squareExt;
                        vec2 rotTriLT = (Rotate(triLT, angle) + triOffset + leftBottom) * squareExt;
                        vec2 rotTriRM = (Rotate(triRM, angle) + triOffset + leftBottom) * squareExt;

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

                    //bottom left
                    texture[texCoord + texOffset + 0] = 0;
                    texture[texCoord + texOffset + 1] = 0;

                    texOffset += 2;

                    //top left
                    texture[texCoord + texOffset + 0] = 0;
                    texture[texCoord + texOffset + 1] = 1;

                    texOffset += 2;

                    //top right
                    texture[texCoord + texOffset + 0] = 1;
                    texture[texCoord + texOffset + 1] = 0.5f;

                    SetColor(colors, 0f, 0f, 0f, coord,3);
                    Rect atlasCoords = ImageAtlas.GetImageAtlas.BlankWhite;//the triangles are black
                    SetAtlasCoordinates(texAtlas, atlasCoords, bottomLeftInd, 3);
                }
            }

            flowField.BindData(gl, 3, vertices, 3, colors, 2, texture, 4, texAtlas);
        }

        #endregion Flowfield
        
        #region Selection frame
        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateSelectionFrame(OpenGL gl)
        {
            selectionFrame = new MyBufferArray(gl);
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
            float viewTop = mapView.Top;
            float viewBottom = mapView.Bottom;
            float viewRight = mapView.Right;
            
            //extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;

            float[] vertices = new float[6 * 3];
            float[] colors = new float[6 * 3];
            float[] textureCoords = new float[6 * 2];
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
                SetSquareTextureCoordinates(textureCoords, 0);
                SetColor(colors, 1f, 1f, 1f, 0, 6);
                Rect atlasCoords = ImageAtlas.GetImageAtlas.UnitsSelector;
                SetAtlasCoordinates(texBottomLeft, atlasCoords, 0, 6);
            }

            selectionFrame.BindData(gl, 3, vertices, 3, colors, 2, textureCoords, 4, texBottomLeft);
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
        /// Write vertex colors with the given parameters to the index of the array.
        /// </summary>
        /// <param name="colors">Writing array.</param>
        /// <param name="index">Index of the first color in array.</param>
        /// <param name="vertCount">Number of times this color should be repeated (for each vertex once).</param>
        private static void SetColor(float[] colors, float red, float green, float blue, int index,
            int vertCount)
        {
            int offset = 0;

            for (int i = 0; i < vertCount; i++)
            {
                colors[index + offset + 0] = red;
                colors[index + offset + 1] = green;
                colors[index + offset + 2] = blue;

                offset += 3;

            }
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
                                            int cInd, float[] colors,
                                            int tInd, float[] textureCoords,
                                            int aInd, float[] texAtlas,
                                            float r, float g, float b)
        {
            //vertices
            SetRectangleVertices(vertices, bottom, top, left, right, -depth, vInd);

            //colors
            SetColor(colors, r, g, b, cInd, 6);

            //texture coordinates
            SetSquareTextureCoordinates(textureCoords, tInd);

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
