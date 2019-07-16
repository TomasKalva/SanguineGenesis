using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;

namespace wpfTest
{
    public class Player
    {
        public List<Unit> Units { get; private set; }
        public VisibilityMap VisibilityMap { get; set; }
        public Players PlayerID { get; }

        public Player(Players playerID)
        {
            PlayerID = playerID;
            InitUnits();
        }

        public void InitUnits()
        {
            Units = new List<Unit>();
            UnitFactory normalUnits = new UnitFactory(UnitType.TIGER, 0.5f,2f,2f,10,10);
            UnitFactory smallFastUnits = new UnitFactory(UnitType.TIGER, 0.25f, 3f, 3f,10,0);
            UnitFactory bigUnits = new UnitFactory(UnitType.BAOBAB, 1f, 2f, 4f,10,0);
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Units.Add(normalUnits.NewInstance(PlayerID, new Vector2(20 + i*.25f,10+ j*.25f)));
                }
            }
            Units.Add(bigUnits.NewInstance(PlayerID, new Vector2(5f, 6f)));
            Units.Add(new Unit(PlayerID, UnitType.TIGER, 10, 10, new Vector2(5f, 6f)));
            Units.Add(new Unit(PlayerID, UnitType.TIGER, 10, 10, new Vector2(7f, 6f)));
            Units.Add(new Unit(PlayerID, UnitType.TIGER, 10, 10, new Vector2(6.5f, 6f)));
            Units.Add(new Unit(PlayerID, UnitType.TIGER, 10, 10, new Vector2(4f, 9f)));
        }

        public void UpdateVisibilityMap(ObstacleMap obstMap)
        {
            VisibilityMap.FindVisibility(Units.Select((unit) => unit.UnitView).ToList(), obstMap);
        }
    }

    public enum Players
    {
        PLAYER0,
        PLAYER1
    }
}
