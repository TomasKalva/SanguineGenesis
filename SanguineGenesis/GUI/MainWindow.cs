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
        private ControlGroupButtonArray ControlGroupButtonArray { get; set; }
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
            //add event handlers
            MouseWheel += Window_MouseWheel;
            Timer timer = new Timer();
            timer.Tick += MapMovementTimer_Tick;
            timer.Enabled = true;
            timer.Interval = 10;
            openGLControl.PreviewKeyDown += OpenGLControl_PreviewKeyDown;

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
            GameMenu = new GameMenu(150, 200)
            {
                Anchor = AnchorStyles.None
            };
            GameMenu.Left = (ClientSize.Width - GameMenu.Width) / 2;
            GameMenu.Top = (ClientSize.Height - GameMenu.Height) / 2;
            GameMenu.SetResumeButtonClickHandler(CloseMenu);
            GameMenu.SetOptionsButtonClickHandler(OpenOptionsMenu);
            GameMenu.SetExitButtonClickHandler(CloseWindow);
            Controls.Add(GameMenu);
            Controls.SetChildIndex(GameMenu, 10);

            //game options panel
            GameOptionsMenu = new GameOptionsMenu(250, 300, Game.GameplayOptions)
            {
                Anchor = AnchorStyles.None
            };
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
            EntityButtonArray = new EntityButtonArray(8, 5, 300, 188);
            Controls.Add(EntityButtonArray);

            //abilities panel
            AbilityButtonArray = new AbilityButtonArray(4, 3, 200, 200);
            Controls.Add(AbilityButtonArray);

            //unit info panel
            EntityInfoPanel = new EntityInfoPanel(250, 200);
            Controls.Add(EntityInfoPanel);

            //additional info
            AdditionalInfo = new AdditionalInfo(100, 200);
            Controls.Add(AdditionalInfo);
            AdditionalInfo.Stats.SetStats(
                new List<Stat>());

            //control groups panel
            ControlGroupButtonArray = new ControlGroupButtonArray(6, 1, EntityButtonArray.Width, 20);
            Controls.Add(ControlGroupButtonArray);

            //add listeners to the buttons
            EntityButtonArray.ShowInfoOnClick(EntityInfoPanel, AbilityButtonArray, GameControls, GameControls.EntityCommandsInput);
            AbilityButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            EntityInfoPanel.CommandButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            EntityInfoPanel.CommandButtonArray.RemoveCommandOnClick();
            EntityInfoPanel.StatusButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            AbilityButtonArray.SelectAbilityOnClick(GameControls);
            ControlGroupButtonArray.SetEventHandlers(GameControls);

            EntityButtonArray.GiveFocusTo(openGLControl);
            AbilityButtonArray.GiveFocusTo(openGLControl);
            ControlGroupButtonArray.GiveFocusTo(openGLControl);
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

            ControlGroupButtonArray.Location = new Point(entityButtonArrayX, windowHeight - EntityButtonArray.Height - ControlGroupButtonArray.Height);

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
        /// Tab is not considered input key, this fixes it.
        /// </summary>
        private void OpenGLControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if(e.KeyData == Keys.Tab)
            {
                e.IsInputKey = true;
            }
        }

        /// <summary>
        /// Updates information about pressed keys.
        /// </summary>
        private void MainWinformWindow_KeyDown(object sender, KeyEventArgs e)
        {
            int abilityIndex;
            int ctrlGroupIndex;
            if ((abilityIndex = AbilityButtonArray.KeyToAbilityIndex(e.KeyCode))!=-1)
            {
                //select the ability
                AbilityButtonArray.SelectAbilityWithIndex(abilityIndex);
            }
            else if(e.KeyCode == Keys.ShiftKey)
                //abilities will be appended to the queue
                GameControls.EntityCommandsInput.ResetCommandsQueue = false;
            else if (e.KeyCode == Keys.Tab)
                //jump to next entity type
                SelectEntityOfNextType();
            else if ((ctrlGroupIndex = ControlGroupButtonArray.KeyToGroupIndex(e.KeyCode)) != -1)
            {
                //modify control groups
                if (e.Control)
                {
                    ControlGroupButtonArray.SaveGroupWithIndex(ctrlGroupIndex);
                }
                else if(GameControls.EntityCommandsInput.State==EntityCommandsInputState.IDLE ||
                    GameControls.EntityCommandsInput.State == EntityCommandsInputState.UNITS_SELECTED)
                {
                    if (e.Shift)
                        GameControls.SelectedGroup.NextOperation = Operation.ADD;

                    ControlGroupButtonArray.LoadGroupWithIndex(ctrlGroupIndex);

                }
            }
        }

        /// <summary>
        /// Updates information about pressed keys.
        /// </summary>
        private void MainWinformWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                GameControls.EntityCommandsInput.ResetCommandsQueue = true;
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
                //update selection frame
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.EntityCommandsInput.NewPoint(mapCoordinates);
                
                SelectEntity();
            }
        }

        public void MapMovementTimer_Tick(object sender, EventArgs e)
        {
            //move map if player is not selecting entities
            if (!(GameControls.EntityCommandsInput.State == EntityCommandsInputState.SELECTING_UNITS))
            {
                Point mousePos = Cursor.Position;

                int movingFrameSize = 2;
                if (mousePos.X >= openGLControl.Width - movingFrameSize)
                {
                    GameControls.MapMovementInput.AddDirection(Direction.RIGHT);
                    GameControls.MapMovementInput.RemoveDirection(Direction.LEFT);
                }
                else if (mousePos.X <= movingFrameSize)
                {
                    GameControls.MapMovementInput.AddDirection(Direction.LEFT);
                    GameControls.MapMovementInput.RemoveDirection(Direction.RIGHT);
                }
                else
                {
                    GameControls.MapMovementInput.RemoveDirection(Direction.LEFT);
                    GameControls.MapMovementInput.RemoveDirection(Direction.RIGHT);
                }

                if (mousePos.Y >= openGLControl.Height - movingFrameSize)
                {
                    GameControls.MapMovementInput.AddDirection(Direction.DOWN);
                    GameControls.MapMovementInput.RemoveDirection(Direction.UP);
                }
                else if (mousePos.Y <= movingFrameSize)
                {
                    GameControls.MapMovementInput.AddDirection(Direction.UP);
                    GameControls.MapMovementInput.RemoveDirection(Direction.DOWN);
                }
                else
                {
                    GameControls.MapMovementInput.RemoveDirection(Direction.UP);
                    GameControls.MapMovementInput.RemoveDirection(Direction.DOWN);
                }
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
                GameControls.EntityCommandsInput.SelectedAbility = null;
            }
        }

        /// <summary>
        /// Selects entity of the next type.
        /// </summary>
        private void SelectEntityOfNextType()
        {
            List<Entity> selectedEntities = EntityButtonArray.InfoSources;
            Entity selectedEntity = EntityButtonArray.Selected;
            if (selectedEntities != null &&
                selectedEntities.Any() &&
                selectedEntity != null)
            {
                string currentType = selectedEntity.EntityType;
                int start = selectedEntities.IndexOf(selectedEntity);
                int length = selectedEntities.Count;
                int i = (start + 1) % length;
                while (i != start)
                {
                    if(selectedEntities[i].EntityType!=currentType)
                    {
                        EntityButtonArray.Selected = selectedEntities[i];
                        GameControls.EntityCommandsInput.SelectedAbility = null;
                        break;
                    }
                    i = (i + 1) % length;
                }
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
