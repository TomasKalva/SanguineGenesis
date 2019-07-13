using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Maps
{
    public class VisibilityMap : IMap<bool>
    {
        private bool[,] visible;

        public bool this[int i, int j] => visible[i, j];

        public int Width => visible.GetLength(0);
        public int Height => visible.GetLength(1);

        public VisibilityMap(int width, int height)
        {
            visible = new bool[width, height];
        }

        public void FindVisibility(List<UnitView> units, ObstacleMap obstMap)
        {
            foreach(UnitView v in units)
            {
                AddVisibility(v, obstMap);
            }
        }

        /// <summary>
        /// Set true to all squares visible by the unit.
        /// </summary>
        public void AddVisibility(UnitView v, ObstacleMap obstMap)
        {
            float viewRange = v.Range;
            int left = (int)(v.Pos.X - viewRange);
            int right = (int)(v.Pos.X + viewRange) + 1;
            int bottom = (int)(v.Pos.Y - viewRange);
            int top = (int)(v.Pos.Y + viewRange) + 1;
            //cast rays to the lines on bottom and top of the square around v
            for (int i = left; i <= right; i++)
            {
                Ray rTop = new Ray(new Vector2(v.Pos.X, v.Pos.Y),
                    new Vector2(i, top),
                    viewRange,
                    obstMap);
                while (rTop.Next(out int x, out int y))
                    visible[x, y] = true;
                Ray rBottom = new Ray(new Vector2(v.Pos.X, v.Pos.Y),
                    new Vector2(i, bottom),
                    viewRange,
                    obstMap);
                while (rBottom.Next(out int x, out int y))
                    visible[x, y] = true;
            }
            //cast rays to the lines on left and right of the square around v
            for (int j = bottom; j <= top; j++)
            {
                Ray rLeft = new Ray(new Vector2(v.Pos.X, v.Pos.Y),
                    new Vector2(left, j),
                    viewRange,
                    obstMap);
                while (rLeft.Next(out int x, out int y))
                    visible[x, y] = true;
                Ray rRight = new Ray(new Vector2(v.Pos.X, v.Pos.Y),
                    new Vector2(right, j),
                    viewRange,
                    obstMap);
                while (rRight.Next(out int x, out int y))
                    visible[x, y] = true;
            }
            //add the square which contains v
            int vX = (int)v.Pos.X;
            int vY = (int)v.Pos.Y;
            if(vX >= 0 && vX < Width && vY >= 0 && vY < Height)
                visible[vX, vY] = true;
        }
    }
}
