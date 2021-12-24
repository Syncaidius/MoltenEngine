using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nint"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=0)]
	public partial struct Vector2N
	{
		///<summary>The X component.</summary>
		public nint X;

		///<summary>The Y component.</summary>
		public nint Y;


		///<summary>The size of <see cref="Vector2N"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2N));

		public static Vector2N One = new Vector2N(1, 1);

		public static Vector2N Zero = new Vector2N(0, 0);

		///<summary>Creates a new instance of <see cref = "Vector2N"/></summary>
		public Vector2N(nint x, nint y)
		{
			X = x;
			Y = y;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2N"/> vectors.
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
		public static void DistanceSquared(ref Vector2N value1, ref Vector2N value2, out nint result)
        {
            nint x = value1.X - value2.X;
            nint y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }
#endregion

#region Add operators
		public static Vector2N operator +(Vector2N left, Vector2N right)
		{
			return new Vector2N(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2N operator +(Vector2N left, nint right)
		{
			return new Vector2N(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2N operator -(Vector2N left, Vector2N right)
		{
			return new Vector2N(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2N operator -(Vector2N left, nint right)
		{
			return new Vector2N(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2N operator /(Vector2N left, Vector2N right)
		{
			return new Vector2N(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2N operator /(Vector2N left, nint right)
		{
			return new Vector2N(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2N operator *(Vector2N left, Vector2N right)
		{
			return new Vector2N(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2N operator *(Vector2N left, nint right)
		{
			return new Vector2N(left.X * right, left.Y * right);
		}
#endregion
	}
}

