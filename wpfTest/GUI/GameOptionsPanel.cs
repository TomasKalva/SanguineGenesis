using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using wpfTest.GameLogic;

namespace wpfTest.GUI
{
    class GameOptionsPanel:Grid
    {
        private CheckBox[] checkboxes;

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
            checkboxes = new CheckBox[rows];
            for(int i=0; i < rows - 1; i++)
            {
                CheckBox cb = new CheckBox();
                int row = i;
                cb.SetValue(ColumnProperty, 0);
                cb.SetValue(RowProperty, row);
                cb.SetValue(ColumnSpanProperty, columns);
                cb.Focusable = false;
                Children.Add(cb);
                checkboxes[i] = cb;
            }

            //create button for exiting this panel
            Button okButton = new Button();
            okButton.Content = "OK";
            okButton.SetValue(ColumnProperty, columns - 1);
            okButton.SetValue(RowProperty, rows - 1);
            okButton.Focusable = false;
            okButton.Click += (sender, ev) => Visibility = Visibility.Hidden;
            Children.Add(okButton);

            checkboxes[0].Content = "Whole map visible";
            RoutedEventHandler evHand = (s, e) =>
            {
                lock (gameplayOptions)
                {
                    if (checkboxes[0].IsChecked != null)
                        gameplayOptions.WholeMapVisible = (bool)checkboxes[0].IsChecked;
                }
            };
            checkboxes[0].Unchecked += evHand;
            checkboxes[0].Checked += evHand;

            checkboxes[1].Content = "Nutrients visible";
            evHand = (s, e) =>
            {
                lock (gameplayOptions)
                {
                    if (checkboxes[1].IsChecked != null)
                        gameplayOptions.NutrientsVisible = (bool)checkboxes[1].IsChecked;
                }
            };
            checkboxes[1].Unchecked += evHand;
            checkboxes[1].Checked += evHand;

            checkboxes[2].Content = "Show flowmap";
            evHand = (s, e) =>
            {
                lock (gameplayOptions)
                {
                    if (checkboxes[2].IsChecked != null)
                        gameplayOptions.ShowFlowmap = (bool)checkboxes[2].IsChecked;
                }
            };
            checkboxes[2].Unchecked += evHand;
            checkboxes[2].Checked += evHand;

            Visibility = Visibility.Hidden;
            Style = (Style)Application.Current.FindResource("GameOptionsStyle");
        }
    }
}
