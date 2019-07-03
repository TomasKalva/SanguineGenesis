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
        Physics Physics;

        public Game(BitmapImage mapBitmap)
        {
            PixelColor[,] mapPC = mapBitmap.GetPixels();
            Map=new Map(mapPC);
            FlowMap = new FlowMap(Map.Width, Map.Height);
            FlowMap = PushingMapGenerator.GeneratePushingMap(Map.GetObstacleMap());
            GameEnded = false;
            Players = new Player[2];
            Players[0] = new Player();
            Players[1] = new Player();
            Physics = Physics.GetPhysics();
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

        public void Update()
        {
            List<Unit> units = GetUnits();
            Physics.PushOutsideOfObstacles(Map, units);
            Physics.Repulse(Map,units);
            Physics.Step(Map,units);
        }
    }
}
