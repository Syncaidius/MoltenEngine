namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of 3 components.</summary>
	public partial struct Vector3D
	{
		///<summary>The X component.</summary>
		public double X;

		///<summary>The Y component.</summary>
		public double Y;

		///<summary>The Z component.</summary>
		public double Z;

		///<summary>Creates a new instance of <see cref = "Vector3D"/></summary>
		public Vector3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region operators
		public static Vector3D operator +(Vector3D left, Vector3D right)
		{
			return new Vector3D()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
				Z = left.Z + right.Z,
			};
		}
#endregion
	}
}

