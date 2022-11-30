using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten
{
	///<summary>A <see cref = "byte"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
    [Serializable]
	public partial struct Byte2 : IFormattable, IVector<byte>
	{
		///<summary>The X component.</summary>
        [DataMember]
		public byte X;

		///<summary>The Y component.</summary>
        [DataMember]
		public byte Y;

		///<summary>The size of <see cref="Byte2"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Byte2));

		///<summary>A Byte2 with every component set to 1.</summary>
		public static readonly Byte2 One = new Byte2(1, 1);

		/// <summary>The X unit <see cref="Byte2"/>.</summary>
		public static readonly Byte2 UnitX = new Byte2(1, 0);

		/// <summary>The Y unit <see cref="Byte2"/>.</summary>
		public static readonly Byte2 UnitY = new Byte2(0, 1);

		/// <summary>Represents a zero'd Byte2.</summary>
		public static readonly Byte2 Zero = new Byte2(0, 0);

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0 && Y == 0;
        }

#region Constructors

		///<summary>Creates a new instance of <see cref = "Byte2"/>.</summary>
		public Byte2(byte x, byte y)
		{
			X = x;
			Y = y;
		}

        ///<summary>Creates a new instance of <see cref = "Byte2"/>.</summary>
		public Byte2(byte value)
		{
			X = value;
			Y = value;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Byte2"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Byte2(byte[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Byte2.");

			X = values[0];
			Y = values[1];
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="Byte2"/> struct from an unsafe pointer. The pointer should point to an array of two elements.
        /// </summary>
		public unsafe Byte2(byte* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
		}
#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref="Byte2"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Byte2"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Byte2"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Byte2 other)
        {
            return other.X == X && other.Y == Y;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Byte2"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Byte2"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Byte2"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Byte2 other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Byte2"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Byte2"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Byte2"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is not Byte2)
                return false;

            var strongValue = (Byte2)value;
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
        public byte LengthSquared()
        {
            return (byte)((X * X) + (Y * Y));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Byte2"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public byte[] ToArray()
        {
            return new byte[] { X, Y};
        }
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(byte min, byte max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Byte2 min, Byte2 max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref Byte2 min, ref Byte2 max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
        }
#endregion

#region Add operators
        public static void Add(ref Byte2 left, ref Byte2 right, out Byte2 result)
        {
			result.X = (byte)(left.X + right.X);
			result.Y = (byte)(left.Y + right.Y);
        }

        public static void Add(ref Byte2 left, byte right, out Byte2 result)
        {
			result.X = (byte)(left.X + right);
			result.Y = (byte)(left.Y + right);
        }

		public static Byte2 operator +(Byte2 left, Byte2 right)
		{
			Add(ref left, ref right, out Byte2 result);
            return result;
		}

		public static Byte2 operator +(Byte2 left, byte right)
		{
            Add(ref left, right, out Byte2 result);
            return result;
		}

        public static Byte2 operator +(byte left, Byte2 right)
		{
            Add(ref right, left, out Byte2 result);
            return result;
		}

		/// <summary>
        /// Assert a <see cref="Byte2"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Byte2"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Byte2"/>.</returns>
        public static Byte2 operator +(Byte2 value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static void Subtract(ref Byte2 left, ref Byte2 right, out Byte2 result)
        {
			result.X = (byte)(left.X - right.X);
			result.Y = (byte)(left.Y - right.Y);
        }

        public static void Subtract(ref Byte2 left, byte right, out Byte2 result)
        {
			result.X = (byte)(left.X - right);
			result.Y = (byte)(left.Y - right);
        }

		public static Byte2 operator -(Byte2 left, Byte2 right)
		{
			Subtract(ref left, ref right, out Byte2 result);
            return result;
		}

		public static Byte2 operator -(Byte2 left, byte right)
		{
            Subtract(ref left, right, out Byte2 result);
            return result;
		}

        public static Byte2 operator -(byte left, Byte2 right)
		{
            Subtract(ref right, left, out Byte2 result);
            return result;
		}

#endregion

#region division operators
		public static void Divide(ref Byte2 left, ref Byte2 right, out Byte2 result)
        {
			result.X = (byte)(left.X / right.X);
			result.Y = (byte)(left.Y / right.Y);
        }

        public static void Divide(ref Byte2 left, byte right, out Byte2 result)
        {
			result.X = (byte)(left.X / right);
			result.Y = (byte)(left.Y / right);
        }

		public static Byte2 operator /(Byte2 left, Byte2 right)
		{
			Divide(ref left, ref right, out Byte2 result);
            return result;
		}

		public static Byte2 operator /(Byte2 left, byte right)
		{
            Divide(ref left, right, out Byte2 result);
            return result;
		}

        public static Byte2 operator /(byte left, Byte2 right)
		{
            Divide(ref right, left, out Byte2 result);
            return result;
		}
#endregion

#region Multiply operators
		public static void Multiply(ref Byte2 left, ref Byte2 right, out Byte2 result)
        {
			result.X = (byte)(left.X * right.X);
			result.Y = (byte)(left.Y * right.Y);
        }

        public static void Multiply(ref Byte2 left, byte right, out Byte2 result)
        {
			result.X = (byte)(left.X * right);
			result.Y = (byte)(left.Y * right);
        }

		public static Byte2 operator *(Byte2 left, Byte2 right)
		{
			Multiply(ref left, ref right, out Byte2 result);
            return result;
		}

		public static Byte2 operator *(Byte2 left, byte right)
		{
            Multiply(ref left, right, out Byte2 result);
            return result;
		}

        public static Byte2 operator *(byte left, Byte2 right)
		{
            Multiply(ref right, left, out Byte2 result);
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
        public static bool operator ==(Byte2 left, Byte2 right)
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
        public static bool operator !=(Byte2 left, Byte2 right)
        {
            return !left.Equals(ref right);
        }
#endregion

#region Operators - Cast
        ///<summary>Casts a <see cref="Byte2"/> to a <see cref="Byte3"/>.</summary>
        public static explicit operator Byte3(Byte2 value)
        {
            return new Byte3(value.X, value.Y, 0);
        }

        ///<summary>Casts a <see cref="Byte2"/> to a <see cref="Byte4"/>.</summary>
        public static explicit operator Byte4(Byte2 value)
        {
            return new Byte4(value.X, value.Y, 0, 0);
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
        public static Byte2 SmoothStep(ref Byte2 start, ref Byte2 end, float amount)
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
        public static Byte2 SmoothStep(Byte2 start, Byte2 end, byte amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Byte2"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Byte2"/>.</param>
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
        public static void Orthogonalize(Byte2[] destination, params Byte2[] source)
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
                Byte2 newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (byte)(Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Byte2"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Byte2"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Byte2 Swizzle(Byte2 val, int xIndex, int yIndex)
        {
            return new Byte2()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Byte2 Swizzle(Byte2 val, uint xIndex, uint yIndex)
        {
            return new Byte2()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Byte2"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Byte2"/> source vector</param>
        /// <param name="right">Second <see cref="Byte2"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Dot(ref Byte2 left, ref Byte2 right)
        {
			return (byte)(((byte)left.X * right.X) + ((byte)left.Y * right.Y));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Byte2"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Byte2"/> source vector</param>
        /// <param name="right">Second <see cref="Byte2"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Dot(Byte2 left, Byte2 right)
        {
			return (byte)((left.X * right.X) + (left.Y * right.Y));
        }

		/// <summary>
        /// Returns a <see cref="Byte2"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Byte2"/> containing the 2D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Byte2"/> containing the 2D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Byte2"/> containing the 2D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Byte2 Barycentric(ref Byte2 value1, ref Byte2 value2, ref Byte2 value3, byte amount1, byte amount2)
        {
			return new Byte2(
				(byte)((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				(byte)((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Byte2"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Byte2"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Byte2 start, ref Byte2 end, float amount, out Byte2 result)
        {
			result.X = (byte)((1F - amount) * start.X + amount * end.X);
			result.Y = (byte)((1F - amount) * start.Y + amount * end.Y);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Byte2"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte2 Lerp(Byte2 start, Byte2 end, float amount)
        {
			return new Byte2()
			{
				X = (byte)((1F - amount) * start.X + amount * end.X),
				Y = (byte)((1F - amount) * start.Y + amount * end.Y),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Byte2"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte2 Lerp(ref Byte2 start, ref Byte2 end, float amount)
        {
			return new Byte2()
			{
				X = (byte)((1F - amount) * start.X + amount * end.X),
				Y = (byte)((1F - amount) * start.Y + amount * end.Y),
			};
        }

        /// <summary>
        /// Returns a <see cref="Byte2"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte2"/>.</param>
        /// <param name="right">The second source <see cref="Byte2"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Byte2"/>.</param>
        /// <returns>A <see cref="Byte2"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref Byte2 left, ref Byte2 right, out Byte2 result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
		}

        /// <summary>
        /// Returns a <see cref="Byte2"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte2"/>.</param>
        /// <param name="right">The second source <see cref="Byte2"/>.</param>
        /// <returns>A <see cref="Byte2"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte2 Min(ref Byte2 left, ref Byte2 right)
		{
			Min(ref left, ref right, out Byte2 result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Byte2"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte2"/>.</param>
        /// <param name="right">The second source <see cref="Byte2"/>.</param>
        /// <returns>A <see cref="Byte2"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte2 Min(Byte2 left, Byte2 right)
		{
			return new Byte2()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
			};
		}

        /// <summary>
        /// Returns a <see cref="Byte2"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte2"/>.</param>
        /// <param name="right">The second source <see cref="Byte2"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Byte2"/>.</param>
        /// <returns>A <see cref="Byte2"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref Byte2 left, ref Byte2 right, out Byte2 result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
		}

        /// <summary>
        /// Returns a <see cref="Byte2"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte2"/>.</param>
        /// <param name="right">The second source <see cref="Byte2"/>.</param>
        /// <returns>A <see cref="Byte2"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte2 Max(ref Byte2 left, ref Byte2 right)
		{
			Max(ref left, ref right, out Byte2 result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Byte2"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte2"/>.</param>
        /// <param name="right">The second source <see cref="Byte2"/>.</param>
        /// <returns>A <see cref="Byte2"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte2 Max(Byte2 left, Byte2 right)
		{
			return new Byte2()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Byte2"/> vectors.
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
		public static byte DistanceSquared(ref Byte2 value1, ref Byte2 value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;

            return (byte)((x * x) + (y * y));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Byte2"/> vectors.
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
		public static byte DistanceSquared(Byte2 value1, Byte2 value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;

            return (byte)((x * x) + (y * y));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Byte2"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte2 Clamp(Byte2 value, byte min, byte max)
        {
			return new Byte2()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Byte2"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Byte2"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref Byte2 value, ref Byte2 min, ref Byte2 max, out Byte2 result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Byte2"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte2 Clamp(Byte2 value, Byte2 min, Byte2 max)
        {
			return new Byte2()
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
        public static Byte2 Reflect(ref Byte2 vector, ref Byte2 normal)
        {
            int dot = (vector.X * normal.X) + (vector.Y * normal.Y);

            return new Byte2()
            {
				X = (byte)(vector.X - ((2 * dot) * normal.X)),
				Y = (byte)(vector.Y - ((2 * dot) * normal.Y)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (byte x, byte y)(Byte2 val)
        {
            return (val.X, val.Y);
        }

        public static implicit operator Byte2((byte x, byte y) val)
        {
            return new Byte2(val.x, val.y);
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
		public byte this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Byte2 run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Byte2 run from 0 to 1, inclusive.");
			}
		}
#endregion

#region Casts - vectors
        ///<summary>Casts a <see cref="Byte2"/> to a <see cref="SByte2"/>.</summary>
        public static explicit operator SByte2(Byte2 val)
        {
            return new SByte2()
            {
                X = (sbyte)val.X,
                Y = (sbyte)val.Y,
            };
        }

        ///<summary>Casts a <see cref="Byte2"/> to a <see cref="Vector2I"/>.</summary>
        public static explicit operator Vector2I(Byte2 val)
        {
            return new Vector2I()
            {
                X = (int)val.X,
                Y = (int)val.Y,
            };
        }

        ///<summary>Casts a <see cref="Byte2"/> to a <see cref="Vector2UI"/>.</summary>
        public static explicit operator Vector2UI(Byte2 val)
        {
            return new Vector2UI()
            {
                X = val.X,
                Y = val.Y,
            };
        }

        ///<summary>Casts a <see cref="Byte2"/> to a <see cref="Vector2S"/>.</summary>
        public static explicit operator Vector2S(Byte2 val)
        {
            return new Vector2S()
            {
                X = (short)val.X,
                Y = (short)val.Y,
            };
        }

        ///<summary>Casts a <see cref="Byte2"/> to a <see cref="Vector2US"/>.</summary>
        public static explicit operator Vector2US(Byte2 val)
        {
            return new Vector2US()
            {
                X = val.X,
                Y = val.Y,
            };
        }

        ///<summary>Casts a <see cref="Byte2"/> to a <see cref="Vector2L"/>.</summary>
        public static explicit operator Vector2L(Byte2 val)
        {
            return new Vector2L()
            {
                X = (long)val.X,
                Y = (long)val.Y,
            };
        }

        ///<summary>Casts a <see cref="Byte2"/> to a <see cref="Vector2UL"/>.</summary>
        public static explicit operator Vector2UL(Byte2 val)
        {
            return new Vector2UL()
            {
                X = val.X,
                Y = val.Y,
            };
        }

        ///<summary>Casts a <see cref="Byte2"/> to a <see cref="Vector2F"/>.</summary>
        public static explicit operator Vector2F(Byte2 val)
        {
            return new Vector2F()
            {
                X = (float)val.X,
                Y = (float)val.Y,
            };
        }

        ///<summary>Casts a <see cref="Byte2"/> to a <see cref="Vector2D"/>.</summary>
        public static explicit operator Vector2D(Byte2 val)
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

