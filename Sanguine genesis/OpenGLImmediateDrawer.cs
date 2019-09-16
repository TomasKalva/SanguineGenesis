using SharpGL;
using SharpGL.Enumerations;
using SharpGL.SceneGraph.Assets;
using SharpGL.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    static class OpenGLImmediateDrawer
    {

        private static void Draw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            //  Get the OpenGL object, for quick access.
            OpenGL gl = args.OpenGL;

            //  Clear the color and depth buffers.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            
            //DrawSquareOGL(gl, 0, 0);
            //TextureVsColorMeasuring(gl);

            //  Flush OpenGL.
            gl.Flush();
        }

        private static void TextureVsColorMeasuring(OpenGL gl)
        {
            //Orthographic(gl);
            int width = 20;
            int height = 10;

            float squareSide = 20f;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    //DrawSquareColorOrographic(gl, i, j, squareSide, squareSide);
                    DrawSquareOGL(gl, i, j, squareSide, squareSide);
            sw.Stop();
            Console.WriteLine("Drawing shapes took: " + sw.Elapsed.Milliseconds);

            sw.Reset();
            sw.Start();

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    //DrawSquareTextureOrographic(gl, i, j, squareSide, squareSide);
                    DrawTextureSquareOGL(gl, i, j, squareSide, squareSide);
            sw.Stop();
            Console.WriteLine("Drawing textures took: " + sw.Elapsed.Milliseconds);
        }

        private static void OpenGlInitialize(OpenGLControl openGLControl1)
        {
            //  Get the OpenGL object, for quick access.
            OpenGL gl = openGLControl1.OpenGL;
            openGLControl1.FrameRate = 30;

            //  A bit of extra initialisation here, we have to enable textures.
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            //  Create our texture object from a file. This creates the texture for OpenGL.
            texture.Create(gl, "Grass.png");
        }

        static Texture texture = new Texture();
        static float rotatePyramid = 0;
        static float rquad = 0;

        private static void DrawCubeOGL(OpenGL gl, float x, float y)
        {
            //  Reset the modelview matrix.
            gl.LoadIdentity();

            //  Move into a more central position.
            gl.Translate(x, y, -70.0f);

            //  Rotate the cube.
            gl.Rotate(rquad, 1.0f, 1.0f, 1.0f);

            //  Provide the cube colors and geometry.
            gl.Begin(OpenGL.GL_QUADS);

            gl.Color(0.0f, 1.0f, 0.0f);
            gl.Vertex(1.0f, 1.0f, -1.0f);
            gl.Vertex(-1.0f, 1.0f, -1.0f);
            gl.Vertex(-1.0f, 1.0f, 1.0f);
            gl.Vertex(1.0f, 1.0f, 1.0f);

            gl.Color(1.0f, 0.5f, 0.0f);
            gl.Vertex(1.0f, -1.0f, 1.0f);
            gl.Vertex(-1.0f, -1.0f, 1.0f);
            gl.Vertex(-1.0f, -1.0f, -1.0f);
            gl.Vertex(1.0f, -1.0f, -1.0f);

            gl.Color(1.0f, 0.0f, 0.0f);
            gl.Vertex(1.0f, 1.0f, 1.0f);
            gl.Vertex(-1.0f, 1.0f, 1.0f);
            gl.Vertex(-1.0f, -1.0f, 1.0f);
            gl.Vertex(1.0f, -1.0f, 1.0f);

            gl.Color(1.0f, 1.0f, 0.0f);
            gl.Vertex(1.0f, -1.0f, -1.0f);
            gl.Vertex(-1.0f, -1.0f, -1.0f);
            gl.Vertex(-1.0f, 1.0f, -1.0f);
            gl.Vertex(1.0f, 1.0f, -1.0f);

            gl.Color(0.0f, 0.0f, 1.0f);
            gl.Vertex(-1.0f, 1.0f, 1.0f);
            gl.Vertex(-1.0f, 1.0f, -1.0f);
            gl.Vertex(-1.0f, -1.0f, -1.0f);
            gl.Vertex(-1.0f, -1.0f, 1.0f);

            gl.Color(1.0f, 0.0f, 1.0f);
            gl.Vertex(1.0f, 1.0f, -1.0f);
            gl.Vertex(1.0f, 1.0f, 1.0f);
            gl.Vertex(1.0f, -1.0f, 1.0f);
            gl.Vertex(1.0f, -1.0f, -1.0f);

            gl.End();

        }

        static float Distance;

        private static void DrawSquareOGL(OpenGL gl, float x, float y, float width, float height)
        {
            Distance = 5f;

            //  Reset the modelview matrix.
            gl.LoadIdentity();

            //  Move into a more central position.
            gl.Translate(x * width, y * height, -Distance);

            //  Rotate the cube.
            gl.Rotate(0, 1.0f, 1.0f, 1.0f);
            gl.Scale(width, height, 1f);

            //  Provide the cube colors and geometry.
            gl.Begin(OpenGL.GL_QUADS);

            gl.Color(0.0f, 1.0f, 0.0f);
            gl.Vertex(0f, 0f, 0);
            gl.Vertex(0f, 1f, 0);
            gl.Vertex(1f, 1f, 0);
            gl.Vertex(1f, 0f, 0);

            gl.End();
        }

        private static void DrawTextureSquareOGL(OpenGL gl, float x, float y, float width, float height)
        {
            Distance = 5f;

            //  Reset the modelview matrix.
            gl.LoadIdentity();

            //  Move into a more central position.
            gl.Translate(x * width, y * height, -Distance);

            //  Rotate the cube.
            gl.Rotate(0, 1.0f, 1.0f, 1.0f);
            gl.Scale(width, height, 1f);

            texture.Bind(gl);
            //  Provide the cube colors and geometry.
            gl.Begin(OpenGL.GL_QUADS);

            gl.TexCoord(0.0f, 1.0f); gl.Vertex(0f, 0f, 0);
            gl.TexCoord(0.0f, 0.0f); gl.Vertex(0f, 1f, 0);
            gl.TexCoord(1.0f, 0.0f); gl.Vertex(1f, 1f, 0);
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(1f, 0f, 0);

            gl.End();
        }

        public static void Orthographic(OpenGL gl, OpenGLControl openGLControl1)
        {
            gl.MatrixMode(MatrixMode.Projection);
            gl.LoadIdentity();

            // NOTE: Basically no matter what I do, the only points I see are those at
            // the "near" surface (with z = -zNear)--in this case, I only see green points
            gl.Ortho(0, openGLControl1.ActualWidth, openGLControl1.ActualHeight, 0, 1, 10);

            //  Back to the modelview.
            gl.MatrixMode(MatrixMode.Modelview);

        }

        private static void DrawTextureOGL(OpenGL gl)
        {
            gl.LoadIdentity();
            gl.Translate(0.0f, 0.0f, -6.0f);

            gl.Rotate(rtri, 0.0f, 1.0f, 0.0f);

            //  Bind the texture.
            texture.Bind(gl);

            gl.Begin(OpenGL.GL_QUADS);

            // Front Face
            gl.TexCoord(0.0f, 0.0f); gl.Vertex(-1.0f, -1.0f, 1.0f); // Bottom Left Of The Texture and Quad
            gl.TexCoord(1.0f, 0.0f); gl.Vertex(1.0f, -1.0f, 1.0f);  // Bottom Right Of The Texture and Quad
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(1.0f, 1.0f, 1.0f);   // Top Right Of The Texture and Quad
            gl.TexCoord(0.0f, 1.0f); gl.Vertex(-1.0f, 1.0f, 1.0f);  // Top Left Of The Texture and Quad

            // Back Face
            gl.TexCoord(1.0f, 0.0f); gl.Vertex(-1.0f, -1.0f, -1.0f);    // Bottom Right Of The Texture and Quad
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(-1.0f, 1.0f, -1.0f); // Top Right Of The Texture and Quad
            gl.TexCoord(0.0f, 1.0f); gl.Vertex(1.0f, 1.0f, -1.0f);  // Top Left Of The Texture and Quad
            gl.TexCoord(0.0f, 0.0f); gl.Vertex(1.0f, -1.0f, -1.0f); // Bottom Left Of The Texture and Quad

            // Top Face
            gl.TexCoord(0.0f, 1.0f); gl.Vertex(-1.0f, 1.0f, -1.0f); // Top Left Of The Texture and Quad
            gl.TexCoord(0.0f, 0.0f); gl.Vertex(-1.0f, 1.0f, 1.0f);  // Bottom Left Of The Texture and Quad
            gl.TexCoord(1.0f, 0.0f); gl.Vertex(1.0f, 1.0f, 1.0f);   // Bottom Right Of The Texture and Quad
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(1.0f, 1.0f, -1.0f);  // Top Right Of The Texture and Quad

            // Bottom Face
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(-1.0f, -1.0f, -1.0f);    // Top Right Of The Texture and Quad
            gl.TexCoord(0.0f, 1.0f); gl.Vertex(1.0f, -1.0f, -1.0f); // Top Left Of The Texture and Quad
            gl.TexCoord(0.0f, 0.0f); gl.Vertex(1.0f, -1.0f, 1.0f);  // Bottom Left Of The Texture and Quad
            gl.TexCoord(1.0f, 0.0f); gl.Vertex(-1.0f, -1.0f, 1.0f); // Bottom Right Of The Texture and Quad

            // Right face
            gl.TexCoord(1.0f, 0.0f); gl.Vertex(1.0f, -1.0f, -1.0f); // Bottom Right Of The Texture and Quad
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(1.0f, 1.0f, -1.0f);  // Top Right Of The Texture and Quad
            gl.TexCoord(0.0f, 1.0f); gl.Vertex(1.0f, 1.0f, 1.0f);   // Top Left Of The Texture and Quad
            gl.TexCoord(0.0f, 0.0f); gl.Vertex(1.0f, -1.0f, 1.0f);  // Bottom Left Of The Texture and Quad

            // Left Face
            gl.TexCoord(0.0f, 0.0f); gl.Vertex(-1.0f, -1.0f, -1.0f);    // Bottom Left Of The Texture and Quad
            gl.TexCoord(1.0f, 0.0f); gl.Vertex(-1.0f, -1.0f, 1.0f); // Bottom Right Of The Texture and Quad
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(-1.0f, 1.0f, 1.0f);  // Top Right Of The Texture and Quad
            gl.TexCoord(0.0f, 1.0f); gl.Vertex(-1.0f, 1.0f, -1.0f);	// Top Left Of The Texture and Quad
            gl.End();


            rtri += 1.0f;// 0.2f;
        }

        static float rtri = 0;
    }
}
