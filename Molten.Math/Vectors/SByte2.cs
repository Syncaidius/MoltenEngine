




using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "sbyte"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public partial struct SByte2
	{
		///<summary>The X component.</summary>
		public sbyte X;

		///<summary>The Y component.</summary>
		public sbyte Y;


		///<summary>The size of <see cref="SByte2"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(SByte2));

		public static SByte2 One = new SByte2(1, 1);

		public static SByte2 Zero = new SByte2(0, 0);

		///<summary>Creates a new instance of <see cref = "SByte2"/></summary>
		public SByte2(sbyte x, sbyte y)
		{
			X = x;
			Y = y;
		}

#region Add operators
		public static SByte2 operator +(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X + right.X, left.Y + right.Y);
		}

		public static SByte2 operator +(SByte2 left, sbyte right)
		{
			return new SByte2(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static SByte2 operator -(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X - right.X, left.Y - right.Y);
		}

		public static SByte2 operator -(SByte2 left, sbyte right)
		{
			return new SByte2(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static SByte2 operator /(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X / right.X, left.Y / right.Y);
		}

		public static SByte2 operator /(SByte2 left, sbyte right)
		{
			return new SByte2(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static SByte2 operator *(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X * right.X, left.Y * right.Y);
		}

		public static SByte2 operator *(SByte2 left, sbyte right)
		{
			return new SByte2(left.X * right, left.Y * right);
		}
#endregion
	}
}

