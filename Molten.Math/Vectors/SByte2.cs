




using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "sbyte"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct SByte2
	{
		///<summary>The X component.</summary>
		public sbyte X;

		///<summary>The Y component.</summary>
		public sbyte Y;

		///<summary>Creates a new instance of <see cref = "SByte2"/></summary>
		public SByte2(sbyte x, sbyte y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static SByte2 operator +(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X + right.X, left.Y + right.Y);
		}

		public static SByte2 operator -(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X - right.X, left.Y - right.Y);
		}

		public static SByte2 operator /(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X / right.X, left.Y / right.Y);
		}

		public static SByte2 operator *(SByte2 left, SByte2 right)
		{
			return new SByte2(left.X * right.X, left.Y * right.Y);
		}
#endregion
	}
}

