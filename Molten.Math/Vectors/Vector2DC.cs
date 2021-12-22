namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of 2 components.</summary>
	public partial struct Vector2DC
	{
		///<summary>The X component.</summary>
		public decimal X;

		///<summary>The Y component.</summary>
		public decimal Y;

		///<summary>Creates a new instance of <see cref = "Vector2DC"/></summary>
		public Vector2DC(decimal x, decimal y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Vector2DC operator +(Vector2DC left, Vector2DC right)
		{
			return new Vector2DC()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
			};
		}
#endregion
	}
}

