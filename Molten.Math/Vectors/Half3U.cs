using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half3U
	{
		///<summary>The X component.</summary>
		public ushort X;

		///<summary>The Y component.</summary>
		public ushort Y;

		///<summary>The Z component.</summary>
		public ushort Z;


		///<summary>The size of <see cref="Half3U"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half3U));

		public static Half3U One = new Half3U((ushort)1, (ushort)1, (ushort)1);

		public static Half3U Zero = new Half3U(0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Half3U"/></summary>
		public Half3U(ushort x, ushort y, ushort z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region Add operators
		public static Half3U operator +(Half3U left, Half3U right)
		{
			return new Half3U(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Half3U operator +(Half3U left, ushort right)
		{
			return new Half3U(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Half3U operator -(Half3U left, Half3U right)
		{
			return new Half3U(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Half3U operator -(Half3U left, ushort right)
		{
			return new Half3U(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Half3U operator /(Half3U left, Half3U right)
		{
			return new Half3U(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Half3U operator /(Half3U left, ushort right)
		{
			return new Half3U(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Half3U operator *(Half3U left, Half3U right)
		{
			return new Half3U(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Half3U operator *(Half3U left, ushort right)
		{
			return new Half3U(left.X * right, left.Y * right, left.Z * right);
		}
#endregion
	}
}

