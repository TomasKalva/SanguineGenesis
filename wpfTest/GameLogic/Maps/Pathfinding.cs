using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Maps
{
    class Pathfinding
    {
        private static Pathfinding pathfinding;
        public static Pathfinding GetPathfinding => pathfinding;

        private enum SquareState
        {
            BLOCKED,
            FOUND,
            FOUND_IN_CURRENT,
            NOT_FOUND
        }

        static Pathfinding()
        {
            pathfinding = new Pathfinding();
        }

        private Pathfinding() { }

        public FlowMap GenerateFlowMap(ObstacleMap obst, List<Unit> units, Vector2 targetLocation)
        {
            FlowMap flMap = new FlowMap(obst.Width, obst.Height);
            //distance from the target
            float[,] distance = new float[obst.Width, obst.Height];
            //squares that need to be explored
            SquareState[,] needsExploring = new SquareState[obst.Width, obst.Height];
            float maxDist = obst.Width * obst.Height;
            for (int i = 0; i < obst.Width; i++)
                for (int j = 0; j < obst.Height; j++)
                {
                    distance[i, j] = maxDist;
                    needsExploring[i, j] = obst[i, j] ? SquareState.BLOCKED 
                        : SquareState.NOT_FOUND;//only non obstacles need to be explored
                }
            int targX = (int)targetLocation.X;
            int targY = (int)targetLocation.Y;
            distance[targX, targY] = 0;
            needsExploring[targX, targY] = SquareState.FOUND;

            RelaxDistances(new Vector2(targetLocation.X, targetLocation.Y), obst, distance, needsExploring, flMap);
            NextIteration(needsExploring);
            while (FindClosest(targetLocation,needsExploring,distance, out int x, out int y))
            {
                RelaxDistances(new Vector2(x + 0.5f, y + 0.5f), obst, distance, needsExploring, flMap);
                NextIteration(needsExploring);
            }

            return flMap;
        }

        /// <summary>
        /// Finds the square that is closest to the point, was already found and
        /// has a not found adjacent neighbour. Returns true if the square exists.
        /// </summary>
        /// <returns></returns>
        private bool FindClosest(Vector2 point, SquareState[,] needsExploring, float[,] distance, out int x, out int y)
        {
            //the coordinates have to be initialized even if the algorithm returns false
            x = -1;
            y = -1;
            float minDist = needsExploring.GetLength(0) * needsExploring.GetLength(1);
            for(int i=0;i<needsExploring.GetLength(0);i++)
                for (int j = 0; j < needsExploring.GetLength(1); j++)
                {
                    float dist = 0;//will be assigned value in the condition
                    if(needsExploring[i,j]==SquareState.FOUND
                        && minDist > (dist = distance[i,j]/*new Vector2(i+0.5f-point.X, j+0.5f-point.Y).Length*/)
                        && NeighborNotFound(i,j,needsExploring))
                    {
                        minDist = dist;
                        x = i; y = j;
                    }
                }
            if (x == -1)
                return false;
            return true;
        }

        /// <summary>
        /// Returns true if at least one adjacent neighbor wasn't found yet.
        /// </summary>
        private bool NeighborNotFound(int x, int y, SquareState[,] needsExploring)
        {
            return ((x - 1 >= 0 && needsExploring[x - 1, y]==SquareState.NOT_FOUND) ||
                (x + 1 < needsExploring.GetLength(0) && needsExploring[x + 1, y] == SquareState.NOT_FOUND) ||
                (y - 1 >= 0 && needsExploring[x, y - 1] == SquareState.NOT_FOUND) ||
                (y + 1 < needsExploring.GetLength(1) && needsExploring[x, y + 1] == SquareState.NOT_FOUND)) ;
        }
        
        private void RelaxDistances(Vector2 center, ObstacleMap obst, float[,] distance, SquareState[,] needsExploring, FlowMap flMap)
        {
            //ray has no length limit
            float rayLength = obst.Width * obst.Height;
            float centerDist = distance[(int)center.X, (int)center.Y];
            //cast rays to the lines on bottom and top of the map
            for (int i = 0; i <= obst.Width; i++)
            {
                Ray rTop = new Ray(center,
                    new Vector2(i + 0.5f, obst.Height - 0.5f),
                    rayLength,
                    obst);
                float angle = rTop.OppositeAngle;
                while (rTop.Next(out int x, out int y) &&
                    RayStep(x, y, angle,(float)Math.Atan2(center.Y - y - 0.5f, center.X - x - 0.5f), centerDist + (new Vector2(x + 0.5f, y + 0.5f) -center).Length, distance, flMap, needsExploring))
                    needsExploring[x, y] = SquareState.FOUND_IN_CURRENT;

                Ray rBottom = new Ray(center,
                    new Vector2(i + 0.5f, 0.5f),
                    rayLength,
                    obst);
                angle = rBottom.OppositeAngle;
                while (rBottom.Next(out int x, out int y) &&
                    RayStep(x, y, angle, (float)Math.Atan2(center.Y - y - 0.5f, center.X - x - 0.5f), centerDist + (new Vector2(x + 0.5f, y + 0.5f) - center).Length, distance, flMap, needsExploring))
                    needsExploring[x, y] = SquareState.FOUND_IN_CURRENT;
            }
            //cast rays to the lines on left and right of the map
            for (int j = 0; j <= obst.Height; j++)
            {
                Ray rLeft = new Ray(center,
                    new Vector2(0.5f, j + 0.5f),
                    rayLength,
                    obst);
                float angle = rLeft.OppositeAngle;
                while (rLeft.Next(out int x, out int y) &&
                    RayStep(x, y, angle, (float)Math.Atan2(center.Y - y - 0.5f, center.X - x - 0.5f), centerDist + (new Vector2(x + 0.5f, y + 0.5f) - center).Length, distance, flMap, needsExploring))
                    needsExploring[x, y] = SquareState.FOUND_IN_CURRENT;

                Ray rRight = new Ray(center,
                    new Vector2(obst.Width - 0.5f, j + 0.5f),
                    rayLength,
                    obst);
                angle = rRight.OppositeAngle;
                while (rRight.Next(out int x, out int y) &&
                    RayStep(x, y, angle, (float)Math.Atan2(center.Y - y - 0.5f, center.X - x - 0.5f), centerDist + (new Vector2(x + 0.5f, y + 0.5f) - center).Length, distance, flMap,needsExploring))
                    needsExploring[x, y] = SquareState.FOUND_IN_CURRENT;
            }
        }

        /// <summary>
        /// Returns true if moving along the ray should continue.
        /// </summary>
        /// <returns></returns>
        private bool RayStep(int x, int y, float rayAngle, float squaresAngle, float newDistance,
                            float[,] distance, FlowMap flMap, SquareState[,] needsExploring)
        {
            if (newDistance < distance[x, y])
            {
                flMap[x, y] = rayAngle;
                distance[x, y] = newDistance;
                return true;
            }
            else if (needsExploring[x,y] == SquareState.FOUND_IN_CURRENT)
            {
                float sqRayAngle = NormaliseAngle(squaresAngle - rayAngle);
                float sqFlMapAngle = NormaliseAngle(squaresAngle - flMap[x, y]);
                if (sqRayAngle<sqFlMapAngle)
                {
                    //ray angle is closer to the angles between the two squares,
                    //than the original angle
                    flMap[x, y] = rayAngle;
                    //distance[x, y] = newDistance;
                }
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Normalises angle to the interval [0,2pi).
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private float NormaliseAngle(float angle)
        {
            while (angle >= 2 * Math.PI)
                angle -= (float)(2 * Math.PI);
            while (angle < 0)
                angle += (float)(2 * Math.PI);
            return angle;
        }

        private void NextIteration(SquareState[,] needsExploring)
        {
            for (int i = 0; i < needsExploring.GetLength(0); i++)
                for (int j = 0; j < needsExploring.GetLength(1); j++)
                    if (needsExploring[i, j] == SquareState.FOUND_IN_CURRENT)
                        needsExploring[i, j] = SquareState.FOUND;
        }
    }
}
