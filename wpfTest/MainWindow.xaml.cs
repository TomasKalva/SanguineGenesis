using SharpGL;
using SharpGL.Enumerations;
using SharpGL.SceneGraph.Assets;
using SharpGL.VertexBuffers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace wpfTest
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Game game;
        MapView mapView;
        MapMovementInput mapMovementInput;

        public MainWindow()
        {
            InitializeComponent();

            game = new Game("hugeMap", this);
            //mapImage.Source = game.Map.MapImage;
            mapView = new MapView(0, 0, 60, game.Map);
            mapMovementInput = new MapMovementInput();
            OpenGlInitialize();

            Thread t = new Thread(() => {
                Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    //wait for window layout initialization
                }));
                Dispatcher.Invoke(ResizeView);
                game.MainLoop();
            });
            t.IsBackground = true;
            t.Start();
            
        }

        public void Draw()
        {
            //update position of mapView
            foreach (Direction d in mapMovementInput.MapDirection)
                mapView.Move(d);

            //draw map
            //DrawMap();
            /*Canvas.SetLeft(testImage, -100);
            Canvas.SetTop(testImage, -100);*/
            //mapImage.Clip = new RectangleGeometry(new Rect(100,100,100,100));
            //DrawMap2();
        }

        public void DrawMap2()
        {
            mapView.SetActualExtents(tiles.ActualWidth, tiles.ActualHeight);
            Canvas.SetLeft(mapImage, -mapView.Left * game.Map.NodeSize);
            Canvas.SetTop(mapImage, -mapView.Top * game.Map.NodeSize);
            mapImage.Clip = mapView.GetViewGeometry();
        }

        /*private void DrawMap()
        {
            Node[,] map = game.Map.Nodes;

            mapView.SetActualExtents(tiles.ActualWidth, tiles.ActualHeight);

            double nodeSize = mapView.NodeSize;
            double viewLeft = mapView.Left;
            double viewTop = mapView.Top;
            IEnumerator tileGrid = tiles.Children.GetEnumerator();
            foreach (Node node in mapView)
            {
                if (!tileGrid.MoveNext())
                    break;
                Grid square = (Grid)tileGrid.Current;
                square.Visibility = Visibility.Visible;
                Rectangle r=(Rectangle)square.Children[0];
                r.Fill = Brushes.Blue;
                double x = node.X - viewLeft;
                double y = node.Y - viewTop;
                Canvas.SetLeft(square, x * (nodeSize));
                Canvas.SetTop(square, y * (nodeSize));
                Label l = (Label)square.Children[1];
                l.Content=node.X+" ; "+node.Y;
            }
            while (tileGrid.MoveNext())
            {
                ((Grid)tileGrid.Current).Visibility = Visibility.Hidden;
            }
        }*/
        

        public void ResizeView()
        {
            mapView.SetActualExtents(tiles.ActualWidth, tiles.ActualHeight);
            mapImageScaler.ScaleX = 1;
            mapImageScaler.ScaleY = 1;
            //InitializeTiles();
        }

        /*private void InitializeTiles()
        {
            tiles.Children.Clear();
            mapView.SetActualExtents(tiles.ActualWidth, tiles.ActualHeight);
            double nodeSize = mapView.NodeSize;
            for (int i=0;i<mapView.MaxEnumeratedElements;i++)
            {
                Grid square = new Grid();
                //tile
                Rectangle r = new Rectangle
                {
                    Width = nodeSize+0.5,
                    Height = nodeSize+0.5
                };
                square.Children.Add(r);
                //text
                Label l = new Label
                {
                    Width = nodeSize,
                    Height = nodeSize,
                    Content = "h",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Red
                };
                square.Children.Add(l);
                tiles.Children.Add(square);
            }
        }*/

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            game.GameEnded = true;
        }

        /// <summary>
        /// The class describes player's view of the map.
        /// Enumerator returns the nodes inside current players view from left top to right bottom.
        /// </summary>
        private class MapView:IEnumerable<Node>
        {
            private Map map;
            private double actualWidth;
            private double actualHeight;
            private double scrollSpeed;
            private double zoomSpeed;
            private double minNodeSize;
            private double maxNodeSize;

            public double NodeSize { get; set; }
            //These values are relative to size of one node.
            public double Top { get; set; }
            public double Left { get; set; }
            public double Width => actualWidth / map.NodeSize;
            public double Height => actualHeight / map.NodeSize;
            public double Right => Left + Width;
            public double Bottom => Top + Height;


            public MapView(double top,double left, double nodeSize, Map map,
                double minNodeSize = 50, double maxNodeSize=90,double scrollSpeed=0.1,double zoomSpeed=20)
            {
                Top = top;
                Left = left;
                NodeSize = nodeSize;
                this.map = map;
                this.scrollSpeed = scrollSpeed;
                this.zoomSpeed = zoomSpeed;
                this.minNodeSize = minNodeSize;
                this.maxNodeSize = maxNodeSize;
            }
            
            public IEnumerator<Node> GetEnumerator()
            {
                if (actualHeight == 0 || actualWidth == 0)
                    throw new InvalidOperationException("Actual extents have to be specified before getting enumerator!");

                int width = (int)(Math.Ceiling(Width) + 1);
                int height = (int)(Math.Ceiling(Height) + 1);
                for (int i = (int)Left; i <= (int)Left+width; i++)
                    for (int j = (int)Top; j <= (int)Top+height; j++)
                    {
                        //check if coordinates are valid
                        if (i >= map.Nodes.GetLength(1))
                            goto afterLoop;
                        if (i < 0)
                            break;
                        if (j >= map.Nodes.GetLength(0))
                            break;
                        if (j < 0)
                            continue;

                        yield return map.Nodes[i, j];
                    }
                afterLoop:;
            }

            public RectangleGeometry GetViewGeometry()
            {
                return new RectangleGeometry(new Rect(Left * map.NodeSize, Top * map.NodeSize,
                    actualWidth, actualHeight));
            }

            public void SetActualExtents(double width, double height)
            {
                this.actualWidth = width;
                this.actualHeight = height;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int MaxEnumeratedElements => (int)(Math.Ceiling(Width) + 2) * (int)(Math.Ceiling(Height) + 2);

            /// <summary>
            /// Moves the view in given direction so that it doesn't leave the map.
            /// </summary>
            /// <param name="dir"></param>
            public void Move(Direction dir)
            {
                switch (dir)
                {

                    case Direction.DOWN: Top += scrollSpeed; break;
                    case Direction.UP: Top -= scrollSpeed; break;
                    case Direction.LEFT: Left -= scrollSpeed; break;
                    case Direction.RIGHT:Left += scrollSpeed;break;
                }
                CorrectPosition();
            }

            /// <summary>
            /// Decreases size of viewed area.
            /// </summary>
            public bool ZoomIn()
            {
                double newNodeSize= Math.Min(maxNodeSize,Math.Max(NodeSize + zoomSpeed,minNodeSize));
                if(newNodeSize != NodeSize)
                {
                    NodeSize = newNodeSize;
                    CorrectPosition();
                    return true;
                }
                return false;
            }
            
            /// <summary>
            /// Increases size of viewed area.
            /// </summary>
            public bool ZoomOut()
            {
                double newNodeSize = Math.Min(maxNodeSize, Math.Max(NodeSize - zoomSpeed, minNodeSize));
                if (newNodeSize != NodeSize)
                {
                    NodeSize = newNodeSize;
                    CorrectPosition();
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Sets view position into the map.
            /// </summary>
            public void CorrectPosition()
            {
                if (Right > map.Width)
                    Left = map.Width - Width;
                if (Left < 0)
                    Left = 0;
                if (Bottom > map.Height)
                    Top = map.Height-Height;
                if (Top < 0)
                    Top = 0;
            }
        }

        private enum Direction
        {
            LEFT,
            UP,
            RIGHT,
            DOWN
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down: mapMovementInput.AddDirection(Direction.DOWN); break;
                case Key.Up: mapMovementInput.AddDirection(Direction.UP); break;
                case Key.Left: mapMovementInput.AddDirection(Direction.LEFT); break;
                case Key.Right: mapMovementInput.AddDirection(Direction.RIGHT); break;
            }
        }


        private class MapMovementInput
        {
            public List<Direction> MapDirection { get; private set; }

            public MapMovementInput()
            {
                MapDirection = new List<Direction>();
            }

            public void AddDirection(Direction dir)
            {
                if (MapDirection.Contains(dir))
                    return;

                if (MapDirection.Contains(Opposite(dir)))
                {
                    MapDirection.Remove(Opposite(dir));
                }
                MapDirection.Add(dir);
            }

            public void RemoveDirection(Direction dir)
            {
                MapDirection.Remove(dir);
            }

            private Direction Opposite(Direction dir)
            {
                switch (dir)
                {
                    case Direction.DOWN: return Direction.UP;
                    case Direction.UP: return Direction.DOWN;
                    case Direction.LEFT: return Direction.RIGHT;
                    default: return Direction.LEFT;
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down: mapMovementInput.RemoveDirection(Direction.DOWN); break;
                case Key.Up: mapMovementInput.RemoveDirection(Direction.UP); break;
                case Key.Left: mapMovementInput.RemoveDirection(Direction.LEFT); break;
                case Key.Right: mapMovementInput.RemoveDirection(Direction.RIGHT); break;
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (mapView.ZoomIn())
                    ResizeView();
            }
            else
            {
                if (mapView.ZoomOut())
                    ResizeView();
            }
        }

        private bool openGlNotInitialized = true;

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {

            //  Get the OpenGL object, for quick access.
            OpenGL gl = args.OpenGL;

            if (openGlNotInitialized)
            {
                OpenGlInitialize();
                openGlNotInitialized=false;
            }

            //  Clear the color and depth buffers.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            


            //DrawSquareOGL(gl, 0, 0);
            TextureVsColorMeasuring(gl);

            //  Flush OpenGL.
            gl.Flush();

            //  Rotate the geometry a bit.
            rotatePyramid += 3.0f;
            rquad -= 3.0f;
        }

        private void TextureVsColorMeasuring(OpenGL gl)
        {
            //Orthographic(gl);
            int width = 20;
            int height = 10;

            float squareSide = 20f;

            CreateVerticesForSquare(gl);
            DrawBuffered(gl);

            /*Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    //DrawSquareColorOrographic(gl, i, j, squareSide, squareSide);
                    DrawSquareOGL(gl,i,j, squareSide, squareSide);
            sw.Stop();
            Console.WriteLine("Drawing shapes took: " + sw.Elapsed.Milliseconds);

            sw.Reset();
            sw.Start();

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    //DrawSquareTextureOrographic(gl, i, j, squareSide, squareSide);
                    DrawTextureSquareOGL(gl, i, j, squareSide, squareSide);
            sw.Stop();
            Console.WriteLine("Drawing textures took: " + sw.Elapsed.Milliseconds);*/
        }

        private void OpenGlInitialize()
        {
            //  Get the OpenGL object, for quick access.
            OpenGL gl = openGLControl1.OpenGL;
            this.openGLControl1.FrameRate = 30;

            //  A bit of extra initialisation here, we have to enable textures.
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            //  Create our texture object from a file. This creates the texture for OpenGL.
            texture.Create(gl, "Grass.png");
        }

        Texture texture = new Texture();
        float rotatePyramid = 0;
        float rquad = 0;

        private void DrawCubeOGL(OpenGL gl, float x , float y)
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

        float Distance;

        private void DrawSquareOGL(OpenGL gl, float x, float y, float width, float height)
        {
            Distance = 5f;

            //  Reset the modelview matrix.
            gl.LoadIdentity();

            //  Move into a more central position.
            gl.Translate(x* width, y* height, -Distance);

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

        private void DrawTextureSquareOGL(OpenGL gl, float x, float y, float width, float height)
        {
            Distance = 5f;

            //  Reset the modelview matrix.
            gl.LoadIdentity();

            //  Move into a more central position.
            gl.Translate(x* width, y * height, -Distance);

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

        public void Orthographic(OpenGL gl)
        {
            gl.MatrixMode(MatrixMode.Projection);
            gl.LoadIdentity();

            // NOTE: Basically no matter what I do, the only points I see are those at
            // the "near" surface (with z = -zNear)--in this case, I only see green points
            gl.Ortho(0, openGLControl1.ActualWidth, openGLControl1.ActualHeight, 0, 1, 10);

            //  Back to the modelview.
            gl.MatrixMode(MatrixMode.Modelview);
            
        }

        private void DrawSquareColorOrographic(OpenGL gl, float x, float y, float width, float height)
        {
            Distance = 5f;

            //  Reset the modelview matrix.
            gl.LoadIdentity();

            //  Move into a more central position.
            gl.Translate(x*width, y*height, -Distance);

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

        private void DrawSquareTextureOrographic(OpenGL gl, float x, float y, float width, float height)
        {
            Distance = 5f;

            //  Reset the modelview matrix.
            gl.LoadIdentity();

            //  Move into a more central position.
            gl.Translate(x*width, y*height, -Distance);

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

        private void DrawBuffered(OpenGL gl)
        {
            //  Bind the out vertex array.
            vertexBufferArray.Bind(gl);

            //  Draw the square.
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 6);

            //  Unbind our vertex array and shader.
            vertexBufferArray.Unbind(gl);
        }

        /// <summary>
        /// Creates the geometry for the square, also creating the vertex buffer array.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        private void CreateVerticesForSquare(OpenGL gl)
        {
            var vertices = new float[18];
            var colors = new float[18]; // Colors for our vertices  
            vertices[0] = -0.5f; vertices[1] = -0.5f; vertices[2] = 0.0f; // Bottom left corner  
            colors[0] = 1.0f; colors[1] = 1.0f; colors[2] = 1.0f; // Bottom left corner  
            vertices[3] = -0.5f; vertices[4] = 0.5f; vertices[5] = 0.0f; // Top left corner  
            colors[3] = 1.0f; colors[4] = 0.0f; colors[5] = 0.0f; // Top left corner  
            vertices[6] = 0.5f; vertices[7] = 0.5f; vertices[8] = 0.0f; // Top Right corner  
            colors[6] = 0.0f; colors[7] = 1.0f; colors[8] = 0.0f; // Top Right corner  
            vertices[9] = 0.5f; vertices[10] = -0.5f; vertices[11] = 0.0f; // Bottom right corner  
            colors[9] = 0.0f; colors[10] = 0.0f; colors[11] = 1.0f; // Bottom right corner  
            vertices[12] = -0.5f; vertices[13] = -0.5f; vertices[14] = 0.0f; // Bottom left corner  
            colors[12] = 1.0f; colors[13] = 1.0f; colors[14] = 1.0f; // Bottom left corner  
            vertices[15] = 0.5f; vertices[16] = 0.5f; vertices[17] = 0.0f; // Top Right corner  
            colors[15] = 0.0f; colors[16] = 1.0f; colors[17] = 0.0f; // Top Right corner  

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

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(gl);
        }

        //  The vertex buffer array which contains the vertex and colour buffers.
        VertexBufferArray vertexBufferArray;


        private void DrawTextureOGL(OpenGL gl)
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

        float rtri = 0;
    }
}
