using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "float"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector3F
	{
		///<summary>The X component.</summary>
		public float X;

		///<summary>The Y component.</summary>
		public float Y;

		///<summary>The Z component.</summary>
		public float Z;


		///<summary>The size of <see cref="Vector3F"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3F));

		public static Vector3F One = new Vector3F(1F, 1F, 1F);

		public static Vector3F Zero = new Vector3F(0F, 0F, 0F);

		///<summary>Creates a new instance of <see cref = "Vector3F"/></summary>
		public Vector3F(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3F"/> vectors.
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
		public static void DistanceSquared(ref Vector3F value1, ref Vector3F value2, out float result)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;
            float z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }
#endregion

#region Add operators
		public static Vector3F operator +(Vector3F left, Vector3F right)
		{
			return new Vector3F(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3F operator +(Vector3F left, float right)
		{
			return new Vector3F(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3F operator -(Vector3F left, Vector3F right)
		{
			return new Vector3F(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3F operator -(Vector3F left, float right)
		{
			return new Vector3F(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3F operator /(Vector3F left, Vector3F right)
		{
			return new Vector3F(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3F operator /(Vector3F left, float right)
		{
			return new Vector3F(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3F operator *(Vector3F left, Vector3F right)
		{
			return new Vector3F(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3F operator *(Vector3F left, float right)
		{
			return new Vector3F(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

