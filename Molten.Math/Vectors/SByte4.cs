using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "sbyte"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct SByte4
	{
		///<summary>The X component.</summary>
		public sbyte X;

		///<summary>The Y component.</summary>
		public sbyte Y;

		///<summary>The Z component.</summary>
		public sbyte Z;

		///<summary>The W component.</summary>
		public sbyte W;


		public static SByte4 One = new SByte4(1, 1, 1, 1);

		public static SByte4 Zero = new SByte4(0, 0, 0, 0);

		///<summary>Creates a new instance of <see cref = "SByte4"/></summary>
		public SByte4(sbyte x, sbyte y, sbyte z, sbyte w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Add operators
		public static SByte4 operator +(SByte4 left, SByte4 right)
		{
			return new SByte4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static SByte4 operator +(SByte4 left, sbyte right)
		{
			return new SByte4(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static SByte4 operator -(SByte4 left, SByte4 right)
		{
			return new SByte4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static SByte4 operator -(SByte4 left, sbyte right)
		{
			return new SByte4(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static SByte4 operator /(SByte4 left, SByte4 right)
		{
			return new SByte4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static SByte4 operator /(SByte4 left, sbyte right)
		{
			return new SByte4(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static SByte4 operator *(SByte4 left, SByte4 right)
		{
			return new SByte4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static SByte4 operator *(SByte4 left, sbyte right)
		{
			return new SByte4(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

