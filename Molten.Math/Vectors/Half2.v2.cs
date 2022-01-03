using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "short"/> vector comprised of 2 components.</summary>
	public partial struct Half2
	{
		


#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static short Cross(ref Half2 left, ref Half2 right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static short Cross(Half2 left, Half2 right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }
#endregion
	}
}

