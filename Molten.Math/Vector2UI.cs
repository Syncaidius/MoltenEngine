using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten
{
    /// <summary>
    /// Represents a four dimensional mathematical vector, composted of 2 unsigned integers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2UI : IEquatable<Vector2UI>
    {
        /// <summary>
        /// A point with (0,0) coordinates.
        /// </summary>
        public static readonly Vector2UI Zero = new Vector2UI(0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2UI"/> struct using a packed <see cref="byte"/> containing two 4-bit values.
        /// </summary>
        /// <param name="packedValue">The packed <see cref="byte"/> containing the X and Y values.</param>
        public Vector2UI(byte packedValue)
        {
            X = (uint)packedValue & 0xF;
            Y = (uint)(packedValue >> 4) & 0xF;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2F"/> struct using a packed <see cref="short"/> containing two 8-bit values.
        /// </summary>
        /// <param name="packedValue">The packed <see cref="short"/> containing the X and Y values.</param>
        public Vector2UI(ushort packedValue)
        {
            X = (uint)packedValue & 0xFF;
            Y = (uint)(packedValue >> 8) & 0xFF;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2F"/> struct using a packed <see cref="int"/> containing two 16-bit values.
        /// </summary>
        /// <param name="packedValue">The packed <see cref="int"/> containing the X and Y values.</param>
        public Vector2UI(uint packedValue)
        {
            X = packedValue & 0xFFFF;
            Y = (packedValue >> 16) & 0xFFFF;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2F"/> struct using a packed <see cref="long"/> containing two 32-bit values.
        /// </summary>
        /// <param name="packedValue">The packed <see cref="long"/> containing the X and Y values.</param>
        public Vector2UI(ulong packedValue)
        {
            X = (uint)(packedValue & 0xFFFFFFFF);
            Y = (uint)((packedValue >> 32) & 0xFFFFFFFF);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2UI"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Vector2UI(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Left coordinate.
        /// </summary>
        public uint X;

        /// <summary>
        /// Top coordinate.
        /// </summary>
        public uint Y;

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector2UI other)
        {
            return other.X == X && other.Y == Y;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2UI other)
        {
            return Equals(ref other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if(!(obj is Vector2UI))
                return false;

            var strongValue = (Vector2UI)obj;
            return Equals(ref strongValue);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (int)((long)((X * 397) ^ Y) - int.MaxValue);
            }
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", X, Y);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Vector2F"/> to <see cref="Vector2UI"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Vector2UI(Vector2F value)
        {
            return new Vector2UI((uint)value.X, (uint)value.Y);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Vector2UI"/> to <see cref="Vector2F"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector2F(Vector2UI value)
        {
            return new Vector2F(value.X, value.Y);
        }

        public static implicit operator Vector2I(Vector2UI value)
        {
            return new Vector2I((int)value.X, (int)value.Y);
        }

        public static explicit operator Vector2UI(Vector2I value)
        {
            return new Vector2UI((uint)value.X, (uint)value.Y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector2UI left, Vector2UI right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector2UI left, Vector2UI right)
        {
            return !left.Equals(ref right);
        }

        public static Vector2UI operator /(Vector2UI left, Vector2UI right)
        {
            return new Vector2UI(left.X / right.X, left.Y / right.Y);
        }

        public static Vector2UI operator /(Vector2UI left, float right)
        {
            return new Vector2UI(left.X / (uint)right, left.Y * (uint)right);
        }

        public static Vector2UI operator /(Vector2UI left, uint right)
        {
            return new Vector2UI(left.X / right, left.Y / right);
        }

        public static Vector2UI operator +(Vector2UI left, Vector2UI right)
        {
            return new Vector2UI(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2UI operator +(Vector2UI left, float right)
        {
            return new Vector2UI(left.X + (uint)right, left.Y + (uint)right);
        }

        public static Vector2UI operator +(Vector2UI left, uint right)
        {
            return new Vector2UI(left.X + right, left.Y + right);
        }

        public static Vector2UI operator -(Vector2UI left, Vector2UI right)
        {
            return new Vector2UI(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2UI operator -(Vector2UI left, float right)
        {
            return new Vector2UI(left.X - (uint)right, left.Y - (uint)right);
        }

        public static Vector2UI operator -(Vector2UI left, uint right)
        {
            return new Vector2UI(left.X - right, left.Y - right);
        }

        public static Vector2UI operator *(Vector2UI left, Vector2UI right)
        {
            return new Vector2UI(left.X * right.X, left.Y * right.Y);
        }

        public static Vector2UI operator *(Vector2UI left, float right)
        {
            return new Vector2UI(left.X * (uint)right, left.Y * (uint)right);
        }

        public static Vector2UI operator *(Vector2UI left, uint right)
        {
            return new Vector2UI(left.X * right, left.Y * right);
        }

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector2UI"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector2UI"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
        /// <param name="xIndex">The axis index to use for the new X value.</param>
        /// <param name="yIndex">The axis index to use for the new Y value.</param>
        /// <returns></returns>
        public static unsafe Vector2UI Swizzle(Vector2UI val, uint xIndex, uint yIndex)
        {
            return new Vector2UI()
            {
                X = *(&val.X + (xIndex * sizeof(uint))),
                Y = *(&val.X + (yIndex * sizeof(uint))),
            };
        }
    }
}