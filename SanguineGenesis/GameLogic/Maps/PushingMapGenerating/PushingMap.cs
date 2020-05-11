using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Maps.PushingMapGenerating
{
    /// <summary>
    /// Used for pushing animals out of terrain they can't move in.
    /// </summary>
    class PushingMap : IMap<PushingSquare?>
    {
        /// <summary>
        /// Data of the map.
        /// </summary>
        private readonly PushingSquare?[,] pushingSquares;

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
        /// Finds the direction at given coordinates. If the coordinates
        /// are out of the map or the subsquare has no direction, returns null.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="speed">Length of the velocity vector.</param>
        public Vector2? GetDirection(Vector2 position)
        {
            int i = (int)position.X; int j = (int)position.Y;
            if (i < 0 || i >= Width || j < 0 || j >= Height ||
                pushingSquares[i, j]==null)
                return null;

            
            float? angle = pushingSquares[i, j].Value.GetDirection(position.X % 1, position.Y % 1);
            if (angle != null)
                //map pushes in the angle direction
                return new Vector2(
                    (float)Math.Cos(angle.Value),
                    (float)Math.Sin(angle.Value)
                    );
            else
                //map doesn't push
                return null;
        }
    }

    /// <summary>
    /// Represents one square divided into four smaller squares. Each variable corresponds to one
    /// of these square and represents a direction where animal will be pushed if it enters this square.
    /// Directions are given by the angle from positive x axis.
    /// </summary>
    public struct PushingSquare
    {
        public readonly float? dir_11; /*top left*/    public readonly float? dir_12; /*top right*/
        public readonly float? dir_21;/*bottom left*/  public readonly float? dir_22; /*bottom right*/

        public PushingSquare(float? dir_11, float? dir_12,
                            float? dir_21, float? dir_22)
        {
            this.dir_11 = dir_11;
            this.dir_12 = dir_12;
            this.dir_21 = dir_21;
            this.dir_22 = dir_22;
        }

        /// <summary>
        /// Returns direction, in which the animal should move. x and y represent position in square.
        /// They have to be from interval [0,1].
        /// </summary>
        public float? GetDirection(float x, float y)
        {
            if (x < 0.5f)
                if (y > 0.5f)
                    return dir_11;
                else
                    return dir_21;
            else
                if (y > 0.5f)
                    return dir_12;
                else
                    return dir_22;
        }

        public override string ToString()
        {
            return dir_11 + "; " + dir_12 + "; \n" + dir_21 + "; " + dir_22;
        }
    }
}
