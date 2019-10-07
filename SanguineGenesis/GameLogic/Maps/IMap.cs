using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis
{
    /// <summary>
    /// Represents rectangle area of squares. Type of the square is Square.
    /// </summary>
    interface IMap<Square>
    {
        Square this[int i, int j] { get;}
        /// <summary>
        /// Width of the map in squares.
        /// </summary>
        int Width { get; }
        /// <summary>
        /// Height of the map in squares.
        /// </summary>
        int Height { get; }
    }
}
