using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Maps
{
    /// <summary>
    /// Represents part of a map visible by a player.
    /// </summary>
    class VisibilityMap : IMap<bool>
    {
        /// <summary>
        /// Data of the map.
        /// </summary>
        private bool[,] visible;

        public bool this[int i, int j]
        {
            get => visible [i, j];
            set => visible[i, j] = value;
        }

        /// <summary>
        /// Width of the map in squares.
        /// </summary>
        public int Width => visible.GetLength(0);
        /// <summary>
        /// Height of the map in squares.
        /// </summary>
        public int Height => visible.GetLength(1);

        /// <summary>
        /// Creates new visibility map with the given widht and height. Nothing is visible.
        /// </summary>
        public VisibilityMap(int width, int height)
        {
            visible = new bool[width, height];
        }
    }
}
