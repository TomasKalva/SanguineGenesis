using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest;
using wpfTest.GameLogic.Maps;

namespace DebuggingUnitTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Init();
            Ray_Next_BottomLeft();
        }

        static ObstacleMap noObstMap = new ObstacleMap(5, 5);
        
        public static void Init()
        {
            noObstMap = new ObstacleMap(5, 5);
        }

        public static void Ray_Next_BottomLeft()
        {
            Ray r = new Ray(new Vector2(2.5f, 3.5f),
                            new Vector2(0.5f, 0.5f),
                            noObstMap);

            int[] expectedX = { 2, 1, 1, 0, 0 };
            int[] expectedY = { 2, 2, 1, 1, 0 };

            TestEquality(r, expectedX, expectedY);
        }

        private static void TestEquality(Ray r, int[] expectedX, int[] expectedY)
        {
            int i = 0;
            while (r.Next(out int x, out int y))
            {
                Console.WriteLine("Expected coordinates: " + expectedX[i] + " ;  " + expectedY[i]);
                Console.WriteLine("Actual coordinates:   " + x + " ;  " + y);
                i++;
            }
        }
    }
}
