using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nint"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=0)]
	public partial struct Vector3N
	{
		///<summary>The X component.</summary>
		public nint X;

		///<summary>The Y component.</summary>
		public nint Y;

		///<summary>The Z component.</summary>
		public nint Z;


		///<summary>The size of <see cref="Vector3N"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3N));

		public static Vector3N One = new Vector3N(1, 1, 1);

		public static Vector3N Zero = new Vector3N(0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Vector3N"/></summary>
		public Vector3N(nint x, nint y, nint z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Add operators
		public static Vector3N operator +(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3N operator +(Vector3N left, nint right)
		{
			return new Vector3N(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3N operator -(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3N operator -(Vector3N left, nint right)
		{
			return new Vector3N(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3N operator /(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3N operator /(Vector3N left, nint right)
		{
			return new Vector3N(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3N operator *(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3N operator *(Vector3N left, nint right)
		{
			return new Vector3N(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

