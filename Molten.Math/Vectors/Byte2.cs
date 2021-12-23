using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "byte"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Byte2
	{
		///<summary>The X component.</summary>
		public byte X;

		///<summary>The Y component.</summary>
		public byte Y;


		public static Byte2 One = new Byte2(1, 1);

		public static Byte2 Zero = new Byte2(0, 0);

		///<summary>Creates a new instance of <see cref = "Byte2"/></summary>
		public Byte2(byte x, byte y)
		{
			X = x;
			Y = y;
		}

#region Add operators
		public static Byte2 operator +(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X + right.X, left.Y + right.Y);
		}

		public static Byte2 operator +(Byte2 left, byte right)
		{
			return new Byte2(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Byte2 operator -(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X - right.X, left.Y - right.Y);
		}

		public static Byte2 operator -(Byte2 left, byte right)
		{
			return new Byte2(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Byte2 operator /(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X / right.X, left.Y / right.Y);
		}

		public static Byte2 operator /(Byte2 left, byte right)
		{
			return new Byte2(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Byte2 operator *(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X * right.X, left.Y * right.Y);
		}

		public static Byte2 operator *(Byte2 left, byte right)
		{
			return new Byte2(left.X * right, left.Y * right);
		}
#endregion
	}
}

