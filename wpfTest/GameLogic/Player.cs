using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;

namespace wpfTest
{
    class Player
    {
        public List<Unit> Units { get; private set; }
        public VisibilityMap VisibilityMap { get; private set; }

        public Player(int mapWidth, int mapHeight)
        {
            InitUnits();
            VisibilityMap = new VisibilityMap(mapWidth, mapHeight);
        }

        public void InitUnits()
        {
            Units = new List<Unit>();
            UnitFactory normalUnits = new UnitFactory(0.5f,2f,2f);
            UnitFactory smallFastUnits = new UnitFactory(0.25f, 3f, 3f);
            UnitFactory bigUnits = new UnitFactory(1f, 2f, 4f);
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Units.Add(normalUnits.NewInstance(new Vector2(20 + i*.25f,10+ j*.25f)));
                }
            }
            Units.Add(bigUnits.NewInstance(new Vector2(5f, 6f)));
            Units.Add(new Unit(new Vector2(5f, 6f)));
            Units.Add(new Unit(new Vector2(7f, 6f)));
            Units.Add(new Unit(new Vector2(6.5f, 6f)));
            Units.Add(new Unit(new Vector2(4f, 9f)));
        }

        public void UpdateVisibilityMap(ObstacleMap obstMap)
        {
            VisibilityMap.UpdateVisibility(Units, obstMap);
        }
    }
}
