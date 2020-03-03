using SanguineGenesis.GameControls;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GameLogic.Maps.MovementGenerating;
using SanguineGenesis.GUI.WinFormsComponents;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static SanguineGenesis.GUI.MainMenuWindow;

namespace SanguineGenesis.GUI
{
    partial class MainWinformWindow : Form
    {
        /// <summary>
        /// Runs main loop of the game.
        /// </summary>
        Timer GameUpdateTimer { get; }

        public MainWinformWindow(MapDescription mapDescription, Biome playersBiome, Icons icons, bool testAnimals)
        {
            InitializeComponent();
            
            //initialize game
            Game = new Game(mapDescription, playersBiome);
            //spawn testing animals
            if (testAnimals)
                Game.SpawnTestingAnimals();
            //initialize game controls
            GameControls = new GameControls.GameControls();
            //initialize game time
            GameTime = new GameTime(Console.Out);

            //create and enable game update timer
            GameUpdateTimer = new Timer();
            GameUpdateTimer.Tick += GameUpdateTimer_MainLoop;
            GameUpdateTimer.Interval = 10;

            //waint until the window initializes and then initialize bottom panel and opengl
            Shown += (s, e) =>
            {
                InitializeOpenGL();
                InitializeUserInterface(icons);
                GameUpdateTimer.Enabled = true;
            };
        }

        #region Game logic

        /// <summary>
        /// The game this window is showing.
        /// </summary>
        private Game Game { get; }
        /// <summary>
        /// Used for manipulating the game by player.
        /// </summary>
        private GameControls.GameControls GameControls { get; }
        /// <summary>
        /// Measures the ingame time.
        /// </summary>
        private GameTime GameTime { get; }

        public void GameUpdateTimer_MainLoop(object sender, EventArgs e)
        {
            if (Game.GameEnded)
                return;

            //update time
            GameTime.NextStep();

            //update MapMovementInput
            UpdateMoveMap();
            //move map view
            GameControls.MoveMapView(Game.Map, GameTime.DeltaT);

            //update selected entities with player's input
            GameControls.UpdateEntitiesByInput(Game);

            //update the state of the game
            Game.Update(GameTime);

            openGLControl.Refresh();
        }

        #endregion Game logic

        #region User interface

        /// <summary>
        /// Shows selected entities.
        /// </summary>
        private EntityButtonArray EntityButtonArray { get; set; }
        /// <summary>
        /// Shows abilities of selected entity.
        /// </summary>
        private AbilityButtonArray AbilityButtonArray { get; set; }
        /// <summary>
        /// Info about selected entity.
        /// </summary>
        private EntityInfoPanel EntityInfoPanel { get; set; }
        /// <summary>
        /// Info about ability or status.
        /// </summary>
        private AdditionalInfo AdditionalInfo { get; set; }
        /// <summary>
        /// Shows control groups.
        /// </summary>
        private ControlGroupButtonArray ControlGroupButtonArray { get; set; }
        /// <summary>
        /// Allows player to modify appearance of the game.
        /// </summary>
        private GameOptionsMenu GameOptionsMenu { get; set; }
        /// <summary>
        /// Button for opening menu.
        /// </summary>
        private Button MenuButton { get; set; }
        /// <summary>
        /// Contains buttons for closing the game and opening options menu.
        /// </summary>
        private GameMenu GameMenu { get; set; }
        /// <summary>
        /// Shows info about current player.
        /// </summary>
        private PlayerPropertiesPanel PlayerPropertiesPanel { get; set; }
        /// <summary>
        /// Shows who won the game.
        /// </summary>
        private VictoryPanel VictoryPanel { get; set; }

        #region Initialization
        /// <summary>
        /// Initializes user interface.
        /// </summary>
        private void InitializeUserInterface(Icons icons)
        {
            //initialize bottom panel
            InitializeBottomPanel(icons);

            //add event handlers to this window and opengl control
            MouseWheel += Window_MouseWheel;
            openGLControl.PreviewKeyDown += OpenGLControl_PreviewKeyDown;

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
        private void InitializeBottomPanel(Icons icons)
        {
            //set icons to ButtonArray
            ButtonArray.Icons = icons;

            //initialize ui elements
            //units panel
            EntityButtonArray = new EntityButtonArray(8, 5, 300, 188);
            Controls.Add(EntityButtonArray);

            //abilities panel
            AbilityButtonArray = new AbilityButtonArray(4, 3, 200, 200);
            Controls.Add(AbilityButtonArray);

            //entity info panel
            EntityInfoPanel = new EntityInfoPanel(350, 220);
            Controls.Add(EntityInfoPanel);

            //additional info
            AdditionalInfo = new AdditionalInfo(150, 200);
            Controls.Add(AdditionalInfo);
            AdditionalInfo.Stats.SetStats(
                new List<Stat>());

            //control groups panel
            ControlGroupButtonArray = new ControlGroupButtonArray(6, EntityButtonArray.Width, 20);
            Controls.Add(ControlGroupButtonArray);

            //add listeners to the buttons
            EntityButtonArray.ShowInfoOnClick(GameControls, GameControls.SelectionInput);
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
            ControlGroupButtonArray.GiveFocusTo(openGLControl);

            //set position of ui elements on the bottom panel
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

            ControlGroupButtonArray.Location = new Point(
                entityButtonArrayX + (entityButtonArrayW - ControlGroupButtonArray.Width)/2,
                windowHeight - EntityButtonArray.Height - ControlGroupButtonArray.Height);

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
            try
            {
                OpenGL gl = openGLControl.OpenGL;
                OpenGLAtlasDrawer.Initialize(gl, (float)openGLControl.Width, (float)openGLControl.Height);
                OpenGLAtlasDrawer.CreateMap(gl);
                OpenGLAtlasDrawer.CreateNutrientsMap(gl);
                OpenGLAtlasDrawer.CreateUnitCircles(gl);
                OpenGLAtlasDrawer.CreateEntities(gl);
                OpenGLAtlasDrawer.CreateEntitiesIndicators(gl);
                OpenGLAtlasDrawer.CreateFlowField(gl);
                OpenGLAtlasDrawer.CreateSelectionFrame(gl);
            }catch(Exception e)
            {
                MessageBox.Show("Failed to initialize window: "+e.Message);
                CloseWindow();
            }
        }
        #endregion Initialization

        #region Event handlers

        /// <summary>
        /// Draws the game.
        /// </summary>
        private void Draw(object sender, RenderEventArgs args)
        {
            OpenGL gl = openGLControl.OpenGL;

            //set correct extents of the window to game controls
            GameControls.MapView.SetActualExtents(openGLControl.Width, openGLControl.Height);

            //update data buffers

            //maps data
            {
                //map
                OpenGLAtlasDrawer.UpdateMapDataBuffers(gl, GameControls.MapView, Game);

                //nutrients map
                if (Game.GameplayOptions.NutrientsVisible)
                    OpenGLAtlasDrawer.UpdateNutrientsMapDataBuffers(gl, GameControls.MapView, Game);
                else
                    OpenGLAtlasDrawer.TryClearNutrientsMapDataBuffers(gl);

                //flowfield
                if (Game.GameplayOptions.ShowFlowfield)
                {
                    FlowField flF;
                    if ((flF = SelectedAnimalFlowfield()) != null)
                        //update flowfield with data of the selected animal if an animal is selected
                        OpenGLAtlasDrawer.UpdateFlowFieldDataBuffers(gl, GameControls.MapView, flF);
                    else
                        //clear flowfield
                        OpenGLAtlasDrawer.TryClearFlowFieldDataBuffers(gl);
                }
                else
                    //clear flowfield
                    OpenGLAtlasDrawer.TryClearFlowFieldDataBuffers(gl);
            }

            //set entities data
            {
                //entity circles
                OpenGLAtlasDrawer.UpdateEntityCirclesDataBuffers(gl, GameControls.MapView, Game);
                //entity images
                OpenGLAtlasDrawer.UpdateEntitiesDataBuffers(gl, GameControls.MapView, Game);
                //entity health+energy indicators
                OpenGLAtlasDrawer.UpdateEntityIndicatorsDataBuffers(gl, GameControls.MapView, Game.CurrentPlayer, Game);
            }

            //selection frame
            OpenGLAtlasDrawer.UpdateSelectionFrameDataBuffers(gl, GameControls.MapView, GameControls.MapSelectorFrame);


            //draw the data
            OpenGLAtlasDrawer.Draw(gl, Game.GameplayOptions);

            //update bottom panel
            UpdateBottomPanel();

            GameTime.PrintTime("Graphics tick length");
            GameTime.PrintFPS();
            GameTime.Delimiter();
        }

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
                GameControls.SelectionInput.ResetCommandsQueue = false;
            else if (e.KeyCode == Keys.Tab)
                //jump to next entity type
                SelectEntityOfNextType();
            else if ((ctrlGroupIndex = ControlGroupButtonArray.KeyToGroupIndex(e.KeyCode)) != -1)
            {
                //modify control groups
                if (e.Control)
                {
                    //saves selected entities to the group
                    ControlGroupButtonArray.SaveGroupWithIndex(ctrlGroupIndex);
                }
                else if(GameControls.SelectionInput.State==SelectionInputState.IDLE ||
                    GameControls.SelectionInput.State == SelectionInputState.UNITS_SELECTED)
                {
                    //loads selected entities from the group
                    if (e.Shift)
                        GameControls.SelectedGroup.NextOperation = Operation.ADD;

                    ControlGroupButtonArray.LoadGroupWithIndex(ctrlGroupIndex);

                }
            }
        }

        /// <summary>
        /// Updates information about shift being pressed.
        /// </summary>
        private void MainWinformWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                GameControls.SelectionInput.ResetCommandsQueue = true;
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
        /// If left button is pressed, starts selecting of selected entities. If right button
        /// is pressed, chooses target of selected ability.
        /// </summary>
        private void MouseButtonDownHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //update vertex of selector frame
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.SelectionInput.NewPoint(mapCoordinates);
                
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

                //choose selected entity
                SelectEntity();
            }
            else
            {
                //set target
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.SelectionInput.SetTarget(mapCoordinates);
            }
        }

        /// <summary>
        /// Finishes selecting entities.
        /// </summary>
        private void MouseButtonUpHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //finish selecting entities
                Vector2 mapCoordinates = GameControls.MapView
                .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.SelectionInput.EndSelection(mapCoordinates);
                
                //choose selected entity
                SelectEntity();
            }
        }

        /// <summary>
        /// Update selected entities.
        /// </summary>
        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (GameControls.SelectionInput.State == SelectionInputState.SELECTING_UNITS)
            {
                //update selection frame
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.SelectionInput.NewPoint(mapCoordinates);
                
                //choose selected entity
                SelectEntity();
            }
        }

        #endregion event handlers

        #region Utility methods for manipulating controls

        /// <summary>
        /// True if EntityButtonArray.Selected should be set to the first entity of GameControls.SelectedGroup.Entities.
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
                selectedEntities.Sort((e1, e2) => string.Compare(e1.EntityType, e2.EntityType));

                //update selected entity if the old one was removed or player is currently selecting entities
                if (GameControls.SelectionInput.State == SelectionInputState.SELECTING_UNITS ||
                    ShouldSetSelected ||
                    !selectedEntities.Contains(EntityButtonArray.Selected))
                    EntityButtonArray.Selected = selectedEntities.FirstOrDefault();
                ShouldSetSelected = GameControls.SelectionInput.State == SelectionInputState.SELECTING_UNITS;

                //set selected entities
                EntityButtonArray.InfoSources = selectedEntities;
            }
            EntityInfoPanel.SelectedEntity = EntityButtonArray.Selected;

            //set selected ability
            AbilityButtonArray.Selected = GameControls.SelectionInput.SelectedAbility;
            if (EntityButtonArray.Selected != null)
                AbilityButtonArray.InfoSources = EntityButtonArray.Selected.Abilities;
            else
                AbilityButtonArray.InfoSources = new List<Ability>();

            //update entities and abilities panels
            EntityButtonArray.UpdateControl();
            AbilityButtonArray.UpdateControl();
            EntityInfoPanel.UpdateControl();
            ControlGroupButtonArray.UpdateControl();

            //update air
            PlayerPropertiesPanel.AirValue.Text = Game.CurrentPlayer.AirTaken + "/" + Game.CurrentPlayer.MaxAirTaken;

            //show victory panel if a player won
            if (Game.Winner != null)
                ShowVictoryPanel(Game.Winner.Value);
        }

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
                GameControls.SelectionInput.SelectedAbility = null;
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
                        GameControls.SelectionInput.SelectedAbility = null;
                        break;
                    }
                    i = (i + 1) % length;
                }
            }
        }

        /// <summary>
        /// Returns flowfield of first command of selected entity if it is
        /// a MoveToCommand and the entity is animal. Returns null otherwise.
        /// </summary>
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
            ControlGroupButtonArray.Enabled = false;
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
            ControlGroupButtonArray.Enabled = true;
            GameOptionsMenu.Visible = false;

            //put focus to the game
            openGLControl.Focus();
        }

        /// <summary>
        /// Closes the game.
        /// </summary>
        private void CloseWindow()
        {
            // openGLControl is directly referenced by GCHandle, event handlers have to be removed
            // to avoid memory leak
            openGLControl.PreviewKeyDown -= OpenGLControl_PreviewKeyDown;
            openGLControl.OpenGLDraw -= new SharpGL.RenderEventHandler(this.Draw);
            openGLControl.KeyDown -= new System.Windows.Forms.KeyEventHandler(this.MainWinformWindow_KeyDown);
            openGLControl.KeyUp -= new System.Windows.Forms.KeyEventHandler(this.MainWinformWindow_KeyUp);
            openGLControl.MouseDown -= new System.Windows.Forms.MouseEventHandler(this.MouseButtonDownHandler);
            openGLControl.MouseMove -= new System.Windows.Forms.MouseEventHandler(this.MouseMoveHandler);
            openGLControl.MouseUp -= new System.Windows.Forms.MouseEventHandler(this.MouseButtonUpHandler);
            GameUpdateTimer.Tick -= GameUpdateTimer_MainLoop;
            Close();
        }

        /// <summary>
        /// Shows panel anouncing winner of the game. Disables other ui. Only shows the panel once.
        /// </summary>
        private void ShowVictoryPanel(FactionType Winner)
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

        /// <summary>
        /// Updates MapMovementInput with mouse input if player is not selecting entities.
        /// </summary>
        public void UpdateMoveMap()
        {
            //check if player is selecting entities
            if (!(GameControls.SelectionInput.State == SelectionInputState.SELECTING_UNITS))
            {
                Point mousePos = Cursor.Position;

                int movingFrameSize = 2;
                if (mousePos.X >= openGLControl.Width - movingFrameSize)
                {
                    //move right
                    GameControls.MapMovementInput.AddDirection(Direction.RIGHT);
                    GameControls.MapMovementInput.RemoveDirection(Direction.LEFT);
                }
                else if (mousePos.X <= movingFrameSize)
                {
                    //move left
                    GameControls.MapMovementInput.AddDirection(Direction.LEFT);
                    GameControls.MapMovementInput.RemoveDirection(Direction.RIGHT);
                }
                else
                {
                    //stop moving horizontally
                    GameControls.MapMovementInput.RemoveDirection(Direction.LEFT);
                    GameControls.MapMovementInput.RemoveDirection(Direction.RIGHT);
                }

                if (mousePos.Y >= openGLControl.Height - movingFrameSize)
                {
                    //move down
                    GameControls.MapMovementInput.AddDirection(Direction.DOWN);
                    GameControls.MapMovementInput.RemoveDirection(Direction.UP);
                }
                else if (mousePos.Y <= movingFrameSize)
                {
                    //move up
                    GameControls.MapMovementInput.AddDirection(Direction.UP);
                    GameControls.MapMovementInput.RemoveDirection(Direction.DOWN);
                }
                else
                {
                    //stop moving vertically
                    GameControls.MapMovementInput.RemoveDirection(Direction.UP);
                    GameControls.MapMovementInput.RemoveDirection(Direction.DOWN);
                }
            }
        }

        #endregion Utility methods for manipulating controls

        #endregion User interface
    }
}

/// <summary>
/// Represents ingame time. Can be used to measure how long certain actions of the game take.
/// </summary>
class GameTime
{
    /// <summary>
    /// Measures time from the start of the game.
    /// </summary>
    private Stopwatch TotalStopwatch { get; }
    /// <summary>
    /// Total time elapsed since the start of the game. Is set in
    /// the main loop.
    /// </summary>
    private long TotalTime { get; set; }
    /// <summary>
    /// Time since the last call of PrintTime or NextStep (the shortest time).
    /// </summary>
    private long LastElapsed { get; set; }
    /// <summary>
    /// Total number of game updates.
    /// </summary>
    private long UpdatesDone { get; set; }
    /// <summary>
    /// Output of messages.
    /// </summary>
    private TextWriter Output { get; }

    /// <summary>
    /// Time in since the last game update in ms.
    /// </summary>
    public float DeltaT { get; private set; }

    public GameTime(TextWriter output)
    {
        TotalStopwatch = new Stopwatch();
        TotalStopwatch.Start();
        Output = output;
        LastElapsed = 0;
    }

    /// <summary>
    /// Starts a new step. Should be called only at the start of a step.
    /// </summary>
    public void NextStep()
    {
        LastElapsed = TotalStopwatch.ElapsedMilliseconds;
        DeltaT = (LastElapsed - (float)TotalTime) / 1000f;
        TotalTime = LastElapsed;
        UpdatesDone++;
    }

    /// <summary>
    /// Prints how long the action took = time from the last call of PrintTime or NextStep (the shortest time).
    /// </summary>
    public void PrintTime(string actionName)
    {
        long elapsed = TotalStopwatch.ElapsedMilliseconds;
        Output.WriteLine((elapsed - LastElapsed)+"\t"+ actionName);
        LastElapsed = elapsed;
    }

    /// <summary>
    /// Prints average fps during the game.
    /// </summary>
    public void PrintFPS()
    {
        //prevent dividing by zero
        if (TotalTime == 0)
            return;

        Output.WriteLine("FPS: "+(UpdatesDone/(float)TotalTime * 1000));
    }

    /// <summary>
    /// Prints line delimiter to Output.
    /// </summary>
    public void Delimiter()
    {
        Output.WriteLine("------------------------");
    }
}
