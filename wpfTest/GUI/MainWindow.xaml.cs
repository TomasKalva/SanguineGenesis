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
using wpfTest.GameLogic;
using wpfTest.GameLogic.Data.Entities;
using wpfTest.GameLogic.Maps;
using wpfTest.GUI;

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


            BitmapImage mapBitmap = (BitmapImage)FindResource("riverMap");
            game = new Game(mapBitmap);
            var MapView = new MapView(0, 0, 60, game.Map, game);
            var MapMovementInput = new MapMovementInput();
            gameControls = new GameControls(MapView, MapMovementInput, game);
            openGLControl1.FrameRate = 30;


            Thread t = new Thread(() =>
            {
                Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    //wait for window layout initialization
                }));
                Dispatcher.Invoke(InitializeOpenGL);
                Dispatcher.Invoke(InitializeBottomPanel);
                MainLoop();
            });
            t.IsBackground = true;
            t.Start();
        }

        private int wantedGameFps = 50;
        private int StepLength => 1000 / wantedGameFps;

        private Stopwatch totalStopwatch = new Stopwatch();
        private double totalTime;

        private Stopwatch stepStopwatch = new Stopwatch();
        private double totalStepTime;


        public void MainLoop()
        {
            totalTime = 0;
            totalStopwatch.Start();
            //game.FlowMap = Pathfinding.GetPathfinding.GenerateFlowMap(game.Map.GetObstacleMap(), new Vector2(10, 2));
            while (true)
            {
                MainLoopStep();
            }
        }
        
        private void MainLoopStep()
        {
            stepStopwatch.Start();

            lock (game)
            {
                if (game.GameEnded)
                    return;

                //process player's input
                //Invoke could cause deadlock because drawing also locks game
                Dispatcher.BeginInvoke(
                    (Action)(() =>
                    gameControls.UpdateMapView(game)));

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
        
        private EntityButtonArray entityButtonArray;
        private AbilityButtonArray abilityButtonArray;
        private EntityInfoPanel entityInfoPanel;
        private AdditionalInfo additionalInfo;
        private GameOptionsPanel gameOptionsPanel;

        private void InitializeBottomPanel()
        {
            //fill ui elements with buttons
            //units panel
            entityButtonArray = new EntityButtonArray(8, 5, 300, 188);
            gui.Children.Add(entityButtonArray);
            
            //abilities panel
            abilityButtonArray = new AbilityButtonArray(4, 4, 200, 200);
            gui.Children.Add(abilityButtonArray);
            
            //unit info panel
            entityInfoPanel = new EntityInfoPanel(250, 200);
            gui.Children.Add(entityInfoPanel);

            //additional info
            additionalInfo = new AdditionalInfo(100, 200);
            gui.Children.Add(additionalInfo);
            additionalInfo.Stats.SetStats(
                new List<Stat>()
                {
                    new Stat("Energy cost: ", "50"),
                    new Stat("Air: ", "2")
                });

            //game options panel
            gameOptionsPanel = new GameOptionsPanel(250, 300, game.GameplayOptions);
            menuLayer.Children.Add(gameOptionsPanel);

            //add listeners to the buttons
            entityButtonArray.ShowInfoOnClick(entityInfoPanel, abilityButtonArray, gameControls);
            abilityButtonArray.ShowInfoOnMouseOver(additionalInfo);
            entityInfoPanel.CommandButtonArray.ShowInfoOnMouseOver(additionalInfo);
            entityInfoPanel.CommandButtonArray.RemoveCommandOnClick();
            entityInfoPanel.StatusButtonArray.ShowInfoOnMouseOver(additionalInfo);
            abilityButtonArray.SelectAbilityOnClick(gameControls);

            //set position of ui elements
            //bottom panel
            double entityInfoW = entityInfoPanel.Width;
            double entityButtonArrayW = entityButtonArray.Width;
            double abilityButtonArrayW = abilityButtonArray.Width;
            double additionalInfoW = additionalInfo.Width;
            double offsetX=(openGLControl1.ActualWidth - (entityInfoW + entityButtonArrayW + abilityButtonArrayW + additionalInfoW))/ 2;
            
            double unitInfoX = offsetX;
            Canvas.SetLeft(entityInfoPanel, unitInfoX);
            Canvas.SetBottom(entityInfoPanel, 0);

            double unitPanelX = unitInfoX + entityInfoW;
            Canvas.SetLeft(entityButtonArray, unitPanelX);
            Canvas.SetBottom(entityButtonArray, 0);

            double abilityPanelX = unitPanelX + entityButtonArrayW;
            Canvas.SetLeft(abilityButtonArray, abilityPanelX);
            Canvas.SetBottom(abilityButtonArray, 0);

            double additionalInfoX = abilityPanelX + abilityButtonArrayW;
            Canvas.SetLeft(additionalInfo, additionalInfoX);
            Canvas.SetBottom(additionalInfo, 0);

            //menu button
            Canvas.SetRight(menuB, 0);
            Canvas.SetTop(menuB, 0);
        }

        private void UpdateBottomPanel()
        {
            //only set new values if the values changed since the last update
            CommandsGroup selected = gameControls.SelectedEntities;
            List<Entity> selectedEntities=null;
            bool changed;
            lock (selected)
            {
                changed = gameControls.SelectedEntities.Changed;
                if(changed)
                    selectedEntities = selected.Entities.ToList();
            }
            if (changed)
            {
                gameControls.SelectedEntities.Changed = false;
                selectedEntities.Sort((u, v) => u.GetHashCode() - v.GetHashCode());
                entityButtonArray.Selected = selectedEntities.FirstOrDefault();
                entityButtonArray.InfoSources = selectedEntities;
            }
            lock (gameControls.EntityCommandsInput)
            {
                //set selected ability
                if (gameControls.EntityCommandsInput.IsAbilitySelected)
                    abilityButtonArray.Selected = gameControls.EntityCommandsInput.SelectedAbility;
                else
                    abilityButtonArray.Selected = null;
            }

            if (entityButtonArray.Selected != null)
                abilityButtonArray.InfoSources = entityButtonArray.Selected.Abilities;
            else
                abilityButtonArray.InfoSources = new List<Ability>();
            entityInfoPanel.SelectedEntity = entityButtonArray.Selected;

            //update units panel
            entityButtonArray.Update();
            abilityButtonArray.Update();
            entityInfoPanel.Update();
            
            airTakenL.Content = game.CurrentPlayer.AirTaken+"/"+game.CurrentPlayer.MaxAirTaken;
        }

        /// <summary>
        /// Sets currently selected entity.
        /// </summary>
        public void SelectEntity()
        {
            List<Entity> selectedEntities = entityButtonArray.InfoSources;
            if (selectedEntities != null && selectedEntities.Any())
            {
                Entity selected = selectedEntities[0];
                entityInfoPanel.SelectedEntity = selected;
                abilityButtonArray.InfoSources = selected.Abilities;
            }
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
                case Key.LeftShift:
                    lock (gameControls.EntityCommandsInput)
                        gameControls.EntityCommandsInput.ResetCommandsQueue = false; break;
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
                case Key.LeftShift:
                    lock (gameControls.EntityCommandsInput)
                        gameControls.EntityCommandsInput.ResetCommandsQueue = true; break;
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                gameControls.MapView.ZoomIn(game.Map) ;
            }
            else
            {
                gameControls.MapView.ZoomOut(game.Map);
            }
                
        }

        private void InitializeOpenGL()
        {
            OpenGL gl = openGLControl1.OpenGL;
            OpenGLAtlasDrawer.Initialize(gl, (float)openGLControl1.ActualWidth, (float)openGLControl1.ActualHeight);
            OpenGLAtlasDrawer.CreateMap(gl);
            OpenGLAtlasDrawer.CreateNutrientsMap(gl);
            OpenGLAtlasDrawer.CreateUnitCircles(gl);
            OpenGLAtlasDrawer.CreateEntities(gl);
            OpenGLAtlasDrawer.CreateUnitIndicators(gl);
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
                if(game.GameplayOptions.NutrientsVisible)
                    OpenGLAtlasDrawer.UpdateNutrientsMapDataBuffers(gl, gameControls.MapView, game);
                OpenGLAtlasDrawer.UpdateEntityCirclesDataBuffers(gl, gameControls.MapView, game);
                OpenGLAtlasDrawer.UpdateEntitiesDataBuffers(gl, gameControls.MapView, game);
                OpenGLAtlasDrawer.UpdateUnitIndicatorsDataBuffers(gl, gameControls.MapView, game);
                if (game.GameplayOptions.ShowFlowmap)
                    OpenGLAtlasDrawer.UpdateFlowMapDataBuffers(gl, gameControls.MapView, game);
                OpenGLAtlasDrawer.UpdateSelectionFrameDataBuffers(gl, gameControls.MapView, gameControls.MapSelectorFrame);
                UpdateBottomPanel();
            }
            OpenGLAtlasDrawer.Draw(gl, game.GameplayOptions);
            sw.Stop();
            //Console.WriteLine("Time drawing: " + sw.Elapsed.Milliseconds);
        }

        private void openGLControl1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPos = e.GetPosition(openGLControl1);
            Vector2 mapCoordinates = gameControls.MapView
                .ScreenToMap(new Vector2((float)clickPos.X,(float)clickPos.Y));
            gameControls.EntityCommandsInput.NewPoint(mapCoordinates);

            //hide gui so that player can select from the whole screen
            gui.Visibility = Visibility.Hidden;
            //set selected entity
            SelectEntity();
        }

        private void openGLControl1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point clickPos = e.GetPosition(openGLControl1);
            Vector2 mapCoordinates = gameControls.MapView
                .ScreenToMap(new Vector2((float)clickPos.X, (float)clickPos.Y));
            gameControls.EntityCommandsInput.EndSelection(mapCoordinates);

            //turn gui back on
            gui.Visibility = Visibility.Visible;
            //set selected entity
            SelectEntity();
        }

        private void openGLControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (gameControls.EntityCommandsInput.State== EntityCommandsInputState.SELECTING_UNITS)
            {

                Point clickPos = e.GetPosition(openGLControl1);
                Vector2 mapCoordinates = gameControls.MapView
                    .ScreenToMap(new Vector2((float)clickPos.X, (float)clickPos.Y));
                gameControls.EntityCommandsInput.NewPoint(mapCoordinates);

                //set selected unit
                SelectEntity();
            }
        }

        private void openGLControl1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPos = e.GetPosition(openGLControl1);
            Vector2 mapCoordinates = gameControls.MapView
                .ScreenToMap(new Vector2((float)clickPos.X, (float)clickPos.Y));
            gameControls.EntityCommandsInput.SetTarget(mapCoordinates);
            if(game.GameplayOptions.ShowFlowmap)
                game.FlowMap = Pathfinding.GetPathfinding.GenerateFlowMap(game.Map.GetObstacleMap(Movement.LAND),  mapCoordinates);
        }

        private void menuB_Click(object sender, RoutedEventArgs e)
        {
            gui.IsEnabled = false;
            openGLControl1.IsEnabled = false;
            menuLayer.Visibility = Visibility.Visible;
        }

        private void menu_resume_Click(object sender, RoutedEventArgs e)
        {
            gui.IsEnabled = true;
            openGLControl1.IsEnabled = true;
            menuLayer.Visibility = Visibility.Hidden;
        }

        private void menu_options_Click(object sender, RoutedEventArgs e)
        {
            gameOptionsPanel.Visibility = Visibility.Visible;
        }

        private void menu_exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
