using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "uint"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector2UI
	{
		///<summary>The X component.</summary>
		public uint X;

		///<summary>The Y component.</summary>
		public uint Y;


		///<summary>The size of <see cref="Vector2UI"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2UI));

		public static Vector2UI One = new Vector2UI(1U, 1U);

		public static Vector2UI Zero = new Vector2UI(0U, 0U);

		///<summary>Creates a new instance of <see cref = "Vector2UI"/></summary>
		public Vector2UI(uint x, uint y)
		{
			X = x;
			Y = y;
		}

#region Add operators
		public static Vector2UI operator +(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2UI operator +(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2UI operator -(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2UI operator -(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2UI operator /(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2UI operator /(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2UI operator *(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2UI operator *(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X * right, left.Y * right);
		}
#endregion
	}
}

