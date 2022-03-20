namespace Molten
{
    ///<summary>A <see cref = "short"/> vector comprised of 4 components.</summary>
    public partial struct Vector4S
	{

#region Operators - Cast
        public static explicit operator Vector4F(Vector4S value)
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

