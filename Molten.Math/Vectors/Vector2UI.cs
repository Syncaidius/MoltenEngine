using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "uint"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector2UI
	{
		///<summary>The X component.</summary>
		public uint X;

		///<summary>The Y component.</summary>
		public uint Y;


		///<summary>The size of <see cref="Vector2UI"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2UI));

		public static Vector2UI One = new Vector2UI(1U, 1U);

		public static Vector2UI Zero = new Vector2UI(0U, 0U);

		///<summary>Creates a new instance of <see cref = "Vector2UI"/></summary>
		public Vector2UI(uint x, uint y)
		{
			X = x;
			Y = y;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2UI"/> vectors.
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
		public static void DistanceSquared(ref Vector2UI value1, ref Vector2UI value2, out uint result)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }
#endregion

#region Add operators
		public static Vector2UI operator +(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2UI operator +(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2UI operator -(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2UI operator -(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2UI operator /(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2UI operator /(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2UI operator *(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2UI operator *(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X * right, left.Y * right);
		}
#endregion
	}
}

