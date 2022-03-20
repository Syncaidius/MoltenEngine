namespace Molten
{
    ///<summary>A <see cref = "ulong"/> vector comprised of 2 components.</summary>
    public partial struct Vector2UL
	{

#region Operators - Cast
        public static explicit operator Vector2D(Vector2UL value)
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

