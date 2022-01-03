using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "nint"/> vector comprised of 2 components.</summary>
	public partial struct Vector2N
	{
		


#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static nint Cross(ref Vector2N left, ref Vector2N right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static nint Cross(Vector2N left, Vector2N right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }
#endregion
	}
}

