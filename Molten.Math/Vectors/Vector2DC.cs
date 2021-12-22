using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector2DC
	{
		///<summary>The X component.</summary>
		public decimal X;

		///<summary>The Y component.</summary>
		public decimal Y;

		///<summary>Creates a new instance of <see cref = "Vector2DC"/></summary>
		public Vector2DC(decimal x, decimal y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Vector2DC operator +(Vector2DC left, Vector2DC right)
		{
			return new Vector2DC(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2DC operator -(Vector2DC left, Vector2DC right)
		{
			return new Vector2DC(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2DC operator /(Vector2DC left, Vector2DC right)
		{
			return new Vector2DC(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2DC operator *(Vector2DC left, Vector2DC right)
		{
			return new Vector2DC(left.X * right.X, left.Y * right.Y);
		}
#endregion
	}
}

