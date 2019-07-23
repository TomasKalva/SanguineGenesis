using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Maps;

namespace wpfTest
{
    public class Player
    {
        public List<Entity> Entities { get; private set; }
        public List<Unit> Units => Entities.Where((e) => e.GetType() == typeof(Unit)).Select((u)=>(Unit)u).ToList();
        public VisibilityMap VisibilityMap { get; set; }
        public bool MapChanged { get; private set; }
        public Map MapView { get; private set; }
        public Players PlayerID { get; }

        public Player(Players playerID)
        {
            PlayerID = playerID;
            InitUnits();
        }

        public void InitUnits()
        {
            Entities = new List<Entity>();
            UnitFactory normalUnits = new UnitFactory(EntityType.TIGER, 0.5f,2f,2f,100,10);
            UnitFactory smallFastUnits = new UnitFactory(EntityType.TIGER, 0.25f, 3f, 3f,50,0);
            UnitFactory bigUnits = new UnitFactory(EntityType.BAOBAB, 1f, 2f, 4f,150,0);
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Entities.Add(normalUnits.NewInstance(PlayerID, new Vector2(20 + i*.25f,10+ j*.25f)));
                }
            }
            Entities.Add(bigUnits.NewInstance(PlayerID, new Vector2(5f, 6f)));
            Entities.Add(new Unit(PlayerID, EntityType.TIGER, 10, 10, new Vector2(5f, 6f)));
            Entities.Add(new Unit(PlayerID, EntityType.TIGER, 10, 10, new Vector2(7f, 6f)));
            Entities.Add(new Unit(PlayerID, EntityType.TIGER, 10, 10, new Vector2(6.5f, 6f)));
            Entities.Add(new Unit(PlayerID, EntityType.TIGER, 10, 10, new Vector2(4f, 9f)));
        }

        public void UpdateVisibilityMap(ObstacleMap obstMap)
        {
            VisibilityMap.FindVisibility(Entities.Select((unit) => unit.UnitView).ToList(), obstMap);
        }

        /// <summary>
        /// Removes dead units and references to them from their commands. The references
        /// from other commands stay - other commands need to check for the death of the unit.
        /// </summary>
        public void RemoveDeadUnits()
        {
            List<Entity> toBeRemoved = new List<Entity>();
            foreach(Entity u in Entities)
            {
                if(u.IsDead)
                {
                    toBeRemoved.Add(u);
                    //CommandsAssignments still have reference to the unit
                    u.RemoveFromAllCommandsAssignments();
                }
            }
            Entities.RemoveAll((unit) => toBeRemoved.Contains(unit));
        }

        public void UpdateMap(Map map)
        {
            //todo: implement with visibility map
            MapView = map;
        }
    }

    public enum Players
    {
        PLAYER0,
        PLAYER1
    }
}
