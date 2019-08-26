using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class GameplayOptions
    {
        static GameplayOptions Get { get; }
        static GameplayOptions()
        {
            Get = new GameplayOptions();
        }

        private bool _wholeMapVisible;
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
