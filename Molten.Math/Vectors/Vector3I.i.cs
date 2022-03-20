namespace Molten
{
    ///<summary>A <see cref = "int"/> vector comprised of 3 components.</summary>
    public partial struct Vector3I
	{

#region Operators - Cast
        public static explicit operator Vector3F(Vector3I value)
		{
			return new Vector3F()
			{
				X = value.X,
				Y = value.Y,
				Z = value.Z,
			};
		}
#endregion
	}
}

