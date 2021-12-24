using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half2U
	{
		///<summary>The X component.</summary>
		public ushort X;

		///<summary>The Y component.</summary>
		public ushort Y;


		///<summary>The size of <see cref="Half2U"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half2U));

		public static Half2U One = new Half2U((ushort)1, (ushort)1);

		public static Half2U Zero = new Half2U(0, 0);

		///<summary>Creates a new instance of <see cref = "Half2U"/></summary>
		public Half2U(ushort x, ushort y)
		{
			X = x;
			Y = y;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Half2U"/> vectors.
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
		public static void DistanceSquared(ref Half2U value1, ref Half2U value2, out ushort result)
        {
            ushort x = value1.X - value2.X;
            ushort y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }
#endregion

#region Add operators
		public static Half2U operator +(Half2U left, Half2U right)
		{
			return new Half2U(left.X + right.X, left.Y + right.Y);
		}

		public static Half2U operator +(Half2U left, ushort right)
		{
			return new Half2U(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Half2U operator -(Half2U left, Half2U right)
		{
			return new Half2U(left.X - right.X, left.Y - right.Y);
		}

		public static Half2U operator -(Half2U left, ushort right)
		{
			return new Half2U(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Half2U operator /(Half2U left, Half2U right)
		{
			return new Half2U(left.X / right.X, left.Y / right.Y);
		}

		public static Half2U operator /(Half2U left, ushort right)
		{
			return new Half2U(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Half2U operator *(Half2U left, Half2U right)
		{
			return new Half2U(left.X * right.X, left.Y * right.Y);
		}

		public static Half2U operator *(Half2U left, ushort right)
		{
			return new Half2U(left.X * right, left.Y * right);
		}
#endregion
	}
}

