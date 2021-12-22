namespace Molten.Math
{
	///<summary>A <see cref = "float"/> vector comprised of 4 components.</summary>
	public partial struct Vector4F
	{
		///<summary>The X component.</summary>
		public float X;

		///<summary>The Y component.</summary>
		public float Y;

		///<summary>The Z component.</summary>
		public float Z;

		///<summary>The W component.</summary>
		public float W;

		///<summary>Creates a new instance of <see cref = "Vector4F"/></summary>
		public Vector4F(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region operators
		public static Vector4F operator +(Vector4F left, Vector4F right)
		{
			return new Vector4F()
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

