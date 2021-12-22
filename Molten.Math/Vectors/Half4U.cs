using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Half4U
	{
		///<summary>The X component.</summary>
		public ushort X;

		///<summary>The Y component.</summary>
		public ushort Y;

		///<summary>The Z component.</summary>
		public ushort Z;

		///<summary>The W component.</summary>
		public ushort W;


		public static Half4U One = new Half4U((ushort)1, (ushort)1, (ushort)1, (ushort)1);

		public static Half4U Zero = new Half4U(0, 0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Half4U"/></summary>
		public Half4U(ushort x, ushort y, ushort z, ushort w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static Half4U operator +(Half4U left, Half4U right)
		{
			return new Half4U(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Half4U operator -(Half4U left, Half4U right)
		{
			return new Half4U(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Half4U operator /(Half4U left, Half4U right)
		{
			return new Half4U(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Half4U operator *(Half4U left, Half4U right)
		{
			return new Half4U(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}
#endregion
	}
}

