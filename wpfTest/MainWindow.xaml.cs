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

            game = new Game("vanilaMap", this);
            mapView = new MapView(0, 0, 60, game.Map);
            mapMovementInput = new MapMovementInput();

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
            DrawMap();
        }

        private void DrawMap()
        {
            Node[,] map = game.Map.Nodes;

            mapView.SetActualExtents(tiles.ActualWidth, tiles.ActualHeight);

            double nodeSize = mapView.NodeSize;
            double viewLeft = mapView.Left;
            double viewTop = mapView.Top;
            IEnumerator tileGrid = tiles.Children.GetEnumerator();
            foreach (Node node in mapView)
            {
                tileGrid.MoveNext();
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
        }

        public void ResizeView()
        {
            mapView.SetActualExtents(tiles.ActualWidth, tiles.ActualHeight);
            InitializeTiles();
        }

        private void InitializeTiles()
        {
            tiles.Children.Clear();
            mapView.SetActualExtents(tiles.ActualWidth, tiles.ActualHeight);
            double nodeSize = mapView.NodeSize;
            foreach (var _ in mapView)
            {
                Grid square = new Grid();
                //tile
                Rectangle r = new Rectangle
                {
                    Width = nodeSize,
                    Height = nodeSize
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
        }

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

            public double NodeSize { get; set; }
            //These values are relative to size of one node.
            public double Top { get; set; }
            public double Left { get; set; }
            public double Width => actualWidth / NodeSize;
            public double Height => actualHeight / NodeSize;
            public double Right => Left + Width;
            public double Bottom => Top + Height;


            public MapView(double top,double left, double nodeSize, Map map,double speed=0.1)
            {
                Top = top;
                Left = left;
                NodeSize = nodeSize;
                this.map = map;
                this.scrollSpeed = speed;
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

            public void SetActualExtents(double width, double height)
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
    }
}
