using GlmNet;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameControls;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps.MovementGenerating;

namespace SanguineGenesis.GUI
{
    /// <summary>
    /// Used for drawing objects in game to the OpenGL control.
    /// </summary>
    static class OpenGLAtlasDrawer
    {
        
        //the shader program for the vertex and fragment shader
        static private ShaderProgram shaderProgram;

        //vertex shader attribute indices
        private const uint attributeIndexPosition = 0;
        private const uint attributeIndexUVCoord = 1;
        private const uint attributeIndexTexBL = 2;

        //the projection matrix
        private static mat4 projectionMatrix;

        //identifier of atlas texture at position 0
        static private uint[] textureIds;

        //vertex buffer arrays which contain the buffers for vertex, 
        //texture and bottom left coordinates of textures
        private static MyBufferArray map;
        private static MyBufferArray flowField;
        private static MyBufferArray nutrientsMap;
        private static MyBufferArray entityCircles;
        private static MyBufferArray entities;
        private static MyBufferArray entityIndicators;
        private static MyBufferArray selectorRect;

        //true if there are no entities to draw
        private static bool entitiesEmpty;
        //true if there are no entity indicators to draw
        private static bool entityIndicatorsEmpty;

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
            shaderProgram.BindAttributeLocation(gl, attributeIndexPosition, "in_Position");
            shaderProgram.BindAttributeLocation(gl, attributeIndexUVCoord, "in_TexCoord");
            shaderProgram.BindAttributeLocation(gl, attributeIndexTexBL, "in_TexLeftBottomWidthHeight");
            shaderProgram.AssertValid(gl);

            //create projection matrix that maps points directly to screen coordinates
            projectionMatrix = glm.ortho(0f, width, 0f, height, 0f, 100f);
            
            //bind the shader program and set its parameters
            shaderProgram.Bind(gl);
            shaderProgram.SetUniformMatrix4(gl, "projectionMatrix", projectionMatrix.to_array());

            //load image used as atlas and pass it to the shader program
            InitializeAtlas(gl, shaderProgram);

            //initializes MyBufferArrays for map, entities and selector rectangle
            InitMyBufferArrays(gl);
        }

        /// <summary>
        /// Loads the atlas texture, binds it and sets texture drawing parameters.
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
            LoadTexture("Images/atlas.png", gl, shaderProgram);

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
            textureIds = new uint[1];
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

        /// <summary>
        /// Initializes MyBufferArrays for map, entities and selector rectangle.
        /// </summary>
        private static void InitMyBufferArrays(OpenGL gl)
        {
            map = new MyBufferArray(gl, CreateSquareUVCoordinatesTri);
            nutrientsMap = new MyBufferArray(gl, CreateSquareUVCoordinatesTri);
            entityCircles = new MyBufferArray(gl, CreateSquareUVCoordinatesTri);
            entities = new MyBufferArray(gl, CreateSquareUVCoordinatesTri);
            entityIndicators = new MyBufferArray(gl, CreateSquareUVCoordinatesTri);
            flowField = new MyBufferArray(gl, CreateSquareUVCoordinatesTri);
            selectorRect = new MyBufferArray(gl, CreateSquareUVCoordinatesTri);
        }

        /// <summary>
        /// Destructs all buffers and programs created in gl.
        /// </summary>
        public static void Destruct(OpenGL gl)
        {
            //delete shader program
            shaderProgram.Delete(gl);
            //delete VAOs
            map.Dispose(gl);
            flowField.Dispose(gl);
            nutrientsMap.Dispose(gl);
            entityCircles.Dispose(gl);
            entities.Dispose(gl);
            entityIndicators.Dispose(gl);
            selectorRect.Dispose(gl);
            //delete textures
            if (textureIds != null)
                gl.DeleteTextures(textureIds.Length, textureIds);
        }
        #endregion Initialization

        /// <summary>
        /// Represents data that can be drawn by the shader program.
        /// </summary>
        private class MyBufferArray
        {
            public VertexBufferArray VertexBufferArray { get; }
            public VertexBuffer VertexDataBuffer { get; }
            public VertexBuffer UVDataBuffer { get; }
            public VertexBuffer TexAtlasDataBuffer { get; }

            public float[] vertices;
            public float[] uv;
            public float[] texAtlas;

            /// <summary>
            /// Creates new array with UV coordinates with given length.
            /// </summary>
            public delegate float[] UVCreator(int size);
            public UVCreator UvCreator { get; }

            /// <summary>
            /// True if the buffers contains no data.
            /// </summary>
            public bool Clear { get; private set; }

            public MyBufferArray(OpenGL gl, UVCreator uvCreator)
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

                UvCreator = uvCreator;
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
            }

            /// <summary>
            /// Initialize arrays with zeros with the given sizes.
            /// </summary>
            public void InitializeArrays(OpenGL gl, int verticesSize, int uvSize, int texAtlasSize)
            {
                InitArray(ref vertices, verticesSize);
                InitUV(gl, ref uv, uvSize);
                InitArray(ref texAtlas, texAtlasSize);
                Clear = false;
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

            /// <summary>
            /// Sets uv coordinates by UvCreator if UVDataBuffer is too small or it was Cleared.
            /// </summary>
            private void InitUV(OpenGL gl, ref float[] uvArray, int uvSize)
            {
                if (uv.Length < uvSize || Clear)
                {
                    uvArray = UvCreator(uvSize);
                    VertexBufferArray.Bind(gl);
                    UVDataBuffer.Bind(gl);
                    UVDataBuffer.SetData(gl, attributeIndexUVCoord, uvArray, false, 2/*uv stride*/);
                    VertexBufferArray.Unbind(gl);
                }
            }

            /// <summary>
            /// Number of vertices. Each vertex is defined by three consecutive floats in
            /// the array vertices.
            /// </summary>
            public int VerticesCount => vertices == null ? 0 : vertices.Length / 3;

            public void ClearBuffers(OpenGL gl)
            {
                BindData(gl, 1, new float[1],
                            1, new float[1],
                            1, new float[1]);
                Clear = true;
            }

            /// <summary>
            /// Removes all buffers in this object from the state of gl. This object can't be receive data
            /// after the buffers are removed.
            /// </summary>
            public void Dispose(OpenGL gl)
            {
                ClearBuffers(gl);
                gl.DeleteBuffers(3, new uint[]
                {
                    VertexDataBuffer.VertexBufferObject,
                    UVDataBuffer.VertexBufferObject,
                    TexAtlasDataBuffer.VertexBufferObject
                });
                VertexBufferArray.Delete(gl);
            }
        }

        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        public static void Draw(OpenGL gl, GameplayOptions gameplayOptions)
        {
            //clear the scene
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            //draw vertex array buffers
            {
                //draw map
                map.VertexBufferArray.Bind(gl);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, map.VerticesCount);

                //draw nutrients map
                if (gameplayOptions.NutrientsVisible)
                {
                    nutrientsMap.VertexBufferArray.Bind(gl);
                    gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, nutrientsMap.VerticesCount);
                }

                //draw flowfield
                if (gameplayOptions.ShowFlowfield)
                {
                    flowField.VertexBufferArray.Bind(gl);
                    gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, flowField.VerticesCount);
                }

                //draw entities
                if (!entitiesEmpty)
                {
                    entityCircles.VertexBufferArray.Bind(gl);
                    gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, entityCircles.VerticesCount);

                    entities.VertexBufferArray.Bind(gl);
                    gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, entities.VerticesCount);
                }
                if (!entityIndicatorsEmpty)
                {
                    entityIndicators.VertexBufferArray.Bind(gl);
                    gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, entityIndicators.VerticesCount);
                }

                //draw selection rectangle
                selectorRect.VertexBufferArray.Bind(gl);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 6);
            }
        }

        #region Updates
        
        #region Map
        /// <summary>
        /// Updates buffers of MyBufferArray with the information about map.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateMapDataBuffers(OpenGL gl, MapView mapView, Game game)
        {
            //prepare data
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewBottom = mapView.Bottom;

            int vertPerObj = 6;

            bool[,] visibleVisibility = mapView.GetVisibleVisibilityMap(game.Players[game.CurrentPlayer.FactionID].VisibilityMap);
            Node[,] visible = mapView.GetVisibleNodes(game.Players[game.CurrentPlayer.FactionID].VisibleMap);
            int width = visible.GetLength(0);
            int height = visible.GetLength(1);

            //extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;

            int verticesPerOne = width * height * vertPerObj;
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
                    int coord = (i + j * width) * vertPerObj * 3;
                    int bottomLeftInd = (i + j * width) * vertPerObj * 4;

                    Node current = visible[i, j];
                    if (current == null)
                        continue;

                    bool isVisible = true;
                    if (visibleVisibility != null)
                        isVisible = visibleVisibility[i, j];
                    
                    Rect atlasCoords = ImageAtlas.GetImageAtlas.GetNodeTexture(current.Biome,current.SoilQuality,current.Terrain, isVisible);

                    //node position
                    float bottom = (current.Y - viewBottom) * sqH;
                    float top = (current.Y - viewBottom + 1) * sqH;
                    float left = (current.X - viewLeft) * sqW;
                    float right = (current.X - viewLeft + 1) * sqW;

                    SetRectangleVerticesTri(vertices, bottom, top, left, right, -10, coord);
                    SetAtlasCoordinates(texAtlas, atlasCoords, bottomLeftInd, vertPerObj);
                }
            }

            map.BindData(gl, 3, vertices, 4, texAtlas);
        }

        #endregion Map

        #region Nutrients map
        /// <summary>
        /// Clears all buffers representing nutrients map.
        /// </summary>
        public static void ClearNutrientsMapDataBuffers(OpenGL gl)
        {
            if (!nutrientsMap.Clear)
                nutrientsMap.ClearBuffers(gl);
        }

        /// <summary>
        /// Updates buffers of MyBufferArray with the information about nutrients map.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateNutrientsMapDataBuffers(OpenGL gl, MapView mapView, Game game)
        {
            //prepare data
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewBottom = mapView.Bottom;

            int vertPerObj = 6;

            Node[,] visible = mapView.GetVisibleNodes(game.CurrentPlayer.VisibleMap);
            int width = visible.GetLength(0);
            int height = visible.GetLength(1);

            // extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;

            int vertexCount = width * height * vertPerObj;
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
                    int coord = (i + j * width) * vertPerObj * 3;
                    int bottomLeftInd = (i + j * width) * vertPerObj * 4;

                    Node current = visible[i, j];
                    if (current == null)
                        continue;

                    //active nutrients
                    int activeNutrients = Math.Min(99,(int)Math.Ceiling(current.ActiveNutrients * 10));
                    Rect atlasActiveNutrients = ImageAtlas.GetImageAtlas.GetNumberedTriangle(activeNutrients);

                    //passive nutrients
                    int passiveNutrients = Math.Min(99, (int)Math.Ceiling(current.PassiveNutrients));
                    Rect atlasPassiveNutrients = ImageAtlas.GetImageAtlas.GetNumberedTriangle(passiveNutrients);

                    //tile position
                    float bottom = (current.Y - viewBottom) * sqH;
                    float top = (current.Y - viewBottom + 1) * sqH;
                    float left = (current.X - viewLeft) * sqW;
                    float right = (current.X - viewLeft + 1) * sqW;

                    SetRectangleVerticesTri(vertices, bottom, top, left, right, -10, coord);
                    //top left triangle
                    SetAtlasCoordinates(texAtlas, atlasPassiveNutrients, bottomLeftInd , 3);
                    //bottom right triangle
                    SetAtlasCoordinates(texAtlas, atlasActiveNutrients, bottomLeftInd + 4 * 3, 3);
                }
            }

            nutrientsMap.BindData(gl, 3, vertices, 4, texAtlas);
        }

        #endregion Nutrients map

        #region Flowfield
        /// <summary>
        /// Clears all buffers representing flow field.
        /// </summary>
        public static void ClearFlowFieldDataBuffers(OpenGL gl)
        {
            if (!flowField.Clear)
                flowField.ClearBuffers(gl);
        }

        /// <summary>
        /// Updates buffers of MyBufferArray with the information about the flow field
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

            int vertPerObj = 3;

            //extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;

            int verticesPerOne = width * height * vertPerObj;
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
                    int coord = (i + j * width) * vertPerObj * 3;
                    int bottomLeftInd = (i + j * width) * vertPerObj * 4;


                    vec2 leftBottom = new vec2((i - (viewLeft % 1)), (j - (viewBottom % 1)));
                    vec2 squareExt = new vec2(sqW, sqH);

                    //geometry of the triangle
                    int offset = 0;
                    if (flowF[i, j] == null)
                    {
                        //no triangle
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
                            //rotates the point vec around the origin by angle
                            vec2 Rotate(vec2 vec)
                            {
                                float cosA = (float)Math.Cos(angle);
                                float sinA = (float)Math.Sin(angle);
                                return new vec2(cosA * vec.x - sinA * vec.y,
                                                sinA * vec.x + cosA * vec.y);
                            }

                            //rotated triangle coordinates
                            rotTriLB = (Rotate(triLB) + triOffset + leftBottom) * squareExt;
                            rotTriLT = (Rotate(triLT) + triOffset + leftBottom) * squareExt;
                            rotTriRM = (Rotate(triRM) + triOffset + leftBottom) * squareExt;
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
                    SetAtlasCoordinates(texAtlas, atlasCoords, bottomLeftInd, vertPerObj);
                }
            }

            flowField.BindData(gl, 3, vertices, 4, texAtlas);
        }

        #endregion Flowfield

        #region Entity circles
        /// <summary>
        /// Updates buffers of MyBufferArray with the information about entities.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateEntityCirclesDataBuffers(OpenGL gl, MapView mapView, Game game)
        {
            //prepare data
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewBottom = mapView.Bottom;

            int vertPerObj = 6;

            List<Entity> visEntity = mapView.GetVisibleEntities(game, game.CurrentPlayer).ToList();

            //check if there are any entities to draw
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

            int verticesPerOne = size * vertPerObj;
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
                int index = i * vertPerObj * 3;
                int atlasInd = i * vertPerObj * 4;

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
                    SetRectangleVerticesTri(vertices, bottom, top, left, right, -8, index);

                    Rect atlasCoords;
                    //colors
                    //fill animals that are state change locked with white color
                    if (current is Animal a &&
                        a.StateChangeLock != null)
                    {
                        atlasCoords = ImageAtlas.GetImageAtlas.UnitCircleWhite;
                    }else if (!current.Selected)
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
                    SetAtlasCoordinates(texAtlas, atlasCoords, atlasInd, vertPerObj);
                }
            }

            entityCircles.BindData(gl, 3, vertices, 4, texAtlas);
        }
        #endregion Entity circles

        #region Entity
        /// <summary>
        /// Updates buffers of MyBufferArray with the information about entities.
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

            int vertPerObj = 6;

            List<Entity> visEntities = mapView.GetVisibleEntities(game, game.CurrentPlayer).ToList();

            //check if there are any entities to draw
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
            
            int verticesPerOne = size * vertPerObj;
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
                int index = i * vertPerObj * 3;
                int texIndex = i * vertPerObj * 2;
                int atlasInd = i * vertPerObj * 4;

                if (current == null)
                    continue;

                float entitySize = nodeSize;
                
                //entity image
                {
                    Animation anim = current.AnimationState.Animation;

                    //position of the center of the image
                    Vector2 imageCenter = anim.Center;
                    if (current is Animal && !((Animal)current).FacingLeft)
                        imageCenter = new Vector2(anim.Width - anim.Center.X, anim.Center.Y);

                    //image position
                    float bottom = (current.Center.Y - imageCenter.Y - viewBottom) * entitySize;
                    float top = (current.Center.Y - imageCenter.Y - viewBottom + anim.Height) * entitySize;
                    float left = (current.Center.X - imageCenter.X - viewLeft) * entitySize;
                    float right = (current.Center.X - imageCenter.X - viewLeft + anim.Width) * entitySize;

                    //depth is from [4,5]
                    float depth = 4f + current.Center.Y / game.Map.Height;

                    //vertices
                    SetRectangleVerticesTri(vertices, bottom, top, left, right, -depth, index);

                    //uv coordinates
                    if(current is Animal && !((Animal)current).FacingLeft)
                        SetHorizFlipSquareTextureCoordinatesTri(uv, texIndex);
                    else
                        SetSquareTextureCoordinatesTri(uv, texIndex);

                    //atlas coordinates
                    Rect entityImage = current.AnimationState.CurrentImage;
                    SetAtlasCoordinates(texAtlas, entityImage, atlasInd, vertPerObj);
                }
            }

            entities.BindData(gl, 3, vertices, 2, uv, 4, texAtlas);
        }
        #endregion Entity

        #region Entity indicators
        /// <summary>
        /// Updates buffers of MyBufferArray with the information entities.
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

            int vertPerObj = 6;
            int objInOne = 4;

            //only show indicators for entities that player can directly see and don't have full
            //health and energy
            List<Entity> visEntities = mapView.GetVisibleEntities(game, observer).Where((e) => observer.CanSee(e))
                .Where(e=>e.Health!=e.Health.MaxValue || e.Energy !=e.Energy.MaxValue).ToList();

            //check if there are any entities to draw
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
            
            int verticesPerOne = size * vertPerObj * objInOne;
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
                int index = i * vertPerObj * objInOne * 3;
                int atlasInd = i * vertPerObj * objInOne * 4;

                if (current == null)
                    continue;

                float unitSize = nodeSize;

                float indicatorWidth = current.Radius * 1.5f;
                float indicatorHeight = 0.15f;
                {
                    Animation anim = current.AnimationState.Animation;

                    //rectangle
                    float bottom = (current.Center.Y - anim.Center.Y - viewBottom + anim.Height) * unitSize;
                    float top = (current.Center.Y - anim.Center.Y - viewBottom + anim.Height + indicatorHeight) * unitSize;
                    float left = (current.Center.X - indicatorWidth/2f - viewLeft) * unitSize;
                    float right = (current.Center.X + indicatorWidth/2f - viewLeft) * unitSize;

                    //depth is from [2,3]
                    float depth = 2f + current.Center.Y / game.Map.Height;

                    Rect blackRect = ImageAtlas.GetImageAtlas.BlackSquare;
                    Rect redRect = ImageAtlas.GetImageAtlas.RedSquare;
                    Rect greenRect = ImageAtlas.GetImageAtlas.GreenSquare;
                    //energy
                    AddRectangleTri(left, bottom, right, top, blackRect, depth, index, vertices,
                         atlasInd, texAtlas);
                    index += vertPerObj * 3;
                    atlasInd += vertPerObj * 4;
                    float energyRight = Math.Max(left, left + (right - left) * (float)(current.Energy.Percentage));
                    AddRectangleTri(left, bottom, energyRight, top, greenRect, depth, index, vertices,
                         atlasInd, texAtlas);
                    index += vertPerObj * 3;
                    atlasInd += vertPerObj * 4;

                    //health
                    bottom += indicatorHeight*unitSize;
                    top += indicatorHeight*unitSize;
                    AddRectangleTri(left, bottom, right, top, blackRect, depth, index, vertices,
                         atlasInd, texAtlas);
                    index += vertPerObj * 3;
                    atlasInd += vertPerObj * 4;
                    float healthRight =Math.Max(left, left + (right - left) * (float)(current.Health.Percentage));
                    AddRectangleTri(left, bottom, healthRight, top, redRect, depth, index, vertices,
                         atlasInd, texAtlas);
                }
            }

            entityIndicators.BindData(gl, 3, vertices, 4, texAtlas);
        }

        #endregion Entity indicators
        
        #region Selector rectangle
        /// <summary>
        /// Updates buffers of MyBufferArray with new positions.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateSelectorRectDataBuffers(OpenGL gl, MapView mapView, MapSelectorRect selectorRect)
        {
            float nodeSize = mapView.NodeSize;
            float viewLeft = mapView.Left;
            float viewBottom = mapView.Bottom;

            int vertPerObj = 6;

            float[] vertices = new float[vertPerObj * 3];
            float[] texBottomLeft = new float[vertPerObj * 4];
            
            //draw selector rect only if it exists
            if (selectorRect != null)
            {
                float bottom = selectorRect.Bottom - viewBottom;
                float top = selectorRect.Top - viewBottom;
                float left = selectorRect.Left - viewLeft;
                float right = selectorRect.Right - viewLeft;

                SetRectangleVerticesTri(vertices, bottom * nodeSize, top * nodeSize,
                    left * nodeSize, right * nodeSize, -1f, 0);
                Rect atlasCoords = ImageAtlas.GetImageAtlas.UnitsSelector;
                SetAtlasCoordinates(texBottomLeft, atlasCoords, 0, vertPerObj);
            }

            OpenGLAtlasDrawer.selectorRect.BindData(gl, 3, vertices, 4, texBottomLeft);
        }
        #endregion Selector rectangle
        
        #endregion Updates

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
        private static void SetRectangleVerticesTri(float[] vertices, float bottom, float top,
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
        private static float[] CreateSquareUVCoordinatesTri(int size)
        {
            float[] uv = new float[size];
            int uvPerObj = 2 * 6;
            for (int i = 0; i < size / uvPerObj; i++)
            {
                int index = i * uvPerObj;
                SetSquareTextureCoordinatesTri(uv, index);
            }
            return uv;
        }

        /// <summary>
        /// Write texture UV coordinates for a square made of two triangles.
        /// </summary>
        /// <param name="textureCoords">Writing array.</param>
        /// <param name="index">Index of the first UV coordinate in array.</param>
        private static void SetSquareTextureCoordinatesTri(float[] textureCoords, int index)
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
        private static void SetHorizFlipSquareTextureCoordinatesTri(float[] textureCoords, int index)
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
        /// <param name="imageCoords">Coordinates of the image in the atlas.</param>
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
        public static void AddRectangleTri(float left, float bottom, float right, float top, Rect image, float depth,
                                            int vInd, float[] vertices,
                                            int aInd, float[] texAtlas)
        {
            //vertices
            SetRectangleVerticesTri(vertices, bottom, top, left, right, -depth, vInd);

            //atlas coordinates
            SetAtlasCoordinates(texAtlas, image, aInd, 6);
        }
        #endregion Array writing methods
    }
}
