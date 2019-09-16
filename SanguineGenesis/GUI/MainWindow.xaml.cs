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
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI;

namespace SanguineGenesis
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Game = new Game();
            var MapView = new MapView(0, 0, 60, Game.Map, Game);
            GameControls = new GameControls(MapView, Game);
            openGLControl1.FrameRate = 60;

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

        #region Game logic
        /// <summary>
        /// The game this window is showing.
        /// </summary>
        private Game Game { get; }
        /// <summary>
        /// Can only be used synchronously from this window. Using this variable or
        /// its contents from other threads requires invocation.
        /// </summary>
        private GameControls GameControls { get; }

        /// <summary>
        /// Number of updates of the game per second.
        /// </summary>
        private int WantedGameUps => 50;
        /// <summary>
        /// Time between two game updates.
        /// </summary>
        private int StepLength => 1000 / WantedGameUps;

        /// <summary>
        /// Measures time from the start of the game.
        /// </summary>
        private Stopwatch totalStopwatch = new Stopwatch();
        /// <summary>
        /// Total time elapsed since the start of the game. Is set in
        /// the main loop.
        /// </summary>
        private double TotalTime { get; set; }

        /// <summary>
        /// Measures time from the start of the step.
        /// </summary>
        private Stopwatch stepStopwatch = new Stopwatch();
        /// <summary>
        /// Total time elapsed since the start of the step. Is set in
        /// the main loop.
        /// </summary>
        private double TotalStepTime { get; set; }


        private int StepsDone { get; set; }

        /// <summary>
        /// The main loop of the game.
        /// </summary>
        public void MainLoop()
        {
            TotalTime = 0; StepsDone = 0;
            totalStopwatch.Start();
            while (true)
            {
                MainLoopStep();
            }
        }

        /// <summary>
        /// One update of the game.
        /// </summary>
        private void MainLoopStep()
        {
            stepStopwatch.Start();

            lock (Game)
            {
                if (Game.GameEnded)
                    return;

                //process player's input
                //Invoke could cause deadlock because drawing also locks game
                Dispatcher.BeginInvoke(
                    (Action)(() =>
                GameControls.UpdateMapView(Game)));

                GameControls.UpdateEntitiesByInput(Game);

                //update the state of the game
                long totalEl = totalStopwatch.ElapsedMilliseconds;
                float deltaT = (totalEl - (float)TotalTime) / 1000f;
                TotalTime = totalEl;
                Game.Update(deltaT);
            }

            stepStopwatch.Stop();

            //calculate sleep time
            double diff = stepStopwatch.Elapsed.TotalMilliseconds;
            stepStopwatch.Reset();
            TotalStepTime += diff;
            int sleepTime = StepLength - (int)TotalStepTime;
            if ((int)TotalStepTime > 0)
                TotalStepTime = TotalStepTime - (int)TotalStepTime;

            StepsDone++;
            //Console.WriteLine("updates per second: " + StepsDone / TotalTime * 1000);
            //Console.WriteLine("draws per second: " + DrawingsDone / TotalTime * 1000);

            //sleep for the rest of the step time
            if (sleepTime > 0)
                Thread.Sleep(sleepTime);
        }
        #endregion Game logic

        #region User interface
        private EntityButtonArray EntityButtonArray { get; set; }
        private AbilityButtonArray AbilityButtonArray { get; set; }
        private EntityInfoPanel EntityInfoPanel { get; set; }
        private AdditionalInfo AdditionalInfo { get; set; }
        private GameOptionsPanel GameOptionsPanel { get; set; }

        /// <summary>
        /// Initializes user interface.
        /// </summary>
        private void InitializeBottomPanel()
        {
            //fill ui elements with buttons
            //units panel
            EntityButtonArray = new EntityButtonArray(8, 5, 300, 188);
            gui.Children.Add(EntityButtonArray);
            
            //abilities panel
            AbilityButtonArray = new AbilityButtonArray(4, 4, 200, 200);
            gui.Children.Add(AbilityButtonArray);
            
            //unit info panel
            EntityInfoPanel = new EntityInfoPanel(250, 200);
            gui.Children.Add(EntityInfoPanel);

            //additional info
            AdditionalInfo = new AdditionalInfo(100, 200);
            gui.Children.Add(AdditionalInfo);
            AdditionalInfo.Stats.SetStats(
                new List<Stat>()
                {
                    new Stat("Energy cost: ", "50"),
                    new Stat("Air: ", "2")
                });

            //game options panel
            GameOptionsPanel = new GameOptionsPanel(250, 300, Game.GameplayOptions);
            menuLayer.Children.Add(GameOptionsPanel);

            //add listeners to the buttons
            EntityButtonArray.ShowInfoOnClick(EntityInfoPanel, AbilityButtonArray, GameControls);
            AbilityButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            EntityInfoPanel.CommandButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            EntityInfoPanel.CommandButtonArray.RemoveCommandOnClick();
            EntityInfoPanel.StatusButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            AbilityButtonArray.SelectAbilityOnClick(GameControls);

            //set position of ui elements
            //bottom panel
            double entityInfoW = EntityInfoPanel.Width;
            double entityButtonArrayW = EntityButtonArray.Width;
            double abilityButtonArrayW = AbilityButtonArray.Width;
            double additionalInfoW = AdditionalInfo.Width;
            double offsetX=(openGLControl1.ActualWidth - (entityInfoW + entityButtonArrayW + abilityButtonArrayW + additionalInfoW))/ 2;
            
            double unitInfoX = offsetX;
            Canvas.SetLeft(EntityInfoPanel, unitInfoX);
            Canvas.SetBottom(EntityInfoPanel, 0);

            double unitPanelX = unitInfoX + entityInfoW;
            Canvas.SetLeft(EntityButtonArray, unitPanelX);
            Canvas.SetBottom(EntityButtonArray, 0);

            double abilityPanelX = unitPanelX + entityButtonArrayW;
            Canvas.SetLeft(AbilityButtonArray, abilityPanelX);
            Canvas.SetBottom(AbilityButtonArray, 0);

            double additionalInfoX = abilityPanelX + abilityButtonArrayW;
            Canvas.SetLeft(AdditionalInfo, additionalInfoX);
            Canvas.SetBottom(AdditionalInfo, 0);

            //menu button
            Canvas.SetRight(menuB, 0);
            Canvas.SetTop(menuB, 0);
        }

        /// <summary>
        /// Updates information for the bottom panel and air taken.
        /// </summary>
        private void UpdateBottomPanel()
        {
            //only set new values if the values changed since the last update
            SelectedGroup selected = GameControls.SelectedEntities;
            List<Entity> selectedEntities=null;
            bool changed;
            lock (selected)
            {
                changed = GameControls.SelectedEntities.Changed;
                if(changed)
                    selectedEntities = selected.Entities.ToList();
            }
            if (changed)
            {
                GameControls.SelectedEntities.Changed = false;
                selectedEntities.Sort((u, v) => u.GetHashCode() - v.GetHashCode());
                EntityButtonArray.Selected = selectedEntities.FirstOrDefault();
                EntityButtonArray.InfoSources = selectedEntities;
            }
            lock (GameControls.EntityCommandsInput)
            {
                //set selected ability
                if (GameControls.EntityCommandsInput.IsAbilitySelected)
                    AbilityButtonArray.Selected = GameControls.EntityCommandsInput.SelectedAbility;
                else
                    AbilityButtonArray.Selected = null;
            }

            if (EntityButtonArray.Selected != null)
                AbilityButtonArray.InfoSources = EntityButtonArray.Selected.Abilities;
            else
                AbilityButtonArray.InfoSources = new List<Ability>();
            EntityInfoPanel.SelectedEntity = EntityButtonArray.Selected;

            //update units panel
            EntityButtonArray.Update();
            AbilityButtonArray.Update();
            EntityInfoPanel.Update();
            
            airTakenL.Content = Game.CurrentPlayer.AirTaken+"/"+Game.CurrentPlayer.MaxAirTaken;
        }

        /// <summary>
        /// Sets currently selected entity.
        /// </summary>
        public void SelectEntity()
        {
            List<Entity> selectedEntities = EntityButtonArray.InfoSources;
            if (selectedEntities != null && selectedEntities.Any())
            {
                Entity selected = selectedEntities[0];
                EntityInfoPanel.SelectedEntity = selected;
                AbilityButtonArray.InfoSources = selected.Abilities;
            }
        }

        /// <summary>
        /// Ends the thread with Game when the window closes.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            lock(Game)
                Game.GameEnded = true;
        }
        
        /// <summary>
        /// Updates information about pressed keys.
        /// </summary>
        public void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down: GameControls.MapMovementInput.AddDirection(Direction.DOWN); break;
                case Key.Up: GameControls.MapMovementInput.AddDirection(Direction.UP); break;
                case Key.Left: GameControls.MapMovementInput.AddDirection(Direction.LEFT); break;
                case Key.Right: GameControls.MapMovementInput.AddDirection(Direction.RIGHT); break;
                case Key.LeftShift:
                    lock (GameControls.EntityCommandsInput)
                        GameControls.EntityCommandsInput.ResetCommandsQueue = false; break;
            }
        }

        /// <summary>
        /// Updates information about pressed keys.
        /// </summary>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down: GameControls.MapMovementInput.RemoveDirection(Direction.DOWN); break;
                case Key.Up:  GameControls.MapMovementInput.RemoveDirection(Direction.UP); break;
                case Key.Left:  GameControls.MapMovementInput.RemoveDirection(Direction.LEFT); break;
                case Key.Right:  GameControls.MapMovementInput.RemoveDirection(Direction.RIGHT); break;
                case Key.LeftShift:
                    lock (GameControls.EntityCommandsInput)
                        GameControls.EntityCommandsInput.ResetCommandsQueue = true; break;
            }
        }

        /// <summary>
        /// Changes the zoom of the map.
        /// </summary>
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                GameControls.MapView.ZoomIn(Game.Map) ;
            }
            else
            {
                GameControls.MapView.ZoomOut(Game.Map);
            }
                
        }

        /// <summary>
        /// Initializes OpenGLAtlasDrawer.
        /// </summary>
        private void InitializeOpenGL()
        {
            OpenGL gl = openGLControl1.OpenGL;
            OpenGLAtlasDrawer.Initialize(gl, (float)openGLControl1.ActualWidth, (float)openGLControl1.ActualHeight);
            OpenGLAtlasDrawer.CreateMap(gl);
            OpenGLAtlasDrawer.CreateNutrientsMap(gl);
            OpenGLAtlasDrawer.CreateUnitCircles(gl);
            OpenGLAtlasDrawer.CreateEntities(gl);
            OpenGLAtlasDrawer.CreateEntitiesIndicators(gl);
            OpenGLAtlasDrawer.CreateFlowField(gl);
            OpenGLAtlasDrawer.CreateSelectionFrame(gl);
        }

        private int DrawingsDone { get; set; }

        /// <summary>
        /// Updates information about what should be drawn.
        /// </summary>
        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            OpenGL gl = args.OpenGL;
            //Game has to be locked, because it can be used from the main loop
            lock (Game)
            {
                GameControls.MapView.SetActualExtents((float)openGLControl1.ActualWidth, (float)openGLControl1.ActualHeight);
                OpenGLAtlasDrawer.UpdateMapDataBuffers(gl, GameControls.MapView, Game);
                if (Game.GameplayOptions.NutrientsVisible)
                    OpenGLAtlasDrawer.UpdateNutrientsMapDataBuffers(gl, GameControls.MapView, Game);
                else
                    OpenGLAtlasDrawer.TryClearNutrientsMapDataBuffers(gl);
                OpenGLAtlasDrawer.UpdateEntityCirclesDataBuffers(gl, GameControls.MapView, Game);
                OpenGLAtlasDrawer.UpdateEntitiesDataBuffers(gl, GameControls.MapView, Game);
                OpenGLAtlasDrawer.UpdateEntityIndicatorsDataBuffers(gl, GameControls.MapView, Game);
                //show flowfield of the selected animal if an animal is selected and player wants to show flowfield
                if (Game.GameplayOptions.ShowFlowfield)
                {
                    Animal selected;
                    MoveToCommand command;
                    FlowField flF = null;
                    if ((selected = (EntityButtonArray.Selected as Animal)) != null &&
                        (command = (selected.CommandQueue.First() as MoveToCommand))!=null &&
                        (flF =command.FlowField)!=null)
                    {
                        OpenGLAtlasDrawer.UpdateFlowFieldDataBuffers(gl, GameControls.MapView, flF);
                    }
                    else
                        OpenGLAtlasDrawer.TryClearFlowFieldDataBuffers(gl);
                }
                else
                    OpenGLAtlasDrawer.TryClearFlowFieldDataBuffers(gl);
                OpenGLAtlasDrawer.UpdateSelectionFrameDataBuffers(gl, GameControls.MapView, GameControls.MapSelectorFrame);
                UpdateBottomPanel();
                DrawingsDone++;
            }
            OpenGLAtlasDrawer.Draw(gl, Game.GameplayOptions);

            sw.Stop();
            //Console.WriteLine("Time drawing: " + sw.Elapsed.Milliseconds);
        }

        private void openGLControl1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPos = e.GetPosition(openGLControl1);
            Vector2 mapCoordinates = GameControls.MapView
                .ScreenToMap(new Vector2((float)clickPos.X,(float)clickPos.Y));
            GameControls.EntityCommandsInput.NewPoint(mapCoordinates);
            
            //hide gui so that player can select from the whole screen
            gui.Visibility = Visibility.Hidden;
            //set selected entity
            SelectEntity();
        }

        private void openGLControl1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point clickPos = e.GetPosition(openGLControl1);
            Vector2 mapCoordinates = GameControls.MapView
                .ScreenToMap(new Vector2((float)clickPos.X, (float)clickPos.Y));
            GameControls.EntityCommandsInput.EndSelection(mapCoordinates);

            //turn gui back on
            gui.Visibility = Visibility.Visible;
            //set selected entity
            SelectEntity();
        }

        private void openGLControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (GameControls.EntityCommandsInput.State== EntityCommandsInputState.SELECTING_UNITS)
            {
                Point clickPos = e.GetPosition(openGLControl1);
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2((float)clickPos.X, (float)clickPos.Y));
                GameControls.EntityCommandsInput.NewPoint(mapCoordinates);

                //set selected unit
                SelectEntity();
            }
        }

        private void openGLControl1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPos = e.GetPosition(openGLControl1);
            Vector2 mapCoordinates = GameControls.MapView
                .ScreenToMap(new Vector2((float)clickPos.X, (float)clickPos.Y));
            GameControls.EntityCommandsInput.SetTarget(mapCoordinates);
        }

        /// <summary>
        /// Opens menu and disables interaction with the game.
        /// </summary>
        private void menuB_Click(object sender, RoutedEventArgs e)
        {
            gui.IsEnabled = false;
            openGLControl1.IsEnabled = false;
            menuLayer.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Closes menu and enables interaction with the game.
        /// </summary>
        private void menu_resume_Click(object sender, RoutedEventArgs e)
        {
            gui.IsEnabled = true;
            openGLControl1.IsEnabled = true;
            menuLayer.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Opens options menu.
        /// </summary>
        private void menu_options_Click(object sender, RoutedEventArgs e)
        {
            GameOptionsPanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Closes the game.
        /// </summary>
        private void menu_exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion User interface
    }
}
