using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=16)]
	public partial struct Vector4M
	{
		///<summary>The X component.</summary>
		public decimal X;

		///<summary>The Y component.</summary>
		public decimal Y;

		///<summary>The Z component.</summary>
		public decimal Z;

		///<summary>The W component.</summary>
		public decimal W;


		///<summary>The size of <see cref="Vector4M"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4M));

		public static Vector4M One = new Vector4M(1M, 1M, 1M, 1M);

		public static Vector4M Zero = new Vector4M(0M, 0M, 0M, 0M);

		///<summary>Creates a new instance of <see cref = "Vector4M"/></summary>
		public Vector4M(decimal x, decimal y, decimal z, decimal w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4M"/> vectors.
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
		public static void DistanceSquared(ref Vector4M value1, ref Vector4M value2, out decimal result)
        {
            decimal x = value1.X - value2.X;
            decimal y = value1.Y - value2.Y;
            decimal z = value1.Z - value2.Z;
            decimal w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }
#endregion

#region Add operators
		public static Vector4M operator +(Vector4M left, Vector4M right)
		{
			return new Vector4M(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4M operator +(Vector4M left, decimal right)
		{
			return new Vector4M(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4M operator -(Vector4M left, Vector4M right)
		{
			return new Vector4M(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4M operator -(Vector4M left, decimal right)
		{
			return new Vector4M(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4M operator /(Vector4M left, Vector4M right)
		{
			return new Vector4M(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4M operator /(Vector4M left, decimal right)
		{
			return new Vector4M(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4M operator *(Vector4M left, Vector4M right)
		{
			return new Vector4M(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4M operator *(Vector4M left, decimal right)
		{
			return new Vector4M(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

