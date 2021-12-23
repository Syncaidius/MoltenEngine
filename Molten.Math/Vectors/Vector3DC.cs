using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=16)]
	public partial struct Vector3DC
	{
		///<summary>The X component.</summary>
		public decimal X;

		///<summary>The Y component.</summary>
		public decimal Y;

		///<summary>The Z component.</summary>
		public decimal Z;


		///<summary>The size of <see cref="Vector3DC"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3DC));

		public static Vector3DC One = new Vector3DC(1M, 1M, 1M);

		public static Vector3DC Zero = new Vector3DC(0M, 0M, 0M);

		///<summary>Creates a new instance of <see cref = "Vector3DC"/></summary>
		public Vector3DC(decimal x, decimal y, decimal z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Add operators
		public static Vector3DC operator +(Vector3DC left, Vector3DC right)
		{
			return new Vector3DC(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3DC operator +(Vector3DC left, decimal right)
		{
			return new Vector3DC(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3DC operator -(Vector3DC left, Vector3DC right)
		{
			return new Vector3DC(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3DC operator -(Vector3DC left, decimal right)
		{
			return new Vector3DC(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3DC operator /(Vector3DC left, Vector3DC right)
		{
			return new Vector3DC(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3DC operator /(Vector3DC left, decimal right)
		{
			return new Vector3DC(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3DC operator *(Vector3DC left, Vector3DC right)
		{
			return new Vector3DC(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3DC operator *(Vector3DC left, decimal right)
		{
			return new Vector3DC(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

