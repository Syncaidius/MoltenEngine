﻿// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten
{
    /// <summary>
    /// Represents a four dimensional mathematical vector, composted of 3 signed integers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector3I : IEquatable<Vector3I>, IFormattable
    {
        /// <summary>
        /// The size of the <see cref = "Vector3I" /> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof (Vector3I));

        /// <summary>
        /// A <see cref = "Vector3I" /> with all of its components set to zero.
        /// </summary>
        public static readonly Vector3I Zero = new Vector3I();

        /// <summary>
        /// The X unit <see cref = "Vector3I" /> (1, 0, 0).
        /// </summary>
        public static readonly Vector3I UnitX = new Vector3I(1, 0, 0);

        /// <summary>
        /// The Y unit <see cref = "Vector3I" /> (0, 1, 0).
        /// </summary>
        public static readonly Vector3I UnitY = new Vector3I(0, 1, 0);

        /// <summary>
        /// The Z unit <see cref = "Vector3I" /> (0, 0, 1).
        /// </summary>
        public static readonly Vector3I UnitZ = new Vector3I(0, 0, 1);

        /// <summary>
        /// A <see cref = "Vector3I" /> with all of its components set to one.
        /// </summary>
        public static readonly Vector3I One = new Vector3I(1, 1, 1);

        /// <summary>
        /// The X component of the vector.
        /// </summary>
        public int X;

        /// <summary>
        /// The Y component of the vector.
        /// </summary>
        public int Y;

        /// <summary>
        /// The Z component of the vector.
        /// </summary>
        public int Z;

        /// <summary>
        /// Initializes a new instance of the <see cref = "Vector3I" /> struct.
        /// </summary>
        /// <param name = "value">The value that will be assigned to all components.</param>
        public Vector3I(int value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref = "Vector3I" /> struct.
        /// </summary>
        /// <param name = "x">Initial value for the X component of the vector.</param>
        /// <param name = "y">Initial value for the Y component of the vector.</param>
        /// <param name = "z">Initial value for the Z component of the vector.</param>
        public Vector3I(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref = "Vector3I" /> struct.
        /// </summary>
        /// <param name = "values">The values to assign to the X, Y, Z, and W components of the vector. This must be an array with four elements.</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "values" /> is <c>null</c>.</exception>
        /// <exception cref = "ArgumentOutOfRangeException">Thrown when <paramref name = "values" /> contains more or less than four elements.</exception>
        public Vector3I(int[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values",
                                                      "There must be three and only three input values for Int3.");

            X = values[0];
            Y = values[1];
            Z = values[2];
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, Z, or W component, depending on the index.</value>
        /// <param name = "index">The index of the component to access. Use 0 for the X component, 1 for the Y component, 2 for the Z component, and 3 for the W component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref = "System.ArgumentOutOfRangeException">Thrown when the <paramref name = "index" /> is out of the range [0, 3].</exception>
        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                }

                throw new ArgumentOutOfRangeException("index", "Indices for Int3 run from 0 to 2, inclusive.");
            }

            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for Int3 run from 0 to 2, inclusive.");
                }
            }
        }

        /// <summary>
        /// Creates an array containing the elements of the vector.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public int[] ToArray()
        {
            return new int[] {X, Y, Z};
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name = "left">The first vector to add.</param>
        /// <param name = "right">The second vector to add.</param>
        /// <param name = "result">When the method completes, contains the sum of the two vectors.</param>
        public static void Add(ref Vector3I left, ref Vector3I right, out Vector3I result)
        {
            result = new Vector3I(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name = "left">The first vector to add.</param>
        /// <param name = "right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector3I Add(Vector3I left, Vector3I right)
        {
            return new Vector3I(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name = "left">The first vector to subtract.</param>
        /// <param name = "right">The second vector to subtract.</param>
        /// <param name = "result">When the method completes, contains the difference of the two vectors.</param>
        public static void Subtract(ref Vector3I left, ref Vector3I right, out Vector3I result)
        {
            result = new Vector3I(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name = "left">The first vector to subtract.</param>
        /// <param name = "right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector3I Subtract(Vector3I left, Vector3I right)
        {
            return new Vector3I(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <param name = "result">When the method completes, contains the scaled vector.</param>
        public static void Multiply(ref Vector3I value, int scale, out Vector3I result)
        {
            result = new Vector3I(value.X*scale, value.Y*scale, value.Z*scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector3I Multiply(Vector3I value, int scale)
        {
            return new Vector3I(value.X*scale, value.Y*scale, value.Z*scale);
        }

        /// <summary>
        /// Modulates a vector with another by performing component-wise multiplication.
        /// </summary>
        /// <param name = "left">The first vector to modulate.</param>
        /// <param name = "right">The second vector to modulate.</param>
        /// <param name = "result">When the method completes, contains the modulated vector.</param>
        public static void Modulate(ref Vector3I left, ref Vector3I right, out Vector3I result)
        {
            result = new Vector3I(left.X*right.X, left.Y*right.Y, left.Z*right.Z);
        }

        /// <summary>
        /// Modulates a vector with another by performing component-wise multiplication.
        /// </summary>
        /// <param name = "left">The first vector to modulate.</param>
        /// <param name = "right">The second vector to modulate.</param>
        /// <returns>The modulated vector.</returns>
        public static Vector3I Modulate(Vector3I left, Vector3I right)
        {
            return new Vector3I(left.X*right.X, left.Y*right.Y, left.Z*right.Z);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <param name = "result">When the method completes, contains the scaled vector.</param>
        public static void Divide(ref Vector3I value, int scale, out Vector3I result)
        {
            result = new Vector3I(value.X/scale, value.Y/scale, value.Z/scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector3I Divide(Vector3I value, int scale)
        {
            return new Vector3I(value.X/scale, value.Y/scale, value.Z/scale);
        }

        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name = "value">The vector to negate.</param>
        /// <param name = "result">When the method completes, contains a vector facing in the opposite direction.</param>
        public static void Negate(ref Vector3I value, out Vector3I result)
        {
            result = new Vector3I(-value.X, -value.Y, -value.Z);
        }

        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name = "value">The vector to negate.</param>
        /// <returns>A vector facing in the opposite direction.</returns>
        public static Vector3I Negate(Vector3I value)
        {
            return new Vector3I(-value.X, -value.Y, -value.Z);
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name = "value">The value to clamp.</param>
        /// <param name = "min">The minimum value.</param>
        /// <param name = "max">The maximum value.</param>
        /// <param name = "result">When the method completes, contains the clamped value.</param>
        public static void Clamp(ref Vector3I value, ref Vector3I min, ref Vector3I max, out Vector3I result)
        {
            int x = value.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            int y = value.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            int z = value.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;

            result = new Vector3I(x, y, z);
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name = "value">The value to clamp.</param>
        /// <param name = "min">The minimum value.</param>
        /// <param name = "max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static Vector3I Clamp(Vector3I value, Vector3I min, Vector3I max)
        {
            Vector3I result;
            Clamp(ref value, ref min, ref max, out result);
            return result;
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name = "left">The first source vector.</param>
        /// <param name = "right">The second source vector.</param>
        /// <param name = "result">When the method completes, contains an new vector composed of the largest components of the source vectors.</param>
        public static void Max(ref Vector3I left, ref Vector3I right, out Vector3I result)
        {
            result.X = (left.X > right.X) ? left.X : right.X;
            result.Y = (left.Y > right.Y) ? left.Y : right.Y;
            result.Z = (left.Z > right.Z) ? left.Z : right.Z;
        }

        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name = "left">The first source vector.</param>
        /// <param name = "right">The second source vector.</param>
        /// <returns>A vector containing the largest components of the source vectors.</returns>
        public static Vector3I Max(Vector3I left, Vector3I right)
        {
            Vector3I result;
            Max(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name = "left">The first source vector.</param>
        /// <param name = "right">The second source vector.</param>
        /// <param name = "result">When the method completes, contains an new vector composed of the smallest components of the source vectors.</param>
        public static void Min(ref Vector3I left, ref Vector3I right, out Vector3I result)
        {
            result.X = (left.X < right.X) ? left.X : right.X;
            result.Y = (left.Y < right.Y) ? left.Y : right.Y;
            result.Z = (left.Z < right.Z) ? left.Z : right.Z;
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name = "left">The first source vector.</param>
        /// <param name = "right">The second source vector.</param>
        /// <returns>A vector containing the smallest components of the source vectors.</returns>
        public static Vector3I Min(Vector3I left, Vector3I right)
        {
            Vector3I result;
            Min(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Calculates the distance between two vectors, as a float value.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">When the method completes, contains the distance between the two vectors.</param>
        /// <remarks>
        /// <see cref="Vector3I.DistanceSquared(ref Vector3I, ref Vector3I, out float)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static void Distance(ref Vector3I value1, ref Vector3I value2, out float result)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;
            float z = value1.Z - value2.Z;

            result = (float)Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Calculates the distance between two vectors, as a float value.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector3I.DistanceSquared(Vector3I, Vector3I)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static float Distance(Vector3I value1, Vector3I value2)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;
            float z = value1.Z - value2.Z;

            return (float)Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Calculates the squared distance between two vectors, as a float value.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">When the method completes, contains the squared distance between the two vectors.</param>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
        public static void DistanceSquared(ref Vector3I value1, ref Vector3I value2, out float result)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;
            float z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
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
        public static float DistanceSquared(Vector3I value1, Vector3I value2)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;
            float z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name = "left">The first vector to add.</param>
        /// <param name = "right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector3I operator +(Vector3I left, Vector3I right)
        {
            return new Vector3I(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        /// <summary>
        /// Assert a vector (return it unchanged).
        /// </summary>
        /// <param name = "value">The vector to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) vector.</returns>
        public static Vector3I operator +(Vector3I value)
        {
            return value;
        }

        public static Vector3I operator +(Vector3I left, int right)
        {
            return new Vector3I(left.X + right, left.Y + right, left.Z + right);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name = "left">The first vector to subtract.</param>
        /// <param name = "right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector3I operator -(Vector3I left, Vector3I right)
        {
            return new Vector3I(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name = "value">The vector to negate.</param>
        /// <returns>A vector facing in the opposite direction.</returns>
        public static Vector3I operator -(Vector3I value)
        {
            return new Vector3I(-value.X, -value.Y, -value.Z);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector3I operator *(int scale, Vector3I value)
        {
            return new Vector3I(value.X*scale, value.Y*scale, value.Z*scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector3I operator *(Vector3I value, int scale)
        {
            return new Vector3I(value.X*scale, value.Y*scale, value.Z*scale);
        }

        public static Vector3I operator *(Vector3I value, Vector3I scale)
        {
            return new Vector3I(value.X * scale.X, value.Y * scale.Y, value.Z * scale.Z);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector3I operator /(Vector3I value, int scale)
        {
            return new Vector3I(value.X/scale, value.Y/scale, value.Z/scale);
        }

        public static Vector3I operator /(Vector3I left, Vector3I right)
        {
            return new Vector3I(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name = "left">The first value to compare.</param>
        /// <param name = "right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name = "left" /> has the same value as <paramref name = "right" />; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3I left, Vector3I right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name = "left">The first value to compare.</param>
        /// <param name = "right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name = "left" /> has a different value than <paramref name = "right" />; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3I left, Vector3I right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref = "Vector3I" /> to <see cref = "Vector2F" />.
        /// </summary>
        /// <param name = "value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Vector2F(Vector3I value)
        {
            return new Vector2F(value.X, value.Y);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref = "Vector3I" /> to <see cref = "Vector3F" />.
        /// </summary>
        /// <param name = "value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector3F(Vector3I value)
        {
            return new Vector3F(value.X, value.Y, value.Z);
        }

        /// <summary>
        /// Returns a <see cref = "System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref = "System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

        /// <summary>
        /// Returns a <see cref = "System.String" /> that represents this instance.
        /// </summary>
        /// <param name = "format">The format.</param>
        /// <returns>
        /// A <see cref = "System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}",
                                 X.ToString(format, CultureInfo.CurrentCulture),
                                 Y.ToString(format, CultureInfo.CurrentCulture),
                                 Z.ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a <see cref = "System.String" /> that represents this instance.
        /// </summary>
        /// <param name = "formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref = "System.String" /> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

        /// <summary>
        /// Returns a <see cref = "System.String" /> that represents this instance.
        /// </summary>
        /// <param name = "format">The format.</param>
        /// <param name = "formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref = "System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X.ToString(format, formatProvider),
                                 Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref = "Vector3I" /> is equal to this instance.
        /// </summary>
        /// <param name = "other">The <see cref = "Vector3I" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref = "Vector3I" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector3I other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        /// <summary>
        /// Determines whether the specified <see cref = "Vector3I" /> is equal to this instance.
        /// </summary>
        /// <param name = "other">The <see cref = "Vector3I" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref = "Vector3I" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3I other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref = "System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name = "value">The <see cref = "System.Object" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref = "System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (!(value is Vector3I))
                return false;

            var strongValue = (Vector3I)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="int"/> array to <see cref="Vector3I"/>.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector3I(int[] input)
        {
            return new Vector3I(input);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Vector3I"/> to <see cref="System.Int32"/> array.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator int[](Vector3I input)
        {
            return input.ToArray();
        }

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector3I"/>. <para />
        /// For example, a swizzle input of (1,1, 2) on a <see cref="Vector3I"/> with the values, 20, 5 and 10, will return a vector with values 5,5,10."
        /// </summary>
        /// <param name="val">The current vector.</param>
        /// <param name="xIndex">The axis index to use for the new X value.</param>
        /// <param name="yIndex">The axis index to use for the new Y value.</param>
        /// <param name="zIndex">The axis index to use for the new Z value.</param>
        /// <returns></returns>
        public static unsafe Vector3I Swizzle(Vector3I val, int xIndex, int yIndex, int zIndex)
        {
            return new Vector3I()
            {
                X = *(&val.X + (xIndex * sizeof(int))),
                Y = *(&val.X + (yIndex * sizeof(int))),
                Z = *(&val.X + (zIndex * sizeof(int))),
            };
        }
    }
}
