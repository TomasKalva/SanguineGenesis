using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    interface IEntity
    {
        float Bottom { get; }
        float Left { get; }
        float Right { get; }
        float Top { get; }
        float Width { get; }
        float Height { get; }
    }

    /// <summary>
    /// Used to implement default methods in the interface IEntity.
    /// </summary>
    static class IEntityExtensions
    {
        public static Rect GetRect(this IEntity entity)
        {
            return new Rect(entity.Left, entity.Bottom, entity.Right, entity.Top);
        }
    }
}
