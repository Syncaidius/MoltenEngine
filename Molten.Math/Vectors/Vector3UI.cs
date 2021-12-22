using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "uint"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector3UI
	{
		///<summary>The X component.</summary>
		public uint X;

		///<summary>The Y component.</summary>
		public uint Y;

		///<summary>The Z component.</summary>
		public uint Z;


		public static Vector3UI One = new Vector3UI(1U, 1U, 1U);

		public static Vector3UI Zero = new Vector3UI(0U, 0U, 0U);

		///<summary>Creates a new instance of <see cref = "Vector3UI"/></summary>
		public Vector3UI(uint x, uint y, uint z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region operators
		public static Vector3UI operator +(Vector3UI left, Vector3UI right)
		{
			return new Vector3UI(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3UI operator -(Vector3UI left, Vector3UI right)
		{
			return new Vector3UI(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3UI operator /(Vector3UI left, Vector3UI right)
		{
			return new Vector3UI(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3UI operator *(Vector3UI left, Vector3UI right)
		{
			return new Vector3UI(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}
#endregion
	}
}

