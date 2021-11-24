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
    /// Represents a four dimensional mathematical vector, composted of 2 signed integers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2I : IEquatable<Vector2I>
    {
        /// <summary>
        /// A point with (0,0) coordinates.
        /// </summary>
        public static readonly Vector2I Zero = new Vector2I(0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2I"/> struct using a packed <see cref="byte"/> containing two 4-bit values.
        /// </summary>
        /// <param name="packedValue">The packed <see cref="byte"/> containing the X and Y values.
        /// The lowest 4 bits provide the X value.
        /// The highest 4 bits provide the Y value.</param>
        public Vector2I(byte packedValue)
        {
            X = packedValue & 0xF;
            Y = (packedValue >> 4) & 0xF;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2I"/> struct using a packed <see cref="short"/> containing two 8-bit values.
        /// </summary>
        /// <param name="packedValue">The packed <see cref="short"/> containing the X and Y values.
        /// The lowest 8 bits provide the X value.
        /// The highest 8 bits provide the Y value.</param>
        public Vector2I(short packedValue)
        {
            X = packedValue & 0xFF;
            Y = (packedValue >> 8) & 0xFF;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2I"/> struct using a packed <see cref="int"/> containing two 16-bit values.
        /// </summary>
        /// <param name="packedValue">The packed <see cref="int"/> containing the X and Y values.
        /// The lowest 16 bits provide the X value.
        /// The highest 16 bits provide the Y value.</param>
        public Vector2I(int packedValue)
        {
            X = packedValue & 0xFFFF;
            Y = (packedValue >> 16) & 0xFFFF;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2I"/> struct using a packed <see cref="long"/> containing two 32-bit values.
        /// </summary>
        /// <param name="packedValue">The packed <see cref="long"/> containing the X and Y values. 
        /// The lowest 32 bits provide the X value.
        /// The highest 32 bits provide the Y value.</param>
        public Vector2I(long packedValue)
        {
            X = (int)(packedValue & 0xFFFFFFFF);
            Y = (int)((packedValue >> 32) & 0xFFFFFFFF);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2I"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Vector2I(int x, int y)
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
        public bool Equals(ref Vector2I other)
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
        public bool Equals(Vector2I other)
        {
            return Equals(ref other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if(!(obj is Vector2I))
                return false;

            var strongValue = (Vector2I)obj;
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
        /// Calculates the length of the vector, as a float value.
        /// </summary>
        /// <returns>The length of the vector.</returns>
        /// <remarks>
        /// <see cref="Vector2F.LengthSquared"/> may be preferred when only the relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public float Length()
        {
            return (float)Math.Sqrt((X * X) + (Y * Y));
        }

        /// <summary>
        /// Calculates the squared length of the vector.
        /// </summary>
        /// <returns>The squared length of the vector.</returns>
        /// <remarks>
        /// This method may be preferred to <see cref="Vector2F.Length"/> when only a relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public float LengthSquared()
        {
            return (X * X) + (Y * Y);
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
        public static bool operator ==(Vector2I left, Vector2I right)
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
        public static Vector2I operator *(Vector2I left, Vector2I right)
        {
            return new Vector2I(left.X * right.X, left.Y * right.Y);
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
        public static Vector2I operator /(Vector2I left, Vector2I right)
        {
            return new Vector2I(left.X / right.X, left.Y / right.Y);
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
        public static Vector2I operator /(Vector2I left, int right)
        {
            return new Vector2I(left.X / right, left.Y / right);
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
        public static Vector2I operator *(Vector2I left, int right)
        {
            return new Vector2I(left.X * right, left.Y * right);
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
        public static Vector2I operator *(Vector2I left, float right)
        {
            return new Vector2I((int)(left.X * right), (int)(left.Y * right));
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
        public static Vector2I operator +(Vector2I left, Vector2I right)
        {
            return new Vector2I(left.X + right.X, left.Y + right.Y);
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
        public static Vector2I operator +(Vector2I left, int right)
        {
            return new Vector2I(left.X + right, left.Y + right);
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
        public static Vector2I operator -(Vector2I value)
        {
            return new Vector2I(-value.X, -value.Y);
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
        public static Vector2I operator -(Vector2I left, Vector2I right)
        {
            return new Vector2I(left.X - right.X, left.Y - right.Y);
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
        public static bool operator !=(Vector2I left, Vector2I right)
        {
            return !left.Equals(ref right);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", X, Y);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Vector2F"/> to <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Vector2I(Vector2F value)
        {
            return new Vector2I((int)value.X, (int)value.Y);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Vector2I"/> to <see cref="Vector2F"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector2F(Vector2I value)
        {
            return new Vector2F(value.X, value.Y);
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4F"/>.</param>
        public static void Transform(ref Vector2I vector, ref Matrix4F transform, out Vector4I result)
        {
            result = new Vector4I(
                (int)((vector.X * transform.M11) + (vector.Y * transform.M21) + transform.M41),
                (int)((vector.X * transform.M12) + (vector.Y * transform.M22) + transform.M42),
                (int)((vector.X * transform.M13) + (vector.Y * transform.M23) + transform.M43),
                (int)((vector.X * transform.M14) + (vector.Y * transform.M24) + transform.M44));
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <returns>The transformed <see cref="Vector4F"/>.</returns>
        public static Vector4I Transform(Vector2I vector, Matrix4F transform)
        {
            Vector4I result;
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
        /// <see cref="Vector2I.DistanceSquared(ref Vector2I, ref Vector2I, out float)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static void Distance(ref Vector2I value1, ref Vector2I value2, out float result)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;

            result = (float)Math.Sqrt((x * x) + (y * y));
        }

        /// <summary>
        /// Calculates the squared distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector</param>
        /// <param name="result">When the method completes, contains the squared distance between the two vectors.</param>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
        public static void DistanceSquared(ref Vector2I value1, ref Vector2I value2, out float result)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

        /// <summary>
        /// Calculates the squared distance between two vectors, as a float value.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between the two vectors.</returns>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
        public static float DistanceSquared(Vector2I value1, Vector2I value2)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

        /// <summary>
        /// Calculates the distance between two vectors, as a float value.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector2I.DistanceSquared(Vector2I, Vector2I)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static float Distance(Vector2I value1, Vector2I value2)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;

            return (float)Math.Sqrt((x * x) + (y * y));
        }

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector2I"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector2I"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
        /// <param name="xIndex">The axis index to use for the new X value.</param>
        /// <param name="yIndex">The axis index to use for the new Y value.</param>
        /// <returns></returns>
        public static unsafe Vector2I Swizzle(Vector2I val, int xIndex, int yIndex)
        {
            return new Vector2I()
            {
                X = *(&val.X + (xIndex * sizeof(int))),
                Y = *(&val.X + (yIndex * sizeof(int))),
            };
        }
    }
}