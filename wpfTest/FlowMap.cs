using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    public class FlowMap:IMap<float>
    {
        //values are oriented angles in radians relative to the positive x axis
        private float[,] directions;

        public float this[int i, int j]
        {
            get => directions[i, j];
            set => directions[i, j] = value;
        }
        public int Width => directions.GetLength(0);
        public int Height => directions.GetLength(1);

        internal FlowMap(int width, int height)
        {
            directions = new float[width, height];

            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    this[i, j] = .57f;
        }

        /// <summary>
        /// Finds the velocity at given coordinates with given speed. If the coordinates
        /// are out of the map, return zero vector.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="speed">Length of the velocity vector.</param>
        public Vector2 GetVelocity(float x, float y, float speed)
        {
            int i = (int)x; int j = (int)y;
            if (i < 0 || i >= Width || j < 0 || j >= Height)
                return new Vector2(0f, 0f);

            float angle = directions[(int)x, (int)y];
            return new Vector2(
                (float)Math.Cos(angle) * speed,
                (float)Math.Sin(angle) * speed
                );
        }

        public void Update()
        {
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    this[i, j] += .01f;
        }
    }

    public struct Vector2
    {
        float X { get; set; }
        float Y { get; set; }

        internal Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
