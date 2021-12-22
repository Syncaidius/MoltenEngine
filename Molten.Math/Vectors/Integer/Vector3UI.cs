namespace Molten.Math
{
	///<summary>A <see cref = "uint"/> vector comprised of 3 components.</summary>
	public partial struct Vector3UI
	{
		///<summary>The X component.</summary>
		public uint X;

		///<summary>The Y component.</summary>
		public uint Y;

		///<summary>The Z component.</summary>
		public uint Z;

		///<summary>Creates a new instance of <see cref = "Vector3UI"/></summary>
		public Vector3UI(uint x, uint y, uint z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
}

