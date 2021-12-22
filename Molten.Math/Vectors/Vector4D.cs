namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of 4 components.</summary>
	public partial struct Vector4D
	{
		///<summary>The X component.</summary>
		public double X;

		///<summary>The Y component.</summary>
		public double Y;

		///<summary>The Z component.</summary>
		public double Z;

		///<summary>The W component.</summary>
		public double W;

		///<summary>Creates a new instance of <see cref = "Vector4D"/></summary>
		public Vector4D(double x, double y, double z, double w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static Vector4D operator +(Vector4D left, Vector4D right)
		{
			return new Vector4D()
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

