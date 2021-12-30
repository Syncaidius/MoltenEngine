




using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "sbyte"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public partial struct SByte2 : IFormattable
	{
		///<summary>The X component.</summary>
		public sbyte X;

		///<summary>The Y component.</summary>
		public sbyte Y;


		///<summary>The size of <see cref="SByte2"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(SByte2));

		public static SByte2 One = new SByte2(1, 1);

		/// <summary>The X unit <see cref="SByte2"/>.</summary>
		public static SByte2 UnitX = new SByte2(1, 0);

		/// <summary>The Y unit <see cref="SByte2"/>.</summary>
		public static SByte2 UnitY = new SByte2(0, 1);

		/// <summary>Represents a zero'd SByte2.</summary>
		public static SByte2 Zero = new SByte2(0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "SByte2"/>.</summary>
		public SByte2(sbyte x, sbyte y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="SByte2"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public SByte2(sbyte[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for SByte2.");

			X = values[0];
			Y = values[1];
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="SByte2"/> struct from an unsafe pointer. The pointer should point to an array of two elements.
        /// </summary>
		public unsafe SByte2(sbyte* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="SByte2"/> vectors.
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
		public static void DistanceSquared(ref SByte2 value1, ref SByte2 value2, out sbyte result)
        {
            sbyte x = value1.X - value2.X;
            sbyte y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="SByte2"/> vectors.
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
		public static sbyte DistanceSquared(ref SByte2 value1, ref SByte2 value2)
        {
            sbyte x = value1.X - value2.X;
            sbyte y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="SByte2"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public sbyte[] ToArray()
        {
            return new sbyte[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="SByte2"/>.
        /// </summary>
        /// <returns>A <see cref="SByte2"/> facing the opposite direction.</returns>
		public SByte2 Negate()
		{
			return new SByte2(-X, -Y);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="SByte2"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static SByte2 Lerp(ref SByte2 start, ref SByte2 end, float amount)
        {
			return new SByte2()
			{
				X = (sbyte)((1F - amount) * start.X + amount * end.X),
				Y = (sbyte)((1F - amount) * start.Y + amount * end.Y),
			};
        }

		/// <summary>
        /// Returns a <see cref="SByte2"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="SByte2"/>.</param>
        /// <param name="right">The second source <see cref="SByte2"/>.</param>
        /// <returns>A <see cref="SByte2"/> containing the smallest components of the source vectors.</returns>
		public static SByte2 Min(SByte2 left, SByte2 right)
		{
			return new SByte2()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>
        /// Returns a <see cref="SByte2"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="SByte2"/>.</param>
        /// <param name="right">The second source <see cref="SByte2"/>.</param>
        /// <returns>A <see cref="SByte2"/> containing the largest components of the source vectors.</returns>
		public static SByte2 Max(SByte2 left, SByte2 right)
		{
			return new SByte2()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(sbyte min, sbyte max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(SByte2 min, SByte2 max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="SByte2"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="SByte2"/> source vector</param>
        /// <param name="right">Second <see cref="SByte2"/> source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the two <see cref="SByte2"/> vectors.</param>
        public static sbyte Dot(SByte2 left, SByte2 right)
        {
			return (left.X * right.X) + (left.Y * right.Y);
        }

		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="SByte2"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="SByte2"/> vector.</param>
        /// <param name="value2">Second source position <see cref="SByte2"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="SByte2"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static SByte2 Hermite(ref SByte2 value1, ref SByte2 tangent1, ref SByte2 value2, ref SByte2 tangent2, sbyte amount)
        {
            float squared = amount * amount;
            float cubed = amount * squared;
            float part1 = ((2.0F * cubed) - (3.0F * squared)) + 1.0F;
            float part2 = (-2.0F * cubed) + (3.0F * squared);
            float part3 = (cubed - (2.0F * squared)) + amount;
            float part4 = cubed - squared;

			return new SByte2()
			{
				X = (sbyte)((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4)),
				Y = (sbyte)((((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4)),
			};
        }

		/// <summary>
        /// Returns a <see cref="SByte2"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="SByte2"/> containing the 2D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="SByte2"/> containing the 2D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="SByte2"/> containing the 2D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static SByte2 Barycentric(ref SByte2 value1, ref SByte2 value2, ref SByte2 value3, sbyte amount1, sbyte amount2)
        {
			return new SByte2(
				(value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X)), 
				(value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))
			);
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="SByte2"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="SByte2"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="SByte2"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="SByte2"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="SByte2"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="SByte2"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="SByte2"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="SByte2"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static SByte2 operator +(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X + right.X, left.Y + right.Y);
		}

		public static SByte2 operator +(SByte2 left, sbyte right)
		{
			return new SByte2(left.X + right, left.Y + right);
		}

		/// <summary>
        /// Assert a <see cref="SByte2"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="SByte2"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="SByte2"/>.</returns>
        public static SByte2 operator +(SByte2 value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static SByte2 operator -(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X - right.X, left.Y - right.Y);
		}

		public static SByte2 operator -(SByte2 left, sbyte right)
		{
			return new SByte2(left.X - right, left.Y - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="SByte2"/>.
        /// </summary>
        /// <param name="value">The <see cref="SByte2"/> to reverse.</param>
        /// <returns>The reversed <see cref="SByte2"/>.</returns>
        public static SByte2 operator -(SByte2 value)
        {
            return new SByte2(-value.X, -value.Y);
        }
#endregion

#region division operators
		public static SByte2 operator /(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X / right.X, left.Y / right.Y);
		}

		public static SByte2 operator /(SByte2 left, sbyte right)
		{
			return new SByte2(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static SByte2 operator *(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X * right.X, left.Y * right.Y);
		}

		public static SByte2 operator *(SByte2 left, sbyte right)
		{
			return new SByte2(left.X * right, left.Y * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="SByte2"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static SByte2 Clamp(SByte2 value, sbyte min, sbyte max)
        {
			return new SByte2()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="SByte2"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static SByte2 Clamp(SByte2 value, SByte2 min, SByte2 max)
        {
			return new SByte2()
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
        
		public sbyte this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for SByte2 run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for SByte2 run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

