using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten
{
	///<summary>A <see cref = "byte"/> vector comprised of four components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
    [Serializable]
	public partial struct Byte4 : IFormattable, IVector<byte>
	{
		///<summary>The X component.</summary>
        [DataMember]
		public byte X;

		///<summary>The Y component.</summary>
        [DataMember]
		public byte Y;

		///<summary>The Z component.</summary>
        [DataMember]
		public byte Z;

		///<summary>The W component.</summary>
        [DataMember]
		public byte W;

		///<summary>The size of <see cref="Byte4"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Byte4));

		///<summary>A Byte4 with every component set to (byte)1.</summary>
		public static readonly Byte4 One = new Byte4((byte)1, (byte)1, (byte)1, (byte)1);

		/// <summary>The X unit <see cref="Byte4"/>.</summary>
		public static readonly Byte4 UnitX = new Byte4((byte)1, (byte)0, (byte)0, (byte)0);

		/// <summary>The Y unit <see cref="Byte4"/>.</summary>
		public static readonly Byte4 UnitY = new Byte4((byte)0, (byte)1, (byte)0, (byte)0);

		/// <summary>The Z unit <see cref="Byte4"/>.</summary>
		public static readonly Byte4 UnitZ = new Byte4((byte)0, (byte)0, (byte)1, (byte)0);

		/// <summary>The W unit <see cref="Byte4"/>.</summary>
		public static readonly Byte4 UnitW = new Byte4((byte)0, (byte)0, (byte)0, (byte)1);

		/// <summary>Represents a zero'd Byte4.</summary>
		public static readonly Byte4 Zero = new Byte4((byte)0, (byte)0, (byte)0, (byte)0);

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == (byte)0 && Y == (byte)0 && Z == (byte)0 && W == (byte)0;
        }

#region Constructors
        ///<summary>Creates a new instance of <see cref = "Byte4"/>, using a <see cref="Byte2"/> to populate the first two components.</summary>
		public Byte4(Byte2 vector, byte z, byte w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
			W = w;
		}
        ///<summary>Creates a new instance of <see cref = "Byte4"/>, using a <see cref="Byte3"/> to populate the first three components.</summary>
		public Byte4(Byte3 vector, byte w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
			W = w;
		}

		///<summary>Creates a new instance of <see cref = "Byte4"/>.</summary>
		public Byte4(byte x, byte y, byte z, byte w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

        ///<summary>Creates a new instance of <see cref = "Byte4"/>.</summary>
		public Byte4(byte value)
		{
			X = value;
			Y = value;
			Z = value;
			W = value;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Byte4"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z and W components of the vector. This must be an array with 4 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Byte4(byte[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be 4 and only 4 input values for Byte4.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="Byte4"/> struct from an unsafe pointer. The pointer should point to an array of four elements.
        /// </summary>
		public unsafe Byte4(byte* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
			W = ptr[3];
		}
#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref="Byte4"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Byte4"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Byte4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Byte4 other)
        {
            return other.X == X && other.Y == Y && other.Z == Z && other.W == W;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Byte4"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Byte4"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Byte4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Byte4 other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Byte4"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Byte4"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Byte4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is not Byte4)
                return false;

            var strongValue = (Byte4)value;
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
        public byte LengthSquared()
        {
            return (byte)((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Byte4"/>.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public byte[] ToArray()
        {
            return new byte[] { X, Y, Z, W};
        }
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(byte min, byte max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
			W = W < min ? min : W > max ? max : W;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Byte4 min, Byte4 max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref Byte4 min, ref Byte4 max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
			W = W < min.W ? min.W : W > max.W ? max.W : W;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte4"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte4"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture), W.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte4"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte4"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte4"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte4"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte4"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte4"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
        }
#endregion

#region Add operators
        public static void Add(ref Byte4 left, ref Byte4 right, out Byte4 result)
        {
			result.X = (byte)(left.X + right.X);
			result.Y = (byte)(left.Y + right.Y);
			result.Z = (byte)(left.Z + right.Z);
			result.W = (byte)(left.W + right.W);
        }

        public static void Add(ref Byte4 left, byte right, out Byte4 result)
        {
			result.X = (byte)(left.X + right);
			result.Y = (byte)(left.Y + right);
			result.Z = (byte)(left.Z + right);
			result.W = (byte)(left.W + right);
        }

		public static Byte4 operator +(Byte4 left, Byte4 right)
		{
			Add(ref left, ref right, out Byte4 result);
            return result;
		}

		public static Byte4 operator +(Byte4 left, byte right)
		{
            Add(ref left, right, out Byte4 result);
            return result;
		}

        public static Byte4 operator +(byte left, Byte4 right)
		{
            Add(ref right, left, out Byte4 result);
            return result;
		}

		/// <summary>
        /// Assert a <see cref="Byte4"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Byte4"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Byte4"/>.</returns>
        public static Byte4 operator +(Byte4 value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static void Subtract(ref Byte4 left, ref Byte4 right, out Byte4 result)
        {
			result.X = (byte)(left.X - right.X);
			result.Y = (byte)(left.Y - right.Y);
			result.Z = (byte)(left.Z - right.Z);
			result.W = (byte)(left.W - right.W);
        }

        public static void Subtract(ref Byte4 left, byte right, out Byte4 result)
        {
			result.X = (byte)(left.X - right);
			result.Y = (byte)(left.Y - right);
			result.Z = (byte)(left.Z - right);
			result.W = (byte)(left.W - right);
        }

		public static Byte4 operator -(Byte4 left, Byte4 right)
		{
			Subtract(ref left, ref right, out Byte4 result);
            return result;
		}

		public static Byte4 operator -(Byte4 left, byte right)
		{
            Subtract(ref left, right, out Byte4 result);
            return result;
		}

        public static Byte4 operator -(byte left, Byte4 right)
		{
            Subtract(ref right, left, out Byte4 result);
            return result;
		}

#endregion

#region division operators
		public static void Divide(ref Byte4 left, ref Byte4 right, out Byte4 result)
        {
			result.X = (byte)(left.X / right.X);
			result.Y = (byte)(left.Y / right.Y);
			result.Z = (byte)(left.Z / right.Z);
			result.W = (byte)(left.W / right.W);
        }

        public static void Divide(ref Byte4 left, byte right, out Byte4 result)
        {
			result.X = (byte)(left.X / right);
			result.Y = (byte)(left.Y / right);
			result.Z = (byte)(left.Z / right);
			result.W = (byte)(left.W / right);
        }

		public static Byte4 operator /(Byte4 left, Byte4 right)
		{
			Divide(ref left, ref right, out Byte4 result);
            return result;
		}

		public static Byte4 operator /(Byte4 left, byte right)
		{
            Divide(ref left, right, out Byte4 result);
            return result;
		}

        public static Byte4 operator /(byte left, Byte4 right)
		{
            Divide(ref right, left, out Byte4 result);
            return result;
		}
#endregion

#region Multiply operators
		public static void Multiply(ref Byte4 left, ref Byte4 right, out Byte4 result)
        {
			result.X = (byte)(left.X * right.X);
			result.Y = (byte)(left.Y * right.Y);
			result.Z = (byte)(left.Z * right.Z);
			result.W = (byte)(left.W * right.W);
        }

        public static void Multiply(ref Byte4 left, byte right, out Byte4 result)
        {
			result.X = (byte)(left.X * right);
			result.Y = (byte)(left.Y * right);
			result.Z = (byte)(left.Z * right);
			result.W = (byte)(left.W * right);
        }

		public static Byte4 operator *(Byte4 left, Byte4 right)
		{
			Multiply(ref left, ref right, out Byte4 result);
            return result;
		}

		public static Byte4 operator *(Byte4 left, byte right)
		{
            Multiply(ref left, right, out Byte4 result);
            return result;
		}

        public static Byte4 operator *(byte left, Byte4 right)
		{
            Multiply(ref right, left, out Byte4 result);
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
        public static bool operator ==(Byte4 left, Byte4 right)
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
        public static bool operator !=(Byte4 left, Byte4 right)
        {
            return !left.Equals(ref right);
        }
#endregion

#region Operators - Cast
        ///<summary>Casts a <see cref="Byte4"/> to a <see cref="Byte2"/>.</summary>
        public static explicit operator Byte2(Byte4 value)
        {
            return new Byte2(value.X, value.Y);
        }

        ///<summary>Casts a <see cref="Byte4"/> to a <see cref="Byte3"/>.</summary>
        public static explicit operator Byte3(Byte4 value)
        {
            return new Byte3(value.X, value.Y, value.Z);
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
        public static Byte4 SmoothStep(ref Byte4 start, ref Byte4 end, float amount)
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
        public static Byte4 SmoothStep(Byte4 start, Byte4 end, byte amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Byte4"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Byte4"/>.</param>
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
        public static void Orthogonalize(Byte4[] destination, params Byte4[] source)
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
                Byte4 newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (byte)(Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Byte4"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Byte4"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
		/// <param name="zIndex">The axis index to use for the new Z value.</param>
		/// <param name="wIndex">The axis index to use for the new W value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Byte4 Swizzle(Byte4 val, int xIndex, int yIndex, int zIndex, int wIndex)
        {
            return new Byte4()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
			   Z = (&val.X)[zIndex],
			   W = (&val.X)[wIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Byte4 Swizzle(Byte4 val, uint xIndex, uint yIndex, uint zIndex, uint wIndex)
        {
            return new Byte4()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
			    Z = (&val.X)[zIndex],
			    W = (&val.X)[wIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Byte4"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Byte4"/> source vector</param>
        /// <param name="right">Second <see cref="Byte4"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Dot(ref Byte4 left, ref Byte4 right)
        {
			return (byte)(((byte)left.X * right.X) + ((byte)left.Y * right.Y) + ((byte)left.Z * right.Z) + ((byte)left.W * right.W));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Byte4"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Byte4"/> source vector</param>
        /// <param name="right">Second <see cref="Byte4"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Dot(Byte4 left, Byte4 right)
        {
			return (byte)((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W));
        }

		/// <summary>
        /// Returns a <see cref="Byte4"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Byte4"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Byte4"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Byte4"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Byte4 Barycentric(ref Byte4 value1, ref Byte4 value2, ref Byte4 value3, byte amount1, byte amount2)
        {
			return new Byte4(
				(byte)((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				(byte)((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))), 
				(byte)((value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z))), 
				(byte)((value1.W + (amount1 * (value2.W - value1.W))) + (amount2 * (value3.W - value1.W)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Byte4"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Byte4"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Byte4 start, ref Byte4 end, float amount, out Byte4 result)
        {
			result.X = (byte)((1F - amount) * start.X + amount * end.X);
			result.Y = (byte)((1F - amount) * start.Y + amount * end.Y);
			result.Z = (byte)((1F - amount) * start.Z + amount * end.Z);
			result.W = (byte)((1F - amount) * start.W + amount * end.W);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Byte4"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte4 Lerp(Byte4 start, Byte4 end, float amount)
        {
			return new Byte4()
			{
				X = (byte)((1F - amount) * start.X + amount * end.X),
				Y = (byte)((1F - amount) * start.Y + amount * end.Y),
				Z = (byte)((1F - amount) * start.Z + amount * end.Z),
				W = (byte)((1F - amount) * start.W + amount * end.W),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Byte4"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte4 Lerp(ref Byte4 start, ref Byte4 end, float amount)
        {
			return new Byte4()
			{
				X = (byte)((1F - amount) * start.X + amount * end.X),
				Y = (byte)((1F - amount) * start.Y + amount * end.Y),
				Z = (byte)((1F - amount) * start.Z + amount * end.Z),
				W = (byte)((1F - amount) * start.W + amount * end.W),
			};
        }

        /// <summary>
        /// Returns a <see cref="Byte4"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte4"/>.</param>
        /// <param name="right">The second source <see cref="Byte4"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Byte4"/>.</param>
        /// <returns>A <see cref="Byte4"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref Byte4 left, ref Byte4 right, out Byte4 result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
				result.Z = (left.Z < right.Z) ? left.Z : right.Z;
				result.W = (left.W < right.W) ? left.W : right.W;
		}

        /// <summary>
        /// Returns a <see cref="Byte4"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte4"/>.</param>
        /// <param name="right">The second source <see cref="Byte4"/>.</param>
        /// <returns>A <see cref="Byte4"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 Min(ref Byte4 left, ref Byte4 right)
		{
			Min(ref left, ref right, out Byte4 result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Byte4"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte4"/>.</param>
        /// <param name="right">The second source <see cref="Byte4"/>.</param>
        /// <returns>A <see cref="Byte4"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 Min(Byte4 left, Byte4 right)
		{
			return new Byte4()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
				W = (left.W < right.W) ? left.W : right.W,
			};
		}

        /// <summary>
        /// Returns a <see cref="Byte4"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte4"/>.</param>
        /// <param name="right">The second source <see cref="Byte4"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Byte4"/>.</param>
        /// <returns>A <see cref="Byte4"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref Byte4 left, ref Byte4 right, out Byte4 result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
				result.Z = (left.Z > right.Z) ? left.Z : right.Z;
				result.W = (left.W > right.W) ? left.W : right.W;
		}

        /// <summary>
        /// Returns a <see cref="Byte4"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte4"/>.</param>
        /// <param name="right">The second source <see cref="Byte4"/>.</param>
        /// <returns>A <see cref="Byte4"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 Max(ref Byte4 left, ref Byte4 right)
		{
			Max(ref left, ref right, out Byte4 result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Byte4"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte4"/>.</param>
        /// <param name="right">The second source <see cref="Byte4"/>.</param>
        /// <returns>A <see cref="Byte4"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 Max(Byte4 left, Byte4 right)
		{
			return new Byte4()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
				W = (left.W > right.W) ? left.W : right.W,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Byte4"/> vectors.
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
		public static byte DistanceSquared(ref Byte4 value1, ref Byte4 value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;
            int w = value1.W - value2.W;

            return (byte)((x * x) + (y * y) + (z * z) + (w * w));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Byte4"/> vectors.
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
		public static byte DistanceSquared(Byte4 value1, Byte4 value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;
            int w = value1.W - value2.W;

            return (byte)((x * x) + (y * y) + (z * z) + (w * w));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Byte4"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte4 Clamp(Byte4 value, byte min, byte max)
        {
			return new Byte4()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
				W = value.W < min ? min : value.W > max ? max : value.W,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Byte4"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Byte4"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref Byte4 value, ref Byte4 min, ref Byte4 max, out Byte4 result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
				result.Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z;
				result.W = value.W < min.W ? min.W : value.W > max.W ? max.W : value.W;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Byte4"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte4 Clamp(Byte4 value, Byte4 min, Byte4 max)
        {
			return new Byte4()
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
        public static Byte4 Reflect(ref Byte4 vector, ref Byte4 normal)
        {
            int dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z) + (vector.W * normal.W);

            return new Byte4()
            {
				X = (byte)(vector.X - ((2 * dot) * normal.X)),
				Y = (byte)(vector.Y - ((2 * dot) * normal.Y)),
				Z = (byte)(vector.Z - ((2 * dot) * normal.Z)),
				W = (byte)(vector.W - ((2 * dot) * normal.W)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (byte x, byte y, byte z, byte w)(Byte4 val)
        {
            return (val.X, val.Y, val.Z, val.W);
        }

        public static implicit operator Byte4((byte x, byte y, byte z, byte w) val)
        {
            return new Byte4(val.x, val.y, val.z, val.w);
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, Z or W component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>  
		public byte this[int index]
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
				throw new ArgumentOutOfRangeException("index", "Indices for Byte4 run from 0 to 3, inclusive.");
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
				throw new ArgumentOutOfRangeException("index", "Indices for Byte4 run from 0 to 3, inclusive.");
			}
		}
#endregion

#region Casts - vectors
        ///<summary>Casts a <see cref="Byte4"/> to a <see cref="SByte4"/>.</summary>
        public static explicit operator SByte4(Byte4 val)
        {
            return new SByte4()
            {
                X = (sbyte)val.X,
                Y = (sbyte)val.Y,
                Z = (sbyte)val.Z,
                W = (sbyte)val.W,
            };
        }

        ///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4I"/>.</summary>
        public static explicit operator Vector4I(Byte4 val)
        {
            return new Vector4I()
            {
                X = (int)val.X,
                Y = (int)val.Y,
                Z = (int)val.Z,
                W = (int)val.W,
            };
        }

        ///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4UI"/>.</summary>
        public static explicit operator Vector4UI(Byte4 val)
        {
            return new Vector4UI()
            {
                X = val.X,
                Y = val.Y,
                Z = val.Z,
                W = val.W,
            };
        }

        ///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4S"/>.</summary>
        public static explicit operator Vector4S(Byte4 val)
        {
            return new Vector4S()
            {
                X = (short)val.X,
                Y = (short)val.Y,
                Z = (short)val.Z,
                W = (short)val.W,
            };
        }

        ///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4US"/>.</summary>
        public static explicit operator Vector4US(Byte4 val)
        {
            return new Vector4US()
            {
                X = val.X,
                Y = val.Y,
                Z = val.Z,
                W = val.W,
            };
        }

        ///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4L"/>.</summary>
        public static explicit operator Vector4L(Byte4 val)
        {
            return new Vector4L()
            {
                X = (long)val.X,
                Y = (long)val.Y,
                Z = (long)val.Z,
                W = (long)val.W,
            };
        }

        ///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4UL"/>.</summary>
        public static explicit operator Vector4UL(Byte4 val)
        {
            return new Vector4UL()
            {
                X = val.X,
                Y = val.Y,
                Z = val.Z,
                W = val.W,
            };
        }

        ///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4F"/>.</summary>
        public static explicit operator Vector4F(Byte4 val)
        {
            return new Vector4F()
            {
                X = (float)val.X,
                Y = (float)val.Y,
                Z = (float)val.Z,
                W = (float)val.W,
            };
        }

        ///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4D"/>.</summary>
        public static explicit operator Vector4D(Byte4 val)
        {
            return new Vector4D()
            {
                X = (double)val.X,
                Y = (double)val.Y,
                Z = (double)val.Z,
                W = (double)val.W,
            };
        }

#endregion
	}
}

