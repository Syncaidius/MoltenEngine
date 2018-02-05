using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten
{
    /// <summary>
    /// Represents a three dimensional mathematical unsigned int vector.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct UIntVector3 : IEquatable<UIntVector3>, IFormattable
    {
        /// <summary>
        /// The size of the <see cref = "UIntVector3" /> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof (UIntVector3));

        /// <summary>
        /// A <see cref = "UIntVector3" /> with all of its components set to zero.
        /// </summary>
        public static readonly UIntVector3 Zero = new UIntVector3();

        /// <summary>
        /// The X unit <see cref = "UIntVector3" /> (1, 0, 0).
        /// </summary>
        public static readonly UIntVector3 UnitX = new UIntVector3(1, 0, 0);

        /// <summary>
        /// The Y unit <see cref = "UIntVector3" /> (0, 1, 0).
        /// </summary>
        public static readonly UIntVector3 UnitY = new UIntVector3(0, 1, 0);

        /// <summary>
        /// The Z unit <see cref = "UIntVector3" /> (0, 0, 1).
        /// </summary>
        public static readonly UIntVector3 UnitZ = new UIntVector3(0, 0, 1);

        /// <summary>
        /// A <see cref = "UIntVector3" /> with all of its components set to one.
        /// </summary>
        public static readonly UIntVector3 One = new UIntVector3(1, 1, 1);

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
        /// Initializes a new instance of the <see cref = "UIntVector3" /> struct.
        /// </summary>
        /// <param name = "value">The value that will be assigned to all components.</param>
        public UIntVector3(uint value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref = "UIntVector3" /> struct.
        /// </summary>
        /// <param name = "x">Initial value for the X component of the vector.</param>
        /// <param name = "y">Initial value for the Y component of the vector.</param>
        /// <param name = "z">Initial value for the Z component of the vector.</param>
        public UIntVector3(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref = "UIntVector3" /> struct.
        /// </summary>
        /// <param name = "values">The values to assign to the X, Y, Z, and W components of the vector. This must be an array with four elements.</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "values" /> is <c>null</c>.</exception>
        /// <exception cref = "ArgumentOutOfRangeException">Thrown when <paramref name = "values" /> contains more or less than four elements.</exception>
        public UIntVector3(uint[] values)
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
        public uint[] ToArray()
        {
            return new uint[] {X, Y, Z};
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name = "left">The first vector to add.</param>
        /// <param name = "right">The second vector to add.</param>
        /// <param name = "result">When the method completes, contains the sum of the two vectors.</param>
        public static void Add(ref UIntVector3 left, ref UIntVector3 right, out UIntVector3 result)
        {
            result = new UIntVector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name = "left">The first vector to add.</param>
        /// <param name = "right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static UIntVector3 Add(UIntVector3 left, UIntVector3 right)
        {
            return new UIntVector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name = "left">The first vector to subtract.</param>
        /// <param name = "right">The second vector to subtract.</param>
        /// <param name = "result">When the method completes, contains the difference of the two vectors.</param>
        public static void Subtract(ref UIntVector3 left, ref UIntVector3 right, out UIntVector3 result)
        {
            result = new UIntVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name = "left">The first vector to subtract.</param>
        /// <param name = "right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static UIntVector3 Subtract(UIntVector3 left, UIntVector3 right)
        {
            return new UIntVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <param name = "result">When the method completes, contains the scaled vector.</param>
        public static void Multiply(ref UIntVector3 value, uint scale, out UIntVector3 result)
        {
            result = new UIntVector3(value.X*scale, value.Y*scale, value.Z*scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static UIntVector3 Multiply(UIntVector3 value, uint scale)
        {
            return new UIntVector3(value.X*scale, value.Y*scale, value.Z*scale);
        }

        /// <summary>
        /// Modulates a vector with another by performing component-wise multiplication.
        /// </summary>
        /// <param name = "left">The first vector to modulate.</param>
        /// <param name = "right">The second vector to modulate.</param>
        /// <param name = "result">When the method completes, contains the modulated vector.</param>
        public static void Modulate(ref UIntVector3 left, ref UIntVector3 right, out UIntVector3 result)
        {
            result = new UIntVector3(left.X*right.X, left.Y*right.Y, left.Z*right.Z);
        }

        /// <summary>
        /// Modulates a vector with another by performing component-wise multiplication.
        /// </summary>
        /// <param name = "left">The first vector to modulate.</param>
        /// <param name = "right">The second vector to modulate.</param>
        /// <returns>The modulated vector.</returns>
        public static UIntVector3 Modulate(UIntVector3 left, UIntVector3 right)
        {
            return new UIntVector3(left.X*right.X, left.Y*right.Y, left.Z*right.Z);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <param name = "result">When the method completes, contains the scaled vector.</param>
        public static void Divide(ref UIntVector3 value, uint scale, out UIntVector3 result)
        {
            result = new UIntVector3(value.X/scale, value.Y/scale, value.Z/scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static UIntVector3 Divide(UIntVector3 value, uint scale)
        {
            return new UIntVector3(value.X/scale, value.Y/scale, value.Z/scale);
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name = "value">The value to clamp.</param>
        /// <param name = "min">The minimum value.</param>
        /// <param name = "max">The maximum value.</param>
        /// <param name = "result">When the method completes, contains the clamped value.</param>
        public static void Clamp(ref UIntVector3 value, ref UIntVector3 min, ref UIntVector3 max, out UIntVector3 result)
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

            result = new UIntVector3(x, y, z);
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name = "value">The value to clamp.</param>
        /// <param name = "min">The minimum value.</param>
        /// <param name = "max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static UIntVector3 Clamp(UIntVector3 value, UIntVector3 min, UIntVector3 max)
        {
            UIntVector3 result;
            Clamp(ref value, ref min, ref max, out result);
            return result;
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name = "left">The first source vector.</param>
        /// <param name = "right">The second source vector.</param>
        /// <param name = "result">When the method completes, contains an new vector composed of the largest components of the source vectors.</param>
        public static void Max(ref UIntVector3 left, ref UIntVector3 right, out UIntVector3 result)
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
        public static UIntVector3 Max(UIntVector3 left, UIntVector3 right)
        {
            UIntVector3 result;
            Max(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name = "left">The first source vector.</param>
        /// <param name = "right">The second source vector.</param>
        /// <param name = "result">When the method completes, contains an new vector composed of the smallest components of the source vectors.</param>
        public static void Min(ref UIntVector3 left, ref UIntVector3 right, out UIntVector3 result)
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
        public static UIntVector3 Min(UIntVector3 left, UIntVector3 right)
        {
            UIntVector3 result;
            Min(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name = "left">The first vector to add.</param>
        /// <param name = "right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static UIntVector3 operator +(UIntVector3 left, UIntVector3 right)
        {
            return new UIntVector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        /// <summary>
        /// Assert a vector (return it unchanged).
        /// </summary>
        /// <param name = "value">The vector to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) vector.</returns>
        public static UIntVector3 operator +(UIntVector3 value)
        {
            return value;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name = "left">The first vector to subtract.</param>
        /// <param name = "right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static UIntVector3 operator -(UIntVector3 left, UIntVector3 right)
        {
            return new UIntVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static UIntVector3 operator *(uint scale, UIntVector3 value)
        {
            return new UIntVector3(value.X*scale, value.Y*scale, value.Z*scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static UIntVector3 operator *(UIntVector3 value, uint scale)
        {
            return new UIntVector3(value.X*scale, value.Y*scale, value.Z*scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name = "value">The vector to scale.</param>
        /// <param name = "scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static UIntVector3 operator /(UIntVector3 value, uint scale)
        {
            return new UIntVector3(value.X/scale, value.Y/scale, value.Z/scale);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name = "left">The first value to compare.</param>
        /// <param name = "right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name = "left" /> has the same value as <paramref name = "right" />; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UIntVector3 left, UIntVector3 right)
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
        public static bool operator !=(UIntVector3 left, UIntVector3 right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref = "UIntVector3" /> to <see cref = "Vector2" />.
        /// </summary>
        /// <param name = "value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Vector2(UIntVector3 value)
        {
            return new Vector2(value.X, value.Y);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref = "UIntVector3" /> to <see cref = "Vector3" />.
        /// </summary>
        /// <param name = "value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Vector3(UIntVector3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
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
                uint hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return (int)((long)hashCode - int.MaxValue);
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref = "UIntVector3" /> is equal to this instance.
        /// </summary>
        /// <param name = "other">The <see cref = "UIntVector3" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref = "UIntVector3" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref UIntVector3 other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        /// <summary>
        /// Determines whether the specified <see cref = "UIntVector3" /> is equal to this instance.
        /// </summary>
        /// <param name = "other">The <see cref = "UIntVector3" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref = "UIntVector3" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(UIntVector3 other)
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
            if (!(value is UIntVector3))
                return false;

            var strongValue = (UIntVector3)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="int"/> array to <see cref="UIntVector3"/>.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator UIntVector3(uint[] input)
        {
            return new UIntVector3(input);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UIntVector3"/> to <see cref="System.Int32"/> array.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator uint[](UIntVector3 input)
        {
            return input.ToArray();
        }

        public static implicit operator IntVector3(UIntVector3 value)
        {
            return new IntVector3((int)value.X,(int)value.Y, (int)value.Z);
        }

        public static explicit operator UIntVector3(IntVector3 value)
        {
            return new UIntVector3((uint)value.X, (uint)value.Y, (uint)value.Z);
        }
    }
}
