using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    /*public abstract class MapSelector:IEntity
    {
        public virtual float Bottom { get; protected set; }
        public virtual float Left { get; protected set; }
        public virtual float Right { get; protected set; }
        public virtual float Top { get; protected set; }
        public virtual float Width { get; protected set; }
        public virtual float Height { get; protected set; }

        public List<Unit> SelectUnits(Game game)
        {

            List<Unit> selected = new List<Unit>();
            foreach (Unit unit in game.GetUnits())
            {
                //todo: add querying for unit extents
                float bottom = unit.GetActualBottom(0);
                float top = unit.GetActualTop(0, 0);
                float left = unit.GetActualLeft(0);
                float right = unit.GetActualRight(0, 0);
                if (PointInside(left, bottom) ||
                    PointInside(right, bottom) ||
                    PointInside(left, top) ||
                    PointInside(right, top))
                {
                    selected.Add(unit);
                }
            }
            return selected;

        }

        /// <summary>
        /// Returns true if the point is inside this map view. Coordinates are
        /// relative to the size of a node.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <returns></returns>
        public bool PointInside(float x, float y)
        {
            return x >= Left && x <= Right && y >= Bottom && y <= Top;
        }

        public Node[,] SelectNodes(Map map)
        {
            return SelectPartOfMap(map);
        }

        public T[,] SelectPartOfMap<T>(IMap<T> map)
        {
            //todo: set more optimal array extents - so that it doesnt contain nulls
            int width = Math.Min((int)(Math.Ceiling(Width) + 1),
                (int)(map.Width - Left));
            int height = Math.Min((int)(Math.Ceiling(Height) + 1),
                (int)(map.Height - Bottom));

            T[,] visible = new T[width + 1, height + 1];

            for (int i = 0; i <= width; i++)
                for (int j = 0; j <= height; j++)
                {
                    int mapI = i + (int)Left;
                    int mapJ = j + (int)Bottom;
                    //check if coordinates are valid
                    if (mapI >= map.Width)
                        goto afterLoop;
                    if (mapI < 0)
                        break;
                    if (mapJ >= map.Height)
                        break;
                    if (mapJ < 0)
                        continue;

                    visible[i, j] = map[mapI, mapJ];
                }
            afterLoop:;
            return visible;
        }
    }*/
}
