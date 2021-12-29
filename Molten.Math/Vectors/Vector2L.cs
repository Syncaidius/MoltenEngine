using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "long"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector2L : IFormattable
	{
		///<summary>The X component.</summary>
		public long X;

		///<summary>The Y component.</summary>
		public long Y;


		///<summary>The size of <see cref="Vector2L"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2L));

		public static Vector2L One = new Vector2L(1L, 1L);

		/// <summary>
        /// The X unit <see cref="Vector2L"/>.
        /// </summary>
		public static Vector2L UnitX = new Vector2L(1L, 0);

		/// <summary>
        /// The Y unit <see cref="Vector2L"/>.
        /// </summary>
		public static Vector2L UnitY = new Vector2L(0, 1L);

		public static Vector2L Zero = new Vector2L(0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector2L"/>.</summary>
		public Vector2L(long x, long y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector2L"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector2L(long[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Vector2L.");

			X = values[0];
			Y = values[1];
        }

		public unsafe Vector2L(long* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2L"/> vectors.
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
		public static void DistanceSquared(ref Vector2L value1, ref Vector2L value2, out long result)
        {
            long x = value1.X - value2.X;
            long y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2L"/> vectors.
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
		public static long DistanceSquared(ref Vector2L value1, ref Vector2L value2)
        {
            long x = value1.X - value2.X;
            long y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector2L"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public long[] ToArray()
        {
            return new long[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector2L"/>.
        /// </summary>
        /// <returns>A <see cref="Vector2L"/> facing the opposite direction.</returns>
		public Vector2L Negate()
		{
			return new Vector2L(-X, -Y);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2L"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector2L Lerp(ref Vector2L start, ref Vector2L end, float amount)
        {
			return new Vector2L()
			{
				X = (long)((1f - amount) * start.X + amount * end.X),
				Y = (long)((1f - amount) * start.Y + amount * end.Y),
			};
        }

		/// <summary>
        /// Returns a <see cref="Vector2L"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2L"/>.</param>
        /// <param name="right">The second source <see cref="Vector2L"/>.</param>
        /// <returns>A <see cref="Vector2L"/> containing the smallest components of the source vectors.</returns>
		public static Vector2L Min(Vector2L left, Vector2L right)
		{
			return new Vector2L()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>
        /// Returns a <see cref="Vector2L"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2L"/>.</param>
        /// <param name="right">The second source <see cref="Vector2L"/>.</param>
        /// <returns>A <see cref="Vector2L"/> containing the largest components of the source vectors.</returns>
		public static Vector2L Max(Vector2L left, Vector2L right)
		{
			return new Vector2L()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(long min, long max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector2L min, Vector2L max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector2L"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector2L"/> source vector</param>
        /// <param name="right">Second <see cref="Vector2L"/> source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the two <see cref="Vector2L"/> vectors.</param>
        public static long Dot(Vector2L left, Vector2L right)
        {
			return (left.X * right.X) + (left.Y * right.Y);
        }

		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector2L"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector2L"/> vector.</param>
        /// <param name="value2">Second source position <see cref="Vector2L"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector2L"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Vector2L Hermite(ref Vector2L value1, ref Vector2L tangent1, ref Vector2L value2, ref Vector2L tangent2, long amount)
        {
            double squared = amount * amount;
            double cubed = amount * squared;
            double part1 = ((2.0D * cubed) - (3.0D * squared)) + 1.0D;
            double part2 = (-2.0D * cubed) + (3.0D * squared);
            double part3 = (cubed - (2.0D * squared)) + amount;
            double part4 = cubed - squared;

			return new Vector2L()
			{
				X = (long)((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4)),
				Y = (long)((((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4)),
			};
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2L"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2L"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2L"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2L"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2L"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2L"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector2L"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector2L"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Vector2L operator +(Vector2L left, Vector2L right)
		{
			return new Vector2L(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2L operator +(Vector2L left, long right)
		{
			return new Vector2L(left.X + right, left.Y + right);
		}

		/// <summary>
        /// Assert a <see cref="Vector2L"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector2L"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector2L"/>.</returns>
        public static Vector2L operator +(Vector2L value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Vector2L operator -(Vector2L left, Vector2L right)
		{
			return new Vector2L(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2L operator -(Vector2L left, long right)
		{
			return new Vector2L(left.X - right, left.Y - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector2L"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector2L"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector2L"/>.</returns>
        public static Vector2L operator -(Vector2L value)
        {
            return new Vector2L(-value.X, -value.Y);
        }
#endregion

#region division operators
		public static Vector2L operator /(Vector2L left, Vector2L right)
		{
			return new Vector2L(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2L operator /(Vector2L left, long right)
		{
			return new Vector2L(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2L operator *(Vector2L left, Vector2L right)
		{
			return new Vector2L(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2L operator *(Vector2L left, long right)
		{
			return new Vector2L(left.X * right, left.Y * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2L"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector2L Clamp(Vector2L value, long min, long max)
        {
			return new Vector2L()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2L"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector2L Clamp(Vector2L value, Vector2L min, Vector2L max)
        {
			return new Vector2L()
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
        
		public long this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2L run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2L run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

