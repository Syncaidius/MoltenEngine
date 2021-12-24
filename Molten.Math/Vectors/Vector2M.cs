using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of 2 components.</summary>
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

		public static Vector2M Zero = new Vector2M(0M, 0M);

		///<summary>Creates a new instance of <see cref = "Vector2M"/></summary>
		public Vector2M(decimal x, decimal y)
		{
			X = x;
			Y = y;
		}

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

#region Indexers
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

