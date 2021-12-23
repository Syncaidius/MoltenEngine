using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "float"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector2F
	{
		///<summary>The X component.</summary>
		public float X;

		///<summary>The Y component.</summary>
		public float Y;


		///<summary>The size of <see cref="Vector2F"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2F));

		public static Vector2F One = new Vector2F(1F, 1F);

		public static Vector2F Zero = new Vector2F(0F, 0F);

		///<summary>Creates a new instance of <see cref = "Vector2F"/></summary>
		public Vector2F(float x, float y)
		{
			X = x;
			Y = y;
		}

#region Add operators
		public static Vector2F operator +(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2F operator +(Vector2F left, float right)
		{
			return new Vector2F(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2F operator -(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2F operator -(Vector2F left, float right)
		{
			return new Vector2F(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2F operator /(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2F operator /(Vector2F left, float right)
		{
			return new Vector2F(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2F operator *(Vector2F left, Vector2F right)
		{
			return new Vector2F(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2F operator *(Vector2F left, float right)
		{
			return new Vector2F(left.X * right, left.Y * right);
		}
#endregion
	}
}

