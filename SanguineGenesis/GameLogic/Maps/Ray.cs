using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Maps
{
    /// <summary>
    /// Represents ray from one point to another. It returns indices of
    /// squares it intersects in the order of the intersections.
    /// </summary>
    public struct Ray
    {
        /// <summary>
        /// Starting point of the ray.
        /// </summary>
        private Vector2 Start { get; }
        /// <summary>
        /// Ending point of the ray.
        /// </summary>
        private Vector2 End { get; }
        /// <summary>
        /// Current position on the ray.
        /// </summary>
        private Vector2 Current { get; set; }
        /// <summary>
        /// Obstacle map that blocks the ray.
        /// </summary>
        private ObstacleMap ObstMap { get; }
        /// <summary>
        /// Angle from Start to End relative to positive x axis.
        /// </summary>
        public float Angle { get; }
        /// <summary>
        /// Angle from End to Start relative to positive x axis.
        /// </summary>
        public float OppositeAngle => Angle + (float)Math.PI;
        /// <summary>
        /// Distance that was already traveled on the ray.
        /// </summary>
        public float TraveledDist => (Current - Start).Length;

        public Ray(Vector2 start, Vector2 end, ObstacleMap obstacleMap)
        {
            this.Start = start;
            this.End = end;
            this.Current = start;
            this.ObstMap = obstacleMap;
            Angle= (float)(Math.Atan2(End.Y - Start.Y, End.X - Start.X) + 2 * Math.PI);
        }

        /// <summary>
        /// Creates a new ray with given direction and length.
        /// </summary>
        public Ray(Vector2 start, Vector2 end, float length, ObstacleMap obstacleMap)
        {
            this.Start = start;
            this.End = start + length * start.UnitDirectionTo(end);
            this.Current = start;
            this.ObstMap = obstacleMap;
            Angle = (float)(Math.Atan2(End.Y - Start.Y, End.X - Start.X) + 2*Math.PI);
        }

        /// <summary>
        /// Moves to the next (from the start to the end) square that 
        /// intersects this ray and returns its coordinates x, y.
        /// Returns false if no such square exists or the square is blocked.
        /// If the square doesn't exist, x=-1 and y=-1. If the square is blocked,
        /// it contains correct coordinates.
        /// </summary>
        public bool Next(out int x, out int y)
        {
            //initial values are set, because we might return false
            x = -1;
            y = -1;
            
            //the next axis to be intersected
            int intersAxX, intersAxY;
            if(End.X > Start.X)
                //moving right
                intersAxX= (int)Current.X + 1;
            else
                //moving left
                intersAxX = (int)Math.Ceiling(Current.X) - 1;

            if (End.Y > Start.Y)
                //moving top
                intersAxY = (int)Current.Y + 1;
            else
                //moving bottom
                intersAxY = (int)Math.Ceiling(Current.Y) - 1;

            Vector2 delta = End - Current;

            //(x,y) = Current + t*Delta
            //t for intersection with vertical axis
            float tY = Math.Abs((intersAxX - Current.X) / delta.X);
            //t for intersection with horizontal axis
            float tX = Math.Abs((intersAxY - Current.Y) / delta.Y);
            //t determines how far across the ray we move
            //we want to move the lower distance
            float t;
            bool horizontalInters;
            if (tX < tY)
            {
                horizontalInters = true;
                t = tX;
            }
            else
            {
                horizontalInters = false;
                t = tY;
            }

            //update Current
            Current = Current + t * delta;

            //return false if the next square is outside of the ray
            Vector2 Dir = delta;
            if (Current.X * Dir.X > End.X * Dir.X || 
                Current.Y * Dir.Y > End.Y * Dir.Y)
                return false;

            //calculate the coordinates of the square that we entered
            if (horizontalInters)
            //horizontal intersection
            {
                if (Dir.Y > 0)
                    y = (int)Current.Y;
                else
                    y = (int)Current.Y - 1;

                x = (int)Current.X;
            }
            else
            //vertical intersection
            {
                if (Dir.X > 0)
                    x = (int)Current.X;
                else
                    x = (int)Current.X - 1;

                y = (int)Current.Y;
            }

            //return false if the next square is outside of the map
            if (x >= ObstMap.Width ||
                x < 0 ||
                y >= ObstMap.Height ||
                y < 0)
            {
                x = -1;
                y = -1;
                return false;
            }

            //return false if the ray hits an obstacle
            if (ObstMap[x, y])
                return false;

            return true;
        }
    }
}
