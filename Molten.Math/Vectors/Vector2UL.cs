using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ulong"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector2UL
	{
		///<summary>The X component.</summary>
		public ulong X;

		///<summary>The Y component.</summary>
		public ulong Y;


		///<summary>The size of <see cref="Vector2UL"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2UL));

		public static Vector2UL One = new Vector2UL(1UL, 1UL);

		public static Vector2UL Zero = new Vector2UL(0UL, 0UL);

		///<summary>Creates a new instance of <see cref = "Vector2UL"/></summary>
		public Vector2UL(ulong x, ulong y)
		{
			X = x;
			Y = y;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2UL"/> vectors.
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
		public static void DistanceSquared(ref Vector2UL value1, ref Vector2UL value2, out ulong result)
        {
            ulong x = value1.X - value2.X;
            ulong y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2UL"/> vectors.
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
		public static ulong DistanceSquared(ref Vector2UL value1, ref Vector2UL value2)
        {
            ulong x = value1.X - value2.X;
            ulong y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }


#endregion

#region Add operators
		public static Vector2UL operator +(Vector2UL left, Vector2UL right)
		{
			return new Vector2UL(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2UL operator +(Vector2UL left, ulong right)
		{
			return new Vector2UL(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2UL operator -(Vector2UL left, Vector2UL right)
		{
			return new Vector2UL(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2UL operator -(Vector2UL left, ulong right)
		{
			return new Vector2UL(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2UL operator /(Vector2UL left, Vector2UL right)
		{
			return new Vector2UL(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2UL operator /(Vector2UL left, ulong right)
		{
			return new Vector2UL(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2UL operator *(Vector2UL left, Vector2UL right)
		{
			return new Vector2UL(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2UL operator *(Vector2UL left, ulong right)
		{
			return new Vector2UL(left.X * right, left.Y * right);
		}
#endregion

#region Indexers
		public ulong this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2UL run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2UL run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

