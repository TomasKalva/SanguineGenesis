using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;

namespace wpfTest
{
    public class FlowMap:IMap<float>
    {
        public const float MIN_VALID_VALUE= -0.01f;

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
                    this[i, j] = -10.57f;
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
                !IsValidValue(directions[i,j]))
                return new Vector2(0f, 0f);

            float angle = directions[i, j];
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

        public static bool IsValidValue(float val) => val >= MIN_VALID_VALUE;
    }

    public struct Vector2: ITargetable, IMovementTarget
    {
        Vector2 ITargetable.Center => this;

        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 operator +(Vector2 u, Vector2 v)=>
            new Vector2(u.X + v.X, u.Y + v.Y);
        public static Vector2 operator -(Vector2 u, Vector2 v) =>
            new Vector2(u.X - v.X, u.Y - v.Y);
        public static Vector2 operator /(Vector2 v, float a) =>
            new Vector2(v.X/a,v.Y/a);
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
            if (dir.Length != 0)
                return dir / dir.Length;
            else
                return new Vector2(0, 0);
        }

        public float Length =>(float)Math.Sqrt(X * X + Y * Y);
    }

    /// <summary>
    /// Represents decimal range [0,MaxValue].
    /// </summary>
    public struct DecRange
    {
        /// <summary>
        /// Maximum value of value.
        /// </summary>
        public decimal MaxValue { get; }
        private decimal value;
        public decimal Value
        {
            get => value;
            set
            {
                value = Math.Max(0, Math.Min(MaxValue, value));
            }
        }

        public DecRange(decimal max, decimal value)
        {
            MaxValue = max;
            this.value = Math.Max(0, Math.Min(MaxValue, value));
        }

        public static DecRange operator +(DecRange decR, decimal dec) 
            => new DecRange(decR.MaxValue,decR.Value + dec);
        public static DecRange operator -(DecRange decR, decimal dec)
            => new DecRange(decR.MaxValue, decR.Value - dec);
        public static DecRange operator *(DecRange decR, decimal dec)
            => new DecRange(decR.MaxValue, decR.Value * dec);
        public static DecRange operator /(DecRange decR, decimal dec)
            => new DecRange(decR.MaxValue, decR.Value / dec);
        public static bool operator ==(DecRange decR, decimal dec)
           => decR.Value == dec;
        public static bool operator !=(DecRange decR, decimal dec)
           => decR.Value != dec;
        public static bool operator ==(DecRange decRa, DecRange decRb)
           => decRa.Value == decRb.Value;
        public static bool operator !=(DecRange decRa, DecRange decRb)
           => decRa.Value != decRb.Value;
        public static bool operator <(DecRange decR, decimal dec)
           => decR.Value < dec;
        public static bool operator >(DecRange decR, decimal dec)
           => decR.Value > dec;
        public static bool operator <=(DecRange decR, decimal dec)
           => decR.Value <= dec;
        public static bool operator >=(DecRange decR, decimal dec)
           => decR.Value >= dec;
        public override bool Equals(object o)
        {
            if (o is DecRange)
                return this == (DecRange)o;
            else if (o is decimal)
                return this == (decimal)o;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public static implicit operator decimal(DecRange decR)
          => decR.Value;

        public decimal Percentage => MaxValue!=0 ? Value / MaxValue : 0;
        public decimal NotFilled => MaxValue - Value;
        public override string ToString() => Value.ToString();
    }
}
