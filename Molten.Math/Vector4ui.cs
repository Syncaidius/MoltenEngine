using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten
{
    /// <summary>
    /// Represents a four dimensional mathematical vector, composed of 4 unsigned integers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector4UI : IEquatable<Vector4UI>, IFormattable
    {
        /// <summary>
        /// The size of the <see cref = "Vector4UI" /> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof (Vector4UI));

        /// <summary>
        /// A <see cref = "Vector4UI" /> with all of its components set to zero.
        /// </summary>
        public static readonly Vector4UI Zero = new Vector4UI();

        /// <summary>
        /// The X unit <see cref = "Vector4UI" /> (1, 0, 0, 0).
        /// </summary>
        public static readonly Vector4UI UnitX = new Vector4UI(1, 0, 0, 0);

        /// <summary>
        /// The Y unit <see cref = "Vector4UI" /> (0, 1, 0, 0).
        /// </summary>
        public static readonly Vector4UI UnitY = new Vector4UI(0, 1, 0, 0);

        /// <summary>
        /// The Z unit <see cref = "Vector4UI" /> (0, 0, 1, 0).
        /// </summary>
        public static readonly Vector4UI UnitZ = new Vector4UI(0, 0, 1, 0);

        /// <summary>
        /// The W unit <see cref = "Vector4UI" /> (0, 0, 0, 1).
        /// </summary>
        public static readonly Vector4UI UnitW = new Vector4UI(0, 0, 0, 1);

        /// <summary>
        /// A <see cref = "Vector4UI" /> with all of its components set to one.
        /// </summary>
        public static readonly Vector4UI One = new Vector4UI(1, 1, 1, 1);

        /// <summary>
        /// The X component of the vector.
        /// </summary>
        public uint X;

        /// <summary>
        /// The Y component of the vector.
        /// </summary>
        public uint Y;

        /// <summary>
        /// The Z component of the vector.
        /// </summary>
        public uint Z;

        /// <summary>
        /// The W component of the vector.
        /// </summary>
        public uint W;

        /// <summary>
        /// Initializes a new instance of the <see cref = "Vector4UI" /> struct.
        /// </summary>
        /// <param name = "value">The value that will be assigned to all components.</param>
        public Vector4UI(uint value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref = "Vector4UI" /> struct.
        /// </summary>
        /// <param name = "x">Initial value for the X component of the vector.</param>
        /// <param name = "y">Initial value for the Y component of the vector.</param>
        /// <param name = "z">Initial value for the Z component of the vector.</param>
        /// <param name = "w">Initial value for the W component of the vector.</param>
        public Vector4UI(uint x, uint y, uint z, uint w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref = "Vector4UI" /> struct.
        /// </summary>
        /// <param name = "values">The values to assign to the X, Y, Z, and W components of the vector. This must be an array with four elements.</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "values" /> is <c>null</c>.</exception>
        /// <exception cref = "ArgumentOutOfRangeException">Thrown when <paramref name = "values" /> contains more or less than four elements.</exception>
        public Vector4UI(uint[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values",
                                                      "There must be four and only four input values for Int4.");

            X = values[0];
            Y = values[1];
            Z = values[2];
            W = values[3];
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, Z, or W component, depending on the index.</value>
        /// <param name = "index">The index of the component to access. Use 0 for the X component, 1 for the Y component, 2 for the Z component, and 3 for the W component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref = "System.ArgumentOutOfRangeException">Thrown when the <paramref name = "index" /> is out of the range [0, 3].</exception>
        public uint this[int index]
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
                    case 3:
                        return W;
                }

                throw new ArgumentOutOfRangeException("index", "Indices for Int4 run from 0 to 3, inclusive.");
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
                    case 3:
                        W = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for Int4 run from 0 to 3, inclusive.");
                }
            }
        }

        /// <summary>
        /// Creates an array containing the elements of the vector.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public uint[] ToArray()
        {
            return new uint[] {X, Y, Z, W};
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name = "left">The first vector to add.</param>
        /// <param name = "right">The second vector to add.</param>
        /// <param name = "result">When the method completes, contains the sum of the two vectors.</param>
        public static void Add(ref Vector4UI left, ref Vector4UI right, out Vector4UI result)
        {
            result = new Vector4UI(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name = "left">The first vector to add.</param>
        /// <param name = "right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector4UI Add(Vector4UI left, Vector4UI right)
        {
            return new Vector4UI(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name = "left">The first vector to subtract.</param>
        /// <param name = "right">The second vector to subtract.</param>
        /// <param name = "result">When the method completes, contains the difference of the two vectors.</param>
        public static void Subtract(ref Vector4UI left, ref Vector4UI right, out Vector4UI result)
        {
            result = new Vector4UI(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name = "left">The first vector to subtract.</param>
        /// <param name = "right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector4UI Subtract(Vector4UI left, Vector4UI right)
        {
            return new Vector4UI(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <param name = "result">When the method completes, contains the scaled vector.</param>
        public static void Multiply(ref Vector4UI value, uint scale, out Vector4UI result)
        {
            result = new Vector4UI(value.X*scale, value.Y*scale, value.Z*scale, value.W*scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4UI Multiply(Vector4UI value, uint scale)
        {
            return new Vector4UI(value.X*scale, value.Y*scale, value.Z*scale, value.W*scale);
        }

        /// <summary>
        /// Modulates a vector with another by performing component-wise multiplication.
        /// </summary>
        /// <param name = "left">The first vector to modulate.</param>
        /// <param name = "right">The second vector to modulate.</param>
        /// <param name = "result">When the method completes, contains the modulated vector.</param>
        public static void Modulate(ref Vector4UI left, ref Vector4UI right, out Vector4UI result)
        {
            result = new Vector4UI(left.X*right.X, left.Y*right.Y, left.Z*right.Z, left.W*right.W);
        }

        /// <summary>
        /// Modulates a vector with another by performing component-wise multiplication.
        /// </summary>
        /// <param name = "left">The first vector to modulate.</param>
        /// <param name = "right">The second vector to modulate.</param>
        /// <returns>The modulated vector.</returns>
        public static Vector4UI Modulate(Vector4UI left, Vector4UI right)
        {
            return new Vector4UI(left.X*right.X, left.Y*right.Y, left.Z*right.Z, left.W*right.W);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <param name = "result">When the method completes, contains the scaled vector.</param>
        public static void Divide(ref Vector4UI value, uint scale, out Vector4UI result)
        {
            result = new Vector4UI(value.X/scale, value.Y/scale, value.Z/scale, value.W/scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4UI Divide(Vector4UI value, uint scale)
        {
            return new Vector4UI(value.X/scale, value.Y/scale, value.Z/scale, value.W/scale);
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name = "value">The value to clamp.</param>
        /// <param name = "min">The minimum value.</param>
        /// <param name = "max">The maximum value.</param>
        /// <param name = "result">When the method completes, contains the clamped value.</param>
        public static void Clamp(ref Vector4UI value, ref Vector4UI min, ref Vector4UI max, out Vector4UI result)
        {
            uint x = value.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            uint y = value.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            uint z = value.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;

            uint w = value.W;
            w = (w > max.W) ? max.W : w;
            w = (w < min.W) ? min.W : w;

            result = new Vector4UI(x, y, z, w);
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name = "value">The value to clamp.</param>
        /// <param name = "min">The minimum value.</param>
        /// <param name = "max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static Vector4UI Clamp(Vector4UI value, Vector4UI min, Vector4UI max)
        {
            Vector4UI result;
            Clamp(ref value, ref min, ref max, out result);
            return result;
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name = "left">The first source vector.</param>
        /// <param name = "right">The second source vector.</param>
        /// <param name = "result">When the method completes, contains an new vector composed of the largest components of the source vectors.</param>
        public static void Max(ref Vector4UI left, ref Vector4UI right, out Vector4UI result)
        {
            result.X = (left.X > right.X) ? left.X : right.X;
            result.Y = (left.Y > right.Y) ? left.Y : right.Y;
            result.Z = (left.Z > right.Z) ? left.Z : right.Z;
            result.W = (left.W > right.W) ? left.W : right.W;
        }

        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name = "left">The first source vector.</param>
        /// <param name = "right">The second source vector.</param>
        /// <returns>A vector containing the largest components of the source vectors.</returns>
        public static Vector4UI Max(Vector4UI left, Vector4UI right)
        {
            Vector4UI result;
            Max(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name = "left">The first source vector.</param>
        /// <param name = "right">The second source vector.</param>
        /// <param name = "result">When the method completes, contains an new vector composed of the smallest components of the source vectors.</param>
        public static void Min(ref Vector4UI left, ref Vector4UI right, out Vector4UI result)
        {
            result.X = (left.X < right.X) ? left.X : right.X;
            result.Y = (left.Y < right.Y) ? left.Y : right.Y;
            result.Z = (left.Z < right.Z) ? left.Z : right.Z;
            result.W = (left.W < right.W) ? left.W : right.W;
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name = "left">The first source vector.</param>
        /// <param name = "right">The second source vector.</param>
        /// <returns>A vector containing the smallest components of the source vectors.</returns>
        public static Vector4UI Min(Vector4UI left, Vector4UI right)
        {
            Vector4UI result;
            Min(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name = "left">The first vector to add.</param>
        /// <param name = "right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector4UI operator +(Vector4UI left, Vector4UI right)
        {
            return new Vector4UI(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        /// <summary>
        /// Assert a vector (return it unchanged).
        /// </summary>
        /// <param name = "value">The vector to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) vector.</returns>
        public static Vector4UI operator +(Vector4UI value)
        {
            return value;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name = "left">The first vector to subtract.</param>
        /// <param name = "right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector4UI operator -(Vector4UI left, Vector4UI right)
        {
            return new Vector4UI(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4UI operator *(uint scale, Vector4UI value)
        {
            return new Vector4UI(value.X*scale, value.Y*scale, value.Z*scale, value.W*scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4UI operator *(Vector4UI value, uint scale)
        {
            return new Vector4UI(value.X*scale, value.Y*scale, value.Z*scale, value.W*scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4UI operator /(Vector4UI value, uint scale)
        {
            return new Vector4UI(value.X/scale, value.Y/scale, value.Z/scale, value.W/scale);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name = "left">The first value to compare.</param>
        /// <param name = "right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name = "left" /> has the same value as <paramref name = "right" />; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector4UI left, Vector4UI right)
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
        public static bool operator !=(Vector4UI left, Vector4UI right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref = "Vector4UI" /> to <see cref = "Vector2F" />.
        /// </summary>
        /// <param name = "value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Vector2F(Vector4UI value)
        {
            return new Vector2F(value.X, value.Y);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref = "Vector4UI" /> to <see cref = "Vector3F" />.
        /// </summary>
        /// <param name = "value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Vector3F(Vector4UI value)
        {
            return new Vector3F(value.X, value.Y, value.Z);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref = "Vector4UI" /> to <see cref = "Vector4F" />.
        /// </summary>
        /// <param name = "value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Vector4F(Vector4UI value)
        {
            return new Vector4F(value.X, value.Y, value.Z, value.W);
        }

        /// <summary>
        /// Returns a <see cref = "System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref = "System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
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

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}",
                                 X.ToString(format, CultureInfo.CurrentCulture),
                                 Y.ToString(format, CultureInfo.CurrentCulture),
                                 Z.ToString(format, CultureInfo.CurrentCulture),
                                 W.ToString(format, CultureInfo.CurrentCulture));
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
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
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

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider),
                                 Y.ToString(format, formatProvider), Z.ToString(format, formatProvider),
                                 W.ToString(format, formatProvider));
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
                hashCode = (hashCode * 397) ^ W;
                return (int)((long)hashCode - int.MaxValue);
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref = "Vector4UI" /> is equal to this instance.
        /// </summary>
        /// <param name = "other">The <see cref = "Vector4UI" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref = "Vector4UI" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector4UI other)
        {
            return other.X == X && other.Y == Y && other.Z == Z && other.W == W;
        }

        /// <summary>
        /// Determines whether the specified <see cref = "Vector4UI" /> is equal to this instance.
        /// </summary>
        /// <param name = "other">The <see cref = "Vector4UI" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref = "Vector4UI" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector4UI other)
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
            if (!(value is Vector4UI))
                return false;

            var strongValue = (Vector4UI)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="int"/> array to <see cref="Vector4UI"/>.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector4UI(uint[] input)
        {
            return new Vector4UI(input);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Vector4UI"/> to <see cref="System.Int32"/> array.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator uint[](Vector4UI input)
        {
            return input.ToArray();
        }

        public static implicit operator Vector4I(Vector4UI value)
        {
            return new Vector4I((int)value.X, (int)value.Y, (int)value.Z, (int)value.W);
        }

        public static explicit operator Vector4UI(Vector4I value)
        {
            return new Vector4UI((uint)value.X, (uint)value.Y, (uint)value.Z, (uint)value.W);
        }
    }
}
