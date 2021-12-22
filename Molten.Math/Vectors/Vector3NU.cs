namespace Molten.Math
{
	///<summary>A <see cref = "nuint"/> vector comprised of 3 components.</summary>
	public partial struct Vector3NU
	{
		///<summary>The X component.</summary>
		public nuint X;

		///<summary>The Y component.</summary>
		public nuint Y;

		///<summary>The Z component.</summary>
		public nuint Z;

		///<summary>Creates a new instance of <see cref = "Vector3NU"/></summary>
		public Vector3NU(nuint x, nuint y, nuint z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region operators
		public static Vector3NU operator +(Vector3NU left, Vector3NU right)
		{
			return new Vector3NU()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
				Z = left.Z + right.Z,
			};
		}
#endregion
	}
}

