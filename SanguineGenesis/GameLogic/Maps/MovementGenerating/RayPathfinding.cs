using SanguineGenesis.GameLogic.Maps.MovementGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Maps.MovementGenerating
{
    /// <summary>
    /// Used for generating flowfield using raycasting algorithm.
    /// </summary>
    class RayPathfinding
    {
        private static RayPathfinding pathfinding;
        public static RayPathfinding GetPathfinding => pathfinding;
        static RayPathfinding()
        {
            pathfinding = new RayPathfinding();
        }
        private RayPathfinding() { }

        private enum SquareState
        {
            BLOCKED,
            DISCOVERED,
            DISCOVERED_IN_CURRENT,
            NOT_DISCOVERED
        }

        public FlowField GenerateFlowField(ObstacleMap obst, Vector2 targetLocation)
        {
            FlowField flFap = new FlowField(obst.Width, obst.Height, targetLocation);
            //distance from the target
            float[,] distance = new float[obst.Width, obst.Height];
            //squares that need to be explored
            SquareState[,] state = new SquareState[obst.Width, obst.Height];
            //starting distance of squares - no square on the map can be further than this number
            float maxDist = obst.Width * obst.Height;
            for (int i = 0; i < obst.Width; i++)
                for (int j = 0; j < obst.Height; j++)
                {
                    distance[i, j] = maxDist;
                    state[i, j] = obst[i, j] ? SquareState.BLOCKED 
                        : SquareState.NOT_DISCOVERED;//only non obstacles need to be discovered
                }
            int targX = (int)targetLocation.X;
            int targY = (int)targetLocation.Y;
            distance[targX, targY] = 0;
            state[targX, targY] = SquareState.DISCOVERED;

            //first iteration
            RelaxDistances(new Vector2(targetLocation.X, targetLocation.Y), obst, distance, state, flFap);
            NextIteration(state);
            int iterations = 0;

            //iterate until all unblocked squares are discovered
            while (FindClosest(targetLocation,state,distance, out int x, out int y))
            {
                RelaxDistances(new Vector2(x + 0.5f, y + 0.5f), obst, distance, state, flFap);
                NextIteration(state);
                InferSmoothly(distance, state, flFap);
                iterations++;
            }
            //Console.WriteLine("Number of iterations: " + iterations);
            //remove vectors pointing to blocked squares
            RepairEdges(flFap, obst);

            return flFap;
        }

        /// <summary>
        /// Finds the square that is closest to the point, was already found and
        /// has a not discovered adjacent neighbour. Returns true if the square exists.
        /// </summary>
        private bool FindClosest(Vector2 point, SquareState[,] state, float[,] distance, out int x, out int y)
        {
            //the coordinates have to be initialized even if the algorithm returns false
            x = -1;
            y = -1;
            float minDist = state.GetLength(0) * state.GetLength(1);
            for(int i=0;i<state.GetLength(0);i++)
                for (int j = 0; j < state.GetLength(1); j++)
                {
                    if(state[i,j]==SquareState.DISCOVERED
                        && minDist > distance[i,j]
                        && NeighborNotFound(i,j,state))
                    {
                        minDist = distance[i,j];
                        x = i; y = j;
                    }
                }
            if (x == -1)
                return false;
            return true;
        }

        /// <summary>
        /// Returns true if at least one adjacent neighbor wasn't discovered yet.
        /// </summary>
        private bool NeighborNotFound(int x, int y, SquareState[,] state)
        {
            return ((x - 1 >= 0 && state[x - 1, y]==SquareState.NOT_DISCOVERED) ||
                (x + 1 < state.GetLength(0) && state[x + 1, y] == SquareState.NOT_DISCOVERED) ||
                (y - 1 >= 0 && state[x, y - 1] == SquareState.NOT_DISCOVERED) ||
                (y + 1 < state.GetLength(1) && state[x, y + 1] == SquareState.NOT_DISCOVERED)) ;
        }
        
        /// <summary>
        /// Raycast to find shorter paths.
        /// </summary>
        private void RelaxDistances(Vector2 center, ObstacleMap obst, float[,] distance, SquareState[,] state, FlowField flFap)
        {
            //ray has no length limit
            float rayLength = obst.Width * obst.Height;
            float centerDist = distance[(int)center.X, (int)center.Y];
            //cast rays to the lines on bottom and top of the map
            for (int i = 0; i <= obst.Width; i++)
            {
                //top
                Ray rTop = new Ray(center,
                    new Vector2(i + 0.5f, obst.Height - 0.5f),
                    rayLength,
                    obst);
                float angle = rTop.OppositeAngle;
                while (rTop.Next(out int x, out int y) &&
                    RayStep(x, y, angle,(float)Math.Atan2(center.Y - y - 0.5f, center.X - x - 0.5f),
                            centerDist + (new Vector2(x + 0.5f, y + 0.5f) -center).Length,
                            distance, flFap, state))
                    state[x, y] = SquareState.DISCOVERED_IN_CURRENT;

                //bottom
                Ray rBottom = new Ray(center,
                    new Vector2(i + 0.5f, 0.5f),
                    rayLength,
                    obst);
                angle = rBottom.OppositeAngle;
                while (rBottom.Next(out int x, out int y) &&
                    RayStep(x, y, angle, (float)Math.Atan2(center.Y - y - 0.5f, center.X - x - 0.5f),
                            centerDist + (new Vector2(x + 0.5f, y + 0.5f) - center).Length,
                            distance, flFap, state))
                    state[x, y] = SquareState.DISCOVERED_IN_CURRENT;
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
                while (rLeft.Next(out int x, out int y) &&
                    RayStep(x, y, angle, (float)Math.Atan2(center.Y - y - 0.5f, center.X - x - 0.5f), 
                            centerDist + (new Vector2(x + 0.5f, y + 0.5f) - center).Length, 
                            distance, flFap, state))
                    state[x, y] = SquareState.DISCOVERED_IN_CURRENT;

                //right
                Ray rRight = new Ray(center,
                    new Vector2(obst.Width - 0.5f, j + 0.5f),
                    rayLength,
                    obst);
                angle = rRight.OppositeAngle;
                while (rRight.Next(out int x, out int y) &&
                    RayStep(x, y, angle, (float)Math.Atan2(center.Y - y - 0.5f, center.X - x - 0.5f),
                            centerDist + (new Vector2(x + 0.5f, y + 0.5f) - center).Length, 
                            distance, flFap,state))
                    state[x, y] = SquareState.DISCOVERED_IN_CURRENT;
            }
        }

        /// <summary>
        /// Returns true if moving along the ray should continue.
        /// </summary>
        /// <returns></returns>
        private bool RayStep(int x, int y, float rayAngle, float squaresAngle, float newDistance,
                            float[,] distance, FlowField flFap, SquareState[,] state)
        {
            if (newDistance < distance[x, y])
            {
                //found shorter path to the square
                flFap[x, y] = rayAngle;
                distance[x, y] = newDistance;
                return true;
            }
            else if (state[x,y] == SquareState.DISCOVERED_IN_CURRENT)
            {
                //check if the path along this ray is more optimal
                float sqRayAngle = NormaliseAngle(squaresAngle - rayAngle);
                float sqFlMapAngle = NormaliseAngle(squaresAngle - flFap[x, y].Value);
                if (sqRayAngle<sqFlMapAngle)
                {
                    //ray angle is closer to the angles between the two squares,
                    //than the original angle
                    flFap[x, y] = rayAngle;
                }
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Normalises angle to the interval [0,2pi).
        /// </summary>
        private float NormaliseAngle(float angle)
        {
            while (angle >= 2 * Math.PI)
                angle -= (float)(2 * Math.PI);
            while (angle < 0)
                angle += (float)(2 * Math.PI);
            return angle;
        }

        /// <summary>
        /// Replaces SquareState.FOUND_IN_CURRENT by SquareState.FOUND.
        /// </summary>
        /// <param name="state"></param>
        private void NextIteration(SquareState[,] state)
        {
            for (int i = 0; i < state.GetLength(0); i++)
                for (int j = 0; j < state.GetLength(1); j++)
                    if (state[i, j] == SquareState.DISCOVERED_IN_CURRENT)
                        state[i, j] = SquareState.DISCOVERED;
        }

        /// <summary>
        /// Fill spaces that are surrounded by already discovered squares with their average angle.
        /// </summary>
        private void InferSmoothly(float[,] distance, SquareState[,] state, FlowField flFap)
        {
            for(int i=1;i<flFap.Width-1;i++)
                for(int j = 1; j < flFap.Height - 1; j++)
                {
                    if (state[i, j] != SquareState.NOT_DISCOVERED)
                        continue;

                    float dist = flFap.Width * flFap.Height;
                    float angle = 0;
                    int neigbCount = 0;
                    if (state[i - 1, j] == SquareState.DISCOVERED
                        && flFap[i - 1, j]!=null)
                    {
                        neigbCount++;
                        angle += flFap[i - 1, j].Value;
                        dist = Math.Min(dist, distance[i - 1, j]);
                    }
                    if (state[i + 1, j] == SquareState.DISCOVERED
                        && flFap[i + 1, j] != null)
                    {
                        neigbCount++;
                        angle += flFap[i + 1, j].Value;
                        dist = Math.Min(dist, distance[i + 1, j]);
                    }
                    if (state[i, j - 1] == SquareState.DISCOVERED
                        && flFap[i, j - 1] != null)
                    {
                        neigbCount++;
                        angle += flFap[i, j - 1].Value;
                        dist = Math.Min(dist, distance[i, j - 1]);
                    }
                    if (state[i, j + 1] == SquareState.DISCOVERED
                        && flFap[i, j + 1] != null)
                    {
                        neigbCount++;
                        angle += flFap[i, j + 1].Value;
                        dist = Math.Min(dist, distance[i, j + 1]);
                    }
                    if (neigbCount >= 3)
                    {
                        distance[i, j] = dist+1;
                        state[i, j] = SquareState.DISCOVERED;
                        flFap[i, j] = angle / neigbCount;
                    }
                }
        }

        /// <summary>
        /// When a component of a vector is pointing towards a blocked square, remove the component of
        /// the vecotr.
        /// </summary>
        private void RepairEdges(FlowField flowField, ObstacleMap obstMap)
        {
            for(int i=0;i<flowField.Width;i++)
                for(int j=0;j<flowField.Height;j++)
                {
                    //update only squares that are not blocked and have valid value of flowField
                    if (obstMap[i, j] || flowField[i,j]==null)
                        continue;

                    float angle = flowField[i, j].Value;
                    //directions of the components
                    int dirX = Math.Sign(Math.Cos(angle));
                    int dirY = Math.Sign(Math.Sin(angle));
                    //coordinate of vertical and horizontal neighbor
                    int neibX = i + dirX;
                    int neibY = j + dirY;

                    //check if the vector is pointing to a blocked square
                    if ((neibX >= flowField.Width || neibX < 0) ||
                        obstMap[neibX, j])
                        //set new angle in the normal direction
                        flowField[i, j] = dirY > 0 ? (float)Math.PI * 1 / 2f //up
                                                 : (float)Math.PI * 3 / 2f;//down


                    //check if the vector is pointing to a blocked square
                    if ((neibY >= flowField.Height || neibY < 0) ||
                        obstMap[i, neibY])
                        //set new angle in the normal direction
                        flowField[i, j] = dirX > 0 ? 0f               //right
                                                : (float)Math.PI;   //left
                }
        }
    }
}
