﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    public struct Rect
    {
        public float Bottom { get; }
        public float Left { get; }
        public float Right { get; }
        public float Top { get; }
        public float Width => Right - Left;
        public float Height => Top - Bottom;

        public Rect(float left, float bottom, float right, float top)
        {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }

        /// <summary>
        /// Returns true if the point is inside this rectangle.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        public bool PointInside(float x, float y)
            => x >= Left && x <= Right && y >= Bottom && y <= Top;

        /*
        /// <summary>
         /// Returns true if this rectangle collides with rect.
         /// </summary>
        public bool CollidesWith(Rect rect)
        {
            if((Left<=rect.Left && rect.Left<=Right
                && rect.Top>=Bottom && rect.Bottom<=Top) 
                || 
                (Left <= rect.Right && rect.Right <= Right
                && rect.Top >= Bottom && rect.Bottom <= Top)
                || 
                (Bottom <= rect.Bottom && rect.Bottom <= Top
                && rect.Right >= Left && rect.Left <= Right)
                || 
                (Bottom <= rect.Top && rect.Top <= Top
                && rect.Right >= Left && rect.Left <= Right))
                return true;
            return false;
        }*/

        public bool IntersectsWith(Rect rect)
        {
            //two rectangles don't overlap if the left edge of one of them is
            //to the right from the other or if one of them is above the other
            //in all other cases the rectangles overlap
            return !(rect.Left > Right || rect.Right < Left ||
                    rect.Bottom > Top || rect.Top < Bottom);
        }
    }
}
