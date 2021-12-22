namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of 2 components.</summary>
	public partial struct Vector2I
	{
		///<summary>The X component.</summary>
		public int X;

		///<summary>The Y component.</summary>
		public int Y;

		///<summary>Creates a new instance of <see cref = "Vector2I"/></summary>
		public Vector2I(int x, int y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Vector2I operator +(Vector2I left, Vector2I right)
		{
			return new Vector2I()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
			};
		}
#endregion
	}
}

