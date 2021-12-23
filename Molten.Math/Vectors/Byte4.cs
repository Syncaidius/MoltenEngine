using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "byte"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
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

