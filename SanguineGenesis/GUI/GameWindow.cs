﻿using SanguineGenesis.GameControls;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.AI;
using SanguineGenesis.GameLogic.Data;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps.MovementGenerating;
using SanguineGenesis.GUI.WinFormsControls;
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
    partial class GameWindow : Form
    {
        /// <summary>
        /// Runs main loop of the game.
        /// </summary>
        private Timer GameUpdateTimer { get; }
        /// <summary>
        /// Window that created this window.
        /// </summary>
        private MainMenuWindow MainMenuWindow { get; }
        /// <summary>
        /// Describes customizable parts of the game.
        /// </summary>
        public GameplayOptions GameplayOptions { get; }
        /// <summary>
        /// True if the window was initialized.
        /// </summary>
        public bool Initialized { get; set; }
        /// <summary>
        /// True if the window should be closed.
        /// </summary>
        public bool CloseWindow { get; set; }
        /// <summary>
        /// True if this window is no longer used for the last game.
        /// </summary>
        public bool GameEnded { get; set; }

        public GameWindow(MainMenuWindow mainMenuWindow)
        {
            //initialize MainMenuWindow
            MainMenuWindow = mainMenuWindow;

            try
            {
                InitializeComponent();
            }catch(Exception e)
            {
                MessageBox.Show($"Failed to initialize OpenGL context: {e.Message}");
                MainMenuWindow.CanCreateGame = false;
                Close();
                return;
            }
            Initialized = false;
            CloseWindow = false;

            //initialize game controls
            GameControls = new GameControls.GameControls();

            //initialize GameplayOptions
            GameplayOptions = new GameplayOptions();

            //create and enable game update timer
            GameUpdateTimer = new Timer();
            GameUpdateTimer.Tick += GameUpdateTimer_MainLoop;
            GameUpdateTimer.Interval = 10;

            //initialize ImageAtlas and GameData
            try
            {
                ImageAtlas.Init();
                GameData = new GameData();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to initialize game data: {e.Message}");
                MainMenuWindow.CanCreateGame = false;
                Close();
                return;
            }

            //wait until the window initializes and then initialize gui and opengl
            Shown += (s, e) =>
            {
                InitializeOpenGL();
                InitializeUserInterface();
                Initialized = true;
                GameUpdateTimer.Enabled = true;
            };
        }

        #region Game logic

        /// <summary>
        /// The game this window is showing.
        /// </summary>
        private Game Game { get; set; }
        /// <summary>
        /// Used for manipulating the game by player.
        /// </summary>
        private GameControls.GameControls GameControls { get; set; }
        /// <summary>
        /// Measures the ingame time.
        /// </summary>
        private GameTime GameTime { get; set; }
        /// <summary>
        /// Data about the game.
        /// </summary>
        public GameData GameData { get; set; }

        public void StartNewGame(MapDescription mapDescription, Biome playersBiome, bool testAnimals, IAIFactory aiFactory)
        {
            if (Initialized)
            {
                //reset window state
                Game.Winner = null;
                VictoryPanel.AlreadyShown = false;
                ControlGroupButtonArray.Reset();
                CloseMenu();
                GameUpdateTimer.Enabled = false;
            }
            //initialize game
            Game = new Game(mapDescription, playersBiome, GameData, GameplayOptions, aiFactory);
            //reset game controls
            GameControls.Reset();
            GameControls.MapView.SetActualExtents(openGLControl.Width, openGLControl.Height);
            //spawn testing animals
            if (testAnimals)
                Game.SpawnTestingAnimals();
            //initialize game time
            GameTime = new GameTime(Console.Out);
            GameUpdateTimer.Enabled = true;
            GameEnded = false;
            initializedPos = false;
        }

        public void GameUpdateTimer_MainLoop(object sender, EventArgs e)
        {
            if (GameEnded)
                return;

            //update time
            GameTime.NextStep();

            //update MapMovementInput
            UpdateMapMovementInput();
            //move map view
            GameControls.MoveMapView(Game.Map);

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
        /// <summary>
        /// Shows error messages.
        /// </summary>
        private Label ErrorList { get; set; }

        #region Initialization
        /// <summary>
        /// Initializes user interface.
        /// </summary>
        private void InitializeUserInterface()
        {
            //initialize bottom panel
            InitializeBottomPanel();

            //add event handlers to this window and opengl control
            MouseWheel += GameWindow_MouseWheel;

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
            GameMenu.SetExitButtonClickHandler(Close);
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

            //error list
            int errListWidth = 500;
            ErrorList = new Label()
            {
                Width = errListWidth,
                Left = (ClientSize.Width - errListWidth) / 2,
                Top = 0,
                ForeColor = Color.Red,
                BackColor = Color.White,
                TextAlign = ContentAlignment.BottomCenter
            };
            ErrorList.Height = GameControls.ActionLog.Size * ErrorList.Font.Height + 2;
            ErrorList.Text = "";
            openGLControl.Controls.Add(ErrorList);

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
            AbilityButtonArray = new AbilityButtonArray(4, 3, 200, 200, GameControls);
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
            ControlGroupButtonArray = new ControlGroupButtonArray(6, EntityButtonArray.Width, 20, GameControls);
            Controls.Add(ControlGroupButtonArray);

            //add handlers to the buttons
            EntityButtonArray.ShowInfoOnClick(GameControls, GameControls.SelectionInput);
            AbilityButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            EntityInfoPanel.CommandButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            EntityInfoPanel.CommandButtonArray.RemoveCommandOnClick();
            EntityInfoPanel.StatusButtonArray.ShowInfoOnMouseOver(AdditionalInfo);
            AbilityButtonArray.SelectAbilityOnClick();
            ControlGroupButtonArray.LoadEntitiesOnClick();

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
                OpenGLAtlasDrawer.Initialize(gl, openGLControl.Width, openGLControl.Height);
            }catch(Exception e)
            {
                MessageBox.Show("Failed to initialize OpenGL context: "+e.Message);
                MainMenuWindow.CanCreateGame = false;
                Close();
            }
        }
        #endregion Initialization

        #region Event handlers

        /// <summary>
        /// False if the first position of MapView wasn't initialised yet.
        /// </summary>
        private bool initializedPos;

        /// <summary>
        /// Draws the game.
        /// </summary>
        private void Draw(object sender, RenderEventArgs args)
        {
            if (!Enabled || !Initialized)
                return;

            OpenGL gl = openGLControl.OpenGL;

            //set correct extents of the window to game controls
            GameControls.MapView.SetActualExtents(openGLControl.Width, openGLControl.Height);
            if (!initializedPos)
            {
                initializedPos = true;
                //center view to main building
                var mainBuilding = Game.CurrentPlayer.GetAll<Building>().First();
                if (mainBuilding != null)
                {
                    GameControls.MapView.CenterTo(Game.Map, mainBuilding.Center);
                }
            }

            //update data buffers

            //maps data
            {
                //map
                OpenGLAtlasDrawer.UpdateMapDataBuffers(gl, GameControls.MapView, Game);

                //nutrients map
                if (Game.GameplayOptions.NutrientsVisible)
                    OpenGLAtlasDrawer.UpdateNutrientsMapDataBuffers(gl, GameControls.MapView, Game);
                else
                    OpenGLAtlasDrawer.ClearNutrientsMapDataBuffers(gl);

                //flowfield
                if (Game.GameplayOptions.ShowFlowfield)
                {
                    FlowField flF;
                    if ((flF = SelectedAnimalFlowfield()) != null)
                        //update flowfield with data of the selected animal if an animal is selected
                        OpenGLAtlasDrawer.UpdateFlowFieldDataBuffers(gl, GameControls.MapView, flF);
                    else
                        //clear flowfield
                        OpenGLAtlasDrawer.ClearFlowFieldDataBuffers(gl);
                }
                else
                    //clear flowfield
                    OpenGLAtlasDrawer.ClearFlowFieldDataBuffers(gl);
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

            //selector rectangle
            OpenGLAtlasDrawer.UpdateSelectorRectDataBuffers(gl, GameControls.MapView, GameControls.MapSelectorRect);


            //draw the data
            OpenGLAtlasDrawer.Draw(gl, Game.GameplayOptions);

            //update bottom panel
            UpdateBottomPanel();

            GameTime.PrintTime("Graphics tick length");
            GameTime.PrintFPS();
            GameTime.Delimiter();
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
        private void OpenGLControl_KeyDown(object sender, KeyEventArgs e)
        {
            int abilityIndex;
            int controlGroupIndex;
            if ((abilityIndex = AbilityButtonArray.KeyToAbilityIndex(e.KeyCode)) != -1)
            {
                //select the ability
                AbilityButtonArray.SelectAbilityWithIndex(abilityIndex);
            }
            else if (e.KeyCode == Keys.ShiftKey)
                //abilities will be appended to the queue
                GameControls.SelectionInput.ResetCommandsQueue = false;
            else if (e.KeyCode == Keys.Tab)
                //jump to next entity type
                EntityButtonArray.SelectEntityOfNextType(GameControls.SelectionInput);
            else if (e.KeyCode == Keys.Space)
            {
                //select only entities of current type
                if(EntityButtonArray.Selected!=null &&
                    GameControls.SelectionInput.State==SelectionInputState.ENTITIES_SELECTED)
                    GameControls.SelectedGroup.KeepSelected(EntityButtonArray.Selected.EntityType);
            }
            else if (e.KeyCode == Keys.T)
            {
                //switch showing of nutrients
                GameplayOptions.NutrientsVisible = !GameplayOptions.NutrientsVisible;
            }
            else if (e.KeyCode == Keys.G)
            {
                //center map to the selected entity
                if (EntityButtonArray.Selected != null)
                    GameControls.MapView.CenterTo(Game.Map, EntityButtonArray.Selected.Center);
            }
            else if ((controlGroupIndex = ControlGroupButtonArray.KeyToGroupIndex(e.KeyCode)) != -1)
            {
                //modify control groups
                if (e.Control)
                {
                    //saves selected entities to the group
                    ControlGroupButtonArray.SaveGroupWithIndex(controlGroupIndex);
                }
                else if (GameControls.SelectionInput.State == SelectionInputState.IDLE ||
                    GameControls.SelectionInput.State == SelectionInputState.ENTITIES_SELECTED)
                {
                    //loads selected entities from the group
                    if (e.Shift)
                        GameControls.SelectedGroup.NextOperation = Operation.ADD;

                    ControlGroupButtonArray.LoadGroupWithIndex(controlGroupIndex);

                }
            }
        }

        /// <summary>
        /// Updates information about shift being pressed.
        /// </summary>
        private void OpenGLControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                GameControls.SelectionInput.ResetCommandsQueue = true;
        }

        /// <summary>
        /// Changes the zoom of the map.
        /// </summary>
        private void GameWindow_MouseWheel(object sender, MouseEventArgs e)
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
        private void OpenGLControl_MouseButtonDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //update vertex of selector rectangle
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
        private void OpenGLControl_MouseButtonUp(object sender, MouseEventArgs e)
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
        private void OpenGLControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (GameControls.SelectionInput.State == SelectionInputState.SELECTING_ENTITIES)
            {
                //update selector rectangle
                Vector2 mapCoordinates = GameControls.MapView
                    .ScreenToMap(new Vector2(e.X, e.Y));
                GameControls.SelectionInput.NewPoint(mapCoordinates);
                
                //choose selected entity
                SelectEntity();
            }
        }

        /// <summary>
        /// Disables this window and enables main menu window.
        /// </summary>
        private void GameWindow_Closing(object sender, FormClosingEventArgs e)
        {
            GameEnded = true;
            GameUpdateTimer.Enabled = false;
            if (!CloseWindow)
            {
                //don't close this window
                e.Cancel = true;
                //hide this window
                Enabled = false;
                Visible = false;
                //show main menu window
                MainMenuWindow.Enabled = true;
                MainMenuWindow.Visible = true;
                MainMenuWindow.BringToFront();
            }
        }

        private void GameWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                OpenGLAtlasDrawer.Destruct(openGLControl.OpenGL);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to destroy OpenGL context: " + ex.Message);
            }
        }

        #endregion event handlers

        #region Utility methods

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
                List<Entity> selectedEntities = GameControls.SelectedGroup.Entities();
                selectedEntities.Sort((e1, e2) => string.Compare(e1.EntityType, e2.EntityType));

                //update selected entity if the old one was removed or player is currently selecting entities
                if (GameControls.SelectionInput.State == SelectionInputState.SELECTING_ENTITIES ||
                    ShouldSetSelected ||
                    !selectedEntities.Contains(EntityButtonArray.Selected))
                    EntityButtonArray.Selected = selectedEntities.FirstOrDefault();
                ShouldSetSelected = GameControls.SelectionInput.State == SelectionInputState.SELECTING_ENTITIES;

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
            PlayerPropertiesPanel.AirValue.Text = $"{Game.CurrentPlayer.AirTaken}/{Game.CurrentPlayer.MaxAirTaken}";

            //update error list
            ErrorList.Text = GameControls.ActionLog.GetMessages();

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
            GameOptionsMenu.Visible = false;
            GameMenu.Visible = false;
        }
        
        /// <summary>
        /// Opens options menu.
        /// </summary>
        private void OpenOptionsMenu()
        {
            GameOptionsMenu.Visible = true;
            GameOptionsMenu.UpdateCheckboxes(GameplayOptions);
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
            VictoryPanel.Message.Text = Winner==FactionType.PLAYER0? "You won!" : "Enemy won!";
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
        public void UpdateMapMovementInput()
        {
            //check if player is selecting entities
            if (!(GameControls.SelectionInput.State == SelectionInputState.SELECTING_ENTITIES))
            {
                Point mousePos = Cursor.Position;

                int movingFrameSize = 2;
                if (mousePos.X >= openGLControl.Width - movingFrameSize)
                {
                    //move right
                    GameControls.MapMovementInput.AddDirection(Direction.RIGHT);
                }
                else if (mousePos.X <= movingFrameSize)
                {
                    //move left
                    GameControls.MapMovementInput.AddDirection(Direction.LEFT);
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
                }
                else if (mousePos.Y <= movingFrameSize)
                {
                    //move up
                    GameControls.MapMovementInput.AddDirection(Direction.UP);
                }
                else
                {
                    //stop moving vertically
                    GameControls.MapMovementInput.RemoveDirection(Direction.UP);
                    GameControls.MapMovementInput.RemoveDirection(Direction.DOWN);
                }
            }
        }

        #endregion Utility methods

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
