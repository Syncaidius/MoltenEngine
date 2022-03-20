namespace Molten
{
    ///<summary>A <see cref = "short"/> vector comprised of 2 components.</summary>
    public partial struct Vector2S
	{

#region Operators - Cast
        public static explicit operator Vector2F(Vector2S value)
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

