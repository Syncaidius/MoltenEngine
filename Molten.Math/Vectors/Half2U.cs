namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of 2 components.</summary>
	public partial struct Half2U
	{
		///<summary>The X component.</summary>
		public ushort X;

		///<summary>The Y component.</summary>
		public ushort Y;

		///<summary>Creates a new instance of <see cref = "Half2U"/></summary>
		public Half2U(ushort x, ushort y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Half2U operator +(Half2U left, Half2U right)
		{
			return new Half2U()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
			};
		}
#endregion
	}
}

