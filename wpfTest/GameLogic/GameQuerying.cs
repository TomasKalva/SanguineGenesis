using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Data.Entities;
using wpfTest.GUI;

namespace wpfTest
{
    public class GameQuerying
    {
        public static GameQuerying GetGameQuerying()=>new GameQuerying(); 
        private GameQuerying() { }

        public IEnumerable<Entity> SelectRectEntities(Game game, Rect area, Func<Entity,bool> entityProperty)
        {
            return game.GetEntities()
                .Where(entityProperty)
                .Where((unit) =>
                {
                    Rect unitRect = ((IRectangle)unit).GetRect();
                    return area.IntersectsWith(unitRect);
                });
        }

        public List<Unit> SelectRectUnits(Game game, Rect area, Func<Unit, bool> unitProperty)
        {
            List<Unit> selected = new List<Unit>();
            foreach (Unit unit in SelectRectEntities(game, area, (e)=>e is Unit))
            {
                Rect unitRect = unit.GetActualRect(ImageAtlas.GetImageAtlas);
                unitRect = ((IRectangle)unit).GetRect();
                //todo: select units by circles on the ground
                if (area.IntersectsWith(unitRect))
                {
                    selected.Add(unit);
                }
            }
            return selected;
        }

        public List<Entity> SelectUnits(Game game, Func<Entity, bool> unitProperty)
        {
            List<Entity> selected = new List<Entity>();
            foreach (Entity unit in game.GetUnits().Where(unitProperty))
            {
                selected.Add(unit);
            }
            return selected;
        }

        public IEnumerable<Entity> SelectVisibleEntities(Game game, Player observer, IEnumerable<Entity> entities)
        {
            return entities.Where((e) =>
            {
                if (e is Unit u)
                {
                    return u.IsVisible(observer.VisibilityMap);
                }
                else
                {
                    return observer.VisibleBuildings.Contains(e);
                }
            });
        }

        public Node[,] SelectNodes(Map map, Rect area)
        {
            return SelectPartOfMap(map, area);
        }

        /// <summary>
        /// Select the rectangle of nodes give by the coordinates.
        /// </summary>
        public Node[,] SelectNodes(Map map, int left, int bottom, int right, int top)
        {
            return SelectPartOfMap(map,left,bottom,right,top);
        }

        /// <summary>
        /// Select the rectangle of T give by the coordinates.
        /// </summary>
        public T[,] SelectPartOfMap<T>(IMap<T> map, int left, int bottom, int right, int top)
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
        /// Selects rectangle of T. Each T intersects the area.
        /// </summary>
        public T[,] SelectPartOfMap<T>(IMap<T> map, Rect area)
        {
            int width = Math.Min((int)(Math.Ceiling(area.Width) + 1),
                (int)(map.Width - area.Left));
            int height = Math.Min((int)(Math.Ceiling(area.Height) + 1),
                (int)(map.Height - area.Bottom));
            return SelectPartOfMap(map, (int)area.Left, (int)area.Bottom, (int)area.Right+1, (int)area.Top+1);
        }
    }
}
