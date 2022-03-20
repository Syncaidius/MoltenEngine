namespace Molten
{
    ///<summary>A <see cref = "sbyte"/> vector comprised of 4 components.</summary>
    public partial struct SByte4
	{

#region Operators - Cast
        public static explicit operator Vector4F(SByte4 value)
		{
			return new Vector4F()
			{
				X = value.X,
				Y = value.Y,
				Z = value.Z,
				W = value.W,
			};
		}
#endregion
	}
}

