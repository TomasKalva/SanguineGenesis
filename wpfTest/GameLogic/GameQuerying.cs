using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    public class GameQuerying
    {
        public static GameQuerying GetGameQuerying()=>new GameQuerying(); 
        private GameQuerying() { }

        public List<Unit> SelectUnits(Game game, Rect area)
        {

            List<Unit> selected = new List<Unit>();
            foreach (Unit unit in game.GetUnits())
            {
                //todo: add querying for unit extents
                float bottom = unit.GetActualBottom(0);
                float top = unit.GetActualTop(0, 0);
                float left = unit.GetActualLeft(0);
                float right = unit.GetActualRight(0, 0);
                Rect unitRect = new Rect(left, bottom, right, top);
                if(area.IntersectsWith(unitRect))
                //if(area.CollidesWith(new Rect(left,bottom,right,top)))
                {
                    selected.Add(unit);
                }
            }
            return selected;

        }

        

        public Node[,] SelectNodes(Map map, Rect area)
        {
            return SelectPartOfMap(map, area);
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
