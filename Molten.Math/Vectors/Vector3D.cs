using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector3D
	{
		///<summary>The X component.</summary>
		public double X;

		///<summary>The Y component.</summary>
		public double Y;

		///<summary>The Z component.</summary>
		public double Z;


		///<summary>The size of <see cref="Vector3D"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3D));

		public static Vector3D One = new Vector3D(1D, 1D, 1D);

		public static Vector3D Zero = new Vector3D(0D, 0D, 0D);

		///<summary>Creates a new instance of <see cref = "Vector3D"/></summary>
		public Vector3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3D"/> vectors.
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
		public static void DistanceSquared(ref Vector3D value1, ref Vector3D value2, out double result)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }
#endregion

#region Add operators
		public static Vector3D operator +(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3D operator +(Vector3D left, double right)
		{
			return new Vector3D(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3D operator -(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3D operator -(Vector3D left, double right)
		{
			return new Vector3D(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3D operator /(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3D operator /(Vector3D left, double right)
		{
			return new Vector3D(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3D operator *(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3D operator *(Vector3D left, double right)
		{
			return new Vector3D(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

