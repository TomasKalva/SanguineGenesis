using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Data.Entities;
using wpfTest.GameLogic.Maps;

namespace wpfTest
{
    public class Player
    {
        public const int MAX_AIR_TAKEN = 100;

        public List<Entity> Entities { get; private set; }
        public List<Unit> Units => Entities.Where((e) => e is Unit).Select((u)=>(Unit)u).ToList();
        public List<Animal> Animals => Entities.Where((e) => e is Animal).Select((u) => (Animal)u).ToList();
        public List<Building> Buildings => Entities.Where((e) => e is Building).Select((u) => (Building)u).ToList();
        public List<Tree> Trees => Entities.Where((e) => e is Tree).Select((u) => (Tree)u).ToList();
        public VisibilityMap VisibilityMap { get; set; }
        public bool MapChanged => MapView.MapWasChanged;
        public Map MapView { get; private set; }
        public Players PlayerID { get; }
        public List<Building> VisibleBuildings { get; }
        public GameStaticData GameStaticData { get; }
        public int MaxAirTaken { get; private set; }
        public int AirTaken { get; private set; }

        public Player(Players playerID)
        {
            PlayerID = playerID;
            InitUnits();
            VisibleBuildings = new List<Building>();
            GameStaticData = new GameStaticData();
        }

        public void InitUnits()
        {

            Entities = new List<Entity>();

            if (PlayerID == Players.PLAYER1)
                return;

            AnimalFactory normalUnits = new AnimalFactory("TIGER" , 200, 150, 0.3m, 0.5f, 0.4f, 5m, 0.5f, 0.1f, false, 4f, 2f, Movement.LAND, false, Diet.CARNIVORE, 5f, true, 20m, 5f, new List<StatusFactory>(), 1);
                //new UnitFactory(string.TIGER, 0.5f,2f,2f,100,10,Movement.LAND,4f);
            /*UnitFactory smallFastUnits = new UnitFactory(string.TIGER, 0.25f, 3f, 3f,50,0,Movement.WATER,4f);
            UnitFactory bigUnits = new UnitFactory(string.BAOBAB, 1f, 2f, 4f,150,0,Movement.LAND_WATER,4f);*/
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    Entities.Add(normalUnits.NewInstance(this, new Vector2(20 + i*.25f,10+ j*.25f)));
                }
            }
            /*Entities.Add(bigUnits.NewInstance(this, new Vector2(5f, 6f)));
            Entities.Add(new Unit(this, string.TIGER, 10, 10, new Vector2(5f, 6f)));
            Entities.Add(new Unit(this, string.TIGER, 10, 10, new Vector2(7f, 6f)));
            Entities.Add(new Unit(this, string.TIGER, 10, 10, new Vector2(6.5f, 6f)));
            Entities.Add(new Unit(this, string.TIGER, 10, 10, new Vector2(4f, 9f)));*/
        }

        public void CalulateAir()
        {
            MaxAirTaken = Math.Min(MAX_AIR_TAKEN, Trees.Sum((t) => t.Air));
            AirTaken = Animals.Sum((a) => a.Air);
        }

        public void UpdateBuildingsView(List<Building> buildings)
        {
            if (VisibilityMap == null)
                return;

            //update view of buildings
            foreach (Building b in buildings)
            {
                Node bottomLeft = b.Nodes[0, 0];
                //check if the building is visible and if it wasn't added to the view map yet
                if (b.IsVisible(VisibilityMap)
                    && MapView[bottomLeft.X, bottomLeft.Y].Building != b)
                {
                    //remove buildings that no longer exist
                    foreach (Node n in b.Nodes)
                    {
                        Building deprecB = MapView[n.X, n.Y].Building;
                        if (deprecB != null)
                            RemoveBuilding(deprecB);
                    }
                    //add the newly visible building
                    AddBuilding(b);
                }
            }
        }

        public void UpdateNodesView(Map map)
        {
            if (VisibilityMap == null)
                return;

            //update view of nodes
            for (int i=0;i<map.Width;i++)
                for(int j = 0; j < map.Height; j++)
                {
                    //update node if it can be seen
                    if (VisibilityMap[i, j])
                    {
                        Node destN=MapView[i, j];
                        Node sourceN = map[i, j];
                        destN.Biome = sourceN.Biome;
                        destN.Nutrients = sourceN.Nutrients;
                        destN.Terrain = sourceN.Terrain;
                    }
                }
        }

        private void RemoveBuilding(Building building)
        {
            VisibleBuildings.Remove(building);
            MapView.RemoveBuilding(building);
        }

        private void AddBuilding(Building building)
        {
            VisibleBuildings.Add(building);
            MapView.AddBuilding(building);
        }

        /// <summary>
        /// Removes dead entities and references to them from their commands. The references
        /// from other commands stay - other commands need to check for dead entities.
        /// </summary>
        public void RemoveDeadEntities()
        {
            //remove player's dead entities
            List<Entity> deadEntities = new List<Entity>();
            foreach(Entity e in Entities)
            {
                if(e.IsDead)
                {
                    deadEntities.Add(e);
                    /*if(e is Building b)
                    {
                        
                    }*/
                    //CommandsAssignments still have reference to the entity
                    //e.RemoveFromAllCommandsAssignments();
                }
            }
            foreach(Entity e in deadEntities)
                e.Die();

            Entities.RemoveAll((entity) => entity.IsDead);
            
            //remove dead visible buildings
            RemoveDeadVisibleBuildings();
        }

        /// <summary>
        /// Removes all visible buildings that are dead.
        /// </summary>
        public void RemoveDeadVisibleBuildings()
        {
            VisibleBuildings.ForEach((building)=> 
            {
                if (building.IsDead) MapView.RemoveBuilding(building);
            });
            VisibleBuildings.RemoveAll((building) => building.IsDead);
        }

        public void InitializeMapView(Map map)
        {
            //todo: implement with visibility map
            MapView = new Map(map);
        }

        /*public ObstacleMap GetViewMap()
        {
            ObstacleMap om = new ObstacleMap(Width, Height);
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    om[i, j] = nodes[i, j].Blocked;
            return om;
        }*/
    }

    public enum Players
    {
        PLAYER0,
        PLAYER1
    }
}
