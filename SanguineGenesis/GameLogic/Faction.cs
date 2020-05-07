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
        /// Animals owned by the faction.
        /// </summary>
        public List<Animal> Animals { get; private set; }
        /// <summary>
        /// Plants owned by the faction.
        /// </summary>
        public List<Plant> Plants { get; private set; }
        /// <summary>
        /// Structures owned by the faction.
        /// </summary>
        public List<Structure> Structures { get; private set; }
        /// <summary>
        /// Corpses owned by the faction.
        /// </summary>
        public List<Corpse> Corpses { get; private set; }
        /// <summary>
        /// Type of faction.
        /// </summary>
        public FactionType FactionID { get; }
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
            Animals = new List<Animal>();
            Plants = new List<Plant>();
            Structures = new List<Structure>();
            Corpses = new List<Corpse>();
            CalulateAir();
        }

        /// <summary>
        /// Adds the entity to the list of its real type.
        /// </summary>
        public void AddEntity(Entity e)
        {
            if (e.GetType() == typeof(Animal))
                Animals.Add((Animal)e);
            if (e.GetType() == typeof(Plant))
                Plants.Add((Plant)e);
            if (e.GetType() == typeof(Structure))
                Structures.Add((Structure)e);
            if (e.GetType() == typeof(Corpse))
                Corpses.Add((Corpse)e);
            CalulateAir();
        }

        /// <summary>
        /// Removes the entity from the list of its real type.
        /// </summary>
        public void RemoveEntity(Entity e)
        {
            if (e.GetType() == typeof(Animal))
                Animals.Remove((Animal)e);
            if (e.GetType() == typeof(Plant))
                Plants.Remove((Plant)e);
            if (e.GetType() == typeof(Structure))
                Structures.Remove((Structure)e);
            if (e.GetType() == typeof(Corpse))
                Corpses.Remove((Corpse)e);
            CalulateAir();
        }

        /// <summary>
        /// Returns all entities of type T owned by the faction.
        /// </summary>
        public IEnumerable<T> GetAll<T>() where T : Entity
        {
            if (typeof(T) == typeof(Entity))
                return Animals.Cast<T>().Concat(
                        Plants.Cast<T>().Concat(
                         Structures.Cast<T>().Concat(
                          Corpses.Cast<T>())));
            else if (typeof(T) == typeof(Unit))
                return Animals.Cast<T>().Concat(
                          Corpses.Cast<T>());
            else if (typeof(T) == typeof(Animal))
                return Animals.Cast<T>();
            else if (typeof(T) == typeof(Corpse))
                return Corpses.Cast<T>();
            else if (typeof(T) == typeof(Building))
                return Plants.Cast<T>().Concat(
                         Structures.Cast<T>());
            else if (typeof(T) == typeof(Plant))
                return Plants.Cast<T>();
            else if (typeof(T) == typeof(Structure))
                return Structures.Cast<T>();

            throw new NotImplementedException("The case when entity is " + typeof(T) + " is not covered!");
        }

        /// <summary>
        /// Sets correct values of AirTaken and MaxAirTaken.
        /// </summary>
        public void CalulateAir()
        {
            MaxAirTaken = Math.Min(MAX_AIR_TAKEN, GetAll<Plant>().Sum((t) => t.Air));
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
            foreach (Entity e in GetAll<Entity>()
                .ToList())//new entities can be added to the list
            {
                if (e.IsDead)
                {
                    e.Die(game);
                }
            }

            Animals.RemoveAll((a) => a.IsDead);
            Plants.RemoveAll((t) => t.IsDead);
            Structures.RemoveAll((s) => s.IsDead);
            Corpses.RemoveAll((c) => c.IsDead);
        }

        /// <summary>
        /// Returns true iff this faction can see entity.
        /// </summary>
        public virtual bool CanSee(Entity entity)
        {
            return true;
        }

        /// <summary>
        /// Spawns 6 instances of each animal in the game.
        /// </summary>
        public void SpawnTestingAnimals(GameData gameData)
        {
            //calculate extents
            var factories = gameData.AnimalFactories.Factorys;
            int gridPoints = (int)Math.Ceiling(Math.Sqrt(factories.Count));
            float gridSize = 29f;
            float step = gridSize / (gridPoints + 1);

            //create animals
            int pos = 0;
            foreach (AnimalFactory a in factories.Select(kvp => kvp.Value))
            {
                for (int j = 0; j < 6; j++)
                {
                    var animal = a.NewInstance(this, new Vector2(step * ((pos % gridPoints) + 1),
                                                                 step * ((pos / gridPoints) + 1)));
                    animal.Energy = animal.Energy.MaxValue;
                    AddEntity(animal);
                }
                pos++;
            }
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
        /// Copy map to the player's VisibleMap.
        /// </summary>
        public void InitializeMapView(Map map)
        {
            VisibleMap = new Map(map);
        }

        /// <summary>
        /// Sets VisibilityMap and updates visible buildings.
        /// </summary>
        public void SetVisibilityMap(VisibilityMap visMap)
        {
            //add vision under this player's buildings
            foreach (Building b in GetAll<Building>())
                foreach (Node n in b.Nodes)
                    visMap[n.X, n.Y] = true;

            VisibilityMap = visMap;
        }

        /// <summary>
        /// Add buildings that weren't visible and now are to VisibleBuildings. Remove 
        /// buildings that were visible but now can be seen that they no longer exist.
        /// </summary>
        public void UpdateVisibleBuildings(IEnumerable<Building> buildings)
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
            foreach(var b in VisibleBuildings) 
            {
                if ( CanSee(b) && b.IsDead) 
                    VisibleMap.RemoveBuilding(b);
            }
            VisibleBuildings.RemoveAll((b) => CanSee(b) && b.IsDead);
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
        /// Removes building from VisibleBuildings and updates VisibleMap.
        /// </summary>
        private void RemoveVisibleBuilding(Building building)
        {
            VisibleBuildings.Remove(building);
            VisibleMap.RemoveBuilding(building);
        }

        /// <summary>
        /// Update visible nodes.
        /// </summary>
        public void UpdateVisibleNodes(Map map)
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
                    }
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

        /// <summary>
        /// Returns the factory of main building of this players biome.
        /// </summary>
        public BuildingFactory GetMainBuildingFactory(Game game)
        {
            switch (Biome)
            {
                case Biome.SAVANNA: return game.GameData.PlantFactories["BAOBAB"];
                case Biome.RAINFOREST: return game.GameData.PlantFactories["KAPOC"];
                default: return game.GameData.StructureFactories["ROCK"];
            }
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
