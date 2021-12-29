using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector2I : IFormattable
	{
		///<summary>The X component.</summary>
		public int X;

		///<summary>The Y component.</summary>
		public int Y;


		///<summary>The size of <see cref="Vector2I"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2I));

		public static Vector2I One = new Vector2I(1, 1);

		/// <summary>
        /// The X unit <see cref="Vector2I"/>.
        /// </summary>
		public static Vector2I UnitX = new Vector2I(1, 0);

		/// <summary>
        /// The Y unit <see cref="Vector2I"/>.
        /// </summary>
		public static Vector2I UnitY = new Vector2I(0, 1);

		public static Vector2I Zero = new Vector2I(0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector2I"/>.</summary>
		public Vector2I(int x, int y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector2I"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector2I(int[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Vector2I.");

			X = values[0];
			Y = values[1];
        }

		public unsafe Vector2I(int* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2I"/> vectors.
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
		public static void DistanceSquared(ref Vector2I value1, ref Vector2I value2, out int result)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2I"/> vectors.
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
		public static int DistanceSquared(ref Vector2I value1, ref Vector2I value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector2I"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public int[] ToArray()
        {
            return new int[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector2I"/>.
        /// </summary>
        /// <returns>A <see cref="Vector2I"/> facing the opposite direction.</returns>
		public Vector2I Negate()
		{
			return new Vector2I(-X, -Y);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector2I Lerp(ref Vector2I start, ref Vector2I end, float amount)
        {
			return new Vector2I()
			{
				X = (int)((1f - amount) * start.X + amount * end.X),
				Y = (int)((1f - amount) * start.Y + amount * end.Y),
			};
        }

		/// <summary>
        /// Returns a <see cref="Vector2I"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2I"/>.</param>
        /// <param name="right">The second source <see cref="Vector2I"/>.</param>
        /// <returns>A <see cref="Vector2I"/> containing the smallest components of the source vectors.</returns>
		public static Vector2I Min(Vector2I left, Vector2I right)
		{
			return new Vector2I()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>
        /// Returns a <see cref="Vector2I"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2I"/>.</param>
        /// <param name="right">The second source <see cref="Vector2I"/>.</param>
        /// <returns>A <see cref="Vector2I"/> containing the largest components of the source vectors.</returns>
		public static Vector2I Max(Vector2I left, Vector2I right)
		{
			return new Vector2I()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(int min, int max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector2I min, Vector2I max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector2I"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector2I"/> source vector</param>
        /// <param name="right">Second <see cref="Vector2I"/> source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the two <see cref="Vector2I"/> vectors.</param>
        public static int Dot(Vector2I left, Vector2I right)
        {
			return (left.X * right.X) + (left.Y * right.Y);
        }

		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector2I"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector2I"/> vector.</param>
        /// <param name="value2">Second source position <see cref="Vector2I"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector2I"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Vector2I Hermite(ref Vector2I value1, ref Vector2I tangent1, ref Vector2I value2, ref Vector2I tangent2, int amount)
        {
            float squared = amount * amount;
            float cubed = amount * squared;
            float part1 = ((2.0F * cubed) - (3.0F * squared)) + 1.0F;
            float part2 = (-2.0F * cubed) + (3.0F * squared);
            float part3 = (cubed - (2.0F * squared)) + amount;
            float part4 = cubed - squared;

			return new Vector2I()
			{
				X = (int)((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4)),
				Y = (int)((((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4)),
			};
        }

		/// <summary>
        /// Returns a <see cref="Vector2I"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector2I"/> containing the 2D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector2I"/> containing the 2D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector2I"/> containing the 2D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector2I Barycentric(ref Vector2I value1, ref Vector2I value2, ref Vector2I value3, int amount1, int amount2)
        {
			return new Vector2I(
				(value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X)), 
				(value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))
			);
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2I"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2I"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2I"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2I"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2I"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Vector2I operator +(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2I operator +(Vector2I left, int right)
		{
			return new Vector2I(left.X + right, left.Y + right);
		}

		/// <summary>
        /// Assert a <see cref="Vector2I"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector2I"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector2I"/>.</returns>
        public static Vector2I operator +(Vector2I value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Vector2I operator -(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2I operator -(Vector2I left, int right)
		{
			return new Vector2I(left.X - right, left.Y - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector2I"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector2I"/>.</returns>
        public static Vector2I operator -(Vector2I value)
        {
            return new Vector2I(-value.X, -value.Y);
        }
#endregion

#region division operators
		public static Vector2I operator /(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2I operator /(Vector2I left, int right)
		{
			return new Vector2I(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2I operator *(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2I operator *(Vector2I left, int right)
		{
			return new Vector2I(left.X * right, left.Y * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2I"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector2I Clamp(Vector2I value, int min, int max)
        {
			return new Vector2I()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2I"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector2I Clamp(Vector2I value, Vector2I min, Vector2I max)
        {
			return new Vector2I()
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
        
		public int this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2I run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2I run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

