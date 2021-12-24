using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "byte"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public partial struct Byte4
	{
		///<summary>The X component.</summary>
		public byte X;

		///<summary>The Y component.</summary>
		public byte Y;

		///<summary>The Z component.</summary>
		public byte Z;

		///<summary>The W component.</summary>
		public byte W;


		///<summary>The size of <see cref="Byte4"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Byte4));

		public static Byte4 One = new Byte4(1, 1, 1, 1);

		public static Byte4 Zero = new Byte4(0, 0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Byte4"/></summary>
		public Byte4(byte x, byte y, byte z, byte w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Byte4"/> vectors.
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
		public static void DistanceSquared(ref Byte4 value1, ref Byte4 value2, out byte result)
        {
            byte x = value1.X - value2.X;
            byte y = value1.Y - value2.Y;
            byte z = value1.Z - value2.Z;
            byte w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }
#endregion

#region Add operators
		public static Byte4 operator +(Byte4 left, Byte4 right)
		{
			return new Byte4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Byte4 operator +(Byte4 left, byte right)
		{
			return new Byte4(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Byte4 operator -(Byte4 left, Byte4 right)
		{
			return new Byte4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Byte4 operator -(Byte4 left, byte right)
		{
			return new Byte4(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Byte4 operator /(Byte4 left, Byte4 right)
		{
			return new Byte4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Byte4 operator /(Byte4 left, byte right)
		{
			return new Byte4(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Byte4 operator *(Byte4 left, Byte4 right)
		{
			return new Byte4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Byte4 operator *(Byte4 left, byte right)
		{
			return new Byte4(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

