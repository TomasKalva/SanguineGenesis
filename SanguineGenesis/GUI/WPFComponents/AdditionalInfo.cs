using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static SanguineGenesis.MainWindow;

namespace SanguineGenesis.GUI
{
    /// <summary>
    /// Shows info about IShowable.
    /// </summary>
    class AdditionalInfo : Canvas
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
        /// TextBlock with the description of Shown.
        /// </summary>
        public TextBlock Description { get; }

        public AdditionalInfo(int width, int height)
        {
            Width = width;
            Height = height;

            double captionHeight = Height / 7;
            double statsHeight = Height / 3;
            double descriptionHeight = Height - (captionHeight + statsHeight);

            //caption
            Caption = new Label()
            {
                Width = Width,
                Height = captionHeight
            };
            Children.Add(Caption);
            SetLeft(Caption, 0);
            SetTop(Caption, 0);

            //stats
            Stats = new StatsTable(6, 1, Width, statsHeight);
            Children.Add(Stats);
            SetLeft(Stats, 0);
            SetTop(Stats, captionHeight);

            //description
            Description = new TextBlock()
            {
                Width = Width,
                Height = descriptionHeight
            };
            Children.Add(Description);
            SetLeft(Description, 0);
            SetTop(Description, captionHeight + statsHeight);

            Style = (Style)Application.Current.FindResource("AdditionalInfoStyle");
        }

        /// <summary>
        /// Sets Shown to showable and updates the components.
        /// </summary>
        public void Update(IShowable showable)
        {
            Shown = showable;
            Caption.Content = showable.GetName();
            Stats.SetStats(showable.Stats());
            Description.Text = showable.Description();
        }

        /// <summary>
        /// Sets Shown to null and reset the components.
        /// </summary>
        public void Reset()
        {
            Shown = null;
            Caption.Content = "";
            Stats.SetStats(new List<Stat>());
            Description.Text = "";
        }
    }
}
