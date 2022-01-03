using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "nuint"/> vector comprised of 2 components.</summary>
	public partial struct Vector2NU
	{
		


#region Static Methods
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static nuint Cross(ref Vector2NU left, ref Vector2NU right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        public static nuint Cross(Vector2NU left, Vector2NU right)
        {
            return (left.X * right.Y) - (left.Y * right.X);
        }
#endregion
	}
}

