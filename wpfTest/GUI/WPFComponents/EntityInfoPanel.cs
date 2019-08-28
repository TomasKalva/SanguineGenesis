using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using wpfTest.GameLogic.Data.Entities;
using static wpfTest.MainWindow;

namespace wpfTest.GUI
{
    /// <summary>
    /// Shows info about entity.
    /// </summary>
    class EntityInfoPanel : Canvas
    {
        /// <summary>
        /// Button array showing statuses.
        /// </summary>
        public StatusButtonArray StatusButtonArray { get; }
        /// <summary>
        /// Button array showing commands.
        /// </summary>
        public CommandButtonArray CommandButtonArray { get; }
        /// <summary>
        /// Stats table with the entity's stats.
        /// </summary>
        public StatsTable EntityStatsTable { get; }
        /// <summary>
        /// Progress bar showing progress of the entity's first command.
        /// </summary>
        public ProgressBar FirstCommandProgress { get; }

        /// <summary>
        /// Entity whose info is shown.
        /// </summary>
        public Entity SelectedEntity { get; set; }

        public EntityInfoPanel(int width, int height)
        {
            Width = width;
            Height = height;

            int buttonArrayColumns = 6;
            double buttonArrayHeight = Height / buttonArrayColumns;
            double progressBarWidth = Width / 15;

            EntityStatsTable = new StatsTable(8, 2, Width, Height - 2 * buttonArrayHeight);
            Children.Add(EntityStatsTable);
            SetLeft(EntityStatsTable, 0);
            SetTop(EntityStatsTable, 0);

            StatusButtonArray = new StatusButtonArray(buttonArrayColumns, 1, Width, buttonArrayHeight);
            Children.Add(StatusButtonArray);
            SetLeft(StatusButtonArray, 0);
            SetBottom(StatusButtonArray, 0);


            CommandButtonArray = new CommandButtonArray(buttonArrayColumns, 1, Width - progressBarWidth, buttonArrayHeight);
            Children.Add(CommandButtonArray);
            SetLeft(CommandButtonArray, progressBarWidth);
            SetBottom(CommandButtonArray, buttonArrayHeight);

            FirstCommandProgress = new ProgressBar()
            {
                Width = progressBarWidth,
                Height = buttonArrayHeight
            };
            Children.Add(FirstCommandProgress);
            FirstCommandProgress.Orientation = Orientation.Vertical;
            SetLeft(FirstCommandProgress, 0);
            SetBottom(FirstCommandProgress, buttonArrayHeight);
            FirstCommandProgress.Value = 70;
        }

        /// <summary>
        /// Update the components with the SelectedEntity info.
        /// </summary>
        public void Update()
        {
            if (SelectedEntity == null || SelectedEntity.IsDead)
            {
                //reset window if SelectedEntity doesn't exist or is dead
                EntityStatsTable.SetStats(new List<Stat>());
                StatusButtonArray.InfoSources = new List<Status>();
                StatusButtonArray.Update();
                CommandButtonArray.InfoSources = new List<Command>();
                CommandButtonArray.Update();
                FirstCommandProgress.Value = 0;
            }
            else
            {
                //show info about SelectedEntity
                EntityStatsTable.SetStats(SelectedEntity.Stats());
                StatusButtonArray.InfoSources = SelectedEntity.Statuses;
                StatusButtonArray.Update();
                List<Command> commandQueue= SelectedEntity.CommandQueue.Queue;
                CommandButtonArray.InfoSources = commandQueue;
                CommandButtonArray.Update();
                if (commandQueue.Any())
                    FirstCommandProgress.Value = commandQueue[0].Progress;
                else
                    FirstCommandProgress.Value = 0;
            }
        }
    }

}
