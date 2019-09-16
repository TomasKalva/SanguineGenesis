using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SanguineGenesis.GameLogic;

namespace SanguineGenesis.GUI
{
    /// <summary>
    /// Panel for enabling and disabling game options.
    /// </summary>
    class GameOptionsPanel:Grid
    {
        /// <summary>
        /// Checkboxes for each gameplay option.
        /// </summary>
        private CheckBox[] optionCheckboxes;

        public GameOptionsPanel(double width, double height, GameplayOptions gameplayOptions)
        {
            Width = width;
            Height = height;

            //define columns and rows
            int rows = 4;
            int columns = 2;
            for (int i = 0; i < columns; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < rows; i++)
            {
                RowDefinitions.Add(new RowDefinition());
            }
            
            //initialize checkboxes
            optionCheckboxes = new CheckBox[rows];
            for(int i=0; i < rows - 1; i++)
            {
                CheckBox cb = new CheckBox();
                int row = i;
                cb.SetValue(ColumnProperty, 0);
                cb.SetValue(RowProperty, row);
                cb.SetValue(ColumnSpanProperty, columns);
                cb.Focusable = false;
                Children.Add(cb);
                optionCheckboxes[i] = cb;
            }

            //create button for exiting this panel
            Button okButton = new Button();
            okButton.Content = "OK";
            okButton.SetValue(ColumnProperty, columns - 1);
            okButton.SetValue(RowProperty, rows - 1);
            okButton.Focusable = false;
            okButton.Click += (sender, ev) => Visibility = Visibility.Hidden;
            Children.Add(okButton);

            optionCheckboxes[0].Content = "Whole map visible";
            RoutedEventHandler evHand = (s, e) =>
            {
                lock (gameplayOptions)
                {
                    if (optionCheckboxes[0].IsChecked != null)
                        gameplayOptions.WholeMapVisible = (bool)optionCheckboxes[0].IsChecked;
                }
            };
            optionCheckboxes[0].Unchecked += evHand;
            optionCheckboxes[0].Checked += evHand;

            optionCheckboxes[1].Content = "Nutrients visible";
            evHand = (s, e) =>
            {
                lock (gameplayOptions)
                {
                    if (optionCheckboxes[1].IsChecked != null)
                        gameplayOptions.NutrientsVisible = (bool)optionCheckboxes[1].IsChecked;
                }
            };
            optionCheckboxes[1].Unchecked += evHand;
            optionCheckboxes[1].Checked += evHand;

            optionCheckboxes[2].Content = "Show flowfield";
            evHand = (s, e) =>
            {
                lock (gameplayOptions)
                {
                    if (optionCheckboxes[2].IsChecked != null)
                        gameplayOptions.ShowFlowfield = (bool)optionCheckboxes[2].IsChecked;
                }
            };
            optionCheckboxes[2].Unchecked += evHand;
            optionCheckboxes[2].Checked += evHand;

            Visibility = Visibility.Hidden;
            Style = (Style)Application.Current.FindResource("GameOptionsStyle");
        }
    }
}
