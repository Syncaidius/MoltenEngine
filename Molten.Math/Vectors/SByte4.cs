using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "sbyte"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public partial struct SByte4
	{
		///<summary>The X component.</summary>
		public sbyte X;

		///<summary>The Y component.</summary>
		public sbyte Y;

		///<summary>The Z component.</summary>
		public sbyte Z;

		///<summary>The W component.</summary>
		public sbyte W;


		///<summary>The size of <see cref="SByte4"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(SByte4));

		public static SByte4 One = new SByte4(1, 1, 1, 1);

		public static SByte4 Zero = new SByte4(0, 0, 0, 0);

		///<summary>Creates a new instance of <see cref = "SByte4"/></summary>
		public SByte4(sbyte x, sbyte y, sbyte z, sbyte w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="SByte4"/> vectors.
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
		public static void DistanceSquared(ref SByte4 value1, ref SByte4 value2, out sbyte result)
        {
            sbyte x = value1.X - value2.X;
            sbyte y = value1.Y - value2.Y;
            sbyte z = value1.Z - value2.Z;
            sbyte w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }
#endregion

#region Add operators
		public static SByte4 operator +(SByte4 left, SByte4 right)
		{
			return new SByte4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static SByte4 operator +(SByte4 left, sbyte right)
		{
			return new SByte4(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static SByte4 operator -(SByte4 left, SByte4 right)
		{
			return new SByte4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static SByte4 operator -(SByte4 left, sbyte right)
		{
			return new SByte4(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static SByte4 operator /(SByte4 left, SByte4 right)
		{
			return new SByte4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static SByte4 operator /(SByte4 left, sbyte right)
		{
			return new SByte4(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static SByte4 operator *(SByte4 left, SByte4 right)
		{
			return new SByte4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static SByte4 operator *(SByte4 left, sbyte right)
		{
			return new SByte4(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

