namespace Molten
{
    ///<summary>A <see cref = "uint"/> vector comprised of 2 components.</summary>
    public partial struct Vector2UI
	{

#region Operators - Cast
        public static explicit operator Vector2F(Vector2UI value)
		{
			return new Vector2F()
			{
				X = value.X,
				Y = value.Y,
			};
		}
#endregion
	}
}

