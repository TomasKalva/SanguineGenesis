using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Entities;
using static SanguineGenesis.MainWindow;

namespace SanguineGenesis.GUI
{
    /// <summary>
    /// Control for drawing rectangle array of buttons.
    /// </summary>
    abstract class ButtonArray<InfoSource> : Grid where InfoSource:IShowable
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
            if (InfoSources!=null && index < InfoSources.Count)
                return InfoSources[index];
            else
                return default(InfoSource);
        }
        /// <summary>
        /// Selected InfoSource.
        /// </summary>
        public InfoSource Selected { get; set; }
        /// <summary>
        /// Number of columns.
        /// </summary>
        public int Columns { get; }
        /// <summary>
        /// Number of rows.
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// Creates new ButtonArray with the given extents and number of rows and columns.
        /// </summary>
        public ButtonArray(int columns, int rows, double width, double height)
        {
            Columns = columns;
            Rows = rows;
            Width = width;
            Height = height;
            
            for (int i = 0; i < columns; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < rows; i++)
            {
                RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < columns * rows; i++)
            {
                Button b = new Button();
                b.SetValue(Grid.ColumnProperty, i % Columns);
                b.SetValue(Grid.RowProperty, i / Columns);
                b.Focusable = false;

                //add text block for the button
                TextBlock t = new TextBlock();
                t.TextWrapping = TextWrapping.Wrap;
                t.TextAlignment = TextAlignment.Center;
                b.Content = t;
                Children.Add(b);
            }
        }

        /// <summary>
        /// Sets listeners to buttons to show corresponding InfoSource in
        /// additionalInfo if mouse is over it.
        /// </summary>
        public void ShowInfoOnMouseOver(AdditionalInfo additionalInfo)
        {
            for (int i = 0; i < Columns * Rows; i++)
            {
                Button b = (Button)Children[i];
                int buttonInd = i;//capture by value
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
                b.IsVisibleChanged += (sender, ev) =>
                {
                    InfoSource info = GetInfoSource(buttonInd);
                    if (b.IsVisible)
                    {
                        //show info if button appeared under mouse
                        if(mouseOverButton && info!=null)
                            additionalInfo.Update(info);
                    }
                    else
                    {
                        //reset info shown in additional info if its button disappeared
                        if(info!=null && info.Equals(additionalInfo.Shown))
                            additionalInfo.Reset();
                    }
                };
            }
        }

        /// <summary>
        /// Update the info on the buttons, hide buttons without info, show buttons
        /// with info.
        /// </summary>
        public void Update()
        {
            if (InfoSources == null)
                return;
            //update units panel
            for (int i = 0; i < Children.Count; i++)
            {
                Button b = (Button)Children[i];
                if (i >= InfoSources.Count)
                {
                    b.Visibility = Visibility.Hidden;
                }
                else
                {
                    ((TextBlock)b.Content).Text = InfoSources[i].GetName();
                    b.Visibility = Visibility.Visible;
                    //highlight selected
                    if (InfoSources[i].Equals(Selected))
                        b.Background = Brushes.DarkGreen;
                    else
                        b.ClearValue(BackgroundProperty);
                }
            }
        }
    }

    /// <summary>
    /// Used for drawing buttons with entities.
    /// </summary>
    class EntityButtonArray : ButtonArray<Entity>
    {
        public EntityButtonArray(int colulmns, int rows, double width, double height)
            : base(colulmns, rows, width, height)
        {
            Style = (Style)Application.Current.FindResource("EntitiesArrayStyle");
        }

        /// <summary>
        /// Sets listeners to buttons click to show corresponding Entity's in
        /// entityInfoPanel and its abilities in abilityButtonArray, and to remove
        /// the entity from selected entities on right click.
        /// </summary>
        public void ShowInfoOnClick(EntityInfoPanel entityInfoPanel, AbilityButtonArray abilityButtonArray, GameControls gameControls)
        {
            for (int i = 0; i < Columns * Rows; i++)
            {
                Button b = (Button)Children[i];
                int buttonInd = i;//capture by value
                b.PreviewMouseDown += (sender, ev) =>
                {
                    Entity info;
                    if ((info = GetInfoSource(buttonInd)) != null)
                    {
                        if (ev.RightButton == MouseButtonState.Pressed)
                        {
                            //remove the entity corresponding to the clicked button from selection
                            gameControls.SelectedEntities.RemoveEntity(info);
                        }
                        else
                        {
                            //select this entity
                            Selected = info;
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
        public AbilityButtonArray(int columns, int rows, double width, double height)
            : base(columns, rows, width, height)
        {
            Style = (Style)Application.Current.FindResource("AbilitiesArrayStyle");
        }
        
        /// <summary>
        /// Sets listeners to buttons click to select ability.
        /// </summary>
        public void SelectAbilityOnClick(GameControls gameControls)
        {
            for (int i = 0; i < Columns * Rows; i++)
            {
                Button b = (Button)Children[i];
                int buttonInd = i;//capture by value
                b.Click += (sender, ev) =>
                {
                    Ability selectedAbility;
                    if ((selectedAbility = GetInfoSource(buttonInd)) != null)
                        lock (gameControls.EntityCommandsInput)
                        {
                            gameControls.EntityCommandsInput.SelectedAbility = selectedAbility;
                        }
                };
            }
        }
    }

    /// <summary>
    /// Used for drawing buttons with statuses.
    /// </summary>
    class StatusButtonArray : ButtonArray<Status>
    {
        public StatusButtonArray(int columns, int rows, double width, double height)
            : base(columns, rows, width, height)
        {
            Style = (Style)Application.Current.FindResource("StatusArrayStyle");
        }
    }

    /// <summary>
    /// Used for drawing buttons with commands.
    /// </summary>
    class CommandButtonArray : ButtonArray<Command>
    {
        public CommandButtonArray(int colulmns, int rows, double width, double height)
            : base(colulmns, rows, width, height)
        {
            if (Children.Count > 0)
                ((Button)Children[0]).Background = Brushes.Green;
            Style = (Style)Application.Current.FindResource("CommandsArrayStyle");
        }

        /// <summary>
        /// Sets listeners to buttons left clicks to remove corresponding command from its entity.
        /// </summary>
        public void RemoveCommandOnClick()
        {
            for (int i = 0; i < Columns * Rows; i++)
            {
                Button b = (Button)Children[i];
                int buttonInd = i;//capture by value
                b.PreviewMouseDown += (sender, ev) =>
                {
                    Command command;
                    if ((command = GetInfoSource(buttonInd)) != null)
                    {
                        if (ev.RightButton == MouseButtonState.Pressed)
                        {
                            //remove the command corresponding to the clicked button from selection
                            command.Remove();
                        }
                    }
                };
            }
        }
    }

    public interface IShowable
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
