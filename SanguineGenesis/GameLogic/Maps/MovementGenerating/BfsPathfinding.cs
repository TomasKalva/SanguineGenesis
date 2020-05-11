using SanguineGenesis.GameLogic.Maps.VisibilityGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Maps.MovementGenerating
{
    interface IPathfinding
    {
        FlowField GenerateFlowField();
    }

    /// <summary>
    /// Used for generating flowfield using bfs with rayuseing heuristics. One instance of the class
    /// is one instance of the algorithm. The flowfield overlaps with movement blocked square by one
    /// node.
    /// </summary>
    class BfsPathfinding : IPathfinding
    {
        /// <summary>
        /// The result of the algorithm.
        /// </summary>
        private readonly FlowField flowField;

        /// <summary>
        /// Contains all discovered coordinates that weren't processed yet.
        /// </summary>
        private Queue<Coords> discovered;
        /// <summary>
        /// Obstacles that need to be avoided.
        /// </summary>
        private readonly ObstacleMap obstacleMap;
        /// <summary>
        /// Ending point of the flowfield.
        /// </summary>
        private Vector2 targetLocation;

        public BfsPathfinding(ObstacleMap obst, Vector2 targetLocation)
        {
            int width = obst.Width;
            int height = obst.Height;
            flowField = new FlowField(width, height, targetLocation);
            obstacleMap = obst;
            this.targetLocation = targetLocation;
            discovered = new Queue<Coords>();
            //initialize walkable nodes to point to target
            for(int i=0; i<width; i++)
                for(int j = 0; j<width; j++)
                    if(!obst[i,j])
                        flowField[i, j] = FlowField.POINT_TO_TARGET;
        }

        public FlowField GenerateFlowField()
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
                //return empty flowfield if target location is invalid
                return flowField;
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
            RepairEdges();

            flowField[target.X, target.Y] = null;

            return flowField;
        }

        /// <summary>
        /// If relaxed wasn't discovered yet, set angle to its coordinates. Add relaxed to the queue.
        /// </summary>
        private void Relax(Coords relaxed, float angle)
        {
            if (relaxed.Valid(flowField.Width, flowField.Height))
            {
                int x = relaxed.X;
                int y = relaxed.Y;
                if (flowField[x, y] == null || FlowField.PointToTarget(flowField[x,y].Value))
                {
                    flowField[x, y] = angle;
                    discovered.Enqueue(relaxed);
                }
            }
        }
        
        /// <summary>
        /// Sets squares of the flowfield that are visible from center to point to the center.
        /// </summary>
        private void PointToCenter()
        {
            //ray has no length limit on the map
            float rayLength = obstacleMap.Width * obstacleMap.Height;
            //use rays to the lines on bottom and top of the map
            for (int i = 0; i <= obstacleMap.Width; i++)
            {
                //top
                Ray rTop = new Ray(targetLocation,
                    new Vector2(i + 0.5f, obstacleMap.Height - 0.5f),
                    rayLength,
                    obstacleMap);
                while (rTop.Next(out int x, out int y))
                    flowField[x, y] = FlowField.POINT_TO_TARGET;//direction will point straight to the target

                //bottom
                Ray rBottom = new Ray(targetLocation,
                    new Vector2(i + 0.5f, 0.5f),
                    rayLength,
                    obstacleMap);
                while (rBottom.Next(out int x, out int y))
                    flowField[x, y] = FlowField.POINT_TO_TARGET;//direction will point straight to the target
            }
            //use rays to the lines on left and right of the map
            for (int j = 0; j <= obstacleMap.Height; j++)
            {
                //left
                Ray rLeft = new Ray(targetLocation,
                    new Vector2(0.5f, j + 0.5f),
                    rayLength,
                    obstacleMap);
                while (rLeft.Next(out int x, out int y))
                    flowField[x, y] = FlowField.POINT_TO_TARGET;//direction will point straight to the target

                //right
                Ray rRight = new Ray(targetLocation,
                    new Vector2(obstacleMap.Width - 0.5f, j + 0.5f),
                    rayLength,
                    obstacleMap);
                while (rRight.Next(out int x, out int y))
                    flowField[x, y] = FlowField.POINT_TO_TARGET;//direction will point straight to the target
            }
        }
        
        /// <summary>
        /// When a component of a vector is pointing towards a blocked square, remove the component of
        /// the vecotr.
        /// </summary>
        private void RepairEdges()
        {
            for (int i = 0; i < flowField.Width; i++)
                for (int j = 0; j < flowField.Height; j++)
                {
                    //update only squares that are not blocked and have valid value of flowField
                    if (obstacleMap[i, j])
                        continue;

                    var dir = flowField.GetIntensity(new Vector2(i + .5f, j + .5f), 1f);
                    //directions of the components
                    int dirX = Math.Sign(dir.X);
                    int dirY = Math.Sign(dir.Y);
                    //coordinate of vertical and horizontal neighbor
                    int neibX = i + dirX;
                    int neibY = j + dirY;

                    //check if the vector is pointing to a blocked square
                    if ((neibX >= flowField.Width || neibX < 0) ||
                        obstacleMap[neibX, j])
                        //set new angle in the normal direction
                        flowField[i, j] = dirY > 0 ? (float)Math.PI * 1 / 2f //up
                                                 : (float)Math.PI * 3 / 2f;//down


                    //check if the vector is pointing to a blocked square
                    if ((neibY >= flowField.Height || neibY < 0) ||
                        obstacleMap[i, neibY])
                        //set new angle in the normal direction
                        flowField[i, j] = dirX > 0 ? 0f               //right
                                                : (float)Math.PI;   //left
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
