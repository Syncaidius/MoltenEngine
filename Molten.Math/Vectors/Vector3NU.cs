using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nuint"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=0)]
	public partial struct Vector3NU
	{
		///<summary>The X component.</summary>
		public nuint X;

		///<summary>The Y component.</summary>
		public nuint Y;

		///<summary>The Z component.</summary>
		public nuint Z;


		///<summary>The size of <see cref="Vector3NU"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3NU));

		public static Vector3NU One = new Vector3NU(1U, 1U, 1U);

		public static Vector3NU Zero = new Vector3NU(0U, 0U, 0U);

		///<summary>Creates a new instance of <see cref = "Vector3NU"/></summary>
		public Vector3NU(nuint x, nuint y, nuint z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Add operators
		public static Vector3NU operator +(Vector3NU left, Vector3NU right)
		{
			return new Vector3NU(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3NU operator +(Vector3NU left, nuint right)
		{
			return new Vector3NU(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3NU operator -(Vector3NU left, Vector3NU right)
		{
			return new Vector3NU(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3NU operator -(Vector3NU left, nuint right)
		{
			return new Vector3NU(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3NU operator /(Vector3NU left, Vector3NU right)
		{
			return new Vector3NU(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3NU operator /(Vector3NU left, nuint right)
		{
			return new Vector3NU(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3NU operator *(Vector3NU left, Vector3NU right)
		{
			return new Vector3NU(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3NU operator *(Vector3NU left, nuint right)
		{
			return new Vector3NU(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

