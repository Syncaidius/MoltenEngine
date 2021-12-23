using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "sbyte"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public partial struct SByte3
	{
		///<summary>The X component.</summary>
		public sbyte X;

		///<summary>The Y component.</summary>
		public sbyte Y;

		///<summary>The Z component.</summary>
		public sbyte Z;


		///<summary>The size of <see cref="SByte3"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(SByte3));

		public static SByte3 One = new SByte3(1, 1, 1);

		public static SByte3 Zero = new SByte3(0, 0, 0);

		///<summary>Creates a new instance of <see cref = "SByte3"/></summary>
		public SByte3(sbyte x, sbyte y, sbyte z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Add operators
		public static SByte3 operator +(SByte3 left, SByte3 right)
		{
			return new SByte3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static SByte3 operator +(SByte3 left, sbyte right)
		{
			return new SByte3(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static SByte3 operator -(SByte3 left, SByte3 right)
		{
			return new SByte3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static SByte3 operator -(SByte3 left, sbyte right)
		{
			return new SByte3(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static SByte3 operator /(SByte3 left, SByte3 right)
		{
			return new SByte3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static SByte3 operator /(SByte3 left, sbyte right)
		{
			return new SByte3(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static SByte3 operator *(SByte3 left, SByte3 right)
		{
			return new SByte3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static SByte3 operator *(SByte3 left, sbyte right)
		{
			return new SByte3(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

