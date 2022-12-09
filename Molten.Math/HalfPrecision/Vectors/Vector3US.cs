using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten.HalfPrecision
{
	///<summary>A <see cref = "ushort"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
    [Serializable]
	public partial struct Vector3US : IFormattable, IVector<ushort>
	{
		///<summary>The size of <see cref="Vector3US"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3US));

		///<summary>A Vector3US with every component set to (ushort)1.</summary>
		public static readonly Vector3US One = new Vector3US((ushort)1, (ushort)1, (ushort)1);

        static readonly string toStringFormat = "X:{0} Y:{1} Z:{2}";

		/// <summary>The X unit <see cref="Vector3US"/>.</summary>
		public static readonly Vector3US UnitX = new Vector3US((ushort)1, (ushort)0, (ushort)0);

		/// <summary>The Y unit <see cref="Vector3US"/>.</summary>
		public static readonly Vector3US UnitY = new Vector3US((ushort)0, (ushort)1, (ushort)0);

		/// <summary>The Z unit <see cref="Vector3US"/>.</summary>
		public static readonly Vector3US UnitZ = new Vector3US((ushort)0, (ushort)0, (ushort)1);

		/// <summary>Represents a zero'd Vector3US.</summary>
		public static readonly Vector3US Zero = new Vector3US((ushort)0, (ushort)0, (ushort)0);

		/// <summary>The X component.</summary>
		[DataMember]
		public ushort X;

		/// <summary>The Y component.</summary>
		[DataMember]
		public ushort Y;

		/// <summary>The Z component.</summary>
		[DataMember]
		public ushort Z;


        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == (ushort)0 && Y == (ushort)0 && Z == (ushort)0;
        }

#region Constructors
		/// <summary>Initializes a new instance of <see cref="Vector3US"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Vector3US(ushort value)
		{
			X = value;
			Y = value;
			Z = value;
		}
		/// <summary>Initializes a new instance of <see cref="Vector3US"/> from an array.</summary>
		/// <param name="values">The values to assign to the X, Y, Z components of the color. This must be an array with at least three elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Vector3US(ushort[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 3)
				throw new ArgumentOutOfRangeException("values", "There must be at least three input values for Vector3US.");

			X = values[0];
			Y = values[1];
			Z = values[2];
		}
		/// <summary>Initializes a new instance of <see cref="Vector3US"/> from a span.</summary>
		/// <param name="values">The values to assign to the X, Y, Z components of the color. This must be an array with at least three elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Vector3US(Span<ushort> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 3)
				throw new ArgumentOutOfRangeException("values", "There must be at least three input values for Vector3US.");

			X = values[0];
			Y = values[1];
			Z = values[2];
		}
		/// <summary>Initializes a new instance of <see cref="Vector3US"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the X, Y, Z components of the color.
		/// <para>There must be at least three elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than four elements.</exception>
		public unsafe Vector3US(ushort* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			X = ptrValues[0];
			Y = ptrValues[1];
			Z = ptrValues[2];
		}
		/// <summary>
		/// Initializes a new instance of <see cref="Vector3US"/>.
		/// </summary>
		/// <param name="x">The X component.</param>
		/// <param name="y">The Y component.</param>
		/// <param name="z">The Z component.</param>
		public Vector3US(ushort x, ushort y, ushort z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		///<summary>Creates a new instance of <see cref="Vector3US"/>, using a <see cref="Vector2US"/> to populate the first two components.</summary>
		public Vector3US(Vector2US vector, ushort z)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
		}

#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref = "Vector3US"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector3US"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3US"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector3US other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector3US"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector3US"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3US"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3US other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector3US"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector3US"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3US"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is not Vector3US)
                return false;

            var strongValue = (Vector3US)value;
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
        public ushort LengthSquared()
        {
            return (ushort)((X * X) + (Y * Y) + (Z * Z));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector3US"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public ushort[] ToArray()
        {
            return new ushort[] { X, Y, Z };
        }
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ushort min, ushort max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector3US min, Vector3US max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref Vector3US min, ref Vector3US max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
        }
#endregion

#region To-String
		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3US"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3US"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, format, X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3US"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3US"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, toStringFormat, X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3US"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3US"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, toStringFormat, X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3US"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3US"/>.
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
		///<summary>Performs a add operation on two <see cref="Vector3US"/>.</summary>
		///<param name="a">The first <see cref="Vector3US"/> to add.</param>
		///<param name="b">The second <see cref="Vector3US"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Vector3US a, ref Vector3US b, out Vector3US result)
		{
			result.X = (ushort)(a.X + b.X);
			result.Y = (ushort)(a.Y + b.Y);
			result.Z = (ushort)(a.Z + b.Z);
		}

		///<summary>Performs a add operation on two <see cref="Vector3US"/>.</summary>
		///<param name="a">The first <see cref="Vector3US"/> to add.</param>
		///<param name="b">The second <see cref="Vector3US"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator +(Vector3US a, Vector3US b)
		{
			Add(ref a, ref b, out Vector3US result);
			return result;
		}

		///<summary>Performs a add operation on a $<see cref="Vector3US"/> and a $<see cref="ushort"/>.</summary>
		///<param name="a">The <see cref="Vector3US"/> to add.</param>
		///<param name="b">The <see cref="ushort"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Vector3US a, ushort b, out Vector3US result)
		{
			result.X = (ushort)(a.X + b);
			result.Y = (ushort)(a.Y + b);
			result.Z = (ushort)(a.Z + b);
		}

		///<summary>Performs a add operation on a $<see cref="Vector3US"/> and a $<see cref="ushort"/>.</summary>
		///<param name="a">The <see cref="Vector3US"/> to add.</param>
		///<param name="b">The <see cref="ushort"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator +(Vector3US a, ushort b)
		{
			Add(ref a, b, out Vector3US result);
			return result;
		}

		///<summary>Performs a add operation on a $<see cref="ushort"/> and a $<see cref="Vector3US"/>.</summary>
		///<param name="a">The <see cref="ushort"/> to add.</param>
		///<param name="b">The <see cref="Vector3US"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator +(ushort a, Vector3US b)
		{
			Add(ref b, a, out Vector3US result);
			return result;
		}


		/// <summary>
        /// Assert a <see cref="Vector3US"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector3US"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector3US"/>.</returns>
        public static Vector3US operator +(Vector3US value)
        {
            return value;
        }
#endregion

#region Subtract operators
		///<summary>Performs a subtract operation on two <see cref="Vector3US"/>.</summary>
		///<param name="a">The first <see cref="Vector3US"/> to subtract.</param>
		///<param name="b">The second <see cref="Vector3US"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Vector3US a, ref Vector3US b, out Vector3US result)
		{
			result.X = (ushort)(a.X - b.X);
			result.Y = (ushort)(a.Y - b.Y);
			result.Z = (ushort)(a.Z - b.Z);
		}

		///<summary>Performs a subtract operation on two <see cref="Vector3US"/>.</summary>
		///<param name="a">The first <see cref="Vector3US"/> to subtract.</param>
		///<param name="b">The second <see cref="Vector3US"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator -(Vector3US a, Vector3US b)
		{
			Subtract(ref a, ref b, out Vector3US result);
			return result;
		}

		///<summary>Performs a subtract operation on a $<see cref="Vector3US"/> and a $<see cref="ushort"/>.</summary>
		///<param name="a">The <see cref="Vector3US"/> to subtract.</param>
		///<param name="b">The <see cref="ushort"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Vector3US a, ushort b, out Vector3US result)
		{
			result.X = (ushort)(a.X - b);
			result.Y = (ushort)(a.Y - b);
			result.Z = (ushort)(a.Z - b);
		}

		///<summary>Performs a subtract operation on a $<see cref="Vector3US"/> and a $<see cref="ushort"/>.</summary>
		///<param name="a">The <see cref="Vector3US"/> to subtract.</param>
		///<param name="b">The <see cref="ushort"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator -(Vector3US a, ushort b)
		{
			Subtract(ref a, b, out Vector3US result);
			return result;
		}

		///<summary>Performs a subtract operation on a $<see cref="ushort"/> and a $<see cref="Vector3US"/>.</summary>
		///<param name="a">The <see cref="ushort"/> to subtract.</param>
		///<param name="b">The <see cref="Vector3US"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator -(ushort a, Vector3US b)
		{
			Subtract(ref b, a, out Vector3US result);
			return result;
		}


#endregion

#region division operators
		///<summary>Performs a divide operation on two <see cref="Vector3US"/>.</summary>
		///<param name="a">The first <see cref="Vector3US"/> to divide.</param>
		///<param name="b">The second <see cref="Vector3US"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Vector3US a, ref Vector3US b, out Vector3US result)
		{
			result.X = (ushort)(a.X / b.X);
			result.Y = (ushort)(a.Y / b.Y);
			result.Z = (ushort)(a.Z / b.Z);
		}

		///<summary>Performs a divide operation on two <see cref="Vector3US"/>.</summary>
		///<param name="a">The first <see cref="Vector3US"/> to divide.</param>
		///<param name="b">The second <see cref="Vector3US"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator /(Vector3US a, Vector3US b)
		{
			Divide(ref a, ref b, out Vector3US result);
			return result;
		}

		///<summary>Performs a divide operation on a $<see cref="Vector3US"/> and a $<see cref="ushort"/>.</summary>
		///<param name="a">The <see cref="Vector3US"/> to divide.</param>
		///<param name="b">The <see cref="ushort"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Vector3US a, ushort b, out Vector3US result)
		{
			result.X = (ushort)(a.X / b);
			result.Y = (ushort)(a.Y / b);
			result.Z = (ushort)(a.Z / b);
		}

		///<summary>Performs a divide operation on a $<see cref="Vector3US"/> and a $<see cref="ushort"/>.</summary>
		///<param name="a">The <see cref="Vector3US"/> to divide.</param>
		///<param name="b">The <see cref="ushort"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator /(Vector3US a, ushort b)
		{
			Divide(ref a, b, out Vector3US result);
			return result;
		}

		///<summary>Performs a divide operation on a $<see cref="ushort"/> and a $<see cref="Vector3US"/>.</summary>
		///<param name="a">The <see cref="ushort"/> to divide.</param>
		///<param name="b">The <see cref="Vector3US"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator /(ushort a, Vector3US b)
		{
			Divide(ref b, a, out Vector3US result);
			return result;
		}

#endregion

#region Multiply operators
		///<summary>Performs a multiply operation on two <see cref="Vector3US"/>.</summary>
		///<param name="a">The first <see cref="Vector3US"/> to multiply.</param>
		///<param name="b">The second <see cref="Vector3US"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Vector3US a, ref Vector3US b, out Vector3US result)
		{
			result.X = (ushort)(a.X * b.X);
			result.Y = (ushort)(a.Y * b.Y);
			result.Z = (ushort)(a.Z * b.Z);
		}

		///<summary>Performs a multiply operation on two <see cref="Vector3US"/>.</summary>
		///<param name="a">The first <see cref="Vector3US"/> to multiply.</param>
		///<param name="b">The second <see cref="Vector3US"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator *(Vector3US a, Vector3US b)
		{
			Multiply(ref a, ref b, out Vector3US result);
			return result;
		}

		///<summary>Performs a multiply operation on a $<see cref="Vector3US"/> and a $<see cref="ushort"/>.</summary>
		///<param name="a">The <see cref="Vector3US"/> to multiply.</param>
		///<param name="b">The <see cref="ushort"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Vector3US a, ushort b, out Vector3US result)
		{
			result.X = (ushort)(a.X * b);
			result.Y = (ushort)(a.Y * b);
			result.Z = (ushort)(a.Z * b);
		}

		///<summary>Performs a multiply operation on a $<see cref="Vector3US"/> and a $<see cref="ushort"/>.</summary>
		///<param name="a">The <see cref="Vector3US"/> to multiply.</param>
		///<param name="b">The <see cref="ushort"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator *(Vector3US a, ushort b)
		{
			Multiply(ref a, b, out Vector3US result);
			return result;
		}

		///<summary>Performs a multiply operation on a $<see cref="ushort"/> and a $<see cref="Vector3US"/>.</summary>
		///<param name="a">The <see cref="ushort"/> to multiply.</param>
		///<param name="b">The <see cref="Vector3US"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US operator *(ushort a, Vector3US b)
		{
			Multiply(ref b, a, out Vector3US result);
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
        public static bool operator ==(Vector3US left, Vector3US right)
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
        public static bool operator !=(Vector3US left, Vector3US right)
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
        public static Vector3US SmoothStep(ref Vector3US start, ref Vector3US end, float amount)
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
        public static Vector3US SmoothStep(Vector3US start, Vector3US end, ushort amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Vector3US"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Vector3US"/>.</param>
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
        public static void Orthogonalize(Vector3US[] destination, params Vector3US[] source)
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
                Vector3US newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (ushort)(Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector3US"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector3US"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
		/// <param name="zIndex">The axis index to use for the new Z value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector3US Swizzle(Vector3US val, int xIndex, int yIndex, int zIndex)
        {
            return new Vector3US()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
			   Z = (&val.X)[zIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector3US Swizzle(Vector3US val, uint xIndex, uint yIndex, uint zIndex)
        {
            return new Vector3US()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
			    Z = (&val.X)[zIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector3US"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3US"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3US"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Dot(ref Vector3US left, ref Vector3US right)
        {
			return (ushort)(((ushort)left.X * right.X) + ((ushort)left.Y * right.Y) + ((ushort)left.Z * right.Z));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector3US"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3US"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3US"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Dot(Vector3US left, Vector3US right)
        {
			return (ushort)((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

		/// <summary>
        /// Returns a <see cref="Vector3US"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector3US"/> containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector3US"/> containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector3US"/> containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector3US Barycentric(ref Vector3US value1, ref Vector3US value2, ref Vector3US value3, ushort amount1, ushort amount2)
        {
			return new Vector3US(
				(ushort)((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				(ushort)((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))), 
				(ushort)((value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3US"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3US"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Vector3US start, ref Vector3US end, float amount, out Vector3US result)
        {
			result.X = (ushort)((1F - amount) * start.X + amount * end.X);
			result.Y = (ushort)((1F - amount) * start.Y + amount * end.Y);
			result.Z = (ushort)((1F - amount) * start.Z + amount * end.Z);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3US"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3US Lerp(Vector3US start, Vector3US end, float amount)
        {
			return new Vector3US()
			{
				X = (ushort)((1F - amount) * start.X + amount * end.X),
				Y = (ushort)((1F - amount) * start.Y + amount * end.Y),
				Z = (ushort)((1F - amount) * start.Z + amount * end.Z),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3US"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3US Lerp(ref Vector3US start, ref Vector3US end, float amount)
        {
			return new Vector3US()
			{
				X = (ushort)((1F - amount) * start.X + amount * end.X),
				Y = (ushort)((1F - amount) * start.Y + amount * end.Y),
				Z = (ushort)((1F - amount) * start.Z + amount * end.Z),
			};
        }

        /// <summary>
        /// Returns a <see cref="Vector3US"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3US"/>.</param>
        /// <param name="right">The second source <see cref="Vector3US"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3US"/>.</param>
        /// <returns>A <see cref="Vector3US"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref Vector3US left, ref Vector3US right, out Vector3US result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
				result.Z = (left.Z < right.Z) ? left.Z : right.Z;
		}

        /// <summary>
        /// Returns a <see cref="Vector3US"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3US"/>.</param>
        /// <param name="right">The second source <see cref="Vector3US"/>.</param>
        /// <returns>A <see cref="Vector3US"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US Min(ref Vector3US left, ref Vector3US right)
		{
			Min(ref left, ref right, out Vector3US result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector3US"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3US"/>.</param>
        /// <param name="right">The second source <see cref="Vector3US"/>.</param>
        /// <returns>A <see cref="Vector3US"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US Min(Vector3US left, Vector3US right)
		{
			return new Vector3US()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

        /// <summary>
        /// Returns a <see cref="Vector3US"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3US"/>.</param>
        /// <param name="right">The second source <see cref="Vector3US"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3US"/>.</param>
        /// <returns>A <see cref="Vector3US"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref Vector3US left, ref Vector3US right, out Vector3US result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
				result.Z = (left.Z > right.Z) ? left.Z : right.Z;
		}

        /// <summary>
        /// Returns a <see cref="Vector3US"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3US"/>.</param>
        /// <param name="right">The second source <see cref="Vector3US"/>.</param>
        /// <returns>A <see cref="Vector3US"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US Max(ref Vector3US left, ref Vector3US right)
		{
			Max(ref left, ref right, out Vector3US result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector3US"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3US"/>.</param>
        /// <param name="right">The second source <see cref="Vector3US"/>.</param>
        /// <returns>A <see cref="Vector3US"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3US Max(Vector3US left, Vector3US right)
		{
			return new Vector3US()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3US"/> vectors.
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
		public static ushort DistanceSquared(ref Vector3US value1, ref Vector3US value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;

            return (ushort)((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Vector3US"/> vectors.
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
		public static ushort DistanceSquared(Vector3US value1, Vector3US value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;

            return (ushort)((x * x) + (y * y) + (z * z));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3US"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3US Clamp(Vector3US value, ushort min, ushort max)
        {
			return new Vector3US()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3US"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Vector3US"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref Vector3US value, ref Vector3US min, ref Vector3US max, out Vector3US result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
				result.Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3US"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3US Clamp(Vector3US value, Vector3US min, Vector3US max)
        {
			return new Vector3US()
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
        public static Vector3US Reflect(ref Vector3US vector, ref Vector3US normal)
        {
            int dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z);

            return new Vector3US()
            {
				X = (ushort)(vector.X - ((2 * dot) * normal.X)),
				Y = (ushort)(vector.Y - ((2 * dot) * normal.Y)),
				Z = (ushort)(vector.Z - ((2 * dot) * normal.Z)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (ushort x, ushort y, ushort z)(Vector3US val)
        {
            return (val.X, val.Y, val.Z);
        }

        public static implicit operator Vector3US((ushort x, ushort y, ushort z) val)
        {
            return new Vector3US(val.x, val.y, val.z);
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of a component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on. This must be between 0 and 2</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 2].</exception>  
		public ushort this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3US run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3US run from 0 to 2, inclusive.");
			}
		}
#endregion

#region Casts - vectors
		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="SByte2"/>.</summary>
		public static explicit operator SByte2(Vector3US value)
		{
			return new SByte2((sbyte)value.X, (sbyte)value.Y);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="SByte3"/>.</summary>
		public static explicit operator SByte3(Vector3US value)
		{
			return new SByte3((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="SByte4"/>.</summary>
		public static explicit operator SByte4(Vector3US value)
		{
			return new SByte4((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z, (sbyte)1);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Byte2"/>.</summary>
		public static explicit operator Byte2(Vector3US value)
		{
			return new Byte2((byte)value.X, (byte)value.Y);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Byte3"/>.</summary>
		public static explicit operator Byte3(Vector3US value)
		{
			return new Byte3((byte)value.X, (byte)value.Y, (byte)value.Z);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Byte4"/>.</summary>
		public static explicit operator Byte4(Vector3US value)
		{
			return new Byte4((byte)value.X, (byte)value.Y, (byte)value.Z, (byte)1);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector2I"/>.</summary>
		public static explicit operator Vector2I(Vector3US value)
		{
			return new Vector2I((int)value.X, (int)value.Y);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3I"/>.</summary>
		public static explicit operator Vector3I(Vector3US value)
		{
			return new Vector3I((int)value.X, (int)value.Y, (int)value.Z);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector4I"/>.</summary>
		public static explicit operator Vector4I(Vector3US value)
		{
			return new Vector4I((int)value.X, (int)value.Y, (int)value.Z, 1);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector2UI"/>.</summary>
		public static explicit operator Vector2UI(Vector3US value)
		{
			return new Vector2UI((uint)value.X, (uint)value.Y);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3UI"/>.</summary>
		public static explicit operator Vector3UI(Vector3US value)
		{
			return new Vector3UI((uint)value.X, (uint)value.Y, (uint)value.Z);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector4UI"/>.</summary>
		public static explicit operator Vector4UI(Vector3US value)
		{
			return new Vector4UI((uint)value.X, (uint)value.Y, (uint)value.Z, 1U);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector2S"/>.</summary>
		public static explicit operator Vector2S(Vector3US value)
		{
			return new Vector2S((short)value.X, (short)value.Y);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3S"/>.</summary>
		public static explicit operator Vector3S(Vector3US value)
		{
			return new Vector3S((short)value.X, (short)value.Y, (short)value.Z);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector4S"/>.</summary>
		public static explicit operator Vector4S(Vector3US value)
		{
			return new Vector4S((short)value.X, (short)value.Y, (short)value.Z, (short)1);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector2US"/>.</summary>
		public static explicit operator Vector2US(Vector3US value)
		{
			return new Vector2US(value.X, value.Y);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector4US"/>.</summary>
		public static explicit operator Vector4US(Vector3US value)
		{
			return new Vector4US(value.X, value.Y, value.Z, (ushort)1);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector2L"/>.</summary>
		public static explicit operator Vector2L(Vector3US value)
		{
			return new Vector2L((long)value.X, (long)value.Y);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3L"/>.</summary>
		public static explicit operator Vector3L(Vector3US value)
		{
			return new Vector3L((long)value.X, (long)value.Y, (long)value.Z);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector4L"/>.</summary>
		public static explicit operator Vector4L(Vector3US value)
		{
			return new Vector4L((long)value.X, (long)value.Y, (long)value.Z, 1L);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector2UL"/>.</summary>
		public static explicit operator Vector2UL(Vector3US value)
		{
			return new Vector2UL((ulong)value.X, (ulong)value.Y);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3UL"/>.</summary>
		public static explicit operator Vector3UL(Vector3US value)
		{
			return new Vector3UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector4UL"/>.</summary>
		public static explicit operator Vector4UL(Vector3US value)
		{
			return new Vector4UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z, 1UL);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector2F"/>.</summary>
		public static explicit operator Vector2F(Vector3US value)
		{
			return new Vector2F((float)value.X, (float)value.Y);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3F"/>.</summary>
		public static explicit operator Vector3F(Vector3US value)
		{
			return new Vector3F((float)value.X, (float)value.Y, (float)value.Z);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector4F"/>.</summary>
		public static explicit operator Vector4F(Vector3US value)
		{
			return new Vector4F((float)value.X, (float)value.Y, (float)value.Z, 1F);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector2D"/>.</summary>
		public static explicit operator Vector2D(Vector3US value)
		{
			return new Vector2D((double)value.X, (double)value.Y);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3D"/>.</summary>
		public static explicit operator Vector3D(Vector3US value)
		{
			return new Vector3D((double)value.X, (double)value.Y, (double)value.Z);
		}

		///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector4D"/>.</summary>
		public static explicit operator Vector4D(Vector3US value)
		{
			return new Vector4D((double)value.X, (double)value.Y, (double)value.Z, 1D);
		}

#endregion
	}
}

