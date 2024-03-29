﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using SanguineGenesis.GameLogic;

namespace SanguineGenesis.GUI.WinFormsControls
{
    /// <summary>
    /// Panel for enabling and disabling game options.
    /// </summary>
    class GameOptionsMenu:TableLayoutPanel
    {
        
        /// <summary>
        /// Checkboxes for each gameplay option.
        /// </summary>
        private readonly CheckBox[] optionCheckboxes;

        public GameOptionsMenu(int width, int height, GameplayOptions gameplayOptions)
        {
            Width = width;
            Height = height;
            
            RowCount = 4;
            ColumnCount = 1;

            //create rows
            for (int i = 0; i < RowCount; i++)
            {
                RowStyle evenRow = new RowStyle(SizeType.Percent)
                {
                    Height = 1 / (float)RowCount
                };
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
            optionCheckboxes[0].CheckedChanged += (_s, _e) =>
                gameplayOptions.WholeMapVisible = optionCheckboxes[0].Checked;

            optionCheckboxes[1].Text = "Nutrients visible";
            optionCheckboxes[1].CheckedChanged += (_s, _e) =>
                gameplayOptions.NutrientsVisible = optionCheckboxes[1].Checked;

            optionCheckboxes[2].Text = "Show flowfield";
            optionCheckboxes[2].CheckedChanged +=(_s, _e) =>
                gameplayOptions.ShowFlowfield = optionCheckboxes[2].Checked;

            BackColor = Color.Gray;
        }

        /// <summary>
        /// Updates values of checkboxes.
        /// </summary>
        public void UpdateCheckboxes(GameplayOptions gameplayOptions)
        {
            optionCheckboxes[0].Checked = gameplayOptions.WholeMapVisible;
            optionCheckboxes[1].Checked = gameplayOptions.NutrientsVisible;
            optionCheckboxes[2].Checked = gameplayOptions.ShowFlowfield;
        }
    }
}
