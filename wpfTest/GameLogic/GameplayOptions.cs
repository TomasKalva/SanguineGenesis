using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    class GameplayOptions
    {
        static GameplayOptions Get { get; }
        static GameplayOptions()
        {
            Get = new GameplayOptions();
        }

        public bool WholeMapVisible { get; set; }
    }
}
