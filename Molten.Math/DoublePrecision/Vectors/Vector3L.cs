using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten.DoublePrecision
{
	///<summary>A <see cref = "long"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
    [Serializable]
	public partial struct Vector3L : IFormattable, IVector<long>
	{
		///<summary>The X component.</summary>
        [DataMember]
		public long X;

		///<summary>The Y component.</summary>
        [DataMember]
		public long Y;

		///<summary>The Z component.</summary>
        [DataMember]
		public long Z;

		///<summary>The size of <see cref="Vector3L"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3L));

		///<summary>A Vector3L with every component set to 1L.</summary>
		public static readonly Vector3L One = new Vector3L(1L, 1L, 1L);

		/// <summary>The X unit <see cref="Vector3L"/>.</summary>
		public static readonly Vector3L UnitX = new Vector3L(1L, 0, 0);

		/// <summary>The Y unit <see cref="Vector3L"/>.</summary>
		public static readonly Vector3L UnitY = new Vector3L(0, 1L, 0);

		/// <summary>The Z unit <see cref="Vector3L"/>.</summary>
		public static readonly Vector3L UnitZ = new Vector3L(0, 0, 1L);

		/// <summary>Represents a zero'd Vector3L.</summary>
		public static readonly Vector3L Zero = new Vector3L(0, 0, 0);

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0 && Y == 0 && Z == 0;
        }

#region Constructors
        ///<summary>Creates a new instance of <see cref = "Vector3L"/>, using a <see cref="Vector2L"/> to populate the first two components.</summary>
		public Vector3L(Vector2L vector, long z)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
		}

		///<summary>Creates a new instance of <see cref = "Vector3L"/>.</summary>
		public Vector3L(long x, long y, long z)
		{
			X = x;
			Y = y;
			Z = z;
		}

        ///<summary>Creates a new instance of <see cref = "Vector3L"/>.</summary>
		public Vector3L(long value)
		{
			X = value;
			Y = value;
			Z = value;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3L"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector3L(long[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for Vector3L.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3L"/> struct from an unsafe pointer. The pointer should point to an array of three elements.
        /// </summary>
		public unsafe Vector3L(long* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
		}
#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref="Vector3L"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector3L"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3L"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector3L other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector3L"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector3L"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3L"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3L other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector3L"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector3L"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3L"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is not Vector3L)
                return false;

            var strongValue = (Vector3L)value;
            return Equals(ref strongValue);
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
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Calculates the squared length of the vector.
        /// </summary>
        /// <returns>The squared length of the vector.</returns>
        /// <remarks>
        /// This method may be preferred to <see cref="Vector2F.Length"/> when only a relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public long LengthSquared()
        {
            return ((X * X) + (Y * Y) + (Z * Z));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector3L"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public long[] ToArray()
        {
            return new long[] { X, Y, Z};
        }
		/// <summary>
        /// Reverses the direction of the current <see cref="Vector3L"/>.
        /// </summary>
        /// <returns>A <see cref="Vector3L"/> facing the opposite direction.</returns>
		public Vector3L Negate()
		{
			return new Vector3L(-X, -Y, -Z);
		}
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(long min, long max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector3L min, Vector3L max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref Vector3L min, ref Vector3L max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3L"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3L"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3L"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3L"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3L"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }
#endregion

#region Add operators
        public static void Add(ref Vector3L left, ref Vector3L right, out Vector3L result)
        {
			result.X = (left.X + right.X);
			result.Y = (left.Y + right.Y);
			result.Z = (left.Z + right.Z);
        }

        public static void Add(ref Vector3L left, long right, out Vector3L result)
        {
			result.X = (left.X + right);
			result.Y = (left.Y + right);
			result.Z = (left.Z + right);
        }

		public static Vector3L operator +(Vector3L left, Vector3L right)
		{
			Add(ref left, ref right, out Vector3L result);
            return result;
		}

		public static Vector3L operator +(Vector3L left, long right)
		{
            Add(ref left, right, out Vector3L result);
            return result;
		}

        public static Vector3L operator +(long left, Vector3L right)
		{
            Add(ref right, left, out Vector3L result);
            return result;
		}

		/// <summary>
        /// Assert a <see cref="Vector3L"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector3L"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector3L"/>.</returns>
        public static Vector3L operator +(Vector3L value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static void Subtract(ref Vector3L left, ref Vector3L right, out Vector3L result)
        {
			result.X = (left.X - right.X);
			result.Y = (left.Y - right.Y);
			result.Z = (left.Z - right.Z);
        }

        public static void Subtract(ref Vector3L left, long right, out Vector3L result)
        {
			result.X = (left.X - right);
			result.Y = (left.Y - right);
			result.Z = (left.Z - right);
        }

		public static Vector3L operator -(Vector3L left, Vector3L right)
		{
			Subtract(ref left, ref right, out Vector3L result);
            return result;
		}

		public static Vector3L operator -(Vector3L left, long right)
		{
            Subtract(ref left, right, out Vector3L result);
            return result;
		}

        public static Vector3L operator -(long left, Vector3L right)
		{
            Subtract(ref right, left, out Vector3L result);
            return result;
		}

        /// <summary>
        /// Negate/reverse the direction of a <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3L"/> to reverse.</param>
        /// <param name="result">The output for the reversed <see cref="Vector3L"/>.</param>
        public static void Negate(ref Vector3L value, out Vector3L result)
        {
			result.X = -value.X;
			result.Y = -value.Y;
			result.Z = -value.Z;
            
        }

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3L"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector3L"/>.</returns>
        public static Vector3L operator -(Vector3L value)
        {
            Negate(ref value, out value);
            return value;
        }
#endregion

#region division operators
		public static void Divide(ref Vector3L left, ref Vector3L right, out Vector3L result)
        {
			result.X = (left.X / right.X);
			result.Y = (left.Y / right.Y);
			result.Z = (left.Z / right.Z);
        }

        public static void Divide(ref Vector3L left, long right, out Vector3L result)
        {
			result.X = (left.X / right);
			result.Y = (left.Y / right);
			result.Z = (left.Z / right);
        }

		public static Vector3L operator /(Vector3L left, Vector3L right)
		{
			Divide(ref left, ref right, out Vector3L result);
            return result;
		}

		public static Vector3L operator /(Vector3L left, long right)
		{
            Divide(ref left, right, out Vector3L result);
            return result;
		}

        public static Vector3L operator /(long left, Vector3L right)
		{
            Divide(ref right, left, out Vector3L result);
            return result;
		}
#endregion

#region Multiply operators
		public static void Multiply(ref Vector3L left, ref Vector3L right, out Vector3L result)
        {
			result.X = (left.X * right.X);
			result.Y = (left.Y * right.Y);
			result.Z = (left.Z * right.Z);
        }

        public static void Multiply(ref Vector3L left, long right, out Vector3L result)
        {
			result.X = (left.X * right);
			result.Y = (left.Y * right);
			result.Z = (left.Z * right);
        }

		public static Vector3L operator *(Vector3L left, Vector3L right)
		{
			Multiply(ref left, ref right, out Vector3L result);
            return result;
		}

		public static Vector3L operator *(Vector3L left, long right)
		{
            Multiply(ref left, right, out Vector3L result);
            return result;
		}

        public static Vector3L operator *(long left, Vector3L right)
		{
            Multiply(ref right, left, out Vector3L result);
            return result;
		}
#endregion

#region Operators - Equality
        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3L left, Vector3L right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3L left, Vector3L right)
        {
            return !left.Equals(ref right);
        }
#endregion

#region Operators - Cast
        ///<summary>Casts a <see cref="Vector3L"/> to a <see cref="Vector2L"/>.</summary>
        public static explicit operator Vector2L(Vector3L value)
        {
            return new Vector2L(value.X, value.Y);
        }

        ///<summary>Casts a <see cref="Vector3L"/> to a <see cref="Vector4L"/>.</summary>
        public static explicit operator Vector4L(Vector3L value)
        {
            return new Vector4L(value.X, value.Y, value.Z, 0);
        }

#endregion

#region Static Methods
        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3L SmoothStep(ref Vector3L start, ref Vector3L end, double amount)
        {
            amount = MathHelper.SmoothStep(amount);
            return Lerp(ref start, ref end, amount);
        }

        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3L SmoothStep(Vector3L start, Vector3L end, long amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Vector3L"/>.</param>
        /// <param name="source">The list of vectors to orthogonalize.</param>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all vectors orthogonal to each other. This
        /// means that any given vector in the list will be orthogonal to any other given vector in the
        /// list.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
        /// tend to be numerically unstable. The numeric stability decreases according to the vectors
        /// position in the list so that the first vector is the most stable and the last vector is the
        /// least stable.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Orthogonalize(Vector3L[] destination, params Vector3L[] source)
        {
            //Uses the modified Gram-Schmidt process.
            //q1 = m1
            //q2 = m2 - ((q1 ⋅ m2) / (q1 ⋅ q1)) * q1
            //q3 = m3 - ((q1 ⋅ m3) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m3) / (q2 ⋅ q2)) * q2
            //q4 = m4 - ((q1 ⋅ m4) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m4) / (q2 ⋅ q2)) * q2 - ((q3 ⋅ m4) / (q3 ⋅ q3)) * q3
            //q5 = ...

            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
            {
                Vector3L newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector3L"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector3L"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
		/// <param name="zIndex">The axis index to use for the new Z value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector3L Swizzle(Vector3L val, int xIndex, int yIndex, int zIndex)
        {
            return new Vector3L()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
			   Z = (&val.X)[zIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector3L Swizzle(Vector3L val, uint xIndex, uint yIndex, uint zIndex)
        {
            return new Vector3L()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
			    Z = (&val.X)[zIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector3L"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3L"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3L"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Dot(ref Vector3L left, ref Vector3L right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector3L"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3L"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3L"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Dot(Vector3L left, Vector3L right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

		/// <summary>
        /// Returns a <see cref="Vector3L"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector3L"/> containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector3L"/> containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector3L"/> containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector3L Barycentric(ref Vector3L value1, ref Vector3L value2, ref Vector3L value3, long amount1, long amount2)
        {
			return new Vector3L(
				((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))), 
				((value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3L"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Vector3L start, ref Vector3L end, double amount, out Vector3L result)
        {
			result.X = (long)((1D - amount) * start.X + amount * end.X);
			result.Y = (long)((1D - amount) * start.Y + amount * end.Y);
			result.Z = (long)((1D - amount) * start.Z + amount * end.Z);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3L Lerp(Vector3L start, Vector3L end, double amount)
        {
			return new Vector3L()
			{
				X = (long)((1D - amount) * start.X + amount * end.X),
				Y = (long)((1D - amount) * start.Y + amount * end.Y),
				Z = (long)((1D - amount) * start.Z + amount * end.Z),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3L Lerp(ref Vector3L start, ref Vector3L end, double amount)
        {
			return new Vector3L()
			{
				X = (long)((1D - amount) * start.X + amount * end.X),
				Y = (long)((1D - amount) * start.Y + amount * end.Y),
				Z = (long)((1D - amount) * start.Z + amount * end.Z),
			};
        }

        /// <summary>
        /// Returns a <see cref="Vector3L"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3L"/>.</param>
        /// <param name="right">The second source <see cref="Vector3L"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3L"/>.</param>
        /// <returns>A <see cref="Vector3L"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref Vector3L left, ref Vector3L right, out Vector3L result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
				result.Z = (left.Z < right.Z) ? left.Z : right.Z;
		}

        /// <summary>
        /// Returns a <see cref="Vector3L"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3L"/>.</param>
        /// <param name="right">The second source <see cref="Vector3L"/>.</param>
        /// <returns>A <see cref="Vector3L"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3L Min(ref Vector3L left, ref Vector3L right)
		{
			Min(ref left, ref right, out Vector3L result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector3L"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3L"/>.</param>
        /// <param name="right">The second source <see cref="Vector3L"/>.</param>
        /// <returns>A <see cref="Vector3L"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3L Min(Vector3L left, Vector3L right)
		{
			return new Vector3L()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

        /// <summary>
        /// Returns a <see cref="Vector3L"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3L"/>.</param>
        /// <param name="right">The second source <see cref="Vector3L"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3L"/>.</param>
        /// <returns>A <see cref="Vector3L"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref Vector3L left, ref Vector3L right, out Vector3L result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
				result.Z = (left.Z > right.Z) ? left.Z : right.Z;
		}

        /// <summary>
        /// Returns a <see cref="Vector3L"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3L"/>.</param>
        /// <param name="right">The second source <see cref="Vector3L"/>.</param>
        /// <returns>A <see cref="Vector3L"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3L Max(ref Vector3L left, ref Vector3L right)
		{
			Max(ref left, ref right, out Vector3L result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector3L"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3L"/>.</param>
        /// <param name="right">The second source <see cref="Vector3L"/>.</param>
        /// <returns>A <see cref="Vector3L"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3L Max(Vector3L left, Vector3L right)
		{
			return new Vector3L()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3L"/> vectors.
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
		public static long DistanceSquared(ref Vector3L value1, ref Vector3L value2)
        {
            long x = value1.X - value2.X;
            long y = value1.Y - value2.Y;
            long z = value1.Z - value2.Z;

            return ((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Vector3L"/> vectors.
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
		public static long DistanceSquared(Vector3L value1, Vector3L value2)
        {
            long x = value1.X - value2.X;
            long y = value1.Y - value2.Y;
            long z = value1.Z - value2.Z;

            return ((x * x) + (y * y) + (z * z));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3L"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3L Clamp(Vector3L value, long min, long max)
        {
			return new Vector3L()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3L"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3L"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref Vector3L value, ref Vector3L min, ref Vector3L max, out Vector3L result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
				result.Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3L"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3L Clamp(Vector3L value, Vector3L min, Vector3L max)
        {
			return new Vector3L()
			{
				X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
				Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y,
				Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z,
			};
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal. 
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">Normal of the surface.</param>
        /// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
        /// whether the original vector was close enough to the surface to hit it.</remarks>
        public static Vector3L Reflect(ref Vector3L vector, ref Vector3L normal)
        {
            long dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z);

            return new Vector3L()
            {
				X = (long)(vector.X - ((2 * dot) * normal.X)),
				Y = (long)(vector.Y - ((2 * dot) * normal.Y)),
				Z = (long)(vector.Z - ((2 * dot) * normal.Z)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (long x, long y, long z)(Vector3L val)
        {
            return (val.X, val.Y, val.Z);
        }

        public static implicit operator Vector3L((long x, long y, long z) val)
        {
            return new Vector3L(val.x, val.y, val.z);
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y or Z component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 2].</exception>  
		public long this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3L run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3L run from 0 to 2, inclusive.");
			}
		}
#endregion

#region Casts - vectors
        ///<summary>Casts a <see cref="Vector3L"/> to a <see cref="SByte3"/>.</summary>
        public static explicit operator SByte3(Vector3L val)
        {
            return new SByte3()
            {
                X = (sbyte)val.X,
                Y = (sbyte)val.Y,
                Z = (sbyte)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3L"/> to a <see cref="Byte3"/>.</summary>
        public static explicit operator Byte3(Vector3L val)
        {
            return new Byte3()
            {
                X = (byte)val.X,
                Y = (byte)val.Y,
                Z = (byte)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3L"/> to a <see cref="Vector3I"/>.</summary>
        public static explicit operator Vector3I(Vector3L val)
        {
            return new Vector3I()
            {
                X = (int)val.X,
                Y = (int)val.Y,
                Z = (int)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3L"/> to a <see cref="Vector3UI"/>.</summary>
        public static explicit operator Vector3UI(Vector3L val)
        {
            return new Vector3UI()
            {
                X = (uint)val.X,
                Y = (uint)val.Y,
                Z = (uint)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3L"/> to a <see cref="Vector3S"/>.</summary>
        public static explicit operator Vector3S(Vector3L val)
        {
            return new Vector3S()
            {
                X = (short)val.X,
                Y = (short)val.Y,
                Z = (short)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3L"/> to a <see cref="Vector3US"/>.</summary>
        public static explicit operator Vector3US(Vector3L val)
        {
            return new Vector3US()
            {
                X = (ushort)val.X,
                Y = (ushort)val.Y,
                Z = (ushort)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3L"/> to a <see cref="Vector3UL"/>.</summary>
        public static explicit operator Vector3UL(Vector3L val)
        {
            return new Vector3UL()
            {
                X = (ulong)val.X,
                Y = (ulong)val.Y,
                Z = (ulong)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3L"/> to a <see cref="Vector3F"/>.</summary>
        public static explicit operator Vector3F(Vector3L val)
        {
            return new Vector3F()
            {
                X = (float)val.X,
                Y = (float)val.Y,
                Z = (float)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3L"/> to a <see cref="Vector3D"/>.</summary>
        public static explicit operator Vector3D(Vector3L val)
        {
            return new Vector3D()
            {
                X = (double)val.X,
                Y = (double)val.Y,
                Z = (double)val.Z,
            };
        }

#endregion
	}
}

