using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Maps
{
    public struct Ray
    {
        private Vector2 Start { get; }
        private Vector2 End { get; }
        private Vector2 Current { get; set; }
        private ObstacleMap ObstMap { get; }
        public float Angle => throw new NotImplementedException();

        public Ray(Vector2 start, Vector2 end, ObstacleMap obstacleMap)
        {
            this.Start = start;
            this.End = end;
            this.Current = start;
            this.ObstMap = obstacleMap;
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
        }

        /// <summary>
        /// Moves to the next (from the start to the end) square that 
        /// intersects this ray and returns its coordinates x, y.
        /// Returns false if no such square exists.
        /// </summary>
        public bool Next(out int x, out int y)
        {
            //initial values are set, because we might return false
            x = -1;
            y = -1;

            ////now just for rays pointing to the right top
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

            Vector2 Delta = End - Current;

            //(x,y) = Current + t*Delta
            //t for intersection with vertical axis
            float tY = Math.Abs((intersAxX - Current.X) / Delta.X);
            //t for intersection with horizontal axis
            float tX = Math.Abs((intersAxY - Current.Y) / Delta.Y);
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
            Current = Current + t * Delta;

            //return false if the next square is outside of the ray
            Vector2 Dir = Delta;
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
                return false;

            //return false if the ray hits an obstacle
            if (ObstMap[x, y])
                return false;

            return true;
        }


    }
}
