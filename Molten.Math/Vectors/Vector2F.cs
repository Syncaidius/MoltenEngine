using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "float"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector2F
	{
		///<summary>The X component.</summary>
		public float X;

		///<summary>The Y component.</summary>
		public float Y;

		///<summary>Creates a new instance of <see cref = "Vector2F"/></summary>
		public Vector2F(float x, float y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Vector2F operator +(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2F operator -(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2F operator /(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2F operator *(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X * right.X, left.Y * right.Y);
		}
#endregion
	}
}

