namespace Molten
{
    ///<summary>A <see cref = "long"/> vector comprised of 2 components.</summary>
    public partial struct Vector2L
	{

#region Operators - Cast
        public static explicit operator Vector2D(Vector2L value)
		{
			return new Vector2D()
			{
				X = value.X,
				Y = value.Y,
			};
		}
#endregion
	}
}

