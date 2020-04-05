using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;

namespace SanguineGenesis.GUI.WinFormsComponents
{
    /// <summary>
    /// Shows info about entity.
    /// </summary>
    class EntityInfoPanel : Panel
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
        /// Show name of the entity.
        /// </summary>
        public Label EntityName { get; }

        /// <summary>
        /// Entity whose info is shown.
        /// </summary>
        public Entity SelectedEntity { get; set; }

        public EntityInfoPanel(int width, int height)
        {
            Width = width;
            Height = height;

            int buttonArrayColumns = 6;
            int buttonArrayHeight = Height / buttonArrayColumns;
            int labelHeight = 30;
            int progressBarWidth = Width / 15;

            //status button array initialization
            StatusButtonArray = new StatusButtonArray(buttonArrayColumns, 1, Width, buttonArrayHeight);
            Controls.Add(StatusButtonArray);
            StatusButtonArray.Location = new System.Drawing.Point(Width / 2 - StatusButtonArray.Width / 2, Height - StatusButtonArray.Height);

            //command button array initialization
            CommandButtonArray = new CommandButtonArray(buttonArrayColumns, 1, Width - progressBarWidth, buttonArrayHeight);
            Controls.Add(CommandButtonArray);
            CommandButtonArray.Location = new System.Drawing.Point(
                progressBarWidth + (Width - progressBarWidth - CommandButtonArray.Width),
                Height - (CommandButtonArray.Height + CommandButtonArray.Height));

            //entity stats table initialization
            EntityStatsTable = new StatsTable(9, 2, Width, Height - (CommandButtonArray.Height + StatusButtonArray.Height + labelHeight));
            Controls.Add(EntityStatsTable);
            EntityStatsTable.Location = new System.Drawing.Point(0, labelHeight);

            //entity name initialization
            EntityName = new Label()
            {
                Width = Width,
                Height = labelHeight,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Label.DefaultFont, System.Drawing.FontStyle.Bold),
                BackColor = Color.LightGray
            };
            Controls.Add(EntityName);
            EntityName.Location = new System.Drawing.Point(0, 0);

            FirstCommandProgress = new ProgressBar()
            {
                Width = progressBarWidth,
                Height =  CommandButtonArray.Height
            };
            Controls.Add(FirstCommandProgress);
            FirstCommandProgress.Maximum = 100;
            FirstCommandProgress.Location = new System.Drawing.Point(0, Height - (CommandButtonArray.Height + StatusButtonArray.Height));
            FirstCommandProgress.Value = 70;

            BackColor = Color.Gray;
        }

        /// <summary>
        /// Update the components with the SelectedEntity info.
        /// </summary>
        public void UpdateControl()
        {
            if (SelectedEntity == null || SelectedEntity.IsDead)
            {
                //reset window if SelectedEntity doesn't exist or is dead
                EntityStatsTable.SetStats(new List<Stat>());
                StatusButtonArray.InfoSources = new List<Status>();
                StatusButtonArray.UpdateControl();
                CommandButtonArray.InfoSources = new List<Command>();
                CommandButtonArray.UpdateControl();
                FirstCommandProgress.Value = 0;
                EntityName.Text = "";
            }
            else
            {
                //show info about SelectedEntity
                EntityStatsTable.SetStats(SelectedEntity.Stats());
                StatusButtonArray.InfoSources = SelectedEntity.Statuses;
                StatusButtonArray.UpdateControl();
                List<Command> commandQueue= SelectedEntity.CommandQueue.Queue;
                CommandButtonArray.InfoSources = commandQueue;
                CommandButtonArray.UpdateControl();
                EntityName.Text = ((IShowable)SelectedEntity).GetName().Replace("_"," ");
                if (commandQueue.Any())
                {
                    //the animation of progress bar is too slow, reducing progress skips animation
                    FirstCommandProgress.Value = Math.Min(100, commandQueue[0].Progress+1);
                    FirstCommandProgress.Value = Math.Min(100, commandQueue[0].Progress);
                }
                else
                    FirstCommandProgress.Value = 0;
            }
        }
    }
}
