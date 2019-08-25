using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Data.Entities;
using static wpfTest.MainWindow;

namespace wpfTest.GUI
{
    abstract class ButtonArray<InfoSource> : Grid where InfoSource:IShowable
    {
        public List<InfoSource> InfoSources { get; set; }
        public InfoSource GetInfoSource(int index)
        {
            if (InfoSources!=null && index < InfoSources.Count)
                return InfoSources[index];
            else
                return default(InfoSource);
        }
        public InfoSource Selected { get; set; }
        public int Columns { get; }
        public int Rows { get; }

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
                    b.Content = InfoSources[i].GetName;
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

    class EntityButtonArray : ButtonArray<Entity>
    {
        public EntityButtonArray(int colulmns, int rows, double width, double height)
            : base(colulmns, rows, width, height)
        {
            Style = (Style)Application.Current.FindResource("EntitiesArrayStyle");
        }

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

    class AbilityButtonArray : ButtonArray<Ability>
    {
        public AbilityButtonArray(int columns, int rows, double width, double height)
            : base(columns, rows, width, height)
        {
            Style = (Style)Application.Current.FindResource("AbilitiesArrayStyle");
        }

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

    class StatusButtonArray : ButtonArray<Status>
    {
        public Status SelectedStatus { get; private set; }

        public StatusButtonArray(int columns, int rows, double width, double height)
            : base(columns, rows, width, height)
        {
            Style = (Style)Application.Current.FindResource("StatusArrayStyle");
        }
    }

    class CommandButtonArray : ButtonArray<Command>
    {
        public CommandButtonArray(int colulmns, int rows, double width, double height)
            : base(colulmns, rows, width, height)
        {
            if (Children.Count > 0)
                ((Button)Children[0]).Background = Brushes.Green;
            Style = (Style)Application.Current.FindResource("CommandsArrayStyle");
        }


    }

    public interface IShowable
    {
        string GetName { get; }
        List<Stat> Stats();
        string Description();
    }
}
