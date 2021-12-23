using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector3D
	{
		///<summary>The X component.</summary>
		public double X;

		///<summary>The Y component.</summary>
		public double Y;

		///<summary>The Z component.</summary>
		public double Z;


		public static Vector3D One = new Vector3D(1D, 1D, 1D);

		public static Vector3D Zero = new Vector3D(0D, 0D, 0D);

		///<summary>Creates a new instance of <see cref = "Vector3D"/></summary>
		public Vector3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Add operators
		public static Vector3D operator +(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3D operator +(Vector3D left, double right)
		{
			return new Vector3D(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3D operator -(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3D operator -(Vector3D left, double right)
		{
			return new Vector3D(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3D operator /(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3D operator /(Vector3D left, double right)
		{
			return new Vector3D(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3D operator *(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3D operator *(Vector3D left, double right)
		{
			return new Vector3D(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

