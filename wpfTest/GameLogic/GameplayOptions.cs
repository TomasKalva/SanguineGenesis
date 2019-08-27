using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    /// <summary>
    /// Describes customizable game features.
    /// </summary>
    public class GameplayOptions
    {
        static GameplayOptions Get { get; }
        static GameplayOptions()
        {
            Get = new GameplayOptions();
        }

        private bool _wholeMapVisible;
        /// <summary>
        /// True if both players can see the whole map.
        /// </summary>
        public bool WholeMapVisible
        {
            get
            {
                lock (this)
                    return _wholeMapVisible;
            }
            set
            {
                lock (this)
                    _wholeMapVisible = value;
            }
        }
        private bool _nutrientsVisible;
        /// <summary>
        /// True if nutrients numbers for each node are visible.
        /// </summary>
        public bool NutrientsVisible
        {
            get
            {
                lock (this)
                    return _nutrientsVisible;
            }
            set
            {
                lock (this)
                    _nutrientsVisible = value;
            }
        }
        private bool _showFlowmap;
        /// <summary>
        /// True if flowmap created by right click is visible.
        /// </summary>
        public bool ShowFlowmap
        {
            get
            {
                lock (this)
                    return _showFlowmap;
            }
            set
            {
                lock (this)
                    _showFlowmap = value;
            }
        }
    }
}
