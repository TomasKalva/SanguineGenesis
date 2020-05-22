using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SanguineGenesis.GameLogic.AI;
using SanguineGenesis.GameLogic.Data;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GameLogic.Maps.MovementGenerating;
using SanguineGenesis.GameLogic.Maps.VisibilityGenerating;
using static SanguineGenesis.GUI.MainMenuWindow;

namespace SanguineGenesis.GameLogic
{
    /// <summary>
    /// Represents game state.
    /// </summary>
    class Game
    {
        /// <summary>
        /// The main map of the game.
        /// </summary>
        public Map Map { get; }
        /// <summary>
        /// Entities factories, abilities and statuses used by the player.
        /// </summary>
        public GameData GameData { get; }
        /// <summary>
        /// Stores entities owned by none of the players.
        /// </summary>
        public Faction NeutralFaction { get; }
        /// <summary>
        /// Dictionary of all players.
        /// </summary>
        public Dictionary<FactionType, Player> Players { get; }

        /// <summary>
        /// The player who won this game.
        /// </summary>
        public FactionType? Winner { get; set; }
        /// <summary>
        /// Player controlled by the user.
        /// </summary>
        public Player CurrentPlayer { get; private set; }
        /// <summary>
        /// Describes customizable parts of the game.
        /// </summary>
        public GameplayOptions GameplayOptions { get; }

        /// <summary>
        /// Used for handling collisions.
        /// </summary>
        public Collisions Collisions { get; }
        /// <summary>
        /// True if the first visibility map was taken from visibility generator this game.
        /// </summary>
        private bool firstVisibilityTaken;
        /// <summary>
        /// The next player to whom will be generated visibility map.
        /// </summary>
        private FactionType nextVisibilityPlayer;

        public Game(MapDescription mapDescription, Biome firstPlayersBiome, GameData gameData, GameplayOptions gameplayOptions)
        {
            Winner = null;
            GameData = gameData;
            GameplayOptions = gameplayOptions;

            //factions
            Players = new Dictionary<FactionType, Player>
            {
                { FactionType.PLAYER0, new Player(FactionType.PLAYER0, firstPlayersBiome, null) },
                { FactionType.PLAYER1, new Player(FactionType.PLAYER1, firstPlayersBiome == Biome.SAVANNA ? Biome.RAINFOREST : Biome.SAVANNA, new DefaultAIFactory()) }
            };
            CurrentPlayer = Players[FactionType.PLAYER0];
            NeutralFaction = new Faction(FactionType.NEUTRAL);

            //map
            var mapLoader = new MapLoader(mapDescription);
            Map = mapLoader.LoadMap();//Map has to be assigned before calling LoadBuildings, because it uses game.Map
            mapLoader.LoadBuildings(this);

            //collisions
            Collisions = new Collisions(Map);
            Collisions.SetPushingMaps(Map);

            //visibility
            foreach (var kvp in Players)
                kvp.Value.InitializeMapView(Map);
            nextVisibilityPlayer = FactionType.PLAYER0;
            firstVisibilityTaken = false;

            //movement generator
            MovementGenerator.GetMovementGenerator.Reset();
        }

        /// <summary>
        /// Spawns 3 instances of each animal in the game for current player.
        /// </summary>
        public void SpawnTestingAnimals()
        {
            CurrentPlayer.SpawnTestingAnimals(this.GameData);
        }

        /// <summary>
        /// Returns all entities of the type T that are in the game.
        /// </summary>
        public IEnumerable<T> GetAll<T>() where T:Entity
        {
            var Ts = NeutralFaction.GetAll<T>();
            foreach (var kvpPlayer in Players)
            {
                Ts = Ts.Concat(kvpPlayer.Value.GetAll<T>());
            }

            return Ts;
        }

        /// <summary>
        /// Update the state of the game.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float deltaT = gameTime.DeltaT;

            //AI
            foreach (var p in Players.Values)
                if (p.AI != null)
                    p.AI.Play(deltaT, this);

            gameTime.PrintTime("AI");

            //update players' visibility map
            VisibilityUpdate();

            gameTime.PrintTime("Visibility");

            //one step of statuses
            //new list has to be constructed because the original collections can change
            var entities = GetAll<Entity>().ToList();
            foreach (Entity e in entities)
            {
                e.StepStatuses(this, deltaT);
            }

            // add suffocating status to animals on wrong terrain
            var suffocFact = GameData.Statuses.SuffocatingFactory;
            foreach (Animal a in GetAll<Animal>())
            {
                int x = (int)a.Position.X;
                int y = (int)a.Position.Y;
                var n = Map[x, y];
                if (n!=null && !a.CanMoveOn(n.Terrain))
                {
                    suffocFact.ApplyToAffected(a);
                }
            }

            //one step of commands
            //the original collections can change
            foreach (Entity e in entities)
            {
                //remove commands of animals who are on wrong terrain
                if(e is Animal a)
                    if (!a.CanMoveOn(Map[(int)a.Position.X, (int)a.Position.Y].Terrain))
                        a.CommandQueue.Clear();

                //one step of command
                e.PerformCommand(this, deltaT);
            }

            //one step of animations
            foreach (Entity e in GetAll<Entity>())
            {
                e.AnimationStep(deltaT);
            }

            //move animal
            foreach (Animal a in GetAll<Animal>())
            {
                a.Move(Map, deltaT);
            }

            //remove dead entities
            foreach (var kvp in Players)
                kvp.Value.RemoveDeadEntities(this);
            NeutralFaction.RemoveDeadEntities(this);

            gameTime.PrintTime("Entities update");

            //attack nearby enemy if idle
            foreach (Animal a in GetAll<Animal>())
            {
                if(!a.CommandQueue.Any())
                {
                    //animal isn't doing anything
                    var opposite = a.Faction.FactionID.Opposite();
                    //find enemy in attack range
                    Entity en = Players[opposite].GetAll<Animal>().Where((v) => a.DistanceTo(v) < a.AttackDistance && a.Faction.CanSee(v)).FirstOrDefault();
                    if(en!=null)
                        a.CommandQueue.Enqueue(GameData.Abilities.Attack.NewCommand(a, en));
                }
            }

            gameTime.PrintTime("Finding target");

            //collisions
            Collisions.ResolveCollisions(this);
            Collisions.PushAllOutOfObstacles(GetAll<Animal>());

            gameTime.PrintTime("Collisions");

            //set and refresh movement commands
            MovementGeneratorInteraction();

            gameTime.PrintTime("Movement");

            //generate and drain nutrients by plants
            Map.UpdateNutrientsMap(GetAll<Plant>(), deltaT);

            //test if someone won the game
            if (Winner == null)
            {
                if (!Players[FactionType.PLAYER0].GetAll<Building>().Any())
                    Winner = FactionType.PLAYER1;
                else if (!Players[FactionType.PLAYER1].GetAll<Building>().Any())
                    Winner = FactionType.PLAYER0;
            }

            gameTime.PrintTime("Nutrients");
        }

        /// <summary>
        /// Sets tasks to visibilityGenerator and updates visibility maps.
        /// </summary>
        private void VisibilityUpdate()
        {
            var visibilityGenerator = VisibilityGenerator.Get;
            if (visibilityGenerator.Done)
            {
                FactionType current = nextVisibilityPlayer;
                FactionType other = nextVisibilityPlayer.Opposite();

                //update current player's visibility map
                var newMap = visibilityGenerator.VisibilityMap;
                if (newMap != null && firstVisibilityTaken)
                    Players[current].SetVisibilityMap(newMap);

                //visibility map was taken
                if (!firstVisibilityTaken)
                    firstVisibilityTaken = true;

                //generate visibility map for the other player
                nextVisibilityPlayer = other;

                //create algorithm for generating visibility map
                IVisibilityGeneratingTask visGenTask;
                if (GameplayOptions.WholeMapVisible)
                    visGenTask = new UnlimitedVisibilityGeneratingTask(Map.Width, Map.Height);
                else
                {
                    //update visibility obstacle map
                    Map.UpdateVisibilityObstacleMap();
                    visGenTask = new RayVisibilityGeneratingTask(Map.VisibilityObstacles,
                        Players[nextVisibilityPlayer].GetAll<Entity>().Select((entity) => entity.View).ToList());
                }

                visibilityGenerator.SetNewTask(visGenTask);
            }

            //update parts of map that can be seen for each player
            foreach (var p in Players)
            {
                p.Value.UpdateVisibleNodes(Map);
                p.Value.UpdateVisibleBuildings(GetAll<Building>());
            }
        }

        /// <summary>
        /// Enable computation of flow maps if map was changed, and update MoveTo commands
        /// with computed flow maps.
        /// </summary>
        private void MovementGeneratorInteraction()
        {
            MovementGenerator mg = MovementGenerator.GetMovementGenerator;
            lock (mg)
            {
                foreach (var kvp in Players)
                {
                    Player p = kvp.Value;
                    //if map changed, set new obstacle maps to mg and make it regenerate all
                    //movement maps
                    if (p.MapChanged)
                    {
                        //update visible obstacle maps
                        p.VisibleMap.UpdateObstacleMaps();
                        //tell movement generator that obstacle maps changed
                        mg.SetNewObstMaps(kvp.Key, p.VisibleMap.ObstacleMaps);
                        p.VisibleMap.MapWasChanged = false;
                    }
                }

                //update move to commands
                mg.UseProcessedCommands();
            }
        }
    }
}
