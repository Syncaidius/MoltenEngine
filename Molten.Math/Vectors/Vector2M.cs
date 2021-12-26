using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=16)]
	public partial struct Vector2M
	{
		///<summary>The X component.</summary>
		public decimal X;

		///<summary>The Y component.</summary>
		public decimal Y;


		///<summary>The size of <see cref="Vector2M"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2M));

		public static Vector2M One = new Vector2M(1M, 1M);

		/// <summary>
        /// The X unit <see cref="Vector2M"/>.
        /// </summary>
		public static Vector2M UnitX = new Vector2M(1M, 0M);

		/// <summary>
        /// The Y unit <see cref="Vector2M"/>.
        /// </summary>
		public static Vector2M UnitY = new Vector2M(0M, 1M);

		public static Vector2M Zero = new Vector2M(0M, 0M);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector2M"/>.</summary>
		public Vector2M(decimal x, decimal y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector2M"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector2M(decimal[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Vector2M.");

			X = values[0];
			Y = values[1];
        }
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2M"/> vectors.
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
		public static void DistanceSquared(ref Vector2M value1, ref Vector2M value2, out decimal result)
        {
            decimal x = value1.X - value2.X;
            decimal y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2M"/> vectors.
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
		public static decimal DistanceSquared(ref Vector2M value1, ref Vector2M value2)
        {
            decimal x = value1.X - value2.X;
            decimal y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector2M"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public decimal[] ToArray()
        {
            return new decimal[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector2M"/>.
        /// </summary>
        /// <returns>A <see cref="Vector2M"/> facing the opposite direction.</returns>
		public Vector2M Negate()
		{
			return new Vector2M(-X, -Y);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2M"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector2M Lerp(ref Vector2M start, ref Vector2M end, float amount)
        {
			return new Vector2M()
			{
				X = (decimal)((1f - amount) * start.X + amount * end.X),
				Y = (decimal)((1f - amount) * start.Y + amount * end.Y),
			};
        }
#endregion

#region Add operators
		public static Vector2M operator +(Vector2M left, Vector2M right)
		{
			return new Vector2M(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2M operator +(Vector2M left, decimal right)
		{
			return new Vector2M(left.X + right, left.Y + right);
		}

		/// <summary>
        /// Assert a <see cref="Vector2M"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector2M"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector2M"/>.</returns>
        public static Vector2M operator +(Vector2M value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Vector2M operator -(Vector2M left, Vector2M right)
		{
			return new Vector2M(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2M operator -(Vector2M left, decimal right)
		{
			return new Vector2M(left.X - right, left.Y - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector2M"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector2M"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector2M"/>.</returns>
        public static Vector2M operator -(Vector2M value)
        {
            return new Vector2M(-value.X, -value.Y);
        }
#endregion

#region division operators
		public static Vector2M operator /(Vector2M left, Vector2M right)
		{
			return new Vector2M(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2M operator /(Vector2M left, decimal right)
		{
			return new Vector2M(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2M operator *(Vector2M left, Vector2M right)
		{
			return new Vector2M(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2M operator *(Vector2M left, decimal right)
		{
			return new Vector2M(left.X * right, left.Y * right);
		}
#endregion

#region Properties

#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X or Y component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 1].</exception>
        
		public decimal this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2M run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2M run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

