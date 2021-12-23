using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nint"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector2N
	{
		///<summary>The X component.</summary>
		public nint X;

		///<summary>The Y component.</summary>
		public nint Y;


		public static Vector2N One = new Vector2N(1, 1);

		public static Vector2N Zero = new Vector2N(0, 0);

		///<summary>Creates a new instance of <see cref = "Vector2N"/></summary>
		public Vector2N(nint x, nint y)
		{
			X = x;
			Y = y;
		}

#region Add operators
		public static Vector2N operator +(Vector2N left, Vector2N right)
		{
			return new Vector2N(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2N operator +(Vector2N left, nint right)
		{
			return new Vector2N(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2N operator -(Vector2N left, Vector2N right)
		{
			return new Vector2N(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2N operator -(Vector2N left, nint right)
		{
			return new Vector2N(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2N operator /(Vector2N left, Vector2N right)
		{
			return new Vector2N(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2N operator /(Vector2N left, nint right)
		{
			return new Vector2N(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2N operator *(Vector2N left, Vector2N right)
		{
			return new Vector2N(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2N operator *(Vector2N left, nint right)
		{
			return new Vector2N(left.X * right, left.Y * right);
		}
#endregion
	}
}

