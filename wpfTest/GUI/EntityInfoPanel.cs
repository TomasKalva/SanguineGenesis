using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace wpfTest.GUI
{

    class EntityInfoPanel : Canvas
    {
        public StatusButtonArray StatusButtonArray { get; }
        public CommandButtonArray CommandButtonArray { get; }
        public StatsTable EntityStatsTable { get; }

        public EntityInfoPanel(int width, int height)
        {
            Width = width;
            Height = height;

            int buttonArrayColumns = 6;
            double buttonArrayHeight = Height / buttonArrayColumns;

            EntityStatsTable = new StatsTable(8, 2, Width, Height - 2 * buttonArrayHeight);
            Children.Add(EntityStatsTable);
            SetLeft(EntityStatsTable, 0);
            SetTop(EntityStatsTable, 0);

            StatusButtonArray = new StatusButtonArray(buttonArrayColumns, 1, Width, buttonArrayHeight);
            Children.Add(StatusButtonArray);
            SetLeft(StatusButtonArray, 0);
            SetBottom(StatusButtonArray, 0);

            CommandButtonArray = new CommandButtonArray(buttonArrayColumns, 1, Width, buttonArrayHeight);
            Children.Add(CommandButtonArray);
            SetLeft(CommandButtonArray, 0);
            SetBottom(CommandButtonArray, buttonArrayHeight);
        }

        public void Update(Entity selectedEntity)
        {
            //dont show info about dead units
            if (selectedEntity == null || selectedEntity.IsDead)
                return;

            EntityStatsTable.SetStats(selectedEntity.Stats());
            StatusButtonArray.Update(selectedEntity.Statuses);
            CommandButtonArray.Update(selectedEntity.CommandQueue.Queue);
        }
    }

}
