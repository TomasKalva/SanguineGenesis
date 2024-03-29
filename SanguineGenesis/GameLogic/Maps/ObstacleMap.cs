﻿using SanguineGenesis.GameLogic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly : InternalsVisibleTo("GameLogicTests")]
namespace SanguineGenesis.GameLogic.Maps
{
    /// <summary>
    /// Represents areas that can't be passed through. Around the map is a
    /// frame of unpassable areas with width one.
    /// </summary>
    class ObstacleMap : IMap<bool>
    {
        /// <summary>
        /// Data of the map.
        /// </summary>
        private readonly bool[,] isObstacle;

        public bool this[int i, int j]
        {
            get => isObstacle[i+1, j+1];
            set => isObstacle[i+1, j+1] = value;
        }
        public int Width => isObstacle.GetLength(0) - 2;
        public int Height => isObstacle.GetLength(1) - 2;

        /// <summary>
        /// Creates an obstacle map with a frame around it.
        /// </summary>
        /// <param name="width">Width of the map.</param>
        /// <param name="height">Height of the map.</param>
        public ObstacleMap(int width, int height)
        {
            isObstacle = new bool[width+2, height+2];
            for (int i = -1; i < Width+1; i++)
                this[i, -1] = true;
            for (int i = -1; i < Width+1; i++)
                this[i, Height] = true;
            for (int j = -1; j < Height+1; j++)
                this[-1, j] = true;
            for (int j = -1; j < Height+1; j++)
                this[Width, j] = true;
        }

        /// <summary>
        /// Creates an obstacle map with a frame around it.
        /// </summary>
        /// <param name="width">Width of the map.</param>
        /// <param name="height">Height of the map.</param>
        public ObstacleMap(ObstacleMap source)
            :this(source.Width, source.Height)
        {
            for(int i=0;i<Width;i++)
                for (int j = 0; j < Height; j++)
                {
                    this[i, j] = source[i, j];
                }
        }
        /// <summary>
        /// Returns true if the point is inside an obstacle.
        /// </summary>
        public bool CollidingWithObstacle(Vector2 point)
        {
            int i = (int)point.X; int j = (int)point.Y;
            if (i < 0 || i >= Width || j < 0 || j >= Height)
                return true;

            return this[i, j];
        }
    }
}
