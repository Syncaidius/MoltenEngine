using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten
{
	///<summary>A <see cref="uint"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
    [Serializable]
	public partial struct Vector2UI : IFormattable, IVector<Vector2UI, uint>, IEquatable<Vector2UI>
	{
		///<summary>The size of <see cref="Vector2UI"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2UI));

		///<summary>A Vector2UI with every component set to 1U.</summary>
		public static readonly Vector2UI One = new Vector2UI(1U, 1U);

        static readonly string toStringFormat = "X:{0} Y:{1}";

		/// <summary>The X unit <see cref="Vector2UI"/>.</summary>
		public static readonly Vector2UI UnitX = new Vector2UI(1U, 0U);

		/// <summary>The Y unit <see cref="Vector2UI"/>.</summary>
		public static readonly Vector2UI UnitY = new Vector2UI(0U, 1U);

		/// <summary>Represents a zero'd Vector2UI.</summary>
		public static readonly Vector2UI Zero = new Vector2UI(0U, 0U);

		/// <summary>The X component.</summary>
		[DataMember]
		public uint X;

		/// <summary>The Y component.</summary>
		[DataMember]
		public uint Y;


        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0U && Y == 0U;
        }

#region Constructors
		/// <summary>Initializes a new instance of <see cref="Vector2UI"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Vector2UI(uint value)
		{
			X = value;
			Y = value;
		}
		/// <summary>Initializes a new instance of <see cref="Vector2UI"/> from an array.</summary>
		/// <param name="values">The values to assign to the X, Y components of the color. This must be an array with at least two elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Vector2UI(uint[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 2)
				throw new ArgumentOutOfRangeException("values", "There must be at least two input values for Vector2UI.");

			X = values[0];
			Y = values[1];
		}
		/// <summary>Initializes a new instance of <see cref="Vector2UI"/> from a span.</summary>
		/// <param name="values">The values to assign to the X, Y components of the color. This must be an array with at least two elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Vector2UI(Span<uint> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 2)
				throw new ArgumentOutOfRangeException("values", "There must be at least two input values for Vector2UI.");

			X = values[0];
			Y = values[1];
		}
		/// <summary>Initializes a new instance of <see cref="Vector2UI"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the X, Y components of the color.
		/// <para>There must be at least two elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than four elements.</exception>
		public unsafe Vector2UI(uint* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			X = ptrValues[0];
			Y = ptrValues[1];
		}
		/// <summary>
		/// Initializes a new instance of <see cref="Vector2UI"/>.
		/// </summary>
		/// <param name="x">The X component.</param>
		/// <param name="y">The Y component.</param>
		public Vector2UI(uint x, uint y)
		{
			X = x;
			Y = y;
		}

#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref = "Vector2UI"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector2UI"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector2UI"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector2UI other)
        {
            return other.X == X && other.Y == Y;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector2UI"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector2UI"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector2UI"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2UI other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector2UI"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector2UI"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector2UI"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is Vector2UI v)
               return Equals(ref v);

            return false;
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
        public uint LengthSquared()
        {
            return ((X * X) + (Y * Y));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector2UI"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public uint[] ToArray()
        {
            return new uint[] { X, Y };
        }
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(uint min, uint max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector2UI min, Vector2UI max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref Vector2UI min, ref Vector2UI max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
        }
#endregion

#region To-String
		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector2UI"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector2UI"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, format, X, Y);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector2UI"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector2UI"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, toStringFormat, X, Y);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector2UI"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector2UI"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, toStringFormat, X, Y);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector2UI"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector2UI"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider,
                toStringFormat,
				X.ToString(format, formatProvider),
				Y.ToString(format, formatProvider)
            );
        }
#endregion

#region Add operators
		///<summary>Performs a add operation on two <see cref="Vector2UI"/>.</summary>
		///<param name="a">The first <see cref="Vector2UI"/> to add.</param>
		///<param name="b">The second <see cref="Vector2UI"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Vector2UI a, ref Vector2UI b, out Vector2UI result)
		{
			result.X = a.X + b.X;
			result.Y = a.Y + b.Y;
		}

		///<summary>Performs a add operation on two <see cref="Vector2UI"/>.</summary>
		///<param name="a">The first <see cref="Vector2UI"/> to add.</param>
		///<param name="b">The second <see cref="Vector2UI"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator +(Vector2UI a, Vector2UI b)
		{
			Add(ref a, ref b, out Vector2UI result);
			return result;
		}

		///<summary>Performs a add operation on a $<see cref="Vector2UI"/> and a $<see cref="uint"/>.</summary>
		///<param name="a">The <see cref="Vector2UI"/> to add.</param>
		///<param name="b">The <see cref="uint"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Vector2UI a, uint b, out Vector2UI result)
		{
			result.X = a.X + b;
			result.Y = a.Y + b;
		}

		///<summary>Performs a add operation on a $<see cref="Vector2UI"/> and a $<see cref="uint"/>.</summary>
		///<param name="a">The <see cref="Vector2UI"/> to add.</param>
		///<param name="b">The <see cref="uint"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator +(Vector2UI a, uint b)
		{
			Add(ref a, b, out Vector2UI result);
			return result;
		}

		///<summary>Performs a add operation on a $<see cref="uint"/> and a $<see cref="Vector2UI"/>.</summary>
		///<param name="a">The <see cref="uint"/> to add.</param>
		///<param name="b">The <see cref="Vector2UI"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator +(uint a, Vector2UI b)
		{
			Add(ref b, a, out Vector2UI result);
			return result;
		}


		/// <summary>
        /// Assert a <see cref="Vector2UI"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector2UI"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector2UI"/>.</returns>
        public static Vector2UI operator +(Vector2UI value)
        {
            return value;
        }
#endregion

#region Subtract operators
		///<summary>Performs a subtract operation on two <see cref="Vector2UI"/>.</summary>
		///<param name="a">The first <see cref="Vector2UI"/> to subtract.</param>
		///<param name="b">The second <see cref="Vector2UI"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Vector2UI a, ref Vector2UI b, out Vector2UI result)
		{
			result.X = a.X - b.X;
			result.Y = a.Y - b.Y;
		}

		///<summary>Performs a subtract operation on two <see cref="Vector2UI"/>.</summary>
		///<param name="a">The first <see cref="Vector2UI"/> to subtract.</param>
		///<param name="b">The second <see cref="Vector2UI"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator -(Vector2UI a, Vector2UI b)
		{
			Subtract(ref a, ref b, out Vector2UI result);
			return result;
		}

		///<summary>Performs a subtract operation on a $<see cref="Vector2UI"/> and a $<see cref="uint"/>.</summary>
		///<param name="a">The <see cref="Vector2UI"/> to subtract.</param>
		///<param name="b">The <see cref="uint"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Vector2UI a, uint b, out Vector2UI result)
		{
			result.X = a.X - b;
			result.Y = a.Y - b;
		}

		///<summary>Performs a subtract operation on a $<see cref="Vector2UI"/> and a $<see cref="uint"/>.</summary>
		///<param name="a">The <see cref="Vector2UI"/> to subtract.</param>
		///<param name="b">The <see cref="uint"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator -(Vector2UI a, uint b)
		{
			Subtract(ref a, b, out Vector2UI result);
			return result;
		}

		///<summary>Performs a subtract operation on a $<see cref="uint"/> and a $<see cref="Vector2UI"/>.</summary>
		///<param name="a">The <see cref="uint"/> to subtract.</param>
		///<param name="b">The <see cref="Vector2UI"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator -(uint a, Vector2UI b)
		{
			Subtract(ref b, a, out Vector2UI result);
			return result;
		}


#endregion

#region division operators
		///<summary>Performs a divide operation on two <see cref="Vector2UI"/>.</summary>
		///<param name="a">The first <see cref="Vector2UI"/> to divide.</param>
		///<param name="b">The second <see cref="Vector2UI"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Vector2UI a, ref Vector2UI b, out Vector2UI result)
		{
			result.X = a.X / b.X;
			result.Y = a.Y / b.Y;
		}

		///<summary>Performs a divide operation on two <see cref="Vector2UI"/>.</summary>
		///<param name="a">The first <see cref="Vector2UI"/> to divide.</param>
		///<param name="b">The second <see cref="Vector2UI"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator /(Vector2UI a, Vector2UI b)
		{
			Divide(ref a, ref b, out Vector2UI result);
			return result;
		}

		///<summary>Performs a divide operation on a $<see cref="Vector2UI"/> and a $<see cref="uint"/>.</summary>
		///<param name="a">The <see cref="Vector2UI"/> to divide.</param>
		///<param name="b">The <see cref="uint"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Vector2UI a, uint b, out Vector2UI result)
		{
			result.X = a.X / b;
			result.Y = a.Y / b;
		}

		///<summary>Performs a divide operation on a $<see cref="Vector2UI"/> and a $<see cref="uint"/>.</summary>
		///<param name="a">The <see cref="Vector2UI"/> to divide.</param>
		///<param name="b">The <see cref="uint"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator /(Vector2UI a, uint b)
		{
			Divide(ref a, b, out Vector2UI result);
			return result;
		}

		///<summary>Performs a divide operation on a $<see cref="uint"/> and a $<see cref="Vector2UI"/>.</summary>
		///<param name="a">The <see cref="uint"/> to divide.</param>
		///<param name="b">The <see cref="Vector2UI"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator /(uint a, Vector2UI b)
		{
			Divide(ref b, a, out Vector2UI result);
			return result;
		}

#endregion

#region Multiply operators
		///<summary>Performs a multiply operation on two <see cref="Vector2UI"/>.</summary>
		///<param name="a">The first <see cref="Vector2UI"/> to multiply.</param>
		///<param name="b">The second <see cref="Vector2UI"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Vector2UI a, ref Vector2UI b, out Vector2UI result)
		{
			result.X = a.X * b.X;
			result.Y = a.Y * b.Y;
		}

		///<summary>Performs a multiply operation on two <see cref="Vector2UI"/>.</summary>
		///<param name="a">The first <see cref="Vector2UI"/> to multiply.</param>
		///<param name="b">The second <see cref="Vector2UI"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator *(Vector2UI a, Vector2UI b)
		{
			Multiply(ref a, ref b, out Vector2UI result);
			return result;
		}

		///<summary>Performs a multiply operation on a $<see cref="Vector2UI"/> and a $<see cref="uint"/>.</summary>
		///<param name="a">The <see cref="Vector2UI"/> to multiply.</param>
		///<param name="b">The <see cref="uint"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Vector2UI a, uint b, out Vector2UI result)
		{
			result.X = a.X * b;
			result.Y = a.Y * b;
		}

		///<summary>Performs a multiply operation on a $<see cref="Vector2UI"/> and a $<see cref="uint"/>.</summary>
		///<param name="a">The <see cref="Vector2UI"/> to multiply.</param>
		///<param name="b">The <see cref="uint"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator *(Vector2UI a, uint b)
		{
			Multiply(ref a, b, out Vector2UI result);
			return result;
		}

		///<summary>Performs a multiply operation on a $<see cref="uint"/> and a $<see cref="Vector2UI"/>.</summary>
		///<param name="a">The <see cref="uint"/> to multiply.</param>
		///<param name="b">The <see cref="Vector2UI"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI operator *(uint a, Vector2UI b)
		{
			Multiply(ref b, a, out Vector2UI result);
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
        public static bool operator ==(Vector2UI left, Vector2UI right)
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
        public static bool operator !=(Vector2UI left, Vector2UI right)
        {
            return !left.Equals(ref right);
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
        public static Vector2UI SmoothStep(ref Vector2UI start, ref Vector2UI end, float amount)
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
        public static Vector2UI SmoothStep(Vector2UI start, Vector2UI end, uint amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Vector2UI"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Vector2UI"/>.</param>
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
        public static void Orthogonalize(Vector2UI[] destination, params Vector2UI[] source)
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
                Vector2UI newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector2UI"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector2UI"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector2UI Swizzle(Vector2UI val, int xIndex, int yIndex)
        {
            return new Vector2UI()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector2UI Swizzle(Vector2UI val, uint xIndex, uint yIndex)
        {
            return new Vector2UI()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector2UI"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector2UI"/> source vector</param>
        /// <param name="right">Second <see cref="Vector2UI"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Dot(ref Vector2UI left, ref Vector2UI right)
        {
			return ((left.X * right.X) + (left.Y * right.Y));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector2UI"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector2UI"/> source vector</param>
        /// <param name="right">Second <see cref="Vector2UI"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Dot(Vector2UI left, Vector2UI right)
        {
			return ((left.X * right.X) + (left.Y * right.Y));
        }

		/// <summary>
        /// Returns a <see cref="Vector2UI"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector2UI"/> containing the 2D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector2UI"/> containing the 2D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector2UI"/> containing the 2D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector2UI Barycentric(ref Vector2UI value1, ref Vector2UI value2, ref Vector2UI value3, uint amount1, uint amount2)
        {
			return new Vector2UI(
				((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2UI"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2UI"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Vector2UI start, ref Vector2UI end, float amount, out Vector2UI result)
        {
			result.X = (uint)((1F - amount) * start.X + amount * end.X);
			result.Y = (uint)((1F - amount) * start.Y + amount * end.Y);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2UI"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2UI Lerp(Vector2UI start, Vector2UI end, float amount)
        {
			return new Vector2UI()
			{
				X = (uint)((1F - amount) * start.X + amount * end.X),
				Y = (uint)((1F - amount) * start.Y + amount * end.Y),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2UI"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2UI Lerp(ref Vector2UI start, ref Vector2UI end, float amount)
        {
			return new Vector2UI()
			{
				X = (uint)((1F - amount) * start.X + amount * end.X),
				Y = (uint)((1F - amount) * start.Y + amount * end.Y),
			};
        }

        /// <summary>
        /// Returns a <see cref="Vector2UI"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector2UI"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2UI"/>.</param>
        /// <returns>A <see cref="Vector2UI"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref Vector2UI left, ref Vector2UI right, out Vector2UI result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
		}

        /// <summary>
        /// Returns a <see cref="Vector2UI"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector2UI"/>.</param>
        /// <returns>A <see cref="Vector2UI"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI Min(ref Vector2UI left, ref Vector2UI right)
		{
			Min(ref left, ref right, out Vector2UI result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector2UI"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector2UI"/>.</param>
        /// <returns>A <see cref="Vector2UI"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI Min(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
			};
		}

        /// <summary>
        /// Returns a <see cref="Vector2UI"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector2UI"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2UI"/>.</param>
        /// <returns>A <see cref="Vector2UI"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref Vector2UI left, ref Vector2UI right, out Vector2UI result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
		}

        /// <summary>
        /// Returns a <see cref="Vector2UI"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector2UI"/>.</param>
        /// <returns>A <see cref="Vector2UI"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI Max(ref Vector2UI left, ref Vector2UI right)
		{
			Max(ref left, ref right, out Vector2UI result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector2UI"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector2UI"/>.</param>
        /// <returns>A <see cref="Vector2UI"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2UI Max(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2UI"/> vectors.
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
		public static uint DistanceSquared(ref Vector2UI value1, ref Vector2UI value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;

            return ((x * x) + (y * y));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Vector2UI"/> vectors.
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
		public static uint DistanceSquared(Vector2UI value1, Vector2UI value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;

            return ((x * x) + (y * y));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2UI"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2UI Clamp(Vector2UI value, uint min, uint max)
        {
			return new Vector2UI()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2UI"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2UI"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref Vector2UI value, ref Vector2UI min, ref Vector2UI max, out Vector2UI result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2UI"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2UI Clamp(Vector2UI value, Vector2UI min, Vector2UI max)
        {
			return new Vector2UI()
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
        public static Vector2UI Reflect(Vector2UI vector, Vector2UI normal)
        {
            return Reflect(ref vector, ref normal);
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal. 
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">Normal of the surface.</param>
        /// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
        /// whether the original vector was close enough to the surface to hit it.</remarks>
        public static Vector2UI Reflect(ref Vector2UI vector, ref Vector2UI normal)
        {
            uint dot = (vector.X * normal.X) + (vector.Y * normal.Y);

            return new Vector2UI()
            {
				X = (uint)(vector.X - ((2 * dot) * normal.X)),
				Y = (uint)(vector.Y - ((2 * dot) * normal.Y)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (uint x, uint y)(Vector2UI val)
        {
            return (val.X, val.Y);
        }

        public static implicit operator Vector2UI((uint x, uint y) val)
        {
            return new Vector2UI(val.x, val.y);
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of a component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on. This must be between 0 and 1</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 1].</exception>  
		public uint this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2UI run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2UI run from 0 to 1, inclusive.");
			}
		}
#endregion

#region Casts - vectors
		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="SByte2"/>.</summary>
		public static explicit operator SByte2(Vector2UI value)
		{
			return new SByte2((sbyte)value.X, (sbyte)value.Y);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="SByte3"/>.</summary>
		public static explicit operator SByte3(Vector2UI value)
		{
			return new SByte3((sbyte)value.X, (sbyte)value.Y, (sbyte)1);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="SByte4"/>.</summary>
		public static explicit operator SByte4(Vector2UI value)
		{
			return new SByte4((sbyte)value.X, (sbyte)value.Y, (sbyte)1, (sbyte)1);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Byte2"/>.</summary>
		public static explicit operator Byte2(Vector2UI value)
		{
			return new Byte2((byte)value.X, (byte)value.Y);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Byte3"/>.</summary>
		public static explicit operator Byte3(Vector2UI value)
		{
			return new Byte3((byte)value.X, (byte)value.Y, (byte)1);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Byte4"/>.</summary>
		public static explicit operator Byte4(Vector2UI value)
		{
			return new Byte4((byte)value.X, (byte)value.Y, (byte)1, (byte)1);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector2I"/>.</summary>
		public static explicit operator Vector2I(Vector2UI value)
		{
			return new Vector2I((int)value.X, (int)value.Y);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector3I"/>.</summary>
		public static explicit operator Vector3I(Vector2UI value)
		{
			return new Vector3I((int)value.X, (int)value.Y, 1);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector4I"/>.</summary>
		public static explicit operator Vector4I(Vector2UI value)
		{
			return new Vector4I((int)value.X, (int)value.Y, 1, 1);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector3UI"/>.</summary>
		public static explicit operator Vector3UI(Vector2UI value)
		{
			return new Vector3UI(value.X, value.Y, 1U);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector4UI"/>.</summary>
		public static explicit operator Vector4UI(Vector2UI value)
		{
			return new Vector4UI(value.X, value.Y, 1U, 1U);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector2S"/>.</summary>
		public static explicit operator Vector2S(Vector2UI value)
		{
			return new Vector2S((short)value.X, (short)value.Y);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector3S"/>.</summary>
		public static explicit operator Vector3S(Vector2UI value)
		{
			return new Vector3S((short)value.X, (short)value.Y, (short)1);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector4S"/>.</summary>
		public static explicit operator Vector4S(Vector2UI value)
		{
			return new Vector4S((short)value.X, (short)value.Y, (short)1, (short)1);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector2US"/>.</summary>
		public static explicit operator Vector2US(Vector2UI value)
		{
			return new Vector2US((ushort)value.X, (ushort)value.Y);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector3US"/>.</summary>
		public static explicit operator Vector3US(Vector2UI value)
		{
			return new Vector3US((ushort)value.X, (ushort)value.Y, (ushort)1);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector4US"/>.</summary>
		public static explicit operator Vector4US(Vector2UI value)
		{
			return new Vector4US((ushort)value.X, (ushort)value.Y, (ushort)1, (ushort)1);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector2L"/>.</summary>
		public static explicit operator Vector2L(Vector2UI value)
		{
			return new Vector2L((long)value.X, (long)value.Y);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector3L"/>.</summary>
		public static explicit operator Vector3L(Vector2UI value)
		{
			return new Vector3L((long)value.X, (long)value.Y, 1L);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector4L"/>.</summary>
		public static explicit operator Vector4L(Vector2UI value)
		{
			return new Vector4L((long)value.X, (long)value.Y, 1L, 1L);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector2UL"/>.</summary>
		public static explicit operator Vector2UL(Vector2UI value)
		{
			return new Vector2UL((ulong)value.X, (ulong)value.Y);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector3UL"/>.</summary>
		public static explicit operator Vector3UL(Vector2UI value)
		{
			return new Vector3UL((ulong)value.X, (ulong)value.Y, 1UL);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector4UL"/>.</summary>
		public static explicit operator Vector4UL(Vector2UI value)
		{
			return new Vector4UL((ulong)value.X, (ulong)value.Y, 1UL, 1UL);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector2F"/>.</summary>
		public static explicit operator Vector2F(Vector2UI value)
		{
			return new Vector2F((float)value.X, (float)value.Y);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector3F"/>.</summary>
		public static explicit operator Vector3F(Vector2UI value)
		{
			return new Vector3F((float)value.X, (float)value.Y, 1F);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector4F"/>.</summary>
		public static explicit operator Vector4F(Vector2UI value)
		{
			return new Vector4F((float)value.X, (float)value.Y, 1F, 1F);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector2D"/>.</summary>
		public static explicit operator Vector2D(Vector2UI value)
		{
			return new Vector2D((double)value.X, (double)value.Y);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector3D"/>.</summary>
		public static explicit operator Vector3D(Vector2UI value)
		{
			return new Vector3D((double)value.X, (double)value.Y, 1D);
		}

		///<summary>Casts a <see cref="Vector2UI"/> to a <see cref="Vector4D"/>.</summary>
		public static explicit operator Vector4D(Vector2UI value)
		{
			return new Vector4D((double)value.X, (double)value.Y, 1D, 1D);
		}

#endregion
	}
}

