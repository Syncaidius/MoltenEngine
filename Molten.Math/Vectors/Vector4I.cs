using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector4I
	{
		///<summary>The X component.</summary>
		public int X;

		///<summary>The Y component.</summary>
		public int Y;

		///<summary>The Z component.</summary>
		public int Z;

		///<summary>The W component.</summary>
		public int W;


		///<summary>The size of <see cref="Vector4I"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4I));

		public static Vector4I One = new Vector4I(1, 1, 1, 1);

		public static Vector4I Zero = new Vector4I(0, 0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Vector4I"/></summary>
		public Vector4I(int x, int y, int z, int w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4I"/> vectors.
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
		public static void DistanceSquared(ref Vector4I value1, ref Vector4I value2, out int result)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;
            int w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }
#endregion

#region Add operators
		public static Vector4I operator +(Vector4I left, Vector4I right)
		{
			return new Vector4I(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4I operator +(Vector4I left, int right)
		{
			return new Vector4I(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4I operator -(Vector4I left, Vector4I right)
		{
			return new Vector4I(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4I operator -(Vector4I left, int right)
		{
			return new Vector4I(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4I operator /(Vector4I left, Vector4I right)
		{
			return new Vector4I(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4I operator /(Vector4I left, int right)
		{
			return new Vector4I(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4I operator *(Vector4I left, Vector4I right)
		{
			return new Vector4I(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4I operator *(Vector4I left, int right)
		{
			return new Vector4I(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

