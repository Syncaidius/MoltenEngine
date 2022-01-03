using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "float"/> vector comprised of 2 components.</summary>
	public partial struct Vector2F
	{
		


#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static float Cross(ref Vector2F left, ref Vector2F right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static float Cross(Vector2F left, Vector2F right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }
#endregion
	}
}

