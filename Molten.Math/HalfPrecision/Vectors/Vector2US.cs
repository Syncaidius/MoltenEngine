using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten.HalfPrecision
{
	///<summary>A <see cref = "ushort"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
    [Serializable]
	public partial struct Vector2US : IFormattable, IVector<ushort>
	{
		///<summary>The X component.</summary>
        [DataMember]
		public ushort X;

		///<summary>The Y component.</summary>
        [DataMember]
		public ushort Y;

		///<summary>The size of <see cref="Vector2US"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2US));

		///<summary>A Vector2US with every component set to (ushort)1.</summary>
		public static readonly Vector2US One = new Vector2US((ushort)1, (ushort)1);

		/// <summary>The X unit <see cref="Vector2US"/>.</summary>
		public static readonly Vector2US UnitX = new Vector2US((ushort)1, 0);

		/// <summary>The Y unit <see cref="Vector2US"/>.</summary>
		public static readonly Vector2US UnitY = new Vector2US(0, (ushort)1);

		/// <summary>Represents a zero'd Vector2US.</summary>
		public static readonly Vector2US Zero = new Vector2US(0, 0);

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0 && Y == 0;
        }

#region Constructors

		///<summary>Creates a new instance of <see cref = "Vector2US"/>.</summary>
		public Vector2US(ushort x, ushort y)
		{
			X = x;
			Y = y;
		}

        ///<summary>Creates a new instance of <see cref = "Vector2US"/>.</summary>
		public Vector2US(ushort value)
		{
			X = value;
			Y = value;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector2US"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector2US(ushort[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Vector2US.");

			X = values[0];
			Y = values[1];
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector2US"/> struct from an unsafe pointer. The pointer should point to an array of two elements.
        /// </summary>
		public unsafe Vector2US(ushort* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
		}
#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref="Vector2US"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector2US"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector2US"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector2US other)
        {
            return other.X == X && other.Y == Y;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector2US"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector2US"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector2US"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2US other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector2US"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector2US"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector2US"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is not Vector2US)
                return false;

            var strongValue = (Vector2US)value;
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
        public ushort LengthSquared()
        {
            return (ushort)((X * X) + (Y * Y));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector2US"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public ushort[] ToArray()
        {
            return new ushort[] { X, Y};
        }
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ushort min, ushort max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector2US min, Vector2US max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref Vector2US min, ref Vector2US max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2US"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2US"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2US"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2US"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2US"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2US"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2US"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2US"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
        }
#endregion

#region Add operators
        public static void Add(ref Vector2US left, ref Vector2US right, out Vector2US result)
        {
			result.X = (ushort)(left.X + right.X);
			result.Y = (ushort)(left.Y + right.Y);
        }

        public static void Add(ref Vector2US left, ushort right, out Vector2US result)
        {
			result.X = (ushort)(left.X + right);
			result.Y = (ushort)(left.Y + right);
        }

		public static Vector2US operator +(Vector2US left, Vector2US right)
		{
			Add(ref left, ref right, out Vector2US result);
            return result;
		}

		public static Vector2US operator +(Vector2US left, ushort right)
		{
            Add(ref left, right, out Vector2US result);
            return result;
		}

        public static Vector2US operator +(ushort left, Vector2US right)
		{
            Add(ref right, left, out Vector2US result);
            return result;
		}

		/// <summary>
        /// Assert a <see cref="Vector2US"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector2US"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector2US"/>.</returns>
        public static Vector2US operator +(Vector2US value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static void Subtract(ref Vector2US left, ref Vector2US right, out Vector2US result)
        {
			result.X = (ushort)(left.X - right.X);
			result.Y = (ushort)(left.Y - right.Y);
        }

        public static void Subtract(ref Vector2US left, ushort right, out Vector2US result)
        {
			result.X = (ushort)(left.X - right);
			result.Y = (ushort)(left.Y - right);
        }

		public static Vector2US operator -(Vector2US left, Vector2US right)
		{
			Subtract(ref left, ref right, out Vector2US result);
            return result;
		}

		public static Vector2US operator -(Vector2US left, ushort right)
		{
            Subtract(ref left, right, out Vector2US result);
            return result;
		}

        public static Vector2US operator -(ushort left, Vector2US right)
		{
            Subtract(ref right, left, out Vector2US result);
            return result;
		}

#endregion

#region division operators
		public static void Divide(ref Vector2US left, ref Vector2US right, out Vector2US result)
        {
			result.X = (ushort)(left.X / right.X);
			result.Y = (ushort)(left.Y / right.Y);
        }

        public static void Divide(ref Vector2US left, ushort right, out Vector2US result)
        {
			result.X = (ushort)(left.X / right);
			result.Y = (ushort)(left.Y / right);
        }

		public static Vector2US operator /(Vector2US left, Vector2US right)
		{
			Divide(ref left, ref right, out Vector2US result);
            return result;
		}

		public static Vector2US operator /(Vector2US left, ushort right)
		{
            Divide(ref left, right, out Vector2US result);
            return result;
		}

        public static Vector2US operator /(ushort left, Vector2US right)
		{
            Divide(ref right, left, out Vector2US result);
            return result;
		}
#endregion

#region Multiply operators
		public static void Multiply(ref Vector2US left, ref Vector2US right, out Vector2US result)
        {
			result.X = (ushort)(left.X * right.X);
			result.Y = (ushort)(left.Y * right.Y);
        }

        public static void Multiply(ref Vector2US left, ushort right, out Vector2US result)
        {
			result.X = (ushort)(left.X * right);
			result.Y = (ushort)(left.Y * right);
        }

		public static Vector2US operator *(Vector2US left, Vector2US right)
		{
			Multiply(ref left, ref right, out Vector2US result);
            return result;
		}

		public static Vector2US operator *(Vector2US left, ushort right)
		{
            Multiply(ref left, right, out Vector2US result);
            return result;
		}

        public static Vector2US operator *(ushort left, Vector2US right)
		{
            Multiply(ref right, left, out Vector2US result);
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
        public static bool operator ==(Vector2US left, Vector2US right)
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
        public static bool operator !=(Vector2US left, Vector2US right)
        {
            return !left.Equals(ref right);
        }
#endregion

#region Operators - Cast
        ///<summary>Casts a <see cref="Vector2US"/> to a <see cref="Vector3US"/>.</summary>
        public static explicit operator Vector3US(Vector2US value)
        {
            return new Vector3US(value.X, value.Y, 0);
        }

        ///<summary>Casts a <see cref="Vector2US"/> to a <see cref="Vector4US"/>.</summary>
        public static explicit operator Vector4US(Vector2US value)
        {
            return new Vector4US(value.X, value.Y, 0, 0);
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
        public static Vector2US SmoothStep(ref Vector2US start, ref Vector2US end, float amount)
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
        public static Vector2US SmoothStep(Vector2US start, Vector2US end, ushort amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Vector2US"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Vector2US"/>.</param>
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
        public static void Orthogonalize(Vector2US[] destination, params Vector2US[] source)
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
                Vector2US newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (ushort)(Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector2US"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector2US"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector2US Swizzle(Vector2US val, int xIndex, int yIndex)
        {
            return new Vector2US()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector2US Swizzle(Vector2US val, uint xIndex, uint yIndex)
        {
            return new Vector2US()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector2US"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector2US"/> source vector</param>
        /// <param name="right">Second <see cref="Vector2US"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Dot(ref Vector2US left, ref Vector2US right)
        {
			return (ushort)(((ushort)left.X * right.X) + ((ushort)left.Y * right.Y));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector2US"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector2US"/> source vector</param>
        /// <param name="right">Second <see cref="Vector2US"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Dot(Vector2US left, Vector2US right)
        {
			return (ushort)((left.X * right.X) + (left.Y * right.Y));
        }

		/// <summary>
        /// Returns a <see cref="Vector2US"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector2US"/> containing the 2D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector2US"/> containing the 2D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector2US"/> containing the 2D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector2US Barycentric(ref Vector2US value1, ref Vector2US value2, ref Vector2US value3, ushort amount1, ushort amount2)
        {
			return new Vector2US(
				(ushort)((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				(ushort)((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2US"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2US"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Vector2US start, ref Vector2US end, float amount, out Vector2US result)
        {
			result.X = (ushort)((1F - amount) * start.X + amount * end.X);
			result.Y = (ushort)((1F - amount) * start.Y + amount * end.Y);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2US"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2US Lerp(Vector2US start, Vector2US end, float amount)
        {
			return new Vector2US()
			{
				X = (ushort)((1F - amount) * start.X + amount * end.X),
				Y = (ushort)((1F - amount) * start.Y + amount * end.Y),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2US"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2US Lerp(ref Vector2US start, ref Vector2US end, float amount)
        {
			return new Vector2US()
			{
				X = (ushort)((1F - amount) * start.X + amount * end.X),
				Y = (ushort)((1F - amount) * start.Y + amount * end.Y),
			};
        }

        /// <summary>
        /// Returns a <see cref="Vector2US"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2US"/>.</param>
        /// <param name="right">The second source <see cref="Vector2US"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2US"/>.</param>
        /// <returns>A <see cref="Vector2US"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref Vector2US left, ref Vector2US right, out Vector2US result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
		}

        /// <summary>
        /// Returns a <see cref="Vector2US"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2US"/>.</param>
        /// <param name="right">The second source <see cref="Vector2US"/>.</param>
        /// <returns>A <see cref="Vector2US"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2US Min(ref Vector2US left, ref Vector2US right)
		{
			Min(ref left, ref right, out Vector2US result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector2US"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2US"/>.</param>
        /// <param name="right">The second source <see cref="Vector2US"/>.</param>
        /// <returns>A <see cref="Vector2US"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2US Min(Vector2US left, Vector2US right)
		{
			return new Vector2US()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
			};
		}

        /// <summary>
        /// Returns a <see cref="Vector2US"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2US"/>.</param>
        /// <param name="right">The second source <see cref="Vector2US"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2US"/>.</param>
        /// <returns>A <see cref="Vector2US"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref Vector2US left, ref Vector2US right, out Vector2US result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
		}

        /// <summary>
        /// Returns a <see cref="Vector2US"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2US"/>.</param>
        /// <param name="right">The second source <see cref="Vector2US"/>.</param>
        /// <returns>A <see cref="Vector2US"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2US Max(ref Vector2US left, ref Vector2US right)
		{
			Max(ref left, ref right, out Vector2US result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector2US"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2US"/>.</param>
        /// <param name="right">The second source <see cref="Vector2US"/>.</param>
        /// <returns>A <see cref="Vector2US"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2US Max(Vector2US left, Vector2US right)
		{
			return new Vector2US()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2US"/> vectors.
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
		public static ushort DistanceSquared(ref Vector2US value1, ref Vector2US value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;

            return (ushort)((x * x) + (y * y));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Vector2US"/> vectors.
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
		public static ushort DistanceSquared(Vector2US value1, Vector2US value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;

            return (ushort)((x * x) + (y * y));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2US"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2US Clamp(Vector2US value, ushort min, ushort max)
        {
			return new Vector2US()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2US"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2US"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref Vector2US value, ref Vector2US min, ref Vector2US max, out Vector2US result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2US"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2US Clamp(Vector2US value, Vector2US min, Vector2US max)
        {
			return new Vector2US()
			{
				X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
				Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y,
			};
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal. 
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">Normal of the surface.</param>
        /// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
        /// whether the original vector was close enough to the surface to hit it.</remarks>
        public static Vector2US Reflect(ref Vector2US vector, ref Vector2US normal)
        {
            int dot = (vector.X * normal.X) + (vector.Y * normal.Y);

            return new Vector2US()
            {
				X = (ushort)(vector.X - ((2 * dot) * normal.X)),
				Y = (ushort)(vector.Y - ((2 * dot) * normal.Y)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (ushort x, ushort y)(Vector2US val)
        {
            return (val.X, val.Y);
        }

        public static implicit operator Vector2US((ushort x, ushort y) val)
        {
            return new Vector2US(val.x, val.y);
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X or Y component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 1].</exception>  
		public ushort this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2US run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2US run from 0 to 1, inclusive.");
			}
		}
#endregion

#region Casts - vectors
        ///<summary>Casts a <see cref="Vector2US"/> to a <see cref="SByte2"/>.</summary>
        public static explicit operator SByte2(Vector2US val)
        {
            return new SByte2()
            {
                X = (sbyte)val.X,
                Y = (sbyte)val.Y,
            };
        }

        ///<summary>Casts a <see cref="Vector2US"/> to a <see cref="Byte2"/>.</summary>
        public static explicit operator Byte2(Vector2US val)
        {
            return new Byte2()
            {
                X = (byte)val.X,
                Y = (byte)val.Y,
            };
        }

        ///<summary>Casts a <see cref="Vector2US"/> to a <see cref="Vector2I"/>.</summary>
        public static explicit operator Vector2I(Vector2US val)
        {
            return new Vector2I()
            {
                X = (int)val.X,
                Y = (int)val.Y,
            };
        }

        ///<summary>Casts a <see cref="Vector2US"/> to a <see cref="Vector2UI"/>.</summary>
        public static explicit operator Vector2UI(Vector2US val)
        {
            return new Vector2UI()
            {
                X = val.X,
                Y = val.Y,
            };
        }

        ///<summary>Casts a <see cref="Vector2US"/> to a <see cref="Vector2S"/>.</summary>
        public static explicit operator Vector2S(Vector2US val)
        {
            return new Vector2S()
            {
                X = (short)val.X,
                Y = (short)val.Y,
            };
        }

        ///<summary>Casts a <see cref="Vector2US"/> to a <see cref="Vector2L"/>.</summary>
        public static explicit operator Vector2L(Vector2US val)
        {
            return new Vector2L()
            {
                X = (long)val.X,
                Y = (long)val.Y,
            };
        }

        ///<summary>Casts a <see cref="Vector2US"/> to a <see cref="Vector2UL"/>.</summary>
        public static explicit operator Vector2UL(Vector2US val)
        {
            return new Vector2UL()
            {
                X = val.X,
                Y = val.Y,
            };
        }

        ///<summary>Casts a <see cref="Vector2US"/> to a <see cref="Vector2F"/>.</summary>
        public static explicit operator Vector2F(Vector2US val)
        {
            return new Vector2F()
            {
                X = (float)val.X,
                Y = (float)val.Y,
            };
        }

        ///<summary>Casts a <see cref="Vector2US"/> to a <see cref="Vector2D"/>.</summary>
        public static explicit operator Vector2D(Vector2US val)
        {
            return new Vector2D()
            {
                X = (double)val.X,
                Y = (double)val.Y,
            };
        }

#endregion
	}
}

