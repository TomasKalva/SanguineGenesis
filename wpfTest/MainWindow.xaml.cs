using GlmNet;
using SharpGL;
using SharpGL.Enumerations;
using SharpGL.SceneGraph.Assets;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            mapView = new MapView(0, 0, 60, game.Map);
            mapMovementInput = new MapMovementInput();
            openGLControl1.FrameRate = 80;

            Thread t = new Thread(() => {
                Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    //wait for window layout initialization
                }));
                Dispatcher.Invoke(InitializeOpenGL);
                Dispatcher.Invoke(ResizeView);
                game.MainLoop();
            });
            t.IsBackground = true;
            t.Start();
            
        }

        public void Draw()
        {
            //update position of mapView
            lock (mapView)
                foreach (Direction d in mapMovementInput.MapDirection)
                    mapView.Move(d);
            
        }

        public void ResizeView()
        {
            mapView.SetActualExtents((float)tiles.ActualWidth, (float)tiles.ActualHeight);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            game.GameEnded = true;
        }

        /// <summary>
        /// The class describes player's view of the map.
        /// Enumerator returns the nodes inside current players view from left top to right bottom.
        /// </summary>
        public class MapView:IEnumerable<Node>
        {
            private Map map;
            private float actualWidth;
            private float actualHeight;
            private float scrollSpeed;
            private float zoomSpeed;
            private float minNodeSize;
            private float maxNodeSize;

            public float NodeSize { get; set; }
            //These values are relative to size of one node.
            public float Top { get; set; }
            public float Left { get; set; }
            public float Width => actualWidth / NodeSize;
            public float Height => actualHeight / NodeSize;
            public float Right => Left + Width;
            public float Bottom => Top + Height;


            public MapView(float top,float left, float nodeSize, Map map,
                float minNodeSize = 30, float maxNodeSize=70,float scrollSpeed=0.5f,float zoomSpeed=20)
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

            public Node[,] GetVisibleNodes()
            {
                if (actualHeight == 0 || actualWidth == 0)
                    throw new InvalidOperationException("Actual extents have to be specified before getting enumerator!");

                //todo: set more optimal array extents - so that it doesnt contain nulls
                int width = Math.Min((int)(Math.Ceiling(Width) + 1),
                    (int)(map.Width-Left));
                int height = Math.Min((int)(Math.Ceiling(Height) + 1),
                    (int)(map.Height-Top));

                Node[,] visible = new Node[width + 1,height + 1];

                for (int i = 0; i <= width; i++)
                    for (int j = 0; j <= height; j++)
                    {
                        int mapI = i + (int)Left;
                        int mapJ = j + (int)Top;
                        //check if coordinates are valid
                        if (mapI >= map.Nodes.GetLength(0))
                            goto afterLoop;
                        if (mapI < 0)
                            break;
                        if (mapJ >= map.Nodes.GetLength(1))
                            break;
                        if (mapJ < 0)
                            continue;

                        visible[i, j] = map.Nodes[mapI,mapJ];
                    }
                afterLoop:;
                return visible;
            }

            public void SetActualExtents(float width, float height)
            {
                this.actualWidth = width;
                this.actualHeight = height;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            
            /// <summary>
            /// Moves the view in given direction so that it doesn't leave the map.
            /// </summary>
            /// <param name="dir">The direction.</param>
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
                float newNodeSize= Math.Min(maxNodeSize,Math.Max(NodeSize + zoomSpeed,minNodeSize));
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
                float newNodeSize = Math.Min(maxNodeSize, Math.Max(NodeSize - zoomSpeed, minNodeSize));
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

        public enum Direction
        {
            LEFT,
            UP,
            RIGHT,
            DOWN
        }

        public void Window_KeyDown(object sender, KeyEventArgs e)
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

        private void InitializeOpenGL()
        {
            OpenGL gl = openGLControl1.OpenGL;
            OpenGLAtlasDrawer.Initialize(gl, (float)ActualWidth, (float)ActualHeight);
            OpenGLAtlasDrawer.CreateMap(gl);
        }

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            //  Get the OpenGL object, for quick access.
            OpenGL gl = args.OpenGL;

            /*if (openGlNotInitialized)
            {
                openGlNotInitialized = false;
                OpenGLAtlasDrawer.Initialize(gl, (float)ActualWidth, (float)ActualHeight);
                OpenGLAtlasDrawer.CreateMap(gl);
            }*/
            Stopwatch sw = new Stopwatch();
            sw.Start();
            mapView.SetActualExtents((float)ActualWidth, (float)ActualHeight);
            OpenGLAtlasDrawer.UpdateMapDataBuffers(gl, mapView);
            OpenGLAtlasDrawer.Draw(gl);
            sw.Stop();
            //Console.WriteLine("Time drawing: " + sw.Elapsed.Milliseconds);
        }
    }

    /// <summary>
    /// A small helper class to load manifest resource files.
    /// </summary>
    public static class ManifestResourceLoader
    {
        /// <summary>
        /// Loads the named manifest resource as a text string.
        /// </summary>
        /// <param name="textFileName">Name of the text file.</param>
        /// <returns>The contents of the manifest resource.</returns>
        public static string LoadTextFile(string textFileName)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var pathToDots = textFileName.Replace("\\", ".");
            var location = string.Format("{0}.{1}", executingAssembly.GetName().Name, pathToDots);

            using (var stream = executingAssembly.GetManifestResourceStream(location))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
