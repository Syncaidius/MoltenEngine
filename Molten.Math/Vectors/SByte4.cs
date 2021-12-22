namespace Molten.Math
{
	///<summary>A <see cref = "sbyte"/> vector comprised of 4 components.</summary>
	public partial struct SByte4
	{
		///<summary>The X component.</summary>
		public sbyte X;

		///<summary>The Y component.</summary>
		public sbyte Y;

		///<summary>The Z component.</summary>
		public sbyte Z;

		///<summary>The W component.</summary>
		public sbyte W;

		///<summary>Creates a new instance of <see cref = "SByte4"/></summary>
		public SByte4(sbyte x, sbyte y, sbyte z, sbyte w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static SByte4 operator +(SByte4 left, SByte4 right)
		{
			return new SByte4()
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

