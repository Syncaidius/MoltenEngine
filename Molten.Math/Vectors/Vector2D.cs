namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of 2 components.</summary>
	public partial struct Vector2D
	{
		///<summary>The X component.</summary>
		public double X;

		///<summary>The Y component.</summary>
		public double Y;

		///<summary>Creates a new instance of <see cref = "Vector2D"/></summary>
		public Vector2D(double x, double y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Vector2D operator +(Vector2D left, Vector2D right)
		{
			return new Vector2D()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
			};
		}
#endregion
	}
}

