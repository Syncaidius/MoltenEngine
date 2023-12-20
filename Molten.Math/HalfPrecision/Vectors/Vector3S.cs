using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten.HalfPrecision
{
	///<summary>A <see cref="short"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Explicit)]
    [DataContract]
	public partial struct Vector3S : IFormattable, ISignedVector<Vector3S, short>, IEquatable<Vector3S>
	{
        ///<summary>The number of components in the current vector type.</summary>
        public static readonly int ComponentCount = 3;

		///<summary>A Vector3S with every component set to (short)1.</summary>
		public static readonly Vector3S One = new Vector3S((short)1, (short)1, (short)1);

        static readonly string toStringFormat = "X:{0} Y:{1} Z:{2}";

		/// <summary>The X unit <see cref="Vector3S"/>.</summary>
		public static readonly Vector3S UnitX = new Vector3S((short)1, (short)0, (short)0);

		/// <summary>The Y unit <see cref="Vector3S"/>.</summary>
		public static readonly Vector3S UnitY = new Vector3S((short)0, (short)1, (short)0);

		/// <summary>The Z unit <see cref="Vector3S"/>.</summary>
		public static readonly Vector3S UnitZ = new Vector3S((short)0, (short)0, (short)1);

		/// <summary>Represents a zero'd Vector3S.</summary>
		public static readonly Vector3S Zero = new Vector3S((short)0, (short)0, (short)0);

		/// <summary>The X component.</summary>
		[DataMember]
		[FieldOffset(0)]
		public short X;

		/// <summary>The Y component.</summary>
		[DataMember]
		[FieldOffset(2)]
		public short Y;

		/// <summary>The Z component.</summary>
		[DataMember]
		[FieldOffset(4)]
		public short Z;

		/// <summary>A fixed array mapped to the same memory space as the individual <see cref="Vector3S"/> components.</summary>
		[IgnoreDataMember]
		[FieldOffset(0)]
		public unsafe fixed short Values[3];

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == (short)0 && Y == (short)0 && Z == (short)0;
        }

#region Constructors
		/// <summary>
		/// Initializes a new instance of <see cref="Vector3S"/>.
		/// </summary>
		/// <param name="x">The X component.</param>
		/// <param name="y">The Y component.</param>
		/// <param name="z">The Z component.</param>
		public Vector3S(short x, short y, short z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		/// <summary>Initializes a new instance of <see cref="Vector3S"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Vector3S(short value)
		{
			X = value;
			Y = value;
			Z = value;
		}
		/// <summary>Initializes a new instance of <see cref="Vector3S"/> from an array.</summary>
		/// <param name="values">The values to assign to the X, Y, Z components of the color. This must be an array with at least three elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 3 elements.</exception>
		public unsafe Vector3S(short[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 3)
				throw new ArgumentOutOfRangeException("values", "There must be at least 3 input values for Vector3S.");

			fixed (short* src = values)
			{
				fixed (short* dst = Values)
					Unsafe.CopyBlock(src, dst, (sizeof(short) * 3));
			}
		}
		/// <summary>Initializes a new instance of <see cref="Vector3S"/> from a span.</summary>
		/// <param name="values">The values to assign to the X, Y, Z components of the color. This must be an array with at least three elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 3 elements.</exception>
		public Vector3S(Span<short> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 3)
				throw new ArgumentOutOfRangeException("values", "There must be at least three input values for Vector3S.");

			X = values[0];
			Y = values[1];
			Z = values[2];
		}
		/// <summary>Initializes a new instance of <see cref="Vector3S"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the X, Y, Z components of the color.
		/// <para>There must be at least three elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than 3 elements.</exception>
		public unsafe Vector3S(short* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			X = ptrValues[0];
			Y = ptrValues[1];
			Z = ptrValues[2];
		}

		///<summary>Creates a new instance of <see cref="Vector3S"/>, using a <see cref="Vector2S"/> to populate the first two components.</summary>
		public Vector3S(Vector2S vector, short z)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
		}

#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref = "Vector3S"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector3S"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3S"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector3S other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector3S"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector3S"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3S"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3S other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector3S"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector3S"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3S"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is Vector3S v)
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
        public short LengthSquared()
        {
            return (short)((X * X) + (Y * Y) + (Z * Z));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector3S"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public short[] ToArray()
        {
            return [X, Y, Z];
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector3S"/>.
        /// </summary>
        /// <returns>A <see cref="Vector3S"/> facing the opposite direction.</returns>
		public Vector3S Negate()
		{
			return new Vector3S((short)-X, (short)-Y, (short)-Z);
		}
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(short min, short max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector3S min, Vector3S max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref Vector3S min, ref Vector3S max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
        }
#endregion

#region To-String
		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector3S"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector3S"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, format, X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector3S"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector3S"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, toStringFormat, X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector3S"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector3S"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, toStringFormat, X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector3S"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector3S"/>.
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
		///<summary>Performs a add operation on two <see cref="Vector3S"/>.</summary>
		///<param name="a">The first <see cref="Vector3S"/> to add.</param>
		///<param name="b">The second <see cref="Vector3S"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Vector3S a, ref Vector3S b, out Vector3S result)
		{
			result.X = (short)(a.X + b.X);
			result.Y = (short)(a.Y + b.Y);
			result.Z = (short)(a.Z + b.Z);
		}

		///<summary>Performs a add operation on two <see cref="Vector3S"/>.</summary>
		///<param name="a">The first <see cref="Vector3S"/> to add.</param>
		///<param name="b">The second <see cref="Vector3S"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator +(Vector3S a, Vector3S b)
		{
			Add(ref a, ref b, out Vector3S result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="Vector3S"/> and a <see cref="short"/>.</summary>
		///<param name="a">The <see cref="Vector3S"/> to add.</param>
		///<param name="b">The <see cref="short"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Vector3S a, short b, out Vector3S result)
		{
			result.X = (short)(a.X + b);
			result.Y = (short)(a.Y + b);
			result.Z = (short)(a.Z + b);
		}

		///<summary>Performs a add operation on a <see cref="Vector3S"/> and a <see cref="short"/>.</summary>
		///<param name="a">The <see cref="Vector3S"/> to add.</param>
		///<param name="b">The <see cref="short"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator +(Vector3S a, short b)
		{
			Add(ref a, b, out Vector3S result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="short"/> and a <see cref="Vector3S"/>.</summary>
		///<param name="a">The <see cref="short"/> to add.</param>
		///<param name="b">The <see cref="Vector3S"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator +(short a, Vector3S b)
		{
			Add(ref b, a, out Vector3S result);
			return result;
		}


		/// <summary>
        /// Assert a <see cref="Vector3S"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector3S"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector3S"/>.</returns>
        public static Vector3S operator +(Vector3S value)
        {
            return value;
        }
#endregion

#region Subtract operators
		///<summary>Performs a subtract operation on two <see cref="Vector3S"/>.</summary>
		///<param name="a">The first <see cref="Vector3S"/> to subtract.</param>
		///<param name="b">The second <see cref="Vector3S"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Vector3S a, ref Vector3S b, out Vector3S result)
		{
			result.X = (short)(a.X - b.X);
			result.Y = (short)(a.Y - b.Y);
			result.Z = (short)(a.Z - b.Z);
		}

		///<summary>Performs a subtract operation on two <see cref="Vector3S"/>.</summary>
		///<param name="a">The first <see cref="Vector3S"/> to subtract.</param>
		///<param name="b">The second <see cref="Vector3S"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator -(Vector3S a, Vector3S b)
		{
			Subtract(ref a, ref b, out Vector3S result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="Vector3S"/> and a <see cref="short"/>.</summary>
		///<param name="a">The <see cref="Vector3S"/> to subtract.</param>
		///<param name="b">The <see cref="short"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Vector3S a, short b, out Vector3S result)
		{
			result.X = (short)(a.X - b);
			result.Y = (short)(a.Y - b);
			result.Z = (short)(a.Z - b);
		}

		///<summary>Performs a subtract operation on a <see cref="Vector3S"/> and a <see cref="short"/>.</summary>
		///<param name="a">The <see cref="Vector3S"/> to subtract.</param>
		///<param name="b">The <see cref="short"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator -(Vector3S a, short b)
		{
			Subtract(ref a, b, out Vector3S result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="short"/> and a <see cref="Vector3S"/>.</summary>
		///<param name="a">The <see cref="short"/> to subtract.</param>
		///<param name="b">The <see cref="Vector3S"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator -(short a, Vector3S b)
		{
			Subtract(ref b, a, out Vector3S result);
			return result;
		}


        /// <summary>
        /// Negate/reverse the direction of a <see cref="Vector3S"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3S"/> to reverse.</param>
        /// <param name="result">The output for the reversed <see cref="Vector3S"/>.</param>
        public static void Negate(ref Vector3S value, out Vector3S result)
        {
			result.X = (short)-value.X;
			result.Y = (short)-value.Y;
			result.Z = (short)-value.Z;
            
        }

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector3S"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3S"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector3S"/>.</returns>
        public static Vector3S operator -(Vector3S value)
        {
            Negate(ref value, out value);
            return value;
        }
#endregion

#region division operators
		///<summary>Performs a divide operation on two <see cref="Vector3S"/>.</summary>
		///<param name="a">The first <see cref="Vector3S"/> to divide.</param>
		///<param name="b">The second <see cref="Vector3S"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Vector3S a, ref Vector3S b, out Vector3S result)
		{
			result.X = (short)(a.X / b.X);
			result.Y = (short)(a.Y / b.Y);
			result.Z = (short)(a.Z / b.Z);
		}

		///<summary>Performs a divide operation on two <see cref="Vector3S"/>.</summary>
		///<param name="a">The first <see cref="Vector3S"/> to divide.</param>
		///<param name="b">The second <see cref="Vector3S"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator /(Vector3S a, Vector3S b)
		{
			Divide(ref a, ref b, out Vector3S result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="Vector3S"/> and a <see cref="short"/>.</summary>
		///<param name="a">The <see cref="Vector3S"/> to divide.</param>
		///<param name="b">The <see cref="short"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Vector3S a, short b, out Vector3S result)
		{
			result.X = (short)(a.X / b);
			result.Y = (short)(a.Y / b);
			result.Z = (short)(a.Z / b);
		}

		///<summary>Performs a divide operation on a <see cref="Vector3S"/> and a <see cref="short"/>.</summary>
		///<param name="a">The <see cref="Vector3S"/> to divide.</param>
		///<param name="b">The <see cref="short"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator /(Vector3S a, short b)
		{
			Divide(ref a, b, out Vector3S result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="short"/> and a <see cref="Vector3S"/>.</summary>
		///<param name="a">The <see cref="short"/> to divide.</param>
		///<param name="b">The <see cref="Vector3S"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator /(short a, Vector3S b)
		{
			Divide(ref b, a, out Vector3S result);
			return result;
		}

#endregion

#region Multiply operators
		///<summary>Performs a multiply operation on two <see cref="Vector3S"/>.</summary>
		///<param name="a">The first <see cref="Vector3S"/> to multiply.</param>
		///<param name="b">The second <see cref="Vector3S"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Vector3S a, ref Vector3S b, out Vector3S result)
		{
			result.X = (short)(a.X * b.X);
			result.Y = (short)(a.Y * b.Y);
			result.Z = (short)(a.Z * b.Z);
		}

		///<summary>Performs a multiply operation on two <see cref="Vector3S"/>.</summary>
		///<param name="a">The first <see cref="Vector3S"/> to multiply.</param>
		///<param name="b">The second <see cref="Vector3S"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator *(Vector3S a, Vector3S b)
		{
			Multiply(ref a, ref b, out Vector3S result);
			return result;
		}

		///<summary>Performs a multiply operation on a <see cref="Vector3S"/> and a <see cref="short"/>.</summary>
		///<param name="a">The <see cref="Vector3S"/> to multiply.</param>
		///<param name="b">The <see cref="short"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Vector3S a, short b, out Vector3S result)
		{
			result.X = (short)(a.X * b);
			result.Y = (short)(a.Y * b);
			result.Z = (short)(a.Z * b);
		}

		///<summary>Performs a multiply operation on a <see cref="Vector3S"/> and a <see cref="short"/>.</summary>
		///<param name="a">The <see cref="Vector3S"/> to multiply.</param>
		///<param name="b">The <see cref="short"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator *(Vector3S a, short b)
		{
			Multiply(ref a, b, out Vector3S result);
			return result;
		}

		///<summary>Performs a multiply operation on a <see cref="short"/> and a <see cref="Vector3S"/>.</summary>
		///<param name="a">The <see cref="short"/> to multiply.</param>
		///<param name="b">The <see cref="Vector3S"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S operator *(short a, Vector3S b)
		{
			Multiply(ref b, a, out Vector3S result);
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
        public static bool operator ==(Vector3S left, Vector3S right)
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
        public static bool operator !=(Vector3S left, Vector3S right)
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
        public static Vector3S SmoothStep(ref Vector3S start, ref Vector3S end, float amount)
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
        public static Vector3S SmoothStep(Vector3S start, Vector3S end, short amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Vector3S"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Vector3S"/>.</param>
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
        public static void Orthogonalize(Vector3S[] destination, params Vector3S[] source)
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
                Vector3S newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (short)(Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector3S"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector3S"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
		/// <param name="zIndex">The axis index to use for the new Z value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector3S Swizzle(Vector3S val, int xIndex, int yIndex, int zIndex)
        {
            return new Vector3S()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
			   Z = (&val.X)[zIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector3S Swizzle(Vector3S val, uint xIndex, uint yIndex, uint zIndex)
        {
            return new Vector3S()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
			    Z = (&val.X)[zIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector3S"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3S"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3S"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Dot(ref Vector3S left, ref Vector3S right)
        {
			return (short)(((short)left.X * right.X) + ((short)left.Y * right.Y) + ((short)left.Z * right.Z));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector3S"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3S"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3S"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Dot(Vector3S left, Vector3S right)
        {
			return (short)((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

		/// <summary>
        /// Returns a <see cref="Vector3S"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector3S"/> containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector3S"/> containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector3S"/> containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector3S Barycentric(ref Vector3S value1, ref Vector3S value2, ref Vector3S value3, short amount1, short amount2)
        {
			return new Vector3S(
				(short)((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				(short)((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))), 
				(short)((value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3S"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3S"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Vector3S start, ref Vector3S end, float amount, out Vector3S result)
        {
			result.X = (short)((1F - amount) * start.X + amount * end.X);
			result.Y = (short)((1F - amount) * start.Y + amount * end.Y);
			result.Z = (short)((1F - amount) * start.Z + amount * end.Z);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3S"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3S Lerp(Vector3S start, Vector3S end, float amount)
        {
			return new Vector3S()
			{
				X = (short)((1F - amount) * start.X + amount * end.X),
				Y = (short)((1F - amount) * start.Y + amount * end.Y),
				Z = (short)((1F - amount) * start.Z + amount * end.Z),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3S"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3S Lerp(ref Vector3S start, ref Vector3S end, float amount)
        {
			return new Vector3S()
			{
				X = (short)((1F - amount) * start.X + amount * end.X),
				Y = (short)((1F - amount) * start.Y + amount * end.Y),
				Z = (short)((1F - amount) * start.Z + amount * end.Z),
			};
        }

        /// <summary>
        /// Returns a <see cref="Vector3S"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3S"/>.</param>
        /// <param name="right">The second source <see cref="Vector3S"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3S"/>.</param>
        /// <returns>A <see cref="Vector3S"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref Vector3S left, ref Vector3S right, out Vector3S result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
				result.Z = (left.Z < right.Z) ? left.Z : right.Z;
		}

        /// <summary>
        /// Returns a <see cref="Vector3S"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3S"/>.</param>
        /// <param name="right">The second source <see cref="Vector3S"/>.</param>
        /// <returns>A <see cref="Vector3S"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S Min(ref Vector3S left, ref Vector3S right)
		{
			Min(ref left, ref right, out Vector3S result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector3S"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3S"/>.</param>
        /// <param name="right">The second source <see cref="Vector3S"/>.</param>
        /// <returns>A <see cref="Vector3S"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S Min(Vector3S left, Vector3S right)
		{
			return new Vector3S()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

        /// <summary>
        /// Returns a <see cref="Vector3S"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3S"/>.</param>
        /// <param name="right">The second source <see cref="Vector3S"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3S"/>.</param>
        /// <returns>A <see cref="Vector3S"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref Vector3S left, ref Vector3S right, out Vector3S result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
				result.Z = (left.Z > right.Z) ? left.Z : right.Z;
		}

        /// <summary>
        /// Returns a <see cref="Vector3S"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3S"/>.</param>
        /// <param name="right">The second source <see cref="Vector3S"/>.</param>
        /// <returns>A <see cref="Vector3S"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S Max(ref Vector3S left, ref Vector3S right)
		{
			Max(ref left, ref right, out Vector3S result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector3S"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3S"/>.</param>
        /// <param name="right">The second source <see cref="Vector3S"/>.</param>
        /// <returns>A <see cref="Vector3S"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3S Max(Vector3S left, Vector3S right)
		{
			return new Vector3S()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3S"/> vectors.
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
		public static short DistanceSquared(ref Vector3S value1, ref Vector3S value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;

            return (short)((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Vector3S"/> vectors.
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
		public static short DistanceSquared(Vector3S value1, Vector3S value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;

            return (short)((x * x) + (y * y) + (z * z));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3S"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3S Clamp(Vector3S value, short min, short max)
        {
			return new Vector3S()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3S"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3S"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref Vector3S value, ref Vector3S min, ref Vector3S max, out Vector3S result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
				result.Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3S"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3S Clamp(Vector3S value, Vector3S min, Vector3S max)
        {
			return new Vector3S()
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
        public static Vector3S Reflect(Vector3S vector, Vector3S normal)
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
        public static Vector3S Reflect(ref Vector3S vector, ref Vector3S normal)
        {
            int dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z);

            return new Vector3S()
            {
				X = (short)(vector.X - ((2 * dot) * normal.X)),
				Y = (short)(vector.Y - ((2 * dot) * normal.Y)),
				Z = (short)(vector.Z - ((2 * dot) * normal.Z)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (short x, short y, short z)(Vector3S val)
        {
            return (val.X, val.Y, val.Z);
        }

        public static implicit operator Vector3S((short x, short y, short z) val)
        {
            return new Vector3S(val.x, val.y, val.z);
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
		public unsafe short this[int index]
		{
			get
            {
                if(index > 2 || index < 0)
                    throw new IndexOutOfRangeException("Index for Vector3S must be between from 0 to 2, inclusive.");

                return Values[index];
            }
            set
            {
                if (index > 2 || index < 0)
                    throw new IndexOutOfRangeException("Index for Vector3S must be between from 0 to 2, inclusive.");

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
		public unsafe short this[uint index]
		{
			get
            {
                if(index > 2)
                    throw new IndexOutOfRangeException("Index for Vector3S must be between from 0 to 2, inclusive.");

                return Values[index];
            }
            set
            {
                if (index > 2)
                    throw new IndexOutOfRangeException("Index for Vector3S must be between from 0 to 2, inclusive.");

                Values[index] = value;
            }
		}
#endregion

#region Casts - vectors
		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="SByte2"/>.</summary>
		public static explicit operator SByte2(Vector3S value)
		{
			return new SByte2((sbyte)value.X, (sbyte)value.Y);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="SByte3"/>.</summary>
		public static explicit operator SByte3(Vector3S value)
		{
			return new SByte3((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="SByte4"/>.</summary>
		public static explicit operator SByte4(Vector3S value)
		{
			return new SByte4((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z, (sbyte)1);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Byte2"/>.</summary>
		public static explicit operator Byte2(Vector3S value)
		{
			return new Byte2((byte)value.X, (byte)value.Y);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Byte3"/>.</summary>
		public static explicit operator Byte3(Vector3S value)
		{
			return new Byte3((byte)value.X, (byte)value.Y, (byte)value.Z);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Byte4"/>.</summary>
		public static explicit operator Byte4(Vector3S value)
		{
			return new Byte4((byte)value.X, (byte)value.Y, (byte)value.Z, (byte)1);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector2I"/>.</summary>
		public static explicit operator Vector2I(Vector3S value)
		{
			return new Vector2I((int)value.X, (int)value.Y);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector3I"/>.</summary>
		public static explicit operator Vector3I(Vector3S value)
		{
			return new Vector3I((int)value.X, (int)value.Y, (int)value.Z);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector4I"/>.</summary>
		public static explicit operator Vector4I(Vector3S value)
		{
			return new Vector4I((int)value.X, (int)value.Y, (int)value.Z, 1);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector2UI"/>.</summary>
		public static explicit operator Vector2UI(Vector3S value)
		{
			return new Vector2UI((uint)value.X, (uint)value.Y);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector3UI"/>.</summary>
		public static explicit operator Vector3UI(Vector3S value)
		{
			return new Vector3UI((uint)value.X, (uint)value.Y, (uint)value.Z);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector4UI"/>.</summary>
		public static explicit operator Vector4UI(Vector3S value)
		{
			return new Vector4UI((uint)value.X, (uint)value.Y, (uint)value.Z, 1U);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector2S"/>.</summary>
		public static explicit operator Vector2S(Vector3S value)
		{
			return new Vector2S(value.X, value.Y);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector4S"/>.</summary>
		public static explicit operator Vector4S(Vector3S value)
		{
			return new Vector4S(value.X, value.Y, value.Z, (short)1);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector2US"/>.</summary>
		public static explicit operator Vector2US(Vector3S value)
		{
			return new Vector2US((ushort)value.X, (ushort)value.Y);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector3US"/>.</summary>
		public static explicit operator Vector3US(Vector3S value)
		{
			return new Vector3US((ushort)value.X, (ushort)value.Y, (ushort)value.Z);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector4US"/>.</summary>
		public static explicit operator Vector4US(Vector3S value)
		{
			return new Vector4US((ushort)value.X, (ushort)value.Y, (ushort)value.Z, (ushort)1);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector2L"/>.</summary>
		public static explicit operator Vector2L(Vector3S value)
		{
			return new Vector2L((long)value.X, (long)value.Y);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector3L"/>.</summary>
		public static explicit operator Vector3L(Vector3S value)
		{
			return new Vector3L((long)value.X, (long)value.Y, (long)value.Z);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector4L"/>.</summary>
		public static explicit operator Vector4L(Vector3S value)
		{
			return new Vector4L((long)value.X, (long)value.Y, (long)value.Z, 1L);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector2UL"/>.</summary>
		public static explicit operator Vector2UL(Vector3S value)
		{
			return new Vector2UL((ulong)value.X, (ulong)value.Y);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector3UL"/>.</summary>
		public static explicit operator Vector3UL(Vector3S value)
		{
			return new Vector3UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector4UL"/>.</summary>
		public static explicit operator Vector4UL(Vector3S value)
		{
			return new Vector4UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z, 1UL);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector2F"/>.</summary>
		public static explicit operator Vector2F(Vector3S value)
		{
			return new Vector2F((float)value.X, (float)value.Y);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector3F"/>.</summary>
		public static explicit operator Vector3F(Vector3S value)
		{
			return new Vector3F((float)value.X, (float)value.Y, (float)value.Z);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector4F"/>.</summary>
		public static explicit operator Vector4F(Vector3S value)
		{
			return new Vector4F((float)value.X, (float)value.Y, (float)value.Z, 1F);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector2D"/>.</summary>
		public static explicit operator Vector2D(Vector3S value)
		{
			return new Vector2D((double)value.X, (double)value.Y);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector3D"/>.</summary>
		public static explicit operator Vector3D(Vector3S value)
		{
			return new Vector3D((double)value.X, (double)value.Y, (double)value.Z);
		}

		///<summary>Casts a <see cref="Vector3S"/> to a <see cref="Vector4D"/>.</summary>
		public static explicit operator Vector4D(Vector3S value)
		{
			return new Vector4D((double)value.X, (double)value.Y, (double)value.Z, 1D);
		}

#endregion
	}
}

