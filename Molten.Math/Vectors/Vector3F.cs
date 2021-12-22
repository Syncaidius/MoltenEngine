namespace Molten.Math
{
	///<summary>A <see cref = "float"/> vector comprised of 3 components.</summary>
	public partial struct Vector3F
	{
		///<summary>The X component.</summary>
		public float X;

		///<summary>The Y component.</summary>
		public float Y;

		///<summary>The Z component.</summary>
		public float Z;

		///<summary>Creates a new instance of <see cref = "Vector3F"/></summary>
		public Vector3F(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region operators
		public static Vector3F operator +(Vector3F left, Vector3F right)
		{
			return new Vector3F()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
				Z = left.Z + right.Z,
			};
		}
#endregion
	}
}

