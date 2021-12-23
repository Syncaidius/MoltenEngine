using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half3
	{
		///<summary>The X component.</summary>
		public short X;

		///<summary>The Y component.</summary>
		public short Y;

		///<summary>The Z component.</summary>
		public short Z;


		///<summary>The size of <see cref="Half3"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half3));

		public static Half3 One = new Half3((short)1, (short)1, (short)1);

		public static Half3 Zero = new Half3(0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Half3"/></summary>
		public Half3(short x, short y, short z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Add operators
		public static Half3 operator +(Half3 left, Half3 right)
		{
			return new Half3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Half3 operator +(Half3 left, short right)
		{
			return new Half3(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Half3 operator -(Half3 left, Half3 right)
		{
			return new Half3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Half3 operator -(Half3 left, short right)
		{
			return new Half3(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Half3 operator /(Half3 left, Half3 right)
		{
			return new Half3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Half3 operator /(Half3 left, short right)
		{
			return new Half3(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Half3 operator *(Half3 left, Half3 right)
		{
			return new Half3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Half3 operator *(Half3 left, short right)
		{
			return new Half3(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

