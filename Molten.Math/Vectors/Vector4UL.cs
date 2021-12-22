namespace Molten.Math
{
	///<summary>A <see cref = "ulong"/> vector comprised of 4 components.</summary>
	public partial struct Vector4UL
	{
		///<summary>The X component.</summary>
		public ulong X;

		///<summary>The Y component.</summary>
		public ulong Y;

		///<summary>The Z component.</summary>
		public ulong Z;

		///<summary>The W component.</summary>
		public ulong W;

		///<summary>Creates a new instance of <see cref = "Vector4UL"/></summary>
		public Vector4UL(ulong x, ulong y, ulong z, ulong w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static Vector4UL operator +(Vector4UL left, Vector4UL right)
		{
			return new Vector4UL()
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

