namespace Molten.Math
{
	///<summary>A <see cref = "nuint"/> vector comprised of 2 components.</summary>
	public partial struct Vector2NU
	{
		///<summary>The X component.</summary>
		public nuint X;

		///<summary>The Y component.</summary>
		public nuint Y;

		///<summary>Creates a new instance of <see cref = "Vector2NU"/></summary>
		public Vector2NU(nuint x, nuint y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Vector2NU operator +(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
			};
		}
#endregion
	}
}

