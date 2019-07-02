using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    public class Unit
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Size { get; }

        public Unit(float x, float y, float size=1f)
        {
            X = x;
            Y = y;
            Size = size;
        }

        public void Move(float dx, float dy)
        {
            X += dx;
            Y += dy;
        }

        public float GetActualBottom(float imageBottom)
            => Math.Min(Y - Size, Y + imageBottom);
        public float GetActualTop(float imageHeight, float imageBottom)
            => Math.Max(Y + Size, Y + imageBottom+imageHeight);
        public float GetActualLeft(float imageLeft)
            => Math.Min(X - Size, X + imageLeft);
        public float GetActualRight(float imageWidth, float imageLeft)
            => Math.Max(X + Size, X + imageLeft + imageWidth);
    }
}
