using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace SanguineGenesis.GUI.WinFormsComponents
{
    /// <summary>
    /// Shows info about IShowable.
    /// </summary>
    class AdditionalInfo : Panel
    {
        /// <summary>
        /// The instance whose info is shown.
        /// </summary>
        public IShowable Shown { get; private set; }

        /// <summary>
        /// Label with the name of Shown.
        /// </summary>
        public Label Caption { get; }
        /// <summary>
        /// Stats table with the stats of Shown.
        /// </summary>
        public StatsTable Stats { get; }
        /// <summary>
        /// Label with the description of Shown.
        /// </summary>
        public Label Description { get; }

        public AdditionalInfo(int width, int height)
        {
            Width = width;
            Height = height;

            int captionHeight = Height / 7;
            int statsHeight = Height * 2 / 5;
            int descriptionHeight = Height - (captionHeight + statsHeight);

            //caption
            Caption = new Label()
            {
                Width = Width,
                Height = captionHeight,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font(Label.DefaultFont, System.Drawing.FontStyle.Bold)
            };
            Controls.Add(Caption);
            Caption.Location = new System.Drawing.Point(0, 0);

            //stats
            Stats = new StatsTable(6, 1, Width, statsHeight);
            Controls.Add(Stats);
            Stats.Location = new System.Drawing.Point(0, captionHeight);

            //description
            Description = new Label()
            {
                Width = Width,
                Height = descriptionHeight
            };
            Description.Enabled = false;
            Controls.Add(Description);
            Description.Location = new System.Drawing.Point(0, captionHeight + statsHeight);
            
            BackColor = Color.LightGray;
        }

        /// <summary>
        /// Sets Shown to showable and updates the components.
        /// </summary>
        public void Update(IShowable showable)
        {
            Shown = showable;
            Caption.Text = showable.GetName().Replace("_"," ");
            Stats.SetStats(showable.Stats());
            Description.Text = showable.Description();
        }

        /// <summary>
        /// Sets Shown to null and reset the components.
        /// </summary>
        public void Reset()
        {
            Shown = null;
            Caption.Text = "";
            Stats.SetStats(new List<Stat>());
            Description.Text = "";
        }
    }
}
