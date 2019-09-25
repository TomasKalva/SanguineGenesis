using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GUI.WPFComponents;
using SharpGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SanguineGenesis.GUI
{
    public partial class MainWinformWindow : Form
    {
        public MainWinformWindow()
        {
            InitializeComponent();

            Game = new Game();
            var MapView = new MapView(0, 0, 60, Game.Map, Game);
            GameControls = new GameControls(MapView, Game);
            openGLControl1.FrameRate = 60;
            totalStopwatch.Start();

            MouseWheel += Window_MouseWheel;

            Shown += (s, e) =>
            {
                InitializeOpenGL();
                InitializeBottomPanel();
            };
            /*
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
            t.Start();*/
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
                //Dispatcher.BeginInvoke(
                //    (Action)(() =>
                GameControls.UpdateMapView(Game);//));

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
            Console.WriteLine("updates per second: " + StepsDone / TotalTime * 1000);
            Console.WriteLine("draws per second: " + DrawingsDone / TotalTime * 1000);

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
        private GameOptionsMenu GameOptionsMenu { get; set; }
        private Button MenuButton { get; set; }
        private GameMenu GameMenu { get; set; }
        private PlayerPropertiesPanel PlayerPropertiesPanel { get; set; }

        /// <summary>
        /// Initializes user interface.
        /// </summary>
        private void InitializeBottomPanel()
        {

            //fill ui elements with buttons
            //units panel
            EntityButtonArray = new EntityButtonArray(8, 5, 300);
            Controls.Add(EntityButtonArray);

            //abilities panel
            AbilityButtonArray = new AbilityButtonArray(4, 4, 200);
            Controls.Add(AbilityButtonArray);

            //unit info panel
            EntityInfoPanel = new EntityInfoPanel(250, 200);
            Controls.Add(EntityInfoPanel);

            //additional info
            AdditionalInfo = new AdditionalInfo(100, 200);
            Controls.Add(AdditionalInfo);
            AdditionalInfo.Stats.SetStats(
                new List<Stat>());

            //game options panel
            GameOptionsMenu = new GameOptionsMenu(250, 300, Game.GameplayOptions);
            GameOptionsMenu.Anchor = AnchorStyles.None;
            GameOptionsMenu.Left = (ClientSize.Width - GameOptionsMenu.Width) / 2;
            GameOptionsMenu.Top = (ClientSize.Height - GameOptionsMenu.Height) / 2;
            Controls.Add(GameOptionsMenu);
            Controls.SetChildIndex(GameOptionsMenu, 0);

            //add listeners to the buttons
            EntityButtonArray.ShowInfoOnClick(EntityInfoPanel, AbilityButtonArray, GameControls);
            AbilityButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            EntityInfoPanel.CommandButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            EntityInfoPanel.CommandButtonArray.RemoveCommandOnClick();
            EntityInfoPanel.StatusButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            AbilityButtonArray.SelectAbilityOnClick(GameControls);

            EntityButtonArray.GiveFocusTo(openGLControl1);
            AbilityButtonArray.GiveFocusTo(openGLControl1);
            EntityInfoPanel.CommandButtonArray.GiveFocusTo(openGLControl1);
            EntityInfoPanel.StatusButtonArray.GiveFocusTo(openGLControl1);

            //set position of ui elements
            //bottom panel
            int entityInfoW = EntityInfoPanel.Width;
            int entityButtonArrayW = EntityButtonArray.Width;
            int abilityButtonArrayW = AbilityButtonArray.Width;
            int additionalInfoW = AdditionalInfo.Width;
            int windowWidth = openGLControl1.Width;
            int windowHeight = openGLControl1.Height;
            int offsetX = (windowWidth - (entityInfoW + entityButtonArrayW + abilityButtonArrayW + additionalInfoW)) / 2;

            int entityInfoX = offsetX;
            EntityInfoPanel.Location = new Point(entityInfoX, windowHeight - EntityInfoPanel.Height);

            int entityButtonArrayX = entityInfoX + entityInfoW;
            EntityButtonArray.Location = new Point(entityButtonArrayX, windowHeight - EntityButtonArray.Height);

            int abilityPanelX = entityButtonArrayX + entityButtonArrayW;
            AbilityButtonArray.Location = new Point(abilityPanelX, windowHeight - AbilityButtonArray.Height);

            int additionalInfoX = abilityPanelX + abilityButtonArrayW;
            AdditionalInfo.Location = new Point(additionalInfoX, windowHeight - AdditionalInfo.Height);

            //menu button
            MenuButton = new Button()
            {
                Width = 80,
                Height = 40,
                Text = "Menu",
                Location = new Point(windowWidth - 80, 0)
            };
            MenuButton.GotFocus += (_s,_e)=>openGLControl1.Focus();
            MenuButton.Click += (_s, _e)=>OpenMenu();
            Controls.Add(MenuButton);

            //menu
            GameMenu = new GameMenu(150, 200);
            GameMenu.Anchor = AnchorStyles.None;
            GameMenu.Left = (ClientSize.Width - GameMenu.Width) / 2;
            GameMenu.Top = (ClientSize.Height - GameMenu.Height) / 2;
            GameMenu.SetResumeButtonClickHandler(CloseMenu);
            GameMenu.SetOptionsButtonClickHandler(OpenOptionsMenu);
            GameMenu.SetExitButtonClickHandler(CloseWindow);
            Controls.Add(GameMenu);
            Controls.SetChildIndex(GameMenu, 10);

            //players properties
            PlayerPropertiesPanel = new PlayerPropertiesPanel(120, 20);
            Controls.Add(PlayerPropertiesPanel);

            //put openGLFocus to the background and set focus to it
            Controls.SetChildIndex(openGLControl1, 100);
            openGLControl1.Focus();

            //GameOptionsMenu.Visible = false;
            GameMenu.Visible = false;
        }
        
        /// <summary>
        /// Updates information for the bottom panel and air taken.
        /// </summary>
        private void UpdateBottomPanel()
        {
            //only set new values if the values changed since the last update
            SelectedGroup selected = GameControls.SelectedEntities;
            List<Entity> selectedEntities = null;
            bool changed;
            lock (selected)
            {
                changed = GameControls.SelectedEntities.Changed;
                if (changed)
                    selectedEntities = selected.Entities.ToList();
            }
            if (changed)
            {
                GameControls.SelectedEntities.Changed = false;
                selectedEntities.Sort((u, v) => u.EntityType.GetHashCode() - v.EntityType.GetHashCode());
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
            EntityButtonArray.UpdateData();
            AbilityButtonArray.UpdateData();
            EntityInfoPanel.UpdateData();

            //update air
            PlayerPropertiesPanel.AirValue.Text = Game.CurrentPlayer.AirTaken + "/" + Game.CurrentPlayer.MaxAirTaken;
            
            //show winner window if a player won
            //if (Game.Winner != null)
            //    ShowWinnerPanel(Game.Winner.Value);
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
            lock (Game)
                Game.GameEnded = true;
        }

        /// <summary>
        /// Updates information about pressed keys.
        /// </summary>
        private void MainWinformWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.S: GameControls.MapMovementInput.AddDirection(Direction.DOWN); break;
                case Keys.W: GameControls.MapMovementInput.AddDirection(Direction.UP); break;
                case Keys.A: GameControls.MapMovementInput.AddDirection(Direction.LEFT); break;
                case Keys.D: GameControls.MapMovementInput.AddDirection(Direction.RIGHT); break;
                case Keys.Shift:
                    lock (GameControls.EntityCommandsInput)
                        GameControls.EntityCommandsInput.ResetCommandsQueue = false; break;
            }
        }

        /// <summary>
        /// Updates information about pressed keys.
        /// </summary>
        private void MainWinformWindow_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.S: GameControls.MapMovementInput.RemoveDirection(Direction.DOWN); break;
                case Keys.W: GameControls.MapMovementInput.RemoveDirection(Direction.UP); break;
                case Keys.A: GameControls.MapMovementInput.RemoveDirection(Direction.LEFT); break;
                case Keys.D: GameControls.MapMovementInput.RemoveDirection(Direction.RIGHT); break;
                case Keys.Shift:
                    lock (GameControls.EntityCommandsInput)
                        GameControls.EntityCommandsInput.ResetCommandsQueue = true; break;
            }
        }
        

        /// <summary>
        /// Changes the zoom of the map.
        /// </summary>
        private void Window_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                GameControls.MapView.ZoomIn(Game.Map);
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
            OpenGLAtlasDrawer.Initialize(gl, (float)openGLControl1.Width, (float)openGLControl1.Height);
            OpenGLAtlasDrawer.CreateMap(gl);
            OpenGLAtlasDrawer.CreateNutrientsMap(gl);
            OpenGLAtlasDrawer.CreateUnitCircles(gl);
            OpenGLAtlasDrawer.CreateEntities(gl);
            OpenGLAtlasDrawer.CreateEntitiesIndicators(gl);
            OpenGLAtlasDrawer.CreateFlowField(gl);
            OpenGLAtlasDrawer.CreateSelectionFrame(gl);
        }

        private int DrawingsDone { get; set; }
        
        
        private void openGLControl1_MouseButtonDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.EntityCommandsInput.NewPoint(mapCoordinates);

                //hide gui so that player can select from the whole screen
                //gui.Visibility = Visibility.Hidden;
                //set selected entity
                SelectEntity();
            }
            else
            {
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.EntityCommandsInput.SetTarget(mapCoordinates);
            }
        }

        private void openGLControl1_MouseButtonUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Vector2 mapCoordinates = GameControls.MapView
                .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.EntityCommandsInput.EndSelection(mapCoordinates);

                //turn gui back on
                //gui.Visibility = Visibility.Visible;
                //set selected entity
                SelectEntity();
            }
        }

        private void openGLControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (GameControls.EntityCommandsInput.State == EntityCommandsInputState.SELECTING_UNITS)
            {
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.EntityCommandsInput.NewPoint(mapCoordinates);

                //set selected unit
                SelectEntity();
            }
        }
        
        
        /// <summary>
        /// Opens menu and disables interaction with the game.
        /// </summary>
        private void OpenMenu()
        {
            //disable game and game info
            openGLControl1.Enabled = false;
            MenuButton.Enabled = false;
            EntityButtonArray.Enabled=false;
            AbilityButtonArray.Enabled=false;
            EntityInfoPanel.Enabled=false;
            AdditionalInfo.Enabled=false;
            GameOptionsMenu.Visible=false;

            //put focus to the main window
            Focus();

            //show menu
            GameMenu.Visible = true;
        }

        /// <summary>
        /// Opens menu and disables interaction with the game.
        /// </summary>
        private void CloseMenu()
        {
            //enable game and game info
            openGLControl1.Enabled = true;
            MenuButton.Enabled = true;
            EntityButtonArray.Enabled = true;
            AbilityButtonArray.Enabled = true;
            EntityInfoPanel.Enabled = true;
            AdditionalInfo.Enabled = true;
            GameOptionsMenu.Visible = false;
            openGLControl1.Focus();

            //hide menu
            GameMenu.Visible = false;
        }

        /*
        /// <summary>
        /// Closes menu and enables interaction with the game.
        /// </summary>
        private void menu_resume_Click(object sender, RoutedEventArgs e)
        {
            gui.IsEnabled = true;
            openGLControl1.IsEnabled = true;
            menuLayer.Visibility = Visibility.Hidden;
        }
        */
        /// <summary>
        /// Opens options menu.
        /// </summary>
        private void OpenOptionsMenu()
        {
            GameOptionsMenu.Visible = true;
        }

        /// <summary>
        /// Closes options menu.
        /// </summary>
        private void CloseOptionsMenu()
        {
            GameOptionsMenu.Visible = false;
        }

        /// <summary>
        /// Closes the game.
        /// </summary>
        private void CloseWindow()
        {
            Close();
        }/*

        bool victoryPanelShown = false;
        /// <summary>
        /// Shows panel anouncing winner of the game. Disables other ui. Only shows the panel once.
        /// </summary>
        private void ShowWinnerPanel(Players Winner)
        {
            //show the panel only once
            if (victoryPanelShown)
                return;

            victoryPanel.Visibility = Visibility.Visible;
            gui.IsEnabled = false;
            menuLayer.IsEnabled = false;
            openGLControl1.IsEnabled = false;
            victoryL.Content = Winner + " won!";
            victoryPanelShown = true;
        }

        /// <summary>
        /// Close victory panel.
        /// </summary>
        private void VictoryButton_Click(object sender, RoutedEventArgs e)
        {
            gui.IsEnabled = true;
            menuLayer.IsEnabled = true;
            openGLControl1.IsEnabled = true;
            victoryPanel.Visibility = Visibility.Hidden;
        }
        */

        /// <summary>
        /// Updates information about what should be drawn.
        /// </summary>
        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            MainLoopStep();

            OpenGL gl = openGLControl1.OpenGL;
            //Game has to be locked, because it can be used from the main loop
            lock (Game)
            {
                GameControls.MapView.SetActualExtents((float)openGLControl1.Width, (float)openGLControl1.Height);
                OpenGLAtlasDrawer.UpdateMapDataBuffers(gl, GameControls.MapView, Game);
                if (Game.GameplayOptions.NutrientsVisible)
                    OpenGLAtlasDrawer.UpdateNutrientsMapDataBuffers(gl, GameControls.MapView, Game);
                else
                    OpenGLAtlasDrawer.TryClearNutrientsMapDataBuffers(gl);
                OpenGLAtlasDrawer.UpdateEntityCirclesDataBuffers(gl, GameControls.MapView, Game);
                OpenGLAtlasDrawer.UpdateEntitiesDataBuffers(gl, GameControls.MapView, Game);
                OpenGLAtlasDrawer.UpdateEntityIndicatorsDataBuffers(gl, GameControls.MapView, Game);
                //show flowfield of the selected animal if an animal is selected and player wants to show flowfield
                /*if (Game.GameplayOptions.ShowFlowfield)
                {
                    Animal selected;
                    MoveToCommand command;
                    FlowField flF = null;
                    if ((selected = (EntityButtonArray.Selected as Animal)) != null &&
                        (command = (selected.CommandQueue.First() as MoveToCommand)) != null &&
                        (flF = command.FlowField) != null)
                    {
                        OpenGLAtlasDrawer.UpdateFlowFieldDataBuffers(gl, GameControls.MapView, flF);
                    }
                    else
                        OpenGLAtlasDrawer.TryClearFlowFieldDataBuffers(gl);
                }
                else
                    OpenGLAtlasDrawer.TryClearFlowFieldDataBuffers(gl);*/
                OpenGLAtlasDrawer.UpdateSelectionFrameDataBuffers(gl, GameControls.MapView, GameControls.MapSelectorFrame);
                UpdateBottomPanel();
            }
            OpenGLAtlasDrawer.Draw(gl, Game.GameplayOptions);

            DrawingsDone++;
            sw.Stop();
            //Console.WriteLine("Time drawing: " + sw.Elapsed.Milliseconds);
        }

        #endregion User interface
    }
}
