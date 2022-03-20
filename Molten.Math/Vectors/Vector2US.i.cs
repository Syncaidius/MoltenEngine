namespace Molten
{
    ///<summary>A <see cref = "ushort"/> vector comprised of 2 components.</summary>
    public partial struct Vector2US
	{

#region Operators - Cast
        public static explicit operator Vector2F(Vector2US value)
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

