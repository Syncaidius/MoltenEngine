using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ulong"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector4UL
	{
		///<summary>The X component.</summary>
		public ulong X;

		///<summary>The Y component.</summary>
		public ulong Y;

		///<summary>The Z component.</summary>
		public ulong Z;

		///<summary>The W component.</summary>
		public ulong W;


		public static Vector4UL One = new Vector4UL(1UL, 1UL, 1UL, 1UL);

		public static Vector4UL Zero = new Vector4UL(0UL, 0UL, 0UL, 0UL);

		///<summary>Creates a new instance of <see cref = "Vector4UL"/></summary>
		public Vector4UL(ulong x, ulong y, ulong z, ulong w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Add operators
		public static Vector4UL operator +(Vector4UL left, Vector4UL right)
		{
			return new Vector4UL(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4UL operator +(Vector4UL left, ulong right)
		{
			return new Vector4UL(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4UL operator -(Vector4UL left, Vector4UL right)
		{
			return new Vector4UL(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4UL operator -(Vector4UL left, ulong right)
		{
			return new Vector4UL(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4UL operator /(Vector4UL left, Vector4UL right)
		{
			return new Vector4UL(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4UL operator /(Vector4UL left, ulong right)
		{
			return new Vector4UL(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4UL operator *(Vector4UL left, Vector4UL right)
		{
			return new Vector4UL(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4UL operator *(Vector4UL left, ulong right)
		{
			return new Vector4UL(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

