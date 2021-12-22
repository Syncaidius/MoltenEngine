namespace Molten.Math
{
	///<summary>A <see cref = "nint"/> vector comprised of 2 components.</summary>
	public partial struct Vector2N
	{
		///<summary>The X component.</summary>
		public nint X;

		///<summary>The Y component.</summary>
		public nint Y;

		///<summary>Creates a new instance of <see cref = "Vector2N"/></summary>
		public Vector2N(nint x, nint y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Vector2N operator +(Vector2N left, Vector2N right)
		{
			return new Vector2N()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
			};
		}
#endregion
	}
}

