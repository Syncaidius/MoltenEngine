




namespace Molten.Math
{
	///<summary>A <see cref = "sbyte"/> vector comprised of 2 components.</summary>
	public partial struct SByte2
	{
		///<summary>The X component.</summary>
		public sbyte X;

		///<summary>The Y component.</summary>
		public sbyte Y;

		///<summary>Creates a new instance of <see cref = "SByte2"/></summary>
		public SByte2(sbyte x, sbyte y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static SByte2 operator +(SByte2 left, SByte2 right)
		{
			return new SByte2()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
			};
		}
#endregion
	}
}

