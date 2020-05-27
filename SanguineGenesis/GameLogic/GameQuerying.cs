using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI;

namespace SanguineGenesis.GameLogic
{
    /// <summary>
    /// Used for extracting information about the game.
    /// </summary>
    static class GameQuerying
    {
        /// <summary>
        /// Select all entities in area.
        /// </summary>
        public static IEnumerable<Entity> SelectEntitiesInArea(Game game, Rect area)
        {
            return game.GetAll<Entity>()
                .Where((entity) =>
                {
                    return area.IntersectsWith(entity.Center, entity.Radius);
                });
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
                    return observer.CanSee(u);
                }
                else
                {
                    return observer.VisibleBuildings.Contains(e);
                }
            });
        }

        /// <summary>
        /// Select buildings in area on map.
        /// </summary>
        public static IEnumerable<Building> SelectBuildingsInArea(Map map, Rect area)
        {
            var nearbyNodes = SelectNodes(
                       map, (int)area.Left - 1, (int)area.Bottom - 1, (int)area.Right + 1, (int)area.Top + 1);
            foreach (Node n in nearbyNodes)
            {
                Building b;
                if ((b = n.Building) != null)
                    yield return b;
            }
        }

        /// <summary>
        /// Select the rectangle of Nodes given by the coordinates.
        /// </summary>
        public static Node[,] SelectNodes(Map map, int left, int bottom, int right, int top)
        {
            return SelectPartOfMap(map,left,bottom,right,top);
        }

        /// <summary>
        /// Select the rectangle of squares T given by the coordinates. Coordinates can be out of range of the map.
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
            return SelectPartOfMap(map, (int)area.Left, (int)area.Bottom, (int)area.Right+1, (int)area.Top+1);
        }

        /// <summary>
        /// Select direct neighbors of the given rectangle of squares T.
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
