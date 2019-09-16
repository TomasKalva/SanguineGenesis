using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    /// <summary>
    /// Describes view of an entity. Used as parameter for VisibilityMapGenerator, which
    /// works in different thread than the game thread.
    /// </summary>
    public struct View
    {
        /// <summary>
        /// Position of the entity.
        /// </summary>
        public Vector2 Position { get; }
        /// <summary>
        /// View range of the entity.
        /// </summary>
        public float Range { get; }

        public View(Vector2 position, float range)
        {
            Position = position;
            Range = range;
        }
    }
}
