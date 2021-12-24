using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nuint"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=0)]
	public partial struct Vector2NU
	{
		///<summary>The X component.</summary>
		public nuint X;

		///<summary>The Y component.</summary>
		public nuint Y;


		///<summary>The size of <see cref="Vector2NU"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2NU));

		public static Vector2NU One = new Vector2NU(1U, 1U);

		public static Vector2NU Zero = new Vector2NU(0U, 0U);

		///<summary>Creates a new instance of <see cref = "Vector2NU"/></summary>
		public Vector2NU(nuint x, nuint y)
		{
			X = x;
			Y = y;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2NU"/> vectors.
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
		public static void DistanceSquared(ref Vector2NU value1, ref Vector2NU value2, out nuint result)
        {
            nuint x = value1.X - value2.X;
            nuint y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }
#endregion

#region Add operators
		public static Vector2NU operator +(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2NU operator +(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2NU operator -(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2NU operator -(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2NU operator /(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2NU operator /(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2NU operator *(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2NU operator *(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X * right, left.Y * right);
		}
#endregion
	}
}

