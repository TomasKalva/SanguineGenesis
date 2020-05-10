using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic
{
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

        float ITargetable.DistanceTo(Entity entity)
        {
            return (entity.Center - this).Length;
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
                this.value = Math.Max(0, Math.Min(MaxValue, value));
            }
        }

        public FloatRange(float max, float value)
        {
            MaxValue = max;
            this.value = Math.Max(0, Math.Min(MaxValue, value));
        }

        public static implicit operator FloatRange(float fl)
            => new FloatRange(fl, fl);
        public static FloatRange operator +(FloatRange flR, float fl)
            => new FloatRange(flR.MaxValue, flR.Value + fl);
        public static FloatRange operator -(FloatRange flR, float fl)
            => new FloatRange(flR.MaxValue, flR.Value - fl);
        public static FloatRange operator *(FloatRange flR, float fl)
            => new FloatRange(flR.MaxValue, flR.Value * fl);
        public static FloatRange operator /(FloatRange flR, float fl)
            => new FloatRange(flR.MaxValue, flR.Value / fl);
        public static bool operator ==(FloatRange flR, float fl)
           => flR.Value == fl;
        public static bool operator !=(FloatRange flR, float fl)
           => flR.Value != fl;
        public static bool operator ==(FloatRange flRa, FloatRange flRb)
           => flRa.Value == flRb.Value;
        public static bool operator !=(FloatRange flRa, FloatRange flRb)
           => flRa.Value != flRb.Value;
        public static bool operator <(FloatRange flR, float fl)
           => flR.Value < fl;
        public static bool operator >(FloatRange flR, float fl)
           => flR.Value > fl;
        public static bool operator <=(FloatRange flR, float fl)
           => flR.Value <= fl;
        public static bool operator >=(FloatRange flR, float fl)
           => flR.Value >= fl;
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
        public static implicit operator float(FloatRange flR)
          => flR.Value;

        public float Percentage => MaxValue != 0 ? Value / MaxValue : 0;
        public float AmountNotFilled => MaxValue - Value;
        public bool Full => Value == MaxValue;
        public override string ToString() => Value + "/" + MaxValue;
        public string ToString(string format) => Value.ToString(format) + "/" + MaxValue.ToString(format);
    }
}
