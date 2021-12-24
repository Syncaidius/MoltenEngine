using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector3I
	{
		///<summary>The X component.</summary>
		public int X;

		///<summary>The Y component.</summary>
		public int Y;

		///<summary>The Z component.</summary>
		public int Z;


		///<summary>The size of <see cref="Vector3I"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3I));

		public static Vector3I One = new Vector3I(1, 1, 1);

		public static Vector3I Zero = new Vector3I(0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Vector3I"/></summary>
		public Vector3I(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3I"/> vectors.
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
		public static void DistanceSquared(ref Vector3I value1, ref Vector3I value2, out int result)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3I"/> vectors.
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
		public static int DistanceSquared(ref Vector3I value1, ref Vector3I value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }


#endregion

#region Add operators
		public static Vector3I operator +(Vector3I left, Vector3I right)
		{
			return new Vector3I(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3I operator +(Vector3I left, int right)
		{
			return new Vector3I(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3I operator -(Vector3I left, Vector3I right)
		{
			return new Vector3I(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3I operator -(Vector3I left, int right)
		{
			return new Vector3I(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3I operator /(Vector3I left, Vector3I right)
		{
			return new Vector3I(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3I operator /(Vector3I left, int right)
		{
			return new Vector3I(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3I operator *(Vector3I left, Vector3I right)
		{
			return new Vector3I(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3I operator *(Vector3I left, int right)
		{
			return new Vector3I(left.X * right, left.Y * right, left.Z * right);
		}
#endregion

#region Indexers
		public int this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3I run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3I run from 0 to 2, inclusive.");
			}
		}
#endregion
	}
}

