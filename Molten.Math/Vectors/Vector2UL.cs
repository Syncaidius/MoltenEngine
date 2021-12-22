using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ulong"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector2UL
	{
		///<summary>The X component.</summary>
		public ulong X;

		///<summary>The Y component.</summary>
		public ulong Y;

		///<summary>Creates a new instance of <see cref = "Vector2UL"/></summary>
		public Vector2UL(ulong x, ulong y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Vector2UL operator +(Vector2UL left, Vector2UL right)
		{
			return new Vector2UL(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2UL operator -(Vector2UL left, Vector2UL right)
		{
			return new Vector2UL(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2UL operator /(Vector2UL left, Vector2UL right)
		{
			return new Vector2UL(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2UL operator *(Vector2UL left, Vector2UL right)
		{
			return new Vector2UL(left.X * right.X, left.Y * right.Y);
		}
#endregion
	}
}

