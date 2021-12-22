namespace Molten.Math
{
	///<summary>A <see cref = "byte"/> vector comprised of 4 components.</summary>
	public partial struct Byte4
	{
		///<summary>The X component.</summary>
		public byte X;

		///<summary>The Y component.</summary>
		public byte Y;

		///<summary>The Z component.</summary>
		public byte Z;

		///<summary>The W component.</summary>
		public byte W;

		///<summary>Creates a new instance of <see cref = "Byte4"/></summary>
		public Byte4(byte x, byte y, byte z, byte w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static Byte4 operator +(Byte4 left, Byte4 right)
		{
			return new Byte4()
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

