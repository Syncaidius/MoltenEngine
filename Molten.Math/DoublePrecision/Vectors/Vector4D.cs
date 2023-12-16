using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten.DoublePrecision
{
	///<summary>A <see cref="double"/> vector comprised of four components.</summary>
	[StructLayout(LayoutKind.Explicit)]
    [Serializable]
	public partial struct Vector4D : IFormattable, ISignedVector<Vector4D, double>, IEquatable<Vector4D>
	{
		///<summary>The size of <see cref="Vector4D"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4D));

        ///<summary>The number of elements in the current vector type.</summary>
        public static readonly int NumElements = 4;

		///<summary>A Vector4D with every component set to 1D.</summary>
		public static readonly Vector4D One = new Vector4D(1D, 1D, 1D, 1D);

        static readonly string toStringFormat = "X:{0} Y:{1} Z:{2} W:{3}";

		/// <summary>The X unit <see cref="Vector4D"/>.</summary>
		public static readonly Vector4D UnitX = new Vector4D(1D, 0D, 0D, 0D);

		/// <summary>The Y unit <see cref="Vector4D"/>.</summary>
		public static readonly Vector4D UnitY = new Vector4D(0D, 1D, 0D, 0D);

		/// <summary>The Z unit <see cref="Vector4D"/>.</summary>
		public static readonly Vector4D UnitZ = new Vector4D(0D, 0D, 1D, 0D);

		/// <summary>The W unit <see cref="Vector4D"/>.</summary>
		public static readonly Vector4D UnitW = new Vector4D(0D, 0D, 0D, 1D);

		/// <summary>Represents a zero'd Vector4D.</summary>
		public static readonly Vector4D Zero = new Vector4D(0D, 0D, 0D, 0D);

		/// <summary>The X component.</summary>
		[DataMember]
		[FieldOffset(0)]
		public double X;

		/// <summary>The Y component.</summary>
		[DataMember]
		[FieldOffset(8)]
		public double Y;

		/// <summary>The Z component.</summary>
		[DataMember]
		[FieldOffset(16)]
		public double Z;

		/// <summary>The W component.</summary>
		[DataMember]
		[FieldOffset(24)]
		public double W;

		/// <summary>A fixed array mapped to the same memory space as the individual vector components.</summary>
		[FieldOffset(0)]
		public unsafe fixed double Values[4];

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0D && Y == 0D && Z == 0D && W == 0D;
        }

#region Constructors
		/// <summary>Initializes a new instance of <see cref="Vector4D"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Vector4D(double value)
		{
			X = value;
			Y = value;
			Z = value;
			W = value;
		}
		/// <summary>Initializes a new instance of <see cref="Vector4D"/> from an array.</summary>
		/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least four elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Vector4D(double[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least four input values for Vector4D.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
		}
		/// <summary>Initializes a new instance of <see cref="Vector4D"/> from a span.</summary>
		/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least four elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Vector4D(Span<double> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least four input values for Vector4D.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
		}
		/// <summary>Initializes a new instance of <see cref="Vector4D"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the X, Y, Z, W components of the color.
		/// <para>There must be at least four elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than four elements.</exception>
		public unsafe Vector4D(double* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			X = ptrValues[0];
			Y = ptrValues[1];
			Z = ptrValues[2];
			W = ptrValues[3];
		}
		/// <summary>
		/// Initializes a new instance of <see cref="Vector4D"/>.
		/// </summary>
		/// <param name="x">The X component.</param>
		/// <param name="y">The Y component.</param>
		/// <param name="z">The Z component.</param>
		/// <param name="w">The W component.</param>
		public Vector4D(double x, double y, double z, double w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		///<summary>Creates a new instance of <see cref="Vector4D"/>, using a <see cref="Vector2D"/> to populate the first two components.</summary>
		public Vector4D(Vector2D vector, double z, double w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
			W = w;
		}

		///<summary>Creates a new instance of <see cref="Vector4D"/>, using a <see cref="Vector3D"/> to populate the first three components.</summary>
		public Vector4D(Vector3D vector, double w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
			W = w;
		}

#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref = "Vector4D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector4D"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector4D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector4D other)
        {
            return MathHelper.NearEqual(other.X, X) && MathHelper.NearEqual(other.Y, Y) && MathHelper.NearEqual(other.Z, Z) && MathHelper.NearEqual(other.W, W);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector4D"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector4D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector4D other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4D"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector4D"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector4D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is Vector4D v)
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
        public double LengthSquared()
        {
            return ((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector4D"/>.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public double[] ToArray()
        {
            return new double[] { X, Y, Z, W };
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector4D"/>.
        /// </summary>
        /// <returns>A <see cref="Vector4D"/> facing the opposite direction.</returns>
		public Vector4D Negate()
		{
			return new Vector4D(-X, -Y, -Z, -W);
		}
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(double min, double max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
			W = W < min ? min : W > max ? max : W;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector4D min, Vector4D max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref Vector4D min, ref Vector4D max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
			W = W < min.W ? min.W : W > max.W ? max.W : W;
        }
#endregion

#region To-String
		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector4D"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, format, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector4D"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, toStringFormat, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector4D"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector4D"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, toStringFormat, X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector4D"/>.
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
		///<summary>Performs a add operation on two <see cref="Vector4D"/>.</summary>
		///<param name="a">The first <see cref="Vector4D"/> to add.</param>
		///<param name="b">The second <see cref="Vector4D"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Vector4D a, ref Vector4D b, out Vector4D result)
		{
			result.X = a.X + b.X;
			result.Y = a.Y + b.Y;
			result.Z = a.Z + b.Z;
			result.W = a.W + b.W;
		}

		///<summary>Performs a add operation on two <see cref="Vector4D"/>.</summary>
		///<param name="a">The first <see cref="Vector4D"/> to add.</param>
		///<param name="b">The second <see cref="Vector4D"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator +(Vector4D a, Vector4D b)
		{
			Add(ref a, ref b, out Vector4D result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="Vector4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Vector4D"/> to add.</param>
		///<param name="b">The <see cref="double"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Vector4D a, double b, out Vector4D result)
		{
			result.X = a.X + b;
			result.Y = a.Y + b;
			result.Z = a.Z + b;
			result.W = a.W + b;
		}

		///<summary>Performs a add operation on a <see cref="Vector4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Vector4D"/> to add.</param>
		///<param name="b">The <see cref="double"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator +(Vector4D a, double b)
		{
			Add(ref a, b, out Vector4D result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="double"/> and a <see cref="Vector4D"/>.</summary>
		///<param name="a">The <see cref="double"/> to add.</param>
		///<param name="b">The <see cref="Vector4D"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator +(double a, Vector4D b)
		{
			Add(ref b, a, out Vector4D result);
			return result;
		}


		/// <summary>
        /// Assert a <see cref="Vector4D"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector4D"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector4D"/>.</returns>
        public static Vector4D operator +(Vector4D value)
        {
            return value;
        }
#endregion

#region Subtract operators
		///<summary>Performs a subtract operation on two <see cref="Vector4D"/>.</summary>
		///<param name="a">The first <see cref="Vector4D"/> to subtract.</param>
		///<param name="b">The second <see cref="Vector4D"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Vector4D a, ref Vector4D b, out Vector4D result)
		{
			result.X = a.X - b.X;
			result.Y = a.Y - b.Y;
			result.Z = a.Z - b.Z;
			result.W = a.W - b.W;
		}

		///<summary>Performs a subtract operation on two <see cref="Vector4D"/>.</summary>
		///<param name="a">The first <see cref="Vector4D"/> to subtract.</param>
		///<param name="b">The second <see cref="Vector4D"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator -(Vector4D a, Vector4D b)
		{
			Subtract(ref a, ref b, out Vector4D result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="Vector4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Vector4D"/> to subtract.</param>
		///<param name="b">The <see cref="double"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Vector4D a, double b, out Vector4D result)
		{
			result.X = a.X - b;
			result.Y = a.Y - b;
			result.Z = a.Z - b;
			result.W = a.W - b;
		}

		///<summary>Performs a subtract operation on a <see cref="Vector4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Vector4D"/> to subtract.</param>
		///<param name="b">The <see cref="double"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator -(Vector4D a, double b)
		{
			Subtract(ref a, b, out Vector4D result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="double"/> and a <see cref="Vector4D"/>.</summary>
		///<param name="a">The <see cref="double"/> to subtract.</param>
		///<param name="b">The <see cref="Vector4D"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator -(double a, Vector4D b)
		{
			Subtract(ref b, a, out Vector4D result);
			return result;
		}


        /// <summary>
        /// Negate/reverse the direction of a <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector4D"/> to reverse.</param>
        /// <param name="result">The output for the reversed <see cref="Vector4D"/>.</param>
        public static void Negate(ref Vector4D value, out Vector4D result)
        {
			result.X = -value.X;
			result.Y = -value.Y;
			result.Z = -value.Z;
			result.W = -value.W;
            
        }

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector4D"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector4D"/>.</returns>
        public static Vector4D operator -(Vector4D value)
        {
            Negate(ref value, out value);
            return value;
        }
#endregion

#region division operators
		///<summary>Performs a divide operation on two <see cref="Vector4D"/>.</summary>
		///<param name="a">The first <see cref="Vector4D"/> to divide.</param>
		///<param name="b">The second <see cref="Vector4D"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Vector4D a, ref Vector4D b, out Vector4D result)
		{
			result.X = a.X / b.X;
			result.Y = a.Y / b.Y;
			result.Z = a.Z / b.Z;
			result.W = a.W / b.W;
		}

		///<summary>Performs a divide operation on two <see cref="Vector4D"/>.</summary>
		///<param name="a">The first <see cref="Vector4D"/> to divide.</param>
		///<param name="b">The second <see cref="Vector4D"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator /(Vector4D a, Vector4D b)
		{
			Divide(ref a, ref b, out Vector4D result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="Vector4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Vector4D"/> to divide.</param>
		///<param name="b">The <see cref="double"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Vector4D a, double b, out Vector4D result)
		{
			result.X = a.X / b;
			result.Y = a.Y / b;
			result.Z = a.Z / b;
			result.W = a.W / b;
		}

		///<summary>Performs a divide operation on a <see cref="Vector4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Vector4D"/> to divide.</param>
		///<param name="b">The <see cref="double"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator /(Vector4D a, double b)
		{
			Divide(ref a, b, out Vector4D result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="double"/> and a <see cref="Vector4D"/>.</summary>
		///<param name="a">The <see cref="double"/> to divide.</param>
		///<param name="b">The <see cref="Vector4D"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator /(double a, Vector4D b)
		{
			Divide(ref b, a, out Vector4D result);
			return result;
		}

#endregion

#region Multiply operators
		///<summary>Performs a multiply operation on two <see cref="Vector4D"/>.</summary>
		///<param name="a">The first <see cref="Vector4D"/> to multiply.</param>
		///<param name="b">The second <see cref="Vector4D"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Vector4D a, ref Vector4D b, out Vector4D result)
		{
			result.X = a.X * b.X;
			result.Y = a.Y * b.Y;
			result.Z = a.Z * b.Z;
			result.W = a.W * b.W;
		}

		///<summary>Performs a multiply operation on two <see cref="Vector4D"/>.</summary>
		///<param name="a">The first <see cref="Vector4D"/> to multiply.</param>
		///<param name="b">The second <see cref="Vector4D"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator *(Vector4D a, Vector4D b)
		{
			Multiply(ref a, ref b, out Vector4D result);
			return result;
		}

		///<summary>Performs a multiply operation on a <see cref="Vector4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Vector4D"/> to multiply.</param>
		///<param name="b">The <see cref="double"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Vector4D a, double b, out Vector4D result)
		{
			result.X = a.X * b;
			result.Y = a.Y * b;
			result.Z = a.Z * b;
			result.W = a.W * b;
		}

		///<summary>Performs a multiply operation on a <see cref="Vector4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Vector4D"/> to multiply.</param>
		///<param name="b">The <see cref="double"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator *(Vector4D a, double b)
		{
			Multiply(ref a, b, out Vector4D result);
			return result;
		}

		///<summary>Performs a multiply operation on a <see cref="double"/> and a <see cref="Vector4D"/>.</summary>
		///<param name="a">The <see cref="double"/> to multiply.</param>
		///<param name="b">The <see cref="Vector4D"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D operator *(double a, Vector4D b)
		{
			Multiply(ref b, a, out Vector4D result);
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
        public static bool operator ==(Vector4D left, Vector4D right)
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
        public static bool operator !=(Vector4D left, Vector4D right)
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
        public static Vector4D SmoothStep(ref Vector4D start, ref Vector4D end, double amount)
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
        public static Vector4D SmoothStep(Vector4D start, Vector4D end, double amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Vector4D"/>.</param>
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
        public static void Orthogonalize(Vector4D[] destination, params Vector4D[] source)
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
                Vector4D newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector4D"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector4D"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
		/// <param name="zIndex">The axis index to use for the new Z value.</param>
		/// <param name="wIndex">The axis index to use for the new W value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector4D Swizzle(Vector4D val, int xIndex, int yIndex, int zIndex, int wIndex)
        {
            return new Vector4D()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
			   Z = (&val.X)[zIndex],
			   W = (&val.X)[wIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector4D Swizzle(Vector4D val, uint xIndex, uint yIndex, uint zIndex, uint wIndex)
        {
            return new Vector4D()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
			    Z = (&val.X)[zIndex],
			    W = (&val.X)[wIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector4D"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector4D"/> source vector</param>
        /// <param name="right">Second <see cref="Vector4D"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(ref Vector4D left, ref Vector4D right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector4D"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector4D"/> source vector</param>
        /// <param name="right">Second <see cref="Vector4D"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(Vector4D left, Vector4D right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W));
        }

		/// <summary>
        /// Returns a <see cref="Vector4D"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector4D"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector4D"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector4D"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector4D Barycentric(ref Vector4D value1, ref Vector4D value2, ref Vector4D value3, double amount1, double amount2)
        {
			return new Vector4D(
				((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))), 
				((value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z))), 
				((value1.W + (amount1 * (value2.W - value1.W))) + (amount2 * (value3.W - value1.W)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4D"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Vector4D start, ref Vector4D end, double amount, out Vector4D result)
        {
			result.X = ((1D - amount) * start.X + amount * end.X);
			result.Y = ((1D - amount) * start.Y + amount * end.Y);
			result.Z = ((1D - amount) * start.Z + amount * end.Z);
			result.W = ((1D - amount) * start.W + amount * end.W);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4D Lerp(Vector4D start, Vector4D end, double amount)
        {
			return new Vector4D()
			{
				X = ((1D - amount) * start.X + amount * end.X),
				Y = ((1D - amount) * start.Y + amount * end.Y),
				Z = ((1D - amount) * start.Z + amount * end.Z),
				W = ((1D - amount) * start.W + amount * end.W),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4D Lerp(ref Vector4D start, ref Vector4D end, double amount)
        {
			return new Vector4D()
			{
				X = ((1D - amount) * start.X + amount * end.X),
				Y = ((1D - amount) * start.Y + amount * end.Y),
				Z = ((1D - amount) * start.Z + amount * end.Z),
				W = ((1D - amount) * start.W + amount * end.W),
			};
        }

        /// <summary>
        /// Returns a <see cref="Vector4D"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4D"/>.</param>
        /// <param name="right">The second source <see cref="Vector4D"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4D"/>.</param>
        /// <returns>A <see cref="Vector4D"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref Vector4D left, ref Vector4D right, out Vector4D result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
				result.Z = (left.Z < right.Z) ? left.Z : right.Z;
				result.W = (left.W < right.W) ? left.W : right.W;
		}

        /// <summary>
        /// Returns a <see cref="Vector4D"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4D"/>.</param>
        /// <param name="right">The second source <see cref="Vector4D"/>.</param>
        /// <returns>A <see cref="Vector4D"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D Min(ref Vector4D left, ref Vector4D right)
		{
			Min(ref left, ref right, out Vector4D result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector4D"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4D"/>.</param>
        /// <param name="right">The second source <see cref="Vector4D"/>.</param>
        /// <returns>A <see cref="Vector4D"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D Min(Vector4D left, Vector4D right)
		{
			return new Vector4D()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
				W = (left.W < right.W) ? left.W : right.W,
			};
		}

        /// <summary>
        /// Returns a <see cref="Vector4D"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4D"/>.</param>
        /// <param name="right">The second source <see cref="Vector4D"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4D"/>.</param>
        /// <returns>A <see cref="Vector4D"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref Vector4D left, ref Vector4D right, out Vector4D result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
				result.Z = (left.Z > right.Z) ? left.Z : right.Z;
				result.W = (left.W > right.W) ? left.W : right.W;
		}

        /// <summary>
        /// Returns a <see cref="Vector4D"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4D"/>.</param>
        /// <param name="right">The second source <see cref="Vector4D"/>.</param>
        /// <returns>A <see cref="Vector4D"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D Max(ref Vector4D left, ref Vector4D right)
		{
			Max(ref left, ref right, out Vector4D result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector4D"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4D"/>.</param>
        /// <param name="right">The second source <see cref="Vector4D"/>.</param>
        /// <returns>A <see cref="Vector4D"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4D Max(Vector4D left, Vector4D right)
		{
			return new Vector4D()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
				W = (left.W > right.W) ? left.W : right.W,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4D"/> vectors.
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
		public static double DistanceSquared(ref Vector4D value1, ref Vector4D value2)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;
            double w = value1.W - value2.W;

            return ((x * x) + (y * y) + (z * z) + (w * w));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Vector4D"/> vectors.
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
		public static double DistanceSquared(Vector4D value1, Vector4D value2)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;
            double w = value1.W - value2.W;

            return ((x * x) + (y * y) + (z * z) + (w * w));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector4D"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4D Clamp(Vector4D value, double min, double max)
        {
			return new Vector4D()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
				W = value.W < min ? min : value.W > max ? max : value.W,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector4D"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4D"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref Vector4D value, ref Vector4D min, ref Vector4D max, out Vector4D result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
				result.Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z;
				result.W = value.W < min.W ? min.W : value.W > max.W ? max.W : value.W;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector4D"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4D Clamp(Vector4D value, Vector4D min, Vector4D max)
        {
			return new Vector4D()
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
        public static Vector4D Reflect(Vector4D vector, Vector4D normal)
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
        public static Vector4D Reflect(ref Vector4D vector, ref Vector4D normal)
        {
            double dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z) + (vector.W * normal.W);

            return new Vector4D()
            {
				X = (vector.X - ((2 * dot) * normal.X)),
				Y = (vector.Y - ((2 * dot) * normal.Y)),
				Z = (vector.Z - ((2 * dot) * normal.Z)),
				W = (vector.W - ((2 * dot) * normal.W)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (double x, double y, double z, double w)(Vector4D val)
        {
            return (val.X, val.Y, val.Z, val.W);
        }

        public static implicit operator Vector4D((double x, double y, double z, double w) val)
        {
            return new Vector4D(val.x, val.y, val.z, val.w);
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
		public unsafe double this[int index]
		{
			get
            {
                if(index > 3 || index < 0)
                    throw new IndexOutOfRangeException("Index for Vector4D must be between from 0 to 3, inclusive.");

                return Values[index];
            }
            set
            {
                if (index > 3 || index < 0)
                    throw new IndexOutOfRangeException("Index for Vector4D must be between from 0 to 3, inclusive.");

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
		public unsafe double this[uint index]
		{
			get
            {
                if(index > 3)
                    throw new IndexOutOfRangeException("Index for Vector4D must be between from 0 to 3, inclusive.");

                return Values[index];
            }
            set
            {
                if (index > 3)
                    throw new IndexOutOfRangeException("Index for Vector4D must be between from 0 to 3, inclusive.");

                Values[index] = value;
            }
		}
#endregion

#region Casts - vectors
		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="SByte2"/>.</summary>
		public static explicit operator SByte2(Vector4D value)
		{
			return new SByte2((sbyte)value.X, (sbyte)value.Y);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="SByte3"/>.</summary>
		public static explicit operator SByte3(Vector4D value)
		{
			return new SByte3((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="SByte4"/>.</summary>
		public static explicit operator SByte4(Vector4D value)
		{
			return new SByte4((sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z, (sbyte)value.W);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Byte2"/>.</summary>
		public static explicit operator Byte2(Vector4D value)
		{
			return new Byte2((byte)value.X, (byte)value.Y);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Byte3"/>.</summary>
		public static explicit operator Byte3(Vector4D value)
		{
			return new Byte3((byte)value.X, (byte)value.Y, (byte)value.Z);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Byte4"/>.</summary>
		public static explicit operator Byte4(Vector4D value)
		{
			return new Byte4((byte)value.X, (byte)value.Y, (byte)value.Z, (byte)value.W);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector2I"/>.</summary>
		public static explicit operator Vector2I(Vector4D value)
		{
			return new Vector2I((int)value.X, (int)value.Y);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector3I"/>.</summary>
		public static explicit operator Vector3I(Vector4D value)
		{
			return new Vector3I((int)value.X, (int)value.Y, (int)value.Z);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector4I"/>.</summary>
		public static explicit operator Vector4I(Vector4D value)
		{
			return new Vector4I((int)value.X, (int)value.Y, (int)value.Z, (int)value.W);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector2UI"/>.</summary>
		public static explicit operator Vector2UI(Vector4D value)
		{
			return new Vector2UI((uint)value.X, (uint)value.Y);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector3UI"/>.</summary>
		public static explicit operator Vector3UI(Vector4D value)
		{
			return new Vector3UI((uint)value.X, (uint)value.Y, (uint)value.Z);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector4UI"/>.</summary>
		public static explicit operator Vector4UI(Vector4D value)
		{
			return new Vector4UI((uint)value.X, (uint)value.Y, (uint)value.Z, (uint)value.W);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector2S"/>.</summary>
		public static explicit operator Vector2S(Vector4D value)
		{
			return new Vector2S((short)value.X, (short)value.Y);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector3S"/>.</summary>
		public static explicit operator Vector3S(Vector4D value)
		{
			return new Vector3S((short)value.X, (short)value.Y, (short)value.Z);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector4S"/>.</summary>
		public static explicit operator Vector4S(Vector4D value)
		{
			return new Vector4S((short)value.X, (short)value.Y, (short)value.Z, (short)value.W);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector2US"/>.</summary>
		public static explicit operator Vector2US(Vector4D value)
		{
			return new Vector2US((ushort)value.X, (ushort)value.Y);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector3US"/>.</summary>
		public static explicit operator Vector3US(Vector4D value)
		{
			return new Vector3US((ushort)value.X, (ushort)value.Y, (ushort)value.Z);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector4US"/>.</summary>
		public static explicit operator Vector4US(Vector4D value)
		{
			return new Vector4US((ushort)value.X, (ushort)value.Y, (ushort)value.Z, (ushort)value.W);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector2L"/>.</summary>
		public static explicit operator Vector2L(Vector4D value)
		{
			return new Vector2L((long)value.X, (long)value.Y);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector3L"/>.</summary>
		public static explicit operator Vector3L(Vector4D value)
		{
			return new Vector3L((long)value.X, (long)value.Y, (long)value.Z);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector4L"/>.</summary>
		public static explicit operator Vector4L(Vector4D value)
		{
			return new Vector4L((long)value.X, (long)value.Y, (long)value.Z, (long)value.W);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector2UL"/>.</summary>
		public static explicit operator Vector2UL(Vector4D value)
		{
			return new Vector2UL((ulong)value.X, (ulong)value.Y);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector3UL"/>.</summary>
		public static explicit operator Vector3UL(Vector4D value)
		{
			return new Vector3UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector4UL"/>.</summary>
		public static explicit operator Vector4UL(Vector4D value)
		{
			return new Vector4UL((ulong)value.X, (ulong)value.Y, (ulong)value.Z, (ulong)value.W);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector2F"/>.</summary>
		public static explicit operator Vector2F(Vector4D value)
		{
			return new Vector2F((float)value.X, (float)value.Y);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector3F"/>.</summary>
		public static explicit operator Vector3F(Vector4D value)
		{
			return new Vector3F((float)value.X, (float)value.Y, (float)value.Z);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector4F"/>.</summary>
		public static explicit operator Vector4F(Vector4D value)
		{
			return new Vector4F((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector2D"/>.</summary>
		public static explicit operator Vector2D(Vector4D value)
		{
			return new Vector2D(value.X, value.Y);
		}

		///<summary>Casts a <see cref="Vector4D"/> to a <see cref="Vector3D"/>.</summary>
		public static explicit operator Vector3D(Vector4D value)
		{
			return new Vector3D(value.X, value.Y, value.Z);
		}

#endregion
	}
}

