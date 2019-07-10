using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using wpfTest;
using wpfTest.GameLogic.Maps;

namespace GameLogicTests
{
    [TestClass]
    public class RayTests
    {
        /// <summary>
        /// Tests if the ray outputs the expected coordinates.
        /// </summary>
        private void TestEquality(Ray r, int[] expectedX, int[] expectedY)
        {
            int i = 0;
            while (r.Next(out int x, out int y))
            {
                Assert.AreEqual(expectedX[i], x);
                Assert.AreEqual(expectedY[i], y);
                i++;
            }
            Assert.AreEqual(expectedX.Length, i);
        }

        /// <summary>
        /// _____
        /// __e__
        /// _xx__
        /// xx___
        /// s____
        /// </summary>
        [TestMethod]
        public void Ray_Next_NoObst_TopRight()
        {
            ObstacleMap noObstMap = new ObstacleMap(5, 5);
            Ray r = new Ray(new Vector2(0.5f, 0.5f),
                            new Vector2(2.5f, 3.5f),
                            noObstMap);

            int[] expectedX = { 0, 1, 1, 2, 2 };
            int[] expectedY = { 1, 1, 2, 2, 3 };
            
            TestEquality(r, expectedX, expectedY);
        }

        /// <summary>
        /// __xx_
        /// __x__
        /// _xx__
        /// xx___
        /// s____
        /// </summary>
        [TestMethod]
        public void Ray_Next_NoObst_TopRightOutsideGrid()
        {
            ObstacleMap noObstMap = new ObstacleMap(5, 5);
            Ray r = new Ray(new Vector2(0.5f, 0.5f),
                            new Vector2(4.5f, 6.5f),
                            noObstMap);

            int[] expectedX = { 0, 1, 1, 2, 2, 2, 3 };
            int[] expectedY = { 1, 1, 2, 2, 3, 4, 4 };

            TestEquality(r, expectedX, expectedY);
        }

        /// <summary>
        /// _____
        /// __s__
        /// _xx__
        /// xx___
        /// e____
        /// </summary>
        [TestMethod]
        public void Ray_Next_NoObst_BottomLeft()
        {
            ObstacleMap noObstMap = new ObstacleMap(5, 5);
            Ray r = new Ray(new Vector2(2.5f, 3.5f),
                            new Vector2(0.5f, 0.5f),
                            noObstMap);
            
            int[] expectedX = { 2, 1, 1, 0, 0 };
            int[] expectedY = { 2, 2, 1, 1, 0 };

            TestEquality(r, expectedX, expectedY);
        }

        /// <summary>
        /// _____
        /// s____
        /// xx___
        /// _xx__
        /// __e__
        /// </summary>
        [TestMethod]
        public void Ray_Next_NoObst_BottomRight()
        {
            ObstacleMap noObstMap = new ObstacleMap(5, 5);
            Ray r = new Ray(new Vector2(0.5f, 3.5f),
                            new Vector2(2.5f, 0.5f),
                            noObstMap);

            int[] expectedX = { 0, 1, 1, 2, 2 };
            int[] expectedY = { 2, 2, 1, 1, 0 };

            TestEquality(r, expectedX, expectedY);
        }

        /// <summary>
        /// _____
        /// e____
        /// xx___
        /// _xx__
        /// __s__
        /// </summary>
        [TestMethod]
        public void Ray_Next_NoObst_TopLeft()
        {
            ObstacleMap noObstMap = new ObstacleMap(5, 5);
            Ray r = new Ray(new Vector2(2.5f, 0.5f),
                            new Vector2(0.5f, 3.5f),
                            noObstMap);
            
            int[] expectedX = { 2, 1, 1, 0, 0 };
            int[] expectedY = { 1, 1, 2, 2, 3 };

            TestEquality(r, expectedX, expectedY);
        }
        
        private ObstacleMap CreateObstacleMap()
        {
            ObstacleMap obstMap = new ObstacleMap(5, 5);
            //map is transposed
            bool[,] obstacles=new bool[,]
            {
                {false, false, true, false, false },
                {false, false, true, false, false },
                {false, false, true, false, false },
                {false, false, true, false, false },
                {false, false, true, false, false },
            };
            for (int i = 0; i < obstMap.Width; i++)
                for (int j = 0; j < obstMap.Height; j++)
                    obstMap[i, j] = obstacles[i, j];

            return obstMap;
        }

        /// <summary>
        /// _____
        /// __e__
        /// _xx__
        /// xx___
        /// s____
        /// </summary>
        [TestMethod]
        public void Ray_Next_Obst_TopRight()
        {
            ObstacleMap obstMap = CreateObstacleMap();
            Ray r = new Ray(new Vector2(0.5f, 0.5f),
                            new Vector2(2.5f, 3.5f),
                            obstMap);

            int[] expectedX = { 0, 1 };
            int[] expectedY = { 1, 1 };

            TestEquality(r, expectedX, expectedY);
        }
    }
}
