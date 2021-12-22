using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nint"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector3N
	{
		///<summary>The X component.</summary>
		public nint X;

		///<summary>The Y component.</summary>
		public nint Y;

		///<summary>The Z component.</summary>
		public nint Z;

		///<summary>Creates a new instance of <see cref = "Vector3N"/></summary>
		public Vector3N(nint x, nint y, nint z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region operators
		public static Vector3N operator +(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3N operator -(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3N operator /(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3N operator *(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}
#endregion
	}
}

