using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten
{
	///<summary>A <see cref="byte"/> vector comprised of four components.</summary>
	[StructLayout(LayoutKind.Explicit)]
    [Serializable]
	public partial struct Byte4 : IFormattable, IUnsignedVector<Byte4, byte>, IEquatable<Byte4>
	{
		///<summary>The size of <see cref="Byte4"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Byte4));

        ///<summary>The number of elements in the current vector type.</summary>
        public static readonly int NumElements = 4;

		///<summary>A Byte4 with every component set to (byte)1.</summary>
		public static readonly Byte4 One = new Byte4((byte)1, (byte)1, (byte)1, (byte)1);

        static readonly string toStringFormat = "X:{0} Y:{1} Z:{2} W:{3}";

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

		/// <summary>The W component.</summary>
		[DataMember]
		[FieldOffset(3)]
		public byte W;

		/// <summary>A fixed array mapped to the same memory space as the individual vector components.</summary>
		[FieldOffset(0)]
		public unsafe fixed byte Values[4];


        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == (byte)0 && Y == (byte)0 && Z == (byte)0 && W == (byte)0;
        }

#region Constructors
		/// <summary>Initializes a new instance of <see cref="Byte4"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Byte4(byte value)
		{
			X = value;
			Y = value;
			Z = value;
			W = value;
		}
		/// <summary>Initializes a new instance of <see cref="Byte4"/> from an array.</summary>
		/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least four elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Byte4(byte[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least four input values for Byte4.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
		}
		/// <summary>Initializes a new instance of <see cref="Byte4"/> from a span.</summary>
		/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least four elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Byte4(Span<byte> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least four input values for Byte4.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
		}
		/// <summary>Initializes a new instance of <see cref="Byte4"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the X, Y, Z, W components of the color.
		/// <para>There must be at least four elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than four elements.</exception>
		public unsafe Byte4(byte* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			X = ptrValues[0];
			Y = ptrValues[1];
			Z = ptrValues[2];
			W = ptrValues[3];
		}
		/// <summary>
		/// Initializes a new instance of <see cref="Byte4"/>.
		/// </summary>
		/// <param name="x">The X component.</param>
		/// <param name="y">The Y component.</param>
		/// <param name="z">The Z component.</param>
		/// <param name="w">The W component.</param>
		public Byte4(byte x, byte y, byte z, byte w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		///<summary>Creates a new instance of <see cref="Byte4"/>, using a <see cref="Byte2"/> to populate the first two components.</summary>
		public Byte4(Byte2 vector, byte z, byte w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
			W = w;
		}

		///<summary>Creates a new instance of <see cref="Byte4"/>, using a <see cref="Byte3"/> to populate the first three components.</summary>
		public Byte4(Byte3 vector, byte w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
			W = w;
		}

#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref = "Byte4"/> is equal to this instance.
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
            if (value is Byte4 v)
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
            return new byte[] { X, Y, Z, W };
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
        /// Returns a <see cref="string"/> that represents this <see cref="Byte4"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Byte4"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, format, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Byte4"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Byte4"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, toStringFormat, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Byte4"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Byte4"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, toStringFormat, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Byte4"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Byte4"/>.
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
		///<summary>Performs a add operation on two <see cref="Byte4"/>.</summary>
		///<param name="a">The first <see cref="Byte4"/> to add.</param>
		///<param name="b">The second <see cref="Byte4"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Byte4 a, ref Byte4 b, out Byte4 result)
		{
			result.X = (byte)(a.X + b.X);
			result.Y = (byte)(a.Y + b.Y);
			result.Z = (byte)(a.Z + b.Z);
			result.W = (byte)(a.W + b.W);
		}

		///<summary>Performs a add operation on two <see cref="Byte4"/>.</summary>
		///<param name="a">The first <see cref="Byte4"/> to add.</param>
		///<param name="b">The second <see cref="Byte4"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator +(Byte4 a, Byte4 b)
		{
			Add(ref a, ref b, out Byte4 result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="Byte4"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte4"/> to add.</param>
		///<param name="b">The <see cref="byte"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Byte4 a, byte b, out Byte4 result)
		{
			result.X = (byte)(a.X + b);
			result.Y = (byte)(a.Y + b);
			result.Z = (byte)(a.Z + b);
			result.W = (byte)(a.W + b);
		}

		///<summary>Performs a add operation on a <see cref="Byte4"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte4"/> to add.</param>
		///<param name="b">The <see cref="byte"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator +(Byte4 a, byte b)
		{
			Add(ref a, b, out Byte4 result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="byte"/> and a <see cref="Byte4"/>.</summary>
		///<param name="a">The <see cref="byte"/> to add.</param>
		///<param name="b">The <see cref="Byte4"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator +(byte a, Byte4 b)
		{
			Add(ref b, a, out Byte4 result);
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
		///<summary>Performs a subtract operation on two <see cref="Byte4"/>.</summary>
		///<param name="a">The first <see cref="Byte4"/> to subtract.</param>
		///<param name="b">The second <see cref="Byte4"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Byte4 a, ref Byte4 b, out Byte4 result)
		{
			result.X = (byte)(a.X - b.X);
			result.Y = (byte)(a.Y - b.Y);
			result.Z = (byte)(a.Z - b.Z);
			result.W = (byte)(a.W - b.W);
		}

		///<summary>Performs a subtract operation on two <see cref="Byte4"/>.</summary>
		///<param name="a">The first <see cref="Byte4"/> to subtract.</param>
		///<param name="b">The second <see cref="Byte4"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator -(Byte4 a, Byte4 b)
		{
			Subtract(ref a, ref b, out Byte4 result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="Byte4"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte4"/> to subtract.</param>
		///<param name="b">The <see cref="byte"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Byte4 a, byte b, out Byte4 result)
		{
			result.X = (byte)(a.X - b);
			result.Y = (byte)(a.Y - b);
			result.Z = (byte)(a.Z - b);
			result.W = (byte)(a.W - b);
		}

		///<summary>Performs a subtract operation on a <see cref="Byte4"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte4"/> to subtract.</param>
		///<param name="b">The <see cref="byte"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator -(Byte4 a, byte b)
		{
			Subtract(ref a, b, out Byte4 result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="byte"/> and a <see cref="Byte4"/>.</summary>
		///<param name="a">The <see cref="byte"/> to subtract.</param>
		///<param name="b">The <see cref="Byte4"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator -(byte a, Byte4 b)
		{
			Subtract(ref b, a, out Byte4 result);
			return result;
		}


#endregion

#region division operators
		///<summary>Performs a divide operation on two <see cref="Byte4"/>.</summary>
		///<param name="a">The first <see cref="Byte4"/> to divide.</param>
		///<param name="b">The second <see cref="Byte4"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Byte4 a, ref Byte4 b, out Byte4 result)
		{
			result.X = (byte)(a.X / b.X);
			result.Y = (byte)(a.Y / b.Y);
			result.Z = (byte)(a.Z / b.Z);
			result.W = (byte)(a.W / b.W);
		}

		///<summary>Performs a divide operation on two <see cref="Byte4"/>.</summary>
		///<param name="a">The first <see cref="Byte4"/> to divide.</param>
		///<param name="b">The second <see cref="Byte4"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator /(Byte4 a, Byte4 b)
		{
			Divide(ref a, ref b, out Byte4 result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="Byte4"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte4"/> to divide.</param>
		///<param name="b">The <see cref="byte"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Byte4 a, byte b, out Byte4 result)
		{
			result.X = (byte)(a.X / b);
			result.Y = (byte)(a.Y / b);
			result.Z = (byte)(a.Z / b);
			result.W = (byte)(a.W / b);
		}

		///<summary>Performs a divide operation on a <see cref="Byte4"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte4"/> to divide.</param>
		///<param name="b">The <see cref="byte"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator /(Byte4 a, byte b)
		{
			Divide(ref a, b, out Byte4 result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="byte"/> and a <see cref="Byte4"/>.</summary>
		///<param name="a">The <see cref="byte"/> to divide.</param>
		///<param name="b">The <see cref="Byte4"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator /(byte a, Byte4 b)
		{
			Divide(ref b, a, out Byte4 result);
			return result;
		}

#endregion

#region Multiply operators
		///<summary>Performs a multiply operation on two <see cref="Byte4"/>.</summary>
		///<param name="a">The first <see cref="Byte4"/> to multiply.</param>
		///<param name="b">The second <see cref="Byte4"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Byte4 a, ref Byte4 b, out Byte4 result)
		{
			result.X = (byte)(a.X * b.X);
			result.Y = (byte)(a.Y * b.Y);
			result.Z = (byte)(a.Z * b.Z);
			result.W = (byte)(a.W * b.W);
		}

		///<summary>Performs a multiply operation on two <see cref="Byte4"/>.</summary>
		///<param name="a">The first <see cref="Byte4"/> to multiply.</param>
		///<param name="b">The second <see cref="Byte4"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator *(Byte4 a, Byte4 b)
		{
			Multiply(ref a, ref b, out Byte4 result);
			return result;
		}

		///<summary>Performs a multiply operation on a <see cref="Byte4"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte4"/> to multiply.</param>
		///<param name="b">The <see cref="byte"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Byte4 a, byte b, out Byte4 result)
		{
			result.X = (byte)(a.X * b);
			result.Y = (byte)(a.Y * b);
			result.Z = (byte)(a.Z * b);
			result.W = (byte)(a.W * b);
		}

		///<summary>Performs a multiply operation on a <see cref="Byte4"/> and a <see cref="byte"/>.</summary>
		///<param name="a">The <see cref="Byte4"/> to multiply.</param>
		///<param name="b">The <see cref="byte"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator *(Byte4 a, byte b)
		{
			Multiply(ref a, b, out Byte4 result);
			return result;
		}

		///<summary>Performs a multiply operation on a <see cref="byte"/> and a <see cref="Byte4"/>.</summary>
		///<param name="a">The <see cref="byte"/> to multiply.</param>
		///<param name="b">The <see cref="Byte4"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Byte4 operator *(byte a, Byte4 b)
		{
			Multiply(ref b, a, out Byte4 result);
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
        public static Byte4 Reflect(Byte4 vector, Byte4 normal)
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
        /// <value>The value of a component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on. This must be between 0 and 3</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="index"/> is outside the range [0, 3].</exception>  
		public unsafe byte this[int index]
		{
			get
            {
                if(index > 3 || index < 0)
                    throw new IndexOutOfRangeException("Index for Byte4 must be between from 0 to 3, inclusive.");

                return Values[index];
            }
            set
            {
                if (index > 3 || index < 0)
                    throw new IndexOutOfRangeException("Index for Byte4 must be between from 0 to 3, inclusive.");

                Values[index] = value;
            }
		}

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of a component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on. This must be between 0 and 3</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="index"/> is greater than 3.</exception>  
		public unsafe byte this[uint index]
		{
			get
            {
                if(index > 3)
                    throw new IndexOutOfRangeException("Index for Byte4 must be between from 0 to 3, inclusive.");

                return Values[index];
            }
            set
            {
                if (index > 3)
                    throw new IndexOutOfRangeException("Index for Byte4 must be between from 0 to 3, inclusive.");

                Values[index] = value;
            }
		}
#endregion

#region Casts - vectors
		///<summary>Casts a <see cref="Byte4"/> to a <see cref="SByte2"/>.</summary>
		public static explicit operator SByte2(Byte4 value)
		{
			return new SByte2((sbyte)value.X, (sbyte)value.Y);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="SByte3"/>.</summary>
		public static explicit operator SByte3(Byte4 value)
		{
			return new SByte3((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="SByte4"/>.</summary>
		public static explicit operator SByte4(Byte4 value)
		{
			return new SByte4((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z, (sbyte)value.W);
		}

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

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector2I"/>.</summary>
		public static explicit operator Vector2I(Byte4 value)
		{
			return new Vector2I((int)value.X, (int)value.Y);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector3I"/>.</summary>
		public static explicit operator Vector3I(Byte4 value)
		{
			return new Vector3I((int)value.X, (int)value.Y, (int)value.Z);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4I"/>.</summary>
		public static explicit operator Vector4I(Byte4 value)
		{
			return new Vector4I((int)value.X, (int)value.Y, (int)value.Z, (int)value.W);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector2UI"/>.</summary>
		public static explicit operator Vector2UI(Byte4 value)
		{
			return new Vector2UI((uint)value.X, (uint)value.Y);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector3UI"/>.</summary>
		public static explicit operator Vector3UI(Byte4 value)
		{
			return new Vector3UI((uint)value.X, (uint)value.Y, (uint)value.Z);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4UI"/>.</summary>
		public static explicit operator Vector4UI(Byte4 value)
		{
			return new Vector4UI((uint)value.X, (uint)value.Y, (uint)value.Z, (uint)value.W);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector2S"/>.</summary>
		public static explicit operator Vector2S(Byte4 value)
		{
			return new Vector2S((short)value.X, (short)value.Y);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector3S"/>.</summary>
		public static explicit operator Vector3S(Byte4 value)
		{
			return new Vector3S((short)value.X, (short)value.Y, (short)value.Z);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4S"/>.</summary>
		public static explicit operator Vector4S(Byte4 value)
		{
			return new Vector4S((short)value.X, (short)value.Y, (short)value.Z, (short)value.W);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector2US"/>.</summary>
		public static explicit operator Vector2US(Byte4 value)
		{
			return new Vector2US((ushort)value.X, (ushort)value.Y);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector3US"/>.</summary>
		public static explicit operator Vector3US(Byte4 value)
		{
			return new Vector3US((ushort)value.X, (ushort)value.Y, (ushort)value.Z);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4US"/>.</summary>
		public static explicit operator Vector4US(Byte4 value)
		{
			return new Vector4US((ushort)value.X, (ushort)value.Y, (ushort)value.Z, (ushort)value.W);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector2L"/>.</summary>
		public static explicit operator Vector2L(Byte4 value)
		{
			return new Vector2L((long)value.X, (long)value.Y);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector3L"/>.</summary>
		public static explicit operator Vector3L(Byte4 value)
		{
			return new Vector3L((long)value.X, (long)value.Y, (long)value.Z);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4L"/>.</summary>
		public static explicit operator Vector4L(Byte4 value)
		{
			return new Vector4L((long)value.X, (long)value.Y, (long)value.Z, (long)value.W);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector2UL"/>.</summary>
		public static explicit operator Vector2UL(Byte4 value)
		{
			return new Vector2UL((ulong)value.X, (ulong)value.Y);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector3UL"/>.</summary>
		public static explicit operator Vector3UL(Byte4 value)
		{
			return new Vector3UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4UL"/>.</summary>
		public static explicit operator Vector4UL(Byte4 value)
		{
			return new Vector4UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z, (ulong)value.W);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector2F"/>.</summary>
		public static explicit operator Vector2F(Byte4 value)
		{
			return new Vector2F((float)value.X, (float)value.Y);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector3F"/>.</summary>
		public static explicit operator Vector3F(Byte4 value)
		{
			return new Vector3F((float)value.X, (float)value.Y, (float)value.Z);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4F"/>.</summary>
		public static explicit operator Vector4F(Byte4 value)
		{
			return new Vector4F((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector2D"/>.</summary>
		public static explicit operator Vector2D(Byte4 value)
		{
			return new Vector2D((double)value.X, (double)value.Y);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector3D"/>.</summary>
		public static explicit operator Vector3D(Byte4 value)
		{
			return new Vector3D((double)value.X, (double)value.Y, (double)value.Z);
		}

		///<summary>Casts a <see cref="Byte4"/> to a <see cref="Vector4D"/>.</summary>
		public static explicit operator Vector4D(Byte4 value)
		{
			return new Vector4D((double)value.X, (double)value.Y, (double)value.Z, (double)value.W);
		}

#endregion
	}
}

