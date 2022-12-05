using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten
{
	///<summary>A <see cref = "uint"/> vector comprised of four components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
    [Serializable]
	public partial struct Vector4UI : IFormattable, IVector<uint>
	{
		///<summary>The size of <see cref="Vector4UI"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4UI));

		///<summary>A Vector4UI with every component set to 1U.</summary>
		public static readonly Vector4UI One = new Vector4UI(1U, 1U, 1U, 1U);

        static readonly string toStringFormat = "X:{0} Y:{1} Z:{2} W:{3}";

		/// <summary>The X unit <see cref="Vector4UI"/>.</summary>
		public static readonly Vector4UI UnitX = new Vector4UI(1U, 0U, 0U, 0U);

		/// <summary>The Y unit <see cref="Vector4UI"/>.</summary>
		public static readonly Vector4UI UnitY = new Vector4UI(0U, 1U, 0U, 0U);

		/// <summary>The Z unit <see cref="Vector4UI"/>.</summary>
		public static readonly Vector4UI UnitZ = new Vector4UI(0U, 0U, 1U, 0U);

		/// <summary>The W unit <see cref="Vector4UI"/>.</summary>
		public static readonly Vector4UI UnitW = new Vector4UI(0U, 0U, 0U, 1U);

		/// <summary>Represents a zero'd Vector4UI.</summary>
		public static readonly Vector4UI Zero = new Vector4UI(0U, 0U, 0U, 0U);

		/// <summary>The X component.</summary>
		[DataMember]
		public uint X;

		/// <summary>The Y component.</summary>
		[DataMember]
		public uint Y;

		/// <summary>The Z component.</summary>
		[DataMember]
		public uint Z;

		/// <summary>The W component.</summary>
		[DataMember]
		public uint W;


        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0U && Y == 0U && Z == 0U && W == 0U;
        }

#region Constructors
		/// <summary>Initializes a new instance of <see cref="Vector4UI"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Vector4UI(uint value)
		{
			X = value;
			Y = value;
			Z = value;
			W = value;
		}
		/// <summary>Initializes a new instance of <see cref="Vector4UI"/> from an array.</summary>
		/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least four elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Vector4UI(uint[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least four input values for Vector4UI.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
		}
		/// <summary>Initializes a new instance of <see cref="Vector4UI"/> from a span.</summary>
		/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least four elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Vector4UI(Span<uint> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least four input values for Vector4UI.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
		}
		/// <summary>Initializes a new instance of <see cref="Vector4UI"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the X, Y, Z, W components of the color.
		/// <para>There must be at least four elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than four elements.</exception>
		public unsafe Vector4UI(uint* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			X = ptrValues[0];
			Y = ptrValues[1];
			Z = ptrValues[2];
			W = ptrValues[3];
		}
		/// <summary>
		/// Initializes a new instance of <see cref="Vector4UI"/>.
		/// </summary>
		/// <param name="x">The X component.</param>
		/// <param name="y">The Y component.</param>
		/// <param name="z">The Z component.</param>
		/// <param name="w">The W component.</param>
		public Vector4UI(uint x, uint y, uint z, uint w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		///<summary>Creates a new instance of <see cref="Vector4UI"/>, using a <see cref="Vector2UI"/> to populate the first two components.</summary>
		public Vector4UI(Vector2UI vector, uint z, uint w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
			W = w;
		}

		///<summary>Creates a new instance of <see cref="Vector4UI"/>, using a <see cref="Vector3UI"/> to populate the first three components.</summary>
		public Vector4UI(Vector3UI vector, uint w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
			W = w;
		}

#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref = "Vector4UI"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector4UI"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector4UI"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector4UI other)
        {
            return other.X == X && other.Y == Y && other.Z == Z && other.W == W;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4UI"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector4UI"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector4UI"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector4UI other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4UI"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector4UI"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector4UI"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is not Vector4UI)
                return false;

            var strongValue = (Vector4UI)value;
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
                hashCode = (hashCode * 397) ^ W.GetHashCode();
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
            return ((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector4UI"/>.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public uint[] ToArray()
        {
            return new uint[] { X, Y, Z, W };
        }
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(uint min, uint max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
			W = W < min ? min : W > max ? max : W;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector4UI min, Vector4UI max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref Vector4UI min, ref Vector4UI max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
			W = W < min.W ? min.W : W > max.W ? max.W : W;
        }
#endregion

#region To-String
		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4UI"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4UI"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, format, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4UI"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4UI"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, toStringFormat, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4UI"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4UI"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, toStringFormat, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4UI"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4UI"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider,
                toStringFormat,
				X.ToString(format, formatProvider),
				Y.ToString(format, formatProvider),
				Z.ToString(format, formatProvider),
				W.ToString(format, formatProvider)
            );
        }
#endregion

#region Add operators
		///<summary>Performs a add operation on two <see cref="Vector4UI"/>.</summary>
		///<param name="a">The first <see cref="Vector4UI"/> to add.</param>
		///<param name="b">The second <see cref="Vector4UI"/>to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Vector4UI a, ref Vector4UI b, out Vector4UI result)
		{
			result.X = a.X + b.X;
			result.Y = a.Y + b.Y;
			result.Z = a.Z + b.Z;
			result.W = a.W + b.W;
		}

		///<summary>Performs a add operation on two <see cref="Vector4UI"/>.</summary>
		///<param name="a">The first <see cref="Vector4UI"/> to add.</param>
		///<param name="b">The second <see cref="Vector4UI"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4UI operator +(Vector4UI a, Vector4UI b)
		{
			Add(ref a, ref b, out Vector4UI result);
			return result;
		}

        public static void Add(ref Vector4UI left, uint right, out Vector4UI result)
        {
			result.X = (left.X + right);
			result.Y = (left.Y + right);
			result.Z = (left.Z + right);
			result.W = (left.W + right);
        }

		public static Vector4UI operator +(Vector4UI left, uint right)
		{
            Add(ref left, right, out Vector4UI result);
            return result;
		}

        public static Vector4UI operator +(uint left, Vector4UI right)
		{
            Add(ref right, left, out Vector4UI result);
            return result;
		}

		/// <summary>
        /// Assert a <see cref="Vector4UI"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector4UI"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector4UI"/>.</returns>
        public static Vector4UI operator +(Vector4UI value)
        {
            return value;
        }
#endregion

#region Subtract operators
		///<summary>Performs a subtract operation on two <see cref="Vector4UI"/>.</summary>
		///<param name="a">The first <see cref="Vector4UI"/> to add.</param>
		///<param name="b">The second <see cref="Vector4UI"/>to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Vector4UI a, ref Vector4UI b, out Vector4UI result)
		{
			result.X = a.X - b.X;
			result.Y = a.Y - b.Y;
			result.Z = a.Z - b.Z;
			result.W = a.W - b.W;
		}

		///<summary>Performs a subtract operation on two <see cref="Vector4UI"/>.</summary>
		///<param name="a">The first <see cref="Vector4UI"/> to add.</param>
		///<param name="b">The second <see cref="Vector4UI"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4UI operator -(Vector4UI a, Vector4UI b)
		{
			Subtract(ref a, ref b, out Vector4UI result);
			return result;
		}

        public static void Subtract(ref Vector4UI left, uint right, out Vector4UI result)
        {
			result.X = (left.X - right);
			result.Y = (left.Y - right);
			result.Z = (left.Z - right);
			result.W = (left.W - right);
        }

		public static Vector4UI operator -(Vector4UI left, uint right)
		{
            Subtract(ref left, right, out Vector4UI result);
            return result;
		}

        public static Vector4UI operator -(uint left, Vector4UI right)
		{
            Subtract(ref right, left, out Vector4UI result);
            return result;
		}

#endregion

#region division operators
		///<summary>Performs a divide operation on two <see cref="Vector4UI"/>.</summary>
		///<param name="a">The first <see cref="Vector4UI"/> to add.</param>
		///<param name="b">The second <see cref="Vector4UI"/>to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Vector4UI a, ref Vector4UI b, out Vector4UI result)
		{
			result.X = a.X / b.X;
			result.Y = a.Y / b.Y;
			result.Z = a.Z / b.Z;
			result.W = a.W / b.W;
		}

		///<summary>Performs a divide operation on two <see cref="Vector4UI"/>.</summary>
		///<param name="a">The first <see cref="Vector4UI"/> to add.</param>
		///<param name="b">The second <see cref="Vector4UI"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4UI operator /(Vector4UI a, Vector4UI b)
		{
			Divide(ref a, ref b, out Vector4UI result);
			return result;
		}

        public static void Divide(ref Vector4UI left, uint right, out Vector4UI result)
        {
			result.X = (left.X / right);
			result.Y = (left.Y / right);
			result.Z = (left.Z / right);
			result.W = (left.W / right);
        }

		public static Vector4UI operator /(Vector4UI left, uint right)
		{
            Divide(ref left, right, out Vector4UI result);
            return result;
		}

        public static Vector4UI operator /(uint left, Vector4UI right)
		{
            Divide(ref right, left, out Vector4UI result);
            return result;
		}
#endregion

#region Multiply operators
		///<summary>Performs a multiply operation on two <see cref="Vector4UI"/>.</summary>
		///<param name="a">The first <see cref="Vector4UI"/> to add.</param>
		///<param name="b">The second <see cref="Vector4UI"/>to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Vector4UI a, ref Vector4UI b, out Vector4UI result)
		{
			result.X = a.X * b.X;
			result.Y = a.Y * b.Y;
			result.Z = a.Z * b.Z;
			result.W = a.W * b.W;
		}

		///<summary>Performs a multiply operation on two <see cref="Vector4UI"/>.</summary>
		///<param name="a">The first <see cref="Vector4UI"/> to add.</param>
		///<param name="b">The second <see cref="Vector4UI"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4UI operator *(Vector4UI a, Vector4UI b)
		{
			Multiply(ref a, ref b, out Vector4UI result);
			return result;
		}

        public static void Multiply(ref Vector4UI left, uint right, out Vector4UI result)
        {
			result.X = (left.X * right);
			result.Y = (left.Y * right);
			result.Z = (left.Z * right);
			result.W = (left.W * right);
        }

		public static Vector4UI operator *(Vector4UI left, uint right)
		{
            Multiply(ref left, right, out Vector4UI result);
            return result;
		}

        public static Vector4UI operator *(uint left, Vector4UI right)
		{
            Multiply(ref right, left, out Vector4UI result);
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
        public static bool operator ==(Vector4UI left, Vector4UI right)
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
        public static bool operator !=(Vector4UI left, Vector4UI right)
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
        public static Vector4UI SmoothStep(ref Vector4UI start, ref Vector4UI end, float amount)
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
        public static Vector4UI SmoothStep(Vector4UI start, Vector4UI end, uint amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Vector4UI"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Vector4UI"/>.</param>
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
        public static void Orthogonalize(Vector4UI[] destination, params Vector4UI[] source)
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
                Vector4UI newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector4UI"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector4UI"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
		/// <param name="zIndex">The axis index to use for the new Z value.</param>
		/// <param name="wIndex">The axis index to use for the new W value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector4UI Swizzle(Vector4UI val, int xIndex, int yIndex, int zIndex, int wIndex)
        {
            return new Vector4UI()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
			   Z = (&val.X)[zIndex],
			   W = (&val.X)[wIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector4UI Swizzle(Vector4UI val, uint xIndex, uint yIndex, uint zIndex, uint wIndex)
        {
            return new Vector4UI()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
			    Z = (&val.X)[zIndex],
			    W = (&val.X)[wIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector4UI"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector4UI"/> source vector</param>
        /// <param name="right">Second <see cref="Vector4UI"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Dot(ref Vector4UI left, ref Vector4UI right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector4UI"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector4UI"/> source vector</param>
        /// <param name="right">Second <see cref="Vector4UI"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Dot(Vector4UI left, Vector4UI right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W));
        }

		/// <summary>
        /// Returns a <see cref="Vector4UI"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector4UI"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector4UI"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector4UI"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector4UI Barycentric(ref Vector4UI value1, ref Vector4UI value2, ref Vector4UI value3, uint amount1, uint amount2)
        {
			return new Vector4UI(
				((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))), 
				((value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z))), 
				((value1.W + (amount1 * (value2.W - value1.W))) + (amount2 * (value3.W - value1.W)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector4UI"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4UI"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Vector4UI start, ref Vector4UI end, float amount, out Vector4UI result)
        {
			result.X = (uint)((1F - amount) * start.X + amount * end.X);
			result.Y = (uint)((1F - amount) * start.Y + amount * end.Y);
			result.Z = (uint)((1F - amount) * start.Z + amount * end.Z);
			result.W = (uint)((1F - amount) * start.W + amount * end.W);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector4UI"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4UI Lerp(Vector4UI start, Vector4UI end, float amount)
        {
			return new Vector4UI()
			{
				X = (uint)((1F - amount) * start.X + amount * end.X),
				Y = (uint)((1F - amount) * start.Y + amount * end.Y),
				Z = (uint)((1F - amount) * start.Z + amount * end.Z),
				W = (uint)((1F - amount) * start.W + amount * end.W),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector4UI"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4UI Lerp(ref Vector4UI start, ref Vector4UI end, float amount)
        {
			return new Vector4UI()
			{
				X = (uint)((1F - amount) * start.X + amount * end.X),
				Y = (uint)((1F - amount) * start.Y + amount * end.Y),
				Z = (uint)((1F - amount) * start.Z + amount * end.Z),
				W = (uint)((1F - amount) * start.W + amount * end.W),
			};
        }

        /// <summary>
        /// Returns a <see cref="Vector4UI"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector4UI"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4UI"/>.</param>
        /// <returns>A <see cref="Vector4UI"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref Vector4UI left, ref Vector4UI right, out Vector4UI result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
				result.Z = (left.Z < right.Z) ? left.Z : right.Z;
				result.W = (left.W < right.W) ? left.W : right.W;
		}

        /// <summary>
        /// Returns a <see cref="Vector4UI"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector4UI"/>.</param>
        /// <returns>A <see cref="Vector4UI"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4UI Min(ref Vector4UI left, ref Vector4UI right)
		{
			Min(ref left, ref right, out Vector4UI result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector4UI"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector4UI"/>.</param>
        /// <returns>A <see cref="Vector4UI"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4UI Min(Vector4UI left, Vector4UI right)
		{
			return new Vector4UI()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
				W = (left.W < right.W) ? left.W : right.W,
			};
		}

        /// <summary>
        /// Returns a <see cref="Vector4UI"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector4UI"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4UI"/>.</param>
        /// <returns>A <see cref="Vector4UI"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref Vector4UI left, ref Vector4UI right, out Vector4UI result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
				result.Z = (left.Z > right.Z) ? left.Z : right.Z;
				result.W = (left.W > right.W) ? left.W : right.W;
		}

        /// <summary>
        /// Returns a <see cref="Vector4UI"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector4UI"/>.</param>
        /// <returns>A <see cref="Vector4UI"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4UI Max(ref Vector4UI left, ref Vector4UI right)
		{
			Max(ref left, ref right, out Vector4UI result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector4UI"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector4UI"/>.</param>
        /// <returns>A <see cref="Vector4UI"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4UI Max(Vector4UI left, Vector4UI right)
		{
			return new Vector4UI()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
				W = (left.W > right.W) ? left.W : right.W,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4UI"/> vectors.
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
		public static uint DistanceSquared(ref Vector4UI value1, ref Vector4UI value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;
            uint z = value1.Z - value2.Z;
            uint w = value1.W - value2.W;

            return ((x * x) + (y * y) + (z * z) + (w * w));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Vector4UI"/> vectors.
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
		public static uint DistanceSquared(Vector4UI value1, Vector4UI value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;
            uint z = value1.Z - value2.Z;
            uint w = value1.W - value2.W;

            return ((x * x) + (y * y) + (z * z) + (w * w));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector4UI"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4UI Clamp(Vector4UI value, uint min, uint max)
        {
			return new Vector4UI()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
				W = value.W < min ? min : value.W > max ? max : value.W,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector4UI"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4UI"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref Vector4UI value, ref Vector4UI min, ref Vector4UI max, out Vector4UI result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
				result.Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z;
				result.W = value.W < min.W ? min.W : value.W > max.W ? max.W : value.W;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector4UI"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4UI Clamp(Vector4UI value, Vector4UI min, Vector4UI max)
        {
			return new Vector4UI()
			{
				X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
				Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y,
				Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z,
				W = value.W < min.W ? min.W : value.W > max.W ? max.W : value.W,
			};
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal. 
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">Normal of the surface.</param>
        /// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
        /// whether the original vector was close enough to the surface to hit it.</remarks>
        public static Vector4UI Reflect(ref Vector4UI vector, ref Vector4UI normal)
        {
            uint dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z) + (vector.W * normal.W);

            return new Vector4UI()
            {
				X = (uint)(vector.X - ((2 * dot) * normal.X)),
				Y = (uint)(vector.Y - ((2 * dot) * normal.Y)),
				Z = (uint)(vector.Z - ((2 * dot) * normal.Z)),
				W = (uint)(vector.W - ((2 * dot) * normal.W)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (uint x, uint y, uint z, uint w)(Vector4UI val)
        {
            return (val.X, val.Y, val.Z, val.W);
        }

        public static implicit operator Vector4UI((uint x, uint y, uint z, uint w) val)
        {
            return new Vector4UI(val.x, val.y, val.z, val.w);
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of a component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on. This must be between 0 and 3</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>  
		public uint this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
					case 3: return W;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4UI run from 0 to 3, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
					case 3: W = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4UI run from 0 to 3, inclusive.");
			}
		}
#endregion

#region Casts - vectors
		public static explicit operator SByte2(Vector4UI value)
		{
			return new SByte2((sbyte)value.X, (sbyte)value.Y);
		}

		public static explicit operator SByte3(Vector4UI value)
		{
			return new SByte3((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z);
		}

		public static explicit operator SByte4(Vector4UI value)
		{
			return new SByte4((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z, (sbyte)value.W);
		}

		public static explicit operator Byte2(Vector4UI value)
		{
			return new Byte2((byte)value.X, (byte)value.Y);
		}

		public static explicit operator Byte3(Vector4UI value)
		{
			return new Byte3((byte)value.X, (byte)value.Y, (byte)value.Z);
		}

		public static explicit operator Byte4(Vector4UI value)
		{
			return new Byte4((byte)value.X, (byte)value.Y, (byte)value.Z, (byte)value.W);
		}

		public static explicit operator Vector2I(Vector4UI value)
		{
			return new Vector2I((int)value.X, (int)value.Y);
		}

		public static explicit operator Vector3I(Vector4UI value)
		{
			return new Vector3I((int)value.X, (int)value.Y, (int)value.Z);
		}

		public static explicit operator Vector4I(Vector4UI value)
		{
			return new Vector4I((int)value.X, (int)value.Y, (int)value.Z, (int)value.W);
		}

		public static explicit operator Vector2UI(Vector4UI value)
		{
			return new Vector2UI(value.X, value.Y);
		}

		public static explicit operator Vector3UI(Vector4UI value)
		{
			return new Vector3UI(value.X, value.Y, value.Z);
		}

		public static explicit operator Vector2S(Vector4UI value)
		{
			return new Vector2S((short)value.X, (short)value.Y);
		}

		public static explicit operator Vector3S(Vector4UI value)
		{
			return new Vector3S((short)value.X, (short)value.Y, (short)value.Z);
		}

		public static explicit operator Vector4S(Vector4UI value)
		{
			return new Vector4S((short)value.X, (short)value.Y, (short)value.Z, (short)value.W);
		}

		public static explicit operator Vector2US(Vector4UI value)
		{
			return new Vector2US((ushort)value.X, (ushort)value.Y);
		}

		public static explicit operator Vector3US(Vector4UI value)
		{
			return new Vector3US((ushort)value.X, (ushort)value.Y, (ushort)value.Z);
		}

		public static explicit operator Vector4US(Vector4UI value)
		{
			return new Vector4US((ushort)value.X, (ushort)value.Y, (ushort)value.Z, (ushort)value.W);
		}

		public static explicit operator Vector2L(Vector4UI value)
		{
			return new Vector2L((long)value.X, (long)value.Y);
		}

		public static explicit operator Vector3L(Vector4UI value)
		{
			return new Vector3L((long)value.X, (long)value.Y, (long)value.Z);
		}

		public static explicit operator Vector4L(Vector4UI value)
		{
			return new Vector4L((long)value.X, (long)value.Y, (long)value.Z, (long)value.W);
		}

		public static explicit operator Vector2UL(Vector4UI value)
		{
			return new Vector2UL((ulong)value.X, (ulong)value.Y);
		}

		public static explicit operator Vector3UL(Vector4UI value)
		{
			return new Vector3UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z);
		}

		public static explicit operator Vector4UL(Vector4UI value)
		{
			return new Vector4UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z, (ulong)value.W);
		}

		public static explicit operator Vector2F(Vector4UI value)
		{
			return new Vector2F((float)value.X, (float)value.Y);
		}

		public static explicit operator Vector3F(Vector4UI value)
		{
			return new Vector3F((float)value.X, (float)value.Y, (float)value.Z);
		}

		public static explicit operator Vector4F(Vector4UI value)
		{
			return new Vector4F((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
		}

		public static explicit operator Vector2D(Vector4UI value)
		{
			return new Vector2D((double)value.X, (double)value.Y);
		}

		public static explicit operator Vector3D(Vector4UI value)
		{
			return new Vector3D((double)value.X, (double)value.Y, (double)value.Z);
		}

		public static explicit operator Vector4D(Vector4UI value)
		{
			return new Vector4D((double)value.X, (double)value.Y, (double)value.Z, (double)value.W);
		}

#endregion
	}
}

