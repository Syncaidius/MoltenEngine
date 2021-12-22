namespace Molten.Math
{
	///<summary>A <see cref = "byte"/> vector comprised of 3 components.</summary>
	public partial struct Byte3
	{
		///<summary>The X component.</summary>
		public byte X;

		///<summary>The Y component.</summary>
		public byte Y;

		///<summary>The Z component.</summary>
		public byte Z;

		///<summary>Creates a new instance of <see cref = "Byte3"/></summary>
		public Byte3(byte x, byte y, byte z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
}

