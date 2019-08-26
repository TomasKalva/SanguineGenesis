﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static wpfTest.MainWindow;

namespace wpfTest.GUI
{
    public class StatsTable : Grid
    {
        public List<Stat> Stats { get; set; }
        public int Rows { get; }
        public int Columns { get; }

        public StatsTable(int rows, int columns, double width, double height)
        {
            Rows = rows;
            Columns = columns;
            Width = width;
            Height = height;

            for (int i = 0; i < 2 * columns; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < rows; i++)
            {
                RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < (2 * columns) * rows; i++)
            {
                Label l = new Label();
                int row = i % (2 * Rows) / 2;
                int column = i % 2 == 0 ? 2 * (i / (2 * Rows)) : 2 * (i / (2 * Rows)) + 1;
                l.SetValue(ColumnProperty, column);
                l.SetValue(RowProperty, row);
                l.Focusable = false;
                if (i % 2 == 0)
                    l.HorizontalAlignment = HorizontalAlignment.Left;
                else
                    l.HorizontalAlignment = HorizontalAlignment.Center;
                Children.Add(l);
            }

            Style = (Style)Application.Current.FindResource("StatsTableStyle");
        }

        public void SetStats(List<Stat> stats)
        {
            for (int i = 0; i < Rows * Columns; i++)
            {
                Label name = (Label)Children[2 * i];
                Label value = (Label)Children[2 * i + 1];
                if (i < stats.Count)
                {
                    Stat s = stats[i];
                    name.Content = s.Name;
                    value.Content = s.Value;
                }
                else
                {
                    name.Content = "";
                    value.Content = "";
                }

            }
        }
    }

    public struct Stat
    {
        public string Name { get; }
        public string Value { get; }

        public Stat(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}