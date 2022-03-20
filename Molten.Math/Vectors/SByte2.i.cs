namespace Molten
{
    ///<summary>A <see cref = "sbyte"/> vector comprised of 2 components.</summary>
    public partial struct SByte2
	{

#region Operators - Cast
        public static explicit operator Vector2F(SByte2 value)
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

