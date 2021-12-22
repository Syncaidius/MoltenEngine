namespace Molten.Math
{
	///<summary>A <see cref = "ulong"/> vector comprised of 3 components.</summary>
	public partial struct Vector3UL
	{
		///<summary>The X component.</summary>
		public ulong X;

		///<summary>The Y component.</summary>
		public ulong Y;

		///<summary>The Z component.</summary>
		public ulong Z;

		///<summary>Creates a new instance of <see cref = "Vector3UL"/></summary>
		public Vector3UL(ulong x, ulong y, ulong z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
}

