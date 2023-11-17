namespace Molten.HalfPrecision
{
	///<summary>A <see cref = "short"/> vector comprised of 2 components.</summary>
	public partial struct Vector2S
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static short Cross(ref Vector2S left, ref Vector2S right)
        {
            return (short)((left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static short Cross(Vector2S left, Vector2S right)
        {
            return (short)((left.X * right.Y) - (left.Y * right.X));
        }
#endregion
	}
}

