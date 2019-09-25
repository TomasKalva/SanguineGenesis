using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using SanguineGenesis.GameLogic;

namespace SanguineGenesis.GUI
{
    /// <summary>
    /// Panel for enabling and disabling game options.
    /// </summary>
    class GameOptionsMenu:TableLayoutPanel
    {
        
        /// <summary>
        /// Checkboxes for each gameplay option.
        /// </summary>
        private CheckBox[] optionCheckboxes;

        public GameOptionsMenu(int width, int height, GameplayOptions gameplayOptions)
        {
            Width = width;
            Height = height;
            
            RowCount = 4;
            ColumnCount = 1;

            //create rows
            for (int i = 0; i < RowCount; i++)
            {
                RowStyle evenRow = new RowStyle(SizeType.Percent);
                evenRow.Height = 1 / (float)RowCount;
                RowStyles.Add(evenRow);
            }

            //initialize checkboxes
            optionCheckboxes = new CheckBox[RowCount];
            for(int i=0; i < RowCount - 1; i++)
            {
                CheckBox cb = new CheckBox();
                Controls.Add(cb);
                cb.TextAlign = ContentAlignment.MiddleCenter;
                cb.ForeColor = Color.White;
                cb.Dock = DockStyle.Fill;
                optionCheckboxes[i] = cb;
            }

            //create button for exiting this panel
            Button okButton = new Button()
            {
                Width = Width / 3,
                Height = Height / ColumnCount,
                BackColor = Color.Beige,
                Dock = DockStyle.Right
            };
            okButton.Text = "OK";
            okButton.Click += (sender, ev) => Visible = false ;
            Controls.Add(okButton);

            optionCheckboxes[0].Text = "Whole map visible";
            EventHandler evHand = (s, e) =>
            {
                lock (gameplayOptions)
                {
                    gameplayOptions.WholeMapVisible = (bool)optionCheckboxes[0].Checked;
                }
            };
            optionCheckboxes[0].CheckedChanged += evHand;

            optionCheckboxes[1].Text = "Nutrients visible";
            evHand = (s, e) =>
            {
                lock (gameplayOptions)
                {
                    gameplayOptions.NutrientsVisible = (bool)optionCheckboxes[1].Checked;
                }
            };
            optionCheckboxes[1].CheckedChanged += evHand;

            optionCheckboxes[2].Text = "Show flowfield";
            evHand = (s, e) =>
            {
                lock (gameplayOptions)
                {
                    gameplayOptions.ShowFlowfield = (bool)optionCheckboxes[2].Checked;
                }
            };
            optionCheckboxes[2].CheckedChanged += evHand;

            BackColor = Color.Gray;
        }
    }
}
