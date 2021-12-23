using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "float"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector4F
	{
		///<summary>The X component.</summary>
		public float X;

		///<summary>The Y component.</summary>
		public float Y;

		///<summary>The Z component.</summary>
		public float Z;

		///<summary>The W component.</summary>
		public float W;


		///<summary>The size of <see cref="Vector4F"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4F));

		public static Vector4F One = new Vector4F(1F, 1F, 1F, 1F);

		public static Vector4F Zero = new Vector4F(0F, 0F, 0F, 0F);

		///<summary>Creates a new instance of <see cref = "Vector4F"/></summary>
		public Vector4F(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Add operators
		public static Vector4F operator +(Vector4F left, Vector4F right)
		{
			return new Vector4F(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4F operator +(Vector4F left, float right)
		{
			return new Vector4F(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4F operator -(Vector4F left, Vector4F right)
		{
			return new Vector4F(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4F operator -(Vector4F left, float right)
		{
			return new Vector4F(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4F operator /(Vector4F left, Vector4F right)
		{
			return new Vector4F(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4F operator /(Vector4F left, float right)
		{
			return new Vector4F(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4F operator *(Vector4F left, Vector4F right)
		{
			return new Vector4F(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4F operator *(Vector4F left, float right)
		{
			return new Vector4F(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

