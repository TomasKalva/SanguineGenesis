using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GUI;

namespace wpfTest
{
    public class GameQuerying
    {
        public static GameQuerying GetGameQuerying()=>new GameQuerying(); 
        private GameQuerying() { }

        public List<Entity> SelectRectEntities(Game game, Rect area, Func<Entity,bool> entityProperty)
        {
            List<Entity> selected = new List<Entity>();
            foreach (Entity unit in game.GetEntities().Where(entityProperty))
            {
                Rect unitRect = unit.GetActualRect(ImageAtlas.GetImageAtlas);
                //todo: select units by circles on the ground
                if (area.IntersectsWith(unitRect))
                {
                    selected.Add(unit);
                }
            }
            return selected;
        }

        public List<Unit> SelectRectUnits(Game game, Rect area, Func<Unit, bool> unitProperty)
        {
            List<Unit> selected = new List<Unit>();
            foreach (Unit unit in SelectRectEntities(game, area, (e)=>e.GetType()==typeof(Unit)))
            {
                Rect unitRect = unit.GetActualRect(ImageAtlas.GetImageAtlas);
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

        public Node[,] SelectNodes(Map map, Rect area)
        {
            return SelectPartOfMap(map, area);
        }

        public Node[,] SelectNodes(Map map, int left, int bottom, int right, int top)
        {
            //clamp extents to be inside of the map
            int validL = Math.Min(map.Width, Math.Max(0, left));
            int validB = Math.Min(map.Height, Math.Max(0, bottom));
            int validR = Math.Min(map.Width, Math.Max(0, right));
            int validT = Math.Min(map.Height, Math.Max(0, top));

            int width = validR - validL + 1;
            int height = validT - validB + 1;
            Node[,] selected = new Node[width, height ];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    selected[i, j] = map[validL + i, validB + j];
                }
            return selected;
        }

        public T[,] SelectPartOfMap<T>(IMap<T> map, Rect area)
        {
            
            //todo: set more optimal array extents - so that it doesnt contain nulls
            int width = Math.Min((int)(Math.Ceiling(area.Width) + 1),
                (int)(map.Width - area.Left));
            int height = Math.Min((int)(Math.Ceiling(area.Height) + 1),
                (int)(map.Height - area.Bottom));

            T[,] visible = new T[width + 1, height + 1];

            for (int i = 0; i <= width; i++)
                for (int j = 0; j <= height; j++)
                {
                    int mapI = i + (int)area.Left;
                    int mapJ = j + (int)area.Bottom;
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
    }
}
