using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector2D
	{
		///<summary>The X component.</summary>
		public double X;

		///<summary>The Y component.</summary>
		public double Y;


		///<summary>The size of <see cref="Vector2D"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2D));

		public static Vector2D One = new Vector2D(1D, 1D);

		public static Vector2D Zero = new Vector2D(0D, 0D);

		///<summary>Creates a new instance of <see cref = "Vector2D"/></summary>
		public Vector2D(double x, double y)
		{
			X = x;
			Y = y;
		}

#region Add operators
		public static Vector2D operator +(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2D operator +(Vector2D left, double right)
		{
			return new Vector2D(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2D operator -(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2D operator -(Vector2D left, double right)
		{
			return new Vector2D(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2D operator /(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2D operator /(Vector2D left, double right)
		{
			return new Vector2D(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2D operator *(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2D operator *(Vector2D left, double right)
		{
			return new Vector2D(left.X * right, left.Y * right);
		}
#endregion
	}
}

