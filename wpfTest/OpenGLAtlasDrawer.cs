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

        //the vertex buffer array which contains the buffers for vertex, 
        //color, texture and bottom left coordinates of textures
        static VertexBufferArray vertexBufferArray;
        public static VertexBuffer vertexDataBuffer;
        public static VertexBuffer colorDataBuffer;
        public static VertexBuffer textureDataBuffer;
        public static VertexBuffer texBLDataBuffer;


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
            projectionMatrix = glm.ortho(0f, width, height, 0f, 0f, 100f);

            //create view matrix that translates graphical objects to visible range
            viewMatrix = glm.translate(new mat4(1.0f), new vec3(0.0f, 0.0f, -1.0f));

            //create identity matrix as model matrix
            modelMatrix = new mat4(1.0f);

            //load image used as atlas and pass it to the shader program
            InitializeAtlas(gl);
        }
        

        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        public static void Draw(OpenGL gl)
        {
            //clear the scene
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);
            
            //bind the shader program and set its parameters
            shaderProgram.Bind(gl);
            shaderProgram.SetUniformMatrix4(gl, "projectionMatrix", projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "viewMatrix", viewMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "modelMatrix", modelMatrix.to_array());
            shaderProgram.SetUniform3(gl, "atlasExtents", 640, 640, 0);
            shaderProgram.SetUniform3(gl, "texExtents", 62, 62, 0);
            
            vertexBufferArray.Bind(gl);

            //draw vertex array buffers
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 10000);

            shaderProgram.Unbind(gl);

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

        /// <summary>
        /// Creates vertex buffer array and its buffers for gl.
        /// </summary>
        /// <param name="gl">The instance of OpenGL.</param>
        public static void CreateMap(OpenGL gl)
        {
            //initialize vertexBufferArray and link it to its VertexBuffers
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);

            //initialize empty vertexDataBuffer
            vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(gl);

            //initialize empty colorDataBuffer
            colorDataBuffer = new VertexBuffer();
            colorDataBuffer.Create(gl);

            //initialize empty textureDataBuffer
            textureDataBuffer = new VertexBuffer();
            textureDataBuffer.Create(gl);

            //initialize empty texBLDataBuffer
            texBLDataBuffer = new VertexBuffer();
            texBLDataBuffer.Create(gl);
            
            vertexBufferArray.Unbind(gl);
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
                viewTop = mapView.Top;
                viewBottom = mapView.Bottom;
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

            float[] vertices = new float[width * height * 6 * 3];
            float[] colors = new float[width * height * 6 * 3];
            float[] texturCoords = new float[width * height * 6 * 2];
            float[] texBottomLeft = new float[width * height * 6 * 2];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //buffer indices
                    int coord = (i + j * width) * 6 * 3;
                    int offset = 0;
                    int texCoord = (i + j * width) * 6 * 2;
                    int texOffset = 0;

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
                    float bottom = (visible[i, j].Y - viewTop + 1) * sqH;
                    float top = (visible[i, j].Y - viewTop) * sqH;
                    float left = (visible[i, j].X - viewLeft) * sqW;
                    float right = (visible[i, j].X - viewLeft + 1) * sqW;

                    //bottom left
                    vertices[coord + offset + 0] = left;
                    vertices[coord + offset + 1] = bottom;
                    vertices[coord + offset + 2] = -1;

                    colors[coord + offset + 0] = i;
                    colors[coord + offset + 1] = 1;
                    colors[coord + offset + 2] = 0;

                    texturCoords[texCoord + texOffset + 0] = 0;
                    texturCoords[texCoord + texOffset + 1] = 0;

                    texBottomLeft[texCoord + texOffset + 0] = blX;
                    texBottomLeft[texCoord + texOffset + 1] = blY;

                    offset += 3;
                    texOffset += 2;

                    //top left
                    vertices[coord + offset + 0] = left;
                    vertices[coord + offset + 1] = top;
                    vertices[coord + offset + 2] = -1;

                    colors[coord + offset + 0] = i;
                    colors[coord + offset + 1] = 1;
                    colors[coord + offset + 2] = 0;

                    texturCoords[texCoord + texOffset + 0] = 0;
                    texturCoords[texCoord + texOffset + 1] = 1;

                    texBottomLeft[texCoord + texOffset + 0] = blX;
                    texBottomLeft[texCoord + texOffset + 1] = blY;

                    offset += 3;
                    texOffset += 2;

                    //top right
                    vertices[coord + offset + 0] = right;
                    vertices[coord + offset + 1] = top;
                    vertices[coord + offset + 2] = -1;

                    colors[coord + offset + 0] = i;
                    colors[coord + offset + 1] = 1;
                    colors[coord + offset + 2] = 0;

                    texturCoords[texCoord + texOffset + 0] = 1;
                    texturCoords[texCoord + texOffset + 1] = 1;

                    texBottomLeft[texCoord + texOffset + 0] = blX;
                    texBottomLeft[texCoord + texOffset + 1] = blY;

                    offset += 3;
                    texOffset += 2;

                    //bottom right
                    vertices[coord + offset + 0] = right;
                    vertices[coord + offset + 1] = bottom;
                    vertices[coord + offset + 2] = -1;

                    colors[coord + offset + 0] = i;
                    colors[coord + offset + 1] = 1;
                    colors[coord + offset + 2] = 0;

                    texturCoords[texCoord + texOffset + 0] = 1;
                    texturCoords[texCoord + texOffset + 1] = 0;

                    texBottomLeft[texCoord + texOffset + 0] = blX;
                    texBottomLeft[texCoord + texOffset + 1] = blY;

                    offset += 3;
                    texOffset += 2;

                    //bottom left
                    vertices[coord + offset + 0] = left;
                    vertices[coord + offset + 1] = bottom;
                    vertices[coord + offset + 2] = -1;

                    colors[coord + offset + 0] = i;
                    colors[coord + offset + 1] = 1;
                    colors[coord + offset + 2] = 0;

                    texturCoords[texCoord + texOffset + 0] = 0;
                    texturCoords[texCoord + texOffset + 1] = 0;

                    texBottomLeft[texCoord + texOffset + 0] = blX;
                    texBottomLeft[texCoord + texOffset + 1] = blY;

                    offset += 3;
                    texOffset += 2;

                    //top right
                    vertices[coord + offset + 0] = right;
                    vertices[coord + offset + 1] = top;
                    vertices[coord + offset + 2] = -1;

                    colors[coord + offset + 0] = i;
                    colors[coord + offset + 1] = 1;
                    colors[coord + offset + 2] = 0;

                    texturCoords[texCoord + texOffset + 0] = 1;
                    texturCoords[texCoord + texOffset + 1] = 1;

                    texBottomLeft[texCoord + texOffset + 0] = blX;
                    texBottomLeft[texCoord + texOffset + 1] = blY;
                }
            }

            //bind vertexBufferArray and set data of its buffers
            vertexBufferArray.Bind(gl);
            
            vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, attributeIndexPosition, vertices, false, 3);
            
            colorDataBuffer.Bind(gl);
            colorDataBuffer.SetData(gl, attributeIndexcolor, colors, false, 3);
            
            textureDataBuffer.Bind(gl);
            textureDataBuffer.SetData(gl, attributeIndexTexCoord, texturCoords, false, 2);
            
            texBLDataBuffer.Bind(gl);
            texBLDataBuffer.SetData(gl, attributeIndexTexBL, texBottomLeft, false, 2);
        }
    }
}
