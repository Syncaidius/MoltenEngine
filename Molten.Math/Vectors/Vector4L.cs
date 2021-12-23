using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "long"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector4L
	{
		///<summary>The X component.</summary>
		public long X;

		///<summary>The Y component.</summary>
		public long Y;

		///<summary>The Z component.</summary>
		public long Z;

		///<summary>The W component.</summary>
		public long W;


		public static Vector4L One = new Vector4L(1L, 1L, 1L, 1L);

		public static Vector4L Zero = new Vector4L(0L, 0L, 0L, 0L);

		///<summary>Creates a new instance of <see cref = "Vector4L"/></summary>
		public Vector4L(long x, long y, long z, long w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Add operators
		public static Vector4L operator +(Vector4L left, Vector4L right)
		{
			return new Vector4L(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4L operator +(Vector4L left, long right)
		{
			return new Vector4L(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4L operator -(Vector4L left, Vector4L right)
		{
			return new Vector4L(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4L operator -(Vector4L left, long right)
		{
			return new Vector4L(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4L operator /(Vector4L left, Vector4L right)
		{
			return new Vector4L(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4L operator /(Vector4L left, long right)
		{
			return new Vector4L(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4L operator *(Vector4L left, Vector4L right)
		{
			return new Vector4L(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4L operator *(Vector4L left, long right)
		{
			return new Vector4L(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

