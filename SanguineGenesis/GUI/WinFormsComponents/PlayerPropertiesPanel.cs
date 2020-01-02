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
    /// Shows information about the player.
    /// </summary>
    class PlayerPropertiesPanel:TableLayoutPanel
    {
        /// <summary>
        /// Shows how much air does the player have.
        /// </summary>
        public Label AirValue { get; }

        public PlayerPropertiesPanel(int width, int height)
        {
            Width = width;
            Height = height;
            RowCount = 1;
            ColumnCount = 2;

            //text
            Label AirText = new Label
            {
                Text = "Air taken: ",
                Margin = Padding.Empty,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Yellow,
                Width = width / 2
            };
            Controls.Add(AirText);

            //value
            AirValue = new Label
            {
                Text = "",
                Margin = Padding.Empty,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.Yellow,
                Width = width / 2
            };
            Controls.Add(AirValue);
            
            BackColor = Color.Black;
        }
    }
}
