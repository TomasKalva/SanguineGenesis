using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using SanguineGenesis;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GUI;
using SanguineGenesis.GUI.WinFormsComponents;

namespace SanguineGenesis.GUI
{
    /// <summary>
    /// Parent of ButtonArray<T>, contains icons for buttons.
    /// </summary>
    abstract class ButtonArray : TableLayoutPanel
    {
        /// <summary>
        /// Icons used for the buttons.
        /// </summary>
        public static Icons Icons { get; set; }
    }

    /// <summary>
    /// Control for drawing rectangle array of buttons.
    /// </summary>
    abstract class ButtonArray<InfoSource> : ButtonArray  where InfoSource : IShowable
    {
        /// <summary>
        /// InfoSources corresponding to the button indexes.
        /// </summary>
        public List<InfoSource> InfoSources { get; set; }
        /// <summary>
        /// Returns info source with the given index or default(InfoSource) if the
        /// index is not valid.
        /// </summary>
        public InfoSource GetInfoSource(int index)
        {
            if (InfoSources != null && index < InfoSources.Count)
                return InfoSources[index];
            else
                return default(InfoSource);
        }
        /// <summary>
        /// Selected InfoSource.
        /// </summary>
        public InfoSource Selected { get; set; }


        protected Button[,] Buttons { get; }

        /// <summary>
        /// Creates new ButtonArray with the given extents and number of rows and columns.
        /// </summary>
        public ButtonArray(int columns, int rows, int preferedWidth, int preferedHeight)
        {
            //initialize extents
            RowCount = rows;
            ColumnCount = columns;
            int buttonWidth = preferedWidth / ColumnCount;
            int buttonHeight = preferedHeight / RowCount;
            Width = buttonWidth * ColumnCount;
            Height = buttonHeight * RowCount;

            //initialize Buttons
            Buttons = new Button[ColumnCount, RowCount];
            for (int i = 0; i < ColumnCount; i++)
                for (int j = 0; j < RowCount; j++)
                {
                    Button b = new Button
                    {
                        Width = buttonWidth,
                        Height = buttonHeight,
                        Padding = Padding.Empty,
                        Margin = Padding.Empty,
                        FlatStyle = FlatStyle.Flat
                    };
                    b.FlatAppearance.BorderSize = 1;
                    b.Font = new Font(Button.DefaultFont.FontFamily, 6, System.Drawing.FontStyle.Regular);
                    Buttons[i, j] = b;
                    Controls.Add(b);
                }
        }

        /// <summary>
        /// Sets listeners to buttons to show corresponding InfoSource in
        /// additionalInfo if mouse is over it.
        /// </summary>
        public void ShowInfoOnMouseOver(AdditionalInfo additionalInfo)
        {
            for (int i = 0; i < ColumnCount; i++)
                for (int j = 0; j < RowCount; j++)
                {
                    Button b = Buttons[i, j];
                    int buttonInd = ButtonIndex(j, i);//capture by value
                    bool mouseOverButton = false;
                    b.MouseEnter += (sender, ev) =>
                    {
                        mouseOverButton = true;
                        InfoSource info;
                        if ((info = GetInfoSource(buttonInd)) != null)
                            additionalInfo.Update(info);
                    };

                    b.MouseLeave += (sender, ev) =>
                    {
                        mouseOverButton = false;
                        additionalInfo.Reset();
                    };
                    b.VisibleChanged += (sender, ev) =>
                    {
                        InfoSource info = GetInfoSource(buttonInd);
                        if (b.Visible)
                        {
                            //show info if button appeared under mouse
                            if (mouseOverButton && info != null)
                                additionalInfo.Update(info);
                        }
                        else
                        {
                            //reset info shown in additional info if its button disappeared
                            if (info != null && info.Equals(additionalInfo.Shown))
                                additionalInfo.Reset();
                        }
                    };
                }
        }

        /// <summary>
        /// Update the info on the buttons.
        /// </summary>
        public virtual void UpdateControl()
        {
            if (InfoSources == null)
                return;

            for (int i = 0; i < ColumnCount; i++)
                for (int j = 0; j < RowCount; j++)
                {
                    Button b = Buttons[i, j];
                    int index = ButtonIndex(j, i);
                    if (index >= InfoSources.Count)
                    {
                        //set default appearance of the button
                        b.Image = null;
                        if (b.BackColor != Color.White)
                            b.BackColor = Color.White;
                    }
                    else
                    {
                        string name = InfoSources[index].GetName();
                        b.Image = Icons.GetIcon(name);
                        //highlight selected
                        if (InfoSources[index].Equals(Selected))
                        {
                            if (b.BackColor != Color.DarkGreen)
                                b.BackColor = Color.DarkGreen;
                        }
                        else if (b.BackColor != Color.Orange)
                            b.BackColor = Color.Orange;
                    }
                }
        }

        protected int ButtonIndex(int row, int column) => row + column * RowCount;

        /// <summary>
        /// Makes buttons give focus to focusTarget on click.
        /// </summary>
        /// <param name="focusTarget">After button is clicked, this component receives focus.</param>
        public void GiveFocusTo(Control focusTarget)
        {
            for (int i = 0; i < ColumnCount; i++)
                for (int j = 0; j < RowCount; j++)
                    Buttons[i, j].GotFocus += (_s, _e)=>focusTarget.Focus();
        }
    }

    /// <summary>
    /// Used for drawing buttons with entities.
    /// </summary>
    class EntityButtonArray : ButtonArray<Entity>
    {
        public EntityButtonArray(int colulmns, int rows, int preferedWidth, int preferedHeight)
            : base(colulmns, rows, preferedWidth, preferedHeight)
        {
            for (int i = 0; i < ColumnCount; i++)
                for (int j = 0; j < RowCount; j++)
                    Buttons[i, j].Font = new Font(Button.DefaultFont.FontFamily, 6 , System.Drawing.FontStyle.Regular);
        }

        /// <summary>
        /// Sets listeners to buttons click to show corresponding Entity's in
        /// entityInfoPanel and its abilities in abilityButtonArray, and to remove
        /// the entity from selected entities on right click.
        /// </summary>
        public void ShowInfoOnClick(EntityInfoPanel entityInfoPanel, AbilityButtonArray abilityButtonArray, GameControls gameControls,
                                    EntityCommandsInput entityCommandsInput)
        {
            for (int i = 0; i < ColumnCount; i++)
                for (int j = 0; j < RowCount; j++)
                {
                    Button b = Buttons[i, j];
                    int buttonInd = ButtonIndex(j, i);//capture by value
                    b.MouseDown += (sender, ev) =>
                    {
                        Entity info;
                        if ((info = GetInfoSource(buttonInd)) != null)
                        {
                            if (ev.Button == MouseButtons.Right)
                            {
                                //remove the entity corresponding to the clicked button from selection
                                gameControls.SelectedGroup.RemoveEntity(info);
                            }
                            else
                            {
                                //select this entity
                                Selected = info;
                                entityCommandsInput.SelectedAbility = null;
                            }
                        }
                    };
                }
        }
    }

    /// <summary>
    /// Used for drawing buttons with abilities.
    /// </summary>
    class AbilityButtonArray : ButtonArray<Ability>
    {
        /// <summary>
        /// Delegate used for selecting ability.
        /// </summary>
        private delegate void SelectAbility ();
        private SelectAbility[] SelectAbilityArray { get; set; }

        public AbilityButtonArray(int columns, int rows, int preferedWidth, int preferedHeight)
            : base(columns, rows, preferedWidth, preferedHeight)
        {
            BackColor = Color.Gray;
            SelectAbilityArray = new SelectAbility[columns * rows];
        }
        
        /// <summary>
        /// Sets listeners to buttons click to select ability.
        /// </summary>
        public void SelectAbilityOnClick(GameControls gameControls)
        {
            for (int i = 0; i < ColumnCount; i++)
                for (int j = 0; j < RowCount; j++)
                {
                    Button b = Buttons[i, j];
                    int buttonInd = ButtonIndex(j, i);//capture by value
                    SelectAbilityArray[buttonInd] =
                        () =>
                        {
                            Ability selectedAbility;
                            if ((selectedAbility = GetInfoSource(buttonInd)) != null)
                                gameControls.EntityCommandsInput.SelectedAbility = selectedAbility;

                        };
                    b.Click += (sender, ev) => SelectAbilityArray[buttonInd]();
                }
        }

        public void SelectAbilityWithIndex(int buttonIndex)
        {
            if(buttonIndex>=0 && buttonIndex< RowCount * ColumnCount)
                SelectAbilityArray[buttonIndex]();
        }

        /// <summary>
        /// Returns index of the ability corresponding to the key.
        /// Returns -1 if no such ability exists.
        /// </summary>
        public int KeyToAbilityIndex(Keys key)
        {
            switch (key)
            {
                case Keys.Q:
                    return 0;
                case Keys.W:
                    return 1;
                case Keys.E:
                    return 2;
                case Keys.R:
                    return 3;

                case Keys.A:
                    return 4;
                case Keys.S:
                    return 5;
                case Keys.D:
                    return 6;
                case Keys.F:
                    return 7;

                case Keys.Z:
                    return 8;
                case Keys.X:
                    return 9;
                case Keys.C:
                    return 10;
                case Keys.V:
                    return 11;
            }
            return -1;
        }
    }

    /// <summary>
    /// Used for drawing buttons with statuses.
    /// </summary>
    class StatusButtonArray : ButtonArray<Status>
    {
        public StatusButtonArray(int columns, int rows, int preferedWidth, int preferedHeight)
            : base(columns, rows, preferedWidth, preferedHeight)
        {
        }
    }

    /// <summary>
    /// Used for drawing buttons with commands.
    /// </summary>
    class CommandButtonArray : ButtonArray<Command>
    {
        public CommandButtonArray(int colulmns, int rows, int preferedWidth, int preferedHeight)
            : base(colulmns, rows, preferedWidth, preferedHeight)
        {
            if (Buttons.Length > 0)
                Buttons[0,0].BackColor = Color.Green;
        }

        /// <summary>
        /// Sets listeners to buttons left clicks to remove corresponding command from its entity.
        /// </summary>
        public void RemoveCommandOnClick()
        {
            for (int i = 0; i < ColumnCount; i++)
                for (int j = 0; j < RowCount; j++)
                {
                    Button b = Buttons[i, j];
                    int buttonInd = ButtonIndex(j, i);//capture by value
                    b.MouseDown += (sender, ev) =>
                    {
                        Command command;
                        if ((command = GetInfoSource(buttonInd)) != null)
                        {
                            if (ev.Button == MouseButtons.Right)
                            {
                                //remove the command corresponding to the clicked button from selection
                                command.Remove();
                            }
                        }
                    };
                }
        }
    }

    /// <summary>
    /// Represent a group of entities.
    /// </summary>
    class ControlGroup : IShowable
    {
        public List<Entity> Entities { get; }

        public ControlGroup(List<Entity> entities)
        {
            Entities = entities;
        }

        public void RemoveDead()
        {
            if (Entities != null)
                Entities.RemoveAll(e => e.IsDead);
        }

        public string GetName() => Entities.Count.ToString();

        public List<Stat> Stats() => new List<Stat>();

        public string Description() => "Group of entities";
    }

    /// <summary>
    /// Used for drawing buttons with control groups.
    /// </summary>
    class ControlGroupButtonArray : ButtonArray<ControlGroup>
    {
        delegate void ManipulateGroup();
        
        /// <summary>
        /// Loads selected group to currently selected entities.
        /// </summary>
        private ManipulateGroup[] LoadGroup { get; }
        /// <summary>
        /// Saves currnetly selected entities to control group.
        /// </summary>
        private ManipulateGroup[] SaveGroup { get; }
        /// <summary>
        /// Returns text indexing the button at position index in Buttons.
        /// </summary>
        private string GetButtonIndexText(int index) => '(' + (index + 1).ToString() + ')';

        public ControlGroupButtonArray(int columns, int preferedWidth, int preferedHeight)
            : base(columns, 1, preferedWidth, preferedHeight)
        {
            InfoSources = new List<ControlGroup>(columns * 1);
            for (int i = 0; i < columns; i++)
                InfoSources.Add(null);
            SaveGroup = new ManipulateGroup[columns * 1];
            LoadGroup = new ManipulateGroup[columns * 1];
            for(int i = 0; i < columns; i++)
            {
                Buttons[i,0].Text = GetButtonIndexText(i);
            }
        }

        public void SetEventHandlers(GameControls gameControls)
        {
            for (int i = 0; i < ColumnCount * RowCount; i++)
            {
                int column = i % ColumnCount;
                int row = i / ColumnCount;
                int index = i;//capture by value
                Button b = Buttons[column, row];
                SaveGroup[i] = () =>
                {
                    {
                        if(index >= 0 && index < InfoSources.Count)
                        { 
                            var entities = gameControls.SelectedGroup.Entities;
                            if (entities.Any())
                            {
                                //group contains entities
                                InfoSources[index] = new ControlGroup(entities);
                            }
                            else
                            {
                                //gropu doesn't contain entities
                                InfoSources[index] = null;
                            }
                        }
                    }
                };
                LoadGroup[i] = () =>
                {
                    {
                        if (index >= 0 && index < InfoSources.Count)
                        {
                            SelectedGroup selectedGroup = gameControls.SelectedGroup;
                            //commit the selected entities
                            if(selectedGroup.NextOperation == Operation.ALREADY_SELECTED)
                                selectedGroup.NextOperation = Operation.REPLACE;

                            if (InfoSources[index] != null)
                            {
                                selectedGroup.SetTemporaryEntities(InfoSources[index].Entities);
                                gameControls.EntityCommandsInput.State = EntityCommandsInputState.UNITS_SELECTED;
                            }
                            else
                            {
                                selectedGroup.SetTemporaryEntities(new List<Entity>());
                                gameControls.EntityCommandsInput.State = EntityCommandsInputState.IDLE;
                            }
                            selectedGroup.CommitEntities();

                        }
                    }
                };
                b.Click += (_s, _e) =>
                {
                    LoadGroupWithIndex(index);
                };
            }
        }

        /// <summary>
        /// Update the info on the buttons, update control groups.
        /// </summary>
        public override void UpdateControl()
        {
            if (InfoSources == null)
                return;

            for (int i = 0; i < ColumnCount; i++)
            {
                Button b = Buttons[i, 0];
                var contrGr = InfoSources[i];

                //update control groups and text and color of buttons
                if (contrGr == null)
                {
                    //set default appearance of the button
                    b.Text = GetButtonIndexText(i);
                    if(b.BackColor != Color.White)
                        b.BackColor = Color.White;
                }
                else
                {
                    //removed dead entities from the group
                    contrGr.RemoveDead();

                    if (contrGr.Entities.Any())
                    {
                        //group contains entities
                        b.Text = GetButtonIndexText(i) + contrGr.Entities.Count.ToString();
                        if (b.BackColor != Color.Green)
                            b.BackColor = Color.Green;
                    }
                    else
                    {
                        //gropu doesn't contain entities - remove the control group
                        InfoSources[i] = null;
                        b.Text = GetButtonIndexText(i);
                        if (b.BackColor != Color.White)
                            b.BackColor = Color.White;
                    }
                }
            }
        }

        /// <summary>
        /// Returns index of button corresponding to the key.
        /// </summary>
        public int KeyToGroupIndex(Keys key)
        {
            switch (key)
            {
                case Keys.D1: return 0;
                case Keys.D2: return 1;
                case Keys.D3: return 2;
                case Keys.D4: return 3;
                case Keys.D5: return 4;
                case Keys.D6: return 5;
                case Keys.D7: return 6;
                default: return -1;
            }
        }

        /// <summary>
        /// Sets entities to the group with index i.
        /// </summary>
        public void SaveGroupWithIndex(int i)
        {
            if (i >= 0 && i < SaveGroup.Length) SaveGroup[i]();
        }

        /// <summary>
        /// Loads entities from this group to GameControls.SelectedGroup.
        /// </summary>
        public void LoadGroupWithIndex(int i)
        {            
            if(i>=0 && i<LoadGroup.Length) LoadGroup[i]();
        }    
    }


    interface IShowable
    {
        /// <summary>
        /// Name of this.
        /// </summary>
        string GetName();
        /// <summary>
        /// Represents parameters and their values.
        /// </summary>
        List<Stat> Stats();
        /// <summary>
        /// Text (~1-4 sentences) describing purpose of this. 
        /// </summary>
        string Description();
    }
}
