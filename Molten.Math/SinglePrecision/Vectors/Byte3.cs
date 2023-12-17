using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten
{
	///<summary>A <see cref="byte"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Explicit)]
    [DataContract]
	public partial struct Byte3 : IFormattable, IUnsignedVector<Byte3, byte>, IEquatable<Byte3>
	{
		///<summary>The size of <see cref="Byte3"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Byte3));

        ///<summary>The number of elements in the current vector type.</summary>
        public static readonly int NumElements = 3;

		///<summary>A Byte3 with every component set to (byte)1.</summary>
		public static readonly Byte3 One = new Byte3((byte)1, (byte)1, (byte)1);

        static readonly string toStringFormat = "X:{0} Y:{1} Z:{2}";

		/// <summary>The X unit <see cref="Byte3"/>.</summary>
		public static readonly Byte3 UnitX = new Byte3((byte)1, (byte)0, (byte)0);

		/// <summary>The Y unit <see cref="Byte3"/>.</summary>
		public static readonly Byte3 UnitY = new Byte3((byte)0, (byte)1, (byte)0);

		/// <summary>The Z unit <see cref="Byte3"/>.</summary>
		public static readonly Byte3 UnitZ = new Byte3((byte)0, (byte)0, (byte)1);

		/// <summary>Represents a zero'd Byte3.</summary>
		public static readonly Byte3 Zero = new Byte3((byte)0, (byte)0, (byte)0);

		/// <summary>The X component.</summary>
		[DataMember]
		[FieldOffset(0)]
		public byte X;

		/// <summary>The Y component.</summary>
		[DataMember]
		[FieldOffset(1)]
		public byte Y;

		/// <summary>The Z component.</summary>
		[DataMember]
		[FieldOffset(2)]
		public byte Z;

		/// <summary>A fixed array mapped to the same memory space as the individual <see cref="Byte3"/> components.</summary>
		[IgnoreDataMember]
		[FieldOffset(0)]
		public unsafe fixed byte Values[3];

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == (byte)0 && Y == (byte)0 && Z == (byte)0;
        }

#region Constructors
		/// <summary>
		/// Initializes a new instance of <see cref="Byte3"/>.
		/// </summary>
		/// <param name="x">The X component.</param>
		/// <param name="y">The Y component.</param>
		/// <param name="z">The Z component.</param>
		public Byte3(byte x, byte y, byte z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		/// <summary>Initializes a new instance of <see cref="Byte3"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Byte3(byte value)
		{
			X = value;
			Y = value;
			Z = value;
		}
		/// <summary>Initializes a new instance of <see cref="Byte3"/> from an array.</summary>
		/// <param name="values">The values to assign to the X, Y, Z components of the color. This must be an array with at least three elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 3 elements.</exception>
		public unsafe Byte3(byte[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 3)
				throw new ArgumentOutOfRangeException("values", "There must be at least 3 input values for Byte3.");

			fixed (byte* src = values)
			{
				fixed (byte* dst = Values)
					Unsafe.CopyBlock(src, dst, (sizeof(byte) * 3));
			}
		}
		/// <summary>Initializes a new instance of <see cref="Byte3"/> from a span.</summary>
		/// <param name="values">The values to assign to the X, Y, Z components of the color. This must be an array with at least three elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 3 elements.</exception>
		public Byte3(Span<byte> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 3)
				throw new ArgumentOutOfRangeException("values", "There must be at least three input values for Byte3.");

			X = values[0];
			Y = values[1];
			Z = values[2];
		}
		/// <summary>Initializes a new instance of <see cref="Byte3"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the X, Y, Z components of the color.
		/// <para>There must be at least three elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than 3 elements.</exception>
		public unsafe Byte3(byte* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			X = ptrValues[0];
			Y = ptrValues[1];
			Z = ptrValues[2];
		}

		///<summary>Creates a new instance of <see cref="Byte3"/>, using a <see cref="Byte2"/> to populate the first two components.</summary>
		public Byte3(Byte2 vector, byte z)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
		}

#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref = "Byte3"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Byte3"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Byte3"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Byte3 other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Byte3"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Byte3"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Byte3"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Byte3 other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Byte3"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Byte3"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Byte3"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is Byte3 v)
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
        public byte LengthSquared()
        {
            return (byte)((X * X) + (Y * Y) + (Z * Z));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Byte3"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public byte[] ToArray()
        {
            return [X, Y, Z];
        }
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(byte min, byte max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Byte3 min, Byte3 max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref Byte3 min, ref Byte3 max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
        }
#endregion

#region To-String
		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Byte3"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Byte3"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, format, X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Byte3"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Byte3"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, toStringFormat, X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Byte3"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Byte3"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, toStringFormat, X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Byte3"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Byte3"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider,
                toStringFormat,
				X.ToString(format, formatProvider),
				Y.ToString(format, formatProvider),
				Z.ToString(format, formatProvider)
            );
        }
#endregion

#region Add operators
		///<summary>Performs a add operation on two <see cref="Byte3"/>.</summary>
		///<param name="a">The first <see cref="Byte3"/> to add.</param>
		///<param name="b">The second <see cref="Byte3"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Byte3 a, ref Byte3 b, out Byte3 result)
		{
			result.X = (byte)(a.X + b.X);
			result.Y = (byte)(a.Y + b.Y);
			result.Z = (byte)(a.Z + b.Z);
		}

		///<summary>Performs a add operation on two <see cref="Byte3"/>.</summary>
		///<param name="a">The first <see cref="Byte3"/> to add.</param>
		///<param name="b">The second <see cref="Byte3"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator +(Byte3 a, Byte3 b)
		{
			Add(ref a, ref b, out Byte3 result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="Byte3"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte3"/> to add.</param>
		///<param name="b">The <see cref="byte"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Byte3 a, byte b, out Byte3 result)
		{
			result.X = (byte)(a.X + b);
			result.Y = (byte)(a.Y + b);
			result.Z = (byte)(a.Z + b);
		}

		///<summary>Performs a add operation on a <see cref="Byte3"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte3"/> to add.</param>
		///<param name="b">The <see cref="byte"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator +(Byte3 a, byte b)
		{
			Add(ref a, b, out Byte3 result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="byte"/> and a <see cref="Byte3"/>.</summary>
		///<param name="a">The <see cref="byte"/> to add.</param>
		///<param name="b">The <see cref="Byte3"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator +(byte a, Byte3 b)
		{
			Add(ref b, a, out Byte3 result);
			return result;
		}


		/// <summary>
        /// Assert a <see cref="Byte3"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Byte3"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Byte3"/>.</returns>
        public static Byte3 operator +(Byte3 value)
        {
            return value;
        }
#endregion

#region Subtract operators
		///<summary>Performs a subtract operation on two <see cref="Byte3"/>.</summary>
		///<param name="a">The first <see cref="Byte3"/> to subtract.</param>
		///<param name="b">The second <see cref="Byte3"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Byte3 a, ref Byte3 b, out Byte3 result)
		{
			result.X = (byte)(a.X - b.X);
			result.Y = (byte)(a.Y - b.Y);
			result.Z = (byte)(a.Z - b.Z);
		}

		///<summary>Performs a subtract operation on two <see cref="Byte3"/>.</summary>
		///<param name="a">The first <see cref="Byte3"/> to subtract.</param>
		///<param name="b">The second <see cref="Byte3"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator -(Byte3 a, Byte3 b)
		{
			Subtract(ref a, ref b, out Byte3 result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="Byte3"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte3"/> to subtract.</param>
		///<param name="b">The <see cref="byte"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Byte3 a, byte b, out Byte3 result)
		{
			result.X = (byte)(a.X - b);
			result.Y = (byte)(a.Y - b);
			result.Z = (byte)(a.Z - b);
		}

		///<summary>Performs a subtract operation on a <see cref="Byte3"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte3"/> to subtract.</param>
		///<param name="b">The <see cref="byte"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator -(Byte3 a, byte b)
		{
			Subtract(ref a, b, out Byte3 result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="byte"/> and a <see cref="Byte3"/>.</summary>
		///<param name="a">The <see cref="byte"/> to subtract.</param>
		///<param name="b">The <see cref="Byte3"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator -(byte a, Byte3 b)
		{
			Subtract(ref b, a, out Byte3 result);
			return result;
		}


#endregion

#region division operators
		///<summary>Performs a divide operation on two <see cref="Byte3"/>.</summary>
		///<param name="a">The first <see cref="Byte3"/> to divide.</param>
		///<param name="b">The second <see cref="Byte3"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Byte3 a, ref Byte3 b, out Byte3 result)
		{
			result.X = (byte)(a.X / b.X);
			result.Y = (byte)(a.Y / b.Y);
			result.Z = (byte)(a.Z / b.Z);
		}

		///<summary>Performs a divide operation on two <see cref="Byte3"/>.</summary>
		///<param name="a">The first <see cref="Byte3"/> to divide.</param>
		///<param name="b">The second <see cref="Byte3"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator /(Byte3 a, Byte3 b)
		{
			Divide(ref a, ref b, out Byte3 result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="Byte3"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte3"/> to divide.</param>
		///<param name="b">The <see cref="byte"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Byte3 a, byte b, out Byte3 result)
		{
			result.X = (byte)(a.X / b);
			result.Y = (byte)(a.Y / b);
			result.Z = (byte)(a.Z / b);
		}

		///<summary>Performs a divide operation on a <see cref="Byte3"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte3"/> to divide.</param>
		///<param name="b">The <see cref="byte"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator /(Byte3 a, byte b)
		{
			Divide(ref a, b, out Byte3 result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="byte"/> and a <see cref="Byte3"/>.</summary>
		///<param name="a">The <see cref="byte"/> to divide.</param>
		///<param name="b">The <see cref="Byte3"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator /(byte a, Byte3 b)
		{
			Divide(ref b, a, out Byte3 result);
			return result;
		}

#endregion

#region Multiply operators
		///<summary>Performs a multiply operation on two <see cref="Byte3"/>.</summary>
		///<param name="a">The first <see cref="Byte3"/> to multiply.</param>
		///<param name="b">The second <see cref="Byte3"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Byte3 a, ref Byte3 b, out Byte3 result)
		{
			result.X = (byte)(a.X * b.X);
			result.Y = (byte)(a.Y * b.Y);
			result.Z = (byte)(a.Z * b.Z);
		}

		///<summary>Performs a multiply operation on two <see cref="Byte3"/>.</summary>
		///<param name="a">The first <see cref="Byte3"/> to multiply.</param>
		///<param name="b">The second <see cref="Byte3"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator *(Byte3 a, Byte3 b)
		{
			Multiply(ref a, ref b, out Byte3 result);
			return result;
		}

		///<summary>Performs a multiply operation on a <see cref="Byte3"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte3"/> to multiply.</param>
		///<param name="b">The <see cref="byte"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Byte3 a, byte b, out Byte3 result)
		{
			result.X = (byte)(a.X * b);
			result.Y = (byte)(a.Y * b);
			result.Z = (byte)(a.Z * b);
		}

		///<summary>Performs a multiply operation on a <see cref="Byte3"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte3"/> to multiply.</param>
		///<param name="b">The <see cref="byte"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator *(Byte3 a, byte b)
		{
			Multiply(ref a, b, out Byte3 result);
			return result;
		}

		///<summary>Performs a multiply operation on a <see cref="byte"/> and a <see cref="Byte3"/>.</summary>
		///<param name="a">The <see cref="byte"/> to multiply.</param>
		///<param name="b">The <see cref="Byte3"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 operator *(byte a, Byte3 b)
		{
			Multiply(ref b, a, out Byte3 result);
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
        public static bool operator ==(Byte3 left, Byte3 right)
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
        public static bool operator !=(Byte3 left, Byte3 right)
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
        public static Byte3 SmoothStep(ref Byte3 start, ref Byte3 end, float amount)
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
        public static Byte3 SmoothStep(Byte3 start, Byte3 end, byte amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Byte3"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Byte3"/>.</param>
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
        public static void Orthogonalize(Byte3[] destination, params Byte3[] source)
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
                Byte3 newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (byte)(Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Byte3"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Byte3"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
		/// <param name="zIndex">The axis index to use for the new Z value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Byte3 Swizzle(Byte3 val, int xIndex, int yIndex, int zIndex)
        {
            return new Byte3()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
			   Z = (&val.X)[zIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Byte3 Swizzle(Byte3 val, uint xIndex, uint yIndex, uint zIndex)
        {
            return new Byte3()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
			    Z = (&val.X)[zIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Byte3"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Byte3"/> source vector</param>
        /// <param name="right">Second <see cref="Byte3"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Dot(ref Byte3 left, ref Byte3 right)
        {
			return (byte)(((byte)left.X * right.X) + ((byte)left.Y * right.Y) + ((byte)left.Z * right.Z));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Byte3"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Byte3"/> source vector</param>
        /// <param name="right">Second <see cref="Byte3"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Dot(Byte3 left, Byte3 right)
        {
			return (byte)((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

		/// <summary>
        /// Returns a <see cref="Byte3"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Byte3"/> containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Byte3"/> containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Byte3"/> containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Byte3 Barycentric(ref Byte3 value1, ref Byte3 value2, ref Byte3 value3, byte amount1, byte amount2)
        {
			return new Byte3(
				(byte)((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				(byte)((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))), 
				(byte)((value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Byte3"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Byte3"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Byte3 start, ref Byte3 end, float amount, out Byte3 result)
        {
			result.X = (byte)((1F - amount) * start.X + amount * end.X);
			result.Y = (byte)((1F - amount) * start.Y + amount * end.Y);
			result.Z = (byte)((1F - amount) * start.Z + amount * end.Z);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Byte3"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte3 Lerp(Byte3 start, Byte3 end, float amount)
        {
			return new Byte3()
			{
				X = (byte)((1F - amount) * start.X + amount * end.X),
				Y = (byte)((1F - amount) * start.Y + amount * end.Y),
				Z = (byte)((1F - amount) * start.Z + amount * end.Z),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Byte3"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte3 Lerp(ref Byte3 start, ref Byte3 end, float amount)
        {
			return new Byte3()
			{
				X = (byte)((1F - amount) * start.X + amount * end.X),
				Y = (byte)((1F - amount) * start.Y + amount * end.Y),
				Z = (byte)((1F - amount) * start.Z + amount * end.Z),
			};
        }

        /// <summary>
        /// Returns a <see cref="Byte3"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte3"/>.</param>
        /// <param name="right">The second source <see cref="Byte3"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Byte3"/>.</param>
        /// <returns>A <see cref="Byte3"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref Byte3 left, ref Byte3 right, out Byte3 result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
				result.Z = (left.Z < right.Z) ? left.Z : right.Z;
		}

        /// <summary>
        /// Returns a <see cref="Byte3"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte3"/>.</param>
        /// <param name="right">The second source <see cref="Byte3"/>.</param>
        /// <returns>A <see cref="Byte3"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 Min(ref Byte3 left, ref Byte3 right)
		{
			Min(ref left, ref right, out Byte3 result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Byte3"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte3"/>.</param>
        /// <param name="right">The second source <see cref="Byte3"/>.</param>
        /// <returns>A <see cref="Byte3"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 Min(Byte3 left, Byte3 right)
		{
			return new Byte3()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

        /// <summary>
        /// Returns a <see cref="Byte3"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte3"/>.</param>
        /// <param name="right">The second source <see cref="Byte3"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Byte3"/>.</param>
        /// <returns>A <see cref="Byte3"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref Byte3 left, ref Byte3 right, out Byte3 result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
				result.Z = (left.Z > right.Z) ? left.Z : right.Z;
		}

        /// <summary>
        /// Returns a <see cref="Byte3"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte3"/>.</param>
        /// <param name="right">The second source <see cref="Byte3"/>.</param>
        /// <returns>A <see cref="Byte3"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 Max(ref Byte3 left, ref Byte3 right)
		{
			Max(ref left, ref right, out Byte3 result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Byte3"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte3"/>.</param>
        /// <param name="right">The second source <see cref="Byte3"/>.</param>
        /// <returns>A <see cref="Byte3"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte3 Max(Byte3 left, Byte3 right)
		{
			return new Byte3()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Byte3"/> vectors.
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
		public static byte DistanceSquared(ref Byte3 value1, ref Byte3 value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;

            return (byte)((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Byte3"/> vectors.
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
		public static byte DistanceSquared(Byte3 value1, Byte3 value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;

            return (byte)((x * x) + (y * y) + (z * z));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Byte3"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte3 Clamp(Byte3 value, byte min, byte max)
        {
			return new Byte3()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Byte3"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Byte3"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref Byte3 value, ref Byte3 min, ref Byte3 max, out Byte3 result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
				result.Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Byte3"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte3 Clamp(Byte3 value, Byte3 min, Byte3 max)
        {
			return new Byte3()
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
        public static Byte3 Reflect(Byte3 vector, Byte3 normal)
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
        public static Byte3 Reflect(ref Byte3 vector, ref Byte3 normal)
        {
            int dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z);

            return new Byte3()
            {
				X = (byte)(vector.X - ((2 * dot) * normal.X)),
				Y = (byte)(vector.Y - ((2 * dot) * normal.Y)),
				Z = (byte)(vector.Z - ((2 * dot) * normal.Z)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (byte x, byte y, byte z)(Byte3 val)
        {
            return (val.X, val.Y, val.Z);
        }

        public static implicit operator Byte3((byte x, byte y, byte z) val)
        {
            return new Byte3(val.x, val.y, val.z);
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of a component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on. This must be between 0 and 2</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="index"/> is outside the range [0, 2].</exception>  
		public unsafe byte this[int index]
		{
			get
            {
                if(index > 2 || index < 0)
                    throw new IndexOutOfRangeException("Index for Byte3 must be between from 0 to 2, inclusive.");

                return Values[index];
            }
            set
            {
                if (index > 2 || index < 0)
                    throw new IndexOutOfRangeException("Index for Byte3 must be between from 0 to 2, inclusive.");

                Values[index] = value;
            }
		}

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of a component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on. This must be between 0 and 2</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="index"/> is greater than 2.</exception>  
		public unsafe byte this[uint index]
		{
			get
            {
                if(index > 2)
                    throw new IndexOutOfRangeException("Index for Byte3 must be between from 0 to 2, inclusive.");

                return Values[index];
            }
            set
            {
                if (index > 2)
                    throw new IndexOutOfRangeException("Index for Byte3 must be between from 0 to 2, inclusive.");

                Values[index] = value;
            }
		}
#endregion

#region Casts - vectors
		///<summary>Casts a <see cref="Byte3"/> to a <see cref="SByte2"/>.</summary>
		public static explicit operator SByte2(Byte3 value)
		{
			return new SByte2((sbyte)value.X, (sbyte)value.Y);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="SByte3"/>.</summary>
		public static explicit operator SByte3(Byte3 value)
		{
			return new SByte3((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="SByte4"/>.</summary>
		public static explicit operator SByte4(Byte3 value)
		{
			return new SByte4((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z, (sbyte)1);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Byte2"/>.</summary>
		public static explicit operator Byte2(Byte3 value)
		{
			return new Byte2(value.X, value.Y);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Byte4"/>.</summary>
		public static explicit operator Byte4(Byte3 value)
		{
			return new Byte4(value.X, value.Y, value.Z, (byte)1);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector2I"/>.</summary>
		public static explicit operator Vector2I(Byte3 value)
		{
			return new Vector2I((int)value.X, (int)value.Y);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector3I"/>.</summary>
		public static explicit operator Vector3I(Byte3 value)
		{
			return new Vector3I((int)value.X, (int)value.Y, (int)value.Z);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector4I"/>.</summary>
		public static explicit operator Vector4I(Byte3 value)
		{
			return new Vector4I((int)value.X, (int)value.Y, (int)value.Z, 1);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector2UI"/>.</summary>
		public static explicit operator Vector2UI(Byte3 value)
		{
			return new Vector2UI((uint)value.X, (uint)value.Y);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector3UI"/>.</summary>
		public static explicit operator Vector3UI(Byte3 value)
		{
			return new Vector3UI((uint)value.X, (uint)value.Y, (uint)value.Z);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector4UI"/>.</summary>
		public static explicit operator Vector4UI(Byte3 value)
		{
			return new Vector4UI((uint)value.X, (uint)value.Y, (uint)value.Z, 1U);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector2S"/>.</summary>
		public static explicit operator Vector2S(Byte3 value)
		{
			return new Vector2S((short)value.X, (short)value.Y);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector3S"/>.</summary>
		public static explicit operator Vector3S(Byte3 value)
		{
			return new Vector3S((short)value.X, (short)value.Y, (short)value.Z);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector4S"/>.</summary>
		public static explicit operator Vector4S(Byte3 value)
		{
			return new Vector4S((short)value.X, (short)value.Y, (short)value.Z, (short)1);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector2US"/>.</summary>
		public static explicit operator Vector2US(Byte3 value)
		{
			return new Vector2US((ushort)value.X, (ushort)value.Y);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector3US"/>.</summary>
		public static explicit operator Vector3US(Byte3 value)
		{
			return new Vector3US((ushort)value.X, (ushort)value.Y, (ushort)value.Z);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector4US"/>.</summary>
		public static explicit operator Vector4US(Byte3 value)
		{
			return new Vector4US((ushort)value.X, (ushort)value.Y, (ushort)value.Z, (ushort)1);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector2L"/>.</summary>
		public static explicit operator Vector2L(Byte3 value)
		{
			return new Vector2L((long)value.X, (long)value.Y);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector3L"/>.</summary>
		public static explicit operator Vector3L(Byte3 value)
		{
			return new Vector3L((long)value.X, (long)value.Y, (long)value.Z);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector4L"/>.</summary>
		public static explicit operator Vector4L(Byte3 value)
		{
			return new Vector4L((long)value.X, (long)value.Y, (long)value.Z, 1L);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector2UL"/>.</summary>
		public static explicit operator Vector2UL(Byte3 value)
		{
			return new Vector2UL((ulong)value.X, (ulong)value.Y);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector3UL"/>.</summary>
		public static explicit operator Vector3UL(Byte3 value)
		{
			return new Vector3UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector4UL"/>.</summary>
		public static explicit operator Vector4UL(Byte3 value)
		{
			return new Vector4UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z, 1UL);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector2F"/>.</summary>
		public static explicit operator Vector2F(Byte3 value)
		{
			return new Vector2F((float)value.X, (float)value.Y);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector3F"/>.</summary>
		public static explicit operator Vector3F(Byte3 value)
		{
			return new Vector3F((float)value.X, (float)value.Y, (float)value.Z);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector4F"/>.</summary>
		public static explicit operator Vector4F(Byte3 value)
		{
			return new Vector4F((float)value.X, (float)value.Y, (float)value.Z, 1F);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector2D"/>.</summary>
		public static explicit operator Vector2D(Byte3 value)
		{
			return new Vector2D((double)value.X, (double)value.Y);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector3D"/>.</summary>
		public static explicit operator Vector3D(Byte3 value)
		{
			return new Vector3D((double)value.X, (double)value.Y, (double)value.Z);
		}

		///<summary>Casts a <see cref="Byte3"/> to a <see cref="Vector4D"/>.</summary>
		public static explicit operator Vector4D(Byte3 value)
		{
			return new Vector4D((double)value.X, (double)value.Y, (double)value.Z, 1D);
		}

#endregion
	}
}

