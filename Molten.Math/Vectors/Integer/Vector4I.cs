namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of 4 components.</summary>
	public partial struct Vector4I
	{
		///<summary>The X component.</summary>
		public int X;

		///<summary>The Y component.</summary>
		public int Y;

		///<summary>The Z component.</summary>
		public int Z;

		///<summary>The W component.</summary>
		public int W;

		///<summary>Creates a new instance of <see cref = "Vector4I"/></summary>
		public Vector4I(int x, int y, int z, int w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}
	}
}

