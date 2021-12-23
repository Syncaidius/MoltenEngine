using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector4I
	{
		///<summary>The X component.</summary>
		public int X;

		///<summary>The Y component.</summary>
		public int Y;

		///<summary>The Z component.</summary>
		public int Z;

		///<summary>The W component.</summary>
		public int W;


		///<summary>The size of <see cref="Vector4I"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4I));

		public static Vector4I One = new Vector4I(1, 1, 1, 1);

		public static Vector4I Zero = new Vector4I(0, 0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Vector4I"/></summary>
		public Vector4I(int x, int y, int z, int w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Add operators
		public static Vector4I operator +(Vector4I left, Vector4I right)
		{
			return new Vector4I(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4I operator +(Vector4I left, int right)
		{
			return new Vector4I(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4I operator -(Vector4I left, Vector4I right)
		{
			return new Vector4I(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4I operator -(Vector4I left, int right)
		{
			return new Vector4I(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4I operator /(Vector4I left, Vector4I right)
		{
			return new Vector4I(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4I operator /(Vector4I left, int right)
		{
			return new Vector4I(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4I operator *(Vector4I left, Vector4I right)
		{
			return new Vector4I(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4I operator *(Vector4I left, int right)
		{
			return new Vector4I(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

