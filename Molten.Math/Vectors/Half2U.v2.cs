using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of 2 components.</summary>
	public partial struct Half2U
	{
		


#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static ushort Cross(ref Half2U left, ref Half2U right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static ushort Cross(Half2U left, Half2U right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }
#endregion
	}
}

