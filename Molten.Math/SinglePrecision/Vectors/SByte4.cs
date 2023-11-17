using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten
{
	///<summary>A <see cref="sbyte"/> vector comprised of four components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
    [Serializable]
	public partial struct SByte4 : IFormattable, ISignedVector<SByte4, sbyte>, IEquatable<SByte4>
	{
		///<summary>The size of <see cref="SByte4"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(SByte4));

		///<summary>A SByte4 with every component set to (sbyte)1.</summary>
		public static readonly SByte4 One = new SByte4((sbyte)1, (sbyte)1, (sbyte)1, (sbyte)1);

        static readonly string toStringFormat = "X:{0} Y:{1} Z:{2} W:{3}";

		/// <summary>The X unit <see cref="SByte4"/>.</summary>
		public static readonly SByte4 UnitX = new SByte4((sbyte)1, (sbyte)0, (sbyte)0, (sbyte)0);

		/// <summary>The Y unit <see cref="SByte4"/>.</summary>
		public static readonly SByte4 UnitY = new SByte4((sbyte)0, (sbyte)1, (sbyte)0, (sbyte)0);

		/// <summary>The Z unit <see cref="SByte4"/>.</summary>
		public static readonly SByte4 UnitZ = new SByte4((sbyte)0, (sbyte)0, (sbyte)1, (sbyte)0);

		/// <summary>The W unit <see cref="SByte4"/>.</summary>
		public static readonly SByte4 UnitW = new SByte4((sbyte)0, (sbyte)0, (sbyte)0, (sbyte)1);

		/// <summary>Represents a zero'd SByte4.</summary>
		public static readonly SByte4 Zero = new SByte4((sbyte)0, (sbyte)0, (sbyte)0, (sbyte)0);

		/// <summary>The X component.</summary>
		[DataMember]
		public sbyte X;

		/// <summary>The Y component.</summary>
		[DataMember]
		public sbyte Y;

		/// <summary>The Z component.</summary>
		[DataMember]
		public sbyte Z;

		/// <summary>The W component.</summary>
		[DataMember]
		public sbyte W;


        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == (sbyte)0 && Y == (sbyte)0 && Z == (sbyte)0 && W == (sbyte)0;
        }

#region Constructors
		/// <summary>Initializes a new instance of <see cref="SByte4"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public SByte4(sbyte value)
		{
			X = value;
			Y = value;
			Z = value;
			W = value;
		}
		/// <summary>Initializes a new instance of <see cref="SByte4"/> from an array.</summary>
		/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least four elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public SByte4(sbyte[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least four input values for SByte4.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
		}
		/// <summary>Initializes a new instance of <see cref="SByte4"/> from a span.</summary>
		/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least four elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public SByte4(Span<sbyte> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least four input values for SByte4.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
		}
		/// <summary>Initializes a new instance of <see cref="SByte4"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the X, Y, Z, W components of the color.
		/// <para>There must be at least four elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than four elements.</exception>
		public unsafe SByte4(sbyte* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			X = ptrValues[0];
			Y = ptrValues[1];
			Z = ptrValues[2];
			W = ptrValues[3];
		}
		/// <summary>
		/// Initializes a new instance of <see cref="SByte4"/>.
		/// </summary>
		/// <param name="x">The X component.</param>
		/// <param name="y">The Y component.</param>
		/// <param name="z">The Z component.</param>
		/// <param name="w">The W component.</param>
		public SByte4(sbyte x, sbyte y, sbyte z, sbyte w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		///<summary>Creates a new instance of <see cref="SByte4"/>, using a <see cref="SByte2"/> to populate the first two components.</summary>
		public SByte4(SByte2 vector, sbyte z, sbyte w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
			W = w;
		}

		///<summary>Creates a new instance of <see cref="SByte4"/>, using a <see cref="SByte3"/> to populate the first three components.</summary>
		public SByte4(SByte3 vector, sbyte w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
			W = w;
		}

#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref = "SByte4"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="SByte4"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="SByte4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref SByte4 other)
        {
            return other.X == X && other.Y == Y && other.Z == Z && other.W == W;
        }

        /// <summary>
        /// Determines whether the specified <see cref="SByte4"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="SByte4"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="SByte4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SByte4 other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="SByte4"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="SByte4"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="SByte4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is SByte4 v)
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
        public sbyte LengthSquared()
        {
            return (sbyte)((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="SByte4"/>.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public sbyte[] ToArray()
        {
            return new sbyte[] { X, Y, Z, W };
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="SByte4"/>.
        /// </summary>
        /// <returns>A <see cref="SByte4"/> facing the opposite direction.</returns>
		public SByte4 Negate()
		{
			return new SByte4((sbyte)-X, (sbyte)-Y, (sbyte)-Z, (sbyte)-W);
		}
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(sbyte min, sbyte max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
			W = W < min ? min : W > max ? max : W;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(SByte4 min, SByte4 max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref SByte4 min, ref SByte4 max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
			W = W < min.W ? min.W : W > max.W ? max.W : W;
        }
#endregion

#region To-String
		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="SByte4"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="SByte4"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, format, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="SByte4"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="SByte4"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, toStringFormat, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="SByte4"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="SByte4"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, toStringFormat, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="SByte4"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="SByte4"/>.
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
		///<summary>Performs a add operation on two <see cref="SByte4"/>.</summary>
		///<param name="a">The first <see cref="SByte4"/> to add.</param>
		///<param name="b">The second <see cref="SByte4"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref SByte4 a, ref SByte4 b, out SByte4 result)
		{
			result.X = (sbyte)(a.X + b.X);
			result.Y = (sbyte)(a.Y + b.Y);
			result.Z = (sbyte)(a.Z + b.Z);
			result.W = (sbyte)(a.W + b.W);
		}

		///<summary>Performs a add operation on two <see cref="SByte4"/>.</summary>
		///<param name="a">The first <see cref="SByte4"/> to add.</param>
		///<param name="b">The second <see cref="SByte4"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator +(SByte4 a, SByte4 b)
		{
			Add(ref a, ref b, out SByte4 result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="SByte4"/> and a <see cref="sbyte"/>.</summary>
		///<param name="a">The <see cref="SByte4"/> to add.</param>
		///<param name="b">The <see cref="sbyte"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref SByte4 a, sbyte b, out SByte4 result)
		{
			result.X = (sbyte)(a.X + b);
			result.Y = (sbyte)(a.Y + b);
			result.Z = (sbyte)(a.Z + b);
			result.W = (sbyte)(a.W + b);
		}

		///<summary>Performs a add operation on a <see cref="SByte4"/> and a <see cref="sbyte"/>.</summary>
		///<param name="a">The <see cref="SByte4"/> to add.</param>
		///<param name="b">The <see cref="sbyte"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator +(SByte4 a, sbyte b)
		{
			Add(ref a, b, out SByte4 result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="sbyte"/> and a <see cref="SByte4"/>.</summary>
		///<param name="a">The <see cref="sbyte"/> to add.</param>
		///<param name="b">The <see cref="SByte4"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator +(sbyte a, SByte4 b)
		{
			Add(ref b, a, out SByte4 result);
			return result;
		}


		/// <summary>
        /// Assert a <see cref="SByte4"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="SByte4"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="SByte4"/>.</returns>
        public static SByte4 operator +(SByte4 value)
        {
            return value;
        }
#endregion

#region Subtract operators
		///<summary>Performs a subtract operation on two <see cref="SByte4"/>.</summary>
		///<param name="a">The first <see cref="SByte4"/> to subtract.</param>
		///<param name="b">The second <see cref="SByte4"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref SByte4 a, ref SByte4 b, out SByte4 result)
		{
			result.X = (sbyte)(a.X - b.X);
			result.Y = (sbyte)(a.Y - b.Y);
			result.Z = (sbyte)(a.Z - b.Z);
			result.W = (sbyte)(a.W - b.W);
		}

		///<summary>Performs a subtract operation on two <see cref="SByte4"/>.</summary>
		///<param name="a">The first <see cref="SByte4"/> to subtract.</param>
		///<param name="b">The second <see cref="SByte4"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator -(SByte4 a, SByte4 b)
		{
			Subtract(ref a, ref b, out SByte4 result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="SByte4"/> and a <see cref="sbyte"/>.</summary>
		///<param name="a">The <see cref="SByte4"/> to subtract.</param>
		///<param name="b">The <see cref="sbyte"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref SByte4 a, sbyte b, out SByte4 result)
		{
			result.X = (sbyte)(a.X - b);
			result.Y = (sbyte)(a.Y - b);
			result.Z = (sbyte)(a.Z - b);
			result.W = (sbyte)(a.W - b);
		}

		///<summary>Performs a subtract operation on a <see cref="SByte4"/> and a <see cref="sbyte"/>.</summary>
		///<param name="a">The <see cref="SByte4"/> to subtract.</param>
		///<param name="b">The <see cref="sbyte"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator -(SByte4 a, sbyte b)
		{
			Subtract(ref a, b, out SByte4 result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="sbyte"/> and a <see cref="SByte4"/>.</summary>
		///<param name="a">The <see cref="sbyte"/> to subtract.</param>
		///<param name="b">The <see cref="SByte4"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator -(sbyte a, SByte4 b)
		{
			Subtract(ref b, a, out SByte4 result);
			return result;
		}


        /// <summary>
        /// Negate/reverse the direction of a <see cref="SByte4"/>.
        /// </summary>
        /// <param name="value">The <see cref="SByte4"/> to reverse.</param>
        /// <param name="result">The output for the reversed <see cref="SByte4"/>.</param>
        public static void Negate(ref SByte4 value, out SByte4 result)
        {
			result.X = (sbyte)-value.X;
			result.Y = (sbyte)-value.Y;
			result.Z = (sbyte)-value.Z;
			result.W = (sbyte)-value.W;
            
        }

		/// <summary>
        /// Negate/reverse the direction of a <see cref="SByte4"/>.
        /// </summary>
        /// <param name="value">The <see cref="SByte4"/> to reverse.</param>
        /// <returns>The reversed <see cref="SByte4"/>.</returns>
        public static SByte4 operator -(SByte4 value)
        {
            Negate(ref value, out value);
            return value;
        }
#endregion

#region division operators
		///<summary>Performs a divide operation on two <see cref="SByte4"/>.</summary>
		///<param name="a">The first <see cref="SByte4"/> to divide.</param>
		///<param name="b">The second <see cref="SByte4"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref SByte4 a, ref SByte4 b, out SByte4 result)
		{
			result.X = (sbyte)(a.X / b.X);
			result.Y = (sbyte)(a.Y / b.Y);
			result.Z = (sbyte)(a.Z / b.Z);
			result.W = (sbyte)(a.W / b.W);
		}

		///<summary>Performs a divide operation on two <see cref="SByte4"/>.</summary>
		///<param name="a">The first <see cref="SByte4"/> to divide.</param>
		///<param name="b">The second <see cref="SByte4"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator /(SByte4 a, SByte4 b)
		{
			Divide(ref a, ref b, out SByte4 result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="SByte4"/> and a <see cref="sbyte"/>.</summary>
		///<param name="a">The <see cref="SByte4"/> to divide.</param>
		///<param name="b">The <see cref="sbyte"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref SByte4 a, sbyte b, out SByte4 result)
		{
			result.X = (sbyte)(a.X / b);
			result.Y = (sbyte)(a.Y / b);
			result.Z = (sbyte)(a.Z / b);
			result.W = (sbyte)(a.W / b);
		}

		///<summary>Performs a divide operation on a <see cref="SByte4"/> and a <see cref="sbyte"/>.</summary>
		///<param name="a">The <see cref="SByte4"/> to divide.</param>
		///<param name="b">The <see cref="sbyte"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator /(SByte4 a, sbyte b)
		{
			Divide(ref a, b, out SByte4 result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="sbyte"/> and a <see cref="SByte4"/>.</summary>
		///<param name="a">The <see cref="sbyte"/> to divide.</param>
		///<param name="b">The <see cref="SByte4"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator /(sbyte a, SByte4 b)
		{
			Divide(ref b, a, out SByte4 result);
			return result;
		}

#endregion

#region Multiply operators
		///<summary>Performs a multiply operation on two <see cref="SByte4"/>.</summary>
		///<param name="a">The first <see cref="SByte4"/> to multiply.</param>
		///<param name="b">The second <see cref="SByte4"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref SByte4 a, ref SByte4 b, out SByte4 result)
		{
			result.X = (sbyte)(a.X * b.X);
			result.Y = (sbyte)(a.Y * b.Y);
			result.Z = (sbyte)(a.Z * b.Z);
			result.W = (sbyte)(a.W * b.W);
		}

		///<summary>Performs a multiply operation on two <see cref="SByte4"/>.</summary>
		///<param name="a">The first <see cref="SByte4"/> to multiply.</param>
		///<param name="b">The second <see cref="SByte4"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator *(SByte4 a, SByte4 b)
		{
			Multiply(ref a, ref b, out SByte4 result);
			return result;
		}

		///<summary>Performs a multiply operation on a <see cref="SByte4"/> and a <see cref="sbyte"/>.</summary>
		///<param name="a">The <see cref="SByte4"/> to multiply.</param>
		///<param name="b">The <see cref="sbyte"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref SByte4 a, sbyte b, out SByte4 result)
		{
			result.X = (sbyte)(a.X * b);
			result.Y = (sbyte)(a.Y * b);
			result.Z = (sbyte)(a.Z * b);
			result.W = (sbyte)(a.W * b);
		}

		///<summary>Performs a multiply operation on a <see cref="SByte4"/> and a <see cref="sbyte"/>.</summary>
		///<param name="a">The <see cref="SByte4"/> to multiply.</param>
		///<param name="b">The <see cref="sbyte"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator *(SByte4 a, sbyte b)
		{
			Multiply(ref a, b, out SByte4 result);
			return result;
		}

		///<summary>Performs a multiply operation on a <see cref="sbyte"/> and a <see cref="SByte4"/>.</summary>
		///<param name="a">The <see cref="sbyte"/> to multiply.</param>
		///<param name="b">The <see cref="SByte4"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 operator *(sbyte a, SByte4 b)
		{
			Multiply(ref b, a, out SByte4 result);
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
        public static bool operator ==(SByte4 left, SByte4 right)
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
        public static bool operator !=(SByte4 left, SByte4 right)
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
        public static SByte4 SmoothStep(ref SByte4 start, ref SByte4 end, float amount)
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
        public static SByte4 SmoothStep(SByte4 start, SByte4 end, sbyte amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="SByte4"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="SByte4"/>.</param>
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
        public static void Orthogonalize(SByte4[] destination, params SByte4[] source)
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
                SByte4 newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (sbyte)(Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="SByte4"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="SByte4"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
		/// <param name="zIndex">The axis index to use for the new Z value.</param>
		/// <param name="wIndex">The axis index to use for the new W value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe SByte4 Swizzle(SByte4 val, int xIndex, int yIndex, int zIndex, int wIndex)
        {
            return new SByte4()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
			   Z = (&val.X)[zIndex],
			   W = (&val.X)[wIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe SByte4 Swizzle(SByte4 val, uint xIndex, uint yIndex, uint zIndex, uint wIndex)
        {
            return new SByte4()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
			    Z = (&val.X)[zIndex],
			    W = (&val.X)[wIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="SByte4"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="SByte4"/> source vector</param>
        /// <param name="right">Second <see cref="SByte4"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Dot(ref SByte4 left, ref SByte4 right)
        {
			return (sbyte)(((sbyte)left.X * right.X) + ((sbyte)left.Y * right.Y) + ((sbyte)left.Z * right.Z) + ((sbyte)left.W * right.W));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="SByte4"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="SByte4"/> source vector</param>
        /// <param name="right">Second <see cref="SByte4"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Dot(SByte4 left, SByte4 right)
        {
			return (sbyte)((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W));
        }

		/// <summary>
        /// Returns a <see cref="SByte4"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="SByte4"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="SByte4"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="SByte4"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static SByte4 Barycentric(ref SByte4 value1, ref SByte4 value2, ref SByte4 value3, sbyte amount1, sbyte amount2)
        {
			return new SByte4(
				(sbyte)((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				(sbyte)((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))), 
				(sbyte)((value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z))), 
				(sbyte)((value1.W + (amount1 * (value2.W - value1.W))) + (amount2 * (value3.W - value1.W)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="SByte4"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="SByte4"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref SByte4 start, ref SByte4 end, float amount, out SByte4 result)
        {
			result.X = (sbyte)((1F - amount) * start.X + amount * end.X);
			result.Y = (sbyte)((1F - amount) * start.Y + amount * end.Y);
			result.Z = (sbyte)((1F - amount) * start.Z + amount * end.Z);
			result.W = (sbyte)((1F - amount) * start.W + amount * end.W);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="SByte4"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SByte4 Lerp(SByte4 start, SByte4 end, float amount)
        {
			return new SByte4()
			{
				X = (sbyte)((1F - amount) * start.X + amount * end.X),
				Y = (sbyte)((1F - amount) * start.Y + amount * end.Y),
				Z = (sbyte)((1F - amount) * start.Z + amount * end.Z),
				W = (sbyte)((1F - amount) * start.W + amount * end.W),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="SByte4"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SByte4 Lerp(ref SByte4 start, ref SByte4 end, float amount)
        {
			return new SByte4()
			{
				X = (sbyte)((1F - amount) * start.X + amount * end.X),
				Y = (sbyte)((1F - amount) * start.Y + amount * end.Y),
				Z = (sbyte)((1F - amount) * start.Z + amount * end.Z),
				W = (sbyte)((1F - amount) * start.W + amount * end.W),
			};
        }

        /// <summary>
        /// Returns a <see cref="SByte4"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="SByte4"/>.</param>
        /// <param name="right">The second source <see cref="SByte4"/>.</param>
        /// <param name="result">The output for the resultant <see cref="SByte4"/>.</param>
        /// <returns>A <see cref="SByte4"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref SByte4 left, ref SByte4 right, out SByte4 result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
				result.Z = (left.Z < right.Z) ? left.Z : right.Z;
				result.W = (left.W < right.W) ? left.W : right.W;
		}

        /// <summary>
        /// Returns a <see cref="SByte4"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="SByte4"/>.</param>
        /// <param name="right">The second source <see cref="SByte4"/>.</param>
        /// <returns>A <see cref="SByte4"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 Min(ref SByte4 left, ref SByte4 right)
		{
			Min(ref left, ref right, out SByte4 result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="SByte4"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="SByte4"/>.</param>
        /// <param name="right">The second source <see cref="SByte4"/>.</param>
        /// <returns>A <see cref="SByte4"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 Min(SByte4 left, SByte4 right)
		{
			return new SByte4()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
				W = (left.W < right.W) ? left.W : right.W,
			};
		}

        /// <summary>
        /// Returns a <see cref="SByte4"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="SByte4"/>.</param>
        /// <param name="right">The second source <see cref="SByte4"/>.</param>
        /// <param name="result">The output for the resultant <see cref="SByte4"/>.</param>
        /// <returns>A <see cref="SByte4"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref SByte4 left, ref SByte4 right, out SByte4 result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
				result.Z = (left.Z > right.Z) ? left.Z : right.Z;
				result.W = (left.W > right.W) ? left.W : right.W;
		}

        /// <summary>
        /// Returns a <see cref="SByte4"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="SByte4"/>.</param>
        /// <param name="right">The second source <see cref="SByte4"/>.</param>
        /// <returns>A <see cref="SByte4"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 Max(ref SByte4 left, ref SByte4 right)
		{
			Max(ref left, ref right, out SByte4 result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="SByte4"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="SByte4"/>.</param>
        /// <param name="right">The second source <see cref="SByte4"/>.</param>
        /// <returns>A <see cref="SByte4"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SByte4 Max(SByte4 left, SByte4 right)
		{
			return new SByte4()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
				W = (left.W > right.W) ? left.W : right.W,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="SByte4"/> vectors.
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
		public static sbyte DistanceSquared(ref SByte4 value1, ref SByte4 value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;
            int w = value1.W - value2.W;

            return (sbyte)((x * x) + (y * y) + (z * z) + (w * w));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="SByte4"/> vectors.
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
		public static sbyte DistanceSquared(SByte4 value1, SByte4 value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;
            int w = value1.W - value2.W;

            return (sbyte)((x * x) + (y * y) + (z * z) + (w * w));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="SByte4"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SByte4 Clamp(SByte4 value, sbyte min, sbyte max)
        {
			return new SByte4()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
				W = value.W < min ? min : value.W > max ? max : value.W,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="SByte4"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="SByte4"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref SByte4 value, ref SByte4 min, ref SByte4 max, out SByte4 result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
				result.Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z;
				result.W = value.W < min.W ? min.W : value.W > max.W ? max.W : value.W;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="SByte4"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SByte4 Clamp(SByte4 value, SByte4 min, SByte4 max)
        {
			return new SByte4()
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
        public static SByte4 Reflect(SByte4 vector, SByte4 normal)
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
        public static SByte4 Reflect(ref SByte4 vector, ref SByte4 normal)
        {
            int dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z) + (vector.W * normal.W);

            return new SByte4()
            {
				X = (sbyte)(vector.X - ((2 * dot) * normal.X)),
				Y = (sbyte)(vector.Y - ((2 * dot) * normal.Y)),
				Z = (sbyte)(vector.Z - ((2 * dot) * normal.Z)),
				W = (sbyte)(vector.W - ((2 * dot) * normal.W)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (sbyte x, sbyte y, sbyte z, sbyte w)(SByte4 val)
        {
            return (val.X, val.Y, val.Z, val.W);
        }

        public static implicit operator SByte4((sbyte x, sbyte y, sbyte z, sbyte w) val)
        {
            return new SByte4(val.x, val.y, val.z, val.w);
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
		public sbyte this[int index]
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
				throw new ArgumentOutOfRangeException("index", "Indices for SByte4 run from 0 to 3, inclusive.");
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
				throw new ArgumentOutOfRangeException("index", "Indices for SByte4 run from 0 to 3, inclusive.");
			}
		}
#endregion

#region Casts - vectors
		///<summary>Casts a <see cref="SByte4"/> to a <see cref="SByte2"/>.</summary>
		public static explicit operator SByte2(SByte4 value)
		{
			return new SByte2(value.X, value.Y);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="SByte3"/>.</summary>
		public static explicit operator SByte3(SByte4 value)
		{
			return new SByte3(value.X, value.Y, value.Z);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Byte2"/>.</summary>
		public static explicit operator Byte2(SByte4 value)
		{
			return new Byte2((byte)value.X, (byte)value.Y);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Byte3"/>.</summary>
		public static explicit operator Byte3(SByte4 value)
		{
			return new Byte3((byte)value.X, (byte)value.Y, (byte)value.Z);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Byte4"/>.</summary>
		public static explicit operator Byte4(SByte4 value)
		{
			return new Byte4((byte)value.X, (byte)value.Y, (byte)value.Z, (byte)value.W);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector2I"/>.</summary>
		public static explicit operator Vector2I(SByte4 value)
		{
			return new Vector2I((int)value.X, (int)value.Y);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector3I"/>.</summary>
		public static explicit operator Vector3I(SByte4 value)
		{
			return new Vector3I((int)value.X, (int)value.Y, (int)value.Z);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector4I"/>.</summary>
		public static explicit operator Vector4I(SByte4 value)
		{
			return new Vector4I((int)value.X, (int)value.Y, (int)value.Z, (int)value.W);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector2UI"/>.</summary>
		public static explicit operator Vector2UI(SByte4 value)
		{
			return new Vector2UI((uint)value.X, (uint)value.Y);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector3UI"/>.</summary>
		public static explicit operator Vector3UI(SByte4 value)
		{
			return new Vector3UI((uint)value.X, (uint)value.Y, (uint)value.Z);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector4UI"/>.</summary>
		public static explicit operator Vector4UI(SByte4 value)
		{
			return new Vector4UI((uint)value.X, (uint)value.Y, (uint)value.Z, (uint)value.W);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector2S"/>.</summary>
		public static explicit operator Vector2S(SByte4 value)
		{
			return new Vector2S((short)value.X, (short)value.Y);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector3S"/>.</summary>
		public static explicit operator Vector3S(SByte4 value)
		{
			return new Vector3S((short)value.X, (short)value.Y, (short)value.Z);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector4S"/>.</summary>
		public static explicit operator Vector4S(SByte4 value)
		{
			return new Vector4S((short)value.X, (short)value.Y, (short)value.Z, (short)value.W);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector2US"/>.</summary>
		public static explicit operator Vector2US(SByte4 value)
		{
			return new Vector2US((ushort)value.X, (ushort)value.Y);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector3US"/>.</summary>
		public static explicit operator Vector3US(SByte4 value)
		{
			return new Vector3US((ushort)value.X, (ushort)value.Y, (ushort)value.Z);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector4US"/>.</summary>
		public static explicit operator Vector4US(SByte4 value)
		{
			return new Vector4US((ushort)value.X, (ushort)value.Y, (ushort)value.Z, (ushort)value.W);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector2L"/>.</summary>
		public static explicit operator Vector2L(SByte4 value)
		{
			return new Vector2L((long)value.X, (long)value.Y);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector3L"/>.</summary>
		public static explicit operator Vector3L(SByte4 value)
		{
			return new Vector3L((long)value.X, (long)value.Y, (long)value.Z);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector4L"/>.</summary>
		public static explicit operator Vector4L(SByte4 value)
		{
			return new Vector4L((long)value.X, (long)value.Y, (long)value.Z, (long)value.W);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector2UL"/>.</summary>
		public static explicit operator Vector2UL(SByte4 value)
		{
			return new Vector2UL((ulong)value.X, (ulong)value.Y);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector3UL"/>.</summary>
		public static explicit operator Vector3UL(SByte4 value)
		{
			return new Vector3UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector4UL"/>.</summary>
		public static explicit operator Vector4UL(SByte4 value)
		{
			return new Vector4UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z, (ulong)value.W);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector2F"/>.</summary>
		public static explicit operator Vector2F(SByte4 value)
		{
			return new Vector2F((float)value.X, (float)value.Y);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector3F"/>.</summary>
		public static explicit operator Vector3F(SByte4 value)
		{
			return new Vector3F((float)value.X, (float)value.Y, (float)value.Z);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector4F"/>.</summary>
		public static explicit operator Vector4F(SByte4 value)
		{
			return new Vector4F((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector2D"/>.</summary>
		public static explicit operator Vector2D(SByte4 value)
		{
			return new Vector2D((double)value.X, (double)value.Y);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector3D"/>.</summary>
		public static explicit operator Vector3D(SByte4 value)
		{
			return new Vector3D((double)value.X, (double)value.Y, (double)value.Z);
		}

		///<summary>Casts a <see cref="SByte4"/> to a <see cref="Vector4D"/>.</summary>
		public static explicit operator Vector4D(SByte4 value)
		{
			return new Vector4D((double)value.X, (double)value.Y, (double)value.Z, (double)value.W);
		}

#endregion
	}
}

