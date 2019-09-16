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
        private bool _showFlowfield;
        /// <summary>
        /// True if flowfield created by right click is visible.
        /// </summary>
        public bool ShowFlowfield
        {
            get
            {
                lock (this)
                    return _showFlowfield;
            }
            set
            {
                lock (this)
                    _showFlowfield = value;
            }
        }
    }
}
