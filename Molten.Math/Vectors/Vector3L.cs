using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "long"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector3L
	{
		///<summary>The X component.</summary>
		public long X;

		///<summary>The Y component.</summary>
		public long Y;

		///<summary>The Z component.</summary>
		public long Z;


		///<summary>The size of <see cref="Vector3L"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3L));

		public static Vector3L One = new Vector3L(1L, 1L, 1L);

		public static Vector3L Zero = new Vector3L(0L, 0L, 0L);

		///<summary>Creates a new instance of <see cref = "Vector3L"/></summary>
		public Vector3L(long x, long y, long z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Add operators
		public static Vector3L operator +(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3L operator +(Vector3L left, long right)
		{
			return new Vector3L(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3L operator -(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3L operator -(Vector3L left, long right)
		{
			return new Vector3L(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3L operator /(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3L operator /(Vector3L left, long right)
		{
			return new Vector3L(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3L operator *(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3L operator *(Vector3L left, long right)
		{
			return new Vector3L(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

