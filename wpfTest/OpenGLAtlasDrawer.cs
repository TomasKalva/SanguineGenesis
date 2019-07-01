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
        //  The projection, view and model matrices.
        static mat4 projectionMatrix;
        static mat4 viewMatrix;
        static mat4 modelMatrix;

        //  Constants that specify the attribute indexes.
        const uint attributeIndexPosition = 0;
        const uint attributeIndexColour = 1;
        const uint attributeIndexTexCoord = 2;
        const uint attributeIndexTexBL = 3;

        //  The vertex buffer array which contains the vertex and colour buffers.
        static VertexBufferArray vertexBufferArray;

        //  The shader program for our vertex and fragment shader.
        static private ShaderProgram shaderProgram;

        /// <summary>
        /// Initialises the scene.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        /// <param name="width">The width of the screen.</param>
        /// <param name="height">The height of the screen.</param>
        public static void Initialise(OpenGL gl, float width, float height)
        {

            //  Set a blue clear colour.
            gl.ClearColor(0.4f, 0.6f, 0.9f, 0.0f);

            //  Create the shader program.
            var vertexShaderSource = ManifestResourceLoader.LoadTextFile("AtlasTexShader.vert");
            var fragmentShaderSource = ManifestResourceLoader.LoadTextFile("AtlasTexShader.frag");
            shaderProgram = new ShaderProgram();
            shaderProgram.Create(gl, vertexShaderSource, fragmentShaderSource, null);
            shaderProgram.BindAttributeLocation(gl, attributeIndexPosition, "in_Position");
            shaderProgram.BindAttributeLocation(gl, attributeIndexColour, "in_Color");
            shaderProgram.BindAttributeLocation(gl, attributeIndexTexCoord, "in_TexCoord");
            shaderProgram.BindAttributeLocation(gl, attributeIndexTexBL, "in_TexBottomLeft");
            shaderProgram.AssertValid(gl);

            //  Create a perspective projection matrix.
            const float rads = (60.0f / 360.0f) * (float)Math.PI * 2.0f;
            projectionMatrix = glm.ortho(0f, width, height, 0f, 0f, 100f);// glm.perspective(rads, width / height, 0.1f, 100.0f); //


            //  Create a view matrix to move us back a bit.
            viewMatrix = glm.translate(new mat4(1.0f), new vec3(0.0f, 0.0f, -5.0f));

            //  Create a model matrix to make the model a little bigger.
            modelMatrix = glm.scale(new mat4(1.0f), new vec3(1f));


            InitializeTexture(gl);
            vertexBufferArray = new VertexBufferArray();
            //  Now create the geometry for the square.
            //CreateGrid(gl);
            CreateTexturedSquare(gl);
        }
        

        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        public static void Draw(OpenGL gl)
        {
            //  Clear the scene.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);
            
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //  Bind the shader, set the matrices.
            shaderProgram.Bind(gl);
            shaderProgram.SetUniformMatrix4(gl, "projectionMatrix", projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "viewMatrix", viewMatrix.to_array());
            shaderProgram.SetUniformMatrix4(gl, "modelMatrix", modelMatrix.to_array());

            shaderProgram.SetUniform3(gl, "atlasExtents", 640, 640, 0);
            shaderProgram.SetUniform3(gl, "texExtents", 62, 62, 0);

            //CreateGrid(gl);
            /*int width = 100;
            int height = 100;
            float[] vertices = new float[width * height * 6 * 3];
            for (int i = 0; i < width * height * 6 * 3; i++)
                vertices[i] = 0f;*/
            /*vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, 0, vertices, false, 3);*/
            //  Bind the out vertex array.
            vertexBufferArray.Bind(gl);
            //gl.BindVertexArray(vertArr[0]);
            //Triangle t = new Triangle(gl, new SolidColorBrush(System.Windows.Media.Colors.Orange));
            //t.NotMyVertexBufferArray.Bind(gl);
            //gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 3);

            //  Draw the square.
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 10000);


            //  Unbind our vertex array and shader.
            //vertexBufferArray.Unbind(gl);

            /*gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[1]);
            
            CreateTexturedSquare(gl);

            vertexBufferArray.Bind(gl);

            //  Draw the square.
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 1000000);

            //  Unbind our vertex array and shader.
            vertexBufferArray.Unbind(gl);*/

            shaderProgram.Unbind(gl);
            sw.Stop();
            //Console.WriteLine(sw.Elapsed.Milliseconds);

        }

        private static void InitializeTexture(OpenGL gl)
        {
            /*
            //  We need to load the texture from file.
            textureImage = new Bitmap("Crate.bmp");

            //  A bit of extra initialisation here, we have to enable textures.
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            Console.WriteLine(gl.IsEnabled(OpenGL.GL_TEXTURE_2D));

            //  Get one texture id, and stick it into the textures array.
            gl.GenTextures(1, textures);

            //  Bind the texture.
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[0]);

            //  Tell OpenGL where the texture data is.
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, 3, textureImage.Width, textureImage.Height, 0, OpenGL.GL_BGR, OpenGL.GL_UNSIGNED_BYTE,
                textureImage.LockBits(new Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb).Scan0);*/

            //  A bit of extra initialisation here, we have to enable textures.
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            //  Get one texture id, and stick it into the textures array.
            gl.GenTextures(1, textures);
            LoadTexture(0, "tileMap.png", gl);
            //LoadTexture(1, "Red.png", gl);

            //gl.GenerateMipmapEXT(OpenGL.GL_TEXTURE_2D);

            //  Specify linear filtering.
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
        }

        //  The texture identifier.
        static uint[] textures = new uint[2];

        //  Storage the texture itself.
        static Bitmap textureImage;

        private static void LoadTexture(int i, String fileName, OpenGL gl)
        {
            //  We need to load the texture from file.
            textureImage = new Bitmap(fileName);
            textureImage.RotateFlip(RotateFlipType.RotateNoneFlipY);


            Console.WriteLine(gl.IsEnabled(OpenGL.GL_TEXTURE_2D));


            //  Bind the texture.
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[i]);

            //  Tell OpenGL where the texture data is.
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, 3, textureImage.Width, textureImage.Height, 0, OpenGL.GL_BGR, OpenGL.GL_UNSIGNED_BYTE,
                textureImage.LockBits(new Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb).Scan0);
        }

        private static void CreateTexturedSquare(OpenGL gl)
        {
            float a = 300f;
            var vertices = new float[18];
            var colors = new float[18];
            var texturCoords = new float[12];
            var texBottomLeft = new float[12];
            for (int i = 0; i < texBottomLeft.Length; i++)
            {
                texBottomLeft[i] = 32;
            }
            vertices[0] = -0.5f * a; vertices[1] = -0.5f * a; vertices[2] = 0.0f * a; // Bottom left corner  
            colors[0] = 1.0f; colors[1] = 1.0f; colors[2] = 1.0f; // Bottom left corner  
            texturCoords[0] = 0.0f; texturCoords[1] = 0.0f;// Bottom left corner  
            vertices[3] = -0.5f * a; vertices[4] = 0.5f * a; vertices[5] = 0.0f * a; // Top left corner  
            colors[3] = 1.0f; colors[4] = 0.0f; colors[5] = 0.0f; // Top left corner  
            texturCoords[2] = 0.0f; texturCoords[3] = 1.0f;// Top left corner  
            vertices[6] = 0.5f * a; vertices[7] = 0.5f * a; vertices[8] = 0.0f * a; // Top Right corner 
            colors[6] = 0.0f; colors[7] = 1.0f; colors[8] = 0.0f; // Top Right corner   
            texturCoords[4] = 1.0f; texturCoords[5] = 1.0f;  // Top Right corner  
            vertices[9] = 0.5f * a; vertices[10] = -0.5f * a; vertices[11] = 0.0f * a; // Bottom right corner  
            colors[9] = 0.0f; colors[10] = 0.0f; colors[11] = 1.0f; // Bottom right corner  
            texturCoords[6] = 1.0f; texturCoords[7] = 0.0f; // Bottom right corner  
            vertices[12] = -0.5f * a; vertices[13] = -0.5f * a; vertices[14] = 0.0f * a; // Bottom left corner 
            colors[12] = 1.0f; colors[13] = 1.0f; colors[14] = 1.0f; // Bottom left corner   
            texturCoords[8] = 0.0f; texturCoords[9] = 0.0f;// Bottom left corner  
            vertices[15] = 0.5f * a; vertices[16] = 0.5f * a; vertices[17] = 0.0f * a; // Top Right corner  
            colors[15] = 0.0f; colors[16] = 1.0f; colors[17] = 0.0f; // Top Right corner 
            texturCoords[10] = 1.0f; texturCoords[11] = 1.0f; // Top Right corner 

            //  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);

            //  Create a vertex buffer for the vertex data.
            var vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(gl);
            vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, 0, vertices, false, 3);

            //  Now do the same for the colour data.
            var colourDataBuffer = new VertexBuffer();
            colourDataBuffer.Create(gl);
            colourDataBuffer.Bind(gl);
            colourDataBuffer.SetData(gl, 1, colors, false, 3);

            //  Now do the same for the colour data.
            var textureDataBuffer = new VertexBuffer();
            textureDataBuffer.Create(gl);
            textureDataBuffer.Bind(gl);
            textureDataBuffer.SetData(gl, 2, texturCoords, false, 2);

            //  Now do the same for the colour data.
            var texBLDataBuffer = new VertexBuffer();
            texBLDataBuffer.Create(gl);
            texBLDataBuffer.Bind(gl);
            texBLDataBuffer.SetData(gl, 3, texBottomLeft, false, 2);

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(gl);
        }

        private static void CreateGrid(OpenGL gl)
        {
            int width = 10;
            int height = 10;

            float blX = 10;
            float blY = 20;

            float sqW = 30f;
            float sqH = 30f;

            var vertices = new float[width * height * 6 * 3];
            var colors = new float[width * height * 6 * 3];
            var texturCoords = new float[width * height * 6 * 2];
            var texBottomLeft = new float[width * height * 6 * 2];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int coord = (i + j * width) * 6 * 3;
                    int offset = 0;
                    int texCoord = (i + j * width) * 6 * 2;
                    int texOffset = 0;
                    //bottom left
                    vertices[coord + offset + 0] = i * sqW;
                    vertices[coord + offset + 1] = j * sqH;
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
                    vertices[coord + offset + 0] = i * sqW;
                    vertices[coord + offset + 1] = (j + 1) * sqH;
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
                    vertices[coord + offset + 0] = (i + 1) * sqW;
                    vertices[coord + offset + 1] = (j + 1) * sqH;
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
                    vertices[coord + offset + 0] = (i + 1) * sqW;
                    vertices[coord + offset + 1] = j * sqH;
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
                    vertices[coord + offset + 0] = i * sqW;
                    vertices[coord + offset + 1] = j * sqH;
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
                    vertices[coord + offset + 0] = (i + 1) * sqW;
                    vertices[coord + offset + 1] = (j + 1) * sqH;
                    vertices[coord + offset + 2] = -1;

                    colors[coord + offset + 0] = i;
                    colors[coord + offset + 1] = 1;
                    colors[coord + offset + 2] = 0;

                    texturCoords[texCoord + texOffset + 0] = 1;
                    texturCoords[texCoord + texOffset + 1] = 1;

                    texBottomLeft[texCoord + texOffset + 0] = blX;
                    texBottomLeft[texCoord + texOffset + 1] = blY;

                    blX = (blX + 1) % 32;
                    blY = (blY + 1) % 32;
                }
            }

            //  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);

            //  Create a vertex buffer for the vertex data.
            var vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(gl);
            vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, 0, vertices, false, 3);

            //  Now do the same for the colour data.
            var colourDataBuffer = new VertexBuffer();
            colourDataBuffer.Create(gl);
            colourDataBuffer.Bind(gl);
            colourDataBuffer.SetData(gl, 1, colors, false, 3);

            //  Now do the same for the colour data.
            var textureDataBuffer = new VertexBuffer();
            textureDataBuffer.Create(gl);
            textureDataBuffer.Bind(gl);
            textureDataBuffer.SetData(gl, 2, texturCoords, false, 2);

            //  Now do the same for the colour data.
            var texBLDataBuffer = new VertexBuffer();
            texBLDataBuffer.Create(gl);
            texBLDataBuffer.Bind(gl);
            texBLDataBuffer.SetData(gl, 3, texBottomLeft, false, 2);

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(gl);
        }

        public static void CreateMap(OpenGL gl, MapView mapView)
        {
            float nodeSize;
            float viewLeft;
            float viewTop;
            float viewBottom;
            float viewRight;
            Node[,] visible;
            lock (mapView)
            {
                nodeSize = mapView.NodeSize;
                viewLeft = mapView.Left;
                viewTop = mapView.Top;
                viewBottom = mapView.Bottom;
                viewRight = mapView.Right;
                visible = mapView.GetVisibleNodes();
            }

            /*for(int i = 0; i < visible.GetLength(0); i++)
            {
                for (int j = 0; j < visible.GetLength(1); j++)
                {

                }
            }
            {
                float x = node.X - viewLeft;
                float y = node.Y - viewTop;
                Canvas.SetLeft(square, x * (nodeSize));
                Canvas.SetTop(square, y * (nodeSize));
            }*/
            int width = visible.GetLength(0);
            int height = visible.GetLength(1);

            float blX = 1;
            float blY = 1;

            float sqW = nodeSize;
            float sqH = nodeSize;

            vertices = new float[width * height * 6 * 3];
            var colors = new float[width * height * 6 * 3];
            var texturCoords = new float[width * height * 6 * 2];
            var texBottomLeft = new float[width * height * 6 * 2];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
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

                    //blX = (blX + 1) % 32;
                    //blY = (blY + 1) % 32;
                }
            }

            //  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);

            //  Create a vertex buffer for the vertex data.
            vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(gl);
            vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, 0, vertices, false, 3);
            //gl.GenBuffers(1, vertBuff);
            //gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vertBuff[0]);

            //  Now do the same for the colour data.
            colourDataBuffer = new VertexBuffer();
            colourDataBuffer.Create(gl);
            colourDataBuffer.Bind(gl);
            colourDataBuffer.SetData(gl, 1, colors, false, 3);

            //  Now do the same for the colour data.
            textureDataBuffer = new VertexBuffer();
            textureDataBuffer.Create(gl);
            textureDataBuffer.Bind(gl);
            textureDataBuffer.SetData(gl, 2, texturCoords, false, 2);

            //  Now do the same for the colour data.
            texBLDataBuffer = new VertexBuffer();
            texBLDataBuffer.Create(gl);
            texBLDataBuffer.Bind(gl);
            texBLDataBuffer.SetData(gl, 3, texBottomLeft, false, 2);


            /*for (int i = 0; i < width * height * 6 * 3; i++)
                vertices[i] = 0f;*/
            vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, 0, vertices, false, 3);

            /*gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vertBuff[0]);
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, vertices, OpenGL.GL_STATIC_DRAW);
            IntPtr verticesPtr = GCHandle.Alloc(vertices, GCHandleType.Pinned).AddrOfPinnedObject();
            gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 3 * sizeof(float), verticesPtr);
            gl.EnableVertexAttribArray(0);*/

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(gl);

            //AlternativeInitialization(vertices, colors, texturCoords, texBottomLeft, gl);
        }

        public static void UpdateMapDataBuffers(OpenGL gl, MapView mapView)
        {
            float nodeSize;
            float viewLeft;
            float viewTop;
            float viewBottom;
            float viewRight;
            Node[,] visible;
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
            //  Create the vertex array object.
            vertexBufferArray.Bind(gl);

            //  Create a vertex buffer for the vertex data.
            vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, 0, vertices, false, 3);
            //gl.GenBuffers(1, vertBuff);
            //gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vertBuff[0]);

            //  Now do the same for the colour data.
            colourDataBuffer.Bind(gl);
            colourDataBuffer.SetData(gl, 1, colors, false, 3);

            //  Now do the same for the colour data.
            textureDataBuffer.Bind(gl);
            textureDataBuffer.SetData(gl, 2, texturCoords, false, 2);

            //  Now do the same for the colour data.
            texBLDataBuffer.Bind(gl);
            texBLDataBuffer.SetData(gl, 3, texBottomLeft, false, 2);
        }

        public static VertexBuffer vertexDataBuffer;
        public static VertexBuffer colourDataBuffer;
        public static VertexBuffer textureDataBuffer;
        public static VertexBuffer texBLDataBuffer;

        private static float[] vertices;


        private static uint[] vertArr = new uint[1];
        private static uint[] vertBuff = new uint[1];
        private static uint[] colBuff = new uint[1];
        private static uint[] texBuff = new uint[1];
        private static uint[] blTexBuff = new uint[1];

        private static void AlternativeInitialization(float[] vertices, float[] colors, float[] tex, float[] blTex, OpenGL gl)
        {


            //  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);
            gl.GenVertexArrays(1, vertArr);
            gl.GenBuffers(1, vertBuff);
            gl.GenBuffers(1, colBuff);
            gl.GenBuffers(1, texBuff);
            gl.GenBuffers(1, blTexBuff);

            gl.BindVertexArray(vertArr[0]);

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vertBuff[0]);
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, vertices, OpenGL.GL_STATIC_DRAW);
            IntPtr verticesPtr = GCHandle.Alloc(vertices, GCHandleType.Pinned).AddrOfPinnedObject();
            gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 3 * sizeof(float), verticesPtr);
            gl.EnableVertexAttribArray(0);

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, colBuff[0]);
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, colors, OpenGL.GL_STATIC_DRAW);
            IntPtr colorsPtr = GCHandle.Alloc(colors, GCHandleType.Pinned).AddrOfPinnedObject();
            gl.VertexAttribPointer(1, 3, OpenGL.GL_FLOAT, false, 3 * sizeof(float), colorsPtr);
            gl.EnableVertexAttribArray(1);

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, texBuff[0]);
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, tex, OpenGL.GL_STATIC_DRAW);
            IntPtr texPtr = GCHandle.Alloc(tex, GCHandleType.Pinned).AddrOfPinnedObject();
            gl.VertexAttribPointer(2, 2, OpenGL.GL_FLOAT, false, 2 * sizeof(float), texPtr);
            gl.EnableVertexAttribArray(2);

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, blTexBuff[0]);
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, blTex, OpenGL.GL_STATIC_DRAW);
            IntPtr blTexPtr = GCHandle.Alloc(blTex, GCHandleType.Pinned).AddrOfPinnedObject();
            gl.VertexAttribPointer(3, 2, OpenGL.GL_FLOAT, false, 2 * sizeof(float), blTexPtr);
            gl.EnableVertexAttribArray(3);
        }

        class Triangle
        {
            private readonly float[] colors = new float[9];

            private readonly ShaderProgram program;

            private readonly float[] trianglePoints =
            {
        0.0f,  50f,  0.0f,
        50f, -50f,  0.0f,
       -50f, -50f,  0.0f
        };

            private IntPtr colorsPtr;

            private IntPtr trianglePointsPtr;

            public readonly VertexBufferArray NotMyVertexBufferArray;

            public Triangle(OpenGL openGl, SolidColorBrush solidColorBrush)
            {

                for (var i = 0; i < this.colors.Length; i += 3)
                {
                    this.colors[i] = solidColorBrush.Color.R / 255.0f;
                    this.colors[i + 1] = solidColorBrush.Color.G / 255.0f;
                    this.colors[i + 2] = solidColorBrush.Color.B / 255.0f;
                }

                this.NotMyVertexBufferArray = new VertexBufferArray();
                this.NotMyVertexBufferArray.Create(openGl);
                this.NotMyVertexBufferArray.Bind(openGl);

                var colorsVboArray = new uint[1];
                openGl.GenBuffers(1, colorsVboArray);
                openGl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, colorsVboArray[0]);
                this.colorsPtr = GCHandle.Alloc(this.colors, GCHandleType.Pinned).AddrOfPinnedObject();
                openGl.BufferData(OpenGL.GL_ARRAY_BUFFER, this.colors.Length * Marshal.SizeOf<float>(), this.colorsPtr,
                    OpenGL.GL_STATIC_DRAW);

                var triangleVboArray = new uint[1];
                openGl.GenBuffers(1, triangleVboArray);
                openGl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, triangleVboArray[0]);
                this.trianglePointsPtr = GCHandle.Alloc(this.trianglePoints, GCHandleType.Pinned).AddrOfPinnedObject();
                openGl.BufferData(OpenGL.GL_ARRAY_BUFFER, this.trianglePoints.Length * Marshal.SizeOf<float>(), this.trianglePointsPtr,
                    OpenGL.GL_STATIC_DRAW);

                openGl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, triangleVboArray[0]);
                openGl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                openGl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, colorsVboArray[0]);
                openGl.VertexAttribPointer(1, 3, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);

                openGl.EnableVertexAttribArray(0);
                openGl.EnableVertexAttribArray(1);
            }
        }
    }
}
