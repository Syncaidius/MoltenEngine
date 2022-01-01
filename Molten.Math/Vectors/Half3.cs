using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half3 : IFormattable
	{
		///<summary>The X component.</summary>
		public short X;

		///<summary>The Y component.</summary>
		public short Y;

		///<summary>The Z component.</summary>
		public short Z;


		///<summary>The size of <see cref="Half3"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half3));

		///<summary>A Half3 with every component set to (short)1.</summary>
		public static readonly Half3 One = new Half3((short)1, (short)1, (short)1);

		/// <summary>The X unit <see cref="Half3"/>.</summary>
		public static readonly Half3 UnitX = new Half3((short)1, 0, 0);

		/// <summary>The Y unit <see cref="Half3"/>.</summary>
		public static readonly Half3 UnitY = new Half3(0, (short)1, 0);

		/// <summary>The Z unit <see cref="Half3"/>.</summary>
		public static readonly Half3 UnitZ = new Half3(0, 0, (short)1);

		/// <summary>Represents a zero'd Half3.</summary>
		public static readonly Half3 Zero = new Half3(0, 0, 0);

		 /// <summary>
        /// Gets a value indicting whether this instance is normalized.
        /// </summary>
        public bool IsNormalized
        {
            get => MathHelper.IsOne((X * X) + (Y * Y) + (Z * Z));
        }

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0 && Y == 0 && Z == 0;
        }

#region Constructors
		///<summary>Creates a new instance of <see cref = "Half3"/>.</summary>
		public Half3(short x, short y, short z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Half3"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Half3(short[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for Half3.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="Half3"/> struct from an unsafe pointer. The pointer should point to an array of three elements.
        /// </summary>
		public unsafe Half3(short* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
		}
#endregion

#region Instance Functions
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
        public short Length()
        {
            return (short)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
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
            return (X * X) + (Y * Y) + (Z * Z);
        }

        /// <summary>
        /// Converts the vector into a unit vector.
        /// </summary>
        public void Normalize()
        {
            short length = Length();
            if (!MathHelper.IsZero(length))
            {
                short inverse = 1.0f / length;
			    X *= inverse;
			    Y *= inverse;
			    Z *= inverse;
            }
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Half3"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public short[] ToArray()
        {
            return new short[] { X, Y, Z};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Half3"/>.
        /// </summary>
        /// <returns>A <see cref="Half3"/> facing the opposite direction.</returns>
		public Half3 Negate()
		{
			return new Half3(-X, -Y, -Z);
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
        public void Clamp(Half3 min, Half3 max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Half3 operator +(Half3 left, Half3 right)
		{
			return new Half3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Half3 operator +(Half3 left, short right)
		{
			return new Half3(left.X + right, left.Y + right, left.Z + right);
		}

		/// <summary>
        /// Assert a <see cref="Half3"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Half3"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Half3"/>.</returns>
        public static Half3 operator +(Half3 value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Half3 operator -(Half3 left, Half3 right)
		{
			return new Half3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Half3 operator -(Half3 left, short right)
		{
			return new Half3(left.X - right, left.Y - right, left.Z - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Half3"/>.
        /// </summary>
        /// <param name="value">The <see cref="Half3"/> to reverse.</param>
        /// <returns>The reversed <see cref="Half3"/>.</returns>
        public static Half3 operator -(Half3 value)
        {
            return new Half3(-value.X, -value.Y, -value.Z);
        }
#endregion

#region division operators
		public static Half3 operator /(Half3 left, Half3 right)
		{
			return new Half3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Half3 operator /(Half3 left, short right)
		{
			return new Half3(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Half3 operator *(Half3 left, Half3 right)
		{
			return new Half3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Half3 operator *(Half3 left, short right)
		{
			return new Half3(left.X * right, left.Y * right, left.Z * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
        /// <summary>Checks to see if any value (x, y, z, w) are within 0.0001 of 0.
        /// If so this method truncates that value to zero.</summary>
        /// <param name="power">The power.</param>
        /// <param name="vec">The vector.</param>
        public static Half3 Pow(Half3 vec, short power)
        {
            return new Half3()
            {
                X = (short)Math.Pow(vec.X, power),
                Y = (short)Math.Pow(vec.Y, power),
            };
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Half3"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Half3"/> source vector</param>
        /// <param name="right">Second <see cref="Half3"/> source vector.</param>
        public static short Dot(Half3 left, Half3 right)
        {
			return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);
        }

		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Half3"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="Half3"/> vector.</param>
        /// <param name="value2">Second source position <see cref="Half3"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="Half3"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Half3 Hermite(ref Half3 value1, ref Half3 tangent1, ref Half3 value2, ref Half3 tangent2, short amount)
        {
            float squared = amount * amount;
            float cubed = amount * squared;
            float part1 = ((2.0F * cubed) - (3.0F * squared)) + 1.0F;
            float part2 = (-2.0F * cubed) + (3.0F * squared);
            float part3 = (cubed - (2.0F * squared)) + amount;
            float part4 = cubed - squared;

			return new Half3()
			{
				X = (short)((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4)),
				Y = (short)((((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4)),
				Z = (short)((((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4)),
			};
        }

		/// <summary>
        /// Returns a <see cref="Half3"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Half3"/> containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Half3"/> containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Half3"/> containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Half3 Barycentric(ref Half3 value1, ref Half3 value2, ref Half3 value3, short amount1, short amount2)
        {
			return new Half3(
				(value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X)), 
				(value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y)), 
				(value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z))
			);
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Half3"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Half3 Lerp(ref Half3 start, ref Half3 end, float amount)
        {
			return new Half3()
			{
				X = (short)((1F - amount) * start.X + amount * end.X),
				Y = (short)((1F - amount) * start.Y + amount * end.Y),
				Z = (short)((1F - amount) * start.Z + amount * end.Z),
			};
        }

		/// <summary>
        /// Returns a <see cref="Half3"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Half3"/>.</param>
        /// <param name="right">The second source <see cref="Half3"/>.</param>
        /// <returns>A <see cref="Half3"/> containing the smallest components of the source vectors.</returns>
		public static Half3 Min(Half3 left, Half3 right)
		{
			return new Half3()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Returns a <see cref="Half3"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Half3"/>.</param>
        /// <param name="right">The second source <see cref="Half3"/>.</param>
        /// <returns>A <see cref="Half3"/> containing the largest components of the source vectors.</returns>
		public static Half3 Max(Half3 left, Half3 right)
		{
			return new Half3()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Half3"/> vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector</param>
        /// <param name="result">When the method completes, contains the squared distance between the two vectors.</param>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
		public static void DistanceSquared(ref Half3 value1, ref Half3 value2, out short result)
        {
            short x = value1.X - value2.X;
            short y = value1.Y - value2.Y;
            short z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Half3"/> vectors.
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
		public static short DistanceSquared(ref Half3 value1, ref Half3 value2)
        {
            short x = value1.X - value2.X;
            short y = value1.Y - value2.Y;
            short z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Half3"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Half3 Clamp(Half3 value, short min, short max)
        {
			return new Half3()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Half3"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Half3 Clamp(Half3 value, Half3 min, Half3 max)
        {
			return new Half3()
			{
				X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
				Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y,
				Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z,
			};
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
        
		public short this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half3 run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half3 run from 0 to 2, inclusive.");
			}
		}
#endregion
	}
}

