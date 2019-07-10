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

namespace wpfTest
{
    public class Game
    {
        public Map Map { get; }
        public FlowMap FlowMap { get; }
        public bool GameEnded { get; set; }
        Player[] Players { get; }
        public GameQuerying GameQuerying { get; }
        Physics physics;

        public Game(BitmapImage mapBitmap)
        {
            PixelColor[,] mapPC = mapBitmap.GetPixels();
            Map=new Map(mapPC);
            FlowMap = new FlowMap(Map.Width, Map.Height);
            FlowMap = PushingMapGenerator.GeneratePushingMap(Map.GetObstacleMap());
            GameEnded = false;
            Players = new Player[2];
            Players[0] = new Player(Map.Width,Map.Height);
            Players[1] = new Player(Map.Width,Map.Height);
            GameQuerying = GameQuerying.GetGameQuerying();
            physics = Physics.GetPhysics();
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
                u.PerformCommand();
            physics.PushOutsideOfObstacles(Map, units,deltaT);
            physics.Repulse(Map,units,deltaT);
            physics.Step(Map,units,deltaT);
            physics.ResetCollision(units);


            //players view of map
            if(++deletable%1000==0)
                foreach (Player pl in Players)
                    pl.UpdateVisibilityMap(Map.GetObstacleMap());
        }

        int deletable=0;
    }
}
