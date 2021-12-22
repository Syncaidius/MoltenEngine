namespace Molten.Math
{
	///<summary>A <see cref = "nint"/> vector comprised of 4 components.</summary>
	public partial struct Vector4N
	{
		///<summary>The X component.</summary>
		public nint X;

		///<summary>The Y component.</summary>
		public nint Y;

		///<summary>The Z component.</summary>
		public nint Z;

		///<summary>The W component.</summary>
		public nint W;

		///<summary>Creates a new instance of <see cref = "Vector4N"/></summary>
		public Vector4N(nint x, nint y, nint z, nint w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static Vector4N operator +(Vector4N left, Vector4N right)
		{
			return new Vector4N()
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

