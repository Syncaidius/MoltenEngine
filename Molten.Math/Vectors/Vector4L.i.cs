namespace Molten
{
    ///<summary>A <see cref = "long"/> vector comprised of 4 components.</summary>
    public partial struct Vector4L
	{

#region Operators - Cast
        public static explicit operator Vector4D(Vector4L value)
		{
			return new Vector4D()
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

