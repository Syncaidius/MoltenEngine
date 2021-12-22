namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of 2 components.</summary>
	public partial struct Half2
	{
		///<summary>The X component.</summary>
		public short X;

		///<summary>The Y component.</summary>
		public short Y;

		///<summary>Creates a new instance of <see cref = "Half2"/></summary>
		public Half2(short x, short y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Half2 operator +(Half2 left, Half2 right)
		{
			return new Half2()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
			};
		}
#endregion
	}
}

