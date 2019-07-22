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

        public Game(BitmapImage mapBitmap)
        {
            PixelColor[,] mapPC = mapBitmap.GetPixels();
            Map=new Map(mapPC);
            FlowMap = new FlowMap(Map.Width, Map.Height);
            FlowMap = PushingMapGenerator.GeneratePushingMap(Map.GetObstacleMap(Movement.GROUND));
            GameEnded = false;
            Players = new Dictionary<Players, Player>();
            Players.Add(wpfTest.Players.PLAYER0, new Player(wpfTest.Players.PLAYER0));
            Players.Add(wpfTest.Players.PLAYER1, new Player(wpfTest.Players.PLAYER1));
            CurrentPlayer = Players[0];
            GameQuerying = GameQuerying.GetGameQuerying();
            physics = Physics.GetPhysics();
            visibilityGenerator = new VisibilityGenerator();
        }

        public List<Unit> GetUnits()
        {
            List<Unit> units=new List<Unit>();
            foreach(Players player in Enum.GetValues(typeof(Players)))
            {
                units=units.Concat(Players[player].Units).ToList();
            }
            return units;
        }

        public void Update(float deltaT)
        {
            //map changing phase

            if (Map.MapWasChanged)
            {
                Map.UpdateObstacleMaps();
            }

            List<Unit> units = GetUnits();
            //commands
            foreach (Unit u in units)
            {
                u.PerformCommand(this, deltaT);
                u.AnimationStep(deltaT);
            }
            //physics
            physics.PushOutsideOfObstacles(Map, units,deltaT);
            physics.PushAway(Map, units, deltaT);
            physics.Step(Map,units,deltaT);
            physics.ResetCollision(units);

            //attack nearby enemy if idle
            foreach(Unit u in units)
            {
                if(!u.CommandQueue.Any())
                {
                    //unit isn't doing anything
                    Unit en = units.Where((v) => v.Owner!=u.Owner && Map.Distance(u, v) < u.AttackDistance).FirstOrDefault();
                    if(en!=null)
                        u.CommandQueue.Enqueue(new AttackCommand(u, en));
                }

            }

            //remove dead units
            Players[wpfTest.Players.PLAYER0].RemoveDeadUnits();
            Players[wpfTest.Players.PLAYER1].RemoveDeadUnits();

            //update players' view of the map
            if (visibilityGenerator.Done)
            {
                Players[0].VisibilityMap = visibilityGenerator.VisibilityMap;

                visibilityGenerator.SetNewTask(Map.GetViewMap(),
                    Players[0].Units.Select((unit) => unit.UnitView).ToList());
            }
            Players[wpfTest.Players.PLAYER0].UpdateMap(Map);
            Players[wpfTest.Players.PLAYER0].UpdateMap(Map);
            MovementGenerator mg = MovementGenerator.GetMovementGenerator();
            if (Players[wpfTest.Players.PLAYER0].MapChanged)
            {
                mg.SetMapChanged(wpfTest.Players.PLAYER0, Map.ObstacleMaps);
                
            }
            if (Players[wpfTest.Players.PLAYER1].MapChanged)
            {
                mg.SetMapChanged(wpfTest.Players.PLAYER1, Map.ObstacleMaps);

            }

            //update move to commands
            mg.UseProcessedCommands();

        }
    }
}
