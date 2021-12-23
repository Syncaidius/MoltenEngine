using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector3I
	{
		///<summary>The X component.</summary>
		public int X;

		///<summary>The Y component.</summary>
		public int Y;

		///<summary>The Z component.</summary>
		public int Z;


		public static Vector3I One = new Vector3I(1, 1, 1);

		public static Vector3I Zero = new Vector3I(0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Vector3I"/></summary>
		public Vector3I(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Add operators
		public static Vector3I operator +(Vector3I left, Vector3I right)
		{
			return new Vector3I(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3I operator +(Vector3I left, int right)
		{
			return new Vector3I(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3I operator -(Vector3I left, Vector3I right)
		{
			return new Vector3I(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3I operator -(Vector3I left, int right)
		{
			return new Vector3I(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3I operator /(Vector3I left, Vector3I right)
		{
			return new Vector3I(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3I operator /(Vector3I left, int right)
		{
			return new Vector3I(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3I operator *(Vector3I left, Vector3I right)
		{
			return new Vector3I(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3I operator *(Vector3I left, int right)
		{
			return new Vector3I(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

