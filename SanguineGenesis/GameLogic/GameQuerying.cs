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
    public class GameQuerying
    {
        public static GameQuerying GetGameQuerying()=>new GameQuerying(); 
        private GameQuerying() { }
        
        //todo: some of the following 4 methods might not be needed

        /// <summary>
        /// Select all entities which satisfy the condition.
        /// </summary>
        public IEnumerable<Entity> SelectRectEntities(Game game, Rect area, Func<Entity,bool> condition)
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
        /// Select all units that intersect area and satisfy the condition.
        /// </summary>
        public List<Unit> SelectRectUnits(Game game, Rect area, Func<Unit, bool> condition)
        {
            List<Unit> selected = new List<Unit>();
            foreach (Unit unit in SelectRectEntities(game, area, (e)=>e is Unit))
            {
                Rect unitRect = unit.GetActualRect();
                unitRect = ((IRectangle)unit).GetRect();
                //todo: select units by circles on the ground
                if (area.IntersectsWith(unitRect) && condition(unit))
                {
                    selected.Add(unit);
                }
            }
            return selected;
        }

        /// <summary>
        /// Select all entities which satisfy the condition.
        /// </summary>
        public List<Entity> SelectEntities(Game game, Func<Entity, bool> condition)
        {
            List<Entity> selected = new List<Entity>();
            foreach (Entity entity in game.GetAll<Entity>().Where(condition))
            {
                selected.Add(entity);
            }
            return selected;
        }

        /// <summary>
        /// Returns all entities visible by observer.
        /// </summary>
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
        /// Select the rectangle of Nodes given by the coordinates.
        /// </summary>
        public Node[,] SelectNodes(Map map, int left, int bottom, int right, int top)
        {
            return SelectPartOfMap(map,left,bottom,right,top);
        }

        /// <summary>
        /// Select the rectangle of squares T given by the coordinates.
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
        /// Selects rectangle of squares T. Each T intersects the area.
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
