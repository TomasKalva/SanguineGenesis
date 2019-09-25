using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis
{
    /// <summary>
    /// Represents where the movement of MapView.
    /// </summary>
    class MapMovementInput
    {
        /// <summary>
        /// List of directions, in which the MapView moves.
        /// </summary>
        public List<Direction> MapDirection { get; private set; }

        public MapMovementInput()
        {
            MapDirection = new List<Direction>();
        }

        /// <summary>
        /// Adds movement direction dir. If the opposite direction is in list of movement directions,
        /// remove it.
        /// </summary>
        public void AddDirection(Direction dir)
        {
            //don't add direction twice
            if (MapDirection.Contains(dir))
                return;

            //remove opposite direction
            if (MapDirection.Contains(Opposite(dir)))
            {
                MapDirection.Remove(Opposite(dir));
            }
            MapDirection.Add(dir);
        }

        /// <summary>
        /// Removes movement direction dir.
        /// </summary>
        public void RemoveDirection(Direction dir)
        {
            MapDirection.Remove(dir);
        }

        /// <summary>
        /// Opposite direction to the dir.
        /// </summary>
        private Direction Opposite(Direction dir)
        {
            switch (dir)
            {
                case Direction.DOWN: return Direction.UP;
                case Direction.UP: return Direction.DOWN;
                case Direction.LEFT: return Direction.RIGHT;
                default: return Direction.LEFT;
            }
        }
    }

    public enum Direction
    {
        LEFT,
        UP,
        RIGHT,
        DOWN
    }
}
