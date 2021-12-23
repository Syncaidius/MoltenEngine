using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ulong"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector3UL
	{
		///<summary>The X component.</summary>
		public ulong X;

		///<summary>The Y component.</summary>
		public ulong Y;

		///<summary>The Z component.</summary>
		public ulong Z;


		///<summary>The size of <see cref="Vector3UL"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3UL));

		public static Vector3UL One = new Vector3UL(1UL, 1UL, 1UL);

		public static Vector3UL Zero = new Vector3UL(0UL, 0UL, 0UL);

		///<summary>Creates a new instance of <see cref = "Vector3UL"/></summary>
		public Vector3UL(ulong x, ulong y, ulong z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Add operators
		public static Vector3UL operator +(Vector3UL left, Vector3UL right)
		{
			return new Vector3UL(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3UL operator +(Vector3UL left, ulong right)
		{
			return new Vector3UL(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3UL operator -(Vector3UL left, Vector3UL right)
		{
			return new Vector3UL(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3UL operator -(Vector3UL left, ulong right)
		{
			return new Vector3UL(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3UL operator /(Vector3UL left, Vector3UL right)
		{
			return new Vector3UL(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3UL operator /(Vector3UL left, ulong right)
		{
			return new Vector3UL(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3UL operator *(Vector3UL left, Vector3UL right)
		{
			return new Vector3UL(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3UL operator *(Vector3UL left, ulong right)
		{
			return new Vector3UL(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

