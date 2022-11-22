using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten
{
	///<summary>A <see cref = "uint"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
    [Serializable]
	public partial struct Vector3UI : IFormattable
	{
		///<summary>The X component.</summary>
        [DataMember]
		public uint X;

		///<summary>The Y component.</summary>
        [DataMember]
		public uint Y;

		///<summary>The Z component.</summary>
        [DataMember]
		public uint Z;

		///<summary>The size of <see cref="Vector3UI"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3UI));

		///<summary>A Vector3UI with every component set to 1U.</summary>
		public static readonly Vector3UI One = new Vector3UI(1U, 1U, 1U);

		/// <summary>The X unit <see cref="Vector3UI"/>.</summary>
		public static readonly Vector3UI UnitX = new Vector3UI(1U, 0, 0);

		/// <summary>The Y unit <see cref="Vector3UI"/>.</summary>
		public static readonly Vector3UI UnitY = new Vector3UI(0, 1U, 0);

		/// <summary>The Z unit <see cref="Vector3UI"/>.</summary>
		public static readonly Vector3UI UnitZ = new Vector3UI(0, 0, 1U);

		/// <summary>Represents a zero'd Vector3UI.</summary>
		public static readonly Vector3UI Zero = new Vector3UI(0, 0, 0);

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0 && Y == 0 && Z == 0;
        }

#region Constructors
        ///<summary>Creates a new instance of <see cref = "Vector3UI"/>, using a <see cref="Vector2UI"/> to populate the first two components.</summary>
		public Vector3UI(Vector2UI vector, uint z)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
		}

		///<summary>Creates a new instance of <see cref = "Vector3UI"/>.</summary>
		public Vector3UI(uint x, uint y, uint z)
		{
			X = x;
			Y = y;
			Z = z;
		}

        ///<summary>Creates a new instance of <see cref = "Vector3UI"/>.</summary>
		public Vector3UI(uint value)
		{
			X = value;
			Y = value;
			Z = value;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3UI"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector3UI(uint[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for Vector3UI.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3UI"/> struct from an unsafe pointer. The pointer should point to an array of three elements.
        /// </summary>
		public unsafe Vector3UI(uint* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
		}
#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref="Vector3UI"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector3UI"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3UI"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector3UI other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector3UI"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector3UI"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3UI"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3UI other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector3UI"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector3UI"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector3UI"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is not Vector3UI)
                return false;

            var strongValue = (Vector3UI)value;
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
        public uint LengthSquared()
        {
            return ((X * X) + (Y * Y) + (Z * Z));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector3UI"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public uint[] ToArray()
        {
            return new uint[] { X, Y, Z};
        }
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(uint min, uint max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector3UI min, Vector3UI max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3UI"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3UI"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3UI"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3UI"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3UI"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3UI"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3UI"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3UI"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }
#endregion

#region Add operators
        public static void Add(ref Vector3UI left, ref Vector3UI right, out Vector3UI result)
        {
			result.X = (left.X + right.X);
			result.Y = (left.Y + right.Y);
			result.Z = (left.Z + right.Z);
        }

        public static void Add(ref Vector3UI left, uint right, out Vector3UI result)
        {
			result.X = (left.X + right);
			result.Y = (left.Y + right);
			result.Z = (left.Z + right);
        }

		public static Vector3UI operator +(Vector3UI left, Vector3UI right)
		{
			Add(ref left, ref right, out Vector3UI result);
            return result;
		}

		public static Vector3UI operator +(Vector3UI left, uint right)
		{
            Add(ref left, right, out Vector3UI result);
            return result;
		}

        public static Vector3UI operator +(uint left, Vector3UI right)
		{
            Add(ref right, left, out Vector3UI result);
            return result;
		}

		/// <summary>
        /// Assert a <see cref="Vector3UI"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector3UI"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector3UI"/>.</returns>
        public static Vector3UI operator +(Vector3UI value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static void Subtract(ref Vector3UI left, ref Vector3UI right, out Vector3UI result)
        {
			result.X = (left.X - right.X);
			result.Y = (left.Y - right.Y);
			result.Z = (left.Z - right.Z);
        }

        public static void Subtract(ref Vector3UI left, uint right, out Vector3UI result)
        {
			result.X = (left.X - right);
			result.Y = (left.Y - right);
			result.Z = (left.Z - right);
        }

		public static Vector3UI operator -(Vector3UI left, Vector3UI right)
		{
			Subtract(ref left, ref right, out Vector3UI result);
            return result;
		}

		public static Vector3UI operator -(Vector3UI left, uint right)
		{
            Subtract(ref left, right, out Vector3UI result);
            return result;
		}

        public static Vector3UI operator -(uint left, Vector3UI right)
		{
            Subtract(ref right, left, out Vector3UI result);
            return result;
		}

#endregion

#region division operators
		public static void Divide(ref Vector3UI left, ref Vector3UI right, out Vector3UI result)
        {
			result.X = (left.X / right.X);
			result.Y = (left.Y / right.Y);
			result.Z = (left.Z / right.Z);
        }

        public static void Divide(ref Vector3UI left, uint right, out Vector3UI result)
        {
			result.X = (left.X / right);
			result.Y = (left.Y / right);
			result.Z = (left.Z / right);
        }

		public static Vector3UI operator /(Vector3UI left, Vector3UI right)
		{
			Divide(ref left, ref right, out Vector3UI result);
            return result;
		}

		public static Vector3UI operator /(Vector3UI left, uint right)
		{
            Divide(ref left, right, out Vector3UI result);
            return result;
		}

        public static Vector3UI operator /(uint left, Vector3UI right)
		{
            Divide(ref right, left, out Vector3UI result);
            return result;
		}
#endregion

#region Multiply operators
		public static void Multiply(ref Vector3UI left, ref Vector3UI right, out Vector3UI result)
        {
			result.X = (left.X * right.X);
			result.Y = (left.Y * right.Y);
			result.Z = (left.Z * right.Z);
        }

        public static void Multiply(ref Vector3UI left, uint right, out Vector3UI result)
        {
			result.X = (left.X * right);
			result.Y = (left.Y * right);
			result.Z = (left.Z * right);
        }

		public static Vector3UI operator *(Vector3UI left, Vector3UI right)
		{
			Multiply(ref left, ref right, out Vector3UI result);
            return result;
		}

		public static Vector3UI operator *(Vector3UI left, uint right)
		{
            Multiply(ref left, right, out Vector3UI result);
            return result;
		}

        public static Vector3UI operator *(uint left, Vector3UI right)
		{
            Multiply(ref right, left, out Vector3UI result);
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
        public static bool operator ==(Vector3UI left, Vector3UI right)
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
        public static bool operator !=(Vector3UI left, Vector3UI right)
        {
            return !left.Equals(ref right);
        }
#endregion

#region Operators - Cast
        ///<summary>Casts a <see cref="Vector3UI"/> to a <see cref="Vector2UI"/>.</summary>
        public static explicit operator Vector2UI(Vector3UI value)
        {
            return new Vector2UI(value.X, value.Y);
        }

        ///<summary>Casts a <see cref="Vector3UI"/> to a <see cref="Vector4UI"/>.</summary>
        public static explicit operator Vector4UI(Vector3UI value)
        {
            return new Vector4UI(value.X, value.Y, value.Z, 0);
        }

#endregion

#region Static Methods
        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        public static Vector3UI SmoothStep(ref Vector3UI start, ref Vector3UI end, float amount)
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
        public static Vector3UI SmoothStep(Vector3UI start, Vector3UI end, uint amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Vector3UI"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Vector3UI"/>.</param>
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
        public static void Orthogonalize(Vector3UI[] destination, params Vector3UI[] source)
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
                Vector3UI newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector3UI"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector3UI"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
		/// <param name="zIndex">The axis index to use for the new Z value.</param>
        /// <returns></returns>
        public static unsafe Vector3UI Swizzle(Vector3UI val, int xIndex, int yIndex, int zIndex)
        {
            return new Vector3UI()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
			   Z = (&val.X)[zIndex],
            };
        }

        /// <returns></returns>
        public static unsafe Vector3UI Swizzle(Vector3UI val, uint xIndex, uint yIndex, uint zIndex)
        {
            return new Vector3UI()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
			    Z = (&val.X)[zIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector3UI"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3UI"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3UI"/> source vector.</param>
        public static uint Dot(ref Vector3UI left, ref Vector3UI right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector3UI"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector3UI"/> source vector</param>
        /// <param name="right">Second <see cref="Vector3UI"/> source vector.</param>
        public static uint Dot(Vector3UI left, Vector3UI right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

		/// <summary>
        /// Returns a <see cref="Vector3UI"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector3UI"/> containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector3UI"/> containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector3UI"/> containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector3UI Barycentric(ref Vector3UI value1, ref Vector3UI value2, ref Vector3UI value3, uint amount1, uint amount2)
        {
			return new Vector3UI(
				((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))), 
				((value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3UI"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Vector3UI start, ref Vector3UI end, float amount, out Vector3UI result)
        {
			result.X = (uint)((1F - amount) * start.X + amount * end.X);
			result.Y = (uint)((1F - amount) * start.Y + amount * end.Y);
			result.Z = (uint)((1F - amount) * start.Z + amount * end.Z);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3UI"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector3UI Lerp(Vector3UI start, Vector3UI end, float amount)
        {
			return new Vector3UI()
			{
				X = (uint)((1F - amount) * start.X + amount * end.X),
				Y = (uint)((1F - amount) * start.Y + amount * end.Y),
				Z = (uint)((1F - amount) * start.Z + amount * end.Z),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3UI"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector3UI Lerp(ref Vector3UI start, ref Vector3UI end, float amount)
        {
			return new Vector3UI()
			{
				X = (uint)((1F - amount) * start.X + amount * end.X),
				Y = (uint)((1F - amount) * start.Y + amount * end.Y),
				Z = (uint)((1F - amount) * start.Z + amount * end.Z),
			};
        }

        /// <summary>
        /// Returns a <see cref="Vector3UI"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector3UI"/>.</param>
        /// <returns>A <see cref="Vector3UI"/> containing the smallest components of the source vectors.</returns>
		public static void Min(ref Vector3UI left, ref Vector3UI right, out Vector3UI result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
				result.Z = (left.Z < right.Z) ? left.Z : right.Z;
		}

        /// <summary>
        /// Returns a <see cref="Vector3UI"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector3UI"/>.</param>
        /// <returns>A <see cref="Vector3UI"/> containing the smallest components of the source vectors.</returns>
		public static Vector3UI Min(ref Vector3UI left, ref Vector3UI right)
		{
			Min(ref left, ref right, out Vector3UI result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector3UI"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector3UI"/>.</param>
        /// <returns>A <see cref="Vector3UI"/> containing the smallest components of the source vectors.</returns>
		public static Vector3UI Min(Vector3UI left, Vector3UI right)
		{
			return new Vector3UI()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

        /// <summary>
        /// Returns a <see cref="Vector3UI"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector3UI"/>.</param>
        /// <returns>A <see cref="Vector3UI"/> containing the largest components of the source vectors.</returns>
		public static void Max(ref Vector3UI left, ref Vector3UI right, out Vector3UI result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
				result.Z = (left.Z > right.Z) ? left.Z : right.Z;
		}

        /// <summary>
        /// Returns a <see cref="Vector3UI"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector3UI"/>.</param>
        /// <returns>A <see cref="Vector3UI"/> containing the largest components of the source vectors.</returns>
		public static Vector3UI Max(ref Vector3UI left, ref Vector3UI right)
		{
			Max(ref left, ref right, out Vector3UI result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector3UI"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3UI"/>.</param>
        /// <param name="right">The second source <see cref="Vector3UI"/>.</param>
        /// <returns>A <see cref="Vector3UI"/> containing the largest components of the source vectors.</returns>
		public static Vector3UI Max(Vector3UI left, Vector3UI right)
		{
			return new Vector3UI()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3UI"/> vectors.
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
		public static uint DistanceSquared(ref Vector3UI value1, ref Vector3UI value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;
            uint z = value1.Z - value2.Z;

            return ((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Vector3UI"/> vectors.
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
		public static uint DistanceSquared(Vector3UI value1, Vector3UI value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;
            uint z = value1.Z - value2.Z;

            return ((x * x) + (y * y) + (z * z));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3UI"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector3UI Clamp(Vector3UI value, uint min, uint max)
        {
			return new Vector3UI()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3UI"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static void Clamp(ref Vector3UI value, ref Vector3UI min, ref Vector3UI max, out Vector3UI result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
				result.Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3UI"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector3UI Clamp(Vector3UI value, Vector3UI min, Vector3UI max)
        {
			return new Vector3UI()
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
        public static Vector3UI Reflect(ref Vector3UI vector, ref Vector3UI normal)
        {
            uint dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z);

            return new Vector3UI()
            {
				X = (uint)(vector.X - ((2 * dot) * normal.X)),
				Y = (uint)(vector.Y - ((2 * dot) * normal.Y)),
				Z = (uint)(vector.Z - ((2 * dot) * normal.Z)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (uint x, uint y, uint z)(Vector3UI val)
        {
            return (val.X, val.Y, val.Z);
        }

        public static implicit operator Vector3UI((uint x, uint y, uint z) val)
        {
            return new Vector3UI(val.x, val.y, val.z);
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
		public uint this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3UI run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3UI run from 0 to 2, inclusive.");
			}
		}
#endregion

#region Casts - vectors
        ///<summary>Casts a <see cref="Vector3UI"/> to a <see cref="SByte3"/>.</summary>
        public static explicit operator SByte3(Vector3UI val)
        {
            return new SByte3()
            {
                X = (sbyte)val.X,
                Y = (sbyte)val.Y,
                Z = (sbyte)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3UI"/> to a <see cref="Byte3"/>.</summary>
        public static explicit operator Byte3(Vector3UI val)
        {
            return new Byte3()
            {
                X = (byte)val.X,
                Y = (byte)val.Y,
                Z = (byte)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3UI"/> to a <see cref="Vector3I"/>.</summary>
        public static explicit operator Vector3I(Vector3UI val)
        {
            return new Vector3I()
            {
                X = (int)val.X,
                Y = (int)val.Y,
                Z = (int)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3UI"/> to a <see cref="Vector3S"/>.</summary>
        public static explicit operator Vector3S(Vector3UI val)
        {
            return new Vector3S()
            {
                X = (short)val.X,
                Y = (short)val.Y,
                Z = (short)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3UI"/> to a <see cref="Vector3US"/>.</summary>
        public static explicit operator Vector3US(Vector3UI val)
        {
            return new Vector3US()
            {
                X = (ushort)val.X,
                Y = (ushort)val.Y,
                Z = (ushort)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3UI"/> to a <see cref="Vector3L"/>.</summary>
        public static explicit operator Vector3L(Vector3UI val)
        {
            return new Vector3L()
            {
                X = (long)val.X,
                Y = (long)val.Y,
                Z = (long)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3UI"/> to a <see cref="Vector3UL"/>.</summary>
        public static explicit operator Vector3UL(Vector3UI val)
        {
            return new Vector3UL()
            {
                X = val.X,
                Y = val.Y,
                Z = val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3UI"/> to a <see cref="Vector3F"/>.</summary>
        public static explicit operator Vector3F(Vector3UI val)
        {
            return new Vector3F()
            {
                X = (float)val.X,
                Y = (float)val.Y,
                Z = (float)val.Z,
            };
        }

        ///<summary>Casts a <see cref="Vector3UI"/> to a <see cref="Vector3D"/>.</summary>
        public static explicit operator Vector3D(Vector3UI val)
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

