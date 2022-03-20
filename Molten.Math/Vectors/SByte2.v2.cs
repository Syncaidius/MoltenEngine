namespace Molten
{
    ///<summary>A <see cref = "sbyte"/> vector comprised of 2 components.</summary>
    public partial struct SByte2
	{
		


#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static sbyte Cross(ref SByte2 left, ref SByte2 right)
        {
            return (sbyte)((left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static sbyte Cross(SByte2 left, SByte2 right)
        {
            return (sbyte)((left.X * right.Y) - (left.Y * right.X));
        }
#endregion
	}
}

