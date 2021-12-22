namespace Molten.Math
{
	///<summary>A <see cref = "uint"/> vector comprised of 2 components.</summary>
	public partial struct Vector2UI
	{
		///<summary>The X component.</summary>
		public uint X;

		///<summary>The Y component.</summary>
		public uint Y;

		///<summary>Creates a new instance of <see cref = "Vector2UI"/></summary>
		public Vector2UI(uint x, uint y)
		{
			X = x;
			Y = y;
		}
	}
}

