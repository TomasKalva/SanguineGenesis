using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static wpfTest.MainWindow;

namespace wpfTest.GUI
{
    class AdditionalInfo : Canvas
    {
        public IShowable Shown { get; private set; }

        public Label Caption { get; }
        public StatsTable Stats { get; }
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

        public void Update(IShowable showable)
        {
            Shown = showable;
            Caption.Content = showable.GetName;
            Stats.SetStats(showable.Stats());
            Description.Text = showable.Description();
        }

        public void Reset()
        {
            Shown = null;
            Caption.Content = "";
            Stats.SetStats(new List<Stat>());
            Description.Text = "";
        }
    }
}
