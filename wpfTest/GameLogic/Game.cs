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
using wpfTest.GameLogic;
using wpfTest.GameLogic.Maps;

namespace wpfTest
{
    public class Game
    {
        public Map Map { get; }
        public FlowMap FlowMap { get; set; }
        public bool GameEnded { get; set; }
        public Dictionary<Players,Player> Players { get; }
        public GameQuerying GameQuerying { get; }
        public Player CurrentPlayer { get; private set; }
        Physics physics;
        VisibilityGenerator visibilityGenerator;
        /// <summary>
        /// The next player for who will be generated visibility map.
        /// </summary>
        private Players nextVisibilityPlayer;
        private GameplayOptions gameplayOptions;

        public Game(BitmapImage mapBitmap)
        {
            PixelColor[,] mapPC = mapBitmap.GetPixels();
            Map=new Map(mapPC);
            FlowMap = new FlowMap(Map.Width, Map.Height);
            FlowMap = PushingMapGenerator.GeneratePushingMap(Map.GetObstacleMap(Movement.LAND));
            GameEnded = false;
            Players = new Dictionary<Players, Player>();
            Players.Add(wpfTest.Players.PLAYER0, new Player(wpfTest.Players.PLAYER0));
            Players.Add(wpfTest.Players.PLAYER1, new Player(wpfTest.Players.PLAYER1));
            CurrentPlayer = Players[0];
            Players[wpfTest.Players.PLAYER0].InitializeMapView(Map);
            Players[wpfTest.Players.PLAYER1].InitializeMapView(Map);
            GameQuerying = GameQuerying.GetGameQuerying();
            physics = Physics.GetPhysics();
            visibilityGenerator = new VisibilityGenerator();
            nextVisibilityPlayer = wpfTest.Players.PLAYER0;
            gameplayOptions = new GameplayOptions();
            gameplayOptions.WholeMapVisible = true;
            foreach (Animal u in CurrentPlayer.Units)
            {
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.Attack);
                u.Abilities.Add(CurrentPlayer.GameStaticData.Abilities.PlantBuilding("KAPOC"));
            }
        }

        public List<Entity> GetEntities()
        {
            List<Entity> units=new List<Entity>();
            foreach(Players player in Enum.GetValues(typeof(Players)))
            {
                units=units.Concat(Players[player].Entities.Where((u) => !u.IsDead).ToList()).ToList();
            }
            return units;
        }

        public List<Animal> GetUnits()
        {
            var units = new List<Animal>();
            foreach (Players player in Enum.GetValues(typeof(Players)))
            {
                units = units.Concat(Players[player].Units.Where((u) => !u.IsDead).ToList()).ToList();
            }
            return units;
        }

        public List<Building> GetBuildings()
        {
            var buildings = new List<Building>();
            foreach (Players player in Enum.GetValues(typeof(Players)))
            {
                buildings = buildings.Concat(Players[player].Buildings.Where((b) => !b.IsDead).ToList()).ToList();
            }
            return buildings;
        }

        private const float NUTRIENT_UPDATE_TIME = 1f;
        private float nutrientUpdateTimer = NUTRIENT_UPDATE_TIME;

        public void Update(float deltaT)
        {
            //map changing phase

            /*if (Map.MapWasChanged)
            {
                Map.UpdateObstacleMaps();
                MovementGenerator.GetMovementGenerator().SetMapChanged(wpfTest.Players.PLAYER0, Map.ObstacleMaps);
            }*/

            List<Entity> entities = GetEntities();
            List<Animal> units = GetUnits();
            List<Building> buildings = GetBuildings();

            //update nutrients
            nutrientUpdateTimer -= deltaT;
            if (nutrientUpdateTimer <= 0)
            {
                nutrientUpdateTimer = NUTRIENT_UPDATE_TIME;
                Map.UpdateNutrients();

                foreach (Building b in buildings)
                {
                    b.DrainEnergy(/*deltaT*/1);
                }
            }


            //nutrients biomes and terrain can't be updated in this step after calling this method
            Map.UpdateBiomes();
            foreach (var p in Players)
                p.Value.UpdateNodesView(Map);

            //commands
            foreach (Entity e in entities)
            {
                e.PerformCommand(this, deltaT);
                e.AnimationStep(deltaT);
            }
            //physics
            physics.PushOutsideOfObstacles(Map, units,deltaT);
            physics.PushAway(Map, units, entities, deltaT);
            physics.Step(Map,units,deltaT);
            physics.ResetCollision(units);

            //attack nearby enemy if idle
            foreach(Animal u in units)
            {
                if(!u.CommandQueue.Any())
                {
                    //unit isn't doing anything
                    Entity en = units.Where((v) => v.Player!=u.Player && u.DistanceTo(v) < u.AttackDistance).FirstOrDefault();
                    if(en!=null)
                        u.CommandQueue.Enqueue(CurrentPlayer.GameStaticData.Abilities.Attack.NewCommand(u, en));
                }

            }

            //remove dead units
            Players[wpfTest.Players.PLAYER0].RemoveDeadEntities();
            Players[wpfTest.Players.PLAYER1].RemoveDeadEntities();

            //update players' view of the map
            if (!gameplayOptions.WholeMapVisible)
            {
                if (visibilityGenerator.Done)
                {
                    Players current = nextVisibilityPlayer;
                    Players other = nextVisibilityPlayer == wpfTest.Players.PLAYER0 ?
                                            wpfTest.Players.PLAYER1 :
                                            wpfTest.Players.PLAYER0;
                    //update current player's view of the map
                    Players[current].VisibilityMap = visibilityGenerator.VisibilityMap;
                    Players[current].UpdateBuildingsView(GetBuildings());

                    //generated visibility map for the other player
                    nextVisibilityPlayer = other;

                    visibilityGenerator.SetNewTask(Map.GetViewMap(nextVisibilityPlayer),
                        Players[nextVisibilityPlayer].Entities.Select((entity) => entity.View).ToList());
                }
            }
            else
            {
                foreach(var kvp in Players)
                {
                    kvp.Value.VisibilityMap = VisibilityMap.GetEverythingVisible(Map.Width, Map.Height);
                    kvp.Value.UpdateBuildingsView(GetBuildings());
                }
            }
            MovementGenerator mg = MovementGenerator.GetMovementGenerator();
            if (Players[wpfTest.Players.PLAYER0].MapChanged)
            {
                Players[wpfTest.Players.PLAYER0].MapView.UpdateObstacleMaps();
                mg.SetMapChanged(wpfTest.Players.PLAYER0, Players[wpfTest.Players.PLAYER0].MapView.ObstacleMaps);
                Players[wpfTest.Players.PLAYER0].MapView.MapWasChanged = false;

            }
            if (Players[wpfTest.Players.PLAYER1].MapChanged)
            {
                Players[wpfTest.Players.PLAYER1].MapView.UpdateObstacleMaps();
                mg.SetMapChanged(wpfTest.Players.PLAYER1, Players[wpfTest.Players.PLAYER1].MapView.ObstacleMaps);
                Players[wpfTest.Players.PLAYER1].MapView.MapWasChanged = false;
            }

            //update move to commands
            mg.UseProcessedCommands();

        }
    }
}
