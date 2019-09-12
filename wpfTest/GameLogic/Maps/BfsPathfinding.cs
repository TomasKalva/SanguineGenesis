using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Maps
{
    /// <summary>
    /// Used for generating flowmap using bfs with raycasting heuristics. One instance of the class
    /// is one instance of the algorithm.
    /// </summary>
    class BfsPathfinding : IPathfinding
    {
        /// <summary>
        /// The result of the algorithm.
        /// </summary>
        private FlowMap flowMap;

        /// <summary>
        /// Contains all discovered coordinates that weren't processed yet.
        /// </summary>
        private Queue<Coords> discovered;
        /// <summary>
        /// Obstacles that need to be avoided.
        /// </summary>
        private ObstacleMap obstacleMap;
        /// <summary>
        /// Ending point of the flowmap.
        /// </summary>
        private Vector2 targetLocation;

        public BfsPathfinding(ObstacleMap obst, Vector2 targetLocation)
        {
            int width = obst.Width;
            int height = obst.Height;
            flowMap = new FlowMap(width, height);
            obstacleMap = obst;
            this.targetLocation = targetLocation;
            discovered = new Queue<Coords>();
        }

        public FlowMap GenerateFlowMap()
        {
            int width = obstacleMap.Width;
            int height = obstacleMap.Height;

            //find paths using bfs
            discovered = new Queue<Coords>();
            Coords target = new Coords((int)targetLocation.X,
                                        (int)targetLocation.Y);
            if(target.Valid(width, height))
            {
                discovered.Enqueue(target);
            }
            else
            {
                //return empty flowmap if target location is invalid
                return flowMap;
            }

            while (discovered.Any())
            {
                Coords coords = discovered.Dequeue();
                int x = coords.X;
                int y = coords.Y;

                //dont' generate path that goes into blocked node
                if (obstacleMap[x, y])
                    continue;

                float _90degrees = (float)Math.PI / 2;
                Relax(new Coords(x, y - 1), _90degrees);
                Relax(new Coords(x + 1, y), _90degrees * 2);
                Relax(new Coords(x, y + 1), _90degrees * 3);
                Relax(new Coords(x - 1, y), _90degrees * 4);
            }

            //find straight path from squares visible from the center
            PointToCenter();

            return flowMap;
        }

        /// <summary>
        /// If relaxed wasn't discovered yet, set angle to its coordinates. Add relaxed to the queue.
        /// </summary>
        private void Relax(Coords relaxed, float angle)
        {
            if (relaxed.Valid(flowMap.Width, flowMap.Height))
            {
                int x = relaxed.X;
                int y = relaxed.Y;
                if (flowMap[x, y] == null)
                {
                    flowMap[x, y] = angle;
                    discovered.Enqueue(relaxed);
                }
            }
        }
        
        /// <summary>
        /// Sets squares of the flowmap that are visible from center to point to the center.
        /// </summary>
        private void PointToCenter()
        {
            //ray has no length limit on the map
            float rayLength = obstacleMap.Width * obstacleMap.Height;
            //cast rays to the lines on bottom and top of the map
            for (int i = 0; i <= obstacleMap.Width; i++)
            {
                //top
                Ray rTop = new Ray(targetLocation,
                    new Vector2(i + 0.5f, obstacleMap.Height - 0.5f),
                    rayLength,
                    obstacleMap);
                while (rTop.Next(out int x, out int y))
                    flowMap[x, y] = new Vector2(x + 0.5f, y + 0.5f).AngleTo(targetLocation);

                //bottom
                Ray rBottom = new Ray(targetLocation,
                    new Vector2(i + 0.5f, 0.5f),
                    rayLength,
                    obstacleMap);
                while (rBottom.Next(out int x, out int y))
                    flowMap[x, y] = new Vector2(x + 0.5f, y + 0.5f).AngleTo(targetLocation);
            }
            //cast rays to the lines on left and right of the map
            for (int j = 0; j <= obstacleMap.Height; j++)
            {
                //left
                Ray rLeft = new Ray(targetLocation,
                    new Vector2(0.5f, j + 0.5f),
                    rayLength,
                    obstacleMap);
                while (rLeft.Next(out int x, out int y))
                    flowMap[x, y] = new Vector2(x + 0.5f, y + 0.5f).AngleTo(targetLocation);

                //right
                Ray rRight = new Ray(targetLocation,
                    new Vector2(obstacleMap.Width - 0.5f, j + 0.5f),
                    rayLength,
                    obstacleMap);
                while (rRight.Next(out int x, out int y))
                    flowMap[x, y] = new Vector2(x + 0.5f, y + 0.5f).AngleTo(targetLocation);
            }
        }

        /// <summary>
        /// Represents a square on the map.
        /// </summary>
        private struct Coords
        {
            public int X { get; }
            public int Y { get; }

            public Coords(int x, int y)
            {
                X = x;
                Y = y;
            }

            public bool Valid(int width, int height)
                => X >= 0 && X < width && Y >= 0 && Y < height;
        }
    }
}
