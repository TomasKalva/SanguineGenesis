using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SanguineGenesis.GameLogic.AI;
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
        public Collisions collisions;
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

            collisions = Collisions.GetCollisions();
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

            //ai
            foreach (var p in Players.Values)
                if (p.Ai != null)
                    p.Ai.Play(deltaT, this);

            //map changing phase

            //update air values
            foreach (var kvp in Players)
                kvp.Value.CalulateAir();

            //generate and drain nutrients by trees
            Map.UpdateNutrientsMap(GetAll<Tree>(), deltaT);

            gameTime.PrintTime("Others");

            //update players' visibility map
            VisibilityGeneratorInteraction(GetAll<Building>());

            //update parts of map that can be seen for each player
            foreach (var p in Players)
                p.Value.UpdateVisibleMap(Map);

            gameTime.PrintTime("Visibility");

            //one step of statuses
            //statuses can't modify any collection of entities of any player
            foreach (Entity e in GetAll<Entity>())
            {
                e.StepStatuses(this, deltaT);
            }
            
            // add suffocating status to animals on wrong terrain
            foreach(Animal a in GetAll<Animal>())
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
            foreach (Entity e in GetAll<Entity>()
                .ToList())//new list has to be constructed because the original collections can change
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

            gameTime.PrintTime("Ingame update");

            //collisions
            collisions.MoveAnimals(Map, GetAll<Animal>(), deltaT);
            collisions.PushAway(this);
            collisions.PushOutsideOfObstacles(Map, GetAll<Animal>());

            gameTime.PrintTime("Collisions");

            //attack nearby enemy if idle
            foreach (Animal a in GetAll<Animal>())
            {
                if(!a.CommandQueue.Any())
                {
                    //animal isn't doing anything
                    var opposite = a.Faction.FactionID.Opposite();
                    Entity en = GetAll<Animal>().Where((v) => v.Faction.FactionID==opposite && a.DistanceTo(v) < a.AttackDistance && a.Faction.CanSee(v)).FirstOrDefault();
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
                if (!Players[FactionType.PLAYER0].GetAll<Building>().Any())
                    Winner = FactionType.PLAYER1;
                else if (!Players[FactionType.PLAYER1].GetAll<Building>().Any())
                    Winner = FactionType.PLAYER0;
            }

            gameTime.PrintTime("Others2");
        }

        /// <summary>
        /// Sets tasks to visibilityGenerator and updates visibility maps.
        /// </summary>
        private void VisibilityGeneratorInteraction(IEnumerable<Building> allBuildings)
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
                    visGenTask = new RayVisibilityGeneratingTask(Map.GetViewObstaclesMap(),
                    Players[nextVisibilityPlayer].GetAll<Entity>().Select((entity) => entity.View).ToList());

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

    /// <summary>
    /// Logs last few errors by user. Used in ability-command pipeline. 
    /// </summary>
    class ActionLog
    {
        public static ActionLog ThrowAway = new ActionLog(1);

        public int Size { get; }
        private readonly Message[] messages;

        /// <summary>
        /// Logged message.
        /// </summary>
        class Message
        {
            public Entity Entity { get; }
            public Ability Ability { get; }
            public string Text { get; }
            public Message(Entity entity, Ability ability, string text)
            {
                Entity = entity;
                Ability = ability;
                Text = text;
            }

            public override string ToString()
            {
                return $"{(Entity != null ? (Entity.EntityType + ", ") : "")}{(Ability != null ? (Ability.GetName() + ": ") : "")}{Text}";
            }
        }

        public ActionLog(int size)
        {
            this.Size = size >= 1 ? size : 1;
            messages = new Message[this.Size];
        }

        /// <summary>
        /// Adds message to the log and moves other messages by one. Can
        /// erase the last message in the log.
        /// </summary>
        private void PushMessage(Message message)
        {
            //move messages by one
            for(int i = Size - 2; i >= 0; i--)
            {
                messages[i + 1] = messages[i];
            }
            messages[0] = message;
        }

        /// <summary>
        /// Log a new message. entity and ability should correspond to the message.
        /// If no corresponding entity or ability exist, use null instead.
        /// </summary>
        public void LogError(Entity entity, Ability ability, string message)
        {
            PushMessage(new Message(entity, ability, message));
        }

        /// <summary>
        /// Returns error messages from oldest to newest separated by \n.
        /// </summary>
        /// <returns></returns>
        public string GetMessages()
        {
            StringBuilder messagesText = new StringBuilder();
            for (int i = Size - 1; i >= 0; i--)
            {
                messagesText.Append(messages[i]);
                messagesText.Append(i!=0?"\n":"");
            }
            return messagesText.ToString();
        }
    }
}
