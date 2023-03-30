namespace Molten.HalfPrecision
{
    ///<summary>A <see cref = "ushort"/> vector comprised of 2 components.</summary>
    public partial struct Vector2US
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static ushort Cross(ref Vector2US left, ref Vector2US right)
        {
            return (ushort)((left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static ushort Cross(Vector2US left, Vector2US right)
        {
            return (ushort)((left.X * right.Y) - (left.Y * right.X));
        }
#endregion
	}
}

