using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Maps
{
    /// <summary>
    /// Represents part of a map visible by a player.
    /// </summary>
    class VisibilityMap : IMap<bool>
    {
        private static VisibilityMap everythingVisible;
        /// <summary>
        /// Visibility map where the whole map is visible.
        /// </summary>
        public static VisibilityMap GetEverythingVisible(int width, int height)
        {
            if(everythingVisible==null)
            {
                everythingVisible = new VisibilityMap(width, height);
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                        everythingVisible.visible[i, j] = true;
            }
            return everythingVisible;
        }

        /// <summary>
        /// Data of the map.
        /// </summary>
        private bool[,] visible;

        public bool this[int i, int j] => visible[i, j];

        /// <summary>
        /// Width of the map in squares.
        /// </summary>
        public int Width => visible.GetLength(0);
        /// <summary>
        /// Height of the map in squares.
        /// </summary>
        public int Height => visible.GetLength(1);

        /// <summary>
        /// Creates new visibility map with the given widht and height. Nothing is visible.
        /// </summary>
        public VisibilityMap(int width, int height)
        {
            visible = new bool[width, height];
        }

        /// <summary>
        /// Sets true to all squares visible by the entities.
        /// </summary>
        public void FindVisibility(List<View> entities, ObstacleMap obstMap)
        {
            foreach(View v in entities)
            {
                AddVisibility(v, obstMap);
            }
        }

        /// <summary>
        /// Set true to all squares visible by the entity.
        /// </summary>
        public void AddVisibility(View v, ObstacleMap obstMap)
        {
            float viewRange = v.Range;
            int left = (int)(v.Position.X - viewRange);
            int right = (int)(v.Position.X + viewRange) + 1;
            int bottom = (int)(v.Position.Y - viewRange);
            int top = (int)(v.Position.Y + viewRange) + 1;
            //cast rays to the lines on bottom and top of the square around v
            for (int i = left; i <= right; i++)
            {
                Ray rTop = new Ray(new Vector2(v.Position.X, v.Position.Y),
                    new Vector2(i, top),
                    viewRange,
                    obstMap);
                int x; int y;
                while (rTop.Next(out x, out y))
                    visible[x, y] = true;
                if (x != -1 && y != -1)
                    visible[x, y] = true;
                Ray rBottom = new Ray(new Vector2(v.Position.X, v.Position.Y),
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
                Ray rLeft = new Ray(new Vector2(v.Position.X, v.Position.Y),
                    new Vector2(left, j),
                    viewRange,
                    obstMap);
                int x; int y;
                while (rLeft.Next(out x, out y))
                    visible[x, y] = true;
                if (x != -1 && y != -1)
                    visible[x, y] = true;
                Ray rRight = new Ray(new Vector2(v.Position.X, v.Position.Y),
                    new Vector2(right, j),
                    viewRange,
                    obstMap);
                while (rRight.Next(out x, out y))
                    visible[x, y] = true;
                if (x != -1 && y != -1)
                   visible[x, y] = true;
            }
            //add the square which contains v
            int vX = (int)v.Position.X;
            int vY = (int)v.Position.Y;
            if(vX >= 0 && vX < Width && vY >= 0 && vY < Height)
                visible[vX, vY] = true;
        }
    }
}
