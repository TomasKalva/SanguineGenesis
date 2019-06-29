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
    class Game
    {
        public Map Map { get; private set; }

        public Game(String mapName, MainWindow window)
        {
            BitmapImage mapBI = (BitmapImage)window.FindResource(mapName);
            PixelColor[,] mapPC = mapBI.GetPixels();
            Map=new Map(mapPC,window);
            wantedFps = 70;
            GameEnded = false;
            gameWindow = window;
        }

        private MainWindow gameWindow;

        public bool GameEnded { get; set; }
        private int wantedFps;
        private int StepLength => 1000 / wantedFps;

        private Stopwatch stepStopwatch = new Stopwatch();
        private double totalStepTime;

        public void MainLoop()
        {
            int i = 0;
            while (true)
            {
                if (GameEnded)
                    break;

                stepStopwatch.Start();

                //logic
                i++;

                //draw
                gameWindow.Dispatcher.Invoke(() =>
                {
                    gameWindow.Draw();
                });

                stepStopwatch.Stop();

                //calculate sleep time
                double diff = stepStopwatch.Elapsed.TotalMilliseconds;
                stepStopwatch.Reset();
                totalStepTime += diff;
                int sleepTime = StepLength - (int)totalStepTime;
                if ((int)totalStepTime > 0)
                    totalStepTime = totalStepTime - (int)totalStepTime;
                //if(sleepTime<0)
                //    Console.WriteLine(sleepTime);
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
                else
                {
                    Thread.Yield();
                }
            }
        }
    }
}
