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
            PixelColor[,] mapPC = GetPixels(mapBI);
            Map=new Map(mapPC);
            wantedFps = 60;
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

                //calculate sleep timex
                double diff = stepStopwatch.Elapsed.TotalMilliseconds;
                stepStopwatch.Reset();
                totalStepTime += diff;
                int sleepTime = StepLength - (int)totalStepTime;
                if ((int)totalStepTime > 0)
                    totalStepTime = totalStepTime - (int)totalStepTime;
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


        private PixelColor[,] GetPixels(BitmapSource source)
        {
            int offset = 0;
            var height = source.PixelHeight;
            var width = source.PixelWidth;
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, width * 4, 0);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            PixelColor[,] pixels = new PixelColor[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixels[x + x0, y + y0] = new PixelColor
                    {
                        Blue = pixelBytes[(y * width + x) * 4 + 0],
                        Green = pixelBytes[(y * width + x) * 4 + 1],
                        Red = pixelBytes[(y * width + x) * 4 + 2],
                        Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    };
            return pixels;
        }
    }
   
    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor
    {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }
}
