// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten
{
    /// <summary>
    /// Structure using the same layout than <see cref="System.Drawing.Point"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct IntVector2 : IEquatable<IntVector2>
    {
        /// <summary>
        /// A point with (0,0) coordinates.
        /// </summary>
        public static readonly IntVector2 Zero = new IntVector2(0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="IntVector2"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public IntVector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Left coordinate.
        /// </summary>
        public int X;

        /// <summary>
        /// Top coordinate.
        /// </summary>
        public int Y;

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref IntVector2 other)
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
        public bool Equals(IntVector2 other)
        {
            return Equals(ref other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if(!(obj is IntVector2))
                return false;

            var strongValue = (IntVector2)obj;
            return Equals(ref strongValue);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
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
        public static bool operator ==(IntVector2 left, IntVector2 right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Implements the operator *
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVector2 operator *(IntVector2 left, IntVector2 right)
        {
            return new IntVector2(left.X * right.X, left.Y * right.Y);
        }

        /// <summary>
        /// Implements the operator /
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVector2 operator /(IntVector2 left, IntVector2 right)
        {
            return new IntVector2(left.X / right.X, left.Y / right.Y);
        }

        /// <summary>
        /// Implements the operator /
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVector2 operator /(IntVector2 left, int right)
        {
            return new IntVector2(left.X / right, left.Y / right);
        }

        /// <summary>
        /// Implements the operator *
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVector2 operator *(IntVector2 left, int right)
        {
            return new IntVector2(left.X * right, left.Y * right);
        }

        /// <summary>
        /// Implements the operator *
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVector2 operator *(IntVector2 left, float right)
        {
            return new IntVector2((int)(left.X * right), (int)(left.Y * right));
        }

        /// <summary>
        /// Implements the operator +
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVector2 operator +(IntVector2 left, IntVector2 right)
        {
            return new IntVector2(left.X + right.X, left.Y + right.Y);
        }

        /// <summary>
        /// Implements the operator +
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVector2 operator +(IntVector2 left, int right)
        {
            return new IntVector2(left.X + right, left.Y + right);
        }

        /// <summary>
        /// Implements the operator negate operator.
        /// </summary>
        /// <param name="value">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVector2 operator -(IntVector2 value)
        {
            return new IntVector2(-value.X, -value.Y);
        }

        /// <summary>
        /// Implements the operator -
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVector2 operator -(IntVector2 left, IntVector2 right)
        {
            return new IntVector2(left.X - right.X, left.Y - right.Y);
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
        public static bool operator !=(IntVector2 left, IntVector2 right)
        {
            return !left.Equals(ref right);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", X, Y);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Vector2"/> to <see cref="IntVector2"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator IntVector2(Vector2 value)
        {
            return new IntVector2((int)value.X, (int)value.Y);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="IntVector2"/> to <see cref="Vector2"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector2(IntVector2 value)
        {
            return new Vector2(value.X, value.Y);
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="Matrix"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4"/>.</param>
        public static void Transform(ref IntVector2 vector, ref Matrix transform, out IntVector4 result)
        {
            result = new IntVector4(
                (int)((vector.X * transform.M11) + (vector.Y * transform.M21) + transform.M41),
                (int)((vector.X * transform.M12) + (vector.Y * transform.M22) + transform.M42),
                (int)((vector.X * transform.M13) + (vector.Y * transform.M23) + transform.M43),
                (int)((vector.X * transform.M14) + (vector.Y * transform.M24) + transform.M44));
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="Matrix"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix"/>.</param>
        /// <returns>The transformed <see cref="Vector4"/>.</returns>
        public static IntVector4 Transform(IntVector2 vector, Matrix transform)
        {
            IntVector4 result;
            Transform(ref vector, ref transform, out result);
            return result;
        }


        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">When the method completes, contains the distance between the two vectors.</param>
        /// <remarks>
        /// <see cref="Vector2.DistanceSquared(ref Vector2, ref Vector2, out float)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static void Distance(ref IntVector2 value1, ref IntVector2 value2, out float result)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;

            result = (float)Math.Sqrt((x * x) + (y * y));
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector2.DistanceSquared(Vector2, Vector2)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static float Distance(IntVector2 value1, IntVector2 value2)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;

            return (float)Math.Sqrt((x * x) + (y * y));
        }
    }
}