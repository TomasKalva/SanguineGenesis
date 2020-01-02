using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SanguineGenesis.GUI.WPFComponents
{
    /// <summary>
    /// Contains buttons for closing the game and opening options menu.
    /// </summary>
    class GameMenu : TableLayoutPanel
    {
        /// <summary>
        /// Clicking it resumes the game.
        /// </summary>
        public Button Resume { get; }
        /// <summary>
        /// Clicking it shows options menu.
        /// </summary>
        public Button Options { get; }
        /// <summary>
        /// Clicking it closes the game.
        /// </summary>
        public Button Exit { get; }

        public GameMenu(int width, int height)
        {
            Width = width;
            Height = height;
            RowCount = 3;
            ColumnCount = 1;
            BackColor = Color.Gray;

            //create rows
            for (int i =0; i < RowCount; i++)
            {
                RowStyle evenRow = new RowStyle(SizeType.Percent);
                evenRow.Height = 1 / (float)RowCount;
                RowStyles.Add(evenRow);
            }

            //create buttons
            Font buttonsFont = new Font(Button.DefaultFont.FontFamily, 15, FontStyle.Bold);
            Color buttonsColor = Color.Beige;

            //resume button
            Resume = new Button();
            Resume.Dock = DockStyle.Fill;
            Resume.Text = "Resume";
            Resume.BackColor = buttonsColor;
            Resume.Font = buttonsFont;
            Controls.Add(Resume);

            //options button
            Options = new Button();
            Options.Dock = DockStyle.Fill;
            Options.Text = "Options";
            Options.BackColor = buttonsColor;
            Options.Font = buttonsFont;
            Controls.Add(Options);

            //exit button
            Exit = new Button();
            Exit.Dock = DockStyle.Fill;
            Exit.Text = "Exit";
            Exit.BackColor = buttonsColor;
            Exit.Font = buttonsFont;
            Controls.Add(Exit);
        }

        /// <summary>
        /// Sets event handler buttonClick to resume button click event.
        /// </summary>
        public void SetResumeButtonClickHandler(Action buttonClick)
        {
            Resume.Click += (_s, _e) => buttonClick();
        }

        /// <summary>
        /// Sets event handler buttonClick to options button click event.
        /// </summary>
        public void SetOptionsButtonClickHandler(Action buttonClick)
        {
            Options.Click += (_s, _e) => buttonClick();
        }

        /// <summary>
        /// Sets event handler buttonClick to exit button click event.
        /// </summary>
        public void SetExitButtonClickHandler(Action buttonClick)
        {
            Exit.Click += (_s, _e) => buttonClick();
        }
    }
}
