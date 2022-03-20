namespace Molten
{
    ///<summary>A <see cref = "long"/> vector comprised of 2 components.</summary>
    public partial struct Vector2L
	{
		


#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static long Cross(ref Vector2L left, ref Vector2L right)
        {
            return ((left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static long Cross(Vector2L left, Vector2L right)
        {
            return ((left.X * right.Y) - (left.Y * right.X));
        }
#endregion
	}
}

