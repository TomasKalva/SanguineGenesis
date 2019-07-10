using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Maps
{
    class VisibilityMap : IMap<bool>
    {
        private bool[,] visible;

        public bool this[int i, int j] => visible[i, j];

        public int Width => visible.GetLength(0);
        public int Height => visible.GetLength(1);

        public VisibilityMap(int width, int height)
        {
            visible = new bool[width, height];
        }

        public void UpdateVisibility(List<Unit> units, ObstacleMap obstMap)
        {
            Clear();
            foreach(Unit u in units)
            {
                AddVisibility(u, obstMap);
            }
        }

        /// <summary>
        /// Set true to all squares visible by the unit.
        /// </summary>
        public void AddVisibility(Unit u, ObstacleMap obstMap)
        {
            float viewRange = u.ViewRange;
            int left = (int)(u.Pos.X - viewRange);
            int right = (int)(u.Pos.X + viewRange);
            int bottom = (int)(u.Pos.Y - viewRange);
            int top = (int)(u.Pos.Y + viewRange);
            for (int i = left; i <= right; i++)
            {
                Ray rTop = new Ray(new Vector2(u.Pos.X, u.Pos.Y),
                    new Vector2(i, top),
                    viewRange,
                    obstMap);
                while (rTop.Next(out int x, out int y))
                    visible[x, y] = true;
                Ray rBottom = new Ray(new Vector2(u.Pos.X, u.Pos.Y),
                    new Vector2(i, bottom),
                    viewRange,
                    obstMap);
                while (rBottom.Next(out int x, out int y))
                    visible[x, y] = true;
            }
            for (int j = bottom; j <= top; j++)
            {
                Ray rLeft = new Ray(new Vector2(u.Pos.X, u.Pos.Y),
                    new Vector2(left, j),
                    viewRange,
                    obstMap);
                while (rLeft.Next(out int x, out int y))
                    visible[x, y] = true;
                Ray rRight = new Ray(new Vector2(u.Pos.X, u.Pos.Y),
                    new Vector2(right, j),
                    viewRange,
                    obstMap);
                while (rRight.Next(out int x, out int y))
                    visible[x, y] = true;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    visible[i, j] = false;
        }
    }
}
