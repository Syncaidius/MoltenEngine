namespace Molten
{
    ///<summary>A <see cref = "long"/> vector comprised of 3 components.</summary>
    public partial struct Vector3L
	{

#region Operators - Cast
        public static explicit operator Vector3D(Vector3L value)
		{
			return new Vector3D()
			{
				X = value.X,
				Y = value.Y,
				Z = value.Z,
			};
		}
#endregion
	}
}

