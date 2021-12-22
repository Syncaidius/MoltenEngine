namespace Molten.Math
{
	///<summary>A <see cref = "uint"/> vector comprised of 4 components.</summary>
	public partial struct Vector4UI
	{
		///<summary>The X component.</summary>
		public uint X;

		///<summary>The Y component.</summary>
		public uint Y;

		///<summary>The Z component.</summary>
		public uint Z;

		///<summary>The W component.</summary>
		public uint W;

		///<summary>Creates a new instance of <see cref = "Vector4UI"/></summary>
		public Vector4UI(uint x, uint y, uint z, uint w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static Vector4UI operator +(Vector4UI left, Vector4UI right)
		{
			return new Vector4UI()
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

