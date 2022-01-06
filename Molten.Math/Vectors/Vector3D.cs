using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace Molten
{
	///<summary>A <see cref = "double"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector3D : IFormattable
	{
		///<summary>The X component.</summary>
		public double X;

		///<summary>The Y component.</summary>
		public double Y;

		///<summary>The Z component.</summary>
		public double Z;


		///<summary>The size of <see cref="Vector3D"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3D));

		///<summary>A Vector3D with every component set to 1D.</summary>
		public static readonly Vector3D One = new Vector3D(1D, 1D, 1D);

		/// <summary>The X unit <see cref="Vector3D"/>.</summary>
		public static readonly Vector3D UnitX = new Vector3D(1D, 0D, 0D);

		/// <summary>The Y unit <see cref="Vector3D"/>.</summary>
		public static readonly Vector3D UnitY = new Vector3D(0D, 1D, 0D);

		/// <summary>The Z unit <see cref="Vector3D"/>.</summary>
		public static readonly Vector3D UnitZ = new Vector3D(0D, 0D, 1D);

		/// <summary>Represents a zero'd Vector3D.</summary>
		public static readonly Vector3D Zero = new Vector3D(0D, 0D, 0D);

		 /// <summary>
        /// Gets a value indicting whether this instance is normalized.
        /// </summary>
        public bool IsNormalized
        {
            get => MathHelperDP.IsOne((X * X) + (Y * Y) + (Z * Z));
        }

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0D && Y == 0D && Z == 0D;
        }

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector3D"/>.</summary>
		public Vector3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

        ///<summary>Creates a new instance of <see cref = "Vector3D"/>.</summary>
		public Vector3D(double value)
		{
			X = value;
			Y = value;
			Z = value;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3D"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector3D(double[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for Vector3D.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3D"/> struct from an unsafe pointer. The pointer should point to an array of three elements.
        /// </summary>
		public unsafe Vector3D(double* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
		}
#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref="Vector3D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector3D"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector3D other)
        {
            return MathHelperDP.NearEqual(other.X, X) && MathHelperDP.NearEqual(other.Y, Y) && MathHelperDP.NearEqual(other.Z, Z);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector3D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector3D"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3D other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector3D"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector3D"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (!(value is Vector3D))
                return false;

            var strongValue = (Vector3D)value;
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
        /// Calculates the length of the vector.
        /// </summary>
        /// <returns>The length of the vector.</returns>
        /// <remarks>
        /// <see cref="Vector2F.LengthSquared"/> may be preferred when only the relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public double Length()
        {
            return Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
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
            return ((X * X) + (Y * Y) + (Z * Z));
        }

        /// <summary>
        /// Converts the vector into a unit vector.
        /// </summary>
        public void Normalize()
        {
            double length = Length();
            if (!MathHelperDP.IsZero(length))
            {
                double inverse = 1.0D / length;
			    X = (X * inverse);
			    Y = (Y * inverse);
			    Z = (Z * inverse);
            }
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector3D"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public double[] ToArray()
        {
            return new double[] { X, Y, Z};
        }
		/// <summary>
        /// Reverses the direction of the current <see cref="Vector3D"/>.
        /// </summary>
        /// <returns>A <see cref="Vector3D"/> facing the opposite direction.</returns>
		public Vector3D Negate()
		{
			return new Vector3D(-X, -Y, -Z);
		}
		
        /// <summary>
        /// Returns a normalized unit vector of the original vector.
        /// </summary>
        public Vector3D GetNormalized()
        {
            double length = Length();
            if (!MathHelperDP.IsZero(length))
            {
                double inverse = 1.0D / length;
                return new Vector3D()
                {
			        X = (this.X * inverse),
			        Y = (this.Y * inverse),
			        Z = (this.Z * inverse),
                };
            }
            else
            {
                return new Vector3D();
            }
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(double min, double max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector3D min, Vector3D max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3D"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3D"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3D"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3D"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3D"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }
#endregion

#region Add operators
        public static void Add(ref Vector3D left, ref Vector3D right, out Vector3D result)
        {
			result.X = (left.X + right.X);
			result.Y = (left.Y + right.Y);
			result.Z = (left.Z + right.Z);
        }

        public static void Add(ref Vector3D left, double right, out Vector3D result)
        {
			result.X = (left.X + right);
			result.Y = (left.Y + right);
			result.Z = (left.Z + right);
        }

		public static Vector3D operator +(Vector3D left, Vector3D right)
		{
			Add(ref left, ref right, out Vector3D result);
            return result;
		}

		public static Vector3D operator +(Vector3D left, double right)
		{
            Add(ref left, right, out Vector3D result);
            return result;
		}

        public static Vector3D operator +(double left, Vector3D right)
		{
            Add(ref right, left, out Vector3D result);
            return result;
		}

		/// <summary>
        /// Assert a <see cref="Vector3D"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector3D"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector3D"/>.</returns>
        public static Vector3D operator +(Vector3D value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static void Subtract(ref Vector3D left, ref Vector3D right, out Vector3D result)
        {
			result.X = (left.X - right.X);
			result.Y = (left.Y - right.Y);
			result.Z = (left.Z - right.Z);
        }

        public static void Subtract(ref Vector3D left, double right, out Vector3D result)
        {
			result.X = (left.X - right);
			result.Y = (left.Y - right);
			result.Z = (left.Z - right);
        }

		public static Vector3D operator -(Vector3D left, Vector3D right)
		{
			Subtract(ref left, ref right, out Vector3D result);
            return result;
		}

		public static Vector3D operator -(Vector3D left, double right)
		{
            Subtract(ref left, right, out Vector3D result);
            return result;
		}

        public static Vector3D operator -(double left, Vector3D right)
		{
            Subtract(ref right, left, out Vector3D result);
            return result;
		}

        /// <summary>
        /// Negate/reverse the direction of a <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3D"/> to reverse.</param>
        /// <param name="result">The output for the reversed <see cref="Vector3D"/>.</param>
        public static void Negate(ref Vector3D value, out Vector3D result)
        {
			result.X = -value.X;
			result.Y = -value.Y;
			result.Z = -value.Z;
            
        }

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3D"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector3D"/>.</returns>
        public static Vector3D operator -(Vector3D value)
        {
            Negate(ref value, out value);
            return value;
        }
#endregion

#region division operators
		public static void Divide(ref Vector3D left, ref Vector3D right, out Vector3D result)
        {
			result.X = (left.X / right.X);
			result.Y = (left.Y / right.Y);
			result.Z = (left.Z / right.Z);
        }

        public static void Divide(ref Vector3D left, double right, out Vector3D result)
        {
			result.X = (left.X / right);
			result.Y = (left.Y / right);
			result.Z = (left.Z / right);
        }

		public static Vector3D operator /(Vector3D left, Vector3D right)
		{
			Divide(ref left, ref right, out Vector3D result);
            return result;
		}

		public static Vector3D operator /(Vector3D left, double right)
		{
            Divide(ref left, right, out Vector3D result);
            return result;
		}

        public static Vector3D operator /(double left, Vector3D right)
		{
            Divide(ref right, left, out Vector3D result);
            return result;
		}
#endregion

#region Multiply operators
		public static void Multiply(ref Vector3D left, ref Vector3D right, out Vector3D result)
        {
			result.X = (left.X * right.X);
			result.Y = (left.Y * right.Y);
			result.Z = (left.Z * right.Z);
        }

        public static void Multiply(ref Vector3D left, double right, out Vector3D result)
        {
			result.X = (left.X * right);
			result.Y = (left.Y * right);
			result.Z = (left.Z * right);
        }

		public static Vector3D operator *(Vector3D left, Vector3D right)
		{
			Multiply(ref left, ref right, out Vector3D result);
            return result;
		}

		public static Vector3D operator *(Vector3D left, double right)
		{
            Multiply(ref left, right, out Vector3D result);
            return result;
		}

        public static Vector3D operator *(double left, Vector3D right)
		{
            Multiply(ref right, left, out Vector3D result);
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
        public static bool operator ==(Vector3D left, Vector3D right)
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
        public static bool operator !=(Vector3D left, Vector3D right)
        {
            return !left.Equals(ref right);
        }
#endregion

#region Operators - Cast
        ///<summary>Casts a <see cref="Vector3D"/> to a <see cref="Vector2D"/>.</summary>
        public static explicit operator Vector2D(Vector3D value)
        {
            return new Vector2D(value.X, value.Y);
        }

        ///<summary>Casts a <see cref="Vector3D"/> to a <see cref="Vector4D"/>.</summary>
        public static explicit operator Vector4D(Vector3D value)
        {
            return new Vector4D(value.X, value.Y, value.Z, 0D);
        }

#endregion

#region Static Methods
        /// <summary>
        /// Tests whether one 3D vector is near another 3D vector.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns><c>true</c> if left and right are near another 3D, <c>false</c> otherwise</returns>
        public static bool NearEqual(Vector3D left, Vector3D right, Vector3D epsilon)
        {
            return NearEqual(ref left, ref right, ref epsilon);
        }

        /// <summary>
        /// Tests whether one 3D vector is near another 3D vector.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns><c>true</c> if left and right are near another 3D, <c>false</c> otherwise</returns>
        public static bool NearEqual(ref Vector3D left, ref Vector3D right, ref Vector3D epsilon)
        {
            return MathHelperDP.WithinEpsilon(left.X, right.X, epsilon.X) && MathHelperDP.WithinEpsilon(left.Y, right.Y, epsilon.Y) && MathHelperDP.WithinEpsilon(left.Z, right.Z, epsilon.Z);
        }

        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        public static Vector3D SmoothStep(ref Vector3D start, ref Vector3D end, double amount)
        {
            amount = MathHelperDP.SmoothStep(amount);
            return Lerp(ref start, ref end, amount);
        }

        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two vectors.</returns>
        public static Vector3D SmoothStep(Vector3D start, Vector3D end, double amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Vector3D"/>.</param>
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
        public static void Orthogonalize(Vector3D[] destination, params Vector3D[] source)
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
                Vector3D newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        /// <summary>
        /// Orthonormalizes a list of vectors.
        /// </summary>
        /// <param name="destination">The list of orthonormalized vectors.</param>
        /// <param name="source">The list of vectors to orthonormalize.</param>
        /// <remarks>
        /// <para>Orthonormalization is the process of making all vectors orthogonal to each
        /// other and making all vectors of unit length. This means that any given vector will
        /// be orthogonal to any other given vector in the list.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
        /// tend to be numerically unstable. The numeric stability decreases according to the vectors
        /// position in the list so that the first vector is the most stable and the last vector is the
        /// least stable.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Orthonormalize(Vector3D[] destination, params Vector3D[] source)
        {
            //Uses the modified Gram-Schmidt process.
            //Because we are making unit vectors, we can optimize the math for orthogonalization
            //and simplify the projection operation to remove the division.
            //q1 = m1 / |m1|
            //q2 = (m2 - (q1 ⋅ m2) * q1) / |m2 - (q1 ⋅ m2) * q1|
            //q3 = (m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2) / |m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2|
            //q4 = (m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3) / |m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3|
            //q5 = ...

            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
            {
                Vector3D newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= Dot(destination[r], newvector) * destination[r];

                newvector.Normalize();
                destination[i] = newvector;
            }
        }

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector3D"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector3D"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
		/// <param name="zIndex">The axis index to use for the new Z value.</param>
        /// <returns></returns>
        public static unsafe Vector3D Swizzle(Vector3D val, int xIndex, int yIndex, int zIndex)
        {
            return new Vector3D()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
			   Z = (&val.X)[zIndex],
            };
        }

        /// <returns></returns>
        public static unsafe Vector3D Swizzle(Vector3D val, uint xIndex, uint yIndex, uint zIndex)
        {
            return new Vector3D()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
			    Z = (&val.X)[zIndex],
            };
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector3D.DistanceSquared(Vector3D, Vector3D)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static double Distance(Vector3D value1, Vector3D value2)
        {
			double x = (value1.X - value2.X);
			double y = (value1.Y - value2.Y);
			double z = (value1.Z - value2.Z);

            return Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        /// <summary>Checks to see if any value (x, y, z, w) are within 0.0001 of 0.
        /// If so this method truncates that value to zero.</summary>
        /// <param name="power">The power.</param>
        /// <param name="vec">The vector.</param>
        public static Vector3D Pow(Vector3D vec, double power)
        {
            return new Vector3D()
            {
				X = Math.Pow(vec.X, power),
				Y = Math.Pow(vec.Y, power),
				Z = Math.Pow(vec.Z, power),
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector3D"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3D"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3D"/> source vector.</param>
        public static double Dot(ref Vector3D left, ref Vector3D right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector3D"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3D"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3D"/> source vector.</param>
        public static double Dot(Vector3D left, Vector3D right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector3D"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector3D"/> vector.</param>
        /// <param name="value2">Second source position <see cref="Vector3D"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector3D"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Vector3D Hermite(ref Vector3D value1, ref Vector3D tangent1, ref Vector3D value2, ref Vector3D tangent2, double amount)
        {
            double squared = amount * amount;
            double cubed = amount * squared;
            double part1 = ((2.0D * cubed) - (3.0D * squared)) + 1.0D;
            double part2 = (-2.0D * cubed) + (3.0D * squared);
            double part3 = (cubed - (2.0D * squared)) + amount;
            double part4 = cubed - squared;

			return new Vector3D()
			{
				X = (((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
				Y = (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4),
				Z = (((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4),
			};
        }

        /// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector3D"/>.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector3D"/>.</param>
        /// <param name="value2">Second source position <see cref="Vector3D"/>.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector3D"/>.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of the Hermite spline interpolation.</returns>
        public static Vector3D Hermite(Vector3D value1, Vector3D tangent1, Vector3D value2, Vector3D tangent2, double amount)
        {
            return Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount);
        }

		/// <summary>
        /// Returns a <see cref="Vector3D"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector3D"/> containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector3D"/> containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector3D"/> containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector3D Barycentric(ref Vector3D value1, ref Vector3D value2, ref Vector3D value3, double amount1, double amount2)
        {
			return new Vector3D(
				((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))), 
				((value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Vector3D start, ref Vector3D end, double amount, out Vector3D result)
        {
			result.X = ((1D - amount) * start.X + amount * end.X);
			result.Y = ((1D - amount) * start.Y + amount * end.Y);
			result.Z = ((1D - amount) * start.Z + amount * end.Z);
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector3D Lerp(ref Vector3D start, ref Vector3D end, double amount)
        {
			return new Vector3D()
			{
				X = ((1D - amount) * start.X + amount * end.X),
				Y = ((1D - amount) * start.Y + amount * end.Y),
				Z = ((1D - amount) * start.Z + amount * end.Z),
			};
        }

        /// <summary>
        /// Returns a <see cref="Vector3D"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3D"/>.</param>
        /// <param name="right">The second source <see cref="Vector3D"/>.</param>
        /// <returns>A <see cref="Vector3D"/> containing the smallest components of the source vectors.</returns>
		public static Vector3D Min(ref Vector3D left, ref Vector3D right)
		{
			return new Vector3D()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Returns a <see cref="Vector3D"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3D"/>.</param>
        /// <param name="right">The second source <see cref="Vector3D"/>.</param>
        /// <returns>A <see cref="Vector3D"/> containing the smallest components of the source vectors.</returns>
		public static Vector3D Min(Vector3D left, Vector3D right)
		{
			return new Vector3D()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

        /// <summary>
        /// Returns a <see cref="Vector3D"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3D"/>.</param>
        /// <param name="right">The second source <see cref="Vector3D"/>.</param>
        /// <returns>A <see cref="Vector3D"/> containing the largest components of the source vectors.</returns>
		public static Vector3D Max(ref Vector3D left, ref Vector3D right)
		{
			return new Vector3D()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Returns a <see cref="Vector3D"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3D"/>.</param>
        /// <param name="right">The second source <see cref="Vector3D"/>.</param>
        /// <returns>A <see cref="Vector3D"/> containing the largest components of the source vectors.</returns>
		public static Vector3D Max(Vector3D left, Vector3D right)
		{
			return new Vector3D()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3D"/> vectors.
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
		public static double DistanceSquared(ref Vector3D value1, ref Vector3D value2)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;

            return ((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Vector3D"/> vectors.
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
		public static double DistanceSquared(Vector3D value1, Vector3D value2)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;

            return ((x * x) + (y * y) + (z * z));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3D"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector3D Clamp(Vector3D value, double min, double max)
        {
			return new Vector3D()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3D"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector3D Clamp(Vector3D value, Vector3D min, Vector3D max)
        {
			return new Vector3D()
			{
				X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
				Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y,
				Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z,
			};
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using the specified positions.
        /// </summary>
        /// <param name="value1">The first position in the interpolation.</param>
        /// <param name="value2">The second position in the interpolation.</param>
        /// <param name="value3">The third position in the interpolation.</param>
        /// <param name="value4">The fourth position in the interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Vector3D CatmullRom(ref Vector3D value1, ref Vector3D value2, ref Vector3D value3, ref Vector3D value4, double amount)
        {
            double squared = amount * amount;
            double cubed = amount * squared;

            return new Vector3D()
            {
				X = (0.5D * ((((2D * value2.X) + 
                ((-value1.X + value3.X) * amount)) + 
                (((((2D * value1.X) - (5D * value2.X)) + (4D * value3.X)) - value4.X) * squared)) +
                ((((-value1.X + (3D * value2.X)) - (3D * value3.X)) + value4.X) * cubed))),

				Y = (0.5D * ((((2D * value2.Y) + 
                ((-value1.Y + value3.Y) * amount)) + 
                (((((2D * value1.Y) - (5D * value2.Y)) + (4D * value3.Y)) - value4.Y) * squared)) +
                ((((-value1.Y + (3D * value2.Y)) - (3D * value3.Y)) + value4.Y) * cubed))),

				Z = (0.5D * ((((2D * value2.Z) + 
                ((-value1.Z + value3.Z) * amount)) + 
                (((((2D * value1.Z) - (5D * value2.Z)) + (4D * value3.Z)) - value4.Z) * squared)) +
                ((((-value1.Z + (3D * value2.Z)) - (3D * value3.Z)) + value4.Z) * cubed))),

            };
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using the specified positions.
        /// </summary>
        /// <param name="value1">The first position in the interpolation.</param>
        /// <param name="value2">The second position in the interpolation.</param>
        /// <param name="value3">The third position in the interpolation.</param>
        /// <param name="value4">The fourth position in the interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>A vector that is the result of the Catmull-Rom interpolation.</returns>
        public static Vector3D CatmullRom(Vector3D value1, Vector3D value2, Vector3D value3, Vector3D value4, double amount)
        {
            return CatmullRom(ref value1, ref value2, ref value3, ref value4, amount);
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal. 
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">Normal of the surface.</param>
        /// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
        /// whether the original vector was close enough to the surface to hit it.</remarks>
        public static Vector3D Reflect(ref Vector3D vector, ref Vector3D normal)
        {
            double dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z);

            return new Vector3D()
            {
				X = (vector.X - ((2.0D * dot) * normal.X)),
				Y = (vector.Y - ((2.0D * dot) * normal.Y)),
				Z = (vector.Z - ((2.0D * dot) * normal.Z)),
            };
        }

        /// <summary>
        /// Converts the <see cref="Vector3D"/> into a unit vector.
        /// </summary>
        /// <param name="value">The <see cref="Vector3D"/> to normalize.</param>
        /// <returns>The normalized <see cref="Vector3D"/>.</returns>
        public static Vector3D Normalize(Vector3D value)
        {
            value.Normalize();
            return value;
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y or Z component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 2].</exception>
        
		public double this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3D run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3D run from 0 to 2, inclusive.");
			}
		}
#endregion
	}
}

