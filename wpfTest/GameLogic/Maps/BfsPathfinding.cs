using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Maps
{
    /// <summary>
    /// Used for generating flowmap using bfs with raycasting heuristics.
    /// </summary>
    class BfsPathfinding : IPathfinding
    {
        public FlowMap GenerateFlowMap(ObstacleMap obst, Vector2 targetLocation)
        {
            int width = obst.Width;
            int height = obst.Height;
            FlowMap flowMap = new FlowMap(width, height);

            //find paths using bfs
            Queue<Coords> discovered = new Queue<Coords>();
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
                if (obst[x, y])
                    continue;

                float _90degrees = (float)Math.PI / 2;
                Relax(new Coords(x, y - 1), flowMap, discovered, _90degrees);
                Relax(new Coords(x + 1, y), flowMap, discovered, _90degrees * 2);
                Relax(new Coords(x, y + 1), flowMap, discovered, _90degrees * 3);
                Relax(new Coords(x - 1, y), flowMap, discovered, _90degrees * 4);
            }

            //find straight path from squares visible from the center
            PointToCenter(targetLocation, obst, flowMap);

            return flowMap;
        }

        /// <summary>
        /// If relaxed wasn't discovered yet, set angle to its coordinates. Add relaxed to the queue.
        /// </summary>
        private void Relax(Coords relaxed, FlowMap flowMap, Queue<Coords> discovered, float angle)
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
        private void PointToCenter(Vector2 center, ObstacleMap obst, FlowMap flowMap)
        {
            //ray has no length limit on the map
            float rayLength = obst.Width * obst.Height;
            //cast rays to the lines on bottom and top of the map
            for (int i = 0; i <= obst.Width; i++)
            {
                //top
                Ray rTop = new Ray(center,
                    new Vector2(i + 0.5f, obst.Height - 0.5f),
                    rayLength,
                    obst);
                float angle = rTop.OppositeAngle;
                while (rTop.Next(out int x, out int y))
                    flowMap[x, y] = angle;

                //bottom
                Ray rBottom = new Ray(center,
                    new Vector2(i + 0.5f, 0.5f),
                    rayLength,
                    obst);
                angle = rBottom.OppositeAngle;
                while (rBottom.Next(out int x, out int y))
                    flowMap[x, y] = angle;
            }
            //cast rays to the lines on left and right of the map
            for (int j = 0; j <= obst.Height; j++)
            {
                //left
                Ray rLeft = new Ray(center,
                    new Vector2(0.5f, j + 0.5f),
                    rayLength,
                    obst);
                float angle = rLeft.OppositeAngle;
                while (rLeft.Next(out int x, out int y))
                    flowMap[x, y] = angle; ;

                //right
                Ray rRight = new Ray(center,
                    new Vector2(obst.Width - 0.5f, j + 0.5f),
                    rayLength,
                    obst);
                angle = rRight.OppositeAngle;
                while (rRight.Next(out int x, out int y))
                    flowMap[x, y] = angle; ;
            }
        }

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
