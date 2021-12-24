using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ulong"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector3UL
	{
		///<summary>The X component.</summary>
		public ulong X;

		///<summary>The Y component.</summary>
		public ulong Y;

		///<summary>The Z component.</summary>
		public ulong Z;


		///<summary>The size of <see cref="Vector3UL"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3UL));

		public static Vector3UL One = new Vector3UL(1UL, 1UL, 1UL);

		public static Vector3UL Zero = new Vector3UL(0UL, 0UL, 0UL);

		///<summary>Creates a new instance of <see cref = "Vector3UL"/></summary>
		public Vector3UL(ulong x, ulong y, ulong z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3UL"/> vectors.
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
		public static void DistanceSquared(ref Vector3UL value1, ref Vector3UL value2, out ulong result)
        {
            ulong x = value1.X - value2.X;
            ulong y = value1.Y - value2.Y;
            ulong z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3UL"/> vectors.
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
		public static ulong DistanceSquared(ref Vector3UL value1, ref Vector3UL value2)
        {
            ulong x = value1.X - value2.X;
            ulong y = value1.Y - value2.Y;
            ulong z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }


#endregion

#region Add operators
		public static Vector3UL operator +(Vector3UL left, Vector3UL right)
		{
			return new Vector3UL(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3UL operator +(Vector3UL left, ulong right)
		{
			return new Vector3UL(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3UL operator -(Vector3UL left, Vector3UL right)
		{
			return new Vector3UL(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3UL operator -(Vector3UL left, ulong right)
		{
			return new Vector3UL(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3UL operator /(Vector3UL left, Vector3UL right)
		{
			return new Vector3UL(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3UL operator /(Vector3UL left, ulong right)
		{
			return new Vector3UL(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3UL operator *(Vector3UL left, Vector3UL right)
		{
			return new Vector3UL(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3UL operator *(Vector3UL left, ulong right)
		{
			return new Vector3UL(left.X * right, left.Y * right, left.Z * right);
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
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3UL run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3UL run from 0 to 2, inclusive.");
			}
		}
#endregion
	}
}

