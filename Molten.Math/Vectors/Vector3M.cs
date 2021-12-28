using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=16)]
	public partial struct Vector3M
	{
		///<summary>The X component.</summary>
		public decimal X;

		///<summary>The Y component.</summary>
		public decimal Y;

		///<summary>The Z component.</summary>
		public decimal Z;


		///<summary>The size of <see cref="Vector3M"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3M));

		public static Vector3M One = new Vector3M(1M, 1M, 1M);

		/// <summary>
        /// The X unit <see cref="Vector3M"/>.
        /// </summary>
		public static Vector3M UnitX = new Vector3M(1M, 0M, 0M);

		/// <summary>
        /// The Y unit <see cref="Vector3M"/>.
        /// </summary>
		public static Vector3M UnitY = new Vector3M(0M, 1M, 0M);

		/// <summary>
        /// The Z unit <see cref="Vector3M"/>.
        /// </summary>
		public static Vector3M UnitZ = new Vector3M(0M, 0M, 1M);

		public static Vector3M Zero = new Vector3M(0M, 0M, 0M);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector3M"/>.</summary>
		public Vector3M(decimal x, decimal y, decimal z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3M"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector3M(decimal[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for Vector3M.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }

		public unsafe Vector3M(decimal* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3M"/> vectors.
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
		public static void DistanceSquared(ref Vector3M value1, ref Vector3M value2, out decimal result)
        {
            decimal x = value1.X - value2.X;
            decimal y = value1.Y - value2.Y;
            decimal z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3M"/> vectors.
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
		public static decimal DistanceSquared(ref Vector3M value1, ref Vector3M value2)
        {
            decimal x = value1.X - value2.X;
            decimal y = value1.Y - value2.Y;
            decimal z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector3M"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public decimal[] ToArray()
        {
            return new decimal[] { X, Y, Z};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector3M"/>.
        /// </summary>
        /// <returns>A <see cref="Vector3M"/> facing the opposite direction.</returns>
		public Vector3M Negate()
		{
			return new Vector3M(-X, -Y, -Z);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3M"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector3M Lerp(ref Vector3M start, ref Vector3M end, float amount)
        {
			return new Vector3M()
			{
				X = (decimal)((1f - amount) * start.X + amount * end.X),
				Y = (decimal)((1f - amount) * start.Y + amount * end.Y),
				Z = (decimal)((1f - amount) * start.Z + amount * end.Z),
			};
        }

		/// <summary>
        /// Returns a <see cref="Vector3M"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3M"/>.</param>
        /// <param name="right">The second source <see cref="Vector3M"/>.</param>
        /// <returns>A <see cref="Vector3M"/> containing the smallest components of the source vectors.</returns>
		public static Vector3M Min(Vector3M left, Vector3M right)
		{
			return new Vector3M()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Returns a <see cref="Vector3M"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector3M"/>.</param>
        /// <param name="right">The second source <see cref="Vector3M"/>.</param>
        /// <returns>A <see cref="Vector3M"/> containing the largest components of the source vectors.</returns>
		public static Vector3M Max(Vector3M left, Vector3M right)
		{
			return new Vector3M()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(decimal min, decimal max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector3M min, Vector3M max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3M"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3M"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3M"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3M"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3M"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3M"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector3M"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector3M"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Vector3M operator +(Vector3M left, Vector3M right)
		{
			return new Vector3M(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3M operator +(Vector3M left, decimal right)
		{
			return new Vector3M(left.X + right, left.Y + right, left.Z + right);
		}

		/// <summary>
        /// Assert a <see cref="Vector3M"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector3M"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector3M"/>.</returns>
        public static Vector3M operator +(Vector3M value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Vector3M operator -(Vector3M left, Vector3M right)
		{
			return new Vector3M(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3M operator -(Vector3M left, decimal right)
		{
			return new Vector3M(left.X - right, left.Y - right, left.Z - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector3M"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3M"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector3M"/>.</returns>
        public static Vector3M operator -(Vector3M value)
        {
            return new Vector3M(-value.X, -value.Y, -value.Z);
        }
#endregion

#region division operators
		public static Vector3M operator /(Vector3M left, Vector3M right)
		{
			return new Vector3M(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3M operator /(Vector3M left, decimal right)
		{
			return new Vector3M(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3M operator *(Vector3M left, Vector3M right)
		{
			return new Vector3M(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3M operator *(Vector3M left, decimal right)
		{
			return new Vector3M(left.X * right, left.Y * right, left.Z * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3M"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector3M Clamp(Vector3M value, decimal min, decimal max)
        {
			return new Vector3M()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector3M"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector3M Clamp(Vector3M value, Vector3M min, Vector3M max)
        {
			return new Vector3M()
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
        
		public decimal this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3M run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3M run from 0 to 2, inclusive.");
			}
		}
#endregion
	}
}

