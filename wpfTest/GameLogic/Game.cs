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
        Player[] Players { get; }
        public GameQuerying GameQuerying { get; }
        public Player CurrentPlayer { get; private set; }
        Physics physics;
        VisibilityGenerator visibilityGenerator;

        public Game(BitmapImage mapBitmap)
        {
            PixelColor[,] mapPC = mapBitmap.GetPixels();
            Map=new Map(mapPC);
            FlowMap = new FlowMap(Map.Width, Map.Height);
            FlowMap = PushingMapGenerator.GeneratePushingMap(Map.GetObstacleMap());
            GameEnded = false;
            Players = new Player[2];
            Players[0] = new Player(wpfTest.Players.PLAYER0);
            Players[1] = new Player(wpfTest.Players.PLAYER1);
            CurrentPlayer = Players[0];
            GameQuerying = GameQuerying.GetGameQuerying();
            physics = Physics.GetPhysics();
            visibilityGenerator = new VisibilityGenerator();
        }

        public List<Unit> GetUnits()
        {
            List<Unit> units=new List<Unit>();
            foreach(Player player in Players)
            {
                units=units.Concat(player.Units).ToList();
            }
            return units;
        }

        public void Update(float deltaT)
        {
            //physics
            List<Unit> units = GetUnits();
            foreach (Unit u in units)
            {
                u.PerformCommand();
                u.AnimationStep(deltaT);
            }
            physics.PushOutsideOfObstacles(Map, units,deltaT);
            physics.Repulse(Map,units,deltaT);
            physics.Step(Map,units,deltaT);
            physics.ResetCollision(units);



            //players view of map
            if (visibilityGenerator.Done)
            {
                Players[0].VisibilityMap = visibilityGenerator.VisibilityMap;

                visibilityGenerator.SetNewTask(Map.GetObstacleMap(),
                    Players[0].Units.Select((unit) => unit.UnitView).ToList());
            }
        }
    }
}
