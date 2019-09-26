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
            GameControls = new GameControls(Game.Map);
            TotalStopwatch = new Stopwatch();
            TotalStopwatch.Start();


            //waint until the window initializes and then initialize bottom panel and opengl
            Shown += (s, e) =>
            {
                InitializeOpenGL();
                InitializeUserInterface();
            };
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
        /// Measures time from the start of the game.
        /// </summary>
        private Stopwatch TotalStopwatch { get; }
        /// <summary>
        /// Total time elapsed since the start of the game. Is set in
        /// the main loop.
        /// </summary>
        private double TotalTime { get; set; }
        /// <summary>
        /// Total number of game updates.
        /// </summary>
        private int UpdatesDone { get; set; }

        /// <summary>
        /// One update of the game.
        /// </summary>
        private void MainLoopStep()
        {
            if (Game.GameEnded)
                return;

            GameControls.UpdateMapView(Game.Map);
            GameControls.UpdateEntitiesByInput(Game);

            //update the state of the game
            long totalEl = TotalStopwatch.ElapsedMilliseconds;
            float deltaT = (totalEl - (float)TotalTime) / 1000f;
            TotalTime = totalEl;
            Game.Update(deltaT);

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
        private VictoryPanel VictoryPanel { get; set; }

        #region Initialization
        /// <summary>
        /// Initializes user interface.
        /// </summary>
        private void InitializeUserInterface()
        {
            MouseWheel += Window_MouseWheel;

            InitializeBottomPanel();

            //menu button
            MenuButton = new Button()
            {
                Width = 80,
                Height = 40,
                Text = "Menu",
                Location = new Point(ClientSize.Width - 80, 0)
            };
            MenuButton.GotFocus += (_s,_e)=>openGLControl.Focus();
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

            //game options panel
            GameOptionsMenu = new GameOptionsMenu(250, 300, Game.GameplayOptions);
            GameOptionsMenu.Anchor = AnchorStyles.None;
            GameOptionsMenu.Left = (ClientSize.Width - GameOptionsMenu.Width) / 2;
            GameOptionsMenu.Top = (ClientSize.Height - GameOptionsMenu.Height) / 2;
            Controls.Add(GameOptionsMenu);
            Controls.SetChildIndex(GameOptionsMenu, 5);

            //victory panel
            VictoryPanel = new VictoryPanel(200, 150);
            VictoryPanel.Left = (ClientSize.Width - VictoryPanel.Width) / 2;
            VictoryPanel.Top = (ClientSize.Height - VictoryPanel.Height) / 2;
            VictoryPanel.Ok.Click += (_s, _e) => HideVictoryPanel();
            Controls.Add(VictoryPanel);
            Controls.SetChildIndex(VictoryPanel, 0);

            //players properties
            PlayerPropertiesPanel = new PlayerPropertiesPanel(120, 20);
            Controls.Add(PlayerPropertiesPanel);

            //put openGLFocus to the background and set focus to it
            Controls.SetChildIndex(openGLControl, 100);
            openGLControl.Focus();

            VictoryPanel.Visible = false;
            GameOptionsMenu.Visible = false;
            GameMenu.Visible = false;
        }

        /// <summary>
        /// Initialize controls of the bottom panel.
        /// </summary>
        private void InitializeBottomPanel()
        {
            //initialize ui elements
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

            //add listeners to the buttons
            EntityButtonArray.ShowInfoOnClick(EntityInfoPanel, AbilityButtonArray, GameControls);
            AbilityButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            EntityInfoPanel.CommandButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            EntityInfoPanel.CommandButtonArray.RemoveCommandOnClick();
            EntityInfoPanel.StatusButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            AbilityButtonArray.SelectAbilityOnClick(GameControls);

            EntityButtonArray.GiveFocusTo(openGLControl);
            AbilityButtonArray.GiveFocusTo(openGLControl);
            EntityInfoPanel.CommandButtonArray.GiveFocusTo(openGLControl);
            EntityInfoPanel.StatusButtonArray.GiveFocusTo(openGLControl);

            //set position of ui elements
            //bottom panel
            int entityInfoW = EntityInfoPanel.Width;
            int entityButtonArrayW = EntityButtonArray.Width;
            int abilityButtonArrayW = AbilityButtonArray.Width;
            int additionalInfoW = AdditionalInfo.Width;
            int windowWidth = openGLControl.Width;
            int windowHeight = openGLControl.Height;
            int offsetX = (windowWidth - (entityInfoW + entityButtonArrayW + abilityButtonArrayW + additionalInfoW)) / 2;

            int entityInfoX = offsetX;
            EntityInfoPanel.Location = new Point(entityInfoX, windowHeight - EntityInfoPanel.Height);

            int entityButtonArrayX = entityInfoX + entityInfoW;
            EntityButtonArray.Location = new Point(entityButtonArrayX, windowHeight - EntityButtonArray.Height);

            int abilityPanelX = entityButtonArrayX + entityButtonArrayW;
            AbilityButtonArray.Location = new Point(abilityPanelX, windowHeight - AbilityButtonArray.Height);

            int additionalInfoX = abilityPanelX + abilityButtonArrayW;
            AdditionalInfo.Location = new Point(additionalInfoX, windowHeight - AdditionalInfo.Height);
        }

        /// <summary>
        /// Initializes OpenGLAtlasDrawer.
        /// </summary>
        private void InitializeOpenGL()
        {
            openGLControl.FrameRate = 60;
            OpenGL gl = openGLControl.OpenGL;
            OpenGLAtlasDrawer.Initialize(gl, (float)openGLControl.Width, (float)openGLControl.Height);
            OpenGLAtlasDrawer.CreateMap(gl);
            OpenGLAtlasDrawer.CreateNutrientsMap(gl);
            OpenGLAtlasDrawer.CreateUnitCircles(gl);
            OpenGLAtlasDrawer.CreateEntities(gl);
            OpenGLAtlasDrawer.CreateEntitiesIndicators(gl);
            OpenGLAtlasDrawer.CreateFlowField(gl);
            OpenGLAtlasDrawer.CreateSelectionFrame(gl);
        }
        #endregion Initialization

        #region Updates

        /// <summary>
        /// Updates and draws the game.
        /// </summary>
        private void UpdateAndDraw(object sender, RenderEventArgs args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            MainLoopStep();

            OpenGL gl = openGLControl.OpenGL;
            //set correct extents of the window to game controls
            GameControls.MapView.SetActualExtents(openGLControl.Width, openGLControl.Height);

            //data buffers
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
                FlowField flF = null;
                if ((flF=SelectedAnimalFlowfield())!=null)
                    OpenGLAtlasDrawer.UpdateFlowFieldDataBuffers(gl, GameControls.MapView, flF);
                else
                    OpenGLAtlasDrawer.TryClearFlowFieldDataBuffers(gl);
            }
            else
                OpenGLAtlasDrawer.TryClearFlowFieldDataBuffers(gl);
            OpenGLAtlasDrawer.UpdateSelectionFrameDataBuffers(gl, GameControls.MapView, GameControls.MapSelectorFrame);
            OpenGLAtlasDrawer.Draw(gl, Game.GameplayOptions);
            
            //update bottom panel
            UpdateBottomPanel();

            sw.Stop();
            //Console.WriteLine("Tick length: " + sw.Elapsed.Milliseconds);

            UpdatesDone++;
            Console.WriteLine("Updates per second: " + UpdatesDone / TotalTime * 1000);
        }

        /// <summary>
        /// True if the first entity of selected entities should be set selected entity.
        /// </summary>
        private bool ShouldSetSelected { get; set; }

        /// <summary>
        /// Updates information for the bottom panel and air taken.
        /// </summary>
        private void UpdateBottomPanel()
        {
            //update selected entities
            if (GameControls.SelectedGroup.Changed)
            {
                //selected entities changed since the last update
                GameControls.SelectedGroup.Changed = false;
                List<Entity> selectedEntities = GameControls.SelectedGroup.Entities;
                //update selected entity if the old one was removed or player is currently selecting entities
                if(GameControls.EntityCommandsInput.State == EntityCommandsInputState.SELECTING_UNITS ||
                    ShouldSetSelected ||
                    !selectedEntities.Contains(EntityButtonArray.Selected))
                    EntityButtonArray.Selected = selectedEntities.FirstOrDefault();
                ShouldSetSelected = GameControls.EntityCommandsInput.State == EntityCommandsInputState.SELECTING_UNITS;
                EntityButtonArray.InfoSources = selectedEntities;
            }
            EntityInfoPanel.SelectedEntity = EntityButtonArray.Selected;

            //set selected ability
            AbilityButtonArray.Selected = GameControls.EntityCommandsInput.SelectedAbility;

            if (EntityButtonArray.Selected != null)
                AbilityButtonArray.InfoSources = EntityButtonArray.Selected.Abilities;
            else
                AbilityButtonArray.InfoSources = new List<Ability>();
            
            //update entities and abilities panels
            EntityButtonArray.UpdateControl();
            AbilityButtonArray.UpdateControl();
            EntityInfoPanel.UpdateControl();

            //update air
            PlayerPropertiesPanel.AirValue.Text = Game.CurrentPlayer.AirTaken + "/" + Game.CurrentPlayer.MaxAirTaken;
            
            //show victory panel if a player won
            if (Game.Winner != null)
                ShowVictoryPanel(Game.Winner.Value);
        }

        #endregion Updates

        #region Event handlers

        /// <summary>
        /// Ends the thread with Game when the window closes.
        /// </summary>
        private void MainWinformWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
                case Keys.ShiftKey:
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
                case Keys.ShiftKey:
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
        
        
        private void MouseButtonDownHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //update vertex of frame buffer
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.EntityCommandsInput.NewPoint(mapCoordinates);
                //set the mode of entity selection
                switch (ModifierKeys)
                {
                    case Keys.Shift:
                        GameControls.SelectedGroup.NextOperation = Operation.ADD;
                        break;
                    case Keys.Control:
                        GameControls.SelectedGroup.NextOperation = Operation.SUBTRACT;
                        break;
                    default:
                        GameControls.SelectedGroup.NextOperation = Operation.REPLACE;
                        break;
                }


                SelectEntity();
            }
            else
            {
                //set target
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.EntityCommandsInput.SetTarget(mapCoordinates);
            }
        }

        private void MouseButtonUpHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Vector2 mapCoordinates = GameControls.MapView
                .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.EntityCommandsInput.EndSelection(mapCoordinates);
                
                SelectEntity();
            }
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (GameControls.EntityCommandsInput.State == EntityCommandsInputState.SELECTING_UNITS)
            {
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.EntityCommandsInput.NewPoint(mapCoordinates);
                
                SelectEntity();
            }
        }

        #endregion event handlers

        /// <summary>
        /// Sets currently selected entity.
        /// </summary>
        private void SelectEntity()
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
        /// Returns flowfield of first command of selected entity if it is
        /// a MoveToCommand and the entity is animal.
        /// </summary>
        /// <returns></returns>
        private FlowField SelectedAnimalFlowfield()
        {
            Animal selected;
            MoveToCommand command;
            FlowField flF = null;
            if ((selected = (EntityButtonArray.Selected as Animal)) != null &&
                (command = (selected.CommandQueue.First() as MoveToCommand)) != null)
                flF = command.FlowField;
            return flF;
        }

        /// <summary>
        /// Opens menu and disables interaction with the game.
        /// </summary>
        private void OpenMenu()
        {
            DisableGameControls();

            //show menu
            GameMenu.Visible = true;
        }

        /// <summary>
        /// Opens menu and disables interaction with the game.
        /// </summary>
        private void CloseMenu()
        {
            EnableGameControls();

            //hide menu
            GameMenu.Visible = false;
        }
        
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
        /// Disables opengl control, bottom panel and menu button.
        /// Gives focus to this window.
        /// </summary>
        private void DisableGameControls()
        {
            //disable game and game info
            openGLControl.Enabled = false;
            MenuButton.Enabled = false;
            EntityButtonArray.Enabled = false;
            AbilityButtonArray.Enabled = false;
            EntityInfoPanel.Enabled = false;
            AdditionalInfo.Enabled = false;
            GameOptionsMenu.Visible = false;

            //put focus to the main window
            Focus();
        }

        /// <summary>
        /// Enables opengl control, bottom panel and menu button.
        /// Gives focus to opengl control.
        /// </summary>
        private void EnableGameControls()
        {
            //enable game and game info
            openGLControl.Enabled = true;
            MenuButton.Enabled = true;
            EntityButtonArray.Enabled = true;
            AbilityButtonArray.Enabled = true;
            EntityInfoPanel.Enabled = true;
            AdditionalInfo.Enabled = true;
            GameOptionsMenu.Visible = false;

            //put focus to the game
            openGLControl.Focus();
        }

        /// <summary>
        /// Closes the game.
        /// </summary>
        private void CloseWindow()
        {
            Close();
        }

        /// <summary>
        /// Shows panel anouncing winner of the game. Disables other ui. Only shows the panel once.
        /// </summary>
        private void ShowVictoryPanel(Players Winner)
        {
            //show the panel only once
            if (VictoryPanel.AlreadyShown)
                return;

            VictoryPanel.Visible = true;
            DisableGameControls();
            GameMenu.Visible = false;
            VictoryPanel.Message.Text = Winner + " won!";
            VictoryPanel.AlreadyShown = true;
        }

        /// <summary>
        /// Close victory panel.
        /// </summary>
        private void HideVictoryPanel()
        {
            EnableGameControls();
            VictoryPanel.Visible = false;
        }
        
        #endregion User interface
    }
}
