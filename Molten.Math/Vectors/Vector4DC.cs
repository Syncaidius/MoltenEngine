namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of 4 components.</summary>
	public partial struct Vector4DC
	{
		///<summary>The X component.</summary>
		public decimal X;

		///<summary>The Y component.</summary>
		public decimal Y;

		///<summary>The Z component.</summary>
		public decimal Z;

		///<summary>The W component.</summary>
		public decimal W;

		///<summary>Creates a new instance of <see cref = "Vector4DC"/></summary>
		public Vector4DC(decimal x, decimal y, decimal z, decimal w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static Vector4DC operator +(Vector4DC left, Vector4DC right)
		{
			return new Vector4DC()
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

