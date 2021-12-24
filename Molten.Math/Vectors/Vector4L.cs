using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "long"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector4L
	{
		///<summary>The X component.</summary>
		public long X;

		///<summary>The Y component.</summary>
		public long Y;

		///<summary>The Z component.</summary>
		public long Z;

		///<summary>The W component.</summary>
		public long W;


		///<summary>The size of <see cref="Vector4L"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4L));

		public static Vector4L One = new Vector4L(1L, 1L, 1L, 1L);

		public static Vector4L Zero = new Vector4L(0L, 0L, 0L, 0L);

		///<summary>Creates a new instance of <see cref = "Vector4L"/></summary>
		public Vector4L(long x, long y, long z, long w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4L"/> vectors.
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
		public static void DistanceSquared(ref Vector4L value1, ref Vector4L value2, out long result)
        {
            long x = value1.X - value2.X;
            long y = value1.Y - value2.Y;
            long z = value1.Z - value2.Z;
            long w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }
#endregion

#region Add operators
		public static Vector4L operator +(Vector4L left, Vector4L right)
		{
			return new Vector4L(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4L operator +(Vector4L left, long right)
		{
			return new Vector4L(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4L operator -(Vector4L left, Vector4L right)
		{
			return new Vector4L(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4L operator -(Vector4L left, long right)
		{
			return new Vector4L(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4L operator /(Vector4L left, Vector4L right)
		{
			return new Vector4L(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4L operator /(Vector4L left, long right)
		{
			return new Vector4L(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4L operator *(Vector4L left, Vector4L right)
		{
			return new Vector4L(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4L operator *(Vector4L left, long right)
		{
			return new Vector4L(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

