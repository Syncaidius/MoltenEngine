using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "byte"/> vector comprised of 2 components.</summary>
	public partial struct Byte2
	{
		


#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static byte Cross(ref Byte2 left, ref Byte2 right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static byte Cross(Byte2 left, Byte2 right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }
#endregion
	}
}

