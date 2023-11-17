namespace Molten.DoublePrecision
{
	///<summary>A <see cref = "ulong"/> vector comprised of 2 components.</summary>
	public partial struct Vector2UL
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static ulong Cross(ref Vector2UL left, ref Vector2UL right)
        {
            return ((left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static ulong Cross(Vector2UL left, Vector2UL right)
        {
            return ((left.X * right.Y) - (left.Y * right.X));
        }
#endregion
	}
}

