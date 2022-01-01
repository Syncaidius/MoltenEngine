using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half2 : IFormattable
	{
		///<summary>The X component.</summary>
		public short X;

		///<summary>The Y component.</summary>
		public short Y;


		///<summary>The size of <see cref="Half2"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half2));

		///<summary>A Half2 with every component set to (short)1.</summary>
		public static readonly Half2 One = new Half2((short)1, (short)1);

		/// <summary>The X unit <see cref="Half2"/>.</summary>
		public static readonly Half2 UnitX = new Half2((short)1, 0);

		/// <summary>The Y unit <see cref="Half2"/>.</summary>
		public static readonly Half2 UnitY = new Half2(0, (short)1);

		/// <summary>Represents a zero'd Half2.</summary>
		public static readonly Half2 Zero = new Half2(0, 0);

		 /// <summary>
        /// Gets a value indicting whether this instance is normalized.
        /// </summary>
        public bool IsNormalized
        {
            get => MathHelper.IsOne((X * X) + (Y * Y));
        }

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0 && Y == 0;
        }

#region Constructors
		///<summary>Creates a new instance of <see cref = "Half2"/>.</summary>
		public Half2(short x, short y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Half2"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Half2(short[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Half2.");

			X = values[0];
			Y = values[1];
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="Half2"/> struct from an unsafe pointer. The pointer should point to an array of two elements.
        /// </summary>
		public unsafe Half2(short* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
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
            return (short)Math.Sqrt((X * X) + (Y * Y));
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
            return (X * X) + (Y * Y);
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
            }
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Half2"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public short[] ToArray()
        {
            return new short[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Half2"/>.
        /// </summary>
        /// <returns>A <see cref="Half2"/> facing the opposite direction.</returns>
		public Half2 Negate()
		{
			return new Half2(-X, -Y);
		}
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(short min, short max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Half2 min, Half2 max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half2"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half2"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half2"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half2"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half2"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half2"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half2"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half2"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Half2 operator +(Half2 left, Half2 right)
		{
			return new Half2(left.X + right.X, left.Y + right.Y);
		}

		public static Half2 operator +(Half2 left, short right)
		{
			return new Half2(left.X + right, left.Y + right);
		}

		/// <summary>
        /// Assert a <see cref="Half2"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Half2"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Half2"/>.</returns>
        public static Half2 operator +(Half2 value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Half2 operator -(Half2 left, Half2 right)
		{
			return new Half2(left.X - right.X, left.Y - right.Y);
		}

		public static Half2 operator -(Half2 left, short right)
		{
			return new Half2(left.X - right, left.Y - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Half2"/>.
        /// </summary>
        /// <param name="value">The <see cref="Half2"/> to reverse.</param>
        /// <returns>The reversed <see cref="Half2"/>.</returns>
        public static Half2 operator -(Half2 value)
        {
            return new Half2(-value.X, -value.Y);
        }
#endregion

#region division operators
		public static Half2 operator /(Half2 left, Half2 right)
		{
			return new Half2(left.X / right.X, left.Y / right.Y);
		}

		public static Half2 operator /(Half2 left, short right)
		{
			return new Half2(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Half2 operator *(Half2 left, Half2 right)
		{
			return new Half2(left.X * right.X, left.Y * right.Y);
		}

		public static Half2 operator *(Half2 left, short right)
		{
			return new Half2(left.X * right, left.Y * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Half2"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector2F"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
        /// <returns></returns>
        public static unsafe Half2 Swizzle(Half2 val, int xIndex, int yIndex)
        {
            return new Half2()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
            };
        }

        /// <returns></returns>
        public static unsafe Half2 Swizzle(Half2 val, uint xIndex, uint yIndex)
        {
            return new Half2()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
            };
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector2F.DistanceSquared(Vector2F, Vector2F)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static short Distance(Half2 value1, Half2 value2)
        {
			short x = value1.X - value2.X;
			short y = value1.Y - value2.Y;

            return (short)Math.Sqrt((x * x) + (y * y));
        }

        /// <summary>Checks to see if any value (x, y, z, w) are within 0.0001 of 0.
        /// If so this method truncates that value to zero.</summary>
        /// <param name="power">The power.</param>
        /// <param name="vec">The vector.</param>
        public static Half2 Pow(Half2 vec, short power)
        {
            return new Half2()
            {
				X = (short)Math.Pow(vec.X, power),
				Y = (short)Math.Pow(vec.Y, power),
            };
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Half2"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Half2"/> source vector</param>
        /// <param name="right">Second <see cref="Half2"/> source vector.</param>
        public static short Dot(Half2 left, Half2 right)
        {
			return (left.X * right.X) + (left.Y * right.Y);
        }

		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Half2"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="Half2"/> vector.</param>
        /// <param name="value2">Second source position <see cref="Half2"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="Half2"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Half2 Hermite(ref Half2 value1, ref Half2 tangent1, ref Half2 value2, ref Half2 tangent2, short amount)
        {
            float squared = amount * amount;
            float cubed = amount * squared;
            float part1 = ((2.0F * cubed) - (3.0F * squared)) + 1.0F;
            float part2 = (-2.0F * cubed) + (3.0F * squared);
            float part3 = (cubed - (2.0F * squared)) + amount;
            float part4 = cubed - squared;

			return new Half2()
			{
				X = (short)((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4)),
				Y = (short)((((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4)),
			};
        }

		/// <summary>
        /// Returns a <see cref="Half2"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Half2"/> containing the 2D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Half2"/> containing the 2D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Half2"/> containing the 2D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Half2 Barycentric(ref Half2 value1, ref Half2 value2, ref Half2 value3, short amount1, short amount2)
        {
			return new Half2(
				(value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X)), 
				(value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))
			);
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Half2"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Half2 Lerp(ref Half2 start, ref Half2 end, float amount)
        {
			return new Half2()
			{
				X = (short)((1F - amount) * start.X + amount * end.X),
				Y = (short)((1F - amount) * start.Y + amount * end.Y),
			};
        }

		/// <summary>
        /// Returns a <see cref="Half2"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Half2"/>.</param>
        /// <param name="right">The second source <see cref="Half2"/>.</param>
        /// <returns>A <see cref="Half2"/> containing the smallest components of the source vectors.</returns>
		public static Half2 Min(Half2 left, Half2 right)
		{
			return new Half2()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>
        /// Returns a <see cref="Half2"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Half2"/>.</param>
        /// <param name="right">The second source <see cref="Half2"/>.</param>
        /// <returns>A <see cref="Half2"/> containing the largest components of the source vectors.</returns>
		public static Half2 Max(Half2 left, Half2 right)
		{
			return new Half2()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Half2"/> vectors.
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
		public static void DistanceSquared(ref Half2 value1, ref Half2 value2, out short result)
        {
            short x = value1.X - value2.X;
            short y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Half2"/> vectors.
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
		public static short DistanceSquared(ref Half2 value1, ref Half2 value2)
        {
            short x = value1.X - value2.X;
            short y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Half2"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Half2 Clamp(Half2 value, short min, short max)
        {
			return new Half2()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Half2"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Half2 Clamp(Half2 value, Half2 min, Half2 max)
        {
			return new Half2()
			{
				X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
				Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y,
			};
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X or Y component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 1].</exception>
        
		public short this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half2 run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half2 run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

