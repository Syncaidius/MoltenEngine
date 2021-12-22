using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector2I
	{
		///<summary>The X component.</summary>
		public int X;

		///<summary>The Y component.</summary>
		public int Y;


		public static Vector2I One = new Vector2I(1, 1);

		public static Vector2I Zero = new Vector2I(0, 0);

		///<summary>Creates a new instance of <see cref = "Vector2I"/></summary>
		public Vector2I(int x, int y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Vector2I operator +(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2I operator -(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2I operator /(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2I operator *(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X * right.X, left.Y * right.Y);
		}
#endregion
	}
}

