using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;

namespace SanguineGenesis
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

    /// <summary>
    /// Represents a 2d vector.
    /// </summary>
    public struct Vector2 : ITargetable, IMovementTarget
    {
        Vector2 ITargetable.Center => this;

        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 operator +(Vector2 u, Vector2 v) =>
            new Vector2(u.X + v.X, u.Y + v.Y);
        public static Vector2 operator -(Vector2 u, Vector2 v) =>
            new Vector2(u.X - v.X, u.Y - v.Y);
        public static Vector2 operator /(Vector2 v, float a) =>
            new Vector2(v.X / a, v.Y / a);
        public static Vector2 operator *(float a, Vector2 v) =>
            new Vector2(v.X * a, v.Y * a);
        public static bool operator ==(Vector2 u, Vector2 v) =>
             u.X == v.X && u.Y == v.Y;
        public static bool operator !=(Vector2 u, Vector2 v) =>
             u.X != v.X || u.Y != v.Y;
        public override bool Equals(object o)
        {
            if (!(o is Vector2))
                return false;

            return this == (Vector2)o;
        }
        public override int GetHashCode()
        {
            return (X * 7 + Y * 13).GetHashCode();
        }

        public Vector2 UnitDirectionTo(Vector2 vec)
        {
            Vector2 dir = vec - this;
            return dir.UnitVector();
        }

        public Vector2 UnitVector()
        {
            if (Length == 0)
                return new Vector2(0, 0);
            else
                return this / Length;
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        float ITargetable.DistanceTo(Animal animal)
        {
            return (animal.Position - this).Length;
        }

        public float AngleTo(Vector2 v)
            => (float)Math.Atan2(v.Y - Y, v.X - X);
    }

    /// <summary>
    /// Represents float range [0,MaxValue].
    /// </summary>
    public struct FloatRange
    {
        /// <summary>
        /// Maximum value of value.
        /// </summary>
        public float MaxValue { get; }
        private float value;
        public float Value
        {
            get => value;
            set
            {
                value = Math.Max(0, Math.Min(MaxValue, value));
            }
        }

        public FloatRange(float max, float value)
        {
            MaxValue = max;
            this.value = Math.Max(0, Math.Min(MaxValue, value));
        }

        public static FloatRange operator +(FloatRange decR, float dec) 
            => new FloatRange(decR.MaxValue,decR.Value + dec);
        public static FloatRange operator -(FloatRange decR, float dec)
            => new FloatRange(decR.MaxValue, decR.Value - dec);
        public static FloatRange operator *(FloatRange decR, float dec)
            => new FloatRange(decR.MaxValue, decR.Value * dec);
        public static FloatRange operator /(FloatRange decR, float dec)
            => new FloatRange(decR.MaxValue, decR.Value / dec);
        public static bool operator ==(FloatRange decR, float dec)
           => decR.Value == dec;
        public static bool operator !=(FloatRange decR, float dec)
           => decR.Value != dec;
        public static bool operator ==(FloatRange decRa, FloatRange decRb)
           => decRa.Value == decRb.Value;
        public static bool operator !=(FloatRange decRa, FloatRange decRb)
           => decRa.Value != decRb.Value;
        public static bool operator <(FloatRange decR, float dec)
           => decR.Value < dec;
        public static bool operator >(FloatRange decR, float dec)
           => decR.Value > dec;
        public static bool operator <=(FloatRange decR, float dec)
           => decR.Value <= dec;
        public static bool operator >=(FloatRange decR, float dec)
           => decR.Value >= dec;
        public override bool Equals(object o)
        {
            if (o is FloatRange)
                return this == (FloatRange)o;
            else if (o is float)
                return this == (float)o;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public static implicit operator float(FloatRange decR)
          => decR.Value;

        public float Percentage => MaxValue!=0 ? Value / MaxValue : 0;
        public float AmountNotFilled => MaxValue - Value;
        public bool Full => Value == MaxValue;
        public override string ToString() => Value +"/"+MaxValue;
    }
}
