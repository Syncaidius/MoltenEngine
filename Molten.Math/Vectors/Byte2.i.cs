namespace Molten
{
    ///<summary>A <see cref = "byte"/> vector comprised of 2 components.</summary>
    public partial struct Byte2
	{

#region Operators - Cast
        public static explicit operator Vector2F(Byte2 value)
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

