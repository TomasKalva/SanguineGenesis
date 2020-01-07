using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.AI;
using SanguineGenesis.GameLogic.Data;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GameLogic.Maps.VisibilityGenerating;

namespace SanguineGenesis.GameLogic
{
    class Faction
    {
        /// <summary>
        /// Maximum value of MaxAirTaken.
        /// </summary>
        public const int MAX_AIR_TAKEN = 100;
        /// <summary>
        /// Entities owned by the faction.
        /// </summary>
        public List<Entity> Entities { get; private set; }
        /// <summary>
        /// Type of faction.
        /// </summary>
        public FactionType FactionID { get; }
        /// <summary>
        /// Entities factories, abilities and statuses used by the player.
        /// </summary>
        public GameData GameStaticData { get; }
        /// <summary>
        /// Maximum amount of air that can be taken by animals. If reached or exceeded, 
        /// no new animals can be created.
        /// </summary>
        public int MaxAirTaken { get; private set; }
        /// <summary>
        /// Air required by the animals.
        /// </summary>
        public int AirTaken { get; private set; }

        public Faction(FactionType factionID)
        {
            FactionID = factionID;
            Entities = new List<Entity>();
            GameStaticData = new GameData();
        }

        /// <summary>
        /// Returns all entities of type T owned by the faction.
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
        /// Removes dead entities and references to them from their commands. The references
        /// from other commands stay - other commands need to check for dead entities.
        /// </summary>
        public void RemoveDeadEntities(Game game)
        {
            //remove player's dead entities
            List<Entity> deadEntities = new List<Entity>();
            foreach (Entity e in Entities)
            {
                if (e.IsDead)
                {
                    deadEntities.Add(e);
                }
            }
            foreach (Entity e in deadEntities)
                e.Die(game);

            Entities.RemoveAll((entity) => entity.IsDead);
        }

        /// <summary>
        /// Returns true iff this faction can see entity.
        /// </summary>
        public virtual bool CanSee(Entity entity)
        {
            return true;
        }
    }

    class Player : Faction
    {
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
        /// Buildings visible by the player. Some of them might no longer
        /// exist.
        /// </summary>
        public List<Building> VisibleBuildings { get; }
        /// <summary>
        /// Biome this player is playing.
        /// </summary>
        public Biome Biome { get; }
        /// <summary>
        /// Aritificial intelligence that controls this player.
        /// </summary>
        public IAI Ai { get; }

        public Player(FactionType factionID, Biome biome, IAIFactory aiFactory)
            : base(factionID)
        {
            //SpawnTestingAnimals();
            VisibleBuildings = new List<Building>();
            Biome = biome;
            if(aiFactory!=null)
                Ai = aiFactory.NewInstance(this);
        }

        /// <summary>
        /// Spawns 3 instances of each animal in the game.
        /// </summary>
        public void SpawnTestingAnimals()
        {
            //calculate extents
            var factories = GameStaticData.AnimalFactories.Factorys;
            int gridPoints = (int)Math.Ceiling(Math.Sqrt(factories.Count));
            float gridSize = 29f;
            float step = gridSize / (gridPoints + 1);
            
            //create animals
            AnimalFactory normalUnits = new AnimalFactory("TIGER", 200, 150, 0.3f, 0.5f, 0.4f, 5f, 0.5f, 0.1f, false, 3f, 2f, Movement.LAND, false, Diet.CARNIVORE, 5f, true, 20f, 5f, new List<StatusFactory>(), 1);
            int i = 0;
            foreach (AnimalFactory a in factories.Select(kvp => kvp.Value))
            {
                for (int j = 0; j < 3; j++)
                {
                    var animal = a.NewInstance(this, new Vector2(step * ((i % gridPoints) + 1),
                                                                 step * ((i / gridPoints) + 1)));
                    animal.Energy = animal.Energy.MaxValue;
                    Entities.Add(animal);
                }
                i++;
            }
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
                if (CanSee(b)
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
            VisibleBuildings.RemoveAll((b) => CanSee(b) &&
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
            for (int i = 0; i < map.Width; i++)
                for (int j = 0; j < map.Height; j++)
                {
                    //update node if it can be seen
                    if (VisibilityMap[i, j])
                    {
                        Node destN = VisibleMap[i, j];
                        Node sourceN = map[i, j];
                        destN.Biome = sourceN.Biome;
                        destN.ActiveNutrients = sourceN.ActiveNutrients;
                        destN.PassiveNutrients = sourceN.PassiveNutrients;
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
            //add vision under this player's buildings
            foreach (Building b in GetAll<Building>())
                foreach (Node n in b.Nodes)
                    visMap[n.X, n.Y] = true;

            VisibilityMap = visMap;
            UpdateBuildingsView(buildings);
        }

        /// <summary>
        /// Removes all visible buildings that are dead.
        /// </summary>
        public void RemoveDeadVisibleBuildings()
        {
            VisibleBuildings.ForEach((building) =>
            {
                if (building.IsDead) VisibleMap.RemoveBuilding(building);
            });
            VisibleBuildings.RemoveAll((building) => building.IsDead);
        }

        /// <summary>
        /// Copy map to the player's VisibleMap.
        /// </summary>
        public void InitializeMapView(Map map)
        {
            VisibleMap = new Map(map);
        }

        /// <summary>
        /// Returns the factory of main building of this players biome.
        /// </summary>
        public BuildingFactory GetMainBuildingFactory()
        {
            switch (Biome)
            {
                case Biome.SAVANNA: return GameStaticData.TreeFactories["BAOBAB"];
                case Biome.RAINFOREST: return GameStaticData.TreeFactories["KAPOC"];
                default: return GameStaticData.StructureFactories["ROCK"];
            }
        }

        /// <summary>
        /// Returns true iff this player can see entity. It is assumed that entity lies on the map,
        /// otherwise OutOfRangeException is thrown.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">When entity doesn't lie on the map (shouldn't happen
        /// because of constraints on entity.</exception>
        public override bool CanSee(Entity entity)
        {
            if (VisibilityMap != null)
            {
                if (entity is Unit)
                {
                    int x = (int)entity.Center.X;
                    int y = (int)entity.Center.Y;
                    return VisibilityMap[x, y];
                }
                else //entity is Building
                {
                    foreach(Node n in (entity as Building).Nodes)
                    {
                        int x = (int)n.X;
                        int y = (int)n.Y;
                        if (VisibilityMap[x, y])
                            return true;
                    }
                    return false;
                }
            }
            else
                //visibility map is null => player can't see anything
                return false;
        }
    }

    /// <summary>
    /// Use for indexing players.
    /// </summary>
    public enum FactionType
    {
        PLAYER0,
        PLAYER1,
        NEUTRAL
    }

    public static class FactionTypeExtensions
    {
        public static FactionType Opposite(this FactionType ft)
        {
            switch (ft)
            {
                case FactionType.PLAYER0: return FactionType.PLAYER1;
                case FactionType.PLAYER1: return FactionType.PLAYER0;
                default: return FactionType.NEUTRAL;
            }
        }
    }
}
