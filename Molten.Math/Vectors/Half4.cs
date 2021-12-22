namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of 4 components.</summary>
	public partial struct Half4
	{
		///<summary>The X component.</summary>
		public short X;

		///<summary>The Y component.</summary>
		public short Y;

		///<summary>The Z component.</summary>
		public short Z;

		///<summary>The W component.</summary>
		public short W;

		///<summary>Creates a new instance of <see cref = "Half4"/></summary>
		public Half4(short x, short y, short z, short w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static Half4 operator +(Half4 left, Half4 right)
		{
			return new Half4()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
				Z = left.Z + right.Z,
				W = left.W + right.W,
			};
		}
#endregion
	}
}

