using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "uint"/> vector comprised of 2 components.</summary>
	public partial struct Vector2UI
	{
		


#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static uint Cross(ref Vector2UI left, ref Vector2UI right)
        {
            return ((left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static uint Cross(Vector2UI left, Vector2UI right)
        {
            return ((left.X * right.Y) - (left.Y * right.X));
        }
#endregion
	}
}

