using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "uint"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector4UI
	{
		///<summary>The X component.</summary>
		public uint X;

		///<summary>The Y component.</summary>
		public uint Y;

		///<summary>The Z component.</summary>
		public uint Z;

		///<summary>The W component.</summary>
		public uint W;


		public static Vector4UI One = new Vector4UI(1U, 1U, 1U, 1U);

		public static Vector4UI Zero = new Vector4UI(0U, 0U, 0U, 0U);

		///<summary>Creates a new instance of <see cref = "Vector4UI"/></summary>
		public Vector4UI(uint x, uint y, uint z, uint w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Add operators
		public static Vector4UI operator +(Vector4UI left, Vector4UI right)
		{
			return new Vector4UI(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4UI operator +(Vector4UI left, uint right)
		{
			return new Vector4UI(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4UI operator -(Vector4UI left, Vector4UI right)
		{
			return new Vector4UI(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4UI operator -(Vector4UI left, uint right)
		{
			return new Vector4UI(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4UI operator /(Vector4UI left, Vector4UI right)
		{
			return new Vector4UI(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4UI operator /(Vector4UI left, uint right)
		{
			return new Vector4UI(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4UI operator *(Vector4UI left, Vector4UI right)
		{
			return new Vector4UI(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4UI operator *(Vector4UI left, uint right)
		{
			return new Vector4UI(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

