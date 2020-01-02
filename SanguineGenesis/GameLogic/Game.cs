using System.Collections.Generic;
using System.Linq;
using SanguineGenesis.GameLogic.AI;
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
        /// True if the game is over.
        /// </summary>
        public bool GameEnded { get; set; }
        /// <summary>
        /// The player who won this game.
        /// </summary>
        public FactionType? Winner { get; set; }
        /// <summary>
        /// Stores entities owned by none of the players.
        /// </summary>
        public Faction NeutralFaction { get; }
        /// <summary>
        /// Dictionary of all players.
        /// </summary>
        public Dictionary<FactionType,Player> Players { get; }
        /// <summary>
        /// Player controlled by the user.
        /// </summary>
        public Player CurrentPlayer { get; private set; }
        /// <summary>
        /// Used for handling collisions.
        /// </summary>
        public Physics physics;
        /// <summary>
        /// The next player to whom will be generated visibility map.
        /// </summary>
        private FactionType nextVisibilityPlayer;
        /// <summary>
        /// Describes customizable parts of the game.
        /// </summary>
        public GameplayOptions GameplayOptions { get; }

        public Game(MapDescription mapDescription, Biome firstPlayersBiome)
        {
            GameEnded = false;
            Winner = null;

            //factions
            Players = new Dictionary<FactionType, Player>
            {
                { FactionType.PLAYER0, new Player(FactionType.PLAYER0, firstPlayersBiome, null) },
                { FactionType.PLAYER1, new Player(FactionType.PLAYER1, firstPlayersBiome == Biome.SAVANNA ? Biome.RAINFOREST : Biome.SAVANNA, new DumbAIFactory()) }
            };
            CurrentPlayer = Players[FactionType.PLAYER0];
            NeutralFaction = new Faction(FactionType.NEUTRAL);

             //map
            var mapLoader = new MapLoader(mapDescription);
            Map = mapLoader.LoadMap();
            mapLoader.LoadBuildings(this);

            foreach(var kvp in Players)
                kvp.Value.InitializeMapView(Map);

            physics = Physics.GetPhysics();
            MovementGenerator.GetMovementGenerator().Reset();
            nextVisibilityPlayer = FactionType.PLAYER0;
            GameplayOptions = new GameplayOptions();
        }

        /// <summary>
        /// Spawns 3 instances of each animal in the game for current player.
        /// </summary>
        public void SpawnTestingAnimals()
        {
            CurrentPlayer.SpawnTestingAnimals();
        }

        /// <summary>
        /// Returns all entities of the type T that are in the game.
        /// </summary>
        public List<T> GetAll<T>() where T:Entity
        {
            var Ts = new List<T>();
            foreach (var kvpPlayer in Players)
            {
                Ts = Ts.Concat(kvpPlayer.Value.GetAll<T>()).ToList();
            }

            Ts = Ts.Concat(NeutralFaction.GetAll<T>()).ToList();
            return Ts;
        }

        /// <summary>
        /// Update the state of the game.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float deltaT = gameTime.DeltaT;

            //ai
            foreach (var p in Players.Values)
                if (p.Ai != null)
                    p.Ai.Play(deltaT, this);

            //map changing phase
            List<Entity> entities = GetAll<Entity>();
            List<Unit> units = GetAll<Unit>();
            List<Animal> animals = GetAll<Animal>();
            List<Building> buildings = GetAll<Building>();
            List<Tree> trees = GetAll<Tree>();


            //update air values
            foreach (var kvp in Players)
                kvp.Value.CalulateAir();

            //generate and drain nutrients by trees
            Map.UpdateNutrientsMap(trees, deltaT);

            gameTime.PrintTime("Others");

            //update players' visibility map
            VisibilityGeneratorInteraction(buildings);

            //update parts of map that can be seen for each player
            foreach (var p in Players)
                p.Value.UpdateVisibleMap(Map);

            gameTime.PrintTime("Visibility");

            //one step of statuses
            foreach (Entity e in entities)
            {
                e.StepStatuses(this, deltaT);
            }
            
            // add suffocating status to animals on wrong terrain
            foreach(Animal a in animals)
            {
                int x = (int)a.Position.X;
                int y = (int)a.Position.Y;
                var n = Map[x, y];
                if (n!=null && !a.CanMoveOn(n.Terrain))
                {
                    NeutralFaction.GameStaticData.Statuses.SuffocatingFactory.ApplyToAffected(a);
                }
            }

            //one step of commands
            foreach (Entity e in entities)
            {
                e.PerformCommand(this, deltaT);
            }

            //one step of animations
            foreach (Entity e in entities)
            {
                e.AnimationStep(deltaT);
            }

            gameTime.PrintTime("Ingame update");

            //physics
            List<Animal> physicalAnimals = animals.Where((a) => a.Physical).ToList();
            physics.MoveAnimals(Map, animals, deltaT);
            physics.PushAway(Map, physicalAnimals);
            physics.PushOutsideOfObstacles(Map, animals);

            gameTime.PrintTime("Physics");

            //attack nearby enemy if idle
            foreach (Animal a in animals)
            {
                if(!a.CommandQueue.Any())
                {
                    //animal isn't doing anything
                    var opposite = a.Faction.FactionID.Opposite();
                    Entity en = units.Where((v) => v.Faction.FactionID==opposite && a.DistanceTo(v) < a.AttackDistance && a.Faction.CanSee(v)).FirstOrDefault();
                    if(en!=null)
                        a.CommandQueue.Enqueue(CurrentPlayer.GameStaticData.Abilities.Attack.NewCommand(a, en));
                }
            }

            gameTime.PrintTime("Finding target");

            //remove dead units
            foreach (var kvp in Players)
                kvp.Value.RemoveDeadEntities(this);
            NeutralFaction.RemoveDeadEntities(this);
            foreach (var kvp in Players)
                kvp.Value.RemoveDeadVisibleBuildings();

            //set and refresh movement commands
            MovementGeneratorInteraction();

            //test if someone won the game
            if (Winner == null)
            {
                if (!Players[FactionType.PLAYER0].GetAll<Tree>().Any())
                    Winner = FactionType.PLAYER1;
                else if (!Players[FactionType.PLAYER1].GetAll<Tree>().Any())
                    Winner = FactionType.PLAYER0;
            }

            gameTime.PrintTime("Others2");
        }

        /// <summary>
        /// Sets tasks to visibilityGenerator and updates visibility maps.
        /// </summary>
        private void VisibilityGeneratorInteraction(List<Building> allBuildings)
        {
            var visibilityGenerator = VisibilityGenerator.Get;
            if (visibilityGenerator.Done)
            {
                FactionType current = nextVisibilityPlayer;
                FactionType other = nextVisibilityPlayer == FactionType.PLAYER0 ?
                                        FactionType.PLAYER1 :
                                        FactionType.PLAYER0;

                //update current player's visibility map
                var newMap = visibilityGenerator.VisibilityMap;
                if (newMap != null)
                    Players[current].SetVisibilityMap(newMap, allBuildings);

                //generated visibility map for the other player
                nextVisibilityPlayer = other;

                //create algorithm for generating visibility map
                IVisibilityGeneratingTask visGenTask;
                if (GameplayOptions.WholeMapVisible)
                    visGenTask = new UnlimitedVisibilityGeneratingTask(Map.Width, Map.Height);
                else
                    visGenTask = new RayVisibilityGeneratingTask(Map.GetViewObstaclesMap(nextVisibilityPlayer),
                    Players[nextVisibilityPlayer].Entities.Select((entity) => entity.View).ToList());

                visibilityGenerator.SetNewTask(visGenTask);
            }
        }

        /// <summary>
        /// Enable computation of flow maps if map was changed, and update MoveTo commands
        /// with computed flow maps.
        /// </summary>
        private void MovementGeneratorInteraction()
        {
            MovementGenerator mg = MovementGenerator.GetMovementGenerator();
            foreach (var kvp in Players)
            {
                Player p = kvp.Value;
                //if map changed, set new obstacle maps to mg and make it re-generate all
                //movement maps
                if (p.MapChanged)
                {
                    p.VisibleMap.UpdateObstacleMaps();
                    mg.SetMapChanged(kvp.Key, p.VisibleMap.ObstacleMaps);
                    p.VisibleMap.MapWasChanged = false;
                }
            }

            //update move to commands
            mg.UseProcessedCommands();
        }
    }
}
