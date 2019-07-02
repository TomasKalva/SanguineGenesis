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
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    static class OpenGLTextureBufferDrawer
    {
        //  The projection, view and model matrices.
        static mat4 projectionMatrix;
        static mat4 viewMatrix;
        static mat4 modelMatrix;

        //  Constants that specify the attribute indexes.
        const uint attributeIndexPosition = 0;
        const uint attributeIndexcolor = 1;
        const uint attributeIndexTexCoord = 2;

        //  The vertex buffer array which contains the vertex and color buffers.
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

            //  Set a blue clear color.
            gl.ClearColor(0.4f, 0.6f, 0.9f, 0.0f);

            //  Create the shader program.
            var vertexShaderSource = ManifestResourceLoader.LoadTextFile("TexShader.vert");
            var fragmentShaderSource = ManifestResourceLoader.LoadTextFile("TexShader.frag");
            shaderProgram = new ShaderProgram();
            shaderProgram.Create(gl, vertexShaderSource, fragmentShaderSource, null);
            shaderProgram.BindAttributeLocation(gl, attributeIndexPosition, "in_Position");
            shaderProgram.BindAttributeLocation(gl, attributeIndexcolor, "in_Color");
            shaderProgram.BindAttributeLocation(gl, attributeIndexTexCoord, "in_TexCoord");
            shaderProgram.AssertValid(gl);

            //  Create a perspective projection matrix.
            projectionMatrix = glm.ortho(0f, width, 0f, height , 0f, 100f);// glm.perspective(rads, width / height, 0.1f, 100.0f); //


            //  Create a view matrix to move us back a bit.
            viewMatrix = glm.translate(new mat4(1.0f), new vec3(0.0f, 0.0f, -5.0f));

            //  Create a model matrix to make the model a little bigger.
            modelMatrix = glm.scale(new mat4(1.0f), new vec3(2.5f));

            InitializeTexture(gl);

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


            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[0]);
            CreateGrid(gl);

            //  Bind the out vertex array.
            vertexBufferArray.Bind(gl);

            //  Draw the square.
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 1000000);

            //  Unbind our vertex array and shader.
            vertexBufferArray.Unbind(gl);

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
            LoadTexture(0, "Crate.bmp", gl);
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


            Console.WriteLine(gl.IsEnabled(OpenGL.GL_TEXTURE_2D));


            //  Bind the texture.
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[i]);

            //  Tell OpenGL where the texture data is.
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, 3, textureImage.Width, textureImage.Height, 0, OpenGL.GL_BGR, OpenGL.GL_UNSIGNED_BYTE,
                textureImage.LockBits(new Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb).Scan0);
        }

        private static void CreateTexturedSquare(OpenGL gl)
        {
            float a = 1f;
            var vertices = new float[18];
            var colors = new float[18];
            var texturCoords = new float[12]; // Colors for our vertices  
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

            //  Now do the same for the color data.
            var colorDataBuffer = new VertexBuffer();
            colorDataBuffer.Create(gl);
            colorDataBuffer.Bind(gl);
            colorDataBuffer.SetData(gl, 1, colors, false, 3);

            //  Now do the same for the color data.
            var textureDataBuffer = new VertexBuffer();
            textureDataBuffer.Create(gl);
            textureDataBuffer.Bind(gl);
            textureDataBuffer.SetData(gl, 2, texturCoords, false, 2);

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(gl);
        }

        private static void CreateGrid(OpenGL gl)
        {
            int width = 10;
            int height = 10;

            float sqW = 30f;
            float sqH = 30f;

            var vertices = new float[width * height * 6 * 3];
            var colors = new float[width * height * 6 * 3];
            var texturCoords = new float[width * height * 6 * 2];

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

            //  Now do the same for the color data.
            var colorDataBuffer = new VertexBuffer();
            colorDataBuffer.Create(gl);
            colorDataBuffer.Bind(gl);
            colorDataBuffer.SetData(gl, 1, colors, false, 3);

            //  Now do the same for the color data.
            var textureDataBuffer = new VertexBuffer();
            textureDataBuffer.Create(gl);
            textureDataBuffer.Bind(gl);
            textureDataBuffer.SetData(gl, 2, texturCoords, false, 2);

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(gl);
        }
    }
}
