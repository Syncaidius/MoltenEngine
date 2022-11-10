using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten
{
	///<summary>A <see cref = "ushort"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
    [Serializable]
	public partial struct Vector3US : IFormattable
	{
		///<summary>The X component.</summary>
        [DataMember]
		public ushort X;

		///<summary>The Y component.</summary>
        [DataMember]
		public ushort Y;

		///<summary>The Z component.</summary>
        [DataMember]
		public ushort Z;

		///<summary>The size of <see cref="Vector3US"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3US));

		///<summary>A Vector3US with every component set to (ushort)1.</summary>
		public static readonly Vector3US One = new Vector3US((ushort)1, (ushort)1, (ushort)1);

		/// <summary>The X unit <see cref="Vector3US"/>.</summary>
		public static readonly Vector3US UnitX = new Vector3US((ushort)1, 0, 0);

		/// <summary>The Y unit <see cref="Vector3US"/>.</summary>
		public static readonly Vector3US UnitY = new Vector3US(0, (ushort)1, 0);

		/// <summary>The Z unit <see cref="Vector3US"/>.</summary>
		public static readonly Vector3US UnitZ = new Vector3US(0, 0, (ushort)1);

		/// <summary>Represents a zero'd Vector3US.</summary>
		public static readonly Vector3US Zero = new Vector3US(0, 0, 0);

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0 && Y == 0 && Z == 0;
        }

#region Constructors
        ///<summary>Creates a new instance of <see cref = "Vector3US"/>, using a <see cref="Vector2US"/> to populate the first two components.</summary>
		public Vector3US(Vector2US vector, ushort z)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
		}

		///<summary>Creates a new instance of <see cref = "Vector3US"/>.</summary>
		public Vector3US(ushort x, ushort y, ushort z)
		{
			X = x;
			Y = y;
			Z = z;
		}

        ///<summary>Creates a new instance of <see cref = "Vector3US"/>.</summary>
		public Vector3US(ushort value)
		{
			X = value;
			Y = value;
			Z = value;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3US"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector3US(ushort[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for Vector3US.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3US"/> struct from an unsafe pointer. The pointer should point to an array of three elements.
        /// </summary>
		public unsafe Vector3US(ushort* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
		}
#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref="Vector3US"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector3US"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3US"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector3US other)
        {
            return MathHelper.NearEqual(other.X, X) && MathHelper.NearEqual(other.Y, Y) && MathHelper.NearEqual(other.Z, Z);
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
        /// Calculates the length of the vector.
        /// </summary>
        /// <returns>The length of the vector.</returns>
        /// <remarks>
        /// <see cref="Vector2F.LengthSquared"/> may be preferred when only the relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public ushort Length()
        {
            return (ushort)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
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
            return new ushort[] { X, Y, Z};
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

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture));
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
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3US"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3US"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
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

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }
#endregion

#region Add operators
        public static void Add(ref Vector3US left, ref Vector3US right, out Vector3US result)
        {
			result.X = (ushort)(left.X + right.X);
			result.Y = (ushort)(left.Y + right.Y);
			result.Z = (ushort)(left.Z + right.Z);
        }

        public static void Add(ref Vector3US left, ushort right, out Vector3US result)
        {
			result.X = (ushort)(left.X + right);
			result.Y = (ushort)(left.Y + right);
			result.Z = (ushort)(left.Z + right);
        }

		public static Vector3US operator +(Vector3US left, Vector3US right)
		{
			Add(ref left, ref right, out Vector3US result);
            return result;
		}

		public static Vector3US operator +(Vector3US left, ushort right)
		{
            Add(ref left, right, out Vector3US result);
            return result;
		}

        public static Vector3US operator +(ushort left, Vector3US right)
		{
            Add(ref right, left, out Vector3US result);
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
		public static void Subtract(ref Vector3US left, ref Vector3US right, out Vector3US result)
        {
			result.X = (ushort)(left.X - right.X);
			result.Y = (ushort)(left.Y - right.Y);
			result.Z = (ushort)(left.Z - right.Z);
        }

        public static void Subtract(ref Vector3US left, ushort right, out Vector3US result)
        {
			result.X = (ushort)(left.X - right);
			result.Y = (ushort)(left.Y - right);
			result.Z = (ushort)(left.Z - right);
        }

		public static Vector3US operator -(Vector3US left, Vector3US right)
		{
			Subtract(ref left, ref right, out Vector3US result);
            return result;
		}

		public static Vector3US operator -(Vector3US left, ushort right)
		{
            Subtract(ref left, right, out Vector3US result);
            return result;
		}

        public static Vector3US operator -(ushort left, Vector3US right)
		{
            Subtract(ref right, left, out Vector3US result);
            return result;
		}

#endregion

#region division operators
		public static void Divide(ref Vector3US left, ref Vector3US right, out Vector3US result)
        {
			result.X = (ushort)(left.X / right.X);
			result.Y = (ushort)(left.Y / right.Y);
			result.Z = (ushort)(left.Z / right.Z);
        }

        public static void Divide(ref Vector3US left, ushort right, out Vector3US result)
        {
			result.X = (ushort)(left.X / right);
			result.Y = (ushort)(left.Y / right);
			result.Z = (ushort)(left.Z / right);
        }

		public static Vector3US operator /(Vector3US left, Vector3US right)
		{
			Divide(ref left, ref right, out Vector3US result);
            return result;
		}

		public static Vector3US operator /(Vector3US left, ushort right)
		{
            Divide(ref left, right, out Vector3US result);
            return result;
		}

        public static Vector3US operator /(ushort left, Vector3US right)
		{
            Divide(ref right, left, out Vector3US result);
            return result;
		}
#endregion

#region Multiply operators
		public static void Multiply(ref Vector3US left, ref Vector3US right, out Vector3US result)
        {
			result.X = (ushort)(left.X * right.X);
			result.Y = (ushort)(left.Y * right.Y);
			result.Z = (ushort)(left.Z * right.Z);
        }

        public static void Multiply(ref Vector3US left, ushort right, out Vector3US result)
        {
			result.X = (ushort)(left.X * right);
			result.Y = (ushort)(left.Y * right);
			result.Z = (ushort)(left.Z * right);
        }

		public static Vector3US operator *(Vector3US left, Vector3US right)
		{
			Multiply(ref left, ref right, out Vector3US result);
            return result;
		}

		public static Vector3US operator *(Vector3US left, ushort right)
		{
            Multiply(ref left, right, out Vector3US result);
            return result;
		}

        public static Vector3US operator *(ushort left, Vector3US right)
		{
            Multiply(ref right, left, out Vector3US result);
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

#region Operators - Cast
        ///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector2US"/>.</summary>
        public static explicit operator Vector2US(Vector3US value)
        {
            return new Vector2US(value.X, value.Y);
        }

        ///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector4US"/>.</summary>
        public static explicit operator Vector4US(Vector3US value)
        {
            return new Vector4US(value.X, value.Y, value.Z, 0);
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
        public static bool NearEqual(Vector3US left, Vector3US right, Vector3US epsilon)
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
        public static bool NearEqual(ref Vector3US left, ref Vector3US right, ref Vector3US epsilon)
        {
            return MathHelper.WithinEpsilon(left.X, right.X, epsilon.X) && MathHelper.WithinEpsilon(left.Y, right.Y, epsilon.Y) && MathHelper.WithinEpsilon(left.Z, right.Z, epsilon.Z);
        }

        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
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
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector3US.DistanceSquared(Vector3US, Vector3US)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static ushort Distance(ref Vector3US value1, ref Vector3US value2)
        {
			ushort x = (ushort)(value1.X - value2.X);
			ushort y = (ushort)(value1.Y - value2.Y);
			ushort z = (ushort)(value1.Z - value2.Z);

            return (ushort)Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector3US.DistanceSquared(Vector3US, Vector3US)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static ushort Distance(Vector3US value1, Vector3US value2)
        {
			ushort x = (ushort)(value1.X - value2.X);
			ushort y = (ushort)(value1.Y - value2.Y);
			ushort z = (ushort)(value1.Z - value2.Z);

            return (ushort)Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        /// <summary>Checks to see if any value (x, y, z, w) are within 0.0001 of 0.
        /// If so this method truncates that value to zero.</summary>
        /// <param name="power">The power.</param>
        /// <param name="vec">The vector.</param>
        public static Vector3US Pow(Vector3US vec, ushort power)
        {
            return new Vector3US()
            {
				X = (ushort)Math.Pow(vec.X, power),
				Y = (ushort)Math.Pow(vec.Y, power),
				Z = (ushort)Math.Pow(vec.Z, power),
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector3US"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3US"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3US"/> source vector.</param>
        public static ushort Dot(ref Vector3US left, ref Vector3US right)
        {
			return (ushort)(((ushort)left.X * right.X) + ((ushort)left.Y * right.Y) + ((ushort)left.Z * right.Z));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector3US"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3US"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3US"/> source vector.</param>
        public static ushort Dot(Vector3US left, Vector3US right)
        {
			return (ushort)((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector3US"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector3US"/> vector.</param>
        /// <param name="value2">Second source position <see cref="Vector3US"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector3US"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Vector3US Hermite(ref Vector3US value1, ref Vector3US tangent1, ref Vector3US value2, ref Vector3US tangent2, ushort amount)
        {
            float squared = amount * amount;
            float cubed = amount * squared;
            float part1 = ((2.0F * cubed) - (3.0F * squared)) + 1.0F;
            float part2 = (-2.0F * cubed) + (3.0F * squared);
            float part3 = (cubed - (2.0F * squared)) + amount;
            float part4 = cubed - squared;

			return new Vector3US()
			{
				X = (ushort)((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4)),
				Y = (ushort)((((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4)),
				Z = (ushort)((((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4)),
			};
        }

        /// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector3US"/>.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector3US"/>.</param>
        /// <param name="value2">Second source position <see cref="Vector3US"/>.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector3US"/>.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of the Hermite spline interpolation.</returns>
        public static Vector3US Hermite(Vector3US value1, Vector3US tangent1, Vector3US value2, Vector3US tangent2, ushort amount)
        {
            return Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount);
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
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
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
        /// <returns>A <see cref="Vector3US"/> containing the smallest components of the source vectors.</returns>
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
        /// <returns>A <see cref="Vector3US"/> containing the largest components of the source vectors.</returns>
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
				X = (ushort)(vector.X - ((2.0F * dot) * normal.X)),
				Y = (ushort)(vector.Y - ((2.0F * dot) * normal.Y)),
				Z = (ushort)(vector.Z - ((2.0F * dot) * normal.Z)),
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
        /// <value>The value of the X, Y or Z component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
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
        ///<summary>Casts a <see cref="Vector3US"/> to a <see cref="SByte3"/>.</summary>
        public static explicit operator SByte3(Vector3US val)
        {
            return new SByte3()
            {
                X = (sbyte)val.X,
                Y = (sbyte)val.Y,
                Z = (sbyte)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Byte3"/>.</summary>
        public static explicit operator Byte3(Vector3US val)
        {
            return new Byte3()
            {
                X = (byte)val.X,
                Y = (byte)val.Y,
                Z = (byte)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3I"/>.</summary>
        public static explicit operator Vector3I(Vector3US val)
        {
            return new Vector3I()
            {
                X = (int)val.X,
                Y = (int)val.Y,
                Z = (int)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3UI"/>.</summary>
        public static explicit operator Vector3UI(Vector3US val)
        {
            return new Vector3UI()
            {
                X = val.X,
                Y = val.Y,
                Z = val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3S"/>.</summary>
        public static explicit operator Vector3S(Vector3US val)
        {
            return new Vector3S()
            {
                X = (short)val.X,
                Y = (short)val.Y,
                Z = (short)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3L"/>.</summary>
        public static explicit operator Vector3L(Vector3US val)
        {
            return new Vector3L()
            {
                X = (long)val.X,
                Y = (long)val.Y,
                Z = (long)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3UL"/>.</summary>
        public static explicit operator Vector3UL(Vector3US val)
        {
            return new Vector3UL()
            {
                X = val.X,
                Y = val.Y,
                Z = val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3F"/>.</summary>
        public static explicit operator Vector3F(Vector3US val)
        {
            return new Vector3F()
            {
                X = (float)val.X,
                Y = (float)val.Y,
                Z = (float)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3US"/> to a <see cref="Vector3D"/>.</summary>
        public static explicit operator Vector3D(Vector3US val)
        {
            return new Vector3D()
            {
                X = (double)val.X,
                Y = (double)val.Y,
                Z = (double)val.Z,
            };
        }

#endregion
	}
}

