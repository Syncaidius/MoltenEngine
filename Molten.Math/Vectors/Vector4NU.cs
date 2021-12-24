using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nuint"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=0)]
	public partial struct Vector4NU
	{
		///<summary>The X component.</summary>
		public nuint X;

		///<summary>The Y component.</summary>
		public nuint Y;

		///<summary>The Z component.</summary>
		public nuint Z;

		///<summary>The W component.</summary>
		public nuint W;


		///<summary>The size of <see cref="Vector4NU"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4NU));

		public static Vector4NU One = new Vector4NU(1U, 1U, 1U, 1U);

		public static Vector4NU Zero = new Vector4NU(0U, 0U, 0U, 0U);

		///<summary>Creates a new instance of <see cref = "Vector4NU"/></summary>
		public Vector4NU(nuint x, nuint y, nuint z, nuint w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4NU"/> vectors.
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
		public static void DistanceSquared(ref Vector4NU value1, ref Vector4NU value2, out nuint result)
        {
            nuint x = value1.X - value2.X;
            nuint y = value1.Y - value2.Y;
            nuint z = value1.Z - value2.Z;
            nuint w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }
#endregion

#region Add operators
		public static Vector4NU operator +(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4NU operator +(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4NU operator -(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4NU operator -(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4NU operator /(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4NU operator /(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4NU operator *(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4NU operator *(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

