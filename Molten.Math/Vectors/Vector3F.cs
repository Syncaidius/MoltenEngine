using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "float"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector3F
	{
		///<summary>The X component.</summary>
		public float X;

		///<summary>The Y component.</summary>
		public float Y;

		///<summary>The Z component.</summary>
		public float Z;


		public static Vector3F One = new Vector3F(1F, 1F, 1F);

		public static Vector3F Zero = new Vector3F(0F, 0F, 0F);

		///<summary>Creates a new instance of <see cref = "Vector3F"/></summary>
		public Vector3F(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Add operators
		public static Vector3F operator +(Vector3F left, Vector3F right)
		{
			return new Vector3F(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3F operator +(Vector3F left, float right)
		{
			return new Vector3F(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3F operator -(Vector3F left, Vector3F right)
		{
			return new Vector3F(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3F operator -(Vector3F left, float right)
		{
			return new Vector3F(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3F operator /(Vector3F left, Vector3F right)
		{
			return new Vector3F(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3F operator /(Vector3F left, float right)
		{
			return new Vector3F(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3F operator *(Vector3F left, Vector3F right)
		{
			return new Vector3F(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3F operator *(Vector3F left, float right)
		{
			return new Vector3F(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

