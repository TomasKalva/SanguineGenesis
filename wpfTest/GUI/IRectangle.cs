using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    interface IRectangle
    {
        float Bottom { get; }
        float Left { get; }
        float Right { get; }
        float Top { get; }
        float Width { get; }
        float Height { get; }
    }

    /// <summary>
    /// Used to implement default methods in the interface IRectangle.
    /// </summary>
    static class IRectangleExtensions
    {
        public static Rect GetRect(this IRectangle rect)
        {
            return new Rect(rect.Left, rect.Bottom, rect.Right, rect.Top);
        }
    }
}
