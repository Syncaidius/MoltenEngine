using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "nuint"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=sizeof(nuint))]
	public partial struct Vector2NU : IFormattable
	{
		///<summary>The X component.</summary>
		public nuint X;

		///<summary>The Y component.</summary>
		public nuint Y;


		///<summary>The size of <see cref="Vector2NU"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2NU));

		public static Vector2NU One = new Vector2NU(1U, 1U);

		/// <summary>
        /// The X unit <see cref="Vector2NU"/>.
        /// </summary>
		public static Vector2NU UnitX = new Vector2NU(1U, 0);

		/// <summary>
        /// The Y unit <see cref="Vector2NU"/>.
        /// </summary>
		public static Vector2NU UnitY = new Vector2NU(0, 1U);

		public static Vector2NU Zero = new Vector2NU(0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector2NU"/>.</summary>
		public Vector2NU(nuint x, nuint y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector2NU"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector2NU(nuint[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Vector2NU.");

			X = values[0];
			Y = values[1];
        }

		public unsafe Vector2NU(nuint* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2NU"/> vectors.
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
		public static void DistanceSquared(ref Vector2NU value1, ref Vector2NU value2, out nuint result)
        {
            nuint x = value1.X - value2.X;
            nuint y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2NU"/> vectors.
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
		public static nuint DistanceSquared(ref Vector2NU value1, ref Vector2NU value2)
        {
            nuint x = value1.X - value2.X;
            nuint y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector2NU"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public nuint[] ToArray()
        {
            return new nuint[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector2NU"/>.
        /// </summary>
        /// <returns>A <see cref="Vector2NU"/> facing the opposite direction.</returns>
		public Vector2NU Negate()
		{
			return new Vector2NU(-X, -Y);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2NU"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector2NU Lerp(ref Vector2NU start, ref Vector2NU end, float amount)
        {
			return new Vector2NU()
			{
				X = (nuint)((1f - amount) * start.X + amount * end.X),
				Y = (nuint)((1f - amount) * start.Y + amount * end.Y),
			};
        }

		/// <summary>
        /// Returns a <see cref="Vector2NU"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2NU"/>.</param>
        /// <param name="right">The second source <see cref="Vector2NU"/>.</param>
        /// <returns>A <see cref="Vector2NU"/> containing the smallest components of the source vectors.</returns>
		public static Vector2NU Min(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>
        /// Returns a <see cref="Vector2NU"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2NU"/>.</param>
        /// <param name="right">The second source <see cref="Vector2NU"/>.</param>
        /// <returns>A <see cref="Vector2NU"/> containing the largest components of the source vectors.</returns>
		public static Vector2NU Max(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(nuint min, nuint max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector2NU min, Vector2NU max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector2NU"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector2NU"/> source vector</param>
        /// <param name="right">Second <see cref="Vector2NU"/> source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the two <see cref="Vector2NU"/> vectors.</param>
        public static nuint Dot(Vector2NU left, Vector2NU right)
        {
			return (left.X * right.X) + (left.Y * right.Y);
        }

		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector2NU"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector2NU"/> vector.</param>
        /// <param name="value2">Second source position <see cref="Vector2NU"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector2NU"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Vector2NU Hermite(ref Vector2NU value1, ref Vector2NU tangent1, ref Vector2NU value2, ref Vector2NU tangent2, nuint amount)
        {
            float squared = amount * amount;
            float cubed = amount * squared;
            float part1 = ((2.0F * cubed) - (3.0F * squared)) + 1.0F;
            float part2 = (-2.0F * cubed) + (3.0F * squared);
            float part3 = (cubed - (2.0F * squared)) + amount;
            float part4 = cubed - squared;

			return new Vector2NU()
			{
				X = (nuint)((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4)),
				Y = (nuint)((((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4)),
			};
        }

		/// <summary>
        /// Returns a <see cref="Vector2NU"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector2NU"/> containing the 2D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector2NU"/> containing the 2D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector2NU"/> containing the 2D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector2NU Barycentric(ref Vector2NU value1, ref Vector2NU value2, ref Vector2NU value3, nuint amount1, nuint amount2)
        {
			return new Vector2NU(
				(value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X)), 
				(value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))
			);
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2NU"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2NU"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2NU"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2NU"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2NU"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2NU"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2NU"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2NU"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Vector2NU operator +(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2NU operator +(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X + right, left.Y + right);
		}

		/// <summary>
        /// Assert a <see cref="Vector2NU"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector2NU"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector2NU"/>.</returns>
        public static Vector2NU operator +(Vector2NU value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Vector2NU operator -(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2NU operator -(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X - right, left.Y - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector2NU"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector2NU"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector2NU"/>.</returns>
        public static Vector2NU operator -(Vector2NU value)
        {
            return new Vector2NU(-value.X, -value.Y);
        }
#endregion

#region division operators
		public static Vector2NU operator /(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2NU operator /(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2NU operator *(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2NU operator *(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X * right, left.Y * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2NU"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector2NU Clamp(Vector2NU value, nuint min, nuint max)
        {
			return new Vector2NU()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2NU"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector2NU Clamp(Vector2NU value, Vector2NU min, Vector2NU max)
        {
			return new Vector2NU()
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
        
		public nuint this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2NU run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2NU run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

