namespace Molten
{
    ///<summary>A <see cref = "ushort"/> vector comprised of 3 components.</summary>
    public partial struct Vector3US
	{

#region Operators - Cast
        public static explicit operator Vector3F(Vector3US value)
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

