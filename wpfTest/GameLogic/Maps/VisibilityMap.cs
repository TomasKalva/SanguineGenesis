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

        public void FindVisibility(List<View> units, ObstacleMap obstMap)
        {
            foreach(View v in units)
            {
                AddVisibility(v, obstMap);
            }
        }

        /// <summary>
        /// Set true to all squares visible by the unit.
        /// </summary>
        public void AddVisibility(View v, ObstacleMap obstMap)
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
                int x; int y;
                while (rTop.Next(out x, out y))
                    visible[x, y] = true;
                if (x != -1 && y != -1)
                    visible[x, y] = true;
                Ray rBottom = new Ray(new Vector2(v.Pos.X, v.Pos.Y),
                    new Vector2(i, bottom),
                    viewRange,
                    obstMap);
                while (rBottom.Next(out x, out y))
                    visible[x, y] = true;
                if (x != -1 && y != -1)
                   visible[x, y] = true;
            }
            //cast rays to the lines on left and right of the square around v
            for (int j = bottom; j <= top; j++)
            {
                Ray rLeft = new Ray(new Vector2(v.Pos.X, v.Pos.Y),
                    new Vector2(left, j),
                    viewRange,
                    obstMap);
                int x; int y;
                while (rLeft.Next(out x, out y))
                    visible[x, y] = true;
                if (x != -1 && y != -1)
                    visible[x, y] = true;
                Ray rRight = new Ray(new Vector2(v.Pos.X, v.Pos.Y),
                    new Vector2(right, j),
                    viewRange,
                    obstMap);
                while (rRight.Next(out x, out y))
                    visible[x, y] = true;
                if (x != -1 && y != -1)
                   visible[x, y] = true;
            }
            //add the square which contains v
            int vX = (int)v.Pos.X;
            int vY = (int)v.Pos.Y;
            if(vX >= 0 && vX < Width && vY >= 0 && vY < Height)
                visible[vX, vY] = true;
        }

        /*
        /// <summary>
        /// Returns true if at least one of the building's nodes is visible.
        /// </summary>
        public bool IsVisible(Building b)
        {
            foreach(Node n in b.Nodes)
            {
                if (visible[n.X, n.Y])
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Returns true if at least part of the unit is visible.
        /// </summary>
        public bool IsVisible(Unit u)
        {
            //todo: check for intersection with the circle instead of the center
            return visible[(int)u.Center.X, (int)u.Center.Y];
        }*/
    }
}
