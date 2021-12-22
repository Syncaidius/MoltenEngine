namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of 3 components.</summary>
	public partial struct Vector3I
	{
		///<summary>The X component.</summary>
		public int X;

		///<summary>The Y component.</summary>
		public int Y;

		///<summary>The Z component.</summary>
		public int Z;

		///<summary>Creates a new instance of <see cref = "Vector3I"/></summary>
		public Vector3I(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
}

