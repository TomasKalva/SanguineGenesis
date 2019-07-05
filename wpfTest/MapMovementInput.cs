using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static wpfTest.MainWindow;

namespace wpfTest
{
    class MapMovementInput
    {
        public List<Direction> MapDirection { get; private set; }

        public MapMovementInput()
        {
            MapDirection = new List<Direction>();
        }

        public void AddDirection(Direction dir)
        {
            if (MapDirection.Contains(dir))
                return;

            if (MapDirection.Contains(Opposite(dir)))
            {
                MapDirection.Remove(Opposite(dir));
            }
            MapDirection.Add(dir);
        }

        public void RemoveDirection(Direction dir)
        {
            MapDirection.Remove(dir);
        }

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
}
