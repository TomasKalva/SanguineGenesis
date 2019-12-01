using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GUI;

namespace SanguineGenesis
{
    /// <summary>
    /// Used for extracting information about the game.
    /// </summary>
    static class GameQuerying
    {
        /// <summary>
        /// Select all entities which satisfy the condition.
        /// </summary>
        public static IEnumerable<Entity> SelectRectEntities(Game game, Rect area, Func<Entity,bool> condition)
        {
            return game.GetAll<Entity>()
                .Where(condition)
                .Where((unit) =>
                {
                    Rect unitRect = ((IRectangle)unit).GetRect();
                    return area.IntersectsWith(unitRect);
                });
        }

        /// <summary>
        /// Select all entities which satisfy the condition.
        /// </summary>
        public static List<Entity> SelectEntities(Game game, Func<Entity, bool> condition)
        {
            return game.GetAll<Entity>().Where(condition).ToList();
        }

        /// <summary>
        /// Returns all entities visible by observer.
        /// </summary>
        public static IEnumerable<Entity> SelectVisibleEntities(Player observer, IEnumerable<Entity> entities)
        {
            return entities.Where((e) =>
            {
                if (e is Unit u)
                {
                    //return u.IsVisible(observer.VisibilityMap);
                    return observer.CanSee(u);
                }
                else
                {
                    return observer.VisibleBuildings.Contains(e);
                }
            });
        }

        /// <summary>
        /// Select the rectangle of Nodes given by the coordinates.
        /// </summary>
        public static Node[,] SelectNodes(Map map, int left, int bottom, int right, int top)
        {
            return SelectPartOfMap(map,left,bottom,right,top);
        }

        /// <summary>
        /// Select the rectangle of squares T given by the coordinates.
        /// </summary>
        public static T[,] SelectPartOfMap<T>(IMap<T> map, int left, int bottom, int right, int top)
        {
            //clamp extents to be inside of the map
            int validL = Math.Min(map.Width - 1, Math.Max(0, left));
            int validB = Math.Min(map.Height - 1, Math.Max(0, bottom));
            int validR = Math.Min(map.Width - 1, Math.Max(0, right));
            int validT = Math.Min(map.Height - 1, Math.Max(0, top));

            int width = validR - validL + 1;
            int height = validT - validB + 1;
            T[,] selected = new T[width, height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    selected[i, j] = map[validL + i, validB + j];
                }
            return selected;
        }

        /// <summary>
        /// Selects rectangle of squares T. Each T intersects the area.
        /// </summary>
        public static T[,] SelectPartOfMap<T>(IMap<T> map, Rect area)
        {
            int width = Math.Min((int)(Math.Ceiling(area.Width) + 1),
                (int)(map.Width - area.Left));
            int height = Math.Min((int)(Math.Ceiling(area.Height) + 1),
                (int)(map.Height - area.Bottom));
            return SelectPartOfMap(map, (int)area.Left, (int)area.Bottom, (int)area.Right+1, (int)area.Top+1);
        }

        /// <summary>
        /// Select neighbors of the given rectangle of squares T.
        /// _++++_
        /// +rrrr+
        /// +rrrr+
        /// _++++_
        /// </summary>
        public static IEnumerable<T> SelectNeighbors<T>(IMap<T> map, int left, int bottom, int right, int top)
        {
            return HorizontalLine(map, bottom - 1, left, right)
                .Concat(VerticalLine(map, right + 1, bottom, top))
                .Concat(VerticalLine(map, left - 1, bottom, top))
                .Concat(HorizontalLine(map, top + 1, left, right));
        }

        /// <summary>
        /// Select vertical line of squares T given by the coordinates.
        /// </summary>
        public static List<T> VerticalLine<T>(IMap<T> map, int x, int bottom, int top)
        {
            List<T> line = new List<T>();
            if (x >= 0 && x < map.Width)
            {
                for (int i = bottom; i < top; i++)
                {
                    line.Add(map[x, i]);
                }
            }
            return line;
        }

        /// <summary>
        /// Select horizontal line of squares T given by the coordinates.
        /// </summary>
        public static List<T> HorizontalLine<T>(IMap<T> map, int y, int left, int right)
        {
            List<T> line = new List<T>();
            if (y >= 0 && y < map.Height)
            {
                for (int i = left; i < right; i++)
                {
                    line.Add(map[i, y]);
                }
            }
            return line;
        }
    }
}
