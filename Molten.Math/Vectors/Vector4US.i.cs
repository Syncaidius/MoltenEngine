namespace Molten
{
    ///<summary>A <see cref = "ushort"/> vector comprised of 4 components.</summary>
    public partial struct Vector4US
	{

#region Operators - Cast
        public static explicit operator Vector4F(Vector4US value)
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

