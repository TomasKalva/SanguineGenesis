using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;

namespace SanguineGenesis.GameLogic.Maps.MovementGenerating
{
    class FlowField:IMap<float?>
    {
        /// <summary>
        /// If value of the flowfield is at most POINT_TO_TARGET, the intensity points to the target.
        /// </summary>
        public const float POINT_TO_TARGET = -50f;

        /// <summary>
        /// Values are oriented angles in radians relative to the positive x axis.
        /// If the value is not present, the direction goes straight to the target.
        /// </summary>
        private float?[,] directions;
        /// <summary>
        /// The point to which this flow field is flowing.
        /// </summary>
        private Vector2 target;

        public float? this[int i, int j]
        {
            get => directions[i, j];
            set => directions[i, j] = value;
        }
        /// <summary>
        /// Width of the map in squares.
        /// </summary>
        public int Width => directions.GetLength(0);
        /// <summary>
        /// Height of the map in squares.
        /// </summary>
        public int Height => directions.GetLength(1);

        internal FlowField(int width, int height, Vector2 target)
        {
            directions = new float?[width, height];
            this.target = target;
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
                directions[i, j] == null)
                return new Vector2(0, 0) ;

            
            float angle = directions[i, j].Value;
            //return direction that points directly to the target
            if(PointToTarget(angle))
                return speed * (position.UnitDirectionTo(target));
            //return direction from this flowfield
            return new Vector2(
                (float)Math.Cos(angle) * speed,
                (float)Math.Sin(angle) * speed
                );
        }

        /// <summary>
        /// Returns true iff the angle indicates that angle pointing to the target should be used,
        /// instead of this angle.
        /// </summary>
        public static bool PointToTarget(float angle) => angle <= POINT_TO_TARGET;
    }
}
