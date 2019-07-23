﻿using GlmNet;
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
using wpfTest.GameLogic.Maps;

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
            

            BitmapImage mapBitmap = (BitmapImage)FindResource("riverBuildingsMap");
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
                Dispatcher.Invoke(InitializeUnitPanel);
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
            //game.FlowMap = Pathfinding.GetPathfinding.GenerateFlowMap(game.Map.GetObstacleMap(), new Vector2(10, 2));
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

        private List<Unit> selectedUnits;
        private Button[] unitButtons;
        private List<AbilityType> selectedUnitsAbilities;
        private Button[] abilityButtons;
        private Unit selectedUnit;
        private Label[] selUnCommands;

        private void InitializeUnitPanel()
        {
            selectedUnitsAbilities = new List<AbilityType>();

            //fill ui elements with buttons
            //units panel
            unitButtons = new Button[48];
            for (int i = 0; i < unitButtons.Length; i++)
            {
                Button b = new Button();
                int buttonInd = i;//capture by value
                //select unit
                b.Click += (button,ev) =>
                {
                    if (buttonInd < selectedUnits.Count)
                    {
                        selectedUnit = selectedUnits[buttonInd];
                        UpdateUnitInfo();
                    }
                };
                b.Content = "";
                b.SetValue(Grid.ColumnProperty, i % 8);
                b.SetValue(Grid.RowProperty, i / 8);
                b.Background = Brushes.LightBlue;
                b.Focusable = false;
                unitPanel.Children.Add(b);
                unitButtons[i] = b;
            }

            //abilities panel
            abilityButtons = new Button[16];
            for(int i = 0; i < abilityButtons.Length; i++)
            {
                Button b= new Button();
                int buttonInd=i;//capture by value
                //select ability
                b.Click += (button, ev) =>
                  {
                      if (buttonInd < selectedUnitsAbilities.Count)
                      {
                          AbilityType at = selectedUnitsAbilities[buttonInd];
                          lock (gameControls.UnitCommandsInput)
                          {
                              gameControls.UnitCommandsInput.AbilityType = at;
                          }
                      }
                  };
                b.Content = "";
                b.SetValue(Grid.ColumnProperty, i % 4);
                b.SetValue(Grid.RowProperty, i / 4);
                b.Background = Brushes.Orange;
                b.Focusable = false;
                abilityPanel.Children.Add(b);
                abilityButtons[i] = b;
            }

            //unit info panel
            selUnCommands = new Label[5];
            for(int i = 0; i < selUnCommands.Length; i++)
            {
                Label l = new Label();
                l.Content = "";
                l.SetValue(Grid.ColumnProperty, i);
                l.SetValue(Grid.RowProperty, 0);
                l.Focusable = false;
                commandsPanel.Children.Add(l);
                selUnCommands[i] = l;
            }
            UpdateUnitInfo();

            //set position of ui elements
            Console.WriteLine(ActualWidth);
            double unitInfoW = unitInfoPanel.ActualWidth;
            double unitPanelW = unitPanel.ActualWidth;
            double abilityPanelW = abilityPanel.ActualWidth;
            double offsetX=(openGLControl1.ActualWidth - (unitInfoW + unitPanelW + abilityPanelW))/ 2;
            
            double unitInfoX = offsetX;
            Canvas.SetLeft(unitInfoPanel, unitInfoX);
            Canvas.SetBottom(unitInfoPanel, 0);

            double unitPanelX = unitInfoX + unitInfoW;
            Canvas.SetLeft(unitPanel, unitPanelX);
            Canvas.SetBottom(unitPanel, 0);

            double abilityPanelX = unitPanelX + unitPanelW;
            Canvas.SetLeft(abilityPanel, abilityPanelX);
            Canvas.SetBottom(abilityPanel, 0);
        }

        private void UpdateUnitPanel()
        {
            //locking the units can slow down the game so we only create a copy
            selectedUnits = gameControls.SelectedUnits.Units.Take(unitButtons.Length).ToList();
            //initialize list of abilities
            selectedUnitsAbilities.Clear();
            foreach (Unit u in selectedUnits)
                foreach(AbilityType at in u.Abilities)
                    if (!selectedUnitsAbilities.Contains(at))
                        selectedUnitsAbilities.Add(at);
            if (selectedUnits == null)
                return;
            selectedUnits.Sort((u, v) => u.UnitType - v.UnitType);
            //units panel
            for (int i = 0; i < unitButtons.Length; i++)
            {
                Button b = unitButtons[i];
                if (i >= selectedUnits.Count)
                {
                    b.Visibility = Visibility.Hidden;
                }
                else
                {
                    b.Content = selectedUnits[i].UnitType;
                    b.Visibility = Visibility.Visible;
                }
            }
            //ability panel
            for (int i = 0; i < abilityButtons.Length; i++)
            {
                Button b = abilityButtons[i];
                if (i >= selectedUnitsAbilities.Count)
                {
                    b.Visibility = Visibility.Hidden;
                }
                else
                {
                    b.Content = selectedUnitsAbilities[i];
                    b.Visibility = Visibility.Visible;
                }
            }

            UpdateUnitInfo();
        }

        /// <summary>
        /// Update info about currently selected unit.
        /// </summary>
        private void UpdateUnitInfo()
        {
            //dont show info about dead units
            if (selectedUnit!=null && selectedUnit.IsDead)
                selectedUnit = null;

            if (selectedUnit == null)
            {
                nameL.Content = "";
                healthL.Content = "";
                energyL.Content = "";
                sizeL.Content = "";
                viewRangeL.Content = "";
                maxSpeedL.Content = "";
                atDamageL.Content = "";
                atPeriodL.Content = "";
                atDistanceL.Content = "";
                //commands panel
                for (int i = 0; i < selUnCommands.Length; i++)
                {
                    selUnCommands[i].Visibility = Visibility.Hidden;
                }
            }
            else
            {
                nameL.Content = selectedUnit.UnitType;
                healthL.Content = selectedUnit.Health + "/" + selectedUnit.MaxHealth;
                energyL.Content = selectedUnit.MaxEnergy > 0 ? selectedUnit.Energy + "/" + selectedUnit.MaxEnergy : "-";
                sizeL.Content = selectedUnit.Range * 2;
                viewRangeL.Content = selectedUnit.ViewRange;
                maxSpeedL.Content = selectedUnit.MaxSpeed;
                atDamageL.Content = selectedUnit.AttackDamage;
                atPeriodL.Content = selectedUnit.AttackPeriod;
                atDistanceL.Content = selectedUnit.AttackDistance;
                //commands panel
                List<Command> commands = selectedUnit.CommandQueue.ToList();
                for (int i = 0; i < selUnCommands.Length; i++)
                {
                    Label l = selUnCommands[i];
                    if (i >= commands.Count)
                    {
                        l.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        l.Content = commands[i].ToString();
                        l.Visibility = Visibility.Visible;
                    }
                }
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
            OpenGLAtlasDrawer.CreateUnitCircles(gl);
            OpenGLAtlasDrawer.CreateUnits(gl);
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
                OpenGLAtlasDrawer.UpdateUnitCirclesDataBuffers(gl, gameControls.MapView, game);
                OpenGLAtlasDrawer.UpdateUnitsDataBuffers(gl, gameControls.MapView, game);
                OpenGLAtlasDrawer.UpdateUnitIndicatorsDataBuffers(gl, gameControls.MapView, game);
                OpenGLAtlasDrawer.UpdateFlowMapDataBuffers(gl, gameControls.MapView, game);
                OpenGLAtlasDrawer.UpdateSelectionFrameDataBuffers(gl, gameControls.MapView, gameControls.MapSelectorFrame);
                OpenGLAtlasDrawer.Draw(gl);
                UpdateUnitPanel();
            }
            sw.Stop();
            //Console.WriteLine("Time drawing: " + sw.Elapsed.Milliseconds);
        }

        private void openGLControl1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //reset selected unit
            selectedUnit = null;

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

            //set selected unit
            if (selectedUnits != null && selectedUnits.Any())
                selectedUnit = selectedUnits.FirstOrDefault();
        }

        private void openGLControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (gameControls.UnitCommandsInput.State== UnitsCommandInputState.SELECTING)
            {

                Point clickPos = e.GetPosition(openGLControl1);
                Vector2 mapCoordinates = gameControls.MapView
                    .ScreenToMap(new Vector2((float)clickPos.X, (float)clickPos.Y));
                gameControls.UnitCommandsInput.NewPoint(mapCoordinates);

                //set selected unit
                if (selectedUnits != null && selectedUnits.Any())
                    selectedUnit = selectedUnits.FirstOrDefault();
            }
        }

        private void openGLControl1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPos = e.GetPosition(openGLControl1);
            Vector2 mapCoordinates = gameControls.MapView
                .ScreenToMap(new Vector2((float)clickPos.X, (float)clickPos.Y));
            gameControls.UnitCommandsInput.SetTarget(mapCoordinates);
           // game.FlowMap = Pathfinding.GetPathfinding.GenerateFlowMap(game.Map.GetObstacleMap(Movement.GROUND),  mapCoordinates);
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
