using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;

namespace SanguineGenesis
{
    /// <summary>
    /// Represents game state.
    /// </summary>
    public class Game
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
        public Players? Winner { get; set; }
        /// <summary>
        /// Dictionary of all players.
        /// </summary>
        public Dictionary<Players,Player> Players { get; }
        /// <summary>
        /// Used for extracting information about the game.
        /// </summary>
        public GameQuerying GameQuerying { get; }
        /// <summary>
        /// Player controlled by the user.
        /// </summary>
        public Player CurrentPlayer { get; private set; }
        /// <summary>
        /// Used for handling collisions.
        /// </summary>
        public Physics physics;
        /// <summary>
        /// Generates visibility maps for players.
        /// </summary>
        VisibilityGenerator visibilityGenerator;
        /// <summary>
        /// The next player to whom will be generated visibility map.
        /// </summary>
        private Players nextVisibilityPlayer;
        /// <summary>
        /// Describes customizable parts of the game.
        /// </summary>
        public GameplayOptions GameplayOptions { get; }

        public Game()
        {
            GameEnded = false;
            Winner = null;

            //players
            Players = new Dictionary<Players, Player>();
            Players.Add(SanguineGenesis.Players.PLAYER0, new Player(SanguineGenesis.Players.PLAYER0));
            Players.Add(SanguineGenesis.Players.PLAYER1, new Player(SanguineGenesis.Players.PLAYER1));
            CurrentPlayer = Players[SanguineGenesis.Players.PLAYER0];

             //map
            var mapLoader = new MapLoader();
            Map = mapLoader.LoadMap("Images\\NewMap\\nutrients.png", "Images\\NewMap\\biomes.png",
                 "Images\\NewMap\\terrain.png", this);
            mapLoader.LoadBuildings(this, "Images\\NewMap\\buildings.png");

            foreach(var kvp in Players)
                kvp.Value.InitializeMapView(Map);

            GameQuerying = GameQuerying.GetGameQuerying();
            physics = Physics.GetPhysics();
            visibilityGenerator = new VisibilityGenerator();
            nextVisibilityPlayer = SanguineGenesis.Players.PLAYER0;
            GameplayOptions = new GameplayOptions();
            GameplayOptions.WholeMapVisible = false;
            foreach (Animal u in CurrentPlayer.GetAll<Unit>())
            {
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.Attack);
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.Jump);
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.Pull);
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.ActivateFarSight);
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.KnockBack);
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.ConsumeAnimal);
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.ClimbTree);
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.ChargeTo);
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.HerbivoreEat);
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.BuildBuilding("KAPOC"));
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.BuildBuilding("BAOBAB"));
            }
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
            return Ts;
        }

        /// <summary>
        /// Update the state of the game.
        /// </summary>
        public void Update(float deltaT)
        {
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

            //update parts of map that can be seen for each player
            foreach (var p in Players)
                p.Value.UpdateVisibleMap(Map);

            //one step of statuses
            foreach (Entity e in entities)
            {
                e.StepStatuses(this, deltaT);
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

            //physics
            List<Entity> physicalEntities = entities.Where((e) => e.Physical).ToList();
            physics.MoveAnimals(Map, animals, deltaT);
            physics.PushAway(Map, animals, physicalEntities);
            physics.PushOutsideOfObstacles(Map, animals);

            //attack nearby enemy if idle
            foreach(Animal a in animals)
            {
                if(!a.CommandQueue.Any())
                {
                    //unit isn't doing anything
                    Entity en = units.Where((v) => v.Player!=a.Player && a.DistanceTo(v) < a.AttackDistance).FirstOrDefault();
                    if(en!=null)
                        a.CommandQueue.Enqueue(CurrentPlayer.GameStaticData.Abilities.Attack.NewCommand(a, en));
                }
            }

            //remove dead units
            foreach(var kvp in Players)
                kvp.Value.RemoveDeadEntities();

            //update players' visibility map
            if (!GameplayOptions.WholeMapVisible)
            {
                VisibilityGeneratorInteraction(buildings);
            }
            else
            {
                foreach(var kvp in Players)
                {
                    VisibilityMap everythingVisible = VisibilityMap.GetEverythingVisible(Map.Width, Map.Height);
                    kvp.Value.SetVisibilityMap(everythingVisible, buildings);
                }
            }

            //set and refresh movement commands
            MovementGeneratorInteraction();

            //test if someone won the game
            if (Winner == null)
            {
                if (!Players[SanguineGenesis.Players.PLAYER0].GetAll<Tree>().Any())
                    Winner = SanguineGenesis.Players.PLAYER1;
                else if (!Players[SanguineGenesis.Players.PLAYER1].GetAll<Tree>().Any())
                    Winner = SanguineGenesis.Players.PLAYER0;
            }
        }

        /// <summary>
        /// Sets tasks to visibilityGenerator and updates visibility maps.
        /// </summary>
        private void VisibilityGeneratorInteraction(List<Building> allBuildings)
        {
            if (visibilityGenerator.Done)
            {
                Players current = nextVisibilityPlayer;
                Players other = nextVisibilityPlayer == SanguineGenesis.Players.PLAYER0 ?
                                        SanguineGenesis.Players.PLAYER1 :
                                        SanguineGenesis.Players.PLAYER0;

                //update current player's visibility map
                Players[current].SetVisibilityMap(visibilityGenerator.VisibilityMap, allBuildings);

                //generated visibility map for the other player
                nextVisibilityPlayer = other;

                visibilityGenerator.SetNewTask(Map.GetViewObstaclesMap(nextVisibilityPlayer),
                    Players[nextVisibilityPlayer].Entities.Select((entity) => entity.View).ToList());
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
