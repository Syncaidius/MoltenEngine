namespace Molten.Math
{
	///<summary>A <see cref = "long"/> vector comprised of 4 components.</summary>
	public partial struct Vector4L
	{
		///<summary>The X component.</summary>
		public long X;

		///<summary>The Y component.</summary>
		public long Y;

		///<summary>The Z component.</summary>
		public long Z;

		///<summary>The W component.</summary>
		public long W;

		///<summary>Creates a new instance of <see cref = "Vector4L"/></summary>
		public Vector4L(long x, long y, long z, long w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static Vector4L operator +(Vector4L left, Vector4L right)
		{
			return new Vector4L()
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

