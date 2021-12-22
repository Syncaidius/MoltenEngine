namespace Molten.Math
{
	///<summary>A <see cref = "nuint"/> vector comprised of 4 components.</summary>
	public partial struct Vector4NU
	{
		///<summary>The X component.</summary>
		public nuint X;

		///<summary>The Y component.</summary>
		public nuint Y;

		///<summary>The Z component.</summary>
		public nuint Z;

		///<summary>The W component.</summary>
		public nuint W;

		///<summary>Creates a new instance of <see cref = "Vector4NU"/></summary>
		public Vector4NU(nuint x, nuint y, nuint z, nuint w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static Vector4NU operator +(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU()
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

