using GlmNet;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static wpfTest.MainWindow;

namespace wpfTest
{
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
            var vertexShaderSource = ManifestResourceLoader.LoadTextFile("AtlasTexShader.vert");
            var fragmentShaderSource = ManifestResourceLoader.LoadTextFile("AtlasTexShader.frag");
            shaderProgram.Create(gl, vertexShaderSource, fragmentShaderSource, null);

            //set indices for the shader program attributes
            shaderProgram.BindAttributeLocation(gl, attributeIndexPosition, "in_Position");
            shaderProgram.BindAttributeLocation(gl, attributeIndexcolor, "in_Color");
            shaderProgram.BindAttributeLocation(gl, attributeIndexTexCoord, "in_TexCoord");
            shaderProgram.BindAttributeLocation(gl, attributeIndexTexBL, "in_TexBottomLeft");

            //compile the program
            shaderProgram.AssertValid(gl);

            //create projection matrix that maps points directly to screen coordinates
            projectionMatrix = glm.ortho(0f, width, 0f, height, 0f, 100f);

            //create view matrix that translates graphical objects to visible range
            viewMatrix = glm.translate(new mat4(1.0f), new vec3(0.0f, 0.0f, -1.0f));

            //create identity matrix as model matrix
            modelMatrix = new mat4(1.0f);

            //load image used as atlas and pass it to the shader program
            InitializeAtlas(gl);
            
            //bind the shader program and set its parameters
            shaderProgram.Bind(gl);
            shaderProgram.SetUniformMatrix4(gl, "projectionMatrix", projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "viewMatrix", viewMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "modelMatrix", modelMatrix.to_array());
            shaderProgram.SetUniform3(gl, "atlasExtents", 640, 640, 0);
            shaderProgram.SetUniform3(gl, "texExtents", 62, 62, 0);
        }
        
        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        public static void Draw(OpenGL gl)
        {
            //clear the scene
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);
            
            //draw vertex array buffers
            map.VertexBufferArray.Bind(gl);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 10000);

            flowMap.VertexBufferArray.Bind(gl);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 10000);

            if (!unitsEmpty)
            {
                units.VertexBufferArray.Bind(gl);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 10000);
            }
        }

        /// <summary>
        /// Loads the tile map texture, binds it and sets texture drawing parameters.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        private static void InitializeAtlas(OpenGL gl)
        {
            //enable drawing textures
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            //enable alpha channel for textures
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            LoadTexture("tileMap.png", gl);

            //set linear filtering
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);

        }

        /// <summary>
        /// Loads the texture from fileName to gl. 
        /// </summary>
        /// <param name="fileName">The name of the file from which the texture will be loaded.</param>
        /// <param name="gl">The instance of OpenGL to which the texture will be loaded.</param>
        private static void LoadTexture(String fileName, OpenGL gl)
        {
            //load image and flit it vertically
            Bitmap textureImage = new Bitmap(fileName);
            textureImage.RotateFlip(RotateFlipType.RotateNoneFlipY);

            //generate id for the texture and then bind the image with this id
            uint[] textureIds = new uint[1];
            gl.GenTextures(1, textureIds);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureIds[0]);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA, textureImage.Width, textureImage.Height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE,
                textureImage.LockBits(new Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb).Scan0);
        }


        private class BufferArray
        {
            public VertexBufferArray VertexBufferArray { get; set; }
            public VertexBuffer VertexDataBuffer { get; set; }
            public VertexBuffer ColorDataBuffer { get; set; }
            public VertexBuffer TextureDataBuffer { get; set; }
            public VertexBuffer TexBLDataBuffer { get; set; }
        }

        //vertex buffer arrays which contain the buffers for vertex, 
        //color, texture and bottom left coordinates of textures
        private static BufferArray map;
        private static BufferArray flowMap;
        private static BufferArray units;

        //true if there are no units to draw
        private static bool unitsEmpty;

        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateMap(OpenGL gl)
        {
            map = new BufferArray();

            //initialize map.VertexBufferArray and link it to its VertexBuffers
            map.VertexBufferArray = new VertexBufferArray();
            map.VertexBufferArray.Create(gl);
            map.VertexBufferArray.Bind(gl);

            //initialize empty map.VertexDataBuffer
            map.VertexDataBuffer = new VertexBuffer();
            map.VertexDataBuffer.Create(gl);

            //initialize empty map.ColorDataBuffer
            map.ColorDataBuffer = new VertexBuffer();
            map.ColorDataBuffer.Create(gl);

            //initialize empty map.TextureDataBuffer
            map.TextureDataBuffer = new VertexBuffer();
            map.TextureDataBuffer.Create(gl);

            //initialize empty map.TexBLDataBuffer
            map.TexBLDataBuffer = new VertexBuffer();
            map.TexBLDataBuffer.Create(gl);
            
            map.VertexBufferArray.Unbind(gl);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateMapDataBuffers(OpenGL gl, MapView mapView)
        {
            float nodeSize;
            float viewLeft;
            float viewTop;
            float viewBottom;
            float viewRight;
            Node[,] visible;
            //mapView has to be locked because it can also be accessed from the main game loop
            lock (mapView)
            {
                nodeSize = mapView.NodeSize;
                viewLeft = mapView.Left;
                viewTop = mapView.Bottom;
                viewBottom = mapView.Top;
                viewRight = mapView.Right;
                visible = mapView.GetVisibleNodes();
            }

            int width = visible.GetLength(0);
            int height = visible.GetLength(1);
            float blX = 1;
            float blY = 1;

            //extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;

            float[] vertices= new float[width * height * 6 * 3];
            float[] colors= new float[width * height * 6 * 3];
            float[] textureCoords = new float[width * height * 6 * 2];
            float[] texBottomLeft = new float[width * height * 6 * 2];

            /*if (map.Vertices == null || map.Vertices.Length < width * height * 6 * 3)
                vertices = map.Vertices = new float[width * height * 6 * 3];
            else
            {
                vertices = map.Vertices;
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i] = 0;
            }

            if (map.Colors == null || map.Colors.Length < width * height * 6 * 3)
                colors = map.Vertices = new float[width * height * 6 * 3];
            else
            {
                colors = map.Colors;
                for (int i = 0; i < colors.Length; i++)
                    colors[i] = 0;
            }

            if (map.TextureCoords == null || map.TextureCoords.Length < width * height * 6 * 2)
                textureCoords = map.Vertices = new float[width * height * 6 * 2];
            else
            {
                textureCoords = map.TextureCoords;
                for (int i = 0; i < textureCoords.Length; i++)
                    textureCoords[i] = 0;
            }

            if (map.TexBottomLeft == null || map.TexBottomLeft.Length < width * height * 6 * 2)
                texBottomLeft = map.TexBottomLeft = new float[width * height * 6 * 2];
            else
            {
                texBottomLeft = map.TexBottomLeft;
                for (int i = 0; i < texBottomLeft.Length; i++)
                    texBottomLeft[i] = 0;
            }*/

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //buffer indices
                    int coord = (i + j * width) * 6 * 3;
                    int texCoord = (i + j * width) * 6 * 2;

                    Node current = visible[i, j];
                    if (current == null)
                        continue;

                    if (current.Terrain == Terrain.GRASS)
                    {
                        blX = 1;
                        blY = 1;
                    }
                    else
                    {
                        blX = 65;
                        blY = 1;
                    }

                    //tile position
                    float bottom = (current.Y - viewTop) * sqH;
                    float top = (current.Y - viewTop + 1) * sqH;
                    float left = (current.X - viewLeft) * sqW;
                    float right = (current.X - viewLeft + 1) * sqW;

                    SetSquareVertices(vertices, bottom, top, left, right, -1, coord);
                    SetColor(colors, 1f, 1f, 1f, coord,6);
                    SetSquareTextureCoordinates(textureCoords, texCoord);
                    SetTexBottomLeft(texBottomLeft, blX,blY,texCoord,6);
                }
            }

            //bind map.VertexBufferArray and set data of its buffers
            map.VertexBufferArray.Bind(gl);
            
            map.VertexDataBuffer.Bind(gl);
            map.VertexDataBuffer.SetData(gl, attributeIndexPosition, vertices, false, 3);
            
            map.ColorDataBuffer.Bind(gl);
            map.ColorDataBuffer.SetData(gl, attributeIndexcolor, colors, false, 3);
            
            map.TextureDataBuffer.Bind(gl);
            map.TextureDataBuffer.SetData(gl, attributeIndexTexCoord, textureCoords, false, 2);
            
            map.TexBLDataBuffer.Bind(gl);
            map.TexBLDataBuffer.SetData(gl, attributeIndexTexBL, texBottomLeft, false, 2);
        }

        private static void SetSquareVertices(float[] vertices, float bottom, float top,
                                                float left, float right, float dist, int coord)
        {
            int offset = 0;

            //bottom left
            vertices[coord + offset + 0] = left;
            vertices[coord + offset + 1] = bottom;
            vertices[coord + offset + 2] = -1;

            offset += 3;

            //top left
            vertices[coord + offset + 0] = left;
            vertices[coord + offset + 1] = top;
            vertices[coord + offset + 2] = -1;

            offset += 3;

            //top right
            vertices[coord + offset + 0] = right;
            vertices[coord + offset + 1] = top;
            vertices[coord + offset + 2] = -1;

            offset += 3;

            //bottom right
            vertices[coord + offset + 0] = right;
            vertices[coord + offset + 1] = bottom;
            vertices[coord + offset + 2] = -1;

            offset += 3;

            //bottom left
            vertices[coord + offset + 0] = left;
            vertices[coord + offset + 1] = bottom;
            vertices[coord + offset + 2] = -1;

            offset += 3;

            //top right
            vertices[coord + offset + 0] = right;
            vertices[coord + offset + 1] = top;
            vertices[coord + offset + 2] = -1;
        }

        private static void SetColor(float[] colors, float red, float green, float blue , int coord,
            int vertCount)
        {
            int offset = 0;

            for(int i = 0; i < vertCount; i++)
            {
                colors[coord + offset + 0] = red;
                colors[coord + offset + 1] = green;
                colors[coord + offset + 2] = blue;

                offset += 3;

            }
            /*//bottom left
            colors[coord + offset + 0] = red;
            colors[coord + offset + 1] = green;
            colors[coord + offset + 2] = blue;

            offset += 3;

            //top left
            colors[coord + offset + 0] = red;
            colors[coord + offset + 1] = green;
            colors[coord + offset + 2] = blue;

            offset += 3;

            //top right
            colors[coord + offset + 0] = red;
            colors[coord + offset + 1] = green;
            colors[coord + offset + 2] = blue;

            offset += 3;

            //bottom right
            colors[coord + offset + 0] = red;
            colors[coord + offset + 1] = green;
            colors[coord + offset + 2] = blue;

            offset += 3;

            //bottom left
            colors[coord + offset + 0] = red;
            colors[coord + offset + 1] = green;
            colors[coord + offset + 2] = blue;

            offset += 3;

            //top right
            colors[coord + offset + 0] = red;
            colors[coord + offset + 1] = green;
            colors[coord + offset + 2] = blue;*/
        }

        private static void SetSquareTextureCoordinates(float[] textureCoords, int coord)
        {
            int offset = 0;

            //bottom left

            textureCoords[coord + offset + 0] = 0;
            textureCoords[coord + offset + 1] = 0;
            
            offset += 2;

            //top left
            textureCoords[coord + offset + 0] = 0;
            textureCoords[coord + offset + 1] = 1;
            
            offset += 2;

            //top right
            textureCoords[coord + offset + 0] = 1;
            textureCoords[coord + offset + 1] = 1;
            
            offset += 2;

            //bottom right
            textureCoords[coord + offset + 0] = 1;
            textureCoords[coord + offset + 1] = 0;

            offset += 2;

            //bottom left
            textureCoords[coord + offset + 0] = 0;
            textureCoords[coord + offset + 1] = 0;
            
            offset += 2;

            //top right
            textureCoords[coord + offset + 0] = 1;
            textureCoords[coord + offset + 1] = 1;
        }

        private static void SetTexBottomLeft(float[] texBottomLeft, float texBottomLeftX, float texBottomLeftY , int coord,
            int vertCount)
        {
            int offset = 0;

            for(int i = 0; i < vertCount; i++)
            {
                texBottomLeft[coord + offset + 0] = texBottomLeftX;
                texBottomLeft[coord + offset + 1] = texBottomLeftY;

                offset += 2;
            }
            /*
            //bottom left
            texBottomLeft[coord + offset + 0] = texBottomLeftX;
            texBottomLeft[coord + offset + 1] = texBottomLeftY;
            
            offset += 2;

            //top left
            texBottomLeft[coord + offset + 0] = texBottomLeftX;
            texBottomLeft[coord + offset + 1] = texBottomLeftY;
            
            offset += 2;

            //top right
            texBottomLeft[coord + offset + 0] = texBottomLeftX;
            texBottomLeft[coord + offset + 1] = texBottomLeftY;
            
            offset += 2;

            //bottom right
            texBottomLeft[coord + offset + 0] = texBottomLeftX;
            texBottomLeft[coord + offset + 1] = texBottomLeftY;
            
            offset += 2;

            //bottom left
            texBottomLeft[coord + offset + 0] = texBottomLeftX;
            texBottomLeft[coord + offset + 1] = texBottomLeftY;
            
            offset += 2;

            //top right
            texBottomLeft[coord + offset + 0] = texBottomLeftX;
            texBottomLeft[coord + offset + 1] = texBottomLeftY;*/
        }


        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateUnits(OpenGL gl)
        {
            units = new BufferArray();

            //initialize map.VertexBufferArray and link it to its VertexBuffers
            units.VertexBufferArray = new VertexBufferArray();
            units.VertexBufferArray.Create(gl);
            units.VertexBufferArray.Bind(gl);

            //initialize empty map.VertexDataBuffer
            units.VertexDataBuffer = new VertexBuffer();
            units.VertexDataBuffer.Create(gl);

            //initialize empty map.ColorDataBuffer
            units.ColorDataBuffer = new VertexBuffer();
            units.ColorDataBuffer.Create(gl);

            //initialize empty map.TextureDataBuffer
            units.TextureDataBuffer = new VertexBuffer();
            units.TextureDataBuffer.Create(gl);

            //initialize empty map.TexBLDataBuffer
            units.TexBLDataBuffer = new VertexBuffer();
            units.TexBLDataBuffer.Create(gl);

            units.VertexBufferArray.Unbind(gl);
        }

        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateUnitsDataBuffers(OpenGL gl, MapView mapView)
        {
            float nodeSize;
            float viewLeft;
            float viewTop;
            float viewBottom;
            float viewRight;
            List<Unit> visUnits;
            //mapView has to be locked because it can also be accessed from the main game loop
            lock (mapView)
            {
                nodeSize = mapView.NodeSize;
                viewLeft = mapView.Left;
                viewTop = mapView.Bottom;
                viewBottom = mapView.Top;
                viewRight = mapView.Right;
                visUnits = mapView.GetVisibleUnits();
            }
            
            float blX = 129;
            float blY = 1;


            int size = visUnits.Count;
            if (size == 0)
            {
                unitsEmpty = true;
                return;
            }
            else
            {
                unitsEmpty = false;
            }

            float[] vertices = new float[size * 6 * 3];
            float[] colors = new float[size * 6 * 3];
            float[] texturCoords = new float[size * 6 * 2];
            float[] texBottomLeft = new float[size * 6 * 2];


            for(int i=0;i<visUnits.Count;i++)
            {
                Unit current = visUnits[i];
                //buffer indices
                int coord = i * 6 * 3;
                int texCoord = i * 6 * 2;

                if (current == null)
                    continue;

                float unitSize = current.Range*2*nodeSize;

                //tile position
                float bottom = (current.Pos.Y - viewTop - 0.5f) * unitSize;
                float top = (current.Pos.Y - viewTop + 1 - 0.5f) * unitSize;
                float left = (current.Pos.X - viewLeft - 0.5f) * unitSize;
                float right = (current.Pos.X - viewLeft + 1 - 0.5f) * unitSize;

                SetSquareVertices(vertices, bottom, top, left, right, -1, coord);
                SetColor(colors, 1f, 1f, 1f, coord,6);
                SetSquareTextureCoordinates(texturCoords, texCoord);
                SetTexBottomLeft(texBottomLeft, blX, blY, texCoord,6);
            }

            //bind map.VertexBufferArray and set data of its buffers
            units.VertexBufferArray.Bind(gl);

            units.VertexDataBuffer.Bind(gl);
            units.VertexDataBuffer.SetData(gl, attributeIndexPosition, vertices, false, 3);

            units.ColorDataBuffer.Bind(gl);
            units.ColorDataBuffer.SetData(gl, attributeIndexcolor, colors, false, 3);

            units.TextureDataBuffer.Bind(gl);
            units.TextureDataBuffer.SetData(gl, attributeIndexTexCoord, texturCoords, false, 2);

            units.TexBLDataBuffer.Bind(gl);
            units.TexBLDataBuffer.SetData(gl, attributeIndexTexBL, texBottomLeft, false, 2);
        }

        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateFlowMap(OpenGL gl)
        {
            flowMap = new BufferArray();

            //initialize map.VertexBufferArray and link it to its VertexBuffers
            flowMap.VertexBufferArray = new VertexBufferArray();
            flowMap.VertexBufferArray.Create(gl);
            flowMap.VertexBufferArray.Bind(gl);

            //initialize empty map.VertexDataBuffer
            flowMap.VertexDataBuffer = new VertexBuffer();
            flowMap.VertexDataBuffer.Create(gl);

            //initialize empty map.ColorDataBuffer
            flowMap.ColorDataBuffer = new VertexBuffer();
            flowMap.ColorDataBuffer.Create(gl);

            //initialize empty map.TextureDataBuffer
            flowMap.TextureDataBuffer = new VertexBuffer();
            flowMap.TextureDataBuffer.Create(gl);

            //initialize empty map.TexBLDataBuffer
            flowMap.TexBLDataBuffer = new VertexBuffer();
            flowMap.TexBLDataBuffer.Create(gl);

            flowMap.VertexBufferArray.Unbind(gl);
        }
        
        /// <summary>
        /// Updates buffers of vertexArrayBuffer with the information about the flow map
        /// from mapView.
        /// </summary>
        /// <param name="gl">Instance of OpenGL.</param>
        /// <param name="mapView">Map view describing the map.</param>
        public static void UpdateFlowMapDataBuffers(OpenGL gl, MapView mapView)
        {
            float nodeSize;
            float viewLeft;
            float viewTop;
            float viewBottom;
            float viewRight;
            float[,] flowM;
            //mapView has to be locked because it can also be accessed from the main game loop
            lock (mapView)
            {
                nodeSize = mapView.NodeSize;
                viewLeft = mapView.Left;
                viewTop = mapView.Bottom;
                viewBottom = mapView.Top;
                viewRight = mapView.Right;
                flowM = mapView.GetVisibleFlowMap();
            }

            int width = flowM.GetLength(0);
            int height = flowM.GetLength(1);
            float blX = 1;
            float blY = 1;

            //extents of one rectangle
            float sqW = nodeSize;
            float sqH = nodeSize;

            float[] vertices = new float[width * height * 3 * 3];
            float[] colors = new float[width * height * 3 * 3];
            float[] textureCoords = new float[width * height * 3 * 2];
            float[] texBottomLeft = new float[width * height * 3 * 2];

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

                    float angle = flowM[i, j];

                    vec2 leftBottom =new vec2((i - (viewLeft % 1)), (j - (viewTop % 1)));
                    vec2 squareExt = new vec2(sqW, sqH);

                    vec2 rotTriLB = (Rotate(triLB,angle) + triOffset + leftBottom) * squareExt;
                    vec2 rotTriLT = (Rotate(triLT, angle) + triOffset + leftBottom) * squareExt;
                    vec2 rotTriRM = (Rotate(triRM, angle) + triOffset + leftBottom) * squareExt;

                    int offset = 0;
                    int texOffset = 0;
                    if (!FlowMap.IsValidValue(angle))
                    {
                        vertices[coord + 0] = vertices[coord + 1] = vertices[coord + 2]
                            = vertices[coord + 3] = vertices[coord + 4] = vertices[coord + 5] = 0;
                    }
                    else
                    {
                        //bottom left
                        vertices[coord + offset + 0] = rotTriLB.x;
                        vertices[coord + offset + 1] = rotTriLB.y;
                        vertices[coord + offset + 2] = -1;

                        offset += 3;

                        //top left
                        vertices[coord + offset + 0] = rotTriLT.x;
                        vertices[coord + offset + 1] = rotTriLT.y;
                        vertices[coord + offset + 2] = -1;

                        offset += 3;

                        //top right
                        vertices[coord + offset + 0] = rotTriRM.x;
                        vertices[coord + offset + 1] = rotTriRM.y;
                        vertices[coord + offset + 2] = -1;

                    }

                    //bottom left
                    textureCoords[texCoord + texOffset + 0] = 0;
                    textureCoords[texCoord + texOffset + 1] = 0;

                    texOffset += 2;

                    //top left
                    textureCoords[texCoord + texOffset + 0] = 0;
                    textureCoords[texCoord + texOffset + 1] = 1;

                    texOffset += 2;

                    //top right
                    textureCoords[texCoord + texOffset + 0] = 1;
                    textureCoords[texCoord + texOffset + 1] = 0.5f;

                    SetColor(colors, 0f, 0f, 0f, coord,3);
                    SetTexBottomLeft(texBottomLeft, blX, blY, texCoord,3);
                }
            }

            //bind map.VertexBufferArray and set data of its buffers
            flowMap.VertexBufferArray.Bind(gl);

            flowMap.VertexDataBuffer.Bind(gl);
            flowMap.VertexDataBuffer.SetData(gl, attributeIndexPosition, vertices, false, 3);

            flowMap.ColorDataBuffer.Bind(gl);
            flowMap.ColorDataBuffer.SetData(gl, attributeIndexcolor, colors, false, 3);

            flowMap.TextureDataBuffer.Bind(gl);
            flowMap.TextureDataBuffer.SetData(gl, attributeIndexTexCoord, textureCoords, false, 2);

            flowMap.TexBLDataBuffer.Bind(gl);
            flowMap.TexBLDataBuffer.SetData(gl, attributeIndexTexBL, texBottomLeft, false, 2);
        }

        private static vec2 Rotate(vec2 vec, float angle)
        {
            float cosA = (float)Math.Cos(angle);
            float sinA = (float)Math.Sin(angle);
            return new vec2(cosA * vec.x - sinA * vec.y,
                            sinA * vec.x + cosA * vec.y);
        }
    }
}
