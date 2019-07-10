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
        private Game game;
        /// <summary>
        /// Can only be used synchronously from this window. Using this variable or
        /// its contents from other threads requires invocation.
        /// </summary>
        private GameControls gameControls;

        public MainWindow()
        {
            InitializeComponent();

            int newThrs = Math.Max(Environment.ProcessorCount - 2, 1);
            for (int i = 0; i < newThrs; i++)
            {
                Thread t1 = new Thread(() => { while (true) ; });
                t1.Start();
            }

            BitmapImage mapBitmap = (BitmapImage)FindResource("frameMap");
            game = new Game(mapBitmap);
            var MapView = new MapView(0, 0, 60, game.Map, game);
            var MapMovementInput = new MapMovementInput();
            gameControls = new GameControls(MapView, MapMovementInput);
            openGLControl1.FrameRate = 30;
            

            Thread t = new Thread(() => {
                Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    //wait for window layout initialization
                }));
                Dispatcher.Invoke(InitializeOpenGL);
                Dispatcher.Invoke(ResizeView);
                MainLoop();
            });
            t.IsBackground = true;
            t.Start();
        }

        private int wantedGameFps=100;
        private int StepLength => 1000 / wantedGameFps;

        private Stopwatch totalStopwatch = new Stopwatch();
        private double totalTime;

        private Stopwatch stepStopwatch = new Stopwatch();
        private double totalStepTime;


        public void MainLoop()
        {
            totalTime = 0;
            totalStopwatch.Start();
            while (true)
            {
                stepStopwatch.Start();

                lock (game)
                {
                    if (game.GameEnded)
                        break;

                    //process player's input
                    //Invoke could cause deadlock because drawing also locks game
                    Dispatcher.BeginInvoke(
                        (Action)(() =>
                        gameControls.ProcessInput(game)));

                    gameControls.UpdateUnitsByInput(game);

                    //update the state of the game
                    long totalEl = totalStopwatch.ElapsedMilliseconds;
                    float deltaT = (totalEl - (float)totalTime) / 1000f;
                    totalTime = totalEl;
                    game.Update(deltaT);
                }

                stepStopwatch.Stop();

                //calculate sleep time
                double diff = stepStopwatch.Elapsed.TotalMilliseconds;
                stepStopwatch.Reset();
                totalStepTime += diff;
                int sleepTime = StepLength - (int)totalStepTime;
                if ((int)totalStepTime > 0)
                    totalStepTime = totalStepTime - (int)totalStepTime;
                //if(sleepTime<0)
                //    Console.WriteLine(sleepTime);
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
                else
                {
                    Thread.Yield();
                }
            }
        }

        public void ResizeView()
        {
            //gameControls.MapView.SetActualExtents((float)tiles.ActualWidth, (float)tiles.ActualHeight);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            game.GameEnded = true;
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
                case Key.Down: gameControls.MapMovementInput.AddDirection(Direction.DOWN); break;
                case Key.Up: gameControls.MapMovementInput.AddDirection(Direction.UP); break;
                case Key.Left: gameControls.MapMovementInput.AddDirection(Direction.LEFT); break;
                case Key.Right: gameControls.MapMovementInput.AddDirection(Direction.RIGHT); break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down: gameControls.MapMovementInput.RemoveDirection(Direction.DOWN); break;
                case Key.Up:  gameControls.MapMovementInput.RemoveDirection(Direction.UP); break;
                case Key.Left:  gameControls.MapMovementInput.RemoveDirection(Direction.LEFT); break;
                case Key.Right:  gameControls.MapMovementInput.RemoveDirection(Direction.RIGHT); break;
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (gameControls.MapView.ZoomIn(game.Map))
                    ResizeView();
            }
            else
            {
                if (gameControls.MapView.ZoomOut(game.Map))
                    ResizeView();
            }
                
        }

        private void InitializeOpenGL()
        {
            OpenGL gl = openGLControl1.OpenGL;
            OpenGLAtlasDrawer.Initialize(gl, (float)openGLControl1.ActualWidth, (float)openGLControl1.ActualHeight);
            OpenGLAtlasDrawer.CreateMap(gl);
            OpenGLAtlasDrawer.CreateUnits(gl);
            OpenGLAtlasDrawer.CreateFlowMap(gl);
            OpenGLAtlasDrawer.CreateSelectionFrame(gl);
        }

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            
            Stopwatch sw = new Stopwatch();
            sw.Start();
            lock (game)
            {
                gameControls.MapView.SetActualExtents((float)openGLControl1.ActualWidth, (float)openGLControl1.ActualHeight);
                OpenGLAtlasDrawer.UpdateMapDataBuffers(gl, gameControls.MapView, game);
                OpenGLAtlasDrawer.UpdateUnitsDataBuffers(gl, gameControls.MapView, game);
                OpenGLAtlasDrawer.UpdateFlowMapDataBuffers(gl, gameControls.MapView, game);
                OpenGLAtlasDrawer.UpdateSelectionFrameDataBuffers(gl, gameControls.MapView, gameControls.MapSelectorFrame);
                OpenGLAtlasDrawer.Draw(gl);
            }
            sw.Stop();
            //Console.WriteLine("Time drawing: " + sw.Elapsed.Milliseconds);
        }

        private void openGLControl1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPos = e.GetPosition(openGLControl1);
            Vector2 mapCoordinates = gameControls.MapView
                .ScreenToMap(new Vector2((float)clickPos.X,(float)clickPos.Y));
            gameControls.UnitCommandsInput.NewPoint(mapCoordinates);
        }

        private void openGLControl1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point clickPos = e.GetPosition(openGLControl1);
            Vector2 mapCoordinates = gameControls.MapView
                .ScreenToMap(new Vector2((float)clickPos.X, (float)clickPos.Y));
            gameControls.UnitCommandsInput.EndSelection(mapCoordinates);
        }

        private void openGLControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (gameControls.UnitCommandsInput.State== UnitsCommandInputState.SELECTING)
            {
                Point clickPos = e.GetPosition(openGLControl1);
                Vector2 mapCoordinates = gameControls.MapView
                    .ScreenToMap(new Vector2((float)clickPos.X, (float)clickPos.Y));
                gameControls.UnitCommandsInput.NewPoint(mapCoordinates);
            }
        }

        private void openGLControl1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPos = e.GetPosition(openGLControl1);
            Vector2 mapCoordinates = gameControls.MapView
                .ScreenToMap(new Vector2((float)clickPos.X, (float)clickPos.Y));
            gameControls.UnitCommandsInput.SetTarget(mapCoordinates);
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
