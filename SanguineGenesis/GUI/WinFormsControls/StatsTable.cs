﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace SanguineGenesis.GUI.WinFormsControls
{
    /// <summary>
    /// Shows list of Stats.
    /// </summary>
    class StatsTable : TableLayoutPanel
    {
        /// <summary>
        /// Array with the real layout of this control. First index is row, second is column.
        /// </summary>
        private Label[,] Stats { get; }

        public StatsTable(int rows, int columns, int width, int height, Color backColor)
        {
            Width = width;
            Height = height;
            RowCount = rows;
            ColumnCount = 2 * columns;
            Stats = new Label[ColumnCount, RowCount];

            int labelWidth = width / ColumnCount;
            int labelHeight = height / RowCount;
            for (int j = 0; j < RowCount; j++)
                for (int i = 0; i < ColumnCount; i++)
                {
                    Label l = new Label
                    {
                        Width = labelWidth,
                        Height = labelHeight,
                        Padding = Padding.Empty,
                        Margin = Padding.Empty
                    };
                    Stats[i, j] = l;
                    if (i % 2 == 0)
                        l.TextAlign = ContentAlignment.MiddleLeft;
                    else
                        l.TextAlign = ContentAlignment.MiddleRight;
                    Controls.Add(l);
                }
            BackColor = backColor;
        }

        /// <summary>
        /// Update the components with stats.
        /// </summary>
        public void SetStats(List<Stat> stats)
        {
            for (int i = 0; i < ColumnCount / 2; i++)
                for (int j = 0; j < RowCount; j++)
                {
                    Label name = Stats[2 * i, j];
                    Label value = Stats[2 * i + 1, j];
                    int statsIndex = i * RowCount + j;
                    if (statsIndex < stats.Count)
                    {
                        Stat s = stats[statsIndex];
                        name.Text = s.Name;
                        value.Text = s.Value;
                    }
                    else
                    {
                        name.Text = "";
                        value.Text = "";
                    }
                }
        }
    }

    /// <summary>
    /// Pair of name and value.
    /// </summary>
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
