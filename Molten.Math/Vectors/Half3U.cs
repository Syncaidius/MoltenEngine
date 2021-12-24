using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half3U
	{
		///<summary>The X component.</summary>
		public ushort X;

		///<summary>The Y component.</summary>
		public ushort Y;

		///<summary>The Z component.</summary>
		public ushort Z;


		///<summary>The size of <see cref="Half3U"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half3U));

		public static Half3U One = new Half3U((ushort)1, (ushort)1, (ushort)1);

		public static Half3U Zero = new Half3U(0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Half3U"/></summary>
		public Half3U(ushort x, ushort y, ushort z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Half3U"/> vectors.
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
		public static void DistanceSquared(ref Half3U value1, ref Half3U value2, out ushort result)
        {
            ushort x = value1.X - value2.X;
            ushort y = value1.Y - value2.Y;
            ushort z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }
#endregion

#region Add operators
		public static Half3U operator +(Half3U left, Half3U right)
		{
			return new Half3U(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Half3U operator +(Half3U left, ushort right)
		{
			return new Half3U(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Half3U operator -(Half3U left, Half3U right)
		{
			return new Half3U(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Half3U operator -(Half3U left, ushort right)
		{
			return new Half3U(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Half3U operator /(Half3U left, Half3U right)
		{
			return new Half3U(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Half3U operator /(Half3U left, ushort right)
		{
			return new Half3U(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Half3U operator *(Half3U left, Half3U right)
		{
			return new Half3U(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Half3U operator *(Half3U left, ushort right)
		{
			return new Half3U(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

