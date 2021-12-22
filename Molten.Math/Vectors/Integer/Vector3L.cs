namespace Molten.Math
{
	///<summary>A <see cref = "long"/> vector comprised of 3 components.</summary>
	public partial struct Vector3L
	{
		///<summary>The X component.</summary>
		public long X;

		///<summary>The Y component.</summary>
		public long Y;

		///<summary>The Z component.</summary>
		public long Z;

		///<summary>Creates a new instance of <see cref = "Vector3L"/></summary>
		public Vector3L(long x, long y, long z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
}

