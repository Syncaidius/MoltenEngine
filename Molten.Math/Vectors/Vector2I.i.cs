namespace Molten
{
    ///<summary>A <see cref = "int"/> vector comprised of 2 components.</summary>
    public partial struct Vector2I
	{

#region Operators - Cast
        public static explicit operator Vector2F(Vector2I value)
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

