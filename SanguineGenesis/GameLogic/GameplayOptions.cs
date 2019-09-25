using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic
{
    /// <summary>
    /// Describes customizable game features.
    /// </summary>
    public class GameplayOptions
    {
        public GameplayOptions()
        {
            WholeMapVisible = false;
            NutrientsVisible = false;
            ShowFlowfield = false;
        }

        /// <summary>
        /// True if both players can see the whole map.
        /// </summary>
        public bool WholeMapVisible { get; set; }
        /// <summary>
        /// True if nutrients numbers for each node are visible.
        /// </summary>
        public bool NutrientsVisible { get; set; }
        /// <summary>
        /// True if flowfield created by right click is visible.
        /// </summary>
        public bool ShowFlowfield { get; set; }
    }
}
