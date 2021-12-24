using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "float"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector2F
	{
		///<summary>The X component.</summary>
		public float X;

		///<summary>The Y component.</summary>
		public float Y;


		///<summary>The size of <see cref="Vector2F"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2F));

		public static Vector2F One = new Vector2F(1F, 1F);

		public static Vector2F Zero = new Vector2F(0F, 0F);

		///<summary>Creates a new instance of <see cref = "Vector2F"/></summary>
		public Vector2F(float x, float y)
		{
			X = x;
			Y = y;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2F"/> vectors.
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
		public static void DistanceSquared(ref Vector2F value1, ref Vector2F value2, out float result)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }
#endregion

#region Add operators
		public static Vector2F operator +(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2F operator +(Vector2F left, float right)
		{
			return new Vector2F(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2F operator -(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2F operator -(Vector2F left, float right)
		{
			return new Vector2F(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2F operator /(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2F operator /(Vector2F left, float right)
		{
			return new Vector2F(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2F operator *(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2F operator *(Vector2F left, float right)
		{
			return new Vector2F(left.X * right, left.Y * right);
		}
#endregion
	}
}

