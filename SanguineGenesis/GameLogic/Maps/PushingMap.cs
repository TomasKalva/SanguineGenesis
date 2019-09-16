using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Maps
{
    /// <summary>
    /// Used for pushing animals out of terrain they can't walk on.
    /// </summary>
    public class PushingMap : IMap<PushingSquare?>
    {
        /// <summary>
        /// Data of the map.
        /// </summary>
        private PushingSquare?[,] pushingSquares;

        public PushingSquare? this[int i, int j]
        {
            get => pushingSquares[i, j];
            set => pushingSquares[i, j] = value;
        }
        /// <summary>
        /// Width of the map in squares.
        /// </summary>
        public int Width => pushingSquares.GetLength(0);
        /// <summary>
        /// Height of the map in squares.
        /// </summary>
        public int Height => pushingSquares.GetLength(1);

        /// <summary>
        /// Creates new pushing map with the given width and height with no pushing squares.
        /// </summary>
        internal PushingMap(int width, int height)
        {
            pushingSquares = new PushingSquare?[width, height];
        }

        /// <summary>
        /// Finds the velocity at given coordinates with given speed. If the coordinates
        /// are out of the map, return zero vector.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="speed">Length of the velocity vector.</param>
        public Vector2 GetIntensity(Vector2 position, float speed)
        {
            int i = (int)position.X; int j = (int)position.Y;
            if (i < 0 || i >= Width || j < 0 || j >= Height ||
                pushingSquares[i, j]==null)
                return new Vector2(0f, 0f);

            
            float angle = pushingSquares[i, j].Value.GetDirection(position.X % 1, position.Y % 1);
            return new Vector2(
                (float)Math.Cos(angle) * speed,
                (float)Math.Sin(angle) * speed
                );
        }
    }

    /// <summary>
    /// Represents one square divided into four smaller squares. Each variable corresponds to one
    /// of these square and represents a direction where animal will be pushed if it enters this square.
    /// </summary>
    public struct PushingSquare
    {
        public float Dir_11 { get; }/*top left*/    public float Dir_12 { get; }/*top right*/
        public float Dir_21 { get; }/*bottom left*/ public float Dir_22 { get; }/*bottom right*/

        public PushingSquare(float dir_11, float dir_12,
                            float dir_21, float dir_22)
        {
            this.Dir_11 = dir_11;
            this.Dir_12 = dir_12;
            this.Dir_21 = dir_21;
            this.Dir_22 = dir_22;
        }

        /// <summary>
        /// Returns direction, in which the animal should move. x and y represent position in square.
        /// They have to be from interval [0,1].
        /// </summary>
        public float GetDirection(float x, float y)
        {
            if (x < 0.5f)
                if (y > 0.5f)
                    return Dir_11;
                else
                    return Dir_21;
            else
                if (y > 0.5f)
                    return Dir_12;
                else
                    return Dir_22;
        }

        public override string ToString()
        {
            return Dir_11 + "; " + Dir_12 + "; \n" + Dir_21 + "; " + Dir_22;
        }
    }
}
