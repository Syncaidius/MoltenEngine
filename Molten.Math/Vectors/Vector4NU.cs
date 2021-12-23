using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nuint"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=0)]
	public partial struct Vector4NU
	{
		///<summary>The X component.</summary>
		public nuint X;

		///<summary>The Y component.</summary>
		public nuint Y;

		///<summary>The Z component.</summary>
		public nuint Z;

		///<summary>The W component.</summary>
		public nuint W;


		///<summary>The size of <see cref="Vector4NU"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4NU));

		public static Vector4NU One = new Vector4NU(1U, 1U, 1U, 1U);

		public static Vector4NU Zero = new Vector4NU(0U, 0U, 0U, 0U);

		///<summary>Creates a new instance of <see cref = "Vector4NU"/></summary>
		public Vector4NU(nuint x, nuint y, nuint z, nuint w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Add operators
		public static Vector4NU operator +(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4NU operator +(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4NU operator -(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4NU operator -(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4NU operator /(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4NU operator /(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4NU operator *(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4NU operator *(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion
	}
}

