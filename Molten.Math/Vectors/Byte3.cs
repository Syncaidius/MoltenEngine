using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "byte"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public partial struct Byte3
	{
		///<summary>The X component.</summary>
		public byte X;

		///<summary>The Y component.</summary>
		public byte Y;

		///<summary>The Z component.</summary>
		public byte Z;


		///<summary>The size of <see cref="Byte3"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Byte3));

		public static Byte3 One = new Byte3(1, 1, 1);

		public static Byte3 Zero = new Byte3(0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Byte3"/></summary>
		public Byte3(byte x, byte y, byte z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Byte3"/> vectors.
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
		public static void DistanceSquared(ref Byte3 value1, ref Byte3 value2, out byte result)
        {
            byte x = value1.X - value2.X;
            byte y = value1.Y - value2.Y;
            byte z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }
#endregion

#region Add operators
		public static Byte3 operator +(Byte3 left, Byte3 right)
		{
			return new Byte3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Byte3 operator +(Byte3 left, byte right)
		{
			return new Byte3(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Byte3 operator -(Byte3 left, Byte3 right)
		{
			return new Byte3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Byte3 operator -(Byte3 left, byte right)
		{
			return new Byte3(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Byte3 operator /(Byte3 left, Byte3 right)
		{
			return new Byte3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Byte3 operator /(Byte3 left, byte right)
		{
			return new Byte3(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Byte3 operator *(Byte3 left, Byte3 right)
		{
			return new Byte3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Byte3 operator *(Byte3 left, byte right)
		{
			return new Byte3(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

