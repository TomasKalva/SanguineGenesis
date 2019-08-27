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
        /// <summary>
        /// Maximum value of MaxAirTaken.
        /// </summary>
        public const int MAX_AIR_TAKEN = 100;
        /// <summary>
        /// Entities owned by the player.
        /// </summary>
        public List<Entity> Entities { get; private set; }
        /// <summary>
        /// Describes the area of map view by the player.
        /// </summary>
        public VisibilityMap VisibilityMap { get; private set; }
        /// <summary>
        /// Set to true after VisibilityMap was changed.
        /// </summary>
        public bool MapChanged => VisibleMap.MapWasChanged;
        /// <summary>
        /// Map seen by the player.
        /// </summary>
        public Map VisibleMap { get; private set; }
        /// <summary>
        /// Index of player.
        /// </summary>
        public Players PlayerID { get; }
        /// <summary>
        /// Buildings visible by the player. Some of them might no longer
        /// exist.
        /// </summary>
        public List<Building> VisibleBuildings { get; }
        /// <summary>
        /// Entities factories, abilities and statuses used by the player.
        /// </summary>
        public GameStaticData GameStaticData { get; }
        /// <summary>
        /// Maximum amount of air that can be taken by animals. If reached or exceeded, 
        /// no new animals can be created.
        /// </summary>
        public int MaxAirTaken { get; private set; }
        /// <summary>
        /// Air required by the animals.
        /// </summary>
        public int AirTaken { get; private set; }

        public Player(Players playerID)
        {
            PlayerID = playerID;
            InitUnits();
            VisibleBuildings = new List<Building>();
            GameStaticData = new GameStaticData();
        }

        //todo: remove this things
        public void InitUnits()
        {

            Entities = new List<Entity>();

            if (PlayerID == Players.PLAYER1)
                return;

            AnimalFactory normalUnits = new AnimalFactory("TIGER" , 200, 150, 0.3m, 0.5f, 0.4f, 5m, 0.5f, 0.1f, false, 3f, 2f, Movement.LAND, false, Diet.CARNIVORE, 5f, true, 20m, 5f, new List<StatusFactory>(), 1);
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

        /// <summary>
        /// Returns all entities of type T owned by the player.
        /// </summary>
        public List<T> GetAll<T>() where T : Entity
        {
            if (typeof(T) == typeof(Entity))
                return Entities.Cast<T>().ToList();
            else if (typeof(T) == typeof(Unit))
                return Entities.Where((e) => e is T).Cast<T>().ToList();
            else if (typeof(T) == typeof(Animal))
                return Entities.Where((e) => e is T).Cast<T>().ToList();
            else if (typeof(T) == typeof(Corpse))
                return Entities.Where((e) => e is T).Cast<T>().ToList();
            else if (typeof(T) == typeof(Building))
                return Entities.Where((e) => e is T).Cast<T>().ToList();
            else if (typeof(T) == typeof(Tree))
                return Entities.Where((e) => e is T).Cast<T>().ToList();
            else if (typeof(T) == typeof(Structure))
                return Entities.Where((e) => e is T).Cast<T>().ToList();

            throw new NotImplementedException("The case when entity is " + typeof(T) + " is not covered!");
        }

        /// <summary>
        /// Sets correct values of AirTaken and MaxAirTaken.
        /// </summary>
        public void CalulateAir()
        {
            MaxAirTaken = Math.Min(MAX_AIR_TAKEN, GetAll<Tree>().Sum((t) => t.Air));
            AirTaken = GetAll<Animal>().Sum((a) => a.Air);
        }

        /// <summary>
        /// Add buildings that weren't visible and now are to VisibleBuildings. Remove 
        /// buildings that were visible but now can be seen that they no longer exist.
        /// </summary>
        /// <param name="buildings"></param>
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
                    && VisibleMap[bottomLeft.X, bottomLeft.Y].Building != b)
                {
                    //remove buildings that no longer exist
                    foreach (Node n in b.Nodes)
                    {
                        Building deprecB = VisibleMap[n.X, n.Y].Building;
                        if (deprecB != null)
                            RemoveVisibleBuilding(deprecB);
                    }
                    //add the newly visible building
                    AddVisibleBuilding(b);
                }
            }

            //remove visible buildings for which it can be seen that they no longer exist
            VisibleBuildings.RemoveAll((b) => b.IsVisible(VisibilityMap) &&
                                                !buildings.Contains(b));
        }

        /// <summary>
        /// Update visible nodes.
        /// </summary>
        public void UpdateVisibleMap(Map map)
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
                        Node destN=VisibleMap[i, j];
                        Node sourceN = map[i, j];
                        destN.Biome = sourceN.Biome;
                        destN.Nutrients = sourceN.Nutrients;
                        destN.Terrain = sourceN.Terrain;
                    }
                }
        }

        /// <summary>
        /// Removes building from VisibleBuildings and updates VisibleMap.
        /// </summary>
        private void RemoveVisibleBuilding(Building building)
        {
            VisibleBuildings.Remove(building);
            VisibleMap.RemoveBuilding(building);
        }

        /// <summary>
        /// Adds building to VisibleBuildings and updates VisibleMap.
        /// </summary>
        private void AddVisibleBuilding(Building building)
        {
            VisibleBuildings.Add(building);
            VisibleMap.AddBuilding(building);
        }

        /// <summary>
        /// Sets VisibilityMap and updates visible buildings.
        /// </summary>
        public void SetVisibilityMap(VisibilityMap visMap, List<Building> buildings)
        {
            VisibilityMap = visMap;
            UpdateBuildingsView(buildings);
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
                if (building.IsDead) VisibleMap.RemoveBuilding(building);
            });
            VisibleBuildings.RemoveAll((building) => building.IsDead);
        }

        /// <summary>
        /// Copy map to the player's VisibleMap.
        /// </summary>
        /// <param name="map"></param>
        public void InitializeMapView(Map map)
        {
            //todo: implement with visibility map
            VisibleMap = new Map(map);
        }
    }

    /// <summary>
    /// Use for indexing players.
    /// </summary>
    public enum Players
    {
        PLAYER0,
        PLAYER1
    }
}
