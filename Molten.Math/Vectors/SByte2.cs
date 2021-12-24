




using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "sbyte"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public partial struct SByte2
	{
		///<summary>The X component.</summary>
		public sbyte X;

		///<summary>The Y component.</summary>
		public sbyte Y;


		///<summary>The size of <see cref="SByte2"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(SByte2));

		public static SByte2 One = new SByte2(1, 1);

		public static SByte2 Zero = new SByte2(0, 0);

		///<summary>Creates a new instance of <see cref = "SByte2"/></summary>
		public SByte2(sbyte x, sbyte y)
		{
			X = x;
			Y = y;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="SByte2"/> vectors.
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
		public static void DistanceSquared(ref SByte2 value1, ref SByte2 value2, out sbyte result)
        {
            sbyte x = value1.X - value2.X;
            sbyte y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }
#endregion

#region Add operators
		public static SByte2 operator +(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X + right.X, left.Y + right.Y);
		}

		public static SByte2 operator +(SByte2 left, sbyte right)
		{
			return new SByte2(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static SByte2 operator -(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X - right.X, left.Y - right.Y);
		}

		public static SByte2 operator -(SByte2 left, sbyte right)
		{
			return new SByte2(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static SByte2 operator /(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X / right.X, left.Y / right.Y);
		}

		public static SByte2 operator /(SByte2 left, sbyte right)
		{
			return new SByte2(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static SByte2 operator *(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X * right.X, left.Y * right.Y);
		}

		public static SByte2 operator *(SByte2 left, sbyte right)
		{
			return new SByte2(left.X * right, left.Y * right);
		}
#endregion
	}
}

