using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SanguineGenesis.GUI.WinFormsComponents
{
    /// <summary>
    /// Shows name of the player who won.
    /// </summary>
    class VictoryPanel:Panel
    {
        /// <summary>
        /// True iff the panel has been shown already.
        /// </summary>
        public bool AlreadyShown { get; set; }
        /// <summary>
        /// Closes this panel.
        /// </summary>
        public Button Ok { get; }
        /// <summary>
        /// Message that is shown to the winner.
        /// </summary>
        public Label Message { get; }

        public VictoryPanel(int width, int height)
        {
            AlreadyShown = false;
            Width = width;
            Height = height;

            Message = new Label()
            {
                Width = Width,
                Height = Height / 2,
                Text = "Player won!",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font(Label.DefaultFont.FontFamily, 17, FontStyle.Bold)
            };
            Controls.Add(Message);

            Ok = new Button()
            {
                Width = Width / 3,
                Height = Height / 4,
                Text = "Ok",
                Left = Width / 2 - Width / 6,
                Top = Message.Height + (Height - Message.Height - Height/4)/2,
                BackColor = Color.Beige
            };
            Controls.Add(Ok);

            BackColor = Color.Gray;
        }
    }
}
