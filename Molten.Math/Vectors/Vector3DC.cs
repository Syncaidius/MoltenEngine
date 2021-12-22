using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector3DC
	{
		///<summary>The X component.</summary>
		public decimal X;

		///<summary>The Y component.</summary>
		public decimal Y;

		///<summary>The Z component.</summary>
		public decimal Z;

		///<summary>Creates a new instance of <see cref = "Vector3DC"/></summary>
		public Vector3DC(decimal x, decimal y, decimal z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region operators
		public static Vector3DC operator +(Vector3DC left, Vector3DC right)
		{
			return new Vector3DC(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3DC operator -(Vector3DC left, Vector3DC right)
		{
			return new Vector3DC(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3DC operator /(Vector3DC left, Vector3DC right)
		{
			return new Vector3DC(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3DC operator *(Vector3DC left, Vector3DC right)
		{
			return new Vector3DC(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}
#endregion
	}
}

