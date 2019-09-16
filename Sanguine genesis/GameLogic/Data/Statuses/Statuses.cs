using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Entities
{
    /// <summary>
    /// Contains statuses that are used in the game.
    /// </summary>
    public class Statuses
    {
        public UndergroundFactory UndergroundFactory { get; }

        public Statuses()
        {
            UndergroundFactory = new UndergroundFactory();
        }

    }
}
