using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "float"/> vector comprised of 3 components.</summary>
	public partial struct Vector3F
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3F"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3F"/>.</param>
        /// <param name="right">Second source <see cref="Vector3F"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Vector3F"/>.</param>
        public static Vector3F Cross(ref Vector3F left, ref Vector3F right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3F"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3F"/>.</param>
        /// <param name="right">Second source <see cref="Vector3F"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3F"/>.</returns>
        public static Vector3F Cross(Vector3F left, Vector3F right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

